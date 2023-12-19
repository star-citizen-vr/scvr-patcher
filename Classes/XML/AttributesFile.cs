using NLog;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SCVRPatcher {
    public class AttributesFile : XmlFile {
        public Attributes Content { get; private set; }
        public static readonly List<string> attributesToRemove = new() {
            "SysSpec", "SysSpecGameEffects", "SysSpecGasCloud", "SysSpecObjectDetail", "SysSpecParticles", "SysSpecPostProcessing", "SysSpecShading", "SysSpecShadows", "SysSpecWater"
        };

        public AttributesFile(FileInfo file) : base(file) {
            Initizalize();
            foreach (var attr in Content.Attr) {
                Logger.Info($"Found attribute: {attr.Name} = {attr.Value}");
            }
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

        public void AddOrUpdate(string name, object value) {
            if (value is null) throw new ArgumentNullException(nameof(value));
            var attr = Get(name).FirstOrDefault();
            if (attr is null) {
                attr = new Attr() { Name = name, Value = value.ToString() };
                Content.Attr.Add(attr);
            } else {
                attr.Value = value.ToString();
            }
            Logger.Info($"Set attribute: {attr.Name.Quote()} to {attr.Value.Quote()}");
        }

        public void Remove(string key) => Remove(new List<string>() { key });
        public void Remove(IEnumerable<string> keys) {
            var found = 0;
            foreach (var key in keys) {
                var attr = Get(key).FirstOrDefault();
                if (attr is not null) {
                    Content.Attr.Remove(attr);
                    found++;
                }
            }
            Logger.Info($"Removed {found} / {keys.Count()} attributes.");
        }

        public override bool Patch(HmdConfig config, Resolution resolution) {
            Logger.Info($"Patching {File.FullName}");
            Remove(attributesToRemove);
            AddOrUpdate("AutoZoomOnSelectedTarget", 0);
            AddOrUpdate("AutoZoomOnSelectedTargetStrength", 0);
            AddOrUpdate("CameraSpringMovement", 0);
            AddOrUpdate("ChromaticAberration", 0);
            AddOrUpdate("FilmGrain", 0);
            AddOrUpdate("GForceHeadBobScale", 0);
            AddOrUpdate("GForceZoomScale", 0);
            AddOrUpdate("HeadtrackingEnableRollFPS", 0);
            AddOrUpdate("LookAheadEnabledShip", 0);
            AddOrUpdate("MotionBlur", 0);
            AddOrUpdate("ShakeScale", 0);
            AddOrUpdate("Sharpening", 0);
            AddOrUpdate("VSync", 0);
            AddOrUpdate("WindowMode", 2);
            AddOrUpdate("ScatterDist", 0);
            AddOrUpdate("TerrainTessDistance", 0);
            if (config.Fov is not null) AddOrUpdate("FOV", config.Fov);
            if (resolution.Height is not null) AddOrUpdate("Height", resolution.Height);
            if (resolution.Width is not null) AddOrUpdate("Width", resolution.Width);
            Save();
            Logger.Info($"Patched {File.FullName}");
            return true;
        }

        public override bool Unpatch() {
            Logger.Info($"Unpatching {File.FullName}");
            // Save();
            Logger.Info($"Unpatched {File.FullName}");
            return true;
        }

        public override void Save(FileInfo? file = null, bool backup = true) {
            file ??= File;
            if (backup) file.Backup();
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