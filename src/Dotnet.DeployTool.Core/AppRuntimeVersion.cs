using System;

namespace Dotnet.DeployTool.Core
{
    public enum AppRuntimeVersion
    {
        DotnetCore2_1,
        DotnetCore3_1,
        DotnetCore5Preview,
    }

    public static class AppRuntimeVersionExtensions
    {
        public static string From(this AppRuntimeVersion appRuntimeVersion)
        {
            switch (appRuntimeVersion)
            {
                case AppRuntimeVersion.DotnetCore2_1: return "netcoreapp2.1";
                case AppRuntimeVersion.DotnetCore3_1: return "netcoreapp3.1";
                case AppRuntimeVersion.DotnetCore5Preview: return "";
                default: throw new ArgumentOutOfRangeException("appRuntimeVersion");
            }
        }
    }
}
