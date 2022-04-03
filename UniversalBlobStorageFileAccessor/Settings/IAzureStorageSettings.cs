using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBlobStorageFileAccessor.Settings
{
    public interface IAzureStorageSettings
    {
        string? StorageAccountConnectionString { get; set; }

        string? InputBlobFilePath { get; set; }
        string? OutputBlobFilePath { get; set; }
    }
}
