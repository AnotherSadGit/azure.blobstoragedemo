
namespace UniversalBlobStorageFileAccessor.FileIO
{
    public interface IFileAccessor
    {
        Result<Stream> GetStreamForFileRead(string? filePath);
        Result<Stream> GetStreamForFileWrite(string? filePath);
    }
}