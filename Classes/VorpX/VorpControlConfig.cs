using IniParser.Model;
using IniParser;
using NLog;
using System.IO;
using System.Diagnostics.Eventing.Reader;
using System.ComponentModel;

namespace SCVRPatcher {
    public partial class VorpX {
        public class VorpControlConfig {
            public static readonly FileInfo File = VorpX.VorpXConfigDir.CombineFile("vorpControl.ini");
            public static FileIniDataParser Parser = new FileIniDataParser();

            public IniData _Data;
            public IniFileData Data { get; set; }

            public VorpControlConfig() {
                Logger.Info("Loading VorpControlConfig");
                Logger.Debug($"VorpControlConfig.File: {File.Quote()}");
                if (!File.Exists) {
                    Logger.Warn($"VorpControlConfig File not found at {File.Quote()}");
                    return;
                }
                Load();
            }

            public void Load(FileInfo? file = null) {
                file ??= File;
                Logger.Info($"Loading VorpControlConfig File: {file.Quote()}");
                _Data = Parser.ReadFile(file.FullName);
                Data = ParseIniFile(_Data);
                Logger.Info($"Loaded VorpControlConfig File");
            }

            public IniFileData ParseIniFile(IniData data) {
                Logger.Info("Parsing VorpX Config File Data");
                IniFileData iniFileData = new();
                var categories = typeof(IniFileData).GetProperties();
                foreach (var category in categories) {
                    Logger.Debug($"Parsing VorpX Config File Category: [{category.Name}]");
                    if (category.Name == "Exclude") {
                        var excludedItems = new List<string>();
                        var excludeSection = data["Exclude"];
                        foreach (var excludedItem in excludeSection) {
                            excludedItems.Add(excludedItem.Value);
                        }
                        category.SetValue(iniFileData, excludedItems);
                        continue;
                    } else {
                        Logger.Debug($"Creating VorpX Config File Category: {category.Name} ({category.PropertyType})");
                        category.SetValue(iniFileData, Activator.CreateInstance(category.PropertyType));
                    }
                    var properties = category.PropertyType.GetProperties();
                    foreach (var property in properties) {
                        var value = data[category.Name][property.Name];
                        Logger.Debug($"Parsing VorpX Config File Property: {category.Name}.{property.Name}: {value}");
                        if (value != null) {
                            Logger.Debug($"Converting VorpX Config File Property from {value.GetType()} to {property.PropertyType}");
                            if (property.PropertyType == typeof(List<string>)) {
                                property.SetValue(iniFileData, value.Split(','));
                            } else if (property.PropertyType == typeof(bool)) {
                                Logger.Debug($"Converting VorpX Config File Property from {value.GetType()} to {typeof(bool)} (bool)");
                                if (value == "True") property.SetValue(iniFileData, true);
                                else if (value == "False") {
                                    Logger.Debug("Test 1");
                                    property.SetValue(iniFileData, false, null);
                                    Logger.Debug("Test 2");
                                }
                                //Logger.Debug($"Converted VorpX Config File Property from {value.GetType()} to {typeof(bool)} (bool): {property.GetValue(iniFileData)}");
                            } else {
                                var converter = TypeDescriptor.GetConverter(property.PropertyType);
                                var canConvert = converter.CanConvertFrom(value.GetType());
                                Logger.Debug($"Converting VorpX Config File Property from {value.GetType()} to {property.PropertyType}: {canConvert}");
                                if (canConvert) {
                                    property.SetValue(iniFileData, converter.ConvertFrom(value));
                                } else {
                                    Logger.Error($"Cannot convert value '{value}' of type '{value.GetType()}' to property '{property.Name}' of type '{property.PropertyType}'");
                                }
                            }
                        }
                    }
                }
                return iniFileData;
            }

            #region definitions
            public class IniFileData {
                public General? General { get; set; }
                public Injection? Injection { get; set; }
                public Display? Display { get; set; }
                public VirtualDisplay? VirtualDisplay { get; set; }
                public List<string> Exclude { get; set; } = new();
            }
            public class General {
                public bool bRunAsAdmin { get; set; }
                public bool bProfileUpdates { get; set; }
                public bool bLegacyUpdater { get; set; }
            }
            public class Injection {
                public int iInjectMode { get; set; }
                public int iStartKillTimeout { get; set; }
                public bool bKillSameName { get; set; }
                public bool bForceAlternativeInjection { get; set; }
                public bool bMinimizeRuntimeWindowsOnGameExit { get; set; }
            }
            public class Display {
                public bool bRestoreWindows { get; set; }
            }
            public class VirtualDisplay {
                public bool bManualAttach { get; set; }
                public bool bNoDisplayAttach { get; set; }
                public bool bHeadsetActivityAttach { get; set; }
                public bool bResolutionsAbove4K { get; set; }
                public string sCustomResolutions { get; set; }
            }
            #endregion
        }
    }

}
