using Humanizer;
using NLog;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Windows;

namespace SCVRPatcher {

    internal static class Extensions {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        #region Reflection

        public static Dictionary<string, object> ToDictionary(this object instanceToConvert) {
            return instanceToConvert.GetType()
              .GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
              .ToDictionary(
              propertyInfo => propertyInfo.Name,
              propertyInfo => Extensions.ConvertPropertyToDictionary(propertyInfo, instanceToConvert));
        }

        private static object ConvertPropertyToDictionary(PropertyInfo propertyInfo, object owner) {
            Type propertyType = propertyInfo.PropertyType;
            object propertyValue = propertyInfo.GetValue(owner);

            // If property is a collection don't traverse collection properties but the items instead
            if (!propertyType.Equals(typeof(string)) && (typeof(ICollection<>).Name.Equals(propertyValue.GetType().BaseType.Name) || typeof(Collection<>).Name.Equals(propertyValue.GetType().BaseType.Name))) {
                var collectionItems = new List<Dictionary<string, object>>();
                var count = (int)propertyType.GetProperty("Count").GetValue(propertyValue);
                PropertyInfo indexerProperty = propertyType.GetProperty("Item");

                // Convert collection items to dictionary
                for (var index = 0; index < count; index++) {
                    object item = indexerProperty.GetValue(propertyValue, new object[] { index });
                    PropertyInfo[] itemProperties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);

                    if (itemProperties.Any()) {
                        Dictionary<string, object> dictionary = itemProperties
                          .ToDictionary(
                            subtypePropertyInfo => subtypePropertyInfo.Name,
                            subtypePropertyInfo => Extensions.ConvertPropertyToDictionary(subtypePropertyInfo, item));
                        collectionItems.Add(dictionary);
                    }
                }

                return collectionItems;
            }

            // If property is a string stop traversal (ignore that string is a char[])
            if (propertyType.IsPrimitive || propertyType.Equals(typeof(string))) {
                return propertyValue;
            }

            PropertyInfo[] properties = propertyType.GetProperties(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance);
            if (properties.Any()) {
                return properties.ToDictionary(
                                    subtypePropertyInfo => subtypePropertyInfo.Name,
                                    subtypePropertyInfo => (object)Extensions.ConvertPropertyToDictionary(subtypePropertyInfo, propertyValue));
            }

            return propertyValue;
        }

        #endregion Reflection

        #region DateTime

        public static bool ExpiredSince(this DateTime dateTime, int minutes) {
            return (dateTime - DateTime.Now).TotalMinutes < minutes;
        }

        public static TimeSpan StripMilliseconds(this TimeSpan time) {
            return new TimeSpan(time.Days, time.Hours, time.Minutes, time.Seconds);
        }

        #endregion DateTime

        #region DirectoryInfo

        public static void OpenInExplorer(this DirectoryInfo dir) => Utils.StartProcess("explorer.exe", args: dir.Quote());

        public static void ShowInExplorer(this DirectoryInfo dir) => Utils.StartProcess("explorer.exe", args: dir.Parent.Quote());

        public static string ToFullString(this DirectoryInfo dir) => $"{dir.Quote()}{(dir.Exists ? " (exists)" : string.Empty)}{(dir.EnumerateFileSystemInfos().Any() ? " (not empty)" : " (empty)")}";

        public static string Quote(this DirectoryInfo dir) => SurroundWith(dir.FullName, "\"");

        public static DirectoryInfo Combine(this DirectoryInfo dir, params string[] paths) {
            var final = dir.FullName;
            foreach (var path in paths) {
                final = Path.Combine(final, path);
            }
            return new DirectoryInfo(final);
        }

        #endregion DirectoryInfo

        #region FileInfo

        public static void OpenWithDefaultApp(this FileInfo file) => System.Diagnostics.Process.Start(file.FullName);

        public static void OpenInExplorer(this FileInfo file) => Utils.StartProcess("explorer.exe", args: file.Quote());

        public static void ShowInExplorer(this FileInfo file) => Utils.StartProcess("explorer.exe", args: file.Directory.Quote());

        public static string ToFullString(this FileInfo file) => $"{file.Quote()}{(file.Exists ? " (" + file.Length.Bytes().Humanize() + ")" : string.Empty)}";

        public static string Quote(this FileInfo file) => SurroundWith(file.FullName, "\"");

        public static string ReadAllText(this FileInfo file) => File.ReadAllText(file.FullName);

        public static IEnumerable<string> ReadAllLines(this FileInfo file) => File.ReadAllLines(file.FullName);

        public static void WriteAllText(this FileInfo file, string text) => File.WriteAllText(file.FullName, text);

        public static void WriteAllLines(this FileInfo file, IEnumerable<string> lines) => File.WriteAllLines(file.FullName, lines);

        public static FileInfo WithExtension(this FileInfo file, string extension) => new FileInfo(file.FullName + "." + extension);

        public static FileInfo WithoutExtension(this FileInfo file, string extension) => new FileInfo(file.FullName.ReplaceLastOccurrence("." + extension, string.Empty));

        public static bool Backup(this FileInfo file, bool force = false, string extension = "bak") {
            var backupFile = file.WithExtension(extension);
            if (backupFile.Exists && !force) Logger.Warn($"Backup file already exists: {backupFile.Quote()} {(force ? ", overwriting anyway..." : string.Empty)}");
            try { file.CopyTo(backupFile.FullName, true); return true; } catch (Exception ex) { Logger.Error(ex); return false; }
        }

        public static bool Restore(this FileInfo file, string extension = "bak") {
            var originalFile = file.WithoutExtension(extension);
            if (!originalFile.Exists) Logger.Warn($"Original file does not exist: {originalFile.Quote()}");
            try { file.CopyTo(originalFile.FullName, true); return true; } catch (Exception ex) { Logger.Error(ex); return false; }
        }

        public static FileInfo CombineFile(this DirectoryInfo dir, params string[] paths) {
            var final = dir.FullName;
            foreach (var path in paths) {
                final = Path.Combine(final, path);
            }
            return new FileInfo(final);
        }

        public static FileInfo Combine(this FileInfo file, params string[] paths) {
            var final = file.DirectoryName;
            foreach (var path in paths) {
                final = Path.Combine(final, path);
            }
            return new FileInfo(final);
        }

        public static string FileNameWithoutExtension(this FileInfo file) {
            return Path.GetFileNameWithoutExtension(file.Name);
        }

        /*public static string Extension(this FileInfo file) {
            return Path.GetExtension(file.Name);
        }*/

        public static void AppendLine(this FileInfo file, string line) {
            try {
                if (!file.Exists) file.Create();
                File.AppendAllLines(file.FullName, new string[] { line });
            } catch { }
        }

        #endregion FileInfo

        #region UI

        public static IEnumerable<System.Windows.Forms.TreeNode> GetAllChilds(this System.Windows.Forms.TreeNodeCollection nodes) {
            foreach (System.Windows.Forms.TreeNode node in nodes) {
                yield return node;

                foreach (var child in GetAllChilds(node.Nodes))
                    yield return child;
            }
        }

        #endregion UI

        #region Object

        public static string ToJson(this object obj, bool indented = false) {
            return JsonSerializer.Serialize(obj, indented ? JsonSerializerOptionsIndented : JsonSerializerOptions);
        }

        #endregion Object

        #region String

        public static string WithSuffix(this string str, string suffix) => $"{str}{suffix}";

        public static string WithPrefix(this string str, string prefix) => $"{prefix}{str}";

        public static string ToTitleCase(this string source, string langCode = "en-US") {
            return new CultureInfo(langCode, false).TextInfo.ToTitleCase(source);
        }

        public static bool Contains(this string source, string toCheck, StringComparison comp) {
            return source?.IndexOf(toCheck, comp) >= 0;
        }

        public static bool IsNullOrEmpty(this string source) {
            return string.IsNullOrEmpty(source);
        }

        public static bool IsNullOrWhiteSpace(this string source) {
            return string.IsNullOrWhiteSpace(source);
        }

        public static string[] Split(this string source, string split, int count = -1, StringSplitOptions options = StringSplitOptions.None) {
            if (count != -1) return source.Split(new string[] { split }, count, options);
            return source.Split(new string[] { split }, options);
        }

        public static string Remove(this string Source, string Replace) {
            return Source.Replace(Replace, string.Empty);
        }

        public static string ReplaceLastOccurrence(this string Source, string Find, string Replace) {
            int place = Source.LastIndexOf(Find);
            if (place == -1)
                return Source;
            string result = Source.Remove(place, Find.Length).Insert(place, Replace);
            return result;
        }

        public static string Ext(this string text, string extension) {
            return text + "." + extension;
        }

        public static string Quote(this string text) {
            return SurroundWith(text, "\"");
        }

        public static string Enclose(this string text) {
            return SurroundWith(text, "(", ")");
        }

        public static string Brackets(this string text) {
            return SurroundWith(text, "[", "]");
        }

        public static string SurroundWith(this string text, string surrounds) {
            return surrounds + text + surrounds;
        }

        public static string SurroundWith(this string text, string starts, string ends) {
            return starts + text + ends;
        }

        #endregion String

        #region Dict

        public static void AddSafe(this IDictionary<string, string> dictionary, string key, string value) {
            if (!dictionary.ContainsKey(key))
                dictionary.Add(key, value);
        }

        #endregion Dict

        #region List

        public static string ToQueryString(this NameValueCollection nvc) {
            if (nvc == null) return string.Empty;

            StringBuilder sb = new StringBuilder();

            foreach (string key in nvc.Keys) {
                if (string.IsNullOrWhiteSpace(key)) continue;

                string[] values = nvc.GetValues(key);
                if (values == null) continue;

                foreach (string value in values) {
                    sb.Append(sb.Length == 0 ? "?" : "&");
                    sb.AppendFormat("{0}={1}", key, value);
                }
            }

            return sb.ToString();
        }

        public static bool GetBool(this NameValueCollection collection, string key, bool defaultValue = false) {
            if (!collection.AllKeys.Contains(key, StringComparer.OrdinalIgnoreCase)) return false;
            var trueValues = new string[] { true.ToString(), "yes", "1" };
            if (trueValues.Contains(collection[key], StringComparer.OrdinalIgnoreCase)) return true;
            var falseValues = new string[] { false.ToString(), "no", "0" };
            if (falseValues.Contains(collection[key], StringComparer.OrdinalIgnoreCase)) return true;
            return defaultValue;
        }

        public static string GetString(this NameValueCollection collection, string key) {
            if (!collection.AllKeys.Contains(key)) return collection[key];
            return null;
        }

        public static T PopFirst<T>(this IEnumerable<T> list) => list.ToList().PopAt(0);

        public static T PopLast<T>(this IEnumerable<T> list) => list.ToList().PopAt(list.Count() - 1);

        public static T PopAt<T>(this List<T> list, int index) {
            T r = list.ElementAt<T>(index);
            list.RemoveAt(index);
            return r;
        }

        #endregion List

        #region Uri

        public static Uri CombinePath(this Uri uri, params string[] paths) { // potentially broken
            var final = uri.AbsoluteUri;
            foreach (var path in paths) {
                final = Path.Combine(final, path);
            }
            return new Uri(final);
        }

        public static void OpenInDefaultBrowser(this Uri url) {
            try {
                System.Diagnostics.Process.Start(url.AbsoluteUri);
            } catch (Exception ex) {
                Logger.Error(ex);
                try {
                    var startInfo = new System.Diagnostics.ProcessStartInfo {
                        FileName = url.AbsoluteUri,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(startInfo);
                } catch (Exception ex2) {
                    Logger.Error(ex2);
                    MessageBox.Show($"We were unable to open the URL\n{url.AbsoluteUri.Quote()}\nin your default browser!\n\nTherefor has been copied to your clipboard instead!", ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
                    Clipboard.SetText(url.AbsoluteUri);
                }
            }
        }

        private static readonly Regex QueryRegex = new Regex(@"[?&](\w[\w.]*)=([^?&]+)");

        public static IReadOnlyDictionary<string, string> ParseQueryString(this Uri uri) {
            var match = QueryRegex.Match(uri.PathAndQuery);
            var paramaters = new Dictionary<string, string>();
            while (match.Success) {
                paramaters.Add(match.Groups[1].Value, match.Groups[2].Value);
                match = match.NextMatch();
            }
            return paramaters;
        }

        #endregion Uri

        #region Enum

        public static string GetDescription(this Enum value) {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null) {
                FieldInfo field = type.GetField(name);
                if (field != null) {
                    //Check for Description Attributes in the Field
                    DescriptionAttribute
                        attr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null) {
                        return attr.Description;
                    }
                }
            }
            return null;
        }

        public static T GetValueFromDescription<T>(string description, bool returnDefault = false) {
            var type = typeof(T);
            if (!type.IsEnum) throw new InvalidOperationException();
            foreach (var field in type.GetFields()) {
                var attribute = Attribute.GetCustomAttribute(field,
                    typeof(DescriptionAttribute)) as DescriptionAttribute;
                if (attribute != null) {
                    if (attribute.Description == description)
                        return (T)field.GetValue(null);
                } else {
                    if (field.Name == description)
                        return (T)field.GetValue(null);
                }
            }
            if (returnDefault) return default(T);
            else throw new ArgumentException("Not found.", "description");
        }

        #endregion Enum

        #region Task

        public static async Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan timeout) {
            using (var timeoutCancellationTokenSource = new CancellationTokenSource()) {
                var completedTask = await Task.WhenAny(task, Task.Delay(timeout, timeoutCancellationTokenSource.Token));
                if (completedTask == task) {
                    timeoutCancellationTokenSource.Cancel();
                    return await task;  // Very important in order to propagate exceptions
                } else {
                    return default(TResult);
                }
            }
        }

        #endregion Task

        #region bool

        public static string ToYesNo(this bool input) => input ? "Yes" : "No";

        public static string ToEnabledDisabled(this bool input) => input ? "Enabled" : "Disabled";

        public static string ToOnOff(this bool input) => input ? "On" : "Off";

        #endregion bool

        #region json

        public static JsonSerializerOptions JsonSerializerOptionsIndented = new JsonSerializerOptions() {
            WriteIndented = true,
            Converters = { new IPAddressConverter() }
        };

        public static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions() {
            WriteIndented = false,
            Converters = { new IPAddressConverter() }
        };

        public class IPAddressConverter : JsonConverter<IPAddress> {

            public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                return IPAddress.Parse(reader.GetString() ?? string.Empty);
            }

            public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options) {
                writer.WriteStringValue(value.ToString());
            }
        }

        internal static class Converter {

            public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General) {
                Converters =
                {
                new DateOnlyConverter(),
                new TimeOnlyConverter(),
                IsoDateTimeOffsetConverter.Singleton
            },
            };
        }

        internal class ParseStringConverter : JsonConverter<long> {

            public override bool CanConvert(Type t) => t == typeof(long);

            public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                var value = reader.GetString();
                long l;
                if (Int64.TryParse(value, out l)) {
                    return l;
                }
                throw new Exception("Cannot unmarshal type long");
            }

            public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options) {
                JsonSerializer.Serialize(writer, value.ToString(), options);
                return;
            }

            public static readonly ParseStringConverter Singleton = new ParseStringConverter();
        }

        public class DateOnlyConverter : JsonConverter<DateOnly> {
            private readonly string serializationFormat;

            public DateOnlyConverter() : this(null) {
            }

            public DateOnlyConverter(string? serializationFormat) {
                this.serializationFormat = serializationFormat ?? "yyyy-MM-dd";
            }

            public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                var value = reader.GetString();
                return DateOnly.Parse(value!);
            }

            public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString(serializationFormat));
        }

        public class TimeOnlyConverter : JsonConverter<TimeOnly> {
            private readonly string serializationFormat;

            public TimeOnlyConverter() : this(null) {
            }

            public TimeOnlyConverter(string? serializationFormat) {
                this.serializationFormat = serializationFormat ?? "HH:mm:ss.fff";
            }

            public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                var value = reader.GetString();
                return TimeOnly.Parse(value!);
            }

            public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
                => writer.WriteStringValue(value.ToString(serializationFormat));
        }

        internal class IsoDateTimeOffsetConverter : JsonConverter<DateTimeOffset> {

            public override bool CanConvert(Type t) => t == typeof(DateTimeOffset);

            private const string DefaultDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.FFFFFFFK";

            private DateTimeStyles _dateTimeStyles = DateTimeStyles.RoundtripKind;
            private string? _dateTimeFormat;
            private CultureInfo? _culture;

            public DateTimeStyles DateTimeStyles {
                get => _dateTimeStyles;
                set => _dateTimeStyles = value;
            }

            public string? DateTimeFormat {
                get => _dateTimeFormat ?? string.Empty;
                set => _dateTimeFormat = (string.IsNullOrEmpty(value)) ? null : value;
            }

            public CultureInfo Culture {
                get => _culture ?? CultureInfo.CurrentCulture;
                set => _culture = value;
            }

            public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options) {
                string text;

                if ((_dateTimeStyles & DateTimeStyles.AdjustToUniversal) == DateTimeStyles.AdjustToUniversal
                    || (_dateTimeStyles & DateTimeStyles.AssumeUniversal) == DateTimeStyles.AssumeUniversal) {
                    value = value.ToUniversalTime();
                }

                text = value.ToString(_dateTimeFormat ?? DefaultDateTimeFormat, Culture);

                writer.WriteStringValue(text);
            }

            public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
                string? dateText = reader.GetString();

                if (string.IsNullOrEmpty(dateText) == false) {
                    if (!string.IsNullOrEmpty(_dateTimeFormat)) {
                        return DateTimeOffset.ParseExact(dateText, _dateTimeFormat, Culture, _dateTimeStyles);
                    } else {
                        return DateTimeOffset.Parse(dateText, Culture, _dateTimeStyles);
                    }
                } else {
                    return default(DateTimeOffset);
                }
            }

            public static readonly IsoDateTimeOffsetConverter Singleton = new IsoDateTimeOffsetConverter();
        }

        #endregion json

        #region Others

        public enum PropertyScope {
            User,
            Application
        }

        public static PropertyScope GetScope(this SettingsPropertyValue property) {
            throw new NotImplementedException("SettingsPropertyValue.GetScope()");
            if (property.Property.Attributes["System.Configuration.UserScopedSettingAttribute"] != null && property.IsDirty)
                return PropertyScope.User;
            return PropertyScope.Application;
        }

        public static string ToFullString(this ProcessStartInfo processStartInfo) {
            var ret = new List<string>();
            ret.Add($"FileName=\"{processStartInfo.FileName}\"");
            ret.Add($"Arguments=\"{processStartInfo.Arguments}\"");
            ret.Add($"Verb=\"{processStartInfo.Verb}\"");
            ret.Add($"UseShellExecute=\"{processStartInfo.UseShellExecute}\"");
            ret.Add($"WorkingDirectory=\"{processStartInfo.WorkingDirectory}\"");
            ret.Add($"WindowStyle=\"{processStartInfo.WindowStyle}\"");
            return $"ProcessStartInfo({string.Join(',', ret)})"; // $"{processStartInfo.FileName.Quote()} {processStartInfo.Arguments}";
        }

        #endregion Others
    }
}