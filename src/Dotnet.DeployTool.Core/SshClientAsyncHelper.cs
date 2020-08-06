using Renci.SshNet;
using System.Threading.Tasks;

namespace Dotnet.DeployTool.Core
{
    public static class SshClientAsyncHelper
    {
        public static Task<string> RunCommandAsync(this SshClient sshClient, string commandText)
        {
            var command = sshClient.RunCommand(commandText);
            return Task<string>.Factory.FromAsync(command.BeginExecute(), command.EndExecute);
        }
    }
}
