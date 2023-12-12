using System.IO;
using System.Windows.Forms;

namespace SCVRPatcher {
    internal class Game {
        public static readonly List<string> BuildDirectoryNames = new() { "LIVE", "PTU", "EPTU" };

        public DirectoryInfo BuildRootDirectory { get; private set; }
        public Dictionary<string, DirectoryInfo> BuildDirectories { get; private set; } = new();

        public Game() {
            BuildRootDirectory = RequestGameRootDirFromUser();
        }

        public DirectoryInfo RequestGameRootDirFromUser(DirectoryInfo? startingDir = null) {
            startingDir ??= new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles));
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = startingDir.FullName;
            dialog.Description = "Select the game's root directory";
            dialog.ShowNewFolderButton = false;
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK) Environment.Exit(0);
            var dir = new DirectoryInfo(dialog.SelectedPath);
            if (!dir.Exists) {
                MessageBox.Show("The selected directory does not exist or is inaccessible, try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return RequestGameRootDirFromUser(dir);
            }
            var subDirs = dir.GetDirectories();
            // if none of the BuildDirectoryNames exist in the selected directory, ask again
            if (!BuildDirectoryNames.Any(x => subDirs.Any())) {
                MessageBox.Show($"The selected directory does not contain any of [{BuildDirectoryNames}], try again!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
