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
        private int trueLabelCount = 0;
        private int endLabelCount = 0;

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
                DecrementStackPointer();
                GetDataAtStackPointer();
                DecrementStackPointer();

                asmCode.Add("@SP"); // *SP += M
                asmCode.Add("A=M");
                asmCode.Add("M=D+M");

                IncrementStackPointer();
            }
            else if(command == "sub")
            {
                DecrementStackPointer();
                GetDataAtStackPointer();
                DecrementStackPointer();

                asmCode.Add("@SP"); // *SP -= M
                asmCode.Add("A=M");
                asmCode.Add("M=M-D");

                IncrementStackPointer();
            }
            else if(command == "eq")
            {
                DecrementStackPointer();
                GetDataAtStackPointer();
                DecrementStackPointer();

                asmCode.Add("@SP"); // *SP == D
                asmCode.Add("A=M");
                asmCode.Add("D=D-M");

                asmCode.Add("@TRUE" + trueLabelCount);
                asmCode.Add("D;JEQ");

                TrueFalseBlock();

                IncrementStackPointer();
            }
            else if(command == "gt")
            {
                DecrementStackPointer();
                GetDataAtStackPointer();
                DecrementStackPointer();

                asmCode.Add("@SP"); // *SP > D
                asmCode.Add("A=M");
                asmCode.Add("D=M-D");

                asmCode.Add("@TRUE" + trueLabelCount);
                asmCode.Add("D;JGT");

                TrueFalseBlock();

                IncrementStackPointer();
            }
            else if(command == "lt")
            {
                DecrementStackPointer();
                GetDataAtStackPointer();
                DecrementStackPointer();
                
                asmCode.Add("@SP"); // *SP < D
                asmCode.Add("A=M");
                asmCode.Add("D=M-D");

                asmCode.Add("@TRUE" + trueLabelCount);
                asmCode.Add("D;JLT");

                TrueFalseBlock();
                
                IncrementStackPointer();
            }
            else if(command == "and")
            {
                DecrementStackPointer();
                GetDataAtStackPointer();
                DecrementStackPointer();  

                asmCode.Add("@SP"); // *SP && D
                asmCode.Add("A=M");
                asmCode.Add("M=D&M");

                IncrementStackPointer();
            }
            else if(command == "or")
            {
                DecrementStackPointer();
                GetDataAtStackPointer();
                DecrementStackPointer();

                asmCode.Add("@SP"); // *SP || D
                asmCode.Add("A=M");
                asmCode.Add("M=D|M");

                IncrementStackPointer();
            }
            else if(command == "neg")
            {
                DecrementStackPointer();

                asmCode.Add("@SP"); // *SP = -*SP
                asmCode.Add("A=M");
                asmCode.Add("M=-M");

                IncrementStackPointer();
            }
            else if(command == "not")
            {
                DecrementStackPointer();

                asmCode.Add("@SP"); // *SP = !*SP
                asmCode.Add("A=M");
                asmCode.Add("M=!M");

                IncrementStackPointer();
            }
            else
                asmCode.Add("//not implemented");
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
                IncrementStackPointer();
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

        private void TrueFalseBlock()
        {
            asmCode.Add("@SP"); //false
            asmCode.Add("A=M");
            asmCode.Add("M=0");

            asmCode.Add("@END" + endLabelCount);
            asmCode.Add("0;JMP");

            asmCode.Add("(TRUE" + trueLabelCount++ + ")"); //true
            asmCode.Add("@SP");
            asmCode.Add("A=M");
            asmCode.Add("M=-1");

            asmCode.Add("(END" + endLabelCount++ + ")");
        }

        private void GetDataAtStackPointer()
        {
            asmCode.Add("@SP"); // D = *SP
            asmCode.Add("A=M");
            asmCode.Add("D=M");
        }

        private void DecrementStackPointer()
        {
            asmCode.Add("@SP"); // *SP - 1
            asmCode.Add("M=M-1");
        }

        private void IncrementStackPointer()
        {
            asmCode.Add("@SP"); // *SP + 1
            asmCode.Add("M=M+1");
        }
    }
}