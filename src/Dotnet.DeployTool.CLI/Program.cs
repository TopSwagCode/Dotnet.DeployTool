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

        static async Task Main(string[] args)
        {
            Console.WriteLine("HelloWorld");
            await Task.Delay(1);
            // https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-publish
            // dotnet publish C:\git\Dotnet.DeployTool\src\Dotnet.DeployTool.CLI\Dotnet.DeployTool.CLI.csproj --configuration Release --framework netcoreapp3.1 --self-contained true --runtime linux-x64 --verbosity quiet --output ./.deploy/test

            /* https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-3.1
             
            [Unit]
            Description=Example .NET Web API App running on Ubuntu

            [Service]
            WorkingDirectory=/var/www/helloapp
            ExecStart=/usr/bin/dotnet /var/www/helloapp/helloapp.dll
            Restart=always    <-- setting?
            # Restart service after 10 seconds if the dotnet service crashes:
            RestartSec=10 <-- setting?
            KillSignal=SIGINT
            SyslogIdentifier=dotnet-example
            User=www-data
            Environment=ASPNETCORE_ENVIRONMENT=Production 
            Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false  <-- setting? Add more -- Colon (:) separators aren't supported in environment variable names. Use a double underscore (__) in place of a colon.

            [Install]
            WantedBy=multi-user.target




            sudo journalctl -fu kestrel-helloapp.service to view logs
             */

        }

        // TODO: Create service definition, eg. the one from Docker-Compose project and use that to startup AspnetCore

        // Publish:
        // dotnet publish --configuration Release --framework netcoreapp3.1 --self-contained false --runtime linux-x64 --verbosity quiet

    }

}
