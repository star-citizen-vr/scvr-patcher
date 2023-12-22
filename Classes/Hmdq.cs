using NLog;
using Octokit;
using SCVRPatcher;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;

public class Hmdq {
    private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
    private const string JsonName = "hmdq_data.json";
    private static readonly List<string> GithubRepo = new() { "risa2000", "hmdq" };
    private static readonly List<string> Arguments = new() { "-n", "--out_json", JsonName
    };
    private FileInfo Path { get; set; }
    public HmdqOutput Data { get; private set; }
    public virtual bool IsEmpty => Data.Openvr.Error is not null && Data.Oculus.Error is not null;
    public Property Hmd => Data.Openvr.Properties.First(p => p.Value.PropRenderModelNameString == "generic_hmd").Value;

    public void Initialize() {
        Logger.Debug("Initializing HMDQ");
        try {
            GetHmdqFromGithubRelease();
        } catch (Exception e) {
            Logger.Error($"Failed to get HMDQ from Github release: {e}");
            Logger.Warn("Falling back to embedded HMDQ");
            GetEmbeddedHmdq();
        }
    }

    public void GetHmdqFromGithubRelease() {
        Logger.Debug($"Getting HMDQ from Github release: {GithubRepo[0]}/{GithubRepo[1]}");
        var client = new GitHubClient(new ProductHeaderValue("SCVRPatcher"));
        var release = client.Repository.Release.GetLatest(GithubRepo[0], GithubRepo[1]).Result;
        Logger.Debug($"Found latest release {release.TagName}");
        var asset = release.Assets.First(a => a.Name.EndsWith("-win64.zip"));
        var downloadUrl = asset.BrowserDownloadUrl;
        Logger.Debug($"Downloading HMDQ from {downloadUrl}");
        var tempPath = Utils.GetTempFile();
        using (var client2 = new WebClient()) {
            client2.DownloadFile(downloadUrl, tempPath.FullName);
        }
        Logger.Debug($"Downloaded HMDQ to {tempPath}");
        var zipDirFile = Utils.GetTempFile();
        var zipDir = zipDirFile.Directory.Combine(zipDirFile.FileNameWithoutExtension());
        Logger.Debug($"Extracting HMDQ to {zipDir}");
        ZipFile.ExtractToDirectory(tempPath.FullName, zipDir.FullName);
        Logger.Debug($"Extracted HMDQ to {zipDir}");
        var zipFile = zipDir.GetFiles().First(f => f.Name == "hmdq.exe");
        Logger.Debug($"Found HMDQ executable {zipFile.FullName}");
        Path = new FileInfo(zipFile.FullName);
        Logger.Debug($"Wrote HMDQ to {Path.FullName}");
    }

    public void GetEmbeddedHmdq() {
        Logger.Debug("Getting embedded hmdq");
        var assembly = Assembly.GetExecutingAssembly();
        using (var stream = assembly.GetManifestResourceStream("SCVRPatcher.Embedded.hmdq.exe")) {
            Path = new FileInfo(System.IO.Path.GetTempFileName());
            using (var fileStream = Path.Create()) {
                stream.CopyTo(fileStream);
                Logger.Debug($"Wrote embedded hmdq to {Path.FullName}");
            }
        }
    }

    public void RunHmdq() {
        var startInfo = new ProcessStartInfo {
            FileName = Path.FullName,
            Arguments = string.Join(" ", Arguments),
            UseShellExecute = false,
            RedirectStandardOutput = true,
            CreateNoWindow = true
        };
        Logger.Debug($"Running {startInfo.FileName.Quote()} {startInfo.Arguments}");
        using (var process = Process.Start(startInfo)) {
            process.WaitForExit();
            Logger.Info($"HMDQ exited with code {process.ExitCode}");
            if (process.ExitCode != 0) {
                throw new Exception($"HMDQ exited with code {process.ExitCode}");
            } else ReadHmdqData();
        }
    }


    public void ReadHmdqData(string? fileName = null) {
        fileName ??= JsonName;
        Logger.Debug($"Reading HMDQ data from {fileName}");
        var json = File.ReadAllText(fileName);
        Data = HmdqOutput.FromJson(json);
        Logger.Debug($"Read HMDQ data from {fileName}");
    }

}
