using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dotnet.DeployTool.UI.Shared.Deploy
{
    /// <summary>
    /// Generic client class that interfaces .NET Standard/Blazor with SignalR Javascript client
    /// </summary>
    public class DeployClient : IAsyncDisposable
    {
        public const string HUBURL = "/DeployHub";

        private readonly string _hubUrl;
        private HubConnection _hubConnection;

        /// <summary>
        /// Ctor: create a new client for the given hub URL
        /// </summary>
        /// <param name="siteUrl">The base URL for the site, e.g. https://localhost:1234 </param>
        /// <remarks>
        /// Changed client to accept just the base server URL so any client can use it, including ConsoleApp!
        /// </remarks>
        public DeployClient(string siteUrl)
        {
            // check inputs
            if (string.IsNullOrWhiteSpace(siteUrl))
                throw new ArgumentNullException(nameof(siteUrl));
            // set the hub URL
            _hubUrl = siteUrl.TrimEnd('/') + HUBURL;
        }

        /// <summary>
        /// Start the SignalR client 
        /// </summary>
        public async Task StartAsync()
        {

            // create the connection using the .NET SignalR client
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_hubUrl)
                .Build();
            Console.WriteLine("ChatClient: calling Start()");

            // add handler for receiving messages
            _hubConnection.On<string>(DeployMessageTypes.Feedback, (feedback) =>
            {
                HandleFeedback(feedback);
            });

            // start the connection
            await _hubConnection.StartAsync();

            Console.WriteLine("ChatClient: Start returned");
            
        }

        /// <summary>
        /// Handle an inbound message from a hub
        /// </summary>
        /// <param name="method">event name</param>
        /// <param name="message">message content</param>
        private void HandleFeedback(string feedback)
        {
            // raise an event to subscribers
            FeedbackReceived?.Invoke(this, new FeedbackReceivedEventArgs(feedback));
        }

        /// <summary>
        /// Event raised when this client receives a message
        /// </summary>
        /// <remarks>
        /// Instance classes should subscribe to this event
        /// </remarks>
        public event FeedbackReceivedEventHandler FeedbackReceived;


        public async Task UpdateConfigAndTestConnection(string pemFilePath, string ip, int port, string username)
        {
            await _hubConnection.SendAsync(DeployMessageTypes.UpdateConfigAndTestConnection, pemFilePath, ip, port, username);
        }


        public async Task InstallAppRuntime(string pemFilePath, string ip, int port, string username, int osVersion, int appRuntimeVersion)
        {
            await _hubConnection.SendAsync(DeployMessageTypes.InstallAppRuntime, pemFilePath, ip, port, username, osVersion, appRuntimeVersion);
        }

        public async Task PublishApp(string pathToCsproj, string appName, int appRuntime)
        {
            await _hubConnection.SendAsync(DeployMessageTypes.PublishApp, pathToCsproj, appName, appRuntime);
        }

        public async Task UploadSolution(string pemFilePath, string ip, int port, string username, string projectName)
        {
            await _hubConnection.SendAsync(DeployMessageTypes.UploadSolution, pemFilePath, ip, port, username, projectName);
        }

        public async Task StopAsync()
        {

            await _hubConnection.StopAsync();
            // There is a bug in the mono/SignalR client that does not
            // close connections even after stop/dispose
            // see https://github.com/mono/mono/issues/18628
            // this means the demo won't show "xxx left the chat" since 
            // the connections are left open
            await _hubConnection.DisposeAsync();
            _hubConnection = null;

        }

        public async ValueTask DisposeAsync()
        {
            Console.WriteLine("ChatClient: Disposing");
            await StopAsync();
        }
    }

    /// <summary>
    /// Delegate for the message handler
    /// </summary>
    /// <param name="sender">the SignalRclient instance</param>
    /// <param name="e">Event args</param>
    public delegate void FeedbackReceivedEventHandler(object sender, FeedbackReceivedEventArgs e);

    /// <summary>
    /// Feedback received argument class
    /// </summary>
    public class FeedbackReceivedEventArgs : EventArgs
    {
        public FeedbackReceivedEventArgs(string feedback)
        {
            Feedback = feedback;
        }

        /// <summary>
        /// Feedback from server
        /// </summary>
        public string Feedback { get; set; }

    }
}
