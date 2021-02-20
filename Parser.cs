using System;
using System.Collections.Generic;
using System.IO;

///<summary>
///</summary>
namespace VMtoHackTranslator
{
    class Parser
    {
        public enum CommandType {C_ARITHMETIC, C_PUSH, C_POP, C_LABEL, C_GOTO, 
        C_IF, C_FUNCTION, C_RETURN, C_CALL, C_ERROR}
        public readonly string fileName;

        string[] lines;
        int currentLine = 0;

        public void ReadFile(string filePath)
        {
            try
            {
                lines = StripCommentsAndWhiteSpace(File.ReadAllLines(filePath));
                currentLine = 0;
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public bool HasMoreCommands()
        {
            if(lines == null || lines.Length == 0)
                return false;

            if(currentLine < lines.Length)
                return true;

            return false;
        }

        public void Advance()
        {
            currentLine++;
        }

        //Returns a constant representing the type of the current command.
        //C_ARITHMETIC is returned for for all the arithmetic/logical commands.
        public CommandType GetCommandType()
        {

            string command = lines[currentLine];

            if(command.StartsWith("push"))
                return CommandType.C_PUSH;
            else if(command.StartsWith("pop"))
                return CommandType.C_POP;
            else if(command.StartsWith("add") || command.StartsWith("sub") || command.StartsWith("neg") 
            || command.StartsWith("eq") || command.StartsWith("gt") ||  command.StartsWith("lt") ||
             command.StartsWith("and") || command.StartsWith("or") || command.StartsWith("not"))
                return CommandType.C_ARITHMETIC;
            else if(command.StartsWith("goto"))
                return CommandType.C_GOTO;
            else if(command.StartsWith("if-goto"))
                return CommandType.C_IF;
            else if(command.StartsWith("label"))
                return CommandType.C_LABEL;
            else if(command.StartsWith("call"))
                return CommandType.C_CALL;
            else if(command.StartsWith("function"))
                return CommandType.C_FUNCTION;
            else if(command.StartsWith("return"))
                return CommandType.C_RETURN;
            else
            {
                UnknownCommand();
                return CommandType.C_ERROR;
            }
        }

        //Returns the first argument of the current command. In the case of C_ARITHMETIC
        //the command itself (add, sub, etc.) is returned. Should not be called if the current
        //command is C_RETURN.
        public string GetArg1()
        {
            string line = lines[currentLine];
            if(line.StartsWith("push"))
                return line.Split()[1];
            else if(line.StartsWith("pop"))
                return line.Split()[1];
            else
                return line.Trim(' ');
        }

        //Returns the second argument of the current command. Should be called only if the current
        //command is C_PUSH, C_POP, C_FUNCTION or C_CALL.
        public int GetArg2()
        {
            if (int.TryParse(lines[currentLine].Split()[2], out int result))
                return result;
            else
                return 0;
        }

        private static string[] StripCommentsAndWhiteSpace(string[] lines)
        {
            List<string> lineList = new List<string>();
            foreach (string line in lines)
            {
                if (!line.TrimStart(' ').StartsWith("//") && !string.IsNullOrWhiteSpace(line))
                {
                    string codeLine = line.Split("//")[0];
                    lineList.Add(codeLine.Trim(' '));
                }
            }
            return lineList.ToArray();
        }
        
        private static string UnknownCommand()
        {
            Console.WriteLine("Unknown command found.");
            return "##UNKNOWN-CMD###";
        }
    }
}