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
        const int TempMemLocation = 5;
        
        string staticIdentifier;
        List<string> asmCode = new List<string>();
        int trueLabelCount = 0;
        int endLabelCount = 0;

        public void SetFileName(string fileName)
        {
            asmCode.Clear();
            staticIdentifier = fileName + ".";
        }

        public void WriteInit()
        {

        }

        public void WriteLabel(string label)
        {

        }

        public void WriteGoto(string label)
        {

        }

        public void WriteIf(string label)
        {

        }

        public void WriteFunction(string functionName, int numVars)
        {

        }

        public void WriteCall(string functionName, int numArgs)
        {

        }

        public void WriteReturn()
        {

        }

        //Writes to the output file the assembly code that implements the given arithmetic command.
        public void WriteArithmetic(string command)
        {
            //Write command as comment.
            asmCode.Add("//" + command);

            if(command == "add")
                TwoFieldsArithmeticOperation("+");
            else if(command == "sub")
                TwoFieldsArithmeticOperation("-");
            else if(command == "eq")
                TrueFalseBlock("JEQ");
            else if(command == "gt")
                TrueFalseBlock("JGT");
            else if(command == "lt")
                TrueFalseBlock("JLT");
            else if(command == "and")
                TwoFieldsArithmeticOperation("&");
            else if(command == "or")
                TwoFieldsArithmeticOperation("|");
            else if(command == "neg")
                SingleFieldArithmeticOperation("-");
            else if(command == "not")
                SingleFieldArithmeticOperation("!");
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
            
            if(segment == "constant" && commandType == Parser.CommandType.C_PUSH)
                Push(index);
            else if(segment == "local")
            {
                if(commandType == Parser.CommandType.C_PUSH)
                    Push("LCL", index);
                else if(commandType == Parser.CommandType.C_POP)
                    Pop("LCL", index);
            }
            else if(segment == "argument")
            {
                if(commandType == Parser.CommandType.C_PUSH)
                    Push("ARG", index);
                else if(commandType == Parser.CommandType.C_POP)
                    Pop("ARG", index);
            }
            else if(segment == "this")
            {
                if(commandType == Parser.CommandType.C_PUSH)
                    Push("THIS", index);
                else if(commandType == Parser.CommandType.C_POP)
                    Pop("THIS", index);
            }
            else if (segment == "that")
            {
                if(commandType == Parser.CommandType.C_PUSH)
                    Push("THAT", index);
                else if(commandType == Parser.CommandType.C_POP)
                    Pop("THAT", index);
            }
            else if(segment == "temp")
            {
                if(commandType == Parser.CommandType.C_PUSH)
                {
                    PushTemp(index);
                }
                else if(commandType == Parser.CommandType.C_POP)
                {
                    PopTemp(index);
                }
            }
            else if(segment == "pointer")
            {
                if(commandType == Parser.CommandType.C_PUSH)
                    PushPointer(index);
                else if(commandType == Parser.CommandType.C_POP)
                    PopPointer(index);
            }
            else if(segment == "static")
            {
                if(commandType == Parser.CommandType.C_PUSH)
                    PushStatic(index);
                else if(commandType == Parser.CommandType.C_POP)
                    PopStatic(index);
            }
            else
                asmCode.Add("// not implemented");
        }

        private void PopStatic(int index)
        {
            DecrementStackPointer();

            GetDataAtStackPointer();

            asmCode.Add("@" + staticIdentifier + index);
            asmCode.Add("M=D");
        }

        private void PushStatic(int index)
        {
            asmCode.Add("@" + staticIdentifier + index);
            asmCode.Add("D=M");

            asmCode.Add("@SP");
            asmCode.Add("A=M");
            asmCode.Add("M=D");

            IncrementStackPointer();
        }

        private void PopTemp(int index)
        {
            asmCode.Add("@" + index);
            asmCode.Add("D=A");

            asmCode.Add("@" + TempMemLocation);
            asmCode.Add("D=D+A");
            asmCode.Add("@R13");
            asmCode.Add("M=D");

            DecrementStackPointer();

            GetDataAtStackPointer();

            asmCode.Add("@R13"); // *THAT = *SP
            asmCode.Add("A=M");
            asmCode.Add("M=D");
        }

        private void PushTemp(int index)
        {
            asmCode.Add("@" + index); // D = index
            asmCode.Add("D=A");

            asmCode.Add("@" + TempMemLocation); // D = *(LCL + index)
            asmCode.Add("A=D+A");
            asmCode.Add("D=M");

            asmCode.Add("@SP"); // *SP = D
            asmCode.Add("A=M");
            asmCode.Add("M=D");

            IncrementStackPointer();
        }

        private void PushPointer(int index)
        {
            if (index == 0)
            {
                asmCode.Add("@THIS");
                asmCode.Add("D=M");
                asmCode.Add("@SP");
                asmCode.Add("A=M");
                asmCode.Add("M=D");
            }
            else if (index == 1)
            {
                asmCode.Add("@THAT");
                asmCode.Add("D=M");
                asmCode.Add("@SP");
                asmCode.Add("A=M");
                asmCode.Add("M=D");
            }
            IncrementStackPointer();
        }

        private void PopPointer(int index)
        {
            DecrementStackPointer();
            GetDataAtStackPointer();
            if (index == 0)
            {
                asmCode.Add("@THIS");
                asmCode.Add("M=D");
            }
            else if (index == 1)
            {
                asmCode.Add("@THAT");
                asmCode.Add("M=D");
            }
        }

        private void SingleFieldArithmeticOperation(string operation)
        {
            DecrementStackPointer();

            asmCode.Add("@SP"); // *SP = -*SP
            asmCode.Add("A=M");
            asmCode.Add("M=" + operation + "M");

            IncrementStackPointer();
        }

        private void TwoFieldsArithmeticOperation(string operation)
        {
            DecrementStackPointer();
            GetDataAtStackPointer();
            DecrementStackPointer();

            asmCode.Add("@SP"); // *SP += M
            asmCode.Add("A=M");
            if(operation == "-")
                asmCode.Add("M=M-D");
            else
                asmCode.Add("M=D" + operation + "M");

            IncrementStackPointer();
        }

        private void Push(int index)
        {
            asmCode.Add("@" + index); // D = index
            asmCode.Add("D=A");

            asmCode.Add("@SP"); // *SP = D
            asmCode.Add("A=M");
            asmCode.Add("M=D");

            IncrementStackPointer();
        }

        private void Push(string symbol, int index)
        {
            asmCode.Add("@" + index); // D = index
            asmCode.Add("D=A");

            asmCode.Add("@" + symbol); // D = *(LCL + index)
            asmCode.Add("A=D+M");
            asmCode.Add("D=M");

            asmCode.Add("@SP"); // *SP = D
            asmCode.Add("A=M");
            asmCode.Add("M=D");

            IncrementStackPointer();
        }

        private void Pop(string symbol, int index)
        {
            asmCode.Add("@" + index);
            asmCode.Add("D=A");
            asmCode.Add("@" + symbol);
            asmCode.Add("D=D+M");
            asmCode.Add("@R13");
            asmCode.Add("M=D");

            DecrementStackPointer();

            GetDataAtStackPointer();

            asmCode.Add("@R13"); // *addr = *SP
            asmCode.Add("A=M");
            asmCode.Add("M=D");
        }

        private void TrueFalseBlock(string condition)
        {
            DecrementStackPointer();
            GetDataAtStackPointer();
            DecrementStackPointer();

            asmCode.Add("@SP"); // *SP ?? D
            asmCode.Add("A=M");
            asmCode.Add("D=M-D");

            asmCode.Add("@TRUE" + trueLabelCount);
            asmCode.Add("D;" + condition);

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
            
            IncrementStackPointer();
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