using System.IO;

namespace SCVRPatcher {

    public partial class VorpX {

        public class VorpXConfig {
            public FileInfo File = VorpX.VorpXConfigDir.CombineFile("vorpX.ini");
            public IniFileData Data { get; set; }

            public VorpXConfig() {
                Logger.Info("Loading VorpXConfig");
                Logger.Debug($"VorpXConfig.File: {File.Quote()}");
                if (!File.Exists) {
                    Logger.Warn($"VorpXConfig File not found at {File.Quote()}");
                    return;
                }
                Load();
            }

            public void Load(FileInfo? file = null) {
                file ??= File;
                Logger.Info($"Loading VorpXConfig File: {file.Quote()}");
                var parser = new IniParser.FileIniDataParser();
                var data = parser.ReadFile(file.FullName);
                //Data = ParseIniFile(data);
                Logger.Info($"Loaded VorpXConfig File");
            }

            public class IniFileData {
                public GeneralCategory? General { get; set; }
                public DisplayCategory? Display { get; set; }
                public GUICategory? GUI { get; set; }
                public AudioCategory? Audio { get; set; }
                public InputCategory? Input { get; set; }
                public AuthoringCategory? Authoring { get; set; }
                public HookingCategory? Hooking { get; set; }
                public MiscCategory? Misc { get; set; }
                public KeyMappingsCategory? KeyMappings { get; set; }
                public ImageCategory? Image { get; set; }
                public DevCategory? Dev { get; set; }

                public class GeneralCategory {
                    public string sDeviceIniName { get; set; }
                }

                public class DisplayCategory {
                    public int iAdapterIndex { get; set; }
                    public bool bDisableDWM { get; set; }
                }

                public class GUICategory {
                    public bool bShowAdvancedParameters { get; set; }
                    public bool bShowStartMessage { get; set; }
                    public bool bShowPosTrackingNotification { get; set; }
                    public bool bHideUnimportantNotifications { get; set; }
                    public int iGuiTexWidth { get; set; }
                    public int iGuiTexHeight { get; set; }
                    public bool bGuiBindToCamera { get; set; }
                    public bool bShowWatermark { get; set; }
                }

                public class AudioCategory {
                    public int iAudioMode { get; set; }
                }

                public class InputCategory {
                    public float fHtRecenterDistance { get; set; }
                    public int iMouseWheelShift { get; set; }
                    public int iMouseWheelAlt { get; set; }
                    public string sOpenTrackPath { get; set; }
                }

                public class AuthoringCategory {
                    public string sAuthorCode { get; set; }
                    public bool bShaderAuthoring { get; set; }
                    public int iAllShadersTimeRangeMs { get; set; }
                    public string sNppPath { get; set; }
                    public string sAuthorDataPath { get; set; }
                    public bool bDumpShaders { get; set; }
                    public bool bDumpShaderBinary { get; set; }
                    public bool bCreateAdditionalHashes { get; set; }
                }

                public class HookingCategory {
                    public bool bOverlayBlock { get; set; }
                    public string sOverlays32 { get; set; }
                    public string sOverlays64 { get; set; }
                    public int iHookStyleDX9 { get; set; }
                    public int iHookStyleDX12 { get; set; }
                    public int iHookThreadMode { get; set; }
                }

                public class MiscCategory {
                    public bool bTsDisableDX9GrabberScan { get; set; }
                    public bool bGhDisallowSettingsChanges { get; set; }
                    public string sSrvMonitorNamesOculus { get; set; }
                }

                public class KeyMappingsCategory {
                    public int iKeyMenu { get; set; }
                    public int iKeyEdgePeek { get; set; }
                    public int iKeyVRHotkeys { get; set; }
                    public int iKeyReset { get; set; }
                    public int iKeyCursor { get; set; }
                    public int iKeyStereoDisable { get; set; }
                    public int iKeyFovAdjust { get; set; }
                    public int iKeyCenterPosTracking { get; set; }
                    public int iKeyCenterGamepad { get; set; }
                    public int iKeyInfoOverlay { get; set; }
                    public int iKeyMagnifier { get; set; }
                    public int iKeyG3DZ3DSwitch { get; set; }
                    public int iKeyDvrScan { get; set; }
                    public int iKeyDvrEnable { get; set; }
                    public int iKeyPitchUnlock { get; set; }
                }

                public class ImageCategory {
                    public int iEdgePeekMode { get; set; }
                }

                public class DevCategory {
                    public bool bForceWindowed { get; set; }
                }
            }
        }
    }
}