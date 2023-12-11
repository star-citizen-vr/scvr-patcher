using NLog;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
        public static ConfigDataBase configDatabase = new();

        public MainWindow() {
            Logger.Debug($"Started {Application.Current.MainWindow.Title}");
            var args = Environment.GetCommandLineArgs();
            Logger.Debug($"Command line arguments: {string.Join(" ", args)}");
            var parser = new Utils.CommandLineParser(args);
            var configsArg = parser.GetStringArgument("config");
            if (configsArg != null) {
                if (File.Exists(configsArg)) availableConfigsFile = new FileInfo(configsArg);
                else if (Uri.TryCreate(configsArg, UriKind.Absolute, out var uriResult)) availableConfigsUrl = uriResult;
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
            Logger.Info($"Loading config from Url: {availableConfigsUrl}");
            configDatabase = GetAvailableConfigsFromUrl(availableConfigsUrl);
            if (configDatabase.IsEmptyOrMissing) {
                Logger.Error($"Failed to load config from Url!");
                Logger.Info($"Loading config from File: {availableConfigsFile.FullName}");
                configDatabase = GetAvailableConfigsFromFile(availableConfigsFile);
            }
            if (configDatabase.IsEmptyOrMissing) {
                Logger.Error("No configs available!");
                MessageBox.Show("No configs available!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Application.Current.Shutdown();
                return;
            }

            ClearBoxes(true);
        }
        public void ClearBoxes(bool refill = false) {
            box_manufacturer.Items.Clear();
            box_hmd.Items.Clear();
            box_config.Items.Clear();
            if (refill) {
                foreach (var brand in configDatabase.Brands) {
                    var item = new ComboBoxItem();
                    item.Content = brand.Key;
                    item.Tag = brand.Value;
                    box_manufacturer.Items.Add(item);
                }
            }
        }
        public static ConfigDataBase GetAvailableConfigsFromFile(FileInfo availableConfigsFile) {
            if (availableConfigsFile.Exists) {
                Logger.Info($"Loading available configs from {availableConfigsFile.FullName}...");
                return ConfigDataBase.FromJson(File.ReadAllText(availableConfigsFile.FullName));
            } else {
                Logger.Warn($"Configs file not found at {availableConfigsFile.FullName}!");
                return new();
            }
        }
        public static ConfigDataBase GetAvailableConfigsFromUrl(Uri availableConfigsUrl) {
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

        private void box_manufacturer_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = (ComboBoxItem)box_manufacturer.SelectedItem;
            var name = (string)selected.Content;
            var brand = (Dictionary<string, Dictionary<string, HmdConfig>>)selected.Tag;
            box_hmd.Items.Clear();
            box_config.Items.Clear();
            foreach (var hmd in brand) {
                var item = new ComboBoxItem();
                item.Content = hmd.Key;
                item.Tag = hmd.Value;
                box_hmd.Items.Add(item);
            }

        }

        private void box_hmd_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = (ComboBoxItem)box_hmd.SelectedItem;
            var hmd = (Dictionary<string, HmdConfig>)selected.Tag;
            box_config.Items.Clear();
            foreach (var config in hmd) {
                var item = new ComboBoxItem();
                item.Content = config.Key;
                item.Tag = config.Value;
                box_config.Items.Add(item);
            }
        }

        private void box_config_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var selected = (ComboBoxItem)box_config.SelectedItem;
            var cfg = (HmdConfig)selected.Tag;
            FillConfig(cfg);
        }

        private void FillConfig(HmdConfig config) {
            if (config is null) return;
            Logger.Debug($"Filling config: {config}");
            Type t = config.GetType();
            Dictionary<string, object> Items = new();
            foreach (var item in t.GetProperties()) {
                var value = item.GetValue(config);
                if (value is null) continue;
                Items.Add(item.Name, value);
            }
            foreach (var item in t.GetFields()) {
                var value = item.GetValue(config);
                if (value is null) continue;
                Items.Add(item.Name, value);
            }
            foreach (var item in Items) {
                var value = item.Value;
                if (value is null) continue;
                if (value is System.Collections.IList) continue;
                var label = new Label();
                label.Content = item.Key + ":";
                label.VerticalAlignment = VerticalAlignment.Center;
                label.HorizontalAlignment = HorizontalAlignment.Right;
                label.Margin = new Thickness(0, 0, 5, 0);
                var textbox = new TextBox();
                textbox.Text = value.ToString();
                textbox.VerticalAlignment = VerticalAlignment.Center;
                textbox.HorizontalAlignment = HorizontalAlignment.Stretch;
                textbox.Margin = new Thickness(0, 0, 5, 0);
                //textbox.TextChanged += Textbox_TextChanged;
                grd_config.RowDefinitions.Add(new RowDefinition());
                Grid.SetRow(label, grd_config.RowDefinitions.Count - 1);
                Grid.SetColumn(label, 1);
                Grid.SetRow(textbox, grd_config.RowDefinitions.Count - 1);
                Grid.SetColumn(textbox, 2);
                grd_config.Children.Add(label);
                grd_config.Children.Add(textbox);
            }
        }

        private void Textbox_TextChanged(object sender, TextChangedEventArgs e) {
            throw new NotImplementedException();
        }
    }
}