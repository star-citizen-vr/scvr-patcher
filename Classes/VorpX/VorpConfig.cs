using System.IO;

namespace SCVRPatcher {
    public partial class VorpX {
        public class VorpConfig {
            public FileInfo File = VorpX.VorpXConfigDir.CombineFile("vorpConfig.ini");

            #region definitions
            public class IniFileData {
                public DialogFoldersCategory DialogFolders { get; set; }
                public GeneralCategory General { get; set; }

                public class DialogFoldersCategory {
                    public string ProfileAddExe { get; set; }
                    public string ProfileImportExport { get; set; }
                }
                public class GeneralCategory {
                    public int iMainWinLeft { get; set; }
                    public int iMainWinTop { get; set; }
                    public bool bDisableStartpage { get; set; }
                }
            }
            #endregion definitions
        }
    }
}
