using System.Collections.Generic;

namespace Dotnet.DeployTool.Core
{
    public interface IScriptManager
    {
        bool GetScript(OsVersion osVersion, AppRuntimeVersion appRuntimeVersion, out List<string> scriptLines);
    }
}