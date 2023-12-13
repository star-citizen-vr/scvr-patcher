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

        public Game() {
            BuildRootDirectory = GetGameRootDirFromRegistry() ?? RequestGameRootDirFromUser();
            Logger.Debug($"Got game root directory: {BuildRootDirectory.FullName}");
            foreach (var buildDir in BuildDirectories) {
                Logger.Info($"Got build directory: {buildDir.Key} ({buildDir.Value.FullName})");
            }
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
                    Logger.Debug($"Found Star Citizen as {subKey.Name}: {matchedExe.FullName}");
                    var dir = matchedExe.Directory?.Parent?.Parent;
                    if (dir is null || !dir.Exists) {
                        Logger.Error($"Star Citizen's root directory does not exist or is invalid: {dir?.FullName}");
                        continue;
                    }
                    Logger.Debug($"Found Star Citizen's root directory: {dir.FullName}");
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
            if (result != CommonFileDialogResult.Ok) Environment.Exit(0);
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
            BuildDirectories.Clear();
            foreach (var buildDirName in BuildDirectoryNames) {
                var buildDir = new DirectoryInfo(Path.Combine(dir.FullName, buildDirName));
                if (buildDir.Exists) BuildDirectories.Add(buildDirName, buildDir);
            }
            return dir;
        }
    }
}
