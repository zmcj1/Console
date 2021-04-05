﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Text;

namespace NativeFunctionTranslator
{
    struct Param
    {
        public string VarType;
        public string VarName;

        public Param(string VarType, string VarName)
        {
            this.VarType = VarType;
            this.VarName = VarName;
        }
    }

    struct ParamLine
    {
        public List<Param> _params;

        public ParamLine(int a)
        {
            _params = new List<Param>();
        }

        public override string ToString()
        {
            string str = string.Empty;
            for (int i = 0; i < _params.Count; i++)
            {
                Param p = _params[i];

                if (_params.Count == 1 || i == _params.Count - 1)
                {
                    str += p.VarType + " " + p.VarName;
                }
                else
                {
                    str += p.VarType + " " + p.VarName + ", ";
                }
            }
            return str;
        }
    }

    class Program
    {
        public static List<string> GetFileListWithExtend(DirectoryInfo directory, string pattern)
        {
            List<string> pathList = new List<string>();
            string result = string.Empty;
            if (directory.Exists || pattern.Trim() != string.Empty)
            {
                foreach (FileInfo info in directory.GetFiles(pattern))
                {
                    result = info.FullName.ToString();
                    pathList.Add(result);
                }
            }
            return pathList;
        }

        static void Main(string[] args)
        {
            DirectoryInfo directory = new DirectoryInfo(Environment.CurrentDirectory);

            string MinConsoleFolder = directory.Parent.Parent.Parent.Parent.Parent.ToString();

            //try to read
            string targetFolder = Path.Combine(MinConsoleFolder, "src\\MinConsoleNative");
            //try to write
            string targetFile = Path.Combine(MinConsoleFolder, "src\\MinConsole\\MinConsoleNativeFuncs.cs");

            List<string> headFiles = GetFileListWithExtend(new DirectoryInfo(targetFolder), "*.h");

            List<string> targetFileLines = new List<string>();
            targetFileLines.Add("using System;");
            targetFileLines.Add("using System.Runtime.InteropServices;");
            targetFileLines.Add("using static MinConsole.MinConsoleNativeStructs;");
            targetFileLines.Add("");
            targetFileLines.Add("namespace MinConsole");
            targetFileLines.Add("{");
            targetFileLines.Add("    //This class is auto generated by NativeFunctionTranslator.");
            targetFileLines.Add("    internal static class MinConsoleNativeFuncs");
            targetFileLines.Add("    {");
            targetFileLines.Add("        //>>>insert_here<<<");
            targetFileLines.Add("    }");
            targetFileLines.Add("}");

            int insertLineNumber = 0;

            const string EXPORT_FUNC = "EXPORT_FUNC";
            const string INSERT_HERE = "//>>>insert_here<<<";
            //In C# file:
            const string EXPORT_FUNC_DLLIMPORT =
                "[DllImport(\"MinConsoleNative.dll\", CallingConvention = CallingConvention.StdCall, SetLastError = true, CharSet = CharSet.Unicode)]";
            const string EXPORT_FUNC_RETURN_TYPE = "public extern static bool";
            const int indent = 8;
            string indentLine = string.Empty;
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < indent; i++)
            {
                builder.Append(" ");
            }
            indentLine = builder.ToString();

            List<string> nativeMethodDeclaration = new List<string>();
            List<ParamLine> paramLines = new List<ParamLine>();

            foreach (string headFile in headFiles)
            {
                string text = File.ReadAllText(headFile);
                string[] lines = text.Split(Environment.NewLine);

                foreach (string line in lines)
                {
                    string _line = line.Trim();

                    if (_line.IndexOf(EXPORT_FUNC) != -1)
                    {
                        bool equal = true;
                        for (int i = 0; i < EXPORT_FUNC.Length; i++)
                        {
                            if (_line[i] != EXPORT_FUNC[i])
                            {
                                equal = false;
                                break;
                            }
                        }

                        if (equal && !nativeMethodDeclaration.Contains(_line))
                        {
                            nativeMethodDeclaration.Add(_line);
                        }
                    }
                }
            }

            for (int i = 0; i < targetFileLines.Count; i++)
            {
                string fileLine = targetFileLines[i];

                string _line = fileLine.Trim();

                if (_line.IndexOf(INSERT_HERE) != -1)
                {
                    bool equal = true;
                    for (int j = 0; j < INSERT_HERE.Length; j++)
                    {
                        if (_line[j] != INSERT_HERE[j])
                        {
                            equal = false;
                            break;
                        }
                    }
                    if (equal)
                    {
                        insertLineNumber = i + 1;
                    }
                }
            }

            List<string> readyToWrite = new List<string>();

            foreach (string item in nativeMethodDeclaration)
            {
                string newLine = item.Replace(EXPORT_FUNC, EXPORT_FUNC_RETURN_TYPE);

                //todo
                int Brackets1Index = newLine.IndexOf('(');
                int Brackets2Index = newLine.IndexOf(')');

                ParamLine line = new ParamLine(0);
                paramLines.Add(line);

                string parameterList = newLine.Substring(Brackets1Index + 1, Brackets2Index - Brackets1Index - 1);
                if (!string.IsNullOrEmpty(parameterList))
                {
                    string[] _params = parameterList.Split(',');

                    foreach (string _param in _params)
                    {
                        string[] type_names = _param.Split(' ');

                        string varType = string.Empty;
                        //
                        for (int i = 0; i < type_names.Length - 1; i++)
                        {
                            //const param
                            if (type_names[i] == "const")
                            {
                                continue;
                            }
                            //ptr
                            int ptrIndex = type_names[i].LastIndexOf('*');
                            if (ptrIndex != -1)
                            {
                                string _type = type_names[i].Substring(0, ptrIndex);
                                if (_type == "wchar*")
                                {
                                    varType = "ref string";
                                }
                                else if (_type == "wchar")
                                {
                                    varType = "string";
                                }
                                else if (_type == "HWND")
                                {
                                    varType = "IntPtr";
                                }
                                else if (_type == "FARPROC")
                                {
                                    varType = "ref object";
                                }
                                else
                                {
                                    varType = "ref " + _type;
                                }
                            }
                            //no ptr
                            else
                            {
                                if (type_names[i] == "HWND")
                                {
                                    varType = "IntPtr";
                                }
                                else if (type_names[i] == "DWORD")
                                {
                                    varType = "uint";
                                }
                                else if (type_names[i] == "HICON")
                                {
                                    varType = "ref ICON";
                                }
                                else if (type_names[i] == "wchar")
                                {
                                    varType = "char";
                                }
                                else
                                {
                                    varType = type_names[i];
                                }
                            }
                        }

                        string varName = type_names[type_names.Length - 1];
                        Param param = new Param(varType, varName);
                        line._params.Add(param);
                    }
                }
                else
                {
                    line._params.Add(new Param("", ""));
                }

                readyToWrite.Add(indentLine + EXPORT_FUNC_DLLIMPORT);
                //replace
                string body = newLine.Substring(0, Brackets1Index + 1);
                newLine = body + line.ToString() + ");";

                readyToWrite.Add(indentLine + newLine);
                readyToWrite.Add("");
            }

            List<string> finalLines = new List<string>();
            //add head
            for (int i = 0; i < insertLineNumber; i++)
            {
                finalLines.Add(targetFileLines[i]);
            }
            //add content
            finalLines.AddRange(readyToWrite);
            //add tail
            for (int i = insertLineNumber; i < targetFileLines.Count; i++)
            {
                finalLines.Add(targetFileLines[i]);
            }

            //output
            StringBuilder builder2 = new StringBuilder();
            foreach (var item in finalLines)
            {
                builder2.Append(item + Environment.NewLine);
            }
            File.WriteAllText(targetFile, builder2.ToString(), Encoding.UTF8);

            foreach (ParamLine item in paramLines)
            {
                for (int i = 0; i < item._params.Count; i++)
                {
                    Param p = item._params[i];

                    if (item._params.Count == 1 || i == item._params.Count - 1)
                    {
                        Console.Write(p.VarType + " " + p.VarName);
                    }
                    else
                    {
                        Console.Write(p.VarType + " " + p.VarName + ", ");
                    }
                }

                Console.WriteLine();
            }

            Console.WriteLine("Success!");
            //Console.ReadKey();
        }
    }
}
