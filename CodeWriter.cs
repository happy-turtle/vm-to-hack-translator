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
        
        string fileIdentifier;
        List<string> asmCode = new List<string>();
        int trueLabelCount = 0;
        int endLabelCount = 0;

        public void SetFileName(string fileName)
        {
            fileIdentifier = fileName + ".";
        }

        public void WriteInit()
        {
            //Write command as comment.
            asmCode.Add("//init");

            // Bootstrap code            
            asmCode.Add("@256"); // SP = 256
            asmCode.Add("D=A");
            asmCode.Add("@SP");
            asmCode.Add("M=D");
            WriteCall("Sys.init"); //call Sys.init
        }

        public void WriteLabel(string label)
        {
            //Write command as comment.
            asmCode.Add("//label " + label);

            asmCode.Add("(" + label + ")");
        }

        public void WriteGoto(string label)
        {
            //Write command as comment.
            asmCode.Add("//goto " + label);

            Goto(label);
        }

        public void WriteIf(string label)
        {
            //Write command as comment.
            asmCode.Add("//if-goto " + label);

            DecrementStackPointer();
            GetDataAtStackPointer();

            asmCode.Add("@" + label);
            asmCode.Add("D;JNE");
        }

        public void WriteFunction(string functionName, int numVars)
        {
            //Write command as comment.
            asmCode.Add("//function " + functionName + " " + numVars);

            asmCode.Add("(" + fileIdentifier + functionName + ")");
            for(int i = 0; i < numVars; i++)
            {
                Push(0);
                Pop("LCL", i);
            }
        }

        public void WriteCall(string functionName, int numArgs = 0)
        {
            //Write command as comment.
            asmCode.Add("//call " + functionName + " " + numArgs);

            string retAddrLabel = functionName + "$ret." + numArgs;

            asmCode.Add("@" + retAddrLabel); //push retAddrLabel
            asmCode.Add("D=A");
            asmCode.Add("@SP");
            asmCode.Add("A=M");
            asmCode.Add("M=D");
            IncrementStackPointer();

            asmCode.Add("@LCL"); //push LCL
            asmCode.Add("D=M");
            asmCode.Add("@SP");
            asmCode.Add("A=M");
            asmCode.Add("M=D");
            IncrementStackPointer();

            asmCode.Add("@ARG"); //push ARG
            asmCode.Add("D=M");
            asmCode.Add("@SP");
            asmCode.Add("A=M");
            asmCode.Add("M=D");
            IncrementStackPointer();

            asmCode.Add("@THIS"); //push THIS
            asmCode.Add("D=M");
            asmCode.Add("@SP");
            asmCode.Add("A=M");
            asmCode.Add("M=D");
            IncrementStackPointer();

            asmCode.Add("@THAT"); //push THAT
            asmCode.Add("D=M");
            asmCode.Add("@SP");
            asmCode.Add("A=M");
            asmCode.Add("M=D");
            IncrementStackPointer();

            asmCode.Add("@SP"); // ARG = SP-5-nArgs // Repositions ARG
            asmCode.Add("D=M");
            asmCode.Add("@5");
            asmCode.Add("D=D-A");
            asmCode.Add("@" + numArgs);
            asmCode.Add("D=D-A");
            asmCode.Add("@ARG"); 
            asmCode.Add("M=D");

            asmCode.Add("@SP"); //LCL = SP // Repositions LCL
            asmCode.Add("D=A");
            asmCode.Add("@LCL"); 
            asmCode.Add("M=D");

            asmCode.Add("(" + retAddrLabel + ")"); //(retAddrLabel)
        }

        public void WriteReturn()
        {
            //Write command as comment.
            asmCode.Add("//return");

            asmCode.Add("@LCL"); // endFrame = LCL // endframe is a temporary variable
            asmCode.Add("D=M");
            asmCode.Add("@R14"); // endFrame = @R14 
            asmCode.Add("M=D");

            asmCode.Add("@5"); // retAddr = *(endFrame – 5) // gets the return address
            asmCode.Add("D=D-A");
            asmCode.Add("@R15"); // retAddr = @R15
            asmCode.Add("M=D");
            
            Pop("ARG", 0); // *ARG = pop() // repositions the return value for the caller

            asmCode.Add("@ARG"); // SP = ARG + 1 // repositions SP of the caller
            asmCode.Add("D=M+1");
            asmCode.Add("@SP");
            asmCode.Add("M=D");

            asmCode.Add("@R14"); // THAT = *(endFrame – 1) // restores THAT of the caller
            asmCode.Add("M=M-1");
            asmCode.Add("A=M");
            asmCode.Add("D=M");
            asmCode.Add("@THAT");
            asmCode.Add("M=D");

            asmCode.Add("@R14");  // THIS = *(endFrame – 2) // restores THIS of the caller
            asmCode.Add("M=M-1");
            asmCode.Add("A=M");
            asmCode.Add("D=M");
            asmCode.Add("@THIS");
            asmCode.Add("M=D");

            asmCode.Add("@R14");
            asmCode.Add("M=M-1"); // ARG = *(endFrame – 3) // restores ARG of the caller
            asmCode.Add("A=M");
            asmCode.Add("D=M");
            asmCode.Add("@ARG");
            asmCode.Add("M=D");

            asmCode.Add("@R14");
            asmCode.Add("M=M-1"); // LCL = *(endFrame – 4) // restores LCL of the caller
            asmCode.Add("A=M");
            asmCode.Add("D=M");
            asmCode.Add("@LCL");
            asmCode.Add("M=D");

            asmCode.Add("@R15"); // goto retAddr
            asmCode.Add("A=M");
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

        private void Goto(string label)
        {
            asmCode.Add("@" + label);
            asmCode.Add("0;JMP");
        }

        private void PopStatic(int index)
        {
            DecrementStackPointer();

            GetDataAtStackPointer();

            asmCode.Add("@" + fileIdentifier + index);
            asmCode.Add("M=D");
        }

        private void PushStatic(int index)
        {
            asmCode.Add("@" + fileIdentifier + index);
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

        public void Close(string path, bool directory = false)
        {
            if(asmCode.Count > 0)
            {
                if(directory)
                {
                    try
                    {
                        string fileName = new DirectoryInfo(path).Name;
                        File.WriteAllLines(path + @"\" + fileName + AssemblyFileExtension, asmCode);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else //single file
                {
                    try
                    {
                        string directoryPath = Path.GetDirectoryName(path);
                        string fileName = Path.GetFileNameWithoutExtension(path);
                        File.WriteAllLines(directoryPath + @"\" + fileName + AssemblyFileExtension, asmCode);
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
            }
            else
                Console.WriteLine("Nothing to write.");
        }
    }
}