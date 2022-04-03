using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;
using UniversalBlobStorageFileAccessor.FileIO;
using UniversalBlobStorageFileAccessor.Settings;
using UniversalBlobStorageFileAccessor.Writer;

namespace UniversalBlobStorageFileAccessor
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using IHost host = CreateHostBuilder(args).Build();

            RunApplication(host.Services);

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, builder) =>
            {
                var env = hostingContext.HostingEnvironment;
                Console.WriteLine($"Environment name: {env.EnvironmentName}");
            })
            .ConfigureServices((context, services) =>
            {
                services
                .AddSingleton<IAzureStorageSettings, AzureStorageSettings>()
                .AddSingleton<ConsoleTextColour>()
                .AddSingleton<IConsoleWriter, ConsoleWriter>()
                .AddTransient<IFileAccessor, FileAccessor>()
                .AddTransient<IWorker, Worker>();
            });

        private static void RunApplication(IServiceProvider services)
        {
            PopulateSettings(services);

            IWorker worker = services.GetRequiredService<IWorker>();
            ThrowIfInstantiatedObjectNull(worker);

            IConsoleWriter consoleWriter = services.GetRequiredService<IConsoleWriter>();
            ThrowIfInstantiatedObjectNull(consoleWriter);

            consoleWriter.WriteEmphasisLine("Attempting to read from file in blob storage...");
            Result<string> readResult = worker.ReadFromFile();
            if (readResult.HasErrors || !readResult.HasValue)
            {
                consoleWriter.WriteErrorLine("Failed to read from file.  Aborting.");
                if (readResult.HasErrors)
                {
                    WriteErrors(readResult.Errors, consoleWriter);
                }

                return;
            }

            consoleWriter.WriteLine($"First line read from file: '{readResult.Value}'");
            consoleWriter.WriteLine();

            consoleWriter.WriteEmphasisLine("Attempting to write to file in blob storage...");
            Result<bool> writeResult = worker.WriteToFile();
            if (writeResult.HasErrors || !writeResult.HasValue || writeResult.Value == false)
            {
                consoleWriter.WriteErrorLine("Failed to write to file.");
                if (writeResult.HasErrors)
                {
                    WriteErrors(writeResult.Errors, consoleWriter);
                }

                return;
            }

            consoleWriter.WriteSuccessLine($"File written successfully.  Check contents of file.");
            consoleWriter.WriteLine();
        }

        /// <summary>
        /// Populates the settings singleton in the DI container with values read from the 
        /// configuration.
        /// </summary>
        private static void PopulateSettings(IServiceProvider services)
        {
            IAzureStorageSettings? settings = services.GetService<IAzureStorageSettings>();
            ThrowIfInstantiatedObjectNull(settings);

            IConfiguration config = services.GetRequiredService<IConfiguration>();
            ThrowIfInstantiatedObjectNull(config);

            settings.StorageAccountConnectionString = 
                config.GetValue<string>("ConnectionStrings:AZURE_STORAGE_CONNECTION_STRING");

            string containerName = config.GetValue<string>("Settings:ContainerName");
            string inputFileName = config.GetValue<string>("Settings:InputFileName");
            string outputFileName = config.GetValue<string>("Settings:OutputFileName");

            settings.InputBlobFilePath = Path.Combine(containerName, inputFileName).Replace("\\", "/");
            settings.OutputBlobFilePath = Path.Combine(containerName, outputFileName).Replace("\\", "/");
        }

        private static void ThrowIfInstantiatedObjectNull<T>(T? objectToTest) where T : class
        {
            if (objectToTest == null)
            {
                string typeName = typeof(T).Name;
                string errorMessage = $"Unable to retrieve {typeName} object from DI container."
                                    + $"  {typeName} is not registered correctly in method CreateHostBuilder.";
                throw new ApplicationException(errorMessage);
            }
        }

        private static void WriteErrors(List<string> errors, IConsoleWriter consoleWriter)
        {
            if (errors == null || !errors.Any())
            {
                return;
            }

            consoleWriter.WriteErrorLine("  ERRORS:");
            foreach (string error in errors)
            {
                consoleWriter.WriteErrorLine($"    {error}");
            }
        }
    }
}
