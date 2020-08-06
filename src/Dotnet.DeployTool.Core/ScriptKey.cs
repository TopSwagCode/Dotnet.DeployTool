using System;

namespace Dotnet.DeployTool.Core
{
    internal class ScriptKey
    {
        public ScriptKey(OsVersion osVersion, AppRuntimeVersion appRuntimeVersion)
        {
            OsVersion = osVersion;
            AppRuntimeVersion = appRuntimeVersion;
        }

        public OsVersion OsVersion { get; set; }
        public AppRuntimeVersion AppRuntimeVersion { get; set; }

        public override int GetHashCode() => HashCode.Combine(OsVersion, AppRuntimeVersion);
        public override bool Equals(object obj) => Equals(obj as ScriptKey);
        public bool Equals(ScriptKey obj) => obj != null && obj.OsVersion == this.OsVersion && obj.AppRuntimeVersion == this.AppRuntimeVersion;

    }
}
