using IniParser;
using IniParser.Model;
using NLog;
using System.IO;

namespace SCVRPatcher {
    public abstract class IniFile {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static FileIniDataParser Parser = new FileIniDataParser();

        public FileInfo File { get; private set; }
        public IniData? _Data { get; set; }

        public IniFile() { }
        public IniFile(FileInfo file) {
            File = file;
            if (!File.Exists) {
                Logger.Warn($"File not found at {File.Quote()}");
                return;
            }
            Load();
        }

        public void Load(FileInfo? file = null) {
            file ??= File;
            File = file;
            Logger.Info($"Loading INI File: {file.Quote()}");
            _Data = Parser.ReadFile(file.FullName);
            Logger.Info($"Loaded INI File");
        }

        public abstract bool Patch(HmdConfig config, Resolution resolution);

        public abstract bool Unpatch();

        public void Save(FileInfo? file = null, bool backup = true) {
            file ??= File;
            Logger.Info($"Saving INI File: {file.Quote()}");
            if (backup) file.Backup();
            Parser.WriteFile(file.FullName, _Data);
            Logger.Info($"Saved INI File");
        }
    }
}
