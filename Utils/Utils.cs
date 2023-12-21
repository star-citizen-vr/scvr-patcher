using NLog;
using System.IO;

namespace SCVRPatcher {
    internal class Utils {
        internal class Screen {
            public System.Windows.Forms.Screen _Screen { get; set; }
            public Resolution Resolution { get; set; }
            public Screen(System.Windows.Forms.Screen screen) {
                _Screen = screen;
                Resolution = new Resolution() { Width = screen.Bounds.Width, Height = screen.Bounds.Height };
            }
            public override string ToString() {
                return $"{_Screen.DeviceName} ({Resolution.Width} x {Resolution.Height}) {(_Screen.Primary ? "[primary]" : "")}";
            }
        }
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static Resolution GetMainScreenResolution() {
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            return new Resolution() { Width = screen.Bounds.Width, Height = screen.Bounds.Height };
        }
        public static IEnumerable<Screen> GetAllScreens() {
            return System.Windows.Forms.Screen.AllScreens.ToList().Select(s => new Screen(s));
        }
        public static FileInfo GetTempFile() {
            var tries = 0;
            var tempPath = Path.GetTempFileName();
            //while (File.Exists(tempPath)) {
            //    tries++;
            //    Logger.Warn($"Temp file already exists {tempPath}");
            //    File.Delete(tempPath);
            //    tempPath = Path.GetTempFileName();
            //    if (tries > 2) {
            //        throw new Exception($"Failed to get temp file after {tries} tries");
            //    }
            //    Task.Delay(250).Wait();
            //}
            return new FileInfo(tempPath);
        }

        public enum TaskbarLocation {
            Top,
            Bottom,
            Left,
            Right
        }
        public static byte? ChangeTaskbarLocation(TaskbarLocation location) {
            var wantedLocation = (byte)location;
            // get current taskbar location from registry
            var regPath = @"Software\Microsoft\Windows\CurrentVersion\Explorer\StuckRects3";
            var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(regPath, true);
            if (regKey is null) {
                Logger.Error($"Failed to open registry key: {regPath}");
                return null;
            }
            var data = regKey.GetValue("Settings") as byte[];
            var taskbarLocation = data?[12];
            if (taskbarLocation == wantedLocation) {
                Logger.Warn($"Taskbar is already in {location} location, doing nothing!");
                return null;
            }
            Logger.Info($"Old taskbar location: {taskbarLocation}");
            // change taskbar location
            data[12] = wantedLocation;
            regKey.SetValue("Settings", data);
            Logger.Info($"Changed taskbar location to: {location}");
            // restart explorer process
            Logger.Info("Restarting explorer process...");
            var explorer = System.Diagnostics.Process.GetProcessesByName("explorer");
            explorer.ToList().ForEach(x => x.Kill());
            Thread.Sleep(1000);
            System.Diagnostics.Process.Start("explorer.exe");
            Logger.Info("Explorer process restarted");
            return taskbarLocation;
        }
    }
}
