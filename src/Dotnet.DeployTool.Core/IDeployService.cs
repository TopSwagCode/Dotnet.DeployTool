using System.Threading.Tasks;

namespace Dotnet.DeployTool.Core
{
    public interface IDeployService
    {
        Task UpdateConfigAndTestConnection(string pemFilePath, string ip, int port, string username);
        Task InstallAppRuntimeAsync(string pemFilePath, string ip, int port, string username, OsVersion osVersion, AppRuntimeVersion appRuntimeVersion);
        Task RunSampleAppAsync();
        Task PublishApp(string pathToCsproj, string appName, AppRuntimeVersion appRuntime);
        Task UploadSolution(string pemFilePath, string ip, int port, string username, string projectName);
    }
}