using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UniversalBlobStorageFileAccessor.FileIO;
using UniversalBlobStorageFileAccessor.Settings;
using UniversalBlobStorageFileAccessor.Utilities;

namespace UniversalBlobStorageFileAccessor
{
    public class Worker : IWorker
    {
        private readonly IAzureStorageSettings _settings;
        private readonly IFileAccessor _fileAccessor;
        public Worker(IAzureStorageSettings settings, 
            IFileAccessor fileAccessor)
        {
            _settings = settings;
            _fileAccessor = fileAccessor;
        }

        public Result<string> ReadFromFile()
        {
            string inputFilePath = _settings.InputBlobFilePath;
            string userFriendlyFilePath = StringHelper.GetUserFriendlyDisplayText(inputFilePath);            

            string errorHeader = $"Unable to read from file '{userFriendlyFilePath}'";

            Result<Stream> getStreamResult =
                _fileAccessor.GetStreamForFileRead(inputFilePath);

            Result<string> result;

            if (getStreamResult.HasErrors || !getStreamResult.HasValue)
            {
                result = new(null, $"{errorHeader}.");
                result.Errors.AddRange(getStreamResult.Errors);
                return result;
            }

            using Stream readStream = getStreamResult.Value;
            using StreamReader fileReader = new StreamReader(readStream);
            string fileFirstLine = fileReader.ReadLine();

            return new Result<string>(fileFirstLine);
        }

        public Result<bool> WriteToFile()
        {
            string outputFilePath = _settings.OutputBlobFilePath;
            string userFriendlyFilePath = StringHelper.GetUserFriendlyDisplayText(outputFilePath);

            string errorHeader = $"Unable to write to file '{userFriendlyFilePath}'";

            Result<Stream> getStreamResult =
                _fileAccessor.GetStreamForFileWrite(outputFilePath);

            Result<bool> result = new(false);

            if (getStreamResult.HasErrors || !getStreamResult.HasValue)
            {
                result.Value = false;
                result.Errors.Add($"{errorHeader}.");
                result.Errors.AddRange(getStreamResult.Errors);
                return result;
            }

            string textToWrite = $"This was written at {DateTime.Now}";
            ReadOnlySpan<byte> spanToWrite = StringHelper.ConvertStringToByteSpan(textToWrite);

            try
            {
                using Stream writeStream = getStreamResult.Value;
                writeStream.Write(spanToWrite);

                result.Value = true;
                return result;
            }
            catch (Exception ex)
            {
                result.Value = false;
                string? exceptionDetails = ExceptionHelper.GetExceptionDetails(ex);
                result.Errors.Add($"Error writing to file '{outputFilePath}': {exceptionDetails}");
                return result;
            }
        }
    }
}
