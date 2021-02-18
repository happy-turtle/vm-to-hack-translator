using System;

///<summary>
///VM to Hack translator program.
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
            CodeWriter codeWriter = new CodeWriter(parser.fileName);

            while(parser.HasMoreCommands())
            {
                Parser.CommandType commandType = parser.GetCommandType();

                if(commandType == Parser.CommandType.C_POP || commandType == Parser.CommandType.C_PUSH)
                {
                    codeWriter.WritePushPop(commandType, parser.GetArg1(), parser.GetArg2());
                }
                else if(commandType == Parser.CommandType.C_ARITHMETIC)
                {
                    codeWriter.WriteArithmetic(parser.GetArg1());
                }

                parser.Advance();
            }
            
            codeWriter.Close(args[0]);
        }
    }
}
