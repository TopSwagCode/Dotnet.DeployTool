using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Dotnet.DeployTool.Core
{

    public class DeployServiceConfiguration // Rename to SSH / SCP info?
    {

        public PrivateKeyFile PemKey { get; set; }
        public string Ip { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
    }

    public class DeployService : IDeployService
    {
        private readonly ILogger<DeployService> _logger;
        private readonly IFeedbackChannel _feedbackChannel;
        private readonly DeployServiceConfiguration _config;
        private readonly IScriptManager _scriptManager;

        private SshClient _sshClient => new SshClient(_config.Ip, _config.Port, _config.Username, _config.PemKey);
        private ScpClient _scpClient => new ScpClient(_config.Ip, _config.Port, _config.Username, _config.PemKey);

        public DeployService(ILogger<DeployService> logger, IFeedbackChannel feedbackChannel, IScriptManager scriptManager)
        {
            _logger = logger;
            _feedbackChannel = feedbackChannel;
            _config = new DeployServiceConfiguration();
            _scriptManager = scriptManager;
        }

        public async Task UpdateConfigAndTestConnection(string pemFilePath, string ip, int port, string username)
        {
            try
            {
                _config.PemKey = new PrivateKeyFile(pemFilePath);
                _config.Ip = ip;
                _config.Port = port;
                _config.Username = username;

                await _feedbackChannel.SendFeedback($" - - - - Updating config and checking connection - - - -");

                bool hasError = false;

                var nodeExists = CheckIfNodeExistsWithOpenPort(ip, port);

                if (nodeExists)
                {
                    await _feedbackChannel.SendFeedback($"Node with Ip: {ip} and Port: {port} found.");
                }
                else
                {
                    await _feedbackChannel.SendFeedback($"Node with Ip: {ip} and Port: {port} not found.");
                    hasError = true;
                }

                if (File.Exists(pemFilePath))
                {
                    await _feedbackChannel.SendFeedback($"PemFile found");
                }
                else
                {
                    await _feedbackChannel.SendFeedback($"PemFile not found");
                    hasError = true;
                }

                if (hasError)
                {
                    await _feedbackChannel.SendFeedback($" - - - - Failed to make connection - - - -");
                    return;
                }

                try
                {
                    using (var client = _scpClient)
                    {
                        client.Connect();

                        if (client.IsConnected)
                        {
                            await _feedbackChannel.SendFeedback($" - - - - Connection Success - - - -");
                        }
                        else
                        {
                            await _feedbackChannel.SendFeedback($" - - - - Failed to make connection - - - -");
                        }
                    }
                }
                catch (Exception e)
                {
                    await _feedbackChannel.SendFeedback($" - - - - Failed to make connection with Exception: {e.Message} - - - -");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            

        }

        private bool CheckIfNodeExistsWithOpenPort(string ip, int port)
        {
            var client = new TcpClient();
            var result = client.BeginConnect(ip, port, null, null);

            var success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(1));

            if (!success)
            {
                return false;
            }

            client.EndConnect(result);
            return true;
        }

        public async Task UploadSolution(string pemFilePath, string ip, int port, string username, string projectName)
        {
            
            var releaseFolderPath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"/dotnetdeploy/{projectName}";

            try
            {
                await _feedbackChannel.SendFeedback($" - - - - Uploading project {projectName} from folder {releaseFolderPath} - - - -");

                _config.PemKey = new PrivateKeyFile(pemFilePath);
                _config.Ip = ip;
                _config.Port = port;
                _config.Username = username;

                using (var client = _sshClient)
                {
                    client.Connect();

                    await client.RunCommandAsync("mkdir dotnetdeploy");
                    await client.RunCommandAsync($"cd dotnetdeploy && mkdir {projectName}");
                }

                using (var client = _scpClient)
                {

                    client.RemotePathTransformation = RemotePathTransformation.ShellQuote;
                    client.Connect();

                    client.Uploading += async delegate (object sender, ScpUploadEventArgs e)
                    {
                        if (e.Size != 0)
                        {
                            await _feedbackChannel.SendFeedback($"Uploading {e.Filename} {e.Uploaded} Bytes Uploaded of {e.Size}, which is: {Convert.ToInt32((e.Uploaded * 100) / e.Size)}%");
                        }
                    };

                    var directoryInfo = new DirectoryInfo(releaseFolderPath);

                    if (directoryInfo.Exists)
                    {
                        client.Upload(directoryInfo, $"/home/{username}/dotnetdeploy/{projectName}/");
                    }
                    else
                    {
                        await _feedbackChannel.SendFeedback($"- - - - Failed uploading folder: {releaseFolderPath}, with error: Directory not found - - - - ");

                        return;
                    }
                }

                await _feedbackChannel.SendFeedback($"- - - - Finished uploading folder: {releaseFolderPath} - - - - ");
            }
            catch (Exception e)
            {
                await _feedbackChannel.SendFeedback($"- - - - Failed uploading folder: {releaseFolderPath}, with error: {e.Message} - - - - ");
                _logger.LogError(e.Message);
            }
        }

        public async Task InstallAppRuntimeAsync(string pemFilePath, string ip, int port, string username, OsVersion osVersion, AppRuntimeVersion appRuntimeVersion)
        {
            try
            {
                await _feedbackChannel.SendFeedback($"- - - - Installing AppRuntimeVersion: {appRuntimeVersion} on Os: {osVersion} - - - -");

                _config.PemKey = new PrivateKeyFile(pemFilePath);
                _config.Ip = ip;
                _config.Port = port;
                _config.Username = username;

                using (var client = _sshClient)
                {
                    client.Connect();

                    if (_scriptManager.GetScript(osVersion, appRuntimeVersion, out List<string> scriptLines))
                    {
                        foreach (var scriptLine in scriptLines)
                        {
                            await _feedbackChannel.SendFeedback($"$ {scriptLine}");

                            var output = await client.RunCommandAsync(scriptLine);

                            await _feedbackChannel.SendFeedback(output);
                        }

                        await _feedbackChannel.SendFeedback($"- - - - Finished installing AppRuntimeVersion: {appRuntimeVersion} on Os: {osVersion} - - - -");
                    }
                    else
                    {
                        await _feedbackChannel.SendFeedback($"{nameof(osVersion)} combined with {nameof(appRuntimeVersion)} is currently not a supported combination.");
                    }
                }

            }
            catch (Exception e)
            {
                await _feedbackChannel.SendFeedback($"- - - - Failed installing AppRuntimeVersion: {appRuntimeVersion} on Os: {osVersion}, with error: {e.Message} - - - - ");
                _logger.LogWarning(e.Message);
            }
        }


        public Task PublishApp(string pathToCsproj, string appName, AppRuntimeVersion appRuntime)
        {
            _feedbackChannel.SendFeedback($"- - - - Publishing App: {appName} - - - -");

            var isRelease = true; // Hardcoded for now.
            var configuration = isRelease ? "Release" : "Debug";
            var framework = appRuntime;
            var isSelfContained = false;
            var osRuntime = "linux-x64";
            var output = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"/dotnetdeploy/{appName}";
            var script = $"publish {pathToCsproj} --configuration {configuration} --framework {framework.From()} --self-contained {isSelfContained.ToString().ToLower()} --runtime {osRuntime} --output {output}"; // --verbosity quiet

            _feedbackChannel.SendFeedback($"dotnet ${script}");

            var result = script.Cmd();

            _feedbackChannel.SendFeedback(result);

            _feedbackChannel.SendFeedback($"- - - - Publishing completed for App: {appName} - - - -");
            return Task.CompletedTask;
        }

        public async Task SetupService(string pemFilePath, string ip, int port, string username, string projectName, string dllName)
        {
            try
            {
                _config.PemKey = new PrivateKeyFile(pemFilePath);
                _config.Ip = ip;
                _config.Port = port;
                _config.Username = username;

                await _feedbackChannel.SendFeedback($"- - - - Setup App: {projectName} - - - -");

                await _feedbackChannel.SendFeedback("Creating Service description file");

                var serviceFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + $"/dotnetdeploy/{projectName.ToLower()}.service";

                File.Copy("description.service", serviceFilePath, true);

                string text = File.ReadAllText(serviceFilePath);
                text = text.Replace("{WorkingDirectory}", $"/home/{username}/dotnetdeploy/{projectName}");
                text = text.Replace("{WorkingDirectoryDll}", $"/home/{username}/dotnetdeploy/{projectName}/{dllName}");
                File.WriteAllText(serviceFilePath, text);

                await _feedbackChannel.SendFeedback($"- - - - Setup completed for App: {projectName} - - - -");

                await _feedbackChannel.SendFeedback("Uploading service description file");


                using (var client = _scpClient)
                {

                    client.RemotePathTransformation = RemotePathTransformation.ShellQuote;
                    client.Connect();

                    client.Uploading += async delegate (object sender, ScpUploadEventArgs e)
                    {
                        if (e.Size != 0)
                        {
                            await _feedbackChannel.SendFeedback($"Uploading {e.Filename} {e.Uploaded} Bytes Uploaded of {e.Size}, which is: {Convert.ToInt32((e.Uploaded * 100) / e.Size)}%");
                        }
                    };

                    var fileInfo = new FileInfo(serviceFilePath);

                    if (fileInfo.Exists)
                    {
                        client.Upload(fileInfo, $"/home/{username}/dotnetdeploy/{projectName.ToLower()}.service");
                    }
                    else
                    {
                        await _feedbackChannel.SendFeedback($"- - - - Failed uploading file: {serviceFilePath}, with error: File not found - - - - ");
                    }
                }

                using (var client = _sshClient)
                {
                    client.Connect();

                    var scriptLines = new List<string>
                    {
                        $"sudo mv /home/{username}/dotnetdeploy/{projectName.ToLower()}.service /etc/systemd/system/{projectName.ToLower()}.service",
                        $"sudo systemctl enable {projectName.ToLower()}.service",
                        $"sudo systemctl start {projectName.ToLower()}.service",
                        $"sudo iptables -A PREROUTING -t nat -i eth0 -p tcp --dport 80 -j REDIRECT --to-port 5000" // Hardcoded for now. Should be an option for your app.
                    };

                    string totalResults = string.Empty;

                    foreach (var scriptLine in scriptLines)
                    {
                        await _feedbackChannel.SendFeedback($"$ {scriptLine}");

                        var output = await client.RunCommandAsync(scriptLine);

                        await _feedbackChannel.SendFeedback(output);
                    }
                }

                // SSH /etc/systemd/system/{projectName.ToLower()}.service
            }
            catch (Exception e)
            {
                await _feedbackChannel.SendFeedback($"- - - - Failed Setup Service, with error: {e.Message} - - - - ");
                _logger.LogError(e.Message);
            }
            
            /*
             *  TODO:
             *  Setup Service
             *  Start Service
             */
        }

        public async Task RunSample(string pemFilePath, string ip, int port, string username, string projectName, string dllName)
        {
            _config.PemKey = new PrivateKeyFile(pemFilePath);
            _config.Ip = ip;
            _config.Port = port;
            _config.Username = username;

            Console.WriteLine(" - - - - Starting Sample App - - - -");
            try
            {
                using (var client = _sshClient)
                {
                    client.Connect();

                    var scriptLines = new List<string>
                    {
                        $"sudo dotnet /home/{username}/dotnetdeploy/{projectName}/{dllName}"
                    };

                    string totalResults = string.Empty;

                    foreach (var scriptLine in scriptLines)
                    {
                        await _feedbackChannel.SendFeedback($"$ {scriptLine}");

                        var output = await client.RunCommandAsync(scriptLine);

                        await _feedbackChannel.SendFeedback(output);
                    }
                }

            }
            catch (Exception e)
            {
                await _feedbackChannel.SendFeedback($"Failed running app, with error: {e.Message}");
                _logger.LogWarning(e.Message);
            }
        }
    }

    public static class ProcessHelper
    {
        public static string Bash(this string cmd)
        {
            var escapedArgs = cmd.Replace("\"", "\\\"");

            var process = new Process()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{escapedArgs}\"",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            process.WaitForExit();
            return "Done";

        }

        public static string Cmd(this string cmd)
        {
            // dotnet publish C:\git\TopSwagCode.Blog.Api\Blog.Api\Blog.Api.csproj --configuration Release --framework netcoreapp3.1 --self-contained false --runtime linux-x64 --output .\.deploy\REQWEQEQ
            //System.Diagnostics.Process.Start("dotnet", @"publish C:\git\TopSwagCode.Blog.Api\Blog.Api\Blog.Api.csproj --configuration Release --framework netcoreapp3.1 --self-contained false --runtime linux-x64 --output .\.deploy\REQWEQEQ");
            System.Diagnostics.Process p = new System.Diagnostics.Process();
            p.StartInfo = new System.Diagnostics.ProcessStartInfo("dotnet", cmd);
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.Start();
            // instead of p.WaitForExit(), do
            StringBuilder q = new StringBuilder();
            while (!p.HasExited)
            {
                q.Append(p.StandardOutput.ReadToEnd());
            }
            string r = q.ToString();

            return r;
        }
    }
}
