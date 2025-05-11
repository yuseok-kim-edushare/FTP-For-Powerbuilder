# FTP Wrapper for PowerBuilder

This library provides a COM-compatible wrapper around the FluentFTP library, designed specifically for use with PowerBuilder applications.

## Thanks to
- [FluentFTP](https://github.com/robinrodricks/FluentFTP)

## Features

- Simple FTP/FTPS operations with intuitive API
- Connection profile management
- Secure credential handling
- Full PowerBuilder integration through COM

## Installation

1. Copy the `FluentFtpWrapper.dll` and `connections.xml` files to your application directory
2. Register the COM library using: (If you use Modern powerbuilder, you can call directly without com, check about .net dll importer in PB IDE)
   ```
   regasm FluentFtpWrapper.dll /tlb:FluentFtpWrapper.tlb /codebase
   ```
3. if you want use your custom key pair file, use sn tool that from .net framework sdk
    ```
    C:\Program Files (x86)\Microsoft SDKs\Windows\v10.0A\bin\NETFX 4.8.1 Tools\x64\sn.exe -k (keyfile.snk)
    ```

## Configuration

The wrapper supports two configuration files:
- Default configuration in the DLL directory (`connections.xml`)
- User-specific configuration in %AppData%\FtpClientWrapper\connections.xml

When both are present, user-specific profiles override the default ones with the same name.

### Sample Configuration File

```xml
<?xml version="1.0" encoding="utf-8"?>
<FtpConnections>
  <Profile name="Development">
    <Host>ftp.dev-server.com</Host>
    <Username>dev-user</Username>
    <Password>dev-password</Password>
    <UseSsl>true</UseSsl>
  </Profile>
  <Profile name="Production">
    <Host>ftp.production-server.com</Host>
    <Username>prod-user</Username>
    <Password>prod-password</Password>
    <UseSsl>true</UseSsl>
  </Profile>
</FtpConnections>
```

## Usage from PowerBuilder

### Initialize and Connect

```
FtpClientWrapper ftpClient
ftpClient = CREATE FtpClientWrapper

// Connect using a saved profile
string result = ftpClient.connectWithProfile("Development")

// Or connect directly
string result = ftpClient.connect("ftp.example.com", "username", "password", true)
```

### Upload and Download Files

```
// Using current connection
string result = ftpClient.uploadFile("C:\local\file.txt", "/remote/file.txt")
string result = ftpClient.downloadFile("/remote/file.txt", "C:\local\file.txt")

// Using a specific profile without changing the current connection
string result = ftpClient.uploadFileWithProfile("C:\local\file.txt", "/remote/file.txt", "Production")
string result = ftpClient.downloadFileWithProfile("/remote/file.txt", "C:\local\file.txt", "Production")

// With explicit connection details
string result = ftpClient.uploadFile("C:\local\file.txt", "/remote/file.txt", "ftp.example.com", "username", "password", true)
```

### Manage Profiles

```
// List available profiles
string profiles[]
string result = ftpClient.listProfiles(profiles)

// Save a profile
string result = ftpClient.saveProfile("MyServer", "ftp.myserver.com", "username", "password", true)

// Delete a profile
string result = ftpClient.deleteProfile("MyServer")
```

### Important Notes

1. Each FTP operation creates a new connection for reliability with PowerBuilder
2. Always check the return value for "SUCCESS:" or "ERROR:" prefixes
3. Remember to call `disconnect()` after you're done with a session

## License

MIT based open source project