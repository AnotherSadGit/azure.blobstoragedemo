using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UniversalBlobStorageFileAccessor.Settings;
using UniversalBlobStorageFileAccessor.Utilities;

namespace UniversalBlobStorageFileAccessor.FileIO
{
    internal class FileAccessor : IFileAccessor
    {
        private readonly string _accountConnectionString;
        private static readonly Regex _containerNameRegex = new("^[a-z0-9]([a-z0-9-]*[a-z0-9])?$");

        public FileAccessor(IAzureStorageSettings settings)
        {
            _accountConnectionString = settings.StorageAccountConnectionString;
        }

        public Result<Stream> GetStreamForFileRead(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(_accountConnectionString))
            {
                return new Result<Stream>(null, "No storage account connection string specified.");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new Result<Stream>(null, "No file path specified.");
            }

            Result<Stream> result = new Result<Stream>(null);

            Result<string[]> filePathSplitResult = GetFilePathParts(filePath);
            if (filePathSplitResult.HasErrors)
            {
                result.Errors.Add("Unable to read file.");
                result.Errors.AddRange(filePathSplitResult.Errors);
                return result;
            }

            string containerName = filePathSplitResult.Value[0];
            string blobFilePath = filePathSplitResult.Value[1];

            try
            {
                BlobContainerClient containerClient = GetContainerClient(_accountConnectionString, containerName);
                if (!containerClient.Exists())
                {
                    return new Result<Stream>(null,
                        $"Unable to read file: Container '{containerName}' not found for the specified Azure storage account.");
                }

                BlobClient blobClient = containerClient.GetBlobClient(blobFilePath);
                if (!blobClient.Exists())
                {
                    return new Result<Stream>(null,
                        $"Unable to read file: File '{blobFilePath}' not found in container '{containerName}' for the specified Azure storage account.");
                }

                var response = blobClient.Download();
                if (response == null || response.Value == null)
                {
                    return new Result<Stream>(null,
                        $"Unable to read file: No response when attempted to download file '{blobFilePath}' in container '{containerName}'.");
                }

                BlobDownloadInfo downloadInfo = response.Value;
                if (downloadInfo.Content == null)
                {
                    return new Result<Stream>(null,
                        $"Unable to read file: No content returned when attempted to download file '{blobFilePath}' in container '{containerName}'.");
                }

                // Use using statement with Stream in calling code to dispose of it cleanly.
                return new Result<Stream>(downloadInfo.Content);
            }
            catch (Exception ex)
            {
                string? exceptionDetails = ExceptionHelper.GetExceptionDetails(ex);
                return new Result<Stream>(null, $"Error reading blob file '{blobFilePath}' in container '{containerName}': {exceptionDetails}");
            }
        }

        public Result<Stream> GetStreamForFileWrite(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(_accountConnectionString))
            {
                return new Result<Stream>(null, "No storage account connection string specified.");
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new Result<Stream>(null, "No file path specified.");
            }

            Result<Stream> result = new Result<Stream>(null);

            Result<string[]> filePathSplitResult = GetFilePathParts(filePath);
            if (filePathSplitResult.HasErrors)
            {
                result.Errors.Add("Unable to write to file.");
                result.Errors.AddRange(filePathSplitResult.Errors);
                return result;
            }

            string containerName = filePathSplitResult.Value[0];
            string blobFilePath = filePathSplitResult.Value[1];

            try
            {
                BlobContainerClient containerClient = GetContainerClient(_accountConnectionString, containerName);
                if (!containerClient.Exists())
                {
                    return new Result<Stream>(null,
                        $"Unable to write to file: Container '{containerName}' not found for the specified Azure storage account.");
                }

                BlockBlobClient blobClient = containerClient.GetBlockBlobClient(blobFilePath);
                if (blobClient == null)
                {
                    return new Result<Stream>(null,
                        $"Unable to write to file: Unable to get a blob client for file '{blobFilePath}' in container '{containerName}'.");
                }

                Stream stream = blobClient.OpenWrite(overwrite: true);
                if (stream == null)
                {
                    return new Result<Stream>(null,
                        $"Unable to write to file: No stream created when attempted to open file '{blobFilePath}' in container '{containerName}' for writing.");
                }

                // Use using statement with Stream in calling code to dispose of it cleanly.
                return new Result<Stream>(stream);
            }
            catch (Exception ex)
            {
                string? exceptionDetails = ExceptionHelper.GetExceptionDetails(ex);
                return new Result<Stream>(null,
                    $"Error writing to blob file '{blobFilePath}' in container '{containerName}': {exceptionDetails}");
            }
        }

        /// <summary>
        /// Splits the specified file path into the container name and the file path 
        /// excluding the container name.
        /// </summary>
        /// <returns>A Result<string[]> object.  If the filePath contains both a valid container 
        /// name and a valid file path then the result.Value will be a two element array, with 
        /// element [0] being the container name and element [1] being the remaining file path. 
        /// If there is an error then the result.Value will be null and there will be one or 
        /// more result.Errors.</returns>
        private Result<string[]> GetFilePathParts(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new Result<string[]>(null, "No file path specified.");
            }

            string[] pathElements = filePath.Split(new char[] { '/' }, 2);

            if (pathElements.Length < 2)
            {
                string errorMessage =
                    $"File path '{filePath}' is invalid: Does not include both a container and a file name.";
                return new Result<string[]>(null, errorMessage);
            }

            List<string> errors = new List<string>();

            string containerName = pathElements[0];
            Result<bool> containerNameValidationResult = IsContainerNameValid(containerName);
            if (containerNameValidationResult.HasErrors)
            {
                errors.AddRange(containerNameValidationResult.Errors);
            }

            string blobFilePath = pathElements[1];
            Result<bool> blobFilePathValidationResult = IsBlobFilePathValid(blobFilePath);
            if (blobFilePathValidationResult.HasErrors)
            {
                errors.AddRange(blobFilePathValidationResult.Errors);
            }

            if (errors.Count > 0)
            {
                return new Result<string[]>(null, errors);
            }

            return new Result<string[]>(pathElements);
        }

        private BlobContainerClient GetContainerClient(string accountConnectionString, string containerName)
        {
            // Create a BlobServiceClient object which will be used to create a container client
            BlobServiceClient blobServiceClient = new BlobServiceClient(accountConnectionString);

            BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            return containerClient;
        }

        /// <summary>
        /// Indicates whether the Blob Storage container name is valid or not.
        /// </summary>
        /// <remarks>Based on container naming rules specified in 
        /// https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata#container-names</remarks>
        private Result<bool> IsContainerNameValid(string containerName)
        {
            if (string.IsNullOrWhiteSpace(containerName))
            {
                return new Result<bool>(false, "Invalid container name: Container name cannot be empty or blank.");
            }

            int minLength = 3;
            int maxLength = 63;
            if (containerName.Length < minLength || containerName.Length > maxLength)
            {
                return new Result<bool>(false,
                    $"Container name '{containerName}' is invalid: Container name must be between {minLength} and {maxLength} characters long.");
            }

            if (!_containerNameRegex.IsMatch(containerName))
            {
                return new Result<bool>(false,
                    $"Container name '{containerName}' is invalid: Container name may only contain lowercase letters, numbers and the dash character ('-'), and may not start or end with a dash.");
            }

            if (containerName.Contains("--"))
            {
                return new Result<bool>(false,
                    $"Container name '{containerName}' is invalid: Container name may not contain consecutive dash ('-') characters.");
            }

            return new Result<bool>(true);
        }

        /// <summary>
        /// Indicates whether the Blob Storage blob file path is valid or not.
        /// </summary>
        /// <remarks>Based on blob naming rules specified in https://docs.microsoft.com/en-us/rest/api/storageservices/naming-and-referencing-containers--blobs--and-metadata#blob-names</remarks>
        private Result<bool> IsBlobFilePathValid(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return new Result<bool>(false, "Invalid blob file path: File path cannot be empty or blank.");
            }

            // Use the Azure Storage emulator max blob name length, which is shorter than the 
            //  Azure max blob name length.  This allows the application to be used with the 
            //  Azure Storage emulator.
            int maxLength = 256;
            if (filePath.Length > maxLength)
            {
                return new Result<bool>(false,
                    $"Blob file path '{filePath}' is invalid: File path must be no more than {maxLength} characters long.");
            }

            if (filePath.EndsWith('.') || filePath.EndsWith('/'))
            {
                return new Result<bool>(false,
                    $"Blob file path '{filePath}' is invalid: File path may not end with a period ('.') or a forward slash ('/').");
            }

            string[] pathElements = filePath.Split('/');

            int maxNumberOfSegments = 254;
            if (pathElements.Length > maxNumberOfSegments)
            {
                return new Result<bool>(false,
                    $"Blob file path '{filePath}' is invalid: File path may contain no more than {maxNumberOfSegments} path segments.");
            }

            foreach (string pathElement in pathElements)
            {
                if (pathElement.EndsWith('.'))
                {
                    return new Result<bool>(false,
                        $"Blob file path '{filePath}' is invalid: No path segment may end with a period ('.').");
                }
            }

            return new Result<bool>(true);
        }
    }
}
