using NLog;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SCVRPatcher {
    public abstract class XmlFile {
        public static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        public static readonly XmlWriterSettings Settings = new XmlWriterSettings() { Indent = true, OmitXmlDeclaration = true };
        public static XmlSerializer Serializer { get; set; }
        public FileInfo File { get; private set; }

        public XmlFile(FileInfo file) {
            File = file;
        }

        public abstract void Initizalize();

        public abstract void Load(FileInfo? file = null);

        public abstract bool Patch(HmdConfig config, Resolution resolution);

        public abstract bool Unpatch();

        public abstract void Save(FileInfo? file = null);

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
        #endregion definitions
    }
}