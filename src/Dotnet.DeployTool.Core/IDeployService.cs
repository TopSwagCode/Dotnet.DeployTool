using System.Threading.Tasks;

namespace Dotnet.DeployTool.Core
{
    public interface IDeployService
    {
        Task UpdateConfigAndTestConnection(string pemFilePath, string ip, int port, string username);
        Task InstallAppRuntimeAsync(OsVersion osVersion, AppRuntimeVersion appRuntimeVersion);
        Task RunSampleAppAsync();
        Task UploadSolution(string releaseFolderPath);
    }
}