/*
 * Original author: Brendan MacLean <brendanx .at. u.washington.edu>,
 *                  MacCoss Lab, Department of Genome Sciences, UW
 *
 * Copyright 2020 University of Washington - Seattle, WA
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
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pwiz.Common.Chemistry;
using pwiz.Common.DataBinding;
using pwiz.Common.DataBinding.Controls;
using pwiz.Common.DataBinding.Controls.Editor;
using pwiz.Skyline;
using pwiz.Skyline.Alerts;
using pwiz.Skyline.Controls;
using pwiz.Skyline.Controls.Databinding;
using pwiz.Skyline.Controls.Graphs;
using pwiz.Skyline.Controls.GroupComparison;
using pwiz.Skyline.EditUI;
using pwiz.Skyline.FileUI;
using pwiz.Skyline.FileUI.PeptideSearch;
using pwiz.Skyline.Model;
using pwiz.Skyline.Model.DocSettings;
using pwiz.Skyline.Model.GroupComparison;
using pwiz.Skyline.Model.Irt;
using pwiz.Skyline.Model.Lib;
using pwiz.Skyline.Model.Results;
using pwiz.Skyline.Model.Serialization;
using pwiz.Skyline.Properties;
using pwiz.Skyline.SettingsUI;
using pwiz.Skyline.SettingsUI.Irt;
using pwiz.Skyline.Util;
using pwiz.Skyline.Util.Extensions;
using pwiz.SkylineTestUtil;

namespace TestPerf
{
    public class DiaSwathTestInfo
    {
        public class InstrumentSpecificValues
        {
            public string InstrumentTypeName;
            public string ZipFileName; // if set will override name calculated from InstrumentTypeName
            public string DiaFilesExtension;
            public string[] DiaFiles;
            public string[] SearchFiles;
            public int LibraryPeptideCount;
            public int ExpectedIrtPeptideCount;
            public IrtStandard IrtStandard;
            public double IrtSlope;
            public double IrtIntercept;
            public bool HasAmbiguousMatches;
            public string IsolationSchemeName;
            public string IsolationSchemeFile;
            public char IsolationSchemeFileSeparator;
            public string ExamplePeptide;
        }

        public class AnalysisValues
        {
            public bool IsWholeProteome;
            public bool KeepPrecursors;

            public string IrtFilterText;
            public int? MinPeptidesPerProtein;
            public bool RemoveDuplicates;
            //public int[] TargetCounts;
            public int[] FinalTargetCounts;
            public string ScoringModelCoefficients;
            public PointF ChromatogramClickPoint;
            public double[][] MassErrorStats;
            public int[] DiffPeptideCounts;
            public int UnpolishedProteins;
            public int? PolishedProteins;

            public string FastaPath =>
                IsWholeProteome
                    ? "DDA_search\\napedro_3mixed_human_yeast_ecoli_20140403_iRT_reverse.fasta"
                    : "DIA\\target_protein_sequences.fasta";
        }

        public InstrumentSpecificValues _instrumentValues;
        public AnalysisValues _analysisValues;
        public string[] DiaFiles => _instrumentValues.DiaFiles;
        public string InstrumentTypeName => _instrumentValues.InstrumentTypeName;
        public string RootName { get; set; }
        public string ZipFileName => _instrumentValues.ZipFileName ?? RootName;

        public string[] TestFilesZipPaths { get; set; }
        public string[] TestFilesPersistent { get; set; }
        public string LinkPdf { get; set; }

        public void TestTtofData(bool fullSet)
        {
            SetInstrumentType(new InstrumentSpecificValues
            {
                InstrumentTypeName = "TTOF",
                DiaFilesExtension = DataSourceUtil.EXT_MZML,
                DiaFiles = new[]
                {
                    "collinsb_I180316_001_SW-A.mzML",
                    "collinsb_I180316_002_SW-B.mzML",
                    "collinsb_I180316_003_SW-A.mzML",
                    "collinsb_I180316_004_SW-B.mzML",
                    "collinsb_I180316_005_SW-A.mzML",
                    "collinsb_I180316_006_SW-B.mzML",
                },
                SearchFiles = new[]
                {
                    "DDA_search\\interact.pep.xml"
                },
                LibraryPeptideCount = 18600,
                ExpectedIrtPeptideCount = 11,
                IrtStandard = IrtStandard.BIOGNOSYS_11,
                IrtSlope = 2.926,
                IrtIntercept = -68.850,
                HasAmbiguousMatches = true,
                IsolationSchemeName = "ETH TTOF (64 variable)",
                IsolationSchemeFile = "64_variable_windows.csv",
                IsolationSchemeFileSeparator = TextUtil.SEPARATOR_CSV,
                ExamplePeptide = "LPQVEGTGGDVQPSQDLVR"
            });

            if (fullSet)
            {
                _analysisValues = new AnalysisValues
                {
                    KeepPrecursors = false,
                    IsWholeProteome = true,
                    IrtFilterText = "iRT",
                    MinPeptidesPerProtein = 2,
                    RemoveDuplicates = true,
                    ChromatogramClickPoint = new PointF(34.18F, 108.0F),
                    //TargetCounts = new[] { 4700, 36628, 37960, 227760 },
                    FinalTargetCounts = new[] { 2399, 26144, 27078, 162468 },
                    ScoringModelCoefficients = "-0.0842|-0.7448|4.2689|-0.2083|-0.2644|0.7594|0.3123|-0.0659",
                    MassErrorStats = new[]
                    {
                        new[] {2.8, 4.6},
                        new[] {2.4, 4.3},
                        new[] {3.9, 4.2},
                        new[] {5.2, 4.1},
                        new[] {4.5, 4.2},
                        new[] {-0.4, 3.9},
                        new[] {1.0, 4.1},
                    },
                    DiffPeptideCounts = new[] { 12808, 7963, 2692, 2142 },
                    UnpolishedProteins = 2207,
                    PolishedProteins = 2207,
                };
            }
            else
            {
                _analysisValues = new AnalysisValues
                {
                    KeepPrecursors = false,
                    IrtFilterText = "standard",
                    ChromatogramClickPoint = new PointF(34.18F, 108.0F),
                    //TargetCounts = new[] { 14, 279, 299, 1793 },
                    FinalTargetCounts = new[] { 13, 279, 299, 1793 },
                    ScoringModelCoefficients = "0.2674|-0.6467|3.4194|-0.0171|-0.4723|0.9270|0.0768|-0.0496",
                    MassErrorStats = new[]
                    {
                        new[] {3.0, 4.4},
                        new[] {2.8, 4.1},
                        new[] {3.9, 4.3},
                        new[] {5.9, 3.6},
                        new[] {4.7, 4.2},
                        new[] {-0.1, 3.4},
                        new[] {1.0, 3.6},
                    },
                    DiffPeptideCounts = new[] { 142, 44, 30, 57 },
                    UnpolishedProteins = 9,
                };
            }
        }

        public bool IsTtof => Equals("TTOF", _instrumentValues.InstrumentTypeName);
        public void TestQeData(bool fullSet)
        {
            SetInstrumentType(new InstrumentSpecificValues
            {
                InstrumentTypeName = "QE",
                DiaFilesExtension = DataSourceUtil.EXT_MZML,
                DiaFiles = new[]
                {
                    "collinsb_X1803_171-A.mzML",
                    "collinsb_X1803_172-B.mzML",
                    "collinsb_X1803_173-A.mzML",
                    "collinsb_X1803_174-B.mzML",
                    "collinsb_X1803_175-A.mzML",
                    "collinsb_X1803_176-B.mzML",
                },
                SearchFiles = new[]
                {
                    "DDA_search\\interact.pep.xml"
                },
                LibraryPeptideCount = 15855,
                ExpectedIrtPeptideCount = 11,
                IrtStandard = IrtStandard.BIOGNOSYS_11,
                IrtSlope = 2.624,
                IrtIntercept = -48.003,
                HasAmbiguousMatches = false,
                IsolationSchemeName = "ETH QE (18 variable)",
                IsolationSchemeFile = "QE_DIA_18var.tsv",
                IsolationSchemeFileSeparator = TextUtil.SEPARATOR_TSV,
                ExamplePeptide = "LPQVEGTGGDVQPSQDLVR"
            });

            if (fullSet)
            {
                _analysisValues = new AnalysisValues
                {
                    KeepPrecursors = false,
                    IsWholeProteome = true,
                    IrtFilterText = "iRT",
                    MinPeptidesPerProtein = 2,
                    RemoveDuplicates = true,
                    ChromatogramClickPoint = new PointF(32.05F, 268334.7F),
                    //TargetCounts = new[] { 3991, 30916, 33841, 203044 },
                    FinalTargetCounts = new[] { 2038, 21960, 24013, 144076 },
                    ScoringModelCoefficients = "0.3173|-0.8915|3.7830|0.2262|-0.0825|0.7332|0.0012|-0.0606",
                    MassErrorStats = new[]
                    {
                        new[] { 2.0, 4.7 },
                        new[] { 1.5, 4.6 },
                        new[] { 2.0, 4.7 },
                        new[] { 2.1, 4.6 },
                        new[] { 2.1, 4.8 },
                        new[] { 2.2, 4.6 },
                        new[] { 1.9, 4.7 },
                    },
                    DiffPeptideCounts = new[] { 10459, 6387, 2238, 1823 },
                    UnpolishedProteins = 1647,
                    PolishedProteins = 1647,
                };
            }
            else
            {
                _analysisValues = new AnalysisValues
                {
                    KeepPrecursors = false,
                    IrtFilterText = "standard",
                    ChromatogramClickPoint = new PointF(31.98F, 285741.3F),
                    //TargetCounts = new[] { 14, 271, 331, 1985 },
                    FinalTargetCounts = new[] { 13, 271, 331, 1985 },
                    ScoringModelCoefficients = "0.3065|-0.7855|4.9580|-0.4976|-0.0812|0.7443|0.0988|-0.0542",
                    MassErrorStats = new[]
                    {
                        new[] { 1.6, 4.2 },
                        new[] { 1.3, 4.1 },
                        new[] { 1.6, 4.4 },
                        new[] { 1.8, 3.7 },
                        new[] { 1.8, 4.4 },
                        new[] { 1.8, 4.1 },
                        new[] { 1.2, 4.3 },
                    },
                    DiffPeptideCounts = new[] { 139, 47, 29, 52 },
                    UnpolishedProteins = 9,
                };
            }
        }

        public void TestPasefData(bool fullSet)
        {
            if (fullSet)
            {
                _analysisValues = new AnalysisValues
                {
                    KeepPrecursors = false,
                    IsWholeProteome = true,
                    IrtFilterText = Resources.IrtDb_MakeDocumentXml_iRT_standards,
                    MinPeptidesPerProtein = 2,
                    RemoveDuplicates = true,
                    ChromatogramClickPoint = new PointF(10.79F, 3800.0F),
                    //TargetCounts = new[] { 4937, 37152, 38716, 232296 },
                    FinalTargetCounts = new[] { 2697, 27225, 28373, 170238 },
                    ScoringModelCoefficients = "-0.3355|-0.9056|4.5022|3.5338|-0.1011|0.7389|0.4436|-0.1319",
                    MassErrorStats = new[]
                    {
                        new[] { 3.6, 2.7 },
                        new[] { 3.6, 2.6 },
                        new[] { 3.5, 2.7 },
                        new[] { 3.7, 2.6 },
                        new[] { 3.6, 2.7 },
                        new[] { 3.9, 2.6 },
                        new[] { 3.6, 2.7 },
                    },
                    DiffPeptideCounts = new[] { 12621, 8498, 2254, 1854 },
                    UnpolishedProteins = 2314,
                    PolishedProteins = 2314,
                };

                SetInstrumentType(new InstrumentSpecificValues
                {
                    InstrumentTypeName = "PASEF",
                    ZipFileName = "DIA-PASEF-full",
                    DiaFilesExtension = DataSourceUtil.EXT_AGILENT_BRUKER_RAW,
                    DiaFiles = new[]
                    {
                        "A210331_bcc_1180_lfqbA_17min_dia_200ng.d",
                        "A210331_bcc_1181_lfqbB_17min_dia_200ng.d",
                        "A210331_bcc_1182_lfqbA_17min_dia_200ng.d",
                        "A210331_bcc_1183_lfqbB_17min_dia_200ng.d",
                        "A210331_bcc_1184_lfqbA_17min_dia_200ng.d",
                        "A210331_bcc_1185_lfqbB_17min_dia_200ng.d",
                    },
                    SearchFiles = new[]
                    {
                        "DDA_search\\out\\interact.pep.xml",
                    },
                    IrtStandard = IrtStandard.AUTO,
                    LibraryPeptideCount = 22111,
                    ExpectedIrtPeptideCount = 15,
                    IrtSlope = 8.621,
                    IrtIntercept = -51.543,
                    HasAmbiguousMatches = false,
                    IsolationSchemeName = "diaPASEF (24 fixed)",
                    IsolationSchemeFile = "diaPASEF_24fix.csv",
                    IsolationSchemeFileSeparator = TextUtil.SEPARATOR_TSV,
                    ExamplePeptide = "LPQVEGTGGDVQPSQDLVR"
                });
            }
            else
            {
                _analysisValues = new AnalysisValues
                {
                    KeepPrecursors = false,
                    IrtFilterText = Resources.IrtDb_MakeDocumentXml_iRT_standards,
                    ChromatogramClickPoint = new PointF(10.79F, 3800.0F),
                    //TargetCounts = new[] { 14, 75, 83, 498 },
                    FinalTargetCounts = new[] { 12, 75, 83, 498 },
                    ScoringModelCoefficients = "-0.5430|-2.3111|7.5218|6.0404|-0.1598|0.6420|0.8685|-0.3133",
                    MassErrorStats = new[]
                    {
                        new[] {3.9, 1.9},
                        new[] {3.8, 2.3},
                        new[] {3.8, 1.8},
                        new[] {3.6, 2.3},
                        new[] {4.0, 1.7},
                        new[] {4.4, 1.7},
                        new[] {3.7, 1.8},
                    },
                    DiffPeptideCounts = new[] { 42, 7, 8, 12 },
                    UnpolishedProteins = 9,
                };

                SetInstrumentType(new InstrumentSpecificValues
                {
                    InstrumentTypeName = "PASEF",
                    ZipFileName = "DIA-PASEF-small",
                    DiaFilesExtension = DataSourceUtil.EXT_AGILENT_BRUKER_RAW,
                    DiaFiles = new[]
                    {
                        "A210331_bcc_1180_lfqbA_17min_dia_200ng.d",
                        "A210331_bcc_1181_lfqbB_17min_dia_200ng.d",
                        "A210331_bcc_1182_lfqbA_17min_dia_200ng.d",
                        "A210331_bcc_1183_lfqbB_17min_dia_200ng.d",
                        "A210331_bcc_1184_lfqbA_17min_dia_200ng.d",
                        "A210331_bcc_1185_lfqbB_17min_dia_200ng.d",
                    },
                    SearchFiles = new[]
                    {
                        "DDA_search\\out\\interact.pep.xml",
                        //"c:\\test\\issues\\skyline-cli-import\\DIA-PASEF-small\\yufe_fragpipe_dda\\interact-A210331_bcc_lfqbA_17min_dda_200ng_1171.pep.xml",
                        //"C:\\test\\issues\\skyline-cli-import\\DIA-PASEF-small\\yufe_fragpipe_dda\\interact-A210331_bcc_lfqbB_17min_dda_200ng_1172.pep.xml"
                    },
                    IrtStandard = IrtStandard.AUTO,
                    LibraryPeptideCount = 6028,
                    ExpectedIrtPeptideCount = 15,
                    IrtSlope = 8.765,
                    IrtIntercept = -54.858,
                    HasAmbiguousMatches = false,
                    IsolationSchemeName = "diaPASEF (24 fixed)",
                    IsolationSchemeFile = "diaPASEF_24fix.csv",
                    IsolationSchemeFileSeparator = TextUtil.SEPARATOR_TSV,
                    ExamplePeptide = "LPQVEGTGGDVQPSQDLVR"
                });
            }
        }

        public bool IsPasef => Equals("PASEF", _instrumentValues.InstrumentTypeName);

        public void SetInstrumentType(InstrumentSpecificValues instrumentValues)
        {
            _instrumentValues = instrumentValues;

            RootName = "DIA-" + InstrumentTypeName;

            const string pdfFormat = "https://skyline.ms/_webdav/home/software/Skyline/%40files/tutorials/{0}-22_2.pdf";
            // const string pdfFormat = "file:///C:/proj/branches/work/pwiz_tools/Skyline/Documentation/Tutorials/{0}-22_2.pdf";
            LinkPdf = string.Format(pdfFormat, RootName);

            TestFilesZipPaths = new[]
            {
                string.Format(@"http://skyline.ms/tutorials/{0}.zip", ZipFileName),
                string.Format(@"TestPerf\DiaSwath{0}Views.zip", InstrumentTypeName)
            };

            TestFilesPersistent = new[] { Path.Combine(ZipFileName, "DDA_search"), Path.Combine(ZipFileName, "DIA") };
        }

        public void ValidateTargets(bool recordMode, int[] targetCounts, int proteinCount, int peptideCount, int precursorCount, int transitionCount, string propName)
        {
            if (recordMode)
            {
                Console.WriteLine(@"{0} = new[] {{ {1}, {2}, {3}, {4} }},", propName, proteinCount, peptideCount, precursorCount, transitionCount);
            }

            var targetCountsActual = new[] { proteinCount, peptideCount, precursorCount, transitionCount };
            if (!ArrayUtil.EqualsDeep(targetCounts, targetCountsActual))
            {
                string msg = string.Format("Expected target counts <{0}> do not match actual <{1}>.",
                    string.Join(", ", targetCounts), string.Join(", ", targetCountsActual));
                if (!recordMode)
                    Assert.Fail(msg);
                else
                    Console.Error.WriteLine(msg);
            }
        }
    }

    /// <summary>
    /// Verify DIA/SWATH tutorial operation
    /// </summary>
    [TestClass]
    public class DiaSwathTutorialTest : AbstractFunctionalTestEx
    {
        private DiaSwathTestInfo _testInfo = new DiaSwathTestInfo();
        private string[] DiaFiles => _testInfo._instrumentValues.DiaFiles;
        private string InstrumentTypeName => _testInfo._instrumentValues.InstrumentTypeName;
        private string RootName => _testInfo.RootName;
        private string ZipFileName => _testInfo._instrumentValues.ZipFileName ?? RootName;
        private bool IsTtof => _testInfo.IsTtof;
        private bool IsPasef => _testInfo.IsPasef;
        private DiaSwathTestInfo.AnalysisValues _analysisValues => _testInfo._analysisValues;
        private DiaSwathTestInfo.InstrumentSpecificValues _instrumentValues => _testInfo._instrumentValues;

        [TestMethod]
        public void TestDiaTtofTutorial()
        {
            _testInfo.TestTtofData(false);
            RunTest();
        }

        [TestMethod, NoParallelTesting(TestExclusionReason.RESOURCE_INTENSIVE)] // Times out on slower worker VMs
        public void TestDiaTtofFullSearchTutorial()
        {
            _testInfo.TestTtofData(true);
            if (!IsCoverShotMode)
                RunTest();
        }


        [TestMethod]
        public void TestDiaQeTutorial()
        {
            _testInfo.TestQeData(false);
            if (!IsCoverShotMode)
                RunTest();
        }

        [TestMethod, NoParallelTesting(TestExclusionReason.RESOURCE_INTENSIVE)] // Times out on slower VMs
        public void TestDiaQeFullSearchTutorial()
        {
            _testInfo.TestQeData(true);
            if (!IsCoverShotMode)
                RunTest();
        }
       

        [TestMethod, NoParallelTesting(TestExclusionReason.VENDOR_FILE_LOCKING)] // Bruker wants exclusive read access to raw data
        [Timeout(int.MaxValue)] // These can take a long time
        public void TestDiaPasefTutorial()
        {
            _testInfo.TestPasefData(false);
            if (!IsCoverShotMode)
                RunTest();
        }

        [TestMethod,
         NoParallelTesting(TestExclusionReason.VENDOR_FILE_LOCKING), // Bruker wants exclusive read access to raw data
         NoNightlyTesting(TestExclusionReason.EXCESSIVE_TIME)] // Skip during Nightly
        [Timeout(int.MaxValue)] // These can take a long time
        public void TestDiaPasefFullDataset()
        {
            _testInfo.TestPasefData(true);
            if (!IsCoverShotMode)
                RunTest();
        }

        private void RunTest()
        {
            TestFilesZipPaths = _testInfo.TestFilesZipPaths;
            TestFilesPersistent = _testInfo.TestFilesPersistent;
            LinkPdf = _testInfo.LinkPdf;

//            IsPauseForScreenShots = true;
//            RunPerfTests = true;
//            IsCoverShotMode = true;
            CoverShotName = IsTtof ? "DIA-SWATH" : RootName;

            RunFunctionalTest();

            Assert.IsFalse(IsRecordMode, "Set IsRecordMode to false before commit");   // Make sure this doesn't get committed as true
        }

        private string DataPath { get { return TestFilesDirs.Last().PersistentFilesDir; } }

        private PropertyPath _resultProperty = PropertyPath.Root.Property("FoldChangeResult");
        private PropertyPath _proteinProperty = PropertyPath.Root.Property("Protein");

        private string GetTestPath(string path)
        {
            return TestFilesDirs[0].GetTestPath(Path.Combine(ZipFileName, path));
        }

        /// <summary>
        /// Change to true to write coefficient arrays.
        /// </summary>
        private bool IsRecordMode { get { return false; } }

        protected override void DoTest()
        {
            // Clean-up before running the test
            RunUI(() => SkylineWindow.ModifyDocument("Set default settings",
                d => d.ChangeSettings(SrmSettingsList.GetDefault())));
            RunUI(() => SkylineWindow.SetIntegrateAll(true));
            Settings.Default.CompactFormatOption = CompactFormatOption.NEVER.Name;

            SrmDocument doc = SkylineWindow.Document;

            string documentBaseName = "DIA-" + InstrumentTypeName + "-tutorial";
            string documentFile = TestFilesDirs[0].GetTestPath(documentBaseName + SrmDocument.EXT);
            RunUI(() => SkylineWindow.SaveDocument(documentFile));

            //var peptideSettingsDlg = ShowDialog<PeptideSettingsUI>(SkylineWindow.ShowPeptideSettingsUI);
            //RunUI(() => peptideSettingsDlg.Prediction.MeasuredRTWindow = 3);
            //OkDialog(peptideSettingsDlg, peptideSettingsDlg.OkDialog);

            // Launch the wizard
            var importPeptideSearchDlg = ShowDialog<ImportPeptideSearchDlg>(SkylineWindow.ShowImportPeptideSearchDlg);

            int screenshotPage = IsTtof ? 3 : 4;
            PauseForScreenShot<ImportPeptideSearchDlg.SpectraPage>("Import Peptide Search - Build Spectral Library empty page", screenshotPage++);

            // We're on the "Build Spectral Library" page of the wizard.
            // Add the test xml file to the search files list and try to 
            // build the document library.
            string[] searchFiles = _instrumentValues.SearchFiles.Select(o => GetTestPath(o)).ToArray();
            foreach (var searchFile in searchFiles)
                Assert.IsTrue(File.Exists(searchFile), string.Format("File {0} does not exist.", searchFile));

            RunUI(() =>
            {
                Assert.IsTrue(importPeptideSearchDlg.CurrentPage == ImportPeptideSearchDlg.Pages.spectra_page);
                importPeptideSearchDlg.BuildPepSearchLibControl.AddSearchFiles(searchFiles);
                importPeptideSearchDlg.BuildPepSearchLibControl.IrtStandards = _instrumentValues.IrtStandard;
                importPeptideSearchDlg.BuildPepSearchLibControl.WorkflowType = ImportPeptideSearchDlg.Workflow.dia;
            });
            WaitForConditionUI(() =>
                Equals(pwiz.BiblioSpec.Properties.Resources.BiblioSpecScoreType_DisplayName_PeptideProphet_confidence,
                    importPeptideSearchDlg.BuildPepSearchLibControl.Grid.Files.FirstOrDefault()?.ScoreType?.ToString()));
            RunUI(() =>
            {
                // Check default settings shown in the tutorial
                Assert.AreEqual(0.95, importPeptideSearchDlg.BuildPepSearchLibControl.Grid.Files.First().ScoreThreshold);
                Assert.IsFalse(importPeptideSearchDlg.BuildPepSearchLibControl.IncludeAmbiguousMatches);
            });
            WaitForConditionUI(() => importPeptideSearchDlg.IsNextButtonEnabled);
            PauseForScreenShot<ImportPeptideSearchDlg.SpectraPage>("Import Peptide Search - Build Spectral Library populated page", screenshotPage++);

            AddIrtPeptidesDlg addIrtPeptidesDlg;
            AddIrtStandardsDlg addIrtStandardsDlg = null;
            if (_instrumentValues.IrtStandard.IsAuto ||
                _instrumentValues.IrtStandard.Equals(IrtStandard.CIRT_SHORT) ||
                _instrumentValues.IrtStandard.Equals(IrtStandard.CIRT))
            {
                addIrtStandardsDlg = ShowDialog<AddIrtStandardsDlg>(() => importPeptideSearchDlg.ClickNextButton());
                RunUI(() => addIrtStandardsDlg.StandardCount = _instrumentValues.ExpectedIrtPeptideCount);
                PauseForScreenShot<AddIrtStandardsDlg>("Add Standard Peptides - Select number of CiRT peptides", screenshotPage);
                addIrtPeptidesDlg = ShowDialog<AddIrtPeptidesDlg>(addIrtStandardsDlg.OkDialog);
            }
            else
                addIrtPeptidesDlg = ShowDialog<AddIrtPeptidesDlg>(() => importPeptideSearchDlg.ClickNextButton());

            RunUI(() =>
            {
                // Check values shown in the tutorial
                Assert.AreEqual(1, addIrtPeptidesDlg.RunsConvertedCount);
                var row = addIrtPeptidesDlg.GetRow(0);
                if (IsRecordMode)
                {
                    Console.WriteLine();
                    Console.WriteLine($@"LibraryPeptideCount = {addIrtPeptidesDlg.PeptidesCount},");
                    Console.WriteLine($@"ExpectedIrtPeptideCount = {row.Cells[1].Value},");
                    Console.WriteLine(ParseIrtProperties(row.Cells[2].Value.ToString()));
                }
                else
                {
                    Assert.AreEqual(_instrumentValues.ExpectedIrtPeptideCount, row.Cells[1].Value);
                    Assert.AreEqual(_instrumentValues.LibraryPeptideCount, addIrtPeptidesDlg.PeptidesCount);
                    var regressionLine = new RegressionLine(_instrumentValues.IrtSlope, _instrumentValues.IrtIntercept);
                    Assert.AreEqual(regressionLine.DisplayEquation, row.Cells[2].Value);
                    //Assert.AreEqual(1.0, double.Parse(row.Cells[3].Value.ToString()));
                }

                Assert.AreEqual(Resources.AddIrtPeptidesDlg_AddIrtPeptidesDlg_Success, row.Cells[4].Value);
            });
            PauseForScreenShot("Add iRT peptides form", screenshotPage);
            if (addIrtStandardsDlg != null)
                screenshotPage++;

            var irtGraph = ShowDialog<GraphRegression>(() => addIrtPeptidesDlg.ShowRegression(0));
            PauseForScreenShot("iRT regression graph", screenshotPage++);

            OkDialog(irtGraph, irtGraph.CloseDialog);
            var recalibrateMessage = ShowDialog<MultiButtonMsgDlg>(addIrtPeptidesDlg.OkDialog);
            RunUI(() => Assert.AreEqual(TextUtil.LineSeparate(Resources.LibraryGridViewDriver_AddToLibrary_Do_you_want_to_recalibrate_the_iRT_standard_values_relative_to_the_peptides_being_added_,
                Resources.LibraryGridViewDriver_AddToLibrary_This_can_improve_retention_time_alignment_under_stable_chromatographic_conditions_), recalibrateMessage.Message));
            if (!_instrumentValues.HasAmbiguousMatches)
            {
                OkDialog(recalibrateMessage, recalibrateMessage.ClickNo);
            }
            else
            {
                var ambiguousDlg = ShowDialog<MessageDlg>(recalibrateMessage.ClickNo);
                RunUI(() => AssertEx.Contains(ambiguousDlg.Message,
                    Resources.BiblioSpecLiteBuilder_AmbiguousMatches_The_library_built_successfully__Spectra_matching_the_following_peptides_had_multiple_ambiguous_peptide_matches_and_were_excluded_));
                OkDialog(ambiguousDlg, ambiguousDlg.OkDialog);
            }
            WaitForConditionUI(() => importPeptideSearchDlg.CurrentPage == ImportPeptideSearchDlg.Pages.chromatograms_page);

            string docLibPath = BiblioSpecLiteSpec.GetLibraryFileName(documentFile);
            string redundantDocLibPath = BiblioSpecLiteSpec.GetRedundantName(docLibPath);
            Assert.IsTrue(File.Exists(docLibPath) && File.Exists(redundantDocLibPath));
            var librarySettings = SkylineWindow.Document.Settings.PeptideSettings.Libraries;
            Assert.IsTrue(librarySettings.HasDocumentLibrary);

            var importResults = importPeptideSearchDlg.ImportResultsControl as ImportResultsDIAControl;
            Assert.IsNotNull(importResults);
            string diaDir = GetTestPath("DIA");
            if (IsPauseForScreenShots)
            {
                // delete -diaumpire files so they don't show up for screenshots
                foreach (var file in Directory.GetFiles(diaDir, "*-diaumpire.*"))
                    FileEx.SafeDelete(file);
            }

            var openDataFiles = ShowDialog<OpenDataSourceDialog>(() => importResults.Browse(diaDir));
            RunUI(() =>
            {
                openDataFiles.SelectAllFileType(_instrumentValues.DiaFilesExtension);
                foreach (var selectedFile in openDataFiles.SelectedFiles)
                    Assert.IsTrue(DiaFiles.Contains(selectedFile));
            });
            PauseForScreenShot("Results files form", screenshotPage++);
            OkDialog(openDataFiles, openDataFiles.Open);

            WaitForConditionUI(() => importPeptideSearchDlg.IsNextButtonEnabled);
            RunUI(() =>
            {
                foreach (var resultFileName in importResults.FoundResultsFiles)
                    Assert.IsTrue(DiaFiles.Contains(Path.GetFileName(resultFileName.Path)));
            });
            PauseForScreenShot<ImportPeptideSearchDlg.ChromatogramsDiaPage>("Extract chromatograms page with files", screenshotPage++);
            if (IsPasef)
                screenshotPage++;   // Because IPS wizard is so tall with IMS added

            var importResultsNameDlg = ShowDialog<ImportResultsNameDlg>(() => importPeptideSearchDlg.ClickNextButton());
            OkDialog(importResultsNameDlg, importResultsNameDlg.YesDialog);

            WaitForConditionUI(() => importPeptideSearchDlg.CurrentPage ==
                                     ImportPeptideSearchDlg.Pages.match_modifications_page);

            // "Add Modifications" page
            RunUI(() =>
            {
                const string modOxidation = "Oxidation (M)";
                // Define expected matched/unmatched modifications
                var expectedMatched = !IsPasef ? new[] { modOxidation } : Array.Empty<string>();
                // Verify matched/unmatched modifications
                AssertEx.AreEqualDeep(expectedMatched, importPeptideSearchDlg.MatchModificationsControl.MatchedModifications.ToArray());
                Assert.IsFalse(importPeptideSearchDlg.MatchModificationsControl.UnmatchedModifications.Any());
            });
            PauseForScreenShot<ImportPeptideSearchDlg.MatchModsPage>("Modifications page - currently no screenshot", screenshotPage);
            RunUI(() => Assert.IsTrue(importPeptideSearchDlg.ClickNextButton()));
            WaitForConditionUI(() => importPeptideSearchDlg.CurrentPage == ImportPeptideSearchDlg.Pages.transition_settings_page);
            RunUI(() =>
            {
                importPeptideSearchDlg.TransitionSettingsControl.ExclusionUseDIAWindow = true;
                importPeptideSearchDlg.TransitionSettingsControl.PeptidePrecursorCharges = new[]
                {
                    Adduct.DOUBLY_PROTONATED, Adduct.TRIPLY_PROTONATED, Adduct.QUADRUPLY_PROTONATED
                };
                // Default is to have precursors
                if (_analysisValues.KeepPrecursors)
                {
                    AssertEx.AreEqualDeep(new[] {IonType.y, IonType.b, IonType.precursor},
                        importPeptideSearchDlg.TransitionSettingsControl.PeptideIonTypes);
                }
                else
                {
                    importPeptideSearchDlg.TransitionSettingsControl.PeptideIonTypes = new[]
                    {
                        IonType.y, IonType.b    // Removes precursor
                    };
                }
                // Verify other values shown in the tutorial
                Assert.AreEqual(50, importPeptideSearchDlg.TransitionSettingsControl.MinIonMz);
                Assert.AreEqual(2000, importPeptideSearchDlg.TransitionSettingsControl.MaxIonMz);
                Assert.AreEqual(TransitionFilter.StartFragmentFinder.ION_3.Label, importPeptideSearchDlg.TransitionSettingsControl.IonRangeFrom);
                Assert.AreEqual(TransitionFilter.EndFragmentFinder.LAST_ION.Label, importPeptideSearchDlg.TransitionSettingsControl.IonRangeTo);
                Assert.AreEqual(6, importPeptideSearchDlg.TransitionSettingsControl.IonCount);
                Assert.AreEqual(6, importPeptideSearchDlg.TransitionSettingsControl.MinIonCount);
                Assert.AreEqual(0.05, importPeptideSearchDlg.TransitionSettingsControl.IonMatchMzTolerance.Value);
                Assert.AreEqual(MzTolerance.Units.mz, importPeptideSearchDlg.TransitionSettingsControl.IonMatchMzTolerance.Unit);
                // CONSIDER: Not that easy to validate 1, 2 in ion charges.
            });
            PauseForScreenShot<ImportPeptideSearchDlg.TransitionSettingsPage>("Transition settings", screenshotPage++);
            RunUI(() => Assert.IsTrue(importPeptideSearchDlg.ClickNextButton()));

            // We're on the "Configure Full-Scan Settings" page of the wizard.
            RunUI(() =>
            {
                Assert.IsTrue(importPeptideSearchDlg.CurrentPage == ImportPeptideSearchDlg.Pages.full_scan_settings_page);
                if (_analysisValues.KeepPrecursors)
                    importPeptideSearchDlg.FullScanSettingsControl.PrecursorRes = 20;
                importPeptideSearchDlg.FullScanSettingsControl.ProductRes = 20;

                Assert.AreEqual(importPeptideSearchDlg.FullScanSettingsControl.PrecursorIsotopesCurrent, FullScanPrecursorIsotopes.None);
                Assert.AreEqual(FullScanMassAnalyzerType.centroided, importPeptideSearchDlg.FullScanSettingsControl.ProductMassAnalyzer);
                Assert.AreEqual(RetentionTimeFilterType.scheduling_windows, importPeptideSearchDlg.FullScanSettingsControl.RetentionTimeFilterType);
                Assert.AreEqual(5, importPeptideSearchDlg.FullScanSettingsControl.TimeAroundPrediction);
            });

            var isolationScheme =
                ShowDialog<EditIsolationSchemeDlg>(importPeptideSearchDlg.FullScanSettingsControl.AddIsolationScheme);
            RunUI(() =>
            {
                isolationScheme.IsolationSchemeName = _instrumentValues.IsolationSchemeName;
                isolationScheme.UseResults = false;
            });
            RunDlg<OpenDataSourceDialog>(isolationScheme.ImportRanges, importRangesDlg =>
            {
                importRangesDlg.CurrentDirectory = new MsDataFilePath(diaDir);
                importRangesDlg.SelectFile(DiaFiles[0]);
                importRangesDlg.Open();
            });
            string schemePath = Path.Combine("DIA", _instrumentValues.IsolationSchemeFile);
            var schemeLines = File.ReadAllLines(GetTestPath(schemePath));
            string[][] windowFields = schemeLines.Select(l =>
                TextUtil.ParseDsvFields(l, _instrumentValues.IsolationSchemeFileSeparator)).ToArray();
            WaitForConditionUI(() => isolationScheme.GetIsolationWindows().Count == schemeLines.Length);

            RunUI(() =>
            {
                bool hasMargin = windowFields[0].Length == 3;
                if (hasMargin)
                    Assert.IsTrue(isolationScheme.SpecifyMargin);
                else
                    Assert.IsFalse(isolationScheme.SpecifyMargin);
                int schemeRow = 0;
                foreach (var isolationWindow in isolationScheme.GetIsolationWindows())
                {
                    var fields = windowFields[schemeRow++];
                    Assert.AreEqual(double.Parse(fields[0], CultureInfo.InvariantCulture), isolationWindow.MethodStart, 0.01);
                    Assert.AreEqual(double.Parse(fields[1], CultureInfo.InvariantCulture), isolationWindow.MethodEnd, 0.01);
                    if (hasMargin)
                        Assert.AreEqual(double.Parse(fields[2], CultureInfo.InvariantCulture), isolationWindow.StartMargin ?? 0, 0.01);
                }
            });
            screenshotPage++;   // One page without a screenshot
            PauseForScreenShot("Isolation scheme", screenshotPage++);

            var isolationGraph = ShowDialog<DiaIsolationWindowsGraphForm>(isolationScheme.OpenGraph);
            PauseForScreenShot("Isolation scheme graph", screenshotPage++);

            OkDialog(isolationGraph, isolationGraph.CloseButton);
            OkDialog(isolationScheme, isolationScheme.OkDialog);

            if (IsPasef)
                PauseForScreenShot<ImportPeptideSearchDlg.ImsFullScanPage>("Import Peptide Search - Configure Full-Scan Settings page", screenshotPage++);
            else
                PauseForScreenShot<ImportPeptideSearchDlg.Ms2FullScanPage>("Import Peptide Search - Configure Full-Scan Settings page", screenshotPage++);

            WaitForConditionUI(() => importPeptideSearchDlg.IsNextButtonEnabled);
            RunUI(() => Assert.IsTrue(importPeptideSearchDlg.ClickNextButton()));

            string fastPath = GetTestPath(_analysisValues.FastaPath);
            RunUI(() =>
            {
                Assert.IsTrue(importPeptideSearchDlg.CurrentPage == ImportPeptideSearchDlg.Pages.import_fasta_page);
                Assert.AreEqual("Trypsin [KR | P]", importPeptideSearchDlg.ImportFastaControl.Enzyme.GetKey());
                Assert.AreEqual(0, importPeptideSearchDlg.ImportFastaControl.MaxMissedCleavages);
                importPeptideSearchDlg.ImportFastaControl.SetFastaContent(fastPath);
                Assert.IsTrue(importPeptideSearchDlg.ImportFastaControl.DecoyGenerationEnabled);
                importPeptideSearchDlg.ImportFastaControl.DecoyGenerationMethod =
                    Resources.DecoyGeneration_SHUFFLE_SEQUENCE_Shuffle_Sequence;
                importPeptideSearchDlg.ImportFastaControl.AutoTrain = true;
                Assert.IsTrue(importPeptideSearchDlg.ImportFastaControl.ContainsFastaContent);
            });
            PauseForScreenShot<ImportPeptideSearchDlg.FastaPage>("Import Peptide Search - Import FASTA page", screenshotPage++);

            if (IsRecordMode)
                Console.WriteLine();

            var peptidesPerProteinDlg = ShowDialog<AssociateProteinsDlg>(() => importPeptideSearchDlg.ClickNextButton());
            WaitForCondition(() => peptidesPerProteinDlg.DocumentFinalCalculated);
            if (IsRecordMode)
                Console.WriteLine();    // Line break after test run information
            RunUI(() =>
            {
                //int proteinCount, peptideCount, precursorCount, transitionCount;
                //peptidesPerProteinDlg.NewTargetsAll(out proteinCount, out peptideCount, out precursorCount, out transitionCount);
                //ValidateTargets(_analysisValues.TargetCounts, proteinCount, peptideCount, precursorCount, transitionCount, @"TargetCounts");
                if (_analysisValues.RemoveDuplicates)
                    peptidesPerProteinDlg.RemoveDuplicatePeptides = true;
                if (_analysisValues.MinPeptidesPerProtein.HasValue)
                    peptidesPerProteinDlg.MinPeptides = _analysisValues.MinPeptidesPerProtein.Value;
            });
            WaitForConditionUI(() => peptidesPerProteinDlg.DocumentFinalCalculated);
            RunUI(() =>
            {
                int proteinCount, peptideCount, precursorCount, transitionCount;
                peptidesPerProteinDlg.NewTargetsFinal(out proteinCount, out peptideCount, out precursorCount, out transitionCount);
                _testInfo.ValidateTargets(IsRecordMode, _analysisValues.FinalTargetCounts, proteinCount, peptideCount, precursorCount, transitionCount, @"FinalTargetCounts");
            });
            PauseForScreenShot("Import FASTA summary form", screenshotPage);

            OkDialog(peptidesPerProteinDlg, peptidesPerProteinDlg.OkDialog);
            //PauseTest();
            PauseForScreenShot<AllChromatogramsGraph>("Loading chromatograms window", screenshotPage++, 30*1000); // 30 second timeout to avoid getting stuck
            WaitForDocumentChangeLoaded(doc, 20 * 60 * 1000); // 20 minutes

            var peakScoringModelDlg = WaitForOpenForm<EditPeakScoringModelDlg>();
            PauseForScreenShot("mProphet model form", screenshotPage++);
            ValidateCoefficients(peakScoringModelDlg, _analysisValues.ScoringModelCoefficients);

            OkDialog(peakScoringModelDlg, peakScoringModelDlg.OkDialog);

            var docLibrary = SkylineWindow.Document.Settings.PeptideSettings.Libraries.Libraries[0];
            Assert.AreEqual(_instrumentValues.LibraryPeptideCount + _instrumentValues.ExpectedIrtPeptideCount, docLibrary.LibraryDetails.UniquePeptideCount);
            RunUI(() => Assert.AreEqual(_instrumentValues.ExpectedIrtPeptideCount, SkylineWindow.Document.PeptideGroups.First().PeptideCount));

            // Setup annotations
            var documentSettingsDlg = ShowDialog<DocumentSettingsDlg>(SkylineWindow.ShowDocumentSettingsDialog);

            AddReplicateAnnotation(documentSettingsDlg, "Condition", AnnotationDef.AnnotationType.value_list,
                new[] { "A", "B" }, screenshotPage++);

            AddReplicateAnnotation(documentSettingsDlg, "BioReplicate");

            RunUI(() =>
            {
                documentSettingsDlg.AnnotationsCheckedListBox.SetItemChecked(0, true);
                documentSettingsDlg.AnnotationsCheckedListBox.SetItemChecked(1, true);
            });

            OkDialog(documentSettingsDlg, documentSettingsDlg.OkDialog);

            // Annotate replicates in Document Grid: Replicates
            var documentGrid = ShowDialog<DocumentGridForm>(() => SkylineWindow.ShowDocumentGrid(true));
            RunUI(() =>
            {
                documentGrid.ChooseView(Resources.SkylineViewContext_GetDocumentGridRowSources_Replicates);
                FormEx.GetParentForm(documentGrid).Size = new Size(591, 283);
            });
            WaitForConditionUI(() => documentGrid.IsComplete); // Let it initialize

            RunUI(() =>
            {
                var pathCondition = PropertyPath.Root.Property(AnnotationDef.GetColumnName("Condition"));
                var columnSubjectId = documentGrid.FindColumn(pathCondition);
                var gridView = documentGrid.DataGridView;
                gridView.CurrentCell = gridView.Rows[0].Cells[columnSubjectId.Index];
            });

            var replicateAnnotations = new[]
            {
                new[] {"A", "1"},
                new[] {"B", "1"},
                new[] {"A", "2"},
                new[] {"B", "2"},
                new[] {"A", "3"},
                new[] {"B", "3"}
            };
            SetClipboardText(TextUtil.LineSeparate(replicateAnnotations.Select(TextUtil.ToEscapedTSV)));

            RunUI(() => documentGrid.DataGridView.SendPaste());

            PauseForScreenShot<DocumentGridForm>("Document Grid - filled", screenshotPage++);

            RunUI(() => SkylineWindow.ShowDocumentGrid(false));

            // Arrange windows for manual inspection
            var arrangeGraphsDlg = ShowDialog<ArrangeGraphsGroupedDlg>(SkylineWindow.ArrangeGraphsGrouped);
            RunUI(() =>
            {
                arrangeGraphsDlg.Groups = 2;
                arrangeGraphsDlg.GroupType = GroupGraphsType.distributed;
                arrangeGraphsDlg.GroupOrder = GroupGraphsOrder.Document;
                arrangeGraphsDlg.DisplayType = DisplayGraphsType.Row;
            });

            OkDialog(arrangeGraphsDlg, arrangeGraphsDlg.OkDialog);

            RunUI(() => SkylineWindow.SaveDocument());

            const string proteinNameToSelect = "sp|P63284|CLPB_ECOLI";
            if (Equals(proteinNameToSelect, SkylineWindow.Document.MoleculeGroups.Skip(1).First().Name))
            SelectNode(SrmDocument.Level.MoleculeGroups, 1);
            else
                FindNode(proteinNameToSelect);

            RunUI(() =>
            {
                Assert.AreEqual(proteinNameToSelect, SkylineWindow.SelectedNode.Text);

                SkylineWindow.ShowPeakAreaReplicateComparison();
                SkylineWindow.ShowRTReplicateGraph();
                SkylineWindow.Size = new Size(1226, 900);
            });
            RestoreViewOnScreenNoSelChange(18);
            WaitForGraphs();
            screenshotPage++;   // Docking drag-drop image page
            PauseForScreenShot("Manual review window layout with protein selected", screenshotPage++);

            FindNode(_instrumentValues.ExamplePeptide);
            WaitForGraphs();
            PauseForScreenShot("Manual review window layout with peptide selected", screenshotPage++);

            FindNode("_HUMAN");
            WaitForGraphs();
            FindNode(_instrumentValues.ExamplePeptide);
            RunUI(SkylineWindow.AutoZoomBestPeak);
            WaitForGraphs();
            PauseForScreenShot("Snip just one chromatogram pane", screenshotPage);

            try
            {
                ClickChromatogram(SkylineWindow.Document.MeasuredResults.Chromatograms[0].Name,
                    _analysisValues.ChromatogramClickPoint.X,
                    _analysisValues.ChromatogramClickPoint.Y);
            }
            catch (AssertFailedException)
            {
                if (!IsRecordMode)
                    throw;
                PauseAndContinueForm.Show($"Clicking the left-side chromatogram at ({_analysisValues.ChromatogramClickPoint.X}, {_analysisValues.ChromatogramClickPoint.Y}) failed.\r\n" +
                                          "Click on and record a new ChromatogramClickPoint at the peak of that chromatogram.");
            }

            PauseForScreenShot<GraphFullScan>("Full-Scan graph window - zoomed", screenshotPage++);

            if (IsPasef)
            {
                RunUI(() => SkylineWindow.GraphFullScan.ShowMobility(true));
                WaitForGraphs();
                PauseForScreenShot<GraphFullScan>("Full-Scan graph window - mobility zoomed", screenshotPage++);
            }

            RunUI(() => SkylineWindow.GraphFullScan.ZoomToSelection(false));
            WaitForGraphs();
            PauseForScreenShot<GraphFullScan>("Full-Scan graph window - unzoomed", screenshotPage++);

            RunUI(SkylineWindow.GraphFullScan.Close);
            RunUI(SkylineWindow.ShowMassErrorHistogramGraph);
            if (IsPasef)
            {
                // diaPASEF has an outlier in a poor scoring peak
                RunUI(() => SkylineWindow.ShowPointsTypeMassError(PointsTypeMassError.targets_1FDR));
            }
            WaitForGraphs();
            Assert.IsTrue(SkylineWindow.GraphMassError.TryGetGraphPane(out MassErrorHistogramGraphPane massErrorPane));
            int massErrorStatsIndex = 0;
            if (IsRecordMode)
            {
                Console.WriteLine(@"MassErrorStats = new[]");
                Console.WriteLine(@"{");
            }
            ValidateMassErrors(massErrorPane, massErrorStatsIndex++);

            // CONSIDER: No way to specify mass error graph window in PauseForScreenShot or ShowDialog
            PauseForScreenShot("Mass errors histogram graph window", screenshotPage++);

            // Review single replicates
            RunUI(SkylineWindow.ShowSingleReplicate);
            foreach (var chromatogramSet in SkylineWindow.Document.MeasuredResults.Chromatograms)
            {
                RunUI(() => SkylineWindow.ActivateReplicate(chromatogramSet.Name));
                WaitForGraphs();
                ValidateMassErrors(massErrorPane, massErrorStatsIndex++);
            }
            if (IsRecordMode)
            {
                Console.WriteLine(@"},");
            }

            RunUI(() =>
            {
                SkylineWindow.ShowPointsTypeMassError(PointsTypeMassError.decoys);
                SkylineWindow.ShowAverageReplicates();
            });
            WaitForGraphs();
            RunUI(() => SkylineWindow.ShowPointsTypeMassError(PointsTypeMassError.targets));    // CONSIDER: 1% FDR
            RunUI(() => SkylineWindow.ShowGraphMassError(false));

            RunUI(SkylineWindow.ShowRTRegressionGraphScoreToRun);
            WaitForGraphs();
            RestoreViewOnScreenNoSelChange(24);
            PauseForScreenShot("Retention time regression graph window - regression", screenshotPage++);

            RunUI(() => SkylineWindow.ShowPlotType(PlotTypeRT.residuals));
            WaitForGraphs();
            PauseForScreenShot("Retention time regression graph window - residuals", screenshotPage++);
            RunUI(() => SkylineWindow.ShowGraphRetentionTime(false, GraphTypeSummary.score_to_run_regression));

            var editGroupComparisonDlg = ShowDialog<EditGroupComparisonDlg>(SkylineWindow.AddGroupComparison);
            const string groupComparisonName = @"By Condition";
            RunUI(() =>
            {
                editGroupComparisonDlg.TextBoxName.Text = groupComparisonName;
                editGroupComparisonDlg.ComboControlAnnotation.SelectedItem = @"Condition";
            });
            WaitForConditionUI(() => editGroupComparisonDlg.ComboControlValue.Items.Count > 0);
            RunUI(() =>
            {
                editGroupComparisonDlg.ComboControlValue.SelectedItem = "A";
                editGroupComparisonDlg.ComboCaseValue.SelectedItem = "B";
                editGroupComparisonDlg.ComboIdentityAnnotation.SelectedItem = "BioReplicate";   // Irrelevant
                editGroupComparisonDlg.ShowAdvanced(true);
                editGroupComparisonDlg.TextBoxQValueCutoff.Text = (0.01).ToString(CultureInfo.CurrentCulture);
            });
            PauseForScreenShot("Group comparison", screenshotPage++);

            OkDialog(editGroupComparisonDlg, editGroupComparisonDlg.OkDialog);
            RunUI(() => SkylineWindow.ShowGroupComparisonWindow(groupComparisonName));
            {
                var fcGrid = WaitForOpenForm<FoldChangeGrid>();
                var fcGridControl = fcGrid.DataboundGridControl;
                WaitForConditionUI(() => fcGridControl.IsComplete && fcGridControl.FindColumn(_resultProperty) != null && fcGridControl.RowCount > 11);
                RunUI(() =>
                {
                    var foldChangeResultColumn = fcGridControl.FindColumn(_resultProperty);
                    fcGridControl.DataGridView.AutoResizeColumn(foldChangeResultColumn.Index);
                    var proteinNameColumn = fcGridControl.FindColumn(_proteinProperty);
                    fcGridControl.DataGridView.AutoResizeColumn(proteinNameColumn.Index);
                    fcGridControl.DataGridView.FirstDisplayedScrollingRowIndex = 11;  // Scroll past iRT peptides
                });
                WaitForConditionUI(() => 0 != fcGridControl.RowCount, "0 != foldChangeGrid.DataboundGridControl.RowCount");
                WaitForConditionUI(() => fcGridControl.IsComplete, "foldChangeGrid.DataboundGridControl.IsComplete");
                PauseForScreenShot<FoldChangeGrid>("By Condition grid", screenshotPage);

                var volcanoPlot = ShowDialog<FoldChangeVolcanoPlot>(fcGrid.ShowVolcanoPlot);
                RestoreViewOnScreenNoSelChange(27);
                fcGrid = WaitForOpenForm<FoldChangeGrid>();
                WaitForConditionUI(() => fcGrid.DataboundGridControl.IsComplete && fcGrid.DataboundGridControl.RowCount > 11);
                RunUI(() => fcGrid.DataboundGridControl.DataGridView.FirstDisplayedScrollingRowIndex = 11); // Re-apply scrolling
                PauseForScreenShot<FoldChangeVolcanoPlot>("By Condition:Volcano Plot - unformatted", screenshotPage++);
                volcanoPlot = WaitForOpenForm<FoldChangeVolcanoPlot>();    // May have changed with RestoreViewsOnScreen
                WaitForConditionUI(() => volcanoPlot.CurveList.Count == 5);
                WaitForConditionUI(() => volcanoPlot.CurveList[4].Points.Count > SkylineWindow.Document.MoleculeCount/4);
                RunUI(() =>
                {
                    int actualPoints = volcanoPlot.CurveList[4].Points.Count;
                    if (IsRecordMode)
                        Console.Write(@"DiffPeptideCounts = new[] { " + actualPoints);
                    else
                        Assert.AreEqual(_analysisValues.DiffPeptideCounts[0], actualPoints);
                });
                var formattingDlg = ShowDialog<VolcanoPlotFormattingDlg>(volcanoPlot.ShowFormattingDialog);
                ApplyFormatting(formattingDlg, "ECOLI", "128, 0, 255");
                var createExprDlg = ShowDialog<CreateMatchExpressionDlg>(() =>
                {
                    var bindingList = formattingDlg.GetCurrentBindingList();
                    formattingDlg.ClickCreateExpression(bindingList.Count - 1);
                });
                PauseForScreenShot("Create Expression form", screenshotPage++);
                OkDialog(createExprDlg, createExprDlg.OkDialog);

                ApplyFormatting(formattingDlg, "YEAS", "255, 128, 0");
                ApplyFormatting(formattingDlg, "HUMAN", "0, 128, 0");
                PauseForScreenShot("Volcano plot formatting form", screenshotPage);
                OkDialog(formattingDlg, formattingDlg.OkDialog);
                //PauseTest();
                WaitForConditionUI(() => volcanoPlot.CurveList.Count == 8 &&
                                         volcanoPlot.CurveList[7].Points.Count == _instrumentValues.ExpectedIrtPeptideCount); // iRTs
                for (int i = 1; i < 4; i++)
                {
                    RunUI(() =>
                    {
                        int actualPoints = volcanoPlot.CurveList[7 - i].Points.Count;
                        if (IsRecordMode)
                            Console.Write(@", " + actualPoints);
                        else
                            Assert.AreEqual(_analysisValues.DiffPeptideCounts[i], actualPoints);
                    });
                }
                if (IsRecordMode)
                    Console.WriteLine(@" },");
                PauseForScreenShot<FoldChangeVolcanoPlot>("By Condition:Volcano Plot - fully formatted", screenshotPage++);
            }

            {
                var fcGrid = WaitForOpenForm<FoldChangeGrid>(); // May have changed with RestoreViewsOnScreen
                RunUI(fcGrid.ShowGraph);
                RestoreViewOnScreenNoSelChange(30);
            }

            {
                var fcGrid = WaitForOpenForm<FoldChangeGrid>(); // May have changed with RestoreViewsOnScreen
                var fcGridControl = fcGrid.DataboundGridControl;
                FilterIrtProtein(fcGridControl);

                var volcanoPlot = WaitForOpenForm<FoldChangeVolcanoPlot>();    // May have changed with RestoreViewsOnScreen
                WaitForConditionUI(() => volcanoPlot.CurveList.Count == 8 && 
                                         volcanoPlot.CurveList[7].Points.Count == 0); // No iRTs
                if (!IsRecordMode)
                {
                    for (int i = 1; i < 4; i++)
                        RunUI(() => Assert.AreEqual(_analysisValues.DiffPeptideCounts[i], volcanoPlot.CurveList[7 - i].Points.Count));
                }
                var barGraph = WaitForOpenForm<FoldChangeBarGraph>();
                int volcanoBarDelta = _instrumentValues.ExpectedIrtPeptideCount - 1; // iRTs - selected peptide
                if (!IsRecordMode)
                    WaitForBarGraphPoints(barGraph, _analysisValues.DiffPeptideCounts[0] - volcanoBarDelta);

                SortByFoldChange(fcGridControl, _resultProperty);
                PauseForScreenShot<FoldChangeBarGraph>("By Condition:Bar Graph - peptides", screenshotPage++);

                var changeGroupComparisonSettings = ShowDialog<EditGroupComparisonDlg>(fcGrid.ShowChangeSettings);
                RunUI(() => changeGroupComparisonSettings.RadioScopePerProtein.Checked = true);

                int targetProteinCount = SkylineWindow.Document.MoleculeGroupCount - 2; // minus iRTs and decoys
                int unpolishedCount = _analysisValues.UnpolishedProteins;
                if (!IsRecordMode)
                    WaitForBarGraphPoints(barGraph, unpolishedCount);
                else
                {
                    WaitForBarGraphPoints(barGraph, targetProteinCount, 1);
                    unpolishedCount = GetBarCount(barGraph);
                    Console.WriteLine(@"UnpolishedProteins = {0},", unpolishedCount);
                }

                RunUI(() => changeGroupComparisonSettings.ComboSummaryMethod.SelectedItem =
                    SummarizationMethod.MEDIANPOLISH);

                if (!IsRecordMode)
                    WaitForBarGraphPoints(barGraph, _analysisValues.PolishedProteins ?? targetProteinCount);
                else
                {
                    WaitForBarGraphPoints(barGraph, targetProteinCount, unpolishedCount);
                    if (GetBarCount(barGraph) != targetProteinCount)
                        Console.WriteLine(@"PolishedProteins = {0},", GetBarCount(barGraph));
                }
                fcGrid = WaitForOpenForm<FoldChangeGrid>();
                var fcGridControlFinal = fcGrid.DataboundGridControl;
                SortByFoldChange(fcGridControlFinal, _resultProperty);  // Re-apply the sort, in case it was lost in restoring views

                RestoreViewOnScreen(31);
                PauseForScreenShot<FoldChangeBarGraph>("By Condition:Bar Graph - proteins", screenshotPage);

                RunQValueSummaryTest();

                if (IsCoverShotMode)
                {
                    RunUI(() =>
                    {
                        Settings.Default.ChromatogramFontSize = 14;
                        Settings.Default.AreaFontSize = 14;
                        SkylineWindow.ChangeTextSize(TreeViewMS.LRG_TEXT_FACTOR);
                    });

                    RestoreCoverViewOnScreen();
                    fcGrid = WaitForOpenForm<FoldChangeGrid>();
                    fcGridControlFinal = fcGrid.DataboundGridControl;
                    FilterIrtProtein(fcGridControlFinal);
                    changeGroupComparisonSettings = ShowDialog<EditGroupComparisonDlg>(fcGrid.ShowChangeSettings);
                    RunUI(() => changeGroupComparisonSettings.RadioScopePerPeptide.Checked = true);
                    OkDialog(changeGroupComparisonSettings, changeGroupComparisonSettings.Close);

                    RunUI(() =>
                    {
                        var fcFloatingWindow = fcGrid.Parent.Parent;
                        fcFloatingWindow.Left = SkylineWindow.Left + 8;
                        fcFloatingWindow.Top = SkylineWindow.Bottom - fcFloatingWindow.Height - 8;
                    });

                    if (IsPasef)
                    {
                        ClickChromatogram(SkylineWindow.Document.MeasuredResults.Chromatograms[0].Name,
                            1.2642E+01, 1.0521E+04);
                        RunUI(() => SkylineWindow.ShowChromatogramLegends(false));
                        RunUI(() => SkylineWindow.GraphFullScan.SetZoom(true));
                        WaitForGraphs();
                    }

                    TakeCoverShot();
                }
            }
        }

        private void RunQValueSummaryTest()
        {
            if (!IsTtof || _analysisValues.IsWholeProteome)
                return;

            var exportReportDlg = ShowDialog<ExportLiveReportDlg>(SkylineWindow.ShowExportReportDialog);
            var editReportListDlg = ShowDialog<ManageViewsForm>(exportReportDlg.EditList);
            var viewEditor = ShowDialog<ViewEditor>(editReportListDlg.AddView);
            string qValuesReportName = "Q-ValuesTest";
            string reportFileName = TestFilesDirs[0].GetTestPath(qValuesReportName + ".csv");
            RunUI(() =>
            {
                viewEditor.ViewName = qValuesReportName;
                var columnsToAdd = new[]
                {
                    // Not L10N
                    PropertyPath.Parse("Proteins!*.Peptides!*.Precursors!*.NeutralMass"),
                    PropertyPath.Parse("Proteins!*.Peptides!*.Precursors!*.Results!*.Value.DetectionQValue"),
                    PropertyPath.Parse("Proteins!*.Peptides!*.Precursors!*.ResultSummary.DetectionQValue.Min"),
                    PropertyPath.Parse("Proteins!*.Peptides!*.Precursors!*.ResultSummary.DetectionQValue.Max"),
                    PropertyPath.Parse(
                        "Proteins!*.Peptides!*.Precursors!*.ResultSummary.DetectionQValue.Median")
                };
                foreach (var id in columnsToAdd)
                {
                    Assert.IsTrue(viewEditor.ChooseColumnsTab.TrySelect(id), "Unable to select {0}", id);
                    viewEditor.ChooseColumnsTab.AddSelectedColumn();
                }

                viewEditor.ViewEditorWidgets.OfType<PivotReplicateAndIsotopeLabelWidget>().First().SetPivotReplicate(true);
            });

            OkDialog(viewEditor, () => viewEditor.OkDialog());
            OkDialog(editReportListDlg, () => editReportListDlg.OkDialog());

            RunUI(() =>
            {
                exportReportDlg.ReportName = qValuesReportName;
                exportReportDlg.OkDialog(reportFileName, TextUtil.SEPARATOR_CSV); // Not L10N
            });

            foreach (var line in File.ReadLines(reportFileName).Skip(1))
            {
                var values = line.Split(',').Select((strValue) =>
                    double.TryParse(strValue, out var res) ? res : double.NaN
                ).ToArray();
                //skip the line if it has any N/As in it
                if (!values.Any(double.IsNaN))
                {
                    var stats = new Statistics(values.Skip(4));
                    //using numeric comparison due to rounding errors
                    Assert.IsTrue(Math.Abs(values[1] - stats.Min()) <= 0.0001);
                    Assert.IsTrue(Math.Abs(values[2] - stats.Max()) <= 0.0001);
                    Assert.IsTrue(Math.Abs(values[3] - stats.Median()) <= 0.0001);
                }
            }
        }

        private void FilterIrtProtein(DataboundGridControl fcGridControl)
        {
            WaitForConditionUI(() => fcGridControl.IsComplete && fcGridControl.FindColumn(_proteinProperty) != null);
            var quickFilterForm = ShowDialog<QuickFilterForm>(() =>
            {
                var proteinNameColumn = fcGridControl.FindColumn(_proteinProperty);
                fcGridControl.QuickFilter(proteinNameColumn);
            });
            RunUI(() =>
            {
                quickFilterForm.SetFilterOperation(0, FilterOperations.OP_NOT_CONTAINS);
                quickFilterForm.SetFilterOperand(0, _analysisValues.IrtFilterText);
            });
            OkDialog(quickFilterForm, quickFilterForm.OkDialog);
        }

        private void RestoreViewOnScreenNoSelChange(int pageName)
        {
            if (!Program.SkylineOffscreen)
            {
                RunUI(() =>
                {
                    var selectedPath = SkylineWindow.SelectedPath;
                    RestoreViewOnScreen(pageName);
                    SkylineWindow.SelectedPath = selectedPath;
                });
            }
        }

        private void ApplyFormatting(VolcanoPlotFormattingDlg formattingDlg, string matchText, string rgbText)
        {
            RunUI(() =>
            {
                var bindingList = formattingDlg.GetCurrentBindingList();
                var color = RgbHexColor.ParseRgb(rgbText).Value;
                bindingList.Add(new MatchRgbHexColor("ProteinName: " + matchText, false, color, PointSymbol.Circle, PointSize.normal));
            });
        }

        private void ValidateCoefficients(EditPeakScoringModelDlg editDlgFromSrm, string expectedCoefficients)
        {
            string coefficients = string.Join(@"|", GetCoefficientStrings(editDlgFromSrm));
            try
            {
                AssertEx.AreEqualLines(expectedCoefficients, coefficients);
            }
            catch (AssertFailedException e)
            {
                if (IsRecordMode)
                {
                    Console.WriteLine(@"ScoringModelCoefficients = ""{0}"",", coefficients); // Not L10N
                    Console.Error.WriteLine("ScoringModelCoefficients: " + e.Message);
                }
                else
                    throw;
            }
        }

        private void ValidateMassErrors(MassErrorHistogramGraphPane massErrorPane, int index)
        {
            double mean = massErrorPane.Mean, stdDev = massErrorPane.StdDev;
            if (IsRecordMode)
                Console.WriteLine(@"new[] {{{0:0.0}, {1:0.0}}},", mean, stdDev);  // Not L10N
            else
            {
                Assert.AreEqual(_analysisValues.MassErrorStats[index][0], mean, 0.05);
                Assert.AreEqual(_analysisValues.MassErrorStats[index][1], stdDev, 0.05);
            }
        }

        private void WaitForBarGraphPoints(FoldChangeBarGraph barGraph, int barCount, int? lowerBoundCount = null)
        {
            WaitForConditionUI(() => barGraph.ZedGraphControl.GraphPane.CurveList.Count == 1);
            if (!lowerBoundCount.HasValue)
            {
                WaitForConditionUI(() => barCount == GetBarCount(barGraph),
                    () => string.Format("Expecting {0} bars, actual {1} bars", barCount, GetBarCount(barGraph)));
            }
            else
            {
                WaitForConditionUI(() => lowerBoundCount.Value <= GetBarCount(barGraph) && GetBarCount(barGraph) <= barCount,
                    () => string.Format("Expecting >= {0} and <= {1} bars, actual {2} bars", lowerBoundCount.Value, barCount, GetBarCount(barGraph)));
            }
        }

        private int GetBarCount(FoldChangeBarGraph barGraph)
        {
            return barGraph.ZedGraphControl.GraphPane.CurveList[0].Points.Count;
        }

        private static void SortByFoldChange(DataboundGridControl fcGridControl, PropertyPath fcResultProperty)
        {
            RunUI(() =>
            {
                var fcResultColumn = fcGridControl.FindColumn(fcResultProperty);
                fcGridControl.SetSortDirection(fcGridControl.GetPropertyDescriptor(fcResultColumn),
                    ListSortDirection.Ascending);
            });
        }
    }

    [TestClass]
    public class DiaSwathCommandLineTest : AbstractUnitTestEx
    {
        private DiaSwathTestInfo _testInfo = new DiaSwathTestInfo();
        private string InstrumentTypeName => _testInfo._instrumentValues.InstrumentTypeName;
        private string RootName => _testInfo.RootName;
        private string ZipFileName => _testInfo._instrumentValues.ZipFileName ?? RootName;
        private DiaSwathTestInfo.AnalysisValues _analysisValues => _testInfo._analysisValues;
        private DiaSwathTestInfo.InstrumentSpecificValues _instrumentValues => _testInfo._instrumentValues;

        [TestMethod]
        public void ConsoleTestDiaTtof()
        {
            _testInfo.TestTtofData(false);
            RunTest();
        }

        [TestMethod, NoParallelTesting(TestExclusionReason.RESOURCE_INTENSIVE)] // Times out on slower worker VMs
        public void ConsoleTestDiaTtofFullSearch()
        {
            _testInfo.TestTtofData(true);
            RunTest();
        }


        [TestMethod]
        public void ConsoleTestDiaQe()
        {
            _testInfo.TestQeData(false);
            RunTest();
        }

        [TestMethod, NoParallelTesting(TestExclusionReason.RESOURCE_INTENSIVE)] // Times out on slower VMs
        public void ConsoleTestDiaQeFullSearch()
        {
            _testInfo.TestQeData(true);
            RunTest();
        }

        [TestMethod, NoParallelTesting(TestExclusionReason.VENDOR_FILE_LOCKING)] // Bruker wants exclusive read access to raw data
        [Timeout(int.MaxValue)] // These can take a long time
        public void ConsoleTestDiaPasef()
        {
            _testInfo.TestPasefData(false);
            RunTest();
        }

        [TestMethod,
         NoParallelTesting(TestExclusionReason.VENDOR_FILE_LOCKING), // Bruker wants exclusive read access to raw data
         NoNightlyTesting(TestExclusionReason.EXCESSIVE_TIME)] // Skip during Nightly
        [Timeout(int.MaxValue)] // These can take a long time
        public void ConsoleTestDiaPasefFullDataset()
        {
            _testInfo.TestPasefData(true);
            RunTest();
        }

        private string GetTestPath(string path)
        {
            return TestFilesDirs[0].GetTestPath(Path.Combine(ZipFileName, path));
        }

        private void RunTest()
        {
            if (!RunPerfTests)
                return;

            TestFilesZipPaths = _testInfo.TestFilesZipPaths;
            TestFilesPersistent = _testInfo.TestFilesPersistent;

            // Unzip test files.
            if (TestFilesZipPaths != null)
            {
                TestFilesDirs = new TestFilesDir[TestFilesZipPaths.Length];
                for (int i = 0; i < TestFilesZipPaths.Length; i++)
                {
                    TestFilesDirs[i] = new TestFilesDir(TestContext, TestFilesZipPaths[i], TestDirectoryName,
                        TestFilesPersistent, IsExtractHere(i));
                }
            }

            string documentBaseName = "DIA-" + InstrumentTypeName + "-tutorial-cli";
            string documentFile = TestFilesDirs[0].GetTestPath(documentBaseName + SrmDocument.EXT);

            // arguments that would normally be quoted on the command-line shouldn't be quoted here
            var settings = new[]
            {
                "--new=" + documentFile,
                "--full-scan-precursor-isotopes=Count",
                "--full-scan-acquisition-method=DIA",
                "--full-scan-precursor-isotopes=None",
                "--full-scan-product-analyzer=centroided",
                "--full-scan-rt-filter=scheduling_windows",
                "--full-scan-rt-filter-tolerance=5",
                "--full-scan-isolation-scheme=" + Path.Combine(GetTestPath("DIA"), _instrumentValues.DiaFiles[0]),
                "--tran-precursor-ion-charges=2,3,4",
                "--tran-product-ion-charges=1,2",
                "--tran-product-start-ion=" + TransitionFilter.StartFragmentFinder.ION_3.Label,
                "--tran-product-end-ion=" + TransitionFilter.EndFragmentFinder.LAST_ION.Label,
                "--tran-product-clear-special-ions",
                "--tran-use-dia-window-exclusion",
                "--library-product-ions=6",
                "--library-min-product-ions=6",
                "--library-match-tolerance=" + 0.05 + "mz",
                "--library-pick-product-ions=filter",
                "--instrument-max-mz=2000",
                "--reintegrate-model-name=" + documentBaseName,
                "--reintegrate-create-model",
                "--reintegrate-model-type=mprophet",
                "--import-search-exclude-library-sources",
                "--import-search-irts=" + _instrumentValues.IrtStandard,
                "--import-fasta=" + GetTestPath(_analysisValues.FastaPath),
                "--decoys-add=shuffle"
            };
            settings = settings.Concat(_instrumentValues.SearchFiles.Select(o => "--import-search-file=" + GetTestPath(o))).ToArray();
            settings = settings.Concat(_instrumentValues.DiaFiles.Select(o => "--import-file=" + Path.Combine(GetTestPath("DIA"), o)).Take(1)).ToArray();

            if (_testInfo.IsPasef)
            {
                settings = settings.Append("--ims-library-res=30").ToArray();
                settings = settings.Append("--import-search-add-mods").ToArray();
            }

            //const string modOxidation = "Oxidation (M)";
            // Define expected matched/unmatched modifications
            //var expectedMatched = !IsPasef ? new[] { modOxidation } : Array.Empty<string>();
            // Verify matched/unmatched modifications
            //AssertEx.AreEqualDeep(expectedMatched, importPeptideSearchDlg.MatchModificationsControl.MatchedModifications.ToArray());
            //Assert.IsFalse(importPeptideSearchDlg.MatchModificationsControl.UnmatchedModifications.Any());

            // Default is to have precursors
            if (_analysisValues.KeepPrecursors)
            {
                settings = settings.Append("--tran-product-ion-types=y,b,p").ToArray();
                settings = settings.Append("--full-scan-precursor-res=20").ToArray();
            }
            else
            {
                settings = settings.Append("--tran-product-ion-types=y,b").ToArray();
                settings = settings.Append("--full-scan-product-res=20").ToArray();
            }

            if (_instrumentValues.IrtStandard.IsAuto ||
                Equals(_instrumentValues.IrtStandard, IrtStandard.CIRT) ||
                Equals(_instrumentValues.IrtStandard, IrtStandard.CIRT_SHORT))
            {
                settings = settings.Append("--import-search-num-cirts=" + _instrumentValues.ExpectedIrtPeptideCount).ToArray();
            }

            // Verify other values shown in the tutorial
            // CONSIDER: Not that easy to validate 1, 2 in ion charges.

            /*string schemePath = Path.Combine("DIA", _instrumentValues.IsolationSchemeFile);
            var schemeLines = File.ReadAllLines(GetTestPath(schemePath));
            string[][] windowFields = schemeLines.Select(l =>
                TextUtil.ParseDsvFields(l, _instrumentValues.IsolationSchemeFileSeparator)).ToArray();
            WaitForConditionUI(() => isolationScheme.GetIsolationWindows().Count == schemeLines.Length);*/

            //Assert.AreEqual("Trypsin [KR | P]", importPeptideSearchDlg.ImportFastaControl.Enzyme.GetKey());
            //Assert.AreEqual(0, importPeptideSearchDlg.ImportFastaControl.MaxMissedCleavages);
            //importPeptideSearchDlg.ImportFastaControl.AutoTrain = true;

            if (_analysisValues.RemoveDuplicates)
                //settings = settings.Append("--refine-remove-duplicates").ToArray();
                settings = settings.Append("--associate-proteins-shared-peptides=Removed").ToArray();
            if (_analysisValues.MinPeptidesPerProtein.HasValue)
                //settings = settings.Append("--refine-min-peptides=" + _analysisValues.MinPeptidesPerProtein.Value).ToArray();
                settings = settings.Append("--associate-proteins-min-peptides=" + _analysisValues.MinPeptidesPerProtein.Value).ToArray();
            else
                settings = settings.Append("--associate-proteins-min-peptides=1").ToArray();

            string output = RunCommand(true, settings);

            try
            {

                var docInit = ResultsUtil.DeserializeDocument(documentFile);
                string docLibFile = BiblioSpecLiteSpec.GetLibraryFileName(documentFile);
                docInit = docInit.ChangeSettings(docInit.Settings.ConnectLibrarySpecs((library, spec) => spec, docLibFile));

                using (var docContainer = new ResultsTestDocumentContainer(null, documentFile))
                {
                    Assert.IsTrue(docContainer.SetDocument(docInit, null, true));
                    var doc = docContainer.Document;
                    var docLibrary = doc.Settings.PeptideSettings.Libraries.Libraries[0];
                    var irtGroup = doc.PeptideGroups.First();
                    Assert.AreEqual(_instrumentValues.ExpectedIrtPeptideCount, irtGroup.PeptideCount);
                    Assert.AreEqual(_instrumentValues.LibraryPeptideCount + _instrumentValues.ExpectedIrtPeptideCount, docLibrary.LibraryDetails.UniquePeptideCount);

                    Assert.AreEqual(50, doc.Settings.TransitionSettings.Instrument.MinMz);
                    Assert.AreEqual(2000, doc.Settings.TransitionSettings.Instrument.MaxMz);
                    Assert.AreEqual(TransitionFilter.StartFragmentFinder.ION_3.Label, doc.Settings.TransitionSettings.Filter.StartFragmentFinderLabel.Label);
                    Assert.AreEqual(TransitionFilter.EndFragmentFinder.LAST_ION.Label, doc.Settings.TransitionSettings.Filter.EndFragmentFinderLabel.Label);
                    Assert.AreEqual(6, doc.Settings.TransitionSettings.Libraries.IonCount);
                    Assert.AreEqual(6, doc.Settings.TransitionSettings.Libraries.MinIonCount);
                    Assert.AreEqual(0.05, doc.Settings.TransitionSettings.Libraries.IonMatchMzTolerance.Value);
                    Assert.AreEqual(MzTolerance.Units.mz, doc.Settings.TransitionSettings.Libraries.IonMatchMzTolerance.Unit);

                    //Assert.AreEqual(0.95, importPeptideSearchDlg.BuildPepSearchLibControl.Grid.Files.First().ScoreThreshold);

                    _testInfo.ValidateTargets(false, _analysisValues.FinalTargetCounts, doc.PeptideGroupCount, doc.PeptideCount,
                        doc.PeptideTransitionGroupCount, doc.PeptideTransitionCount, @"FinalTargetCounts");
                    //ValidateCoefficients(peakScoringModelDlg, _analysisValues.ScoringModelCoefficients);
                    docContainer.AssertComplete();
                }
            }
            catch (Exception)
            {
                Console.Error.WriteLine(output);
                throw;
            }
        }
    }
}
