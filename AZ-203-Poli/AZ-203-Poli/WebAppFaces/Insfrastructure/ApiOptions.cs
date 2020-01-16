using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAppFaces.Insfrastructure
{
    public class ApiOptions
    {
        public string ApiUrl { get; set; }

        public string ApiKey { get; set; }

        public bool UseApiKey => !string.IsNullOrEmpty(ApiKey);
    }
}
