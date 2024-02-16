using NLog;
using System.IO;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace SCVRPatcher {

    public class AttributesFile : XmlFile {
        public Attributes Content { get; private set; }

        public static readonly List<string> attributesToRemove = new() {
            "SysSpec", "SysSpecGameEffects", "SysSpecGasCloud", "SysSpecObjectDetail", "SysSpecParticles", "SysSpecPostProcessing", "SysSpecShading", "SysSpecShadows", "SysSpecWater"
        };

        public static readonly Dictionary<string, object>
        sToSet = new() {
            { "AutoZoomOnSelectedTarget", 0 },
            { "AutoZoomOnSelectedTargetStrength", 0 },
            { "CameraSpringMovement", 0 },
            { "ChromaticAberration", 0 },
            { "FilmGrain", 0 },
            { "GForceHeadBobScale", 0 },
            { "GForceZoomScale", 0 },
            { "HeadtrackingEnableRollFPS", 0 },
            { "LookAheadEnabledShip", 0 },
            { "MotionBlur", 0 },
            { "ShakeScale", 0 },
            { "Sharpening", 0 },
            { "VSync", 0 },
            { "WindowMode", 2 },
            //{ "ScatterDist", 0 },
            //{ "TerrainTessDistance", 0 }
        };

        public AttributesFile(FileInfo file) : base(file) {
            Initizalize();
            Logger.Info($"Found attributes file with version {Content?.Version} containing {Content?.Attr.Count} attributes");
        }

        public override void Initizalize() {
            Serializer = new XmlSerializer(typeof(Attributes));
            Load();
        }

        public override void Load(FileInfo? file = null) {
            file ??= File;
            using (var reader = File.OpenText()) {
                var deserialized = Serializer.Deserialize(reader) as Attributes;
                if (deserialized is null) throw new Exception($"Failed to deserialize {File.FullName}");
                Content = deserialized;
            }
            Logger.Info($"Loaded {File.FullName}");
        }

        public List<Attr> Get(string name) {
            return Content.Attr.Where(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList();
        }

        public bool AddOrUpdate(string name, object? value) {
            if (value is null) throw new ArgumentNullException(nameof(value));
            var attr = Get(name).FirstOrDefault();
            var changed = false;
            if (attr is null) {
                attr = new Attr() { Name = name, Value = value?.ToString() };
                Content.Attr.Add(attr);
                changed = true;
            } else {
                if (attr.Value != value?.ToString()) {
                    attr.Value = value?.ToString();
                    changed = true;
                }
            }
            if (changed) Logger.Info($"Set attribute: {attr.Name.Quote()} to {attr.Value?.Quote()}");
            else Logger.Info($"Attribute {attr.Name.Quote()} already set to {attr.Value?.Quote()}");
            return changed;
        }

        public bool Remove(string key) => Remove(new List<string>() { key });

        public bool Remove(IEnumerable<string> keys) {
            var found = 0;
            foreach (var key in keys) {
                var attr = Get(key).FirstOrDefault();
                if (attr is not null) {
                    Content.Attr.Remove(attr);
                    found++;
                }
            }
            Logger.Info($"Removed {found} / {keys.Count()} attributes.");
            return found > 0;
        }
        public override bool Patch(HmdConfig config, Resolution resolution) => Patch(config, resolution, false);
        public bool Patch(HmdConfig config, Resolution resolution, bool changeresolution) {
            Logger.Info($"Patching {File.FullName}");
            var changed = Remove(attributesToRemove);
            Logger.Info($"Removed {attributesToRemove.Count} attributes.");
            var attributesToSet = new Dictionary<string, object>(); // sToSet.Where(x => !Get(x.Key).Any()).ToDictionary(x => x.Key, x => x.Value);
            foreach (var attribute in sToSet) {
                var existingAttributes = Get(attribute.Key);
                if (existingAttributes.Count > 0) {
                    attributesToSet.Add(attribute.Key, attribute.Value);
                }
            }
            foreach (var item in attributesToSet) {
                changed |= AddOrUpdate(item.Key, item.Value);
                Logger.Info($"Set attribute: {item.Key} to {item.Value}");
            }
            if (config.Fov is not null) changed |= AddOrUpdate("FOV", config.Fov);
            // TODO: Add a way to change resolution based on if user checks a checkbox, see below
            // I'm not able to figure out how to get the 'isChecked' to work here....

            if (changeresolution) {
            Logger.Info("Changing resolution to match HMD");
                if (resolution.Height is not null) changed |= AddOrUpdate("Height", resolution.Height);
                if (resolution.Width is not null) changed |= AddOrUpdate("Width", resolution.Width);
                Logger.Info($"Changed resolution to {resolution.Width}x{resolution.Height}");
            }
            if (changed) {
                Save();
                Logger.Info($"Patched {File.FullName}");
            } else Logger.Info($"No changes to {File.FullName}");
            return true;
        }

        public override bool Unpatch() {
            Logger.Info($"Unpatching {File.FullName}");
            File.Restore();
            //Save();
            Logger.Info($"Unpatched {File.FullName}");
            return true;
        }

        public override void Save(FileInfo? file = null, bool backup = true) {
            file ??= File;
            if (backup) file.Backup(force: true);
            using (var writer = XmlWriter.Create(File.CreateText(), Settings)) {
                Serializer.Serialize(writer, Content);
            }
            Logger.Info($"Saved {File.FullName}");
        }

        #region definitions

        [XmlRoot(ElementName = "Attr")]
        public class Attr {

            [XmlAttribute(AttributeName = "name")]
            public string Name { get; set; }

            [XmlAttribute(AttributeName = "value")]
            public string Value { get; set; }
        }

        [XmlRoot(ElementName = "Attributes")]
        public class Attributes {

            [XmlElement(ElementName = "Attr")]
            public List<Attr> Attr { get; set; }

            [XmlAttribute(AttributeName = "Version")]
            public int Version { get; set; }
        }

        #endregion definitions
    }
}