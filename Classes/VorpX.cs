using IniParser;
using IniParser.Model;
using System.IO;

namespace SCVRPatcher {
    public partial class VorpX {
        internal static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        internal static readonly DirectoryInfo VorpXConfigDir = new DirectoryInfo(@"C:\ProgramData\Animation Labs\vorpX");
        internal static readonly FileInfo VorpControlFile = VorpXConfigDir.CombineFile(".ini");

        public VorpControlConfig vorpControlConfig { get; set; }
        public VorpConfig vorpConfig { get; set; }
        public VorpXConfig vorpXConfig { get; set; }

        public VorpX() {
            Logger.Info("Initializing VorpX");
            Logger.Debug($"VorpXConfigDir: {VorpXConfigDir.Quote()}");
            Load();
            Logger.Info("Initialized VorpX");
        }

        public void Load() {
            Logger.Info("Loading VorpX");
            vorpControlConfig = new();
            vorpConfig = new();
            vorpXConfig = new();
            Logger.Info("Loaded VorpX");
        }

        public void Patch() {
            Logger.Info("Patching VorpX");
            Logger.Info("Patched VorpX");
        }

        public void UnPatch() {
            Logger.Info("Unpatching VorpX");
            Logger.Info("Unpatched VorpX");
        }
    }
}
