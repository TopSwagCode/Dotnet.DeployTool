using Renci.SshNet;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Dotnet.DeployTool.CLI
{
    
    class Program
    {

        static string ip = "54.229.153.219";

        static async Task Main(string[] args)
        {



            var test = PingHost("34.255.4.203", 22);

            /*

               Console.WriteLine("");

               UploadSolution();

               Console.WriteLine("");

               await InstallDotnetCore3_1();

               Console.WriteLine("");

               await RunSampleApp();

               Console.WriteLine("DONE!");

               Console.ReadLine();
             */
        }




        // TODO: Create service definition, eg. the one from Docker-Compose project and use that to startup AspnetCore

        // Publish:
        // dotnet publish --configuration Release --framework netcoreapp3.1 --self-contained false --runtime linux-x64 --verbosity quiet

    }

    
}
