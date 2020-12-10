using System;
using System.IO;
using System.Collections.Generic;

///<summary>
///VM Translator program.
///Built as part of the nand2tetris course part 2 by Shimon Schocken 
///and Noam Nisan, further information at www.nand2tetris.org.
///</summary>
namespace VMtoHackTranslator
{
    class Program
    {
        const string AssemblyFileExtension = ".asm";

        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("No argument given. Path for input file must be specified.");
                return;
            }

            //read file
            Parser parser = new Parser(args[0]);

            List<string> asmCode = new List<string>();
            while(parser.HasMoreCommands())
            {
                string newLine = string.Empty;

                if(!string.IsNullOrWhiteSpace(newLine))
                    asmCode.Add(newLine);

                parser.Advance();
            }

            if(asmCode.Count > 0)
                SaveAsmCodeFile(asmCode, args[0]);
            else
                Console.WriteLine("Nothing to write.");
        }

        private static string UnknownCommand()
        {
            Console.WriteLine("Unknown command found.");
            return "##UNKNOWN-CMD###";
        }

        private static void SaveAsmCodeFile(List<string> hackCode, string filePath)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                File.WriteAllLines(directoryPath + @"\" + fileName + AssemblyFileExtension, hackCode);
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
