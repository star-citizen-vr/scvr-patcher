using Humanizer;
using NLog;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Globalization;
using System.IO;
using System.Management;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Windows.Forms;

namespace SCVRPatcher {
    internal class Utils {
        public class AssemblyAttributes {
            public string Title { get; set; }
            public string Version { get; set; }
            public Uri RepositoryUrl { get; set; }

            public AssemblyAttributes(Assembly? assembly = null) {
                assembly = assembly ?? typeof(MainWindow).Assembly;
                var attributes = assembly.GetCustomAttributes(false);
                Title = attributes.OfType<AssemblyTitleAttribute>().FirstOrDefault().Title;
                Version = attributes.OfType<AssemblyFileVersionAttribute>().FirstOrDefault().Version;
                RepositoryUrl = new Uri(attributes.OfType<AssemblyMetadataAttribute>().FirstOrDefault(x => x.Key == "RepositoryUrl").Value);
            }
        }
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

        public class PageFile {
            public FileInfo File { get; set; }
            public string? Caption { get; set; }
            public uint? PeakUsage { get; set; }
            public string Size => File.Length.Bytes().Humanize();
            public PageFile(ManagementBaseObject obj) {
                File = new FileInfo(obj.GetPropertyValue("Name") as string);
                Caption = obj.GetPropertyValue("Caption") as string;
                PeakUsage = (uint)obj.GetPropertyValue("PeakUsage");
            }
            public PageFile(FileInfo file) {
                File = file;
            }
            public override string ToString() {
                return $"{File.Quote()} ({Size})";
            }
        }

        public static List<PageFile> GetPageFileSizes() {
            List<PageFile> dict = new();
            using (var query = new ManagementObjectSearcher("SELECT * FROM Win32_PageFileUsage")) {
                foreach (ManagementBaseObject obj in query.Get()) {
                    dict.Add(new PageFile(obj));
                }
            }
            var allVolumes = DriveInfo.GetDrives();
            foreach (var volume in allVolumes) {
                var pageFile = new PageFile(new FileInfo($"{volume.Name}pagefile.sys"));
                var hasPageFile = dict.Any(x => x.File.FullName == pageFile.File.FullName);
                if (pageFile.File.Exists && !hasPageFile) {
                    Logger.Warn($"PageFile {pageFile} exists but is not enabled, check your windows page file settings!");
                }
            }
            return dict;
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

        public static FileInfo getOwnPath() {
            return new FileInfo(Path.GetDirectoryName(Application.ExecutablePath));
        }
        /*[DllImport("User32.dll")]
        public static extern Int32 SetForegroundWindow(int hWnd);*/
        /*public static void BringSelfToFront()
        {
            var window = Program.mainWindow;
            if (window.WindowState == FormWindowState.Minimized)
                window.WindowState = FormWindowState.Normal;
            else
            {
                window.TopMost = true;
                window.Focus();
                window.BringToFront();
                window.TopMost = false;
            }
            /*Program.mainWindow.Activate();
            Program.mainWindow.Focus();
            // SetForegroundWindow(SafeHandle.ToInt32());
        }*/

        public static bool IsAlreadyRunning(string appName) {
            Mutex m = new Mutex(false, appName);
            if (m.WaitOne(1, false) == false) {
                return true;
            }
            return false;
        }

        internal static void Exit() {
            Application.Exit();
            var currentP = Process.GetCurrentProcess();
            currentP.Kill();
        }

        public static void RestartAsAdmin(string[] arguments) {
            if (IsAdmin()) {
                Logger.Warn("Wanted to restart as admin, but we already are. duh");
                return;
            }
            try {
                var startInfo = new ProcessStartInfo {
                    FileName = Application.ExecutablePath,
                    Arguments = string.Join(" ", arguments),
                    Verb = "runas"
                };
                Process.Start(startInfo);
                Exit();
            } catch (Exception ex) {
                Logger.Error("Unable to restart as admin!", ex.Message);
                MessageBox.Show("Unable to restart as admin for you. Please do this manually now!", "Can't restart as admin", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Exit();
            }
        }

        internal static bool IsAdmin() {
            bool isAdmin;
            try {
                WindowsIdentity user = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(user);
                isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
            } catch (UnauthorizedAccessException) {
                isAdmin = false;
            } catch (Exception) {
                isAdmin = false;
            }
            return isAdmin;
        }

        public static string Base64Encode(string plainText) {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData) {
            var base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static FileInfo DownloadFile(string url, DirectoryInfo destinationPath, string fileName = null) {
            if (fileName == null) fileName = url.Split('/').Last();
            // Main.webClient.DownloadFile(url, Path.Combine(destinationPath.FullName, fileName));
            return new FileInfo(Path.Combine(destinationPath.FullName, fileName));
        }

        public static FileInfo pickFile(string title = null, string initialDirectory = null, string filter = null) {
            using (var fileDialog = new OpenFileDialog()) {
                if (title != null) fileDialog.Title = title;
                fileDialog.InitialDirectory = initialDirectory ?? "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
                if (filter != null) fileDialog.Filter = filter;
                fileDialog.Multiselect = false;
                var result = fileDialog.ShowDialog();
                if (result == DialogResult.OK) {
                    var file = new FileInfo(fileDialog.FileName);
                    if (file.Exists) return file;
                }
                return null;
            }
        }

        public static FileInfo saveFile(string title = null, string initialDirectory = null, string filter = null, string fileName = null, string content = null) {
            using (var fileDialog = new SaveFileDialog()) {
                if (title != null) fileDialog.Title = title;
                fileDialog.InitialDirectory = initialDirectory ?? "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
                if (filter != null) fileDialog.Filter = filter;
                fileDialog.FileName = fileName ?? null;
                var result = fileDialog.ShowDialog();
                if (result != DialogResult.OK || fileDialog.FileName.IsNullOrWhiteSpace()) return null;
                if (content != null) {
                    using (var fileStream = fileDialog.OpenFile()) {
                        byte[] info = new UTF8Encoding(true).GetBytes(content);
                        fileStream.Write(info, 0, info.Length);
                    }
                }
                return new FileInfo(fileDialog.FileName);
            }
        }

        public static DirectoryInfo pickFolder(string title = null, string initialDirectory = null) {
            Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog();
            if (title != null) dialog.Title = title;
            dialog.IsFolderPicker = true;
            dialog.DefaultDirectory = initialDirectory ?? "::{20D04FE0-3AEA-1069-A2D8-08002B30309D}";
            if (dialog.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok) {
                var dir = new DirectoryInfo(dialog.FileName);
                if (dir.Exists) return dir;
            }
            return null;
        }

        public static Process StartProcess(FileInfo file, params string[] args) => StartProcess(file.FullName, file.DirectoryName, args);

        public static Process StartProcess(string file, string workDir = null, params string[] args) {
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.FileName = file;
            proc.Arguments = string.Join(" ", args);
            Logger.Debug("Starting Process: {0} {1}", proc.FileName, proc.Arguments);
            if (workDir != null) {
                proc.WorkingDirectory = workDir;
                Logger.Debug("Working Directory: {0}", proc.WorkingDirectory);
            }
            return Process.Start(proc);
        }

        public static IPEndPoint ParseIPEndPoint(string endPoint) {
            string[] ep = endPoint.Split(':');
            if (ep.Length < 2) return null;
            IPAddress ip;
            if (ep.Length > 2) {
                if (!IPAddress.TryParse(string.Join(":", ep, 0, ep.Length - 1), out ip)) {
                    return null;
                }
            } else {
                if (!IPAddress.TryParse(ep[0], out ip)) {
                    return null;
                }
            }
            int port;
            if (!int.TryParse(ep[ep.Length - 1], NumberStyles.None, NumberFormatInfo.CurrentInfo, out port)) {
                return null;
            }
            return new IPEndPoint(ip, port);
        }



    }
}
