using Microsoft.Extensions.Logging;
using Renci.SshNet;
using Renci.SshNet.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Dotnet.DeployTool.Core
{

    public class DeployServiceConfiguration
    {
        public DeployServiceConfiguration(PrivateKeyFile pemKey, string ip, int port, string username)
        {
            PemKey = pemKey;
            Ip = ip;
            Port = port;
            Username = username;
        }

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

        public DeployService(ILogger<DeployService> logger, IFeedbackChannel feedbackChannel, DeployServiceConfiguration deployServiceConfiguration, IScriptManager scriptManager)
        {
            _logger = logger;
            _feedbackChannel = feedbackChannel;
            _config = deployServiceConfiguration;
            _scriptManager = scriptManager;
        }

        public async Task UpdateConfigAndTestConnection(string pemFilePath, string ip, int port, string username)
        {
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

            _config.PemKey = new PrivateKeyFile(pemFilePath);
            _config.Ip = ip;
            _config.Port = port;
            _config.Username = username;

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

        public async Task UploadSolution(string releaseFolderPath)
        {
            try
            {
                await _feedbackChannel.SendFeedback($" - - - - Uploading folder {releaseFolderPath} - - - -");

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

                    var directoryInfo =  new DirectoryInfo(releaseFolderPath);

                    if(directoryInfo.Exists)
                    {
                        client.Upload(directoryInfo, releaseFolderPath);
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

        public async Task InstallAppRuntimeAsync(OsVersion osVersion, AppRuntimeVersion appRuntimeVersion)
        {
            try
            {
                await _feedbackChannel.SendFeedback($"- - - - Installing AppRuntimeVersion: {appRuntimeVersion} on Os: {osVersion} - - - -");

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

        public async Task RunSampleAppAsync()
        {
            Console.WriteLine(" - - - - Starting Sample App - - - -");
            try
            {
                using (var client = _sshClient)
                {
                    client.Connect();

                    var scriptLines = new List<string>
                    {
                        "sudo dotnet publish/SampleApp.dll"
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
}
