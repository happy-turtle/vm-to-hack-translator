using System;

///<summary>
///VM Translator program.
///Built as part of the nand2tetris course part 2 by Shimon Schocken 
///and Noam Nisan, further information at www.nand2tetris.org.
///</summary>
namespace VMtoHackTranslator
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("No argument given. Path for input file must be specified.");
                return;
            }

            //read file
            Parser parser = new Parser(args[0]);
            CodeWriter codeWriter = new CodeWriter();

            while(parser.HasMoreCommands())
            {
                string newLine = string.Empty;

                newLine = "line";

                if(!string.IsNullOrWhiteSpace(newLine))
                    asmCode.Add(newLine);

                parser.Advance();
            }

            codeWriter.Close(args[0]);

        }

        private static string UnknownCommand()
        {
            Console.WriteLine("Unknown command found.");
            return "##UNKNOWN-CMD###";
        }
    }
}
