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
        C_IF, C_FUNCTION, C_RETURN, C_CALL}

        string[] lines;
        int currentLine = 0;

        public Parser(string filePath)
        {
            try
            {
                lines = StripCommentsAndWhiteSpace(File.ReadAllLines(filePath));
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

        public void Reset()
        {
            currentLine = 0;
        }

        public CommandType GetCommandType()
        {
            //Returns a constant representing the type of the current command.
            //C_ARITHMETIC is returned for for all the arithmetic/logical commands.

            // string command = lines[currentLine];
            return CommandType.C_ARITHMETIC;
        }

        public string arg1()
        {
            //Returns the first argument of the current command. In the case of C_ARITHMETIC
            //the command itself (add, sub, etc.) is returned. Should not be called if the current
            //command is C_RETURN.
            return lines[currentLine].Trim('@', '(', ')');
        }

        public string arg2()
        {
            //Returns the second argument of the current command. Should be called only if the current
            //command is C_PUSH, C_POP, C_FUNCTION or C_CALL.
            return lines[currentLine].Trim('@', '(', ')');
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
    }
}