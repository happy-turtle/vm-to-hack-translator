using System;
using System.IO;
using System.Collections.Generic;

///<summary>
///</summary>
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

        //Writes to the output file the assembly code that implements the given arithmetic command.
        public void WriteArithmetic(string command)
        {
            //Write command as comment.
            asmCode.Add("//" + command);

            if(command == "add")
            {
                asmCode.Add("@SP"); // *SP - 1
                asmCode.Add("M=M-1");
                asmCode.Add("@SP"); // D = SP
                asmCode.Add("A=M");
                asmCode.Add("D=M");
                asmCode.Add("@SP"); // *SP - 1
                asmCode.Add("M=M-1");
                asmCode.Add("@SP"); // M += SP
                asmCode.Add("A=M");
                asmCode.Add("M=M+D");
                asmCode.Add("@SP"); // *SP + 1
                asmCode.Add("M=M+1");
            }
            else
                asmCode.Add("//not yet implemented");
        }

        //Writes to the output file the assembly code that implements the given command,
        //where command is either C_PUSH or C_POP.
        public void WritePushPop(Parser.CommandType commandType, string segment, int index)
        {
            //Write command as comment.
            if(commandType == Parser.CommandType.C_PUSH)
                asmCode.Add("//push " + segment + " " + index);
            else if (commandType == Parser.CommandType.C_POP)
                asmCode.Add("//pop " + segment + " " + index);
            
            if(segment == "constant")
            {
                asmCode.Add("@" + index); // D = index
                asmCode.Add("D=A");
                asmCode.Add("@SP"); // *SP = D
                asmCode.Add("A=M");
                asmCode.Add("M=D");
                asmCode.Add("@SP"); // *SP + 1
                asmCode.Add("M=M+1");
            }
            else
            {
                asmCode.Add("// not yet implemented");
            }
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