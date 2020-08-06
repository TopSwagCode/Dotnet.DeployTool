using Dotnet.DeployTool.Core;
using Dotnet.DeployTool.WebClient.Hubs;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dotnet.DeployTool.WebClient
{
    public class SignalRFeedbackChannel : IFeedbackChannel
    {
        private readonly IHubContext<DeployHub> _hubContext;

        public SignalRFeedbackChannel(IHubContext<DeployHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendFeedback(string feedback) // TODO: Maybe we should only call the client that started us? If user has more tabs open for diffrent tasks.
        {
           await _hubContext.Clients.All.SendAsync("Feedback", feedback);
        }
    }
}
