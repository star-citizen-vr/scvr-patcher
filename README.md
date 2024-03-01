# Star Citizen VR Patcher v3 [![Stars](https://img.shields.io/github/stars/star-citizen-vr/scvr-patcher?style=flat-square)](https://github.com/star-citizen-vr/scvr-patcher/stargazers) [![Forks](https://img.shields.io/github/forks/star-citizen-vr/scvr-patcher?style=flat-square)](https://github.com/star-citizen-vr/scvr-patcher/network) [![Latest Release](https://img.shields.io/github/v/release/star-citizen-vr/scvr-patcher?style=flat-square)](https://github.com/star-citizen-vr/scvr-patcher/releases) ![Total Downloads)](https://img.shields.io/github/downloads/star-citizen-vr/scvr-patcher/total)

## Description

SCVR Patcher is a tool designed to help players get Star Citizen up and running in VR using VorpX

## Features

- Patching EAC's game config, hosts entry and AppData Dir
- Automatically detecting HMD and it's values through [hmdq](https://github.com/risa2000/hmdq)
- Patching game attribute files for VR in each of your installed SC builds
- Single Automatic backup of all modified files (<original_filename.bak>)
- Automatically filling your VorpX Exclusion list with exclusions required for SCVR

<details>
<summary>Screenshot(s)</summary>

![scvr-patcher_evkOLRFJlQ](https://github.com/star-citizen-vr/scvr-patcher/assets/3318223/ea984694-d31a-4355-8304-403f11dab38e)

![scvr-patcher_XtEKPMQvbn](https://github.com/star-citizen-vr/scvr-patcher/assets/3318223/add218e7-8717-4fcf-9c4a-1cbd02c6cf87)


</details>

## Setup Instructions

### Prerequisites
- [Star Citizen](https://robertsspaceindustries.com/star-citizen/)
- [VorpX](https://www.vorpx.com/) (Paid)
- [.NET 8.0.0 Desktop Runtime x86](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.0-windows-x86-installer)

1. Download the latest release from the [releases page](https://github.com/star-citizen-vr/scvr-patcher/releases).
2. Extract the contents of the ZIP file to a directory of your choice.
3. Run the `SCVR-Patcher.exe` file to launch the application.
4. Follow the on-screen instructions to configure the tool.

## Usage Instructions

1. Launch the SCVR Patcher application.
2. Select the patch you wish to apply from the treeview.
3. Click the "Enable VR" button.
4. Wait for the patch to be applied.
5. Start your Star Citizen

## Command Line Arguments

| Argument | Alias | Description |
| --- | --- | --- |
| `--config <url/file>` |  | Allows you to manually specify a config database from a URL or File Path. |
| `--no-admin` |  | Forces no UAC Elevation prompt. |
| `--no-update` |  | Skips the auto updater check. |

## Contributing

Contributions are welcome! Please read the [contributing guidelines](CONTRIBUTING.md) before submitting any pull requests.

## Credits

- SCVR Patcher is developed and maintained by the Star Citizen VR community.
- Special thanks to the contributors who have helped make this tool possible.
