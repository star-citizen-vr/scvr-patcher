using IniParser.Model;
using System.IO;

namespace SCVRPatcher {
    public partial class VorpX {
        public class VorpControlConfig : IniFile {
            public static readonly FileInfo File = VorpX.VorpXConfigDir.CombineFile("vorpControl.ini");
            public static readonly List<string> itemsToExclude = new() { "RSI Launcher.exe", "StarCitizen_Launcher.exe", "EasyAntiCheat_EOS_Setup.exe" };

            public List<string> Excludes => _Data?["Exclude"].ToDictionary().Values.Select(c => c.ToString()).ToList();

            public KeyData GetExcludesAsIniData(List<string> excludes) {
                var dict = new KeyData("Exclude");
                var cnt = 0;
                foreach (var item in excludes) {
                    dict.Value = new ($"sExcl{cnt}", item);
                }
                return dict;
            }

            public override bool Patch(HmdConfig config, Resolution resolution) {
                Logger.Info($"Patching {File.FullName}");
                var currentExcludes = Excludes;
                foreach (var item in itemsToExclude) {
                    if (!currentExcludes.Contains(item)) {
                        currentExcludes.Add(item);
                        Logger.Debug($"Excluded {item}");
                    }
                }
                _Data["Exclude"].SetKeyData(GetExcludesAsIniData(currentExcludes));

                Logger.Info($"Patched {File.FullName}");
                return true;
            }

            public override bool Unpatch() {
                Logger.Info($"Unpatching {File.FullName}");
                Logger.Info($"Unpatched {File.FullName}");
                return true;
            }

            #region definitions
            public class IniFileData {
                public General? General { get; set; }
                public Injection? Injection { get; set; }
                public Display? Display { get; set; }
                public VirtualDisplay? VirtualDisplay { get; set; }
                public List<string> Exclude { get; set; } = new();
            }
            public class General {
                public bool bRunAsAdmin { get; set; }
                public bool bProfileUpdates { get; set; }
                public bool bLegacyUpdater { get; set; }
            }
            public class Injection {
                public int iInjectMode { get; set; }
                public int iStartKillTimeout { get; set; }
                public bool bKillSameName { get; set; }
                public bool bForceAlternativeInjection { get; set; }
                public bool bMinimizeRuntimeWindowsOnGameExit { get; set; }
            }
            public class Display {
                public bool bRestoreWindows { get; set; }
            }
            public class VirtualDisplay {
                public bool bManualAttach { get; set; }
                public bool bNoDisplayAttach { get; set; }
                public bool bHeadsetActivityAttach { get; set; }
                public bool bResolutionsAbove4K { get; set; }
                public string sCustomResolutions { get; set; }
            }
            #endregion
        }
    }

}
