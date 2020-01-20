using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FaceAPI.Infrastructure
{
    public class StorageOptions
    {
        public string AccountName { get; set; }

        public string AccountKey { get; set; }

        public string StorageConnectionString { get; set; }

        public string FullImageContainerName { get; set; }

        public string ThumbnailImageContainerName { get; set; }

        public string BaseUrl { get; set; }
    }
}
