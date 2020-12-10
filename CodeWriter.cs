using System;

namespace VMtoHackTranslator
{
    class CodeWriter
    {
        public CodeWriter()
        {
            //opens the output file/stream and gets ready to write into it
        }

        public void WriteArithmetic(string command)
        {
            //Writes to the output file the assembly code that implements the given arithmetic command.
        }

        public void WritePushPop(Parser.CommandType commandType, string segment, int index)
        {
            //Writes to the output file the assembly code that implements the given command,
            //where command is either C_PUSH or C_POP.
        }

        public void Close()
        {
            //closes the output file
        }
    }
}