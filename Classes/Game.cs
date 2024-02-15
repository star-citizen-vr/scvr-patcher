using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using NLog;
using System.IO;
using System.Windows;

namespace SCVRPatcher {
    internal class Game {
        public const string Name = "Star Citizen";
        public const string Author = "Cloud Imperium Games";
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly List<string> BuildDirectoryNames = new() { "LIVE", "PTU", "EPTU", "HOTFIX", "TECH-Preview" };

        public DirectoryInfo BuildRootDirectory { get; private set; }
        public Dictionary<string, DirectoryInfo> BuildDirectories { get; private set; } = new();

        public void Initialize() {
            BuildRootDirectory = GetLastUsedGameRootDir() ?? GetGameRootDirFromRegistry() ?? RequestGameRootDirFromUser();
            Logger.Debug($"Got game root directory: {BuildRootDirectory.ToFullString()}");
            if (!BuildRootDirectory.Exists) {
                var msg = $"Game root directory does not exist: {BuildRootDirectory.Quote()}";
                Logger.Error(msg);
                MessageBox.Show(msg, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            if (AppSettings.Default.gameRootDir != BuildRootDirectory.FullName) AppSettings.Default.gameRootDir = BuildRootDirectory.FullName;
            BuildDirectories.Clear();
            foreach (var buildDirName in BuildDirectoryNames) {
                var buildDir = BuildRootDirectory.Combine(buildDirName);
                if (buildDir.Exists) BuildDirectories.Add(buildDirName, buildDir);
            }
        }
        public bool Patch(HmdConfig config, Resolution resolution) {
            foreach (var buildDir in BuildDirectories) {
                Logger.Info($"Got build directory: {buildDir.Key} ({buildDir.Value.ToFullString()})");
                var profileDir = buildDir.Value.Combine("user", "Client", "0", "Profiles", "default");
                var attributesFile = profileDir.CombineFile("attributes.xml");
                if (!attributesFile.Exists) {
                    Logger.Error($"Could not find {attributesFile.Quote()}!");
                    continue;
                }
                var attributes = new AttributesFile(attributesFile);
                if (!attributes.Patch(config, resolution)) {
                    Logger.Error($"Failed to patch {attributesFile.Quote()}!");
                    continue;
                }
            }
            return true;
        }

        public bool Unpatch() {
            foreach (var buildDir in BuildDirectories) {
                Logger.Info($"Got build directory: {buildDir.Key} ({buildDir.Value.ToFullString()})");
                var profileDir = buildDir.Value.Combine("user", "Client", "0", "Profiles", "default");
                var attributesFile = profileDir.CombineFile("attributes.xml");
                if (!attributesFile.Exists) {
                    Logger.Error($"Could not find {attributesFile.Quote()}!");
                    continue;
                }
                var attributes = new AttributesFile(attributesFile);
                if (!attributes.Unpatch()) {
                    Logger.Error($"Failed to unpatch {attributesFile.Quote()}!");
                    continue;
                }
            }
            return true;
        }

        public DirectoryInfo? GetLastUsedGameRootDir() {
            if (string.IsNullOrWhiteSpace(AppSettings.Default.gameRootDir)) return null;
            var dir = new DirectoryInfo(AppSettings.Default.gameRootDir);
            return dir.Exists ? dir : null;
        }

        public DirectoryInfo? GetGameRootDirFromRegistry() {
            var key = Registry.CurrentUser.OpenSubKey(@"System\GameConfigStore\Children");
            if (key is null) {
                Logger.Error($"Unable to open registry key: {key}");
                return null;
            }
            var subKeysNames = key.GetSubKeyNames();
            foreach (var subKeyName in subKeysNames) {
                var subKey = key.OpenSubKey(subKeyName);
                if (subKey is null) continue;
                var matchedExeFullPath = subKey.GetValue("MatchedExeFullPath") as string;
                if (matchedExeFullPath == null) continue;
                var matchedExe = new FileInfo(matchedExeFullPath);
                if (matchedExe.Name == "StarCitizen.exe") {
                    Logger.Debug($"Found Star Citizen as {subKey.Name}: {matchedExe.Quote()}");
                    var dir = matchedExe.Directory?.Parent?.Parent;
                    if (dir is null || !dir.Exists) {
                        Logger.Error($"Star Citizen's root directory does not exist or is invalid: {dir?.Quote()}");
                        continue;
                    }
                    Logger.Debug($"Found Star Citizen's root directory: {dir.ToFullString()}");
                    return dir;
                }
            }
            Logger.Error("Could not find Star Citizen's root directory in the registry, please select it manually!");
            return null;
        }

        public DirectoryInfo RequestGameRootDirFromUser(DirectoryInfo? startingDir = null) {
            startingDir ??= new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            dialog.InitialDirectory = startingDir.FullName;
            var subDirNames = string.Join(", ", BuildDirectoryNames);
            dialog.Title = $"Select Star Citizen's root directory (Containing {subDirNames})";
            var result = dialog.ShowDialog();
            if (result != CommonFileDialogResult.Ok) {
                Logger.Warn("User cancelled directory selection, exiting...");
                MessageBox.Show("You must select a directory!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
            Logger.Debug($"User selected directory: {dialog.FileName}");
            var dir = new DirectoryInfo(dialog.FileName);
            if (!dir.Exists) {
                MessageBox.Show("The selected directory does not exist or is inaccessible, try again!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return RequestGameRootDirFromUser(dir);
            }
            var subDirs = dir.GetDirectories();
            // if none of the BuildDirectoryNames exist in the selected directory, ask again
            if (!BuildDirectoryNames.Any(x => subDirs.Any())) {
                MessageBox.Show($"The selected directory does not contain any of [{subDirNames}], try again!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return RequestGameRootDirFromUser(dir);
            }
            BuildRootDirectory = dir;
            return dir;
        }
    }
}
