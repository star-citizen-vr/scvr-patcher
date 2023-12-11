using NLog;
using SCVRPatcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace SCVRPatcher {
    internal class HostsFile {
        internal const string EACHostName = "modules-cdn.eac-prod.on.epicgames.com";
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly Regex EntryRegex = new Regex(@"^([\w.]+)\s+(.*)$");
        private static readonly Regex WhiteSpaceRegex = new Regex(@"\s+");
        internal static readonly IPAddress Null = new IPAddress(new byte[] { 0, 0, 0, 0 });
        internal static readonly IPAddress Localhost = new IPAddress(new byte[] { 127, 0, 0, 1 });
        internal static FileInfo HostFile = new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\System32\drivers\etc\hosts");

        public List<HostsEntry> Entries = new();

        public HostsFile() {
            Load(HostFile);
        }

        public IEnumerable<HostsEntry> GetEntriesByDomain(string domain) => Entries.Where(e => e.Hostnames.Contains(domain));
        public IEnumerable<HostsEntry> GetEntryByIp(IPAddress ip) => Entries.Where(e => e.Ip.Equals(ip));
        public bool AddOrEnableByDomain(string domain, IPAddress ip, string comment = null) {
            var entries = GetEntriesByDomain(domain);
            if (entries.Count() > 1) {
                Logger.Warn($"{domain} found multiple times in hosts file, disabling all except first!");
                entries.Skip(1).ToList().ForEach(e => e.Enabled = false);
                entries.First().Enabled = true;
                entries.First().Ip = ip;
            }
            if (entries.Count() < 1) {
                Logger.Info($"{domain} not found in hosts file, adding it now...");
                Entries.Add(new HostsEntry() { Ip = ip, Hostnames = new List<string>() { domain } });
            } else {
                Logger.Info($"{HostsFile.EACHostName} found in hosts file, enabling it now...");
                entries.First().Enabled = true;
                entries.First().Ip = ip;
            }
            return true;
        }

        public override string ToString() {
            return string.Join(Environment.NewLine, ToLines());
        }
        public List<string> ToLines() {
            var lines = new List<string>();
            foreach (var entry in Entries) {
                lines.Add(entry.ToString());
            }
            return lines;
        }

        public void Save(FileInfo file = null, bool backup = true) {
            file ??= HostFile;
            Logger.Info($"Saving hosts file to {file.FullName}");
            if (file.Exists && backup) {
                var backupFile = new FileInfo(file.FullName + ".bak");
                Logger.Info($"Hosts file already exists, creating backup at {backupFile.FullName}");
                if (backupFile.Exists) backupFile.Delete();
                file.MoveTo(backupFile.FullName);
            } // Why is this buggy?
            File.WriteAllText(file.FullName, this.ToString());
            Logger.Info($"Saved hosts file to {file.FullName}");
        }

        public List<HostsEntry> Load(FileInfo file = null) {
            file ??= HostFile;
            if (!file.Exists) {
                Logger.Error($"Hosts file not found: {file.FullName}");
                return null;
            }
            Logger.Trace($"Loading hosts file from {file.FullName}");
            var lines = File.ReadAllLines(file.FullName);
            Logger.Info($"Loaded hosts file with {lines.Length} lines.");
            Entries.Clear();
            foreach (var _line in lines) {
                var line = _line.Trim();
                var entry = new HostsEntry();
                if (string.IsNullOrWhiteSpace(line)) {
                    Logger.Trace($"Line is empty or whitespace: {line}");
                    entry.Empty = true;
                    Entries.Add(entry);
                    continue;
                }
                if (line.StartsWith("#")) {
                    Logger.Trace($"Line starts with #, treating as disabled entry: {line}");
                    entry.Enabled = false;
                    line = line.Substring(1).Trim();
                }
                var parts = WhiteSpaceRegex.Split(line); // line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) {
                    Logger.Trace($"Line is too short: {line}");
                    entry.Comment = line.Replace("#", string.Empty).Trim();
                    Entries.Add(entry);
                    continue;
                }
                //if (!EntryRegex.IsMatch(line)) {
                //    Logger.Trace($"Line does not match valid EntryRegex: {line}");
                //    entry.Comment = line;
                //    Entries.Add(entry);
                //    continue;
                //}
                var success = IPAddress.TryParse(parts[0], out var ip);
                if (!success) {
                    Logger.Trace($"Not a valid IP: {parts[0]}");
                    entry.Comment = line;
                    Entries.Add(entry);
                    continue;
                }
                entry.Ip = ip;
                for (var i = 1; i < parts.Length; i++) {
                    if (parts[i].StartsWith("#")) {
                        entry.Comment = string.Join(' ', parts.Skip(i)).Replace("#", string.Empty).Trim();
                        break;
                    }
                    entry.Hostnames.Add(parts[i]);
                }
                Entries.Add(entry);
            }
            Logger.Debug($"Loaded {Entries.Count} entries from hosts file ({Entries.Where(e => !e.Enabled).Count()} disabled, {Entries.Where(e => e.Empty).Count()} empty)");
            return Entries;
        }
    }

    internal class HostsEntry {
        public bool Enabled { get; set; } = true;
        public bool Empty { get; set; } = false;
        public IPAddress? Ip { get; set; }
        public List<string> Hostnames { get; set; } = new();
        public string? Comment { get; set; }

        public override string ToString() {
            var line = "";
            if (!Enabled) line += "# ";
            if (Empty) { return string.Empty; }
            if (Ip is null && !string.IsNullOrWhiteSpace(Comment)) return $"# {Comment}";
            if (Ip is not null) line += Ip.ToString();
            if (Hostnames.Count > 0) line += "\t" + string.Join(' ', Hostnames);
            if (Comment is not null) line += "\t# " + Comment;
            return line;
        }
    }
}
