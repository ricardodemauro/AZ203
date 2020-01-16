using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FaceAPI.Infrastructure
{
    public class StorageOptions
    {
        public string StorageConnectionString { get; set; }

        public string FullImageContainerName { get; set; }

        public string ThumbnailImageContainerName { get; set; }
    }
}
