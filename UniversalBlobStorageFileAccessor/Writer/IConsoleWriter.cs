
namespace UniversalBlobStorageFileAccessor.Writer
{
    public interface IConsoleWriter
    {
        void WriteColouredLine(string? text, ConsoleColor textColour);
        void WriteErrorLine(string text);
        void WriteLine();
        void WriteLine(string text);
        void WritePartialErrorLine(string text);
        void WriteSuccessLine(string text);
        void WriteEmphasisLine(string text);
    }
}