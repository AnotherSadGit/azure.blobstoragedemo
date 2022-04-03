using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniversalBlobStorageFileAccessor.Writer
{
    public class ConsoleTextColour
    {
        public ConsoleColor Default => System.Console.ForegroundColor;
        public ConsoleColor Success => ConsoleColor.Green;
        public ConsoleColor Error => ConsoleColor.Red;
        public ConsoleColor PartialError => ConsoleColor.Yellow;
        public ConsoleColor Emphasis => ConsoleColor.Cyan;
    }
}
