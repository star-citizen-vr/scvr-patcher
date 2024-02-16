using System.Configuration;
using System.Windows;
using System.Windows.Controls;

namespace SCVRPatcher.UI {

    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public Settings() {
            InitializeComponent();
            FillSettings();
        }

        private void FillSettings() {
            stackpanel_settings.Children.Clear();
            var settings = AppSettings.Default;
            foreach (SettingsPropertyValue property in settings.PropertyValues) {
                try {
                    if (property is null) continue;
                    //var scope = property.GetScope();
                    //if (scope != Extensions.PropertyScope.User) continue;
                    var name = property.Name;
                    var value = property.PropertyValue;
                    var type = value.GetType();
                    var horizontalStackPanel = new StackPanel {
                        Orientation = Orientation.Horizontal
                    };
                    horizontalStackPanel.Children.Add(new TextBlock {
                        Text = name,
                        FontWeight = FontWeights.Bold
                    });
                    if (type == typeof(bool)) {
                        var checkBox = new CheckBox {
                            Content = name,
                            IsChecked = (bool)value
                        };
                        checkBox.Checked += (sender, e) => {
                            settings[name] = true;
                            settings.Save();
                        };
                        checkBox.Unchecked += (sender, e) => {
                            settings[name] = false;
                            settings.Save();
                        };
                        stackpanel_settings.Children.Add(checkBox);
                    } else if (type == typeof(string)) {
                        var textBox = new TextBox {
                            Text = (string)value
                        };
                        textBox.TextChanged += (sender, e) => {
                            settings[name] = textBox.Text;
                            settings.Save();
                        };
                        horizontalStackPanel.Children.Add(textBox);
                    }
                    if (horizontalStackPanel.Children.Count > 1) {
                        stackpanel_settings.Children.Add(horizontalStackPanel);
                    }
                } catch (Exception e) {
                    Logger.Error($"Failed to add setting {property}: {e}");
                }
            }
        }

        private void onSaveButtonClicked(object sender, RoutedEventArgs e) {
        }
    }
}