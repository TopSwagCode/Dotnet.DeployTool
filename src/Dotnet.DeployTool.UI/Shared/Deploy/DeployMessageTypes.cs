namespace Dotnet.DeployTool.UI.Shared.Deploy
{
    public static class DeployMessageTypes
    {
        /// <summary>
        /// Name of the Hub method to Update config and test connection
        /// </summary>
        public const string UpdateConfigAndTestConnection = "UpdateConfigAndTestConnection";

        /// <summary>
        /// Name of the Hub method to install app runtime
        /// </summary>
        public const string InstallAppRuntime = "InstallAppRuntime";

        /// <summary>
        /// Name of the Hub method to upload solution
        /// </summary>
        public const string UploadSolution = "UploadSolution";

        /// <summary>
        /// Name of the Hub method to send feedback to clients
        /// </summary>
        public const string Feedback = "Feedback";

        public const string PublishApp = "PublishApp";

        public static string SetupService = "SetupService";

        public static string RunSample = "RunSample";
    }
}
