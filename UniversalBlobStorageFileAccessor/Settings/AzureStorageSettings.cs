using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBlobStorageFileAccessor.Settings
{
    public class AzureStorageSettings : IAzureStorageSettings
    {
        public string? StorageAccountConnectionString { get; set; }
        public string? InputBlobFilePath { get; set; }
        public string? OutputBlobFilePath { get; set; }
    }
}
