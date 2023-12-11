using NLog;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

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
            Logger.Info($"Started {Application.Current.MainWindow.Title}");
            var args = Environment.GetCommandLineArgs();
            Logger.Info($"Command line arguments: {string.Join(" ", args)}");
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
            FillConfigs(configDatabase);
            stackpanel_config.Children.Clear();
            var hf = new HostsFile();
            //var entries = hf.GetEntriesByDomain(HostsFile.EACHostName);
            //foreach (var entry in entries) {
            //    entry.Enabled = false;
            //}
            hf.AddOrEnableByDomain(HostsFile.EACHostName, HostsFile.Localhost);
            hf.Save(new FileInfo("hosts.txt"), true);
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
            Logger.Info($"Loaded {configDatabase.Brands.Count} brands.");
            //foreach (var brand in configDatabase.Brands) {
            //    foreach (var model in brand.Value) {
            //        foreach (var config in model.Value) {
            //            config.Value.Brand = brand.Key;
            //            config.Value.Model = model.Key;
            //            config.Value.Name = config.Key;
            //        }
            //    }
            //}
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


        public void FillConfigs(ConfigDataBase db) {
            tree_configs.Items.Clear();
            foreach (var brand in db.Brands) {
                var brandItem = new TreeViewItem() { Header = brand.Key };
                foreach (var model in brand.Value) {
                    var modelItem = new TreeViewItem() { Header = model.Key };
                    foreach (var config in model.Value) {
                        var configItem = new TreeViewItem() { Header = config.Key };
                        modelItem.Items.Add(configItem);
                    }
                    brandItem.Items.Add(modelItem);
                }
                tree_configs.Items.Add(brandItem);
            }
        }
        private void FillConfig(HmdConfig config) {
            if (config is null) return;
            stackpanel_config.Children.Clear();
            Logger.Info($"Filling config: {config}");
            stackpanel_config.Children.Add(new Label() { Content = string.Join(" ", configDatabase.GetPath(config)) });
            Type t = config.GetType();
            Dictionary<string, object> Items = new();
            foreach (var item in t.GetProperties()) {
                var value = item.GetValue(config);
                if (value is null) continue;
                //if (new string[] { "Brand", "Model", "Name" }.Contains(item.Name)) continue;
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
                // add label and textbox to stackpanel_config
                stackpanel_config.Children.Add(label);
                stackpanel_config.Children.Add(textbox);
            }
        }

        private void VREnableButton_Click(object sender, RoutedEventArgs e) {
            MessageBox.Show("Not implemented yet, silly :)", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void tree_configs_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            var selectedItem = (TreeViewItem)tree_configs.SelectedItem;
            var hasParent = selectedItem.Parent is not null;
            var hasChildren = selectedItem.Items.Count > 0;
            if (!hasParent || hasChildren) return;
            var configName = selectedItem.Header.ToString();
            var hmdName = ((TreeViewItem)selectedItem.Parent).Header.ToString();
            var brandItem = (TreeViewItem)selectedItem.Parent;
            var brandName = (((TreeViewItem)brandItem.Parent).Header).ToString();
            FillConfig(configDatabase.GetConfig(brandName, hmdName, configName));
        }
    }
}