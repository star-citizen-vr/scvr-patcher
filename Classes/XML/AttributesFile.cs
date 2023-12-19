using NLog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;
using System.Xml.Serialization;

namespace SCVRPatcher {
    public class AttributesFile : XmlFile {
        public Attributes Content { get; private set; }

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
                Content = Serializer.Deserialize(reader) as Attributes;
            }
            Logger.Info($"Loaded {File.FullName}");
        }

        public override void AddOrUpdate(string name, string value) {
            var attr = Content.Attr.FirstOrDefault(x => x.Name == name);
            if (attr is null) {
                attr = new Attr() { Name = name, Value = value };
                Content.Attr.Add(attr);
            } else {
                attr.Value = value;
            }
            Logger.Info($"Set attribute: {attr.Name.Quote()} to {attr.Value.Quote()}");
        }

        public override bool Patch(HmdConfig config, Resolution resolution) {
            config.Fov.ToString();
            resolution.Height.ToString();
            resolution.Width.ToString();
            AddOrUpdate("AutoZoomOnSelectedTarget", "0");
            AddOrUpdate("AutoZoomOnSelectedTargetStrength", "0");
            AddOrUpdate("CameraSpringMovement", "0");
            AddOrUpdate("ChromaticAberration", "0");
            AddOrUpdate("FilmGrain", "0");
            AddOrUpdate("GForceHeadBobScale", "0");
            AddOrUpdate("GForceZoomScale", "0");
            AddOrUpdate("HeadtrackingEnableRollFPS", "0");
            AddOrUpdate("LookAheadEnabledShip", "0");
            AddOrUpdate("MotionBlur", "0");
            AddOrUpdate("ShakeScale", "0");
            AddOrUpdate("Sharpening", "0");
            AddOrUpdate("VSync", "0");
            AddOrUpdate("WindowMode", "2");
            AddOrUpdate("ScatterDist", "0");
            AddOrUpdate("TerrainTessDistance", "0");
            AddOrUpdate("FOV", config.Fov.ToString());
            AddOrUpdate("Height", resolution.Height.ToString());
            AddOrUpdate("Width", resolution.Width.ToString());
            Save();
            return true;
        }

        public override bool Unpatch() {
            return true;
        }

        public override void Save(FileInfo? file = null) {
            file ??= File;
            using (var writer = XmlWriter.Create(File.CreateText(), Settings)) {
                Serializer.Serialize(writer, Content);
            }
            Logger.Info($"Saved {File.FullName}");
        }

        //public void ToFile(FileInfo file) => file.WriteAllText(ToString());
        //public string ToString() {
        //    using (StringWriter writer = new StringWriter()) {
        //        Serializer.Serialize(writer, this);
        //        return writer.ToString();
        //    }
        //}

        //public static Attributes FromFile(FileInfo file) => FromString(file.ReadAllText());
        //public static Attributes FromString(string xml) {
        //    using (StringReader reader = new StringReader(xml)) {
        //        return Serializer.Deserialize(reader) as Attributes;
        //    }
        //}

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