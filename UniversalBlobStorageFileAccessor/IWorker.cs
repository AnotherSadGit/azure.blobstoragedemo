namespace UniversalBlobStorageFileAccessor
{
    public interface IWorker
    {
        Result<string> ReadFromFile();
        Result<bool> WriteToFile();
    }
}