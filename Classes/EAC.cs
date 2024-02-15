using NLog;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using MessageBox = System.Windows.Forms.MessageBox;

namespace SCVRPatcher {
    internal class EAC {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static readonly DirectoryInfo EACAppdataDir = new DirectoryInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "EasyAntiCheat"));

        public EAC() {
            Logger.Info("Loading EAC");
            //Logger.Debug($"EACAppdataDir: {EACAppdataDir.ToFullString()}");
            Logger.Info("Loaded EAC");
        }

        public void Patch() {
            Logger.Info("Patching EAC");
            DeleteEACAppdataDir();
            PatchHostsFile();
            Logger.Info("Patched EAC");
        }

        public void UnPatch() {
            Logger.Info("Unpatching EAC");
            UnPatchHostsFile();
            Logger.Info("Unpatched EAC");
        }

        public static void DeleteEACAppdataDir() {
            Logger.Info($"Deleting EAC Appdata Directory: {EACAppdataDir.Quote()}");
            if (EACAppdataDir.Exists) {
                try {
                    EACAppdataDir.Delete(true);
                } catch (Exception ex) {
                    Logger.Error(ex, "Error deleting EAC Appdata Directory");
                }
            }
            Logger.Info("Deleted EAC Appdata Directory");
        }

        public static void PatchHostsFile(bool backup = true) {
            if (!Utils.IsAdmin()) {
                var hostsLine = $"{HostsFile.Localhost} {AppSettings.Default.EACHostName} # {AppSettings.Default.EACComment}";
                var result = MessageBox.Show($"Since you did not start this program as Administrator, you will now have to open\n\n{HostsFile.HostFile.Quote()}\n\nand add the following line:\n\n{hostsLine}\n\nWhen you click on OK it will be copied to your clipboard and the hosts file will be opened in explorer.", "Manual edit required", MessageBoxButtons.OKCancel, MessageBoxIcon.Error);
                if (result == DialogResult.OK) {
                    System.Windows.Forms.Clipboard.SetText(hostsLine);
                    HostsFile.HostFile.ShowInExplorer();
                }
                return;
            }
            Logger.Info($"Patching Hosts File at {HostsFile.HostFile.Quote()}");
            try {
                var hf = new HostsFile();
                hf.AddOrEnableDomain(AppSettings.Default.EACHostName, HostsFile.Localhost, AppSettings.Default.EACComment);
                hf.Save(backup: backup);
            } catch (Exception ex) {
                Logger.Error(ex, "Error patching Hosts File");
            }
            Logger.Info("Patched Hosts File");
        }

        public static void UnPatchHostsFile(bool backup = true) {
            Logger.Info($"Unpatching Hosts File at {HostsFile.HostFile.Quote()}");
            try {
                var hf = new HostsFile();
                hf.DisableDomain(AppSettings.Default.EACHostName, remove: true);
                hf.Save(backup: backup);
            } catch (Exception ex) {
                Logger.Error(ex, "Error unpatching Hosts File");
            }
            Logger.Info("Unpatched Hosts File");
        }
    }
}
