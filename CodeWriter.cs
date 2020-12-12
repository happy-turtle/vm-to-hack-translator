using System;
using System.IO;
using System.Collections.Generic;

namespace VMtoHackTranslator
{
    class CodeWriter
    {
        const string AssemblyFileExtension = ".asm";
        
        private List<string> asmCode;

        public CodeWriter()
        {
            asmCode = new List<string>();
        }

        public void WriteArithmetic(string command)
        {
            //Writes to the output file the assembly code that implements the given arithmetic command.
            asmCode.Add("//" + command);
  
            string newLine = string.Empty;

            newLine = "arithmetic";

            if(!string.IsNullOrWhiteSpace(newLine))
                asmCode.Add(newLine);
        }

        public void WritePushPop(Parser.CommandType commandType, string segment, int index)
        {
            //Writes to the output file the assembly code that implements the given command,
            //where command is either C_PUSH or C_POP.
            if(commandType == Parser.CommandType.C_PUSH)
                asmCode.Add("//push " + segment + " " + index);
            else if (commandType == Parser.CommandType.C_POP)
                asmCode.Add("//pop " + segment + " " + index);
            
            string newLine = string.Empty;

            newLine = segment + " " + index;

            if(!string.IsNullOrWhiteSpace(newLine))
                asmCode.Add(newLine);
        }

        public void Close(string filePath)
        {
            if(asmCode.Count > 0)
                SaveAsmCodeFile(asmCode, filePath);
            else
                Console.WriteLine("Nothing to write.");
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