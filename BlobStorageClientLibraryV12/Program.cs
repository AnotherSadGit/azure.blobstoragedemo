using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlobQuickstartV12
{
    /// <summary>
    /// How to access files in Azure Blob Storage programmatically and read from a file.
    /// </summary>
    /// <remarks>From "Quickstart: Azure Blob Storage client library v12 for .NET", 
    /// https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet?tabs=environment-variable-windows
    /// 
    /// Requires NuGet package Azure.Storage.Blobs
    /// 
    /// Original tutorial read storage account connection string from environment variable.  
    /// Changed to read it from appsettings.json, based on 
    /// "Configuration in .NET" > Configure console apps > Basic example with hosting, 
    /// https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration#basic-example-with-hosting
    /// This requires NuGet package Microsoft.Extensions.Hosting
    /// 
    /// Reading the content of a file in Blob Storage was based on
    /// "Reading string content from Azure Blob Storage using CSharp (C#)", 
    /// https://www.pritambaldota.com/reading-string-content-from-azure-blob-storage-using-csharp/
    /// and, to a lesser extent, on
    /// "How read all files from azure blob storage in C# Core", 
    /// https://stackoverflow.com/questions/61007127/how-read-all-files-from-azure-blob-storage-in-c-sharp-core
    /// </remarks>
    class Program
    {
        /// <summary>
        /// 
        /// </summary>
        /// <remarks>Main must be async.
        /// 
        /// </remarks>
        static async Task Main()
        {
            using IHost host = Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostingContext, builder) =>
                {
                    var env = hostingContext.HostingEnvironment;
                    Console.WriteLine($"Environment name: {env.EnvironmentName}");
                })
                .Build();

            // Ask the service provider for the configuration abstraction.
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();            

            // Get values from the config given their key and their target type.
            string accountConnectionString = config.GetValue<string>("ConnectionStrings:AZURE_STORAGE_CONNECTION_STRING");
            string containerName = config.GetValue<string>("Settings:ContainerName");
            string fileName = config.GetValue<string>("Settings:FileName");

            // Console.WriteLine($"AZURE_STORAGE_CONNECTION_STRING = {accountConnectionString}");

            // Application code which might rely on the config could start here.

            BlobContainerClient containerClient = GetContainerClient(accountConnectionString, containerName);
            await ListBlobsAsync(containerClient);

            using StreamReader fileReader = await GetFileReaderAsync(containerClient, fileName);
            if (fileReader == null)
            {
                Console.WriteLine($"Unable to get stream reader for file {fileName}");
            }
            else
            {
                string fileFirstLine = await fileReader.ReadLineAsync();
                Console.WriteLine($"First line read from file {fileName}: {fileFirstLine}");
            }

            await host.RunAsync();
        }

        static BlobContainerClient GetContainerClient(string accountConnectionString, string containerName)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(accountConnectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            return containerClient;
        }

        static async Task ListBlobsAsync(BlobContainerClient containerClient)
        {
            Console.WriteLine("Listing blobs:");

            // List all blobs in the container
            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                Console.WriteLine("\t" + blobItem.Name);
            }
        }

        static async Task<StreamReader> GetFileReaderAsync(BlobContainerClient containerClient, string fileName)
        {
            BlobClient blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DownloadAsync();
            var streamReader = new StreamReader(response.Value.Content);
            return streamReader;
        }

        static async Task<string> GetFileFirstLineAsync(StreamReader streamReader)
        {
            var firstLine = await streamReader.ReadLineAsync();
            return firstLine;
        }
    }
}