using AutoUpdaterDotNET;
using NLog;
using NLog.Config;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Application = System.Windows.Application;
using Label = System.Windows.Controls.Label;
using ListViewItem = System.Windows.Controls.ListViewItem;
using MessageBox = System.Windows.MessageBox;
using TextBox = System.Windows.Controls.TextBox;

namespace SCVRPatcher {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        internal static Utils.AssemblyAttributes AssemblyAttributes { get; } = new();
        internal static Uri availableConfigsUrl = new(AppSettings.Default.availableConfigsUrl);
        internal static FileInfo availableConfigsFile = new(AppSettings.Default.availableConfigsFile);
        internal static Resolution mainScreenResolution = Utils.GetMainScreenResolution();
        internal static CommandLineParser? CommandLineParser { get; private set; }

        internal static ConfigDataBase? configDatabase { get; private set; }
        internal static EAC? eac { get; private set; }
        internal static Game? game { get; private set; }
        internal static VorpX? vorpx { get; private set; }
        internal static Hmdq? hmdq { get; private set; }

        public static void SetupLogging() {
            Stream? stream = typeof(MainWindow).Assembly.GetManifestResourceStream("SCVRPatcher.NLog.config");
            string xml;
            using (StreamReader reader = new(stream)) {
                xml = reader.ReadToEnd();
            }
            LogManager.Configuration = XmlLoggingConfiguration.CreateFromXmlString(xml);
            //Logger = LogManager.GetCurrentClassLogger();
        }

        public MainWindow() {
            System.Reflection.Assembly assembly = typeof(MainWindow).Assembly;
            object[] attributes = assembly.GetCustomAttributes(false);
            System.Reflection.AssemblyTitleAttribute? title = attributes.OfType<System.Reflection.AssemblyTitleAttribute>().FirstOrDefault();
            System.Reflection.AssemblyFileVersionAttribute? version = attributes.OfType<System.Reflection.AssemblyFileVersionAttribute>().FirstOrDefault();
            System.Reflection.AssemblyMetadataAttribute? repositoryUrl = attributes.OfType<System.Reflection.AssemblyMetadataAttribute>().FirstOrDefault(x => x.Key == "RepositoryUrl");

            SetupLogging();
            Logger.Info($"Started {Application.Current.MainWindow.Title}");     // TODO: If a user doesn't have EAC (because they removed it for some reason), make sure to not hang here...
            string[] args = Environment.GetCommandLineArgs();
            Logger.Info($"Command line arguments: {string.Join(" ", args)}");
            string processName = Process.GetCurrentProcess().ProcessName;
            bool alreadyRunning = Utils.IsAlreadyRunning(processName);
            if (alreadyRunning) {
                Logger.Warn($"Already running as {Process.GetProcessesByName(processName).Select(p => p.Id).ToJson()}");
            }

            CommandLineParser = new CommandLineParser(args);
            string? configsArg = CommandLineParser.GetStringArgument("config");
            if (configsArg != null) {
                Logger.Info($"--config argument: {configsArg}");
                if (File.Exists(configsArg)) {
                    Logger.Info($"--config argument is a file: {configsArg}");
                    availableConfigsFile = new FileInfo(configsArg);
                } else if (Uri.TryCreate(configsArg, UriKind.Absolute, out Uri? uriResult)) {
                    Logger.Info($"--config argument is a url: {configsArg}");
                    availableConfigsUrl = uriResult;
                } else {
                    Logger.Error($"Invalid --config argument: \"{configsArg}\"");
                }
            }
            if (CommandLineParser.GetSwitchArgument("console", 'c')) {
                _ = AllocConsole();
                Logger.Info("Console ready!");
            }
            AutoUpdater.RunUpdateAsAdmin = false;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e) {
            double oldHeight = Height; Height = 0; double oldWidth = Width; Width = 0; // This is a dumb way to work around the fact that MessageBoxes close on their own in the constructor!
            if (!CommandLineParser.GetSwitchArgument("no-update")) {
                AutoUpdater.Start("https://raw.githubusercontent.com/star-citizen-vr/scvr-patcher/csharp/release.xml");
            }
            if (!CommandLineParser.GetSwitchArgument("no-admin") && !Utils.IsAdmin()) {
                Logger.Info("Missing elevation and --no-admin not set, restarting as admin!");
                MessageBoxResult result = MessageBox.Show("SCVR-Patcher needs to run as admin to be able to patch the hosts file for the EAC bypass", "Restart as admin?", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes) {
                    Utils.RestartAsAdmin();
                } else {
                    _ = MessageBox.Show("Admin permissions refused, you will have to patch your hosts file manually later.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            List<Utils.PageFile> pageFiles = Utils.GetPageFileSizes();
            foreach (Utils.PageFile pageFile in pageFiles) {
                Logger.Info($"PageFile: {pageFile}");
            }

            eac = new();
            game = new();
            vorpx = new();
            hmdq = new();
            vorpx.Load();
            configDatabase = new();
            LoadAvailableConfigs(availableConfigsUrl, availableConfigsFile);
            hmdq.Initialize();
            hmdq.RunHmdq();
            if (hmdq.IsEmpty) {
                Logger.Error("Failed to get HMD info through HMDQ!");
                bool isSteamVRRunning = Process.GetProcessesByName("vrmonitor").Length > 0;
                bool isOculusRunning = Process.GetProcessesByName("OVRServer_x64").Length > 0;
                if (!isSteamVRRunning && !isOculusRunning) {
                    Logger.Error("SteamVR or Oculus not running!");
                    //var _ = MessageBox.Show("SteamVR or Oculus not running!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    // present user with option to start SteamVR or continue without starting either
                    MessageBoxResult result = System.Windows.MessageBox.Show("Oculus not running! Start Oculus?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                    if (result == MessageBoxResult.Yes) {
                        // start Oculus with oculus:// uri protocol
                        Logger.Info("Starting Oculus!");
                        Uri oculusUri = new("oculus://");
                        oculusUri.OpenInDefaultBrowser();
                        // wait for Oculus process to start and then continue
                        Process[] oculusProcess = Process.GetProcessesByName("OVRServer_x64");
                        while (oculusProcess.Length == 0) {
                            System.Threading.Thread.Sleep(1000);
                            oculusProcess = Process.GetProcessesByName("OVRServer_x64");
                        }
                        hmdq.RunHmdq();
                    } else {
                        result = MessageBox.Show("SteamVR not running! Start SteamVR?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                        if (result == MessageBoxResult.Yes) {
                            // start SteamVR with steam://run/250820
                            Logger.Info("Starting SteamVR!");
                            Uri steamUri = new("steam://run/250820");
                            steamUri.OpenInDefaultBrowser();
                            // wait for SteamVR process to start and then continue
                            Process[] steamProcess = Process.GetProcessesByName("vrmonitor");
                            while (steamProcess.Length == 0) {
                                System.Threading.Thread.Sleep(1000);
                                steamProcess = Process.GetProcessesByName("vrmonitor");
                            }
                            hmdq.RunHmdq();
                        }
                    }
                } else {
                    Logger.Info("SteamVR or Oculus running!");
                }
            }
            if (!hmdq.IsEmpty) {
                Logger.Info($"Manufacturer: {hmdq.Manufacturer} Model: {hmdq.Model} {hmdq.Width}x{hmdq.Height} (fov: {hmdq.VerticalFov})");
                HmdConfig detectedHmdConfig = new() {
                    Fov = hmdq.VerticalFov,
                    CustomResolutions = [
                        new Resolution() {
                            Height = hmdq.Height,
                            Width = hmdq.Width,
                            Description = "Pulled from HMDQ",
                            //Percentage = "100"
                        }
                    ]
                };
                Dictionary<string, HmdConfig> detectedHmd = new() { { hmdq.Model, detectedHmdConfig } };
                Dictionary<string, Dictionary<string, HmdConfig>> detectedBrand = new() { { hmdq.Manufacturer, detectedHmd } };
                configDatabase.Brands.Add("Detected", detectedBrand);

                Logger.Info($"Added HMDQ info to configDatabase");
            }
            game.Initialize();
            InitializeComponent();
            FillHmds(configDatabase);

            stackpanel_config.Children.Clear();
            //VREnableButton.IsEnabled = true;
            // VRDisableButton.IsEnabled = true;

            // This checkbox thing... breaking my brain...

            //if (ChangeResolutionCheckbox.IsChecked == true) {
            //    Logger.Info("ChangeResolutionCheckbox is checked");
            //    // set a variable that a user wants to change the resolution to match their hmd
            //    var useHMDResolution = true;
            //    } else
            //{
            //       Logger.Info("ChangeResolutionCheckbox is not checked");
            //    var useHMDResolution = false;
            //}
            Height = oldHeight; Width = oldWidth; // This is a dumb way to work around the fact that MessageBoxes close on their own in the constructor!
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool AllocConsole();

        public void LoadAvailableConfigs(Uri availableConfigsUrl, FileInfo availableConfigsFile) {
            //Logger.Info($"Loading config from Url: {availableConfigsUrl}");
            ConfigDataBase? onlineConfigs = ConfigDataBase.FromUrl(availableConfigsUrl);
            Logger.Debug($"onlineConfigs: null={onlineConfigs == null} IsEmptyOrMissing={onlineConfigs?.IsEmptyOrMissing} ({availableConfigsUrl})");
            ConfigDataBase? offlineConfigs = ConfigDataBase.FromFile(availableConfigsFile);
            Logger.Debug($"offlineConfigs: null={offlineConfigs == null} IsEmptyOrMissing={offlineConfigs?.IsEmptyOrMissing} ({availableConfigsFile})");
            if (onlineConfigs != null) {
                Logger.Debug($"onlineConfigs available");
                if (onlineConfigs != offlineConfigs && onlineConfigs?.Md5 != offlineConfigs?.Md5) {
                    Logger.Info("onlineConfigs and offlineConfigs differ!");
                    MessageBoxResult result = MessageBox.Show("Online and offline configs differ, overwrite?", "New configs available", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (result == MessageBoxResult.Yes) {
                        onlineConfigs?.ToFile(availableConfigsFile);
                        Logger.Info($"Saved config from {availableConfigsUrl} to {availableConfigsFile.Quote()}");
                    } else { Logger.Info("User chose not to overwrite offline configs."); }
                }
                configDatabase = onlineConfigs;
            }
            if (offlineConfigs != null) {
                Logger.Info($"Loaded config from {availableConfigsFile.Quote()}");
                configDatabase = offlineConfigs;
            }

            //if (configDatabase.IsEmptyOrMissing) {
            //    Logger.Error($"Failed to load config from Url!");
            //    Logger.Info($"Loading config from File: {availableConfigsFile.Quote()}");
            //    configDatabase = GetAvailableConfigsFromFile(availableConfigsFile);
            //}
            if (configDatabase.IsEmptyOrMissing) {
                Logger.Error("No configs available!");
                _ = System.Windows.MessageBox.Show("No configs available!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                //Application.Current.Shutdown();
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

        public void FillHmds(ConfigDataBase db) {
            tree_hmds.Items.Clear();
            if (db.IsEmptyOrMissing) {
                Logger.Error("Still no configs available!"); return;
            }
            foreach (KeyValuePair<string, Dictionary<string, Dictionary<string, HmdConfig>>> brand in db.Brands) {
                TreeViewItem brandItem = new() { Header = brand.Key };
                if (brand.Key == "Detected") {
                    SolidColorBrush redBrush = new(Colors.DarkGreen);
                    brandItem.Foreground = redBrush;
                }
                foreach (KeyValuePair<string, Dictionary<string, HmdConfig>> model in brand.Value) {
                    TreeViewItem modelItem = new() { Header = model.Key };
                    foreach (KeyValuePair<string, HmdConfig> config in model.Value) {
                        TreeViewItem configItem = new() { Header = config.Key };
                        _ = modelItem.Items.Add(configItem);
                    }
                    _ = brandItem.Items.Add(modelItem);
                }
                _ = tree_hmds.Items.Add(brandItem);
            }
        }

        public void FillConfigList(ConfigDataBase db, HmdConfig config) {
            list_configs.Items.Clear();
            SolidColorBrush redBrush = new(Colors.Red);
            foreach (Resolution res in config.CustomResolutions) {
                ListViewItem item = new() { Content = res.ToString(), Tag = res };
                if (res > mainScreenResolution) {
                    item.Foreground = redBrush;
                    item.ToolTip = $"Resolution is higher than your main screen's resolution! ({mainScreenResolution.Width} x {mainScreenResolution.Height})";
                }
                _ = list_configs.Items.Add(item);
            }
            // sort list_configs
            list_configs.Items.SortDescriptions.Add(new System.ComponentModel.SortDescription("Tag.Width", System.ComponentModel.ListSortDirection.Ascending));
        }

        private void FillConfig(HmdConfig config) {
            if (config is null) {
                return;
            }

            stackpanel_config.Children.Clear();
            Logger.Info($"Filling config: {config}");
            _ = stackpanel_config.Children.Add(new Label() { Content = string.Join(" ", configDatabase.GetPath(config)) });
            Type t = config.GetType();
            Dictionary<string, object> Items = [];
            foreach (System.Reflection.PropertyInfo item in t.GetProperties()) {
                object? value = item.GetValue(config);
                if (value is null) {
                    continue;
                }
                //if (new string[] { "Brand", "Model", "Name" }.Contains(item.Name)) continue;
                Items.Add(item.Name, value);
            }
            foreach (System.Reflection.FieldInfo item in t.GetFields()) {
                object? value = item.GetValue(config);
                if (value is null) {
                    continue;
                }

                Items.Add(item.Name, value);
            }
            foreach (KeyValuePair<string, object> item in Items) {
                object? value = item.Value;
                if (value is null) {
                    continue;
                }

                if (value is System.Collections.IList) {
                    continue;
                }

                Label label = new() {
                    Content = item.Key + ":",
                    VerticalAlignment = VerticalAlignment.Center,
                    //label.HorizontalAlignment = HorizontalAlignment.Right;
                    Margin = new Thickness(0, 0, 5, 0)
                };
                TextBox textbox = new() {
                    Text = value.ToString(),
                    VerticalAlignment = VerticalAlignment.Center,
                    //textbox.HorizontalAlignment = HorizontalAlignment.Stretch;
                    Margin = new Thickness(0, 0, 5, 0)
                };
                // add label and textbox to stackpanel_config
                _ = stackpanel_config.Children.Add(label);
                _ = stackpanel_config.Children.Add(textbox);
            }
        }

        private void VREnableButton_Click(object sender, RoutedEventArgs e) {
            //VREnableButton.IsEnabled = false;
            //VRDisableButton.IsEnabled = true;
            HmdConfig? selectedConfig = GetSelectedConfig();
            if (selectedConfig is null) {
                _ = MessageBox.Show("No HMD selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Resolution? selectedResolution = GetSelectedResolution();
            if (selectedResolution is null) {
                _ = MessageBox.Show("No config selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Logger.Info("Patching VR");
            eac.Patch();
            vorpx.Patch(selectedConfig, selectedResolution);
            // Logger.Info(vorpx.ToJson(true));
            // foreach (var excludedItem in vorpx.vorpControlConfig.Data.Exclude) {
            //     Logger.Info($"Excluding {excludedItem.Quote()} from VorpX");
            // }
            _ = game.Patch(selectedConfig, selectedResolution, ChangeResolutionCheckbox.IsChecked ?? false);
            Logger.Info("Patched VR");
            _ = MessageBox.Show("Success, patched Attriubtes for VR. \n Open the RSI Launcher and Launch the game!", "VR Enabled", MessageBoxButton.OK, MessageBoxImage.Information);
            _ = MessageBox.Show("Any further changes made to your attributes.xml file will be lost when disabling VR. \n\nIf you would like to keep settings for future use, after making all changes to your settings, close the game and copy the attributes.xml before hitting the 'Disable VR' button.", "WARNING");
            /*Logger.Info("Opening RSI Launcher");
            MessageBox.Show("Opening RSI Launcher", "VR Enabled", MessageBoxButton.OK, MessageBoxImage.Information);*/
            //WindowState = WindowState.Minimized;

            // TODO: See if we can launch the RSI Launcher and then check if starcitizen.exe is running, and if it is, then minimize this window
            //var rsiLauncherPath = Utils.GetRsiLauncherPath();
            //if (rsiLauncherPath is null)
            //{
            //    Logger.Error("Failed to get RSI Launcher path!");
            //    MessageBox.Show("Failed to get RSI Launcher path!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            //else Process.Start("C:\\Program Files\\Roberts Space Industries\\RSI Launcher\\RSI Launcher.exe");


            //openRSI.Initialized += (s, e) =>
            //{
            //    Logger.Info("RSI Launcher initialized");
            //    openRSI.Start();
            //};

            // TODO: See if we can start vorpx after RSI Launcher is started...

            //the RSI launcher will open, users will launnch the game, and the 'starcitizen.exe' application will start running. I want to check if the 'starcitizen.exe' is running, and if it is, then I want to loop to check if starcitizen.exe stops running. If it stops running, then I want to unminimize this window
            // TODO: If a user closes the RSI Launcher before launching star citizen, we won't bring up the window, currently... Perhaps look at this later.
            if (Process.GetProcessesByName("starcitizen").Length > 0) {
                Logger.Info("Star Citizen is running");
                while (Process.GetProcessesByName("starcitizen").Length > 0) {
                    System.Threading.Thread.Sleep(1000);
                }
                Logger.Info("Star Citizen stopped running");
                WindowState = WindowState.Normal;
                Activated += (s, e) => Topmost = true;
                // TODO: Can't get this to come back as active window.. maybe we can do something else...
            }
        }

        private void VRDisableButton_Click(object sender, RoutedEventArgs e) {
            //VRDisableButton.IsEnabled = false;
            //VREnableButton.IsEnabled = true;
            eac.UnPatch();
            Logger.Info("UnPatched EAC");
            vorpx.UnPatch();
            Logger.Info("UnPatched VorpX");
            _ = game.Unpatch();
            Logger.Info("UnPatched Game");
            _ = MessageBox.Show("Success, rolled back Attriubtes. \nAny changes made to the attributes.xml file during VR gameplay was lost.", "VR Disabled", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private HmdConfig? GetSelectedConfig() {
            TreeViewItem selectedItem = (TreeViewItem)tree_hmds.SelectedItem;
            if (selectedItem is null)
            {
                MessageBox.Show("No HMD selected!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            bool hasParent = selectedItem.Parent is not null;
            bool hasChildren = selectedItem.Items.Count > 0;
            if (!hasParent || hasChildren) {
                return null;
            }

            string? configName = selectedItem.Header.ToString();
            string? hmdName = ((TreeViewItem)selectedItem.Parent).Header.ToString();
            TreeViewItem brandItem = (TreeViewItem)selectedItem.Parent;
            string? brandName = ((TreeViewItem)brandItem.Parent).Header.ToString();
            return configDatabase.GetConfig(brandName, hmdName, configName);
        }

        private Resolution? GetSelectedResolution() {
            ListViewItem? selectedItem = (ListViewItem)list_configs.SelectedItem;
            return selectedItem is null ? null : (Resolution)selectedItem.Tag;
        }

        private void tree_hmds_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            HmdConfig? selectedConfig = GetSelectedConfig();
            if (selectedConfig is null) {
                return;
            }

            FillConfigList(configDatabase, selectedConfig);
        }

        private void list_configs_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            HmdConfig? selectedConfig = GetSelectedConfig();
            if (selectedConfig is null) {
                return;
            }

            FillConfig(selectedConfig);
        }

        private void onAboutButtonClicked(object sender, RoutedEventArgs e) {
            AssemblyAttributes.RepositoryUrl.OpenInDefaultBrowser();
        }

        private void onCheckForUpdatesClicked(object sender, RoutedEventArgs e) {
            AssemblyAttributes.RepositoryUrl.CombinePath("releases").OpenInDefaultBrowser();
        }

        private void onDiscordButtonClicked(object sender, RoutedEventArgs e) {
            new Uri(AppSettings.Default.DiscordInviteUrl).OpenInDefaultBrowser();
        }

        private void onOpenFileButtonClicked(object sender, RoutedEventArgs e) {
            string? buttonText = ((MenuItem)sender).Header.ToString();
            _ = MessageBox.Show(buttonText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void onOpenDirectoryButtonClicked(object sender, RoutedEventArgs e) {
            string? buttonText = ((MenuItem)sender).Header.ToString();
            _ = MessageBox.Show(buttonText, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void onSettingsButtonClicked(object sender, RoutedEventArgs e) {
            UI.Settings settingsWindow = new();
            settingsWindow.Show();
        }

        private void onExitButtonClicked(object sender, RoutedEventArgs e) {
            Application.Current.Shutdown();
        }

        private void ChangeResolutionCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            ChangeResolutionCheckbox.IsChecked = true;
        }
    }
}