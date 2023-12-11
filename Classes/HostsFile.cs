using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace scvr_patcher {
    internal class HostsFile {
        private const string EACHostName = "modules-cdn.eac-prod.on.epicgames.com";
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private static readonly Regex EntryRegex = new Regex(@"^([\w.]+)\s+(.*)$");
        private static readonly Regex WhiteSpaceRegex = new Regex(@"\s+");
        private static readonly IPAddress Null = new IPAddress(new byte[] { 0, 0, 0, 0 });
        public static FileInfo HostFile = new FileInfo(Environment.GetEnvironmentVariable("windir") + @"\System32\drivers\etc\hosts");

        public List<HostsEntry> Entries = new();

        public HostsFile() {
            Load(HostFile);
        }

        public void Save(FileInfo file = null, bool backup = true) {
            file ??= HostFile;
            Logger.Info($"Saving hosts file to {file.FullName}");
            if (file.Exists && backup) {
                var backupFile = new FileInfo(file.FullName + ".bak");
                Logger.Info($"Hosts file already exists, creating backup at {backupFile.FullName}");
                if (backupFile.Exists) backupFile.Delete();
                file.MoveTo(backupFile.FullName);
            }
            var lines = new List<string>();
            foreach (var entry in Entries) {
                var line = "";
                if (entry.Ip is not null) line += entry.Ip.ToString();
                if (entry.Hostnames.Count > 0) line += "\t" + string.Join(' ', entry.Hostnames);
                if (entry.Comment is not null) line += "\t# " + entry.Comment;
                if (line.Length > 0) lines.Add(line);
            }
            File.WriteAllLines(file.FullName, lines);
        }

        public List<HostsEntry> Load(FileInfo file = null) {
            file ??= HostFile;
            if (!file.Exists) {
                Logger.Error($"Hosts file not found: {file.FullName}");
                return null;
            }
            Logger.Info($"Loading hosts file from {file.FullName}");
            var lines = File.ReadAllLines(file.FullName);
            Logger.Info($"Loaded hosts file with {lines.Length} lines.");
            Entries.Clear();
            foreach (var _line in lines) {
                var line = _line.Trim();
                var entry = new HostsEntry();
                if (line.StartsWith("#")) {
                    Entries.Add(new HostsEntry { Comment = line }); continue;
                }
                var parts = WhiteSpaceRegex.Split(line); // line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 2) {
                    Logger.Debug($"Not a valid entry: {line}");
                    continue;
                }
                var success = IPAddress.TryParse(parts[0], out var ip);
                if (!success) {
                    Logger.Debug($"Not a valid IP: {parts[0]}");
                    continue;
                }
                entry.Ip = ip;
                for (var i = 1; i < parts.Length; i++) {
                    if (parts[i].StartsWith("#")) {
                        entry.Comment = string.Join(' ', parts.Skip(i));
                        break;
                    }
                    entry.Hostnames.Add(parts[i]);
                }
                Entries.Add(entry);
            }
            return Entries;
        }
    }

    internal class HostsEntry {
        public bool Enabled { get; set; } = true;
        public IPAddress Ip { get; set; }
        public List<string> Hostnames { get; set; } = new();
        public string? Comment { get; set; }
    }
}
