using System;
using System.IO;

///<summary>
///VM to Hack translator program.
///Built as part of the nand2tetris course part 2 by Shimon Schocken 
///and Noam Nisan, further information at www.nand2tetris.org.
///</summary>
namespace VMtoHackTranslator
{
    class Program
    {
        static Parser parser = new Parser();
        static CodeWriter codeWriter = new CodeWriter();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("No argument given. Path for input file must be specified.");
                return;
            }
            
            FileAttributes attributes = File.GetAttributes(args[0]);
            if(attributes.HasFlag(FileAttributes.Directory))
            {
                string[] files = Directory.GetFiles(args[0], "*.vm");
                foreach(string filePath in files)
                {
                    TranslateFile(filePath);
                }
            }
            else
            {
                TranslateFile(args[0]);
            }
        }

        private static void TranslateFile(string path)
        {
            //read file
            parser.ReadFile(path);
            codeWriter.SetFileName(Path.GetFileNameWithoutExtension(path));

            while (parser.HasMoreCommands())
            {
                Parser.CommandType commandType = parser.GetCommandType();

                if (commandType == Parser.CommandType.C_POP || commandType == Parser.CommandType.C_PUSH)
                {
                    codeWriter.WritePushPop(commandType, parser.GetArg1(), parser.GetArg2());
                }
                else if (commandType == Parser.CommandType.C_ARITHMETIC)
                {
                    codeWriter.WriteArithmetic(parser.GetArg1());
                }

                parser.Advance();
            }

            codeWriter.Close(path);
        }
    }
}
