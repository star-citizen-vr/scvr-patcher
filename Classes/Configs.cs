﻿#nullable enable
#pragma warning disable CS8618
#pragma warning disable CS8601
#pragma warning disable CS8603

namespace SCVRPatcher {
    using System;
    using System.Collections.Generic;

    using System.Text.Json;
    using System.Text.Json.Serialization;
    using System.Globalization;
    using System.IO;
    using System.Net.Http;
    using System.Text;
    using Brand = Dictionary<string, Dictionary<string, Dictionary<string, HmdConfig>>>;
    using System.Net.Http.Json;

    public partial class ConfigDataBase {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        [JsonIgnore]
        public string? Raw { get; private set; }
        [JsonIgnore]
        public string? Md5 { get; private set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("common")]
        public virtual Common Common { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("brands")]
        public Brand Brands { get; set; }

        [JsonIgnore]
        public bool IsEmptyOrMissing => Brands is null || Brands.Count == 0;

        public HmdConfig GetConfig(string brand, string hmd, string config) {
            Logger.Info($"Trying to get config for {brand}/{hmd}/{config}");
            var b = Brands.First(b => b.Key == brand);
            var h = b.Value.First(h => h.Key == hmd);
            var c = h.Value.First(c => c.Key == config);
            return c.Value;
        }

        public List<string> GetPath(HmdConfig config) {
            var path = new List<string>();
            foreach (var brand in Brands) {
                foreach (var hmd in brand.Value) {
                    foreach (var c in hmd.Value) {
                        if (c.Value == config) {
                            path.Add(brand.Key);
                            path.Add(hmd.Key);
                            path.Add(c.Key);
                            return path;
                        }
                    }
                }
            }
            return path;
        }

        public void ToFile(FileInfo file) {
            File.WriteAllText(file.FullName, this.ToJson());//.Replace("\r\n", "\n"););
        }

        public static ConfigDataBase? FromUrl(Uri uri) {
            try {
                using (var client = new HttpClient()) {
                    Logger.Info($"Downloading available configs from {uri}...");
                    var response = client.GetAsync(uri).Result;
                    if (response.IsSuccessStatusCode) {
                        var json = response.Content.ReadAsStringAsync().Result;
                        return FromJson(json);
                    } else {
                        throw new Exception($"Failed to download available configs! (Error {response.StatusCode})");
                    }
                }
            } catch (Exception e) {
                Logger.Error(e);
            }
            return null;
        }
        public static ConfigDataBase? FromFile(FileInfo file) {
            if (!file.Exists) {
                Logger.Error($"Config file not found: {file.Quote()}");
                return null;
            }
            try { File.ReadAllText(file.FullName); } catch (Exception e) {
                Logger.Error($"Error reading file {file.Quote()}!", e);
            }
            var text = File.ReadAllText(file.FullName);
            Logger.Debug($"Read {text.Length} chars from {file.Quote()}");
            return FromJson(text);
        }
        public static ConfigDataBase? FromJson(string json) {
            try {
                var db = JsonSerializer.Deserialize<ConfigDataBase>(json, Converter.Settings);
                db.Raw = json;
                db.Md5 = json.GetMd5Hash();
                return db;
            } catch (Exception e) {
                Logger.Error($"Error parsing json: {e} (Length: {json.Length})");
            }
            return null;
        }
    }
    public static partial class Serialize {
        public static string ToJson(this ConfigDataBase self) => JsonSerializer.Serialize(self, Converter.Settings);
    }

    public partial class Common {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Alternative Interger Resolutions (small list)")]
        public virtual List<Resolution> AlternativeIntergerResolutionsSmallList { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Alternative Interger Resolutions (big list)")]
        public virtual List<Resolution> AlternativeIntergerResolutionsBigList { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Give me all the resolutions")]
        public virtual List<Resolution> GiveMeAllTheResolutions { get; set; }
    }

    public partial class Resolution {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("w")]
        public virtual double? Width { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("h")]
        public virtual double? Height { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("d")]
        public virtual string? Description { get; set; }
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("p")]
        public virtual string? Percentage { get; set; }

        public override string ToString() {
            var sb = new StringBuilder($"{Width} x {Height}");
            if (!string.IsNullOrEmpty(Description)) sb.Append($" ({Description})");
            if (!string.IsNullOrEmpty(Percentage)) sb.Append($" {Percentage}%");
            return sb.ToString();
        }

        public static bool operator >(Resolution a, Resolution b) {
            return a.Width > b.Width || a.Height > b.Height;
        }
        public static bool operator <(Resolution a, Resolution b) {
            return a.Width < b.Width || a.Height < b.Height;
        }
    }

    public partial class HmdConfig {

        //[JsonIgnore]
        //public string Brand;
        //[JsonIgnore]
        //public string Model;
        //[JsonIgnore]
        //public string Name;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Hz")]
        public virtual double? Hz { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("SC Attributes FOV")]
        public virtual double? Fov { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("VorpX Config Pixel 1:1 Zoom")]
        public virtual double? Zoom { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] // Todo: Do we need this?
        //[JsonPropertyName("All Possible Lens Configurations")]
        //public virtual List<string> LensConfigurations { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("Custom Resolution List")]
        public virtual List<Resolution> CustomResolutions { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("Change Game Window Resolution Checkbox")]
        //public virtual bool? ChangeResolutionCheckbox { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("Headset Name")]
        //public virtual string HeadsetName { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("DB-UID")]
        //public virtual long? DbUid { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("All Possible Lens Configurations")]
        //public virtual List<string> AllPossibleLensConfigurations { get; set; } = new();

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("Hz")]
        //public virtual int? Hz { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("SC Attributes FOV")]
        //public virtual long? Sc
        //sFOV { get; set; }




        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("Notes")]
        //public virtual List<string>? Notes { get; set; } = new();

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("Suggested Minimum VorpX Pixel 1:1 Zoom")]
        //public virtual double? SuggestedMinimumVorpXPixel11Zoom { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("Suggested Maximum VorpX Pixel 1:1 Zoom")]
        //public virtual double? SuggestedMaximumVorpXPixel11Zoom { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("VorpX Config Pixel 1:1 Zoom")]
        //public virtual double? VorpXConfigPixel11Zoom { get; set; }

        //[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        //[JsonPropertyName("Custom Resolution List")]
        //public virtual List<string> CustomResolutionList { get; set; } = new();
        //public virtual List<string> CustomResolutionList { get; set; } = new();
    }

    //public enum ErrorReportScFovCap120 { ScCanTNativelyRunYourHeadsetSFov, UseVorpXZoomFunction };

    //public enum VorpXConfigPixel11Zoom { WAwAwAw, YouWillNeedToFindThisSetting };

    internal static class Converter {
        public static readonly JsonSerializerOptions Settings = new(JsonSerializerDefaults.General) {
            Converters =
            {
                //ErrorReportScFovCap120Converter.Singleton,
                //VorpXConfigPixel11ZoomConverter.Singleton,
                new DateOnlyConverter(),
                new TimeOnlyConverter(),
                IsoDateTimeOffsetConverter.Singleton
            },
        };
    }

    //internal class ErrorReportScFovCap120Converter : JsonConverter<ErrorReportScFovCap120> {
    //    public override bool CanConvert(Type t) => t == typeof(ErrorReportScFovCap120);

    //    public override ErrorReportScFovCap120 Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
    //        var value = reader.GetString();
    //        switch (value) {
    //            case "SC can't natively run your headset's FOV":
    //                return ErrorReportScFovCap120.ScCanTNativelyRunYourHeadsetSFov;
    //            case "Use VorpX Zoom Function":
    //                return ErrorReportScFovCap120.UseVorpXZoomFunction;
    //        }
    //        throw new Exception("Cannot unmarshal type ErrorReportScFovCap120");
    //    }

    //    public override void Write(Utf8JsonWriter writer, ErrorReportScFovCap120 value, JsonSerializerOptions options) {
    //        switch (value) {
    //            case ErrorReportScFovCap120.ScCanTNativelyRunYourHeadsetSFov:
    //                JsonSerializer.Serialize(writer, "SC can't natively run your headset's FOV", options);
    //                return;
    //            case ErrorReportScFovCap120.UseVorpXZoomFunction:
    //                JsonSerializer.Serialize(writer, "Use VorpX Zoom Function", options);
    //                return;
    //        }
    //        throw new Exception("Cannot marshal type ErrorReportScFovCap120");
    //    }

    //    public static readonly ErrorReportScFovCap120Converter Singleton = new ErrorReportScFovCap120Converter();
    //}
    //internal class VorpXConfigPixel11ZoomConverter : JsonConverter<VorpXConfigPixel11Zoom> {
    //    public override bool CanConvert(Type t) => t == typeof(VorpXConfigPixel11Zoom);

    //    public override VorpXConfigPixel11Zoom Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
    //        var value = reader.GetString();
    //        switch (value) {
    //            case "You will need to find this setting":
    //                return VorpXConfigPixel11Zoom.YouWillNeedToFindThisSetting;
    //            case "w Aw aw aw":
    //                return VorpXConfigPixel11Zoom.WAwAwAw;
    //        }
    //        throw new Exception("Cannot unmarshal type VorpXConfigPixel11Zoom");
    //    }

    //    public override void Write(Utf8JsonWriter writer, VorpXConfigPixel11Zoom value, JsonSerializerOptions options) {
    //        switch (value) {
    //            case VorpXConfigPixel11Zoom.YouWillNeedToFindThisSetting:
    //                JsonSerializer.Serialize(writer, "You will need to find this setting", options);
    //                return;
    //            case VorpXConfigPixel11Zoom.WAwAwAw:
    //                JsonSerializer.Serialize(writer, "w Aw aw aw", options);
    //                return;
    //        }
    //        throw new Exception("Cannot marshal type VorpXConfigPixel11Zoom");
    //    }

    //    public static readonly VorpXConfigPixel11ZoomConverter Singleton = new VorpXConfigPixel11ZoomConverter();
    //}

    public class DateOnlyConverter : JsonConverter<DateOnly> {
        private readonly string serializationFormat;
        public DateOnlyConverter() : this(null) { }

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

        public TimeOnlyConverter() : this(null) { }

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
}
#pragma warning restore CS8618
#pragma warning restore CS8601
#pragma warning restore CS8603

