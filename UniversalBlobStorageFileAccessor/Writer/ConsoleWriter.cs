using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBlobStorageFileAccessor.Writer
{
    public class ConsoleWriter : IConsoleWriter
    {
        private readonly ConsoleTextColour _consoleTextColour;

        public ConsoleWriter(ConsoleTextColour consoleTextColour)
        {
            _consoleTextColour = consoleTextColour;
        }

        public void WriteLine()
        {
            this.WriteColouredLine(null, _consoleTextColour.Default);
        }

        public void WriteLine(string text)
        {
            this.WriteColouredLine(text, _consoleTextColour.Default);
        }

        public void WriteErrorLine(string text)
        {
            this.WriteColouredLine(text, _consoleTextColour.Error);
        }

        public void WritePartialErrorLine(string text)
        {
            this.WriteColouredLine(text, _consoleTextColour.PartialError);
        }

        public void WriteSuccessLine(string text)
        {
            this.WriteColouredLine(text, _consoleTextColour.Success);
        }

        public void WriteEmphasisLine(string text)
        {
            this.WriteColouredLine(text, _consoleTextColour.Emphasis);
        }

        public void WriteColouredLine(string? text, ConsoleColor textColour)
        {
            ConsoleColor previousColour = System.Console.ForegroundColor;

            System.Console.ForegroundColor = textColour;
            System.Console.WriteLine(text);
            System.Console.ForegroundColor = previousColour;
        }
    }
}
