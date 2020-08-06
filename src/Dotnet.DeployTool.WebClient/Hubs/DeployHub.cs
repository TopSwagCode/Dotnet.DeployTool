using Dotnet.DeployTool.Core;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Dotnet.DeployTool.WebClient.Hubs
{
    public class DeployHub : Hub
    {
        private readonly IDeployService _deployService;

        public DeployHub(IDeployService deployService)
        {
            this._deployService = deployService;
        }

        public async Task UpdateConfigAndTestConnection(string pemFilePath, string ip, string port, string username)
        {
            await _deployService.UpdateConfigAndTestConnection(pemFilePath, ip, int.Parse(port), username);
        }

        public async Task InstallAppRuntime(string osVersion, string appRuntimeVersion)
        {
            await _deployService.InstallAppRuntimeAsync((OsVersion)int.Parse(osVersion), (AppRuntimeVersion)int.Parse(appRuntimeVersion));
        }

        public async Task UploadSolution(string projectPath)
        {
            await _deployService.UploadSolution(projectPath);
        }
    }
}
