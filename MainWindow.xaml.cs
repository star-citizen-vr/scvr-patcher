using NLog;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SCVRPatcher {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public static Uri availableConfigsUrl = new Uri("https://raw.githubusercontent.com/Bluscream/scvr-patcher/main/configs.json");
        public static FileInfo availableConfigsFile = new FileInfo("configs.json");
        public static Dictionary<string, HmdConfig> availableConfigs = new();

        public MainWindow() {
            Logger.Debug($"Started {Application.Current.MainWindow.Title}");
            var args = Environment.GetCommandLineArgs();
            Logger.Debug($"Command line arguments: {string.Join(" ", args)}");
            var parser = new Utils.CommandLineParser(args);
            var configsArg = parser.GetStringArgument("config");
            if (configsArg != null) {
                var isUrl = Uri.TryCreate(configsArg, UriKind.Absolute, out var uriResult);
                if (isUrl) availableConfigsUrl = uriResult;
                else if (File.Exists(configsArg)) availableConfigsFile = new FileInfo(configsArg);
                else Logger.Error($"Invalid --config argument: \"{configsArg}\"");
            }
            if (parser.GetSwitchArgument("console", 'c')) {
                AllocConsole();
                Logger.Info("Console ready!");
            }
            InitializeComponent();
            LoadAvailableConfigs(availableConfigsUrl, availableConfigsFile);
            //HMDSelector.DisplayMemberPath = "Name";
            //HMDSelector.ItemsSource = availableConfigs;
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public void LoadAvailableConfigs(Uri availableConfigsUrl, FileInfo availableConfigsFile) {
            availableConfigs = GetAvailableConfigsFromUrl(availableConfigsUrl);
            if (availableConfigs.Count == 0) {
                availableConfigs = GetAvailableConfigsFromFile(availableConfigsFile);
            }
            if (availableConfigs.Count == 0) {
                Logger.Error("No configs available!");
                MessageBox.Show("No configs available!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Application.Current.Shutdown();
            }
            HMDSelector.Items.Clear();
            foreach (var config in availableConfigs) {
                var item = new ListViewItem();
                item.Content = config.Key;
                item.Tag = config;
                HMDSelector.Items.Add(item);
            }
        }
        public static Dictionary<string, HmdConfig> GetAvailableConfigsFromFile(FileInfo availableConfigsFile) {
            if (availableConfigsFile.Exists) {
                Logger.Info($"Loading available configs from {availableConfigsFile.FullName}...");
                return ConfigDataBase.FromJson(File.ReadAllText(availableConfigsFile.FullName));
            } else {
                Logger.Warn($"Configs file not found at {availableConfigsFile.FullName}!");
                return new();
            }
        }
        public static Dictionary<string, HmdConfig> GetAvailableConfigsFromUrl(Uri availableConfigsUrl) {
            try {
                using (var client = new HttpClient()) {
                    Logger.Info($"Downloading available configs from {availableConfigsUrl}...");
                    var response = client.GetAsync(availableConfigsUrl).Result;
                    if (response.IsSuccessStatusCode) {
                        var json = response.Content.ReadAsStringAsync().Result;
                        File.WriteAllText(availableConfigsFile.FullName, json);
                        return ConfigDataBase.FromJson(json);
                    } else {
                        throw new Exception($"Failed to download available configs! (Error {response.StatusCode})");
                    }
                }
            } catch (Exception e) {
                Logger.Error(e);
                if (availableConfigsFile.Exists) {
                    return ConfigDataBase.FromJson(File.ReadAllText(availableConfigsFile.FullName));
                }
            }
            return new();
        }

        private void VREnableButton_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("Not implemented yet, silly :)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}