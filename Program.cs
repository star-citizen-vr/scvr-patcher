using System;
using System.IO;
using System.Text.Json;

namespace scvr_patcher {
    public class Program {
        public static void Main() {
            // Call the methods as needed
        }

        public static bool IsHostsUpdated() {
            try {
                string hostsPath = @"C:\Windows\System32\drivers\etc\hosts";
                string hostsContent = "\n#Star Citizen EAC workaround\n127.0.0.1        modules-cdn.eac-prod.on.epicgames.com";

                string existingContent = File.ReadAllText(hostsPath);

                // Check if the lines are already present
                return !existingContent.Contains(hostsContent);
            } catch (FileNotFoundException) {
                return false;  // Indicate failure: File not found
            } catch (Exception) {
                return false;  // Indicate failure: An error occurred
            }
        }

        public static string UpdateSettings(string directory, Settings newValues) {
            try {
                string filePath = Path.Combine(directory, "settings.json");

                // Check if the directory exists
                if (!Directory.Exists(directory)) {
                    return $"Directory not found: {directory}";
                }

                // Load the JSON file
                var settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText(filePath));

                // Update the specific lines
                settings.ProductId = newValues.ProductId;
                settings.SandboxId = newValues.SandboxId;
                settings.ClientId = newValues.ClientId;
                settings.DeploymentId = newValues.DeploymentId;

                // Save the updated JSON back to the file
                File.WriteAllText(filePath, JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true }));

                return $"Settings updated successfully in {filePath}";
            } catch (FileNotFoundException ex) {
                return $"File not found: {ex.Message}";
            }
        }
    }

    public class Settings {
        public string ProductId { get; set; }
        public string SandboxId { get; set; }
        public string ClientId { get; set; }
        public string DeploymentId { get; set; }
    }
}
