# PortNumberSync

PortNumberSync automatically keeps your qBittorrent listening port in sync with the port forwarded by [Private Internet Access (PIA)](https://www.privateinternetaccess.com/) VPN.

When PIA changes your forwarded port, this tool detects the change and updates qBittorrent's settings accordingly, so your torrents always use the correct forwarded port for optimal download speeds.

## How It Works

The solution consists of two applications that communicate via a **named pipe**:

### 1. PortNumberSync (Windows Service)

Runs as a background Windows service that:

- Monitors PIA's port forwarding using `piactl.exe monitor portforward`
- Parses the forwarded port number from the output
- Sends the port to any connected client via a named pipe (`piaPortMonitorServicePipe`)

### 2. qBittorrentDaemon (Tray Application)

Runs as a Windows Forms tray application that:

- Connects to the PortNumberSync service via the named pipe
- Listens for port change notifications
- When a new port is received:
  - Optionally restarts qBittorrent (to handle network reconnection issues)
  - Logs into qBittorrent's Web API
  - Updates the listening port setting
  - Shows a system tray notification with the new port

## Prerequisites

- **.NET 10.0 SDK** — required to build the project
- **Windows** — the project uses Windows-specific features (Windows Services, Named Pipes, Windows Forms)
- **Private Internet Access (PIA) VPN** — installed at the default location (`C:\Program Files\Private Internet Access\piactl.exe`)
- **qBittorrent** — installed with Web UI enabled (default port `8080`)

## Setup

### Building

Open `PortNumberSync.sln` in Visual Studio or JetBrains Rider and build the solution, or build from the command line:

```powershell
dotnet build PortNumberSync.sln
```

### Configuring qBittorrent

Update the settings in `qBittorentDaemon\appsettings.json`:

```json
{
  "QBittorrent": {
    "Path": "C:\\Program Files\\qBittorrent\\qbittorrent.exe",
    "RestartBeforePortUpdate": true,
    "Username": "admin",
    "Password": "your-password"
  }
}
```

- `Path` — full path to the qBittorrent executable (update if not installed at the default location)
- `RestartBeforePortUpdate` — whether to restart qBittorrent before updating the port (useful when the network drops for extended periods)
- `Username` — the username for qBittorrent's Web UI authentication
- `Password` — the password for qBittorrent's Web UI authentication

> **Note:** Make sure to enable the Web UI in qBittorrent (Tools → Options → Web UI) and set the `Username` and `Password` fields to match your Web UI credentials.

## Usage

### Installing the PortNumberSync Service

Run the PortNumberSync executable with the `-i` flag to register it as a Windows service:

```powershell
.\PortNumberSync.exe -i
```

This creates an auto-start Windows service named `PortNumberSync`.

### Starting the Service

After installation, start the service:

```powershell
Start-Service PortNumberSync
```

The service will automatically start on boot since it is configured as `start=auto`.

### Running qBittorrentDaemon

Run the qBittorrentDaemon application directly:

```powershell
.\qBittorentDaemon.exe
```

The application runs in the system tray with a qBittorrent icon. Right-click the icon to exit.

### Uninstalling the Service

To remove the Windows service:

```powershell
.\PortNumberSync.exe -u
```

## Project Structure

```
PortNumberSync/
├── PipeCommunication/          # Shared library for named pipe communication
│   ├── PipeServer.cs           # Named pipe server implementation
│   ├── PipeClient.cs           # Named pipe client implementation
│   └── ServiceCollectionExtensions.cs  # DI registration helpers
├── PortNumberSync/             # Windows service — monitors PIA port forwarding
│   ├── PiaService.cs           # Runs piactl.exe and parses port output
│   └── Worker.cs               # Background service — sends ports to pipe clients
└── qBittorentDaemon/           # Tray app — receives ports and updates qBittorrent
    ├── QBittorrentApiClient.cs # qBittorrent Web API client
    ├── QBittorrentService.cs   # Process management for qBittorrent
    ├── NewPortNotificationService.cs  # Event notification service
    ├── MyCustomApplicationContext.cs  # System tray icon and notifications
    └── Worker.cs               # Background service — listens for port changes
```

## Logging

Both applications log to text files in their working directory:

- **PortNumberSync** → `portmonitor-log.txt`
- **qBittorrentDaemon** → `daemon-log.txt`

Logs are written using Serilog and include port detection events, pipe communication, and qBittorrent API interactions.

## Architecture Diagram

```
┌─────────────────────┐         Named Pipe          ┌──────────────────────┐
│  PortNumberSync      │  ──────────────────────►    │  qBittorrentDaemon   │
│  (Windows Service)   │   piaPortMonitorServicePipe  │  (Tray Application)  │
│                      │                              │                      │
│  piactl.exe          │                              │  qBittorrent Web UI  │
│  monitor portforward │                              │  API (localhost:8080)│
└─────────────────────┘                              └──────────────────────┘
```

## Troubleshooting

- **Service won't start:** Ensure PIA VPN is installed and `piactl.exe` exists at `C:\Program Files\Private Internet Access\piactl.exe`.
- **qBittorrent connection fails:** Verify the Web UI is enabled and the credentials in `QBittorrentApiClient.cs` match your qBittorrent settings.
- **Port not updating:** Check `daemon-log.txt` for errors. Ensure qBittorrent is running and the Web UI is accessible on port `8080`.
