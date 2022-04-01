How to Access Files in Azure Blob Storage Programmatically
==========================================================
Simon Elms, 10 Mar 2022

Uses .NET 6 and Azure Blob Storage client library v12 for .NET

Follows tutorial in "Quickstart: Azure Blob Storage client library v12 for .NET", 
https://docs.microsoft.com/en-us/azure/storage/blobs/storage-quickstart-blobs-dotnet?tabs=environment-variable-windows

Requires NuGet package: 
* Azure.Storage.Blobs

Prerequisites
-------------
* Azure subscription
* Azure storage account
* .NET Core SDK for Windows (must be SDK, not runtime)
* Visual Studio Code or Visual Studio (actually can do it from any text editor)

Notes
-----
### Adding appsettings.json file and reading configuration from it based on: 

"Configuration in .NET" > Configure console apps > Basic example with hosting,
https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration#basic-example-with-hosting

Requires NuGet package: 
* Microsoft.Extensions.Hosting

### Reading the content of a file in Blob Storage was based on: 

"Reading string content from Azure Blob Storage using CSharp (C#)", 
https://www.pritambaldota.com/reading-string-content-from-azure-blob-storage-using-csharp/

and, to a lesser extent, on

"How read all files from azure blob storage in C# Core", 
https://stackoverflow.com/questions/61007127/how-read-all-files-from-azure-blob-storage-in-c-sharp-core