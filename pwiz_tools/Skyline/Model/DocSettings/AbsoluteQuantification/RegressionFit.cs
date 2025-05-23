﻿/*
 * Original author: Nicholas Shulman <nicksh .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2015 University of Washington - Seattle, WA
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using System;
using System.Collections.Generic;
using System.Linq;
using MathNet.Numerics.LinearRegression;
using pwiz.Common.Collections;
using pwiz.Common.DataAnalysis;
using pwiz.Common.SystemUtil;
using pwiz.Skyline.Model.Databinding.Entities;

namespace pwiz.Skyline.Model.DocSettings.AbsoluteQuantification
{
    public abstract class RegressionFit : LabeledValues<string>
    {
        public static readonly RegressionFit NONE = new SimpleRegressionFit(@"none",
            ()=>QuantificationStrings.RegressionFit_NONE_None, points=>CalibrationCurve.NO_EXTERNAL_STANDARDS);

        public static readonly RegressionFit LINEAR = new SimpleRegressionFit(@"linear",
            () => QuantificationStrings.RegressionFit_LINEAR_Linear, LinearFit);

        public static readonly RegressionFit LINEAR_THROUGH_ZERO = new SimpleRegressionFit(@"linear_through_zero",
            () => QuantificationStrings.RegressionFit_LINEAR_THROUGH_ZERO_Linear_through_zero, LinearFitThroughZero);

        public static readonly RegressionFit QUADRATIC = new QuadraticFit();

        public static readonly RegressionFit BILINEAR = new BilinearFit();

        public static readonly RegressionFit LINEAR_IN_LOG_SPACE = new LinearInLogSpace();
        public static readonly ImmutableList<RegressionFit> All = ImmutableList<RegressionFit>.ValueOf(new[]
        {
            NONE, LINEAR_THROUGH_ZERO, LINEAR, BILINEAR, QUADRATIC, LINEAR_IN_LOG_SPACE
        });

        protected RegressionFit(string name, Func<string> getLabelFunc) : base(name, getLabelFunc)
        {
        }

        public CalibrationCurve Fit(IList<WeightedPoint> points)
        {
            try
            {
                CalibrationCurve curve = FitPoints(points);
                return curve;
            }
            catch (Exception e)
            {
                return new CalibrationCurve.Error(e.Message);
            }
        }

        public CalibrationCurveMetrics GetCalibrationCurveMetrics(IList<WeightedPoint> points)
        {
            return Fit(points).GetMetrics(points);
        }

        public override string ToString()
        {
            return Label;
        }

        protected abstract CalibrationCurve FitPoints(IList<WeightedPoint> points);

        private class SimpleRegressionFit : RegressionFit
        {
            private readonly Func<IList<WeightedPoint>, CalibrationCurve> _fitFunc;
            public SimpleRegressionFit(String name, Func<String> getLabelFunc, Func<IList<WeightedPoint>, CalibrationCurve> fitFunc) : base(name, getLabelFunc)
            {
                _fitFunc = fitFunc;
            }

            protected override CalibrationCurve FitPoints(IList<WeightedPoint> points)
            {
                return _fitFunc(points);
            }
        }

        protected static CalibrationCurve LinearFit(IList<WeightedPoint> points)
        {
            double[] values = WeightedRegression.Weighted(points.Select(p => new Tuple<double[], double>(new[] {p.X}, p.Y)),
                points.Select(p => p.Weight).ToArray(), true);
            return new CalibrationCurve.Linear(values[1], values[0]);
        }

        protected static CalibrationCurve LinearFitThroughZero(IList<WeightedPoint> points)
        {
            // ReSharper disable RedundantArgumentDefaultValue
            var values = WeightedRegression.Weighted(points.Select(p => new Tuple<double[], double>(new[] { p.X }, p.Y)),
                points.Select(p => p.Weight).ToArray(), false);
            // ReSharper restore RedundantArgumentDefaultValue
            return new CalibrationCurve.Linear(values[0], null);
        }


        public static RegressionFit Parse(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return NONE;
            }
            return All.FirstOrDefault(fit => fit.Name == name) ?? LINEAR;
        }

        private class QuadraticFit : RegressionFit
        {
            public QuadraticFit() : base(@"quadratic", () => QuantificationStrings.RegressionFit_QUADRATIC_Quadratic)
            {
                
            }

            protected override CalibrationCurve FitPoints(IList<WeightedPoint> points)
            {
                double[] result = MathNet.Numerics.Fit.PolynomialWeighted(
                    points.Select(p => p.X).ToArray(),
                    points.Select(p => p.Y).ToArray(),
                    points.Select(p => p.Weight).ToArray(),
                    2
                );
                return new CalibrationCurve.Quadratic(result[0], result[1], result[2]);
            }
        }

        private class BilinearFit : RegressionFit
        {
            public BilinearFit() : base(@"bilinear", () => QuantificationStrings.RegressionFit_BILINEAR_Bilinear)
            {
            }

            protected override CalibrationCurve FitPoints(IList<WeightedPoint> weightedPoints)
            {
                double? bestLod = null;
                double bestScore = double.MaxValue;
                var xValues = weightedPoints.Select(pt => pt.X).Distinct().OrderBy(x => x).ToArray();
                for (int i = 0; i < xValues.Length - 1; i++)
                {
                    var simplexConstant = new NelderMeadSimplex.SimplexConstant((xValues[i] + xValues[i + 1]) / 2,
                        (xValues[i + 1] - xValues[i]) / 4);
                    var regressionResult = NelderMeadSimplex.Regress(new[] { simplexConstant }, 0, 50,
                        constants => LodObjectiveFunction(constants[0], weightedPoints));
                    if (regressionResult.ErrorValue < bestScore)
                    {
                        bestLod = regressionResult.Constants[0];
                        bestScore = regressionResult.ErrorValue;
                    }
                }
                if (!bestLod.HasValue)
                {
                    return LinearFit(weightedPoints);
                }
                return GetCalibrationCurveWithLod(bestLod.Value, weightedPoints);
            }
            private static CalibrationCurve.Bilinear GetCalibrationCurveWithLod(double lod, IList<WeightedPoint> weightedPoints)
            {
                var linearPoints = weightedPoints.Select(pt => pt.X > lod ? pt : new WeightedPoint(lod, pt.Y, pt.Weight)).ToArray();
                if (linearPoints.Select(p => p.X).Distinct().Count() <= 1)
                {
                    return null;
                }

                try
                {
                    var linearCalibrationCurve = LinearFit(linearPoints) as CalibrationCurve.Linear;
                    if (linearCalibrationCurve == null)
                    {
                        return null;
                    }
                    return new CalibrationCurve.Bilinear(linearCalibrationCurve, lod);
                }
                catch (Exception)
                {
                    return null;
                }
            }

            /// <summary>
            /// Optimization function used when doing NelderMeadSimplex to find the best Limit of Detection.
            /// </summary>
            private static double LodObjectiveFunction(double lod, IList<WeightedPoint> weightedPoints)
            {
                CalibrationCurve.Bilinear calibrationCurve = GetCalibrationCurveWithLod(lod, weightedPoints);
                if (calibrationCurve == null)
                {
                    return double.MaxValue;
                }
                double totalDelta = 0;
                double totalWeight = 0;
                foreach (var pt in weightedPoints)
                {
                    double delta = pt.Y - calibrationCurve.GetY(pt.X).Value;
                    totalWeight += pt.Weight;
                    totalDelta += pt.Weight * delta * delta;
                }
                return totalDelta / totalWeight;
            }
        }

        private class LinearInLogSpace : RegressionFit
        {
            public LinearInLogSpace() : base(@"linear_in_log_space", () => AbsoluteQuantificationResources.LinearInLogSpace_Label_Linear_in_Log_Space)
            {
                
            }

            protected override CalibrationCurve FitPoints(IList<WeightedPoint> points)
            {
                if (points.Any(pt => pt.Y <= 0 || pt.X <= 0))
                {
                    return new CalibrationCurve.Error(AbsoluteQuantificationResources.LinearInLogSpace_FitPoints_Unable_to_do_a_regression_in_log_space_because_one_or_more_points_are_non_positive_);
                }
                var logPoints = points.Select(LogOfPoint).ToList();
                var calibrationCurve = (CalibrationCurve.Linear) LinearFit(logPoints);
                return new CalibrationCurve.LinearInLogSpace(calibrationCurve);
            }

            protected WeightedPoint LogOfPoint(WeightedPoint pt)
            {
                return CalibrationCurve.LinearInLogSpace.LogOfPoint(pt);
            }
        }
    }
}
