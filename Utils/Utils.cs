using NLog;
using System.IO;

namespace SCVRPatcher {
    internal class Utils {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static Resolution GetMainScreenResolution() {
            var screen = System.Windows.Forms.Screen.PrimaryScreen;
            return new Resolution() { Width = screen.Bounds.Width, Height = screen.Bounds.Height };
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
    }
}
