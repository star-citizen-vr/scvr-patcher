using System.IO;
using System.Net;
using System.Net.Http;
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

        public static readonly Uri availableConfigsUrl = new Uri("https://raw.githubusercontent.com/Bluscream/scvr-patcher/main/configs.json");
        public static readonly FileInfo availableConfigsFile = new FileInfo("configs.json");
        public static Dictionary<string, HmdConfig> availableConfigs = new();

        public MainWindow() {
            LoadAvailableConfigs(availableConfigsUrl);
            InitializeComponent();
            //HMDSelector.ItemsSource = availableConfigs;
        }

        public static void LoadAvailableConfigs(Uri availableConfigsUrl) {
            availableConfigs = GetAvailableConfigs(availableConfigsUrl);
        }

        public static Dictionary<string, HmdConfig> GetAvailableConfigs(Uri availableConfigsUrl) {
            try {
                using (var client = new HttpClient()) {
                    Logger.Info($"Downloading available configs from {availableConfigsUrl}...");
                    var response = client.GetAsync(availableConfigsUrl).Result;
                    if (response.IsSuccessStatusCode) {
                        var json = response.Content.ReadAsStringAsync().Result;
                        File.WriteAllText(availableConfigsFile.FullName, json);
                        return HmdConfigs.FromJson(json);
                    } else {
                        throw new Exception($"Failed to download available configs! (Error {response.StatusCode})");
                    }
                }
            } catch (Exception e) {
                Logger.Error(e);
                if (availableConfigsFile.Exists) {
                    return HmdConfigs.FromJson(File.ReadAllText(availableConfigsFile.FullName));
                }
            }
            return new();
        }
    }
}