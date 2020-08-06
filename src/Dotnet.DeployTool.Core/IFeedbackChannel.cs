using System.Threading.Tasks;

namespace Dotnet.DeployTool.Core
{
    public interface IFeedbackChannel
    {
        Task SendFeedback(string feedback);
    }
}
