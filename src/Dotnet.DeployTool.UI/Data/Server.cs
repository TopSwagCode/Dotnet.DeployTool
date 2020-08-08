using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dotnet.DeployTool.UI.Data
{
    public class Server
    {
        [Required]
        [StringLength(100, ErrorMessage = "Ip required", MinimumLength = 1)]
        public string IP { get; set; }

        [Required]
        [Range(1, 100000, ErrorMessage = "Port required")]
        public int Port { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Username required", MinimumLength = 1)]
        public string Username { get; set; }

        [Required]
        [StringLength(5000, ErrorMessage = "Pem key filepath required", MinimumLength = 1)]
        public string PemKeyFilePath { get; set; }
    }
}
