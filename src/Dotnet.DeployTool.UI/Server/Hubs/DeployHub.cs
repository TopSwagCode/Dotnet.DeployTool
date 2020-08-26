using Dotnet.DeployTool.Core;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Dotnet.DeployTool.UI.Server.Hubs
{
    public class DeployHub : Hub
    {
        private readonly IDeployService _deployService;

        public DeployHub(IDeployService deployService)
        {
            _deployService = deployService;
        }

        public async Task UpdateConfigAndTestConnection(string pemFilePath, string ip, int port, string username)
        {
            await _deployService.UpdateConfigAndTestConnection(pemFilePath, ip, port, username);
        }

        public async Task InstallAppRuntime(string pemFilePath, string ip, int port, string username, int osVersion, int appRuntimeVersion)
        {
            await _deployService.InstallAppRuntimeAsync(pemFilePath, ip, port, username, (OsVersion)osVersion, (AppRuntimeVersion)appRuntimeVersion);
        }

        public async Task UploadSolution(string pemFilePath, string ip, int port, string username, string projectName)
        {
            await _deployService.UploadSolution(pemFilePath, ip, port, username, projectName);
        }

        public async Task SetupService(string pemFilePath, string ip, int port, string username, string projectName)
        {
            await _deployService.SetupService(pemFilePath, ip, port, username, projectName);
        }

        public async Task PublishApp(string pathToCsproj, string appName, int appRuntime)
        {
            await _deployService.PublishApp(pathToCsproj, appName, (AppRuntimeVersion)appRuntime);
        }

        public override Task OnConnectedAsync()
        {
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
