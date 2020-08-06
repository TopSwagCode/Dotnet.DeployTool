using System.Collections.Generic;

namespace Dotnet.DeployTool.Core
{
    public class ScriptManager : IScriptManager
    {
        private Dictionary<ScriptKey, List<string>> ScriptDictionary { get; set; }
        public ScriptManager()
        {
            ScriptDictionary = new Dictionary<ScriptKey, List<string>>
            {
                {
                    new ScriptKey(
                        OsVersion.Ubuntu_20_04_LTS,
                        AppRuntimeVersion.DotnetCore3_1
                    ),
                    new List<string>
                    {
                        "wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb",
                        "sudo dpkg -i packages-microsoft-prod.deb",

                        "sudo apt-get update",
                        "sudo apt-get install -y apt-transport-https",
                        "sudo apt-get update",
                        "sudo apt-get install -y aspnetcore-runtime-3.1 "
                    }
                },
                {
                    new ScriptKey(
                        OsVersion.Ubuntu_20_04_LTS,
                        AppRuntimeVersion.DotnetCore2_1
                    ),
                    new List<string>
                    {
                        "wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb",
                        "sudo dpkg -i packages-microsoft-prod.deb",

                        "sudo apt-get update",
                        "sudo apt-get install -y apt-transport-https",
                        "sudo apt-get update",
                        "sudo apt-get install -y aspnetcore-runtime-2.1 "
                    }
                },
                {
                    new ScriptKey(
                        OsVersion.Ubuntu_18_04_LTS,
                        AppRuntimeVersion.DotnetCore3_1
                    ),
                    new List<string>
                    {
                        "wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb",
                        "sudo dpkg -i packages-microsoft-prod.deb",

                        "sudo apt-get update",
                        "sudo apt-get install -y apt-transport-https",
                        "sudo apt-get update",
                        "sudo apt-get install -y aspnetcore-runtime-3.1 "
                    }
                },
                {
                    new ScriptKey(
                        OsVersion.Ubuntu_18_04_LTS,
                        AppRuntimeVersion.DotnetCore2_1
                    ),
                    new List<string>
                    {
                        "wget https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb",
                        "sudo dpkg -i packages-microsoft-prod.deb",

                        "sudo apt-get update",
                        "sudo apt-get install -y apt-transport-https",
                        "sudo apt-get update",
                        "sudo apt-get install -y aspnetcore-runtime-2.1 "
                    }
                },
                {
                    new ScriptKey(
                        OsVersion.Ubuntu_16_04_LTS,
                        AppRuntimeVersion.DotnetCore3_1
                    ),
                    new List<string>
                    {
                        "wget https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb",
                        "sudo dpkg -i packages-microsoft-prod.deb",

                        "sudo apt-get update",
                        "sudo apt-get install -y apt-transport-https",
                        "sudo apt-get update",
                        "sudo apt-get install -y aspnetcore-runtime-3.1 "
                    }
                },
                {
                    new ScriptKey(
                        OsVersion.Ubuntu_16_04_LTS,
                        AppRuntimeVersion.DotnetCore2_1
                    ),
                    new List<string>
                    {
                        "wget https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb",
                        "sudo dpkg -i packages-microsoft-prod.deb",

                        "sudo apt-get update",
                        "sudo apt-get install -y apt-transport-https",
                        "sudo apt-get update",
                        "sudo apt-get install -y aspnetcore-runtime-2.1 "
                    }
                }
            };
        }

        public bool GetScript(OsVersion osVersion, AppRuntimeVersion appRuntimeVersion, out List<string> scriptLines)
        {
            return ScriptDictionary.TryGetValue(new ScriptKey(osVersion, appRuntimeVersion), out scriptLines);
        }
    }
}
