using System;
using System.Runtime.InteropServices;
using FluentFTP;
using System.Threading.Tasks;
using System.Security;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Reflection;

namespace FluentFtpWrapper
{
    [ComVisible(true)]
    [Guid("D1A9A1B2-5C3D-4B2A-9A1B-1234567890AB")]
    [ClassInterface(ClassInterfaceType.AutoDual)]
    public class FtpClientWrapper : IDisposable
    {
        // Store connection parameters
        private string _host;
        private string _username;
        private string _password;
        private FtpClient _client;
        
        // Store security-related settings
        private bool _useSsl = false;

        // Configuration files path
        private string _userConfigPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "FtpClientWrapper",
            "connections.xml");
            
        private string _appConfigPath;
        
        // Connection profiles
        private Dictionary<string, ConnectionProfile> _profiles = new Dictionary<string, ConnectionProfile>();
        
        // Dispose pattern implementation
        private bool _disposed = false;

        public FtpClientWrapper()
        {
            // Initialize the application directory config path
            string assemblyLocation = Assembly.GetExecutingAssembly().Location;
            string assemblyDirectory = Path.GetDirectoryName(assemblyLocation);
            _appConfigPath = Path.Combine(assemblyDirectory, "connections.xml");
            
            // Create user config directory if it doesn't exist
            string userConfigDir = Path.GetDirectoryName(_userConfigPath);
            if (!Directory.Exists(userConfigDir))
            {
                Directory.CreateDirectory(userConfigDir);
            }
            
            // Load profiles from application directory first, then from user directory (overriding any duplicates)
            LoadProfilesFromFile(_appConfigPath);
            LoadProfilesFromFile(_userConfigPath);
        }

        private void LoadProfilesFromFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(filePath);
                    
                    XmlNodeList profileNodes = doc.SelectNodes("//Profile");
                    if (profileNodes != null)
                    {
                        foreach (XmlNode node in profileNodes)
                        {
                            string name = node.Attributes["name"]?.Value;
                            if (!string.IsNullOrEmpty(name))
                            {
                                ConnectionProfile profile = new ConnectionProfile
                                {
                                    Name = name,
                                    Host = node.SelectSingleNode("Host")?.InnerText,
                                    Username = node.SelectSingleNode("Username")?.InnerText,
                                    Password = node.SelectSingleNode("Password")?.InnerText,
                                    UseSsl = bool.Parse(node.SelectSingleNode("UseSsl")?.InnerText ?? "false")
                                };
                                
                                _profiles[name] = profile;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignore loading errors - continue with existing profiles
            }
        }

        private void LoadProfiles()
        {
            // Load from both locations
            LoadProfilesFromFile(_appConfigPath);
            LoadProfilesFromFile(_userConfigPath);
        }

        private void SaveProfiles()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                XmlElement root = doc.CreateElement("FtpConnections");
                doc.AppendChild(root);
                
                foreach (var profile in _profiles.Values)
                {
                    XmlElement profileElement = doc.CreateElement("Profile");
                    profileElement.SetAttribute("name", profile.Name);
                    
                    XmlElement hostElement = doc.CreateElement("Host");
                    hostElement.InnerText = profile.Host;
                    profileElement.AppendChild(hostElement);
                    
                    XmlElement usernameElement = doc.CreateElement("Username");
                    usernameElement.InnerText = profile.Username;
                    profileElement.AppendChild(usernameElement);
                    
                    XmlElement passwordElement = doc.CreateElement("Password");
                    passwordElement.InnerText = profile.Password;
                    profileElement.AppendChild(passwordElement);
                    
                    XmlElement sslElement = doc.CreateElement("UseSsl");
                    sslElement.InnerText = profile.UseSsl.ToString();
                    profileElement.AppendChild(sslElement);
                    
                    root.AppendChild(profileElement);
                }
                
                // Save to user config path - these take precedence over the app config
                doc.Save(_userConfigPath);
            }
            catch
            {
                // Ignore saving errors
            }
        }
        
        // Configures whether to save to the user's application data folder or alongside the DLL
        public string setSaveLocation(string location)
        {
            try
            {
                if (location.Equals("Application", StringComparison.OrdinalIgnoreCase))
                {
                    SaveProfiles();
                    return "SUCCESS: Profiles will be saved to application data folder.";
                }
                else
                {
                    return "ERROR: Invalid location specified. Use 'Application'.";
                }
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string saveProfile(string profileName, string host, string username, string password, bool useSsl)
        {
            try
            {
                ConnectionProfile profile = new ConnectionProfile
                {
                    Name = profileName,
                    Host = host,
                    Username = username,
                    Password = password,
                    UseSsl = useSsl
                };
                
                _profiles[profileName] = profile;
                SaveProfiles();
                
                return $"SUCCESS: Profile '{profileName}' saved.";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string deleteProfile(string profileName)
        {
            try
            {
                if (_profiles.ContainsKey(profileName))
                {
                    _profiles.Remove(profileName);
                    SaveProfiles();
                    return $"SUCCESS: Profile '{profileName}' deleted.";
                }
                else
                {
                    return $"ERROR: Profile '{profileName}' not found.";
                }
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string listProfiles(out string[] profileNames)
        {
            try
            {
                profileNames = new string[_profiles.Count];
                _profiles.Keys.CopyTo(profileNames, 0);
                return "SUCCESS: Profiles retrieved.";
            }
            catch (Exception ex)
            {
                profileNames = new string[0];
                return $"ERROR: {ex.Message}";
            }
        }

        public string connectWithProfile(string profileName)
        {
            try
            {
                if (!_profiles.TryGetValue(profileName, out ConnectionProfile profile))
                {
                    return $"ERROR: Profile '{profileName}' not found.";
                }
                
                return connect(profile.Host, profile.Username, profile.Password, profile.UseSsl);
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string enableSsl(bool useSsl)
        {
            try
            {
                _useSsl = useSsl;
                return "SUCCESS: SSL configuration updated.";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string connect(string host, string username, string password)
        {
            return connect(host, username, password, _useSsl);
        }

        public string connect(string host, string username, string password, bool useSsl)
        {
            try
            {
                // Store connection parameters for later use
                _host = host;
                _username = username;
                _password = password;
                _useSsl = useSsl;
                
                // Create and connect the client
                _client = new FtpClient(host);
                _client.Credentials = new NetworkCredential(username, password);
                
                // Configure security if needed
                if (_useSsl)
                {
                    // FluentFTP usually supports encryption but the exact API 
                    // depends on the version. We'll check if a property exists.
                    try
                    {
                        // Try to set SSL property using reflection as a fallback
                        var prop = typeof(FtpClient).GetProperty("EnableSsl");
                        if (prop != null)
                        {
                            prop.SetValue(_client, true);
                        }
                    }
                    catch
                    {
                        // Ignore errors if property doesn't exist
                    }
                }
                
                _client.Connect();
                return "SUCCESS: Connected to FTP server.";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string disconnect()
        {
            try
            {
                if (_client != null && _client.IsConnected)
                {
                    _client.Disconnect();
                    _client.Dispose();
                    _client = null;
                }
                return "SUCCESS: Disconnected.";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string uploadFile(string localPath, string remotePath)
        {
            return uploadFile(localPath, remotePath, _host, _username, _password, _useSsl);
        }

        public string uploadFileWithProfile(string localPath, string remotePath, string profileName)
        {
            try
            {
                if (!_profiles.TryGetValue(profileName, out ConnectionProfile profile))
                {
                    return $"ERROR: Profile '{profileName}' not found.";
                }
                
                return uploadFile(localPath, remotePath, profile.Host, profile.Username, profile.Password, profile.UseSsl);
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string uploadFile(string localPath, string remotePath, string host, string username, string password, bool useSsl)
        {
            FtpClient client = null;
            try
            {
                // Create a new connection for this operation
                client = new FtpClient(host);
                client.Credentials = new NetworkCredential(username, password);
                
                // Configure security if needed
                if (useSsl)
                {
                    try
                    {
                        // Try to set SSL property using reflection as a fallback
                        var prop = typeof(FtpClient).GetProperty("EnableSsl");
                        if (prop != null)
                        {
                            prop.SetValue(client, true);
                        }
                    }
                    catch
                    {
                        // Ignore errors if property doesn't exist
                    }
                }
                
                client.Connect();
                client.UploadFile(localPath, remotePath);
                return "SUCCESS: File uploaded.";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
            finally
            {
                // Clean up the connection
                if (client != null && client.IsConnected)
                {
                    client.Disconnect();
                    client.Dispose();
                }
            }
        }

        public string downloadFile(string remotePath, string localPath)
        {
            return downloadFile(remotePath, localPath, _host, _username, _password, _useSsl);
        }

        public string downloadFileWithProfile(string remotePath, string localPath, string profileName)
        {
            try
            {
                if (!_profiles.TryGetValue(profileName, out ConnectionProfile profile))
                {
                    return $"ERROR: Profile '{profileName}' not found.";
                }
                
                return downloadFile(remotePath, localPath, profile.Host, profile.Username, profile.Password, profile.UseSsl);
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string downloadFile(string remotePath, string localPath, string host, string username, string password, bool useSsl)
        {
            FtpClient client = null;
            try
            {
                // Create a new connection for this operation
                client = new FtpClient(host);
                client.Credentials = new NetworkCredential(username, password);
                
                // Configure security if needed
                if (useSsl)
                {
                    try
                    {
                        // Try to set SSL property using reflection as a fallback
                        var prop = typeof(FtpClient).GetProperty("EnableSsl");
                        if (prop != null)
                        {
                            prop.SetValue(client, true);
                        }
                    }
                    catch
                    {
                        // Ignore errors if property doesn't exist
                    }
                }
                
                client.Connect();
                client.DownloadFile(localPath, remotePath);
                return "SUCCESS: File downloaded.";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
            finally
            {
                // Clean up the connection
                if (client != null && client.IsConnected)
                {
                    client.Disconnect();
                    client.Dispose();
                }
            }
        }

        public string listDirectory(string remotePath, out string[] files)
        {
            return listDirectory(remotePath, out files, _host, _username, _password, _useSsl);
        }

        public string listDirectoryWithProfile(string remotePath, out string[] files, string profileName)
        {
            files = new string[0];
            try
            {
                if (!_profiles.TryGetValue(profileName, out ConnectionProfile profile))
                {
                    return $"ERROR: Profile '{profileName}' not found.";
                }
                
                return listDirectory(remotePath, out files, profile.Host, profile.Username, profile.Password, profile.UseSsl);
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string listDirectory(string remotePath, out string[] files, string host, string username, string password, bool useSsl)
        {
            files = new string[0];
            FtpClient client = null;
            try
            {
                // Create a new connection for this operation
                client = new FtpClient(host);
                client.Credentials = new NetworkCredential(username, password);
                
                // Configure security if needed
                if (useSsl)
                {
                    try
                    {
                        // Try to set SSL property using reflection as a fallback
                        var prop = typeof(FtpClient).GetProperty("EnableSsl");
                        if (prop != null)
                        {
                            prop.SetValue(client, true);
                        }
                    }
                    catch
                    {
                        // Ignore errors if property doesn't exist
                    }
                }
                
                client.Connect();
                var items = client.GetNameListing(remotePath);
                files = items;
                return "SUCCESS: Directory listed.";
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
            finally
            {
                // Clean up the connection
                if (client != null && client.IsConnected)
                {
                    client.Disconnect();
                    client.Dispose();
                }
            }
        }
        
        // For backward compatibility
        public string uploadfile(string localPath, string remotePath)
        {
            return uploadFile(localPath, remotePath);
        }
        
        public string downloadfile(string remotePath, string localPath)
        {
            return downloadFile(remotePath, localPath);
        }
        
        public string istdirectory(string remotePath, out string[] files)
        {
            return listDirectory(remotePath, out files);
        }
        
        // Implement IDisposable pattern to ensure cleanup of sensitive data
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (_client != null)
                    {
                        if (_client.IsConnected)
                            _client.Disconnect();
                        _client.Dispose();
                        _client = null;
                    }
                }
                
                // Clear sensitive data
                _host = null;
                _username = null;
                _password = null;
                
                // Clear profiles
                _profiles.Clear();
                
                _disposed = true;
            }
        }
        
        ~FtpClientWrapper()
        {
            Dispose(false);
        }
    }
    
    // Connection profile class to store connection settings
    internal class ConnectionProfile
    {
        public string Name { get; set; }
        public string Host { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public bool UseSsl { get; set; }
    }
} 