using fix;
using System.Diagnostics;

class Program
{

    static string curCommand = "";
    public static Modes curMode = Modes.Commands;
    public static Screens curScreen = Screens.Starting;
    public static List<string> infoBarValues = new List<string>();
    public static Dictionary<string, FileTemp> curFiles = new Dictionary<string, FileTemp>();
    public static string curFileName;
    public static string[] curExplorerFiles;
    public static string[] curExplorerDirs;
    public static int curExplorerInd = 0;
    public static int curExplorerType = 0;
    public static string curDir = Directory.GetCurrentDirectory();
    public static int dirIndent = 0;
    public static int expOffset = 0;
    public static readonly int endFilePos = 4;
    public static readonly int barPos = 3;
    public static readonly int commandPos = 2;
    public static readonly int infoPos = 1;
    public static string selectedItem;

    public static string[] validCommands;

    public static string[] startCommands = new string[]
    {
        "fe",
        "q",
        "lf",
        "help",
        "rf",
        "gf",
        "cr",
    };

    public static string[] fileCommands = new string[]
    {
        "fe",
        "q",
        "mw",
        "s",
        "fs",
        "fsa",
        "lf",
        "gf",
        "rf",
        "help",
        "cr",
        "run",
        "build",
        "br",
    };

    public static string[] explorerCommands = new string[]
    {
        "s",
        "nf",
        "nd",
        "me",
        "q",
        "lf",
        "gf",
        "rf",
        "help",
        "cr",
        "build",
    };

    public static string fmtFileSize(long size)
    {
        if (size < 1_024)
        {
            return size.ToString() + "b";
        }
        else if (size < 1_048_576)
        {
            return (size / 1024).ToString() + "kb";
        }
        else if (size < 1_073_741_824)
        {
            return (size / 1_048_576).ToString() + "mb";
        }
        else if (size < 1_099_511_627_776)
        {
            return (size / 1_073_741_824).ToString() + "gb";
        }
        return size.ToString();
    }

    public static void disStartScreen()
    {
        int height = Console.WindowHeight;

        Console.SetCursorPosition(Console.WindowWidth / 2 - 7, 1); // Try to center
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Welcome to FIX");
        Console.ForegroundColor = ConsoleColor.Gray;

        Console.SetCursorPosition(0, 3);
        Console.WriteLine("fe\t\t\t\tBring up file explorer.");
        Console.WriteLine("nf\t(filename)\t\tBring up a new file with compulsory file name.");
        Console.WriteLine("q\t\t\t\tQuits the program.");
        Console.WriteLine("help\t\t\t\tShows all commands.");
        Console.WriteLine("rf\t(filename | index)\tRemoves a file from currently open files.");
        Console.WriteLine("gf\t(filename | index)\tGoto a specified open file.");
        Console.WriteLine("cr\t\t\t\tRe-initializes the config.");

        regularBar();

        validCommands = startCommands;
        Console.SetCursorPosition(0, height - commandPos);
    }

    public static void regularBar()
    {
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        string bar = "";

        for (int x = 0; x < width; x++) bar += "=";

        Console.SetCursorPosition(0, height - barPos);
        Console.Write(bar);
    }

    public static void infoBar()
    {
        int index;
        string extension;
        int height = Console.WindowHeight;

        Console.SetCursorPosition(0, height - infoPos);
        for (int i = 0; i < infoBarValues.Count; i++)
        {
            index = infoBarValues[i].IndexOf('.');
            if (index != -1)
            {
                extension = infoBarValues[i].Substring(index);

                if (Configure.fileColors.ContainsKey(extension))
                {
                    Console.ForegroundColor = (ConsoleColor)Configure.fileColors[extension];
                }
            }

            Console.Write(infoBarValues[i] + ' ');

            Console.ForegroundColor = ConsoleColor.Gray;
        }
        Console.SetCursorPosition(0, height - commandPos);
    }

    public static void clearLine(int line)
    {
        int height = Console.WindowHeight;
        int width = Console.WindowWidth;
        string bar = "";

        if (line < 0) line = height + line;

        for (int x = 0; x < width; x++) bar += " ";

        Console.SetCursorPosition(0, line);
        Console.Write(bar);
        Console.SetCursorPosition(0, line);
    }

    public static void clearLine(int line, int amt)
    {
        int height = Console.WindowHeight;
        int width = Console.WindowWidth;
        string bar = " ";

        if (line < 0) line = height + line;

        for (int x = 0; x < amt + 1 && x < width; x++) bar += " ";

        Console.SetCursorPosition(0, line);
        Console.Write(bar);
        Console.SetCursorPosition(0, line);
    }

    public static void displayDir()
    {
        Console.Clear();
        regularBar();

        infoBarValues = new List<string>() { curDir };
        infoBar();

        Console.SetCursorPosition(0, 0);

        int max = 0;
        int height = Console.WindowHeight - endFilePos;
        for (int i = expOffset; i < curExplorerDirs.Length && i - expOffset < height; i++)
        {
            Console.WriteLine(curExplorerDirs[i]);
            max = curExplorerDirs[i].Length > max ? curExplorerDirs[i].Length : max;
        }

        dirIndent = max + Configure.explorerGap;

        int index;
        string extension;
        for (int i = expOffset; i < curExplorerFiles.Length && i - expOffset < height; i++)
        {
            Console.SetCursorPosition(dirIndent, i - expOffset);

            // Colouring
            index = curExplorerFiles[i].IndexOf('.');
            if (index != -1)
            {
                extension = curExplorerFiles[i].Substring(index);

                if (Configure.fileColors.ContainsKey(extension))
                {
                    Console.ForegroundColor = (ConsoleColor)Configure.fileColors[extension];
                }
            }

            Console.Write(curExplorerFiles[i]);

            max = Console.CursorLeft > max ? Console.CursorLeft : max;

            Console.ForegroundColor = ConsoleColor.Gray;
        }

        int subIndent = max + Configure.explorerSubGap;

        if (Configure.showSizeFile)
        {
            long size;
            for (int i = expOffset; i < curExplorerFiles.Length && i - expOffset < height; i++)
            {
                size = new FileInfo(curDir + "\\" + curExplorerFiles[i]).Length;
                Console.SetCursorPosition(subIndent, i - expOffset);
                Console.Write(fmtFileSize(size));
            }
        }

        Console.SetCursorPosition(0, 0);
    }

    private static void WritingInstruct()
    {
        int left = Console.CursorLeft;
        int top = Console.CursorTop;
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        ConsoleKeyInfo curKeyInfo = Console.ReadKey(true);
        char curChar = curKeyInfo.KeyChar;
        FileTemp curFile = curFiles[curFileName];

        if (curChar == '\r')
        {
            height -= endFilePos;

            curFile.clearFile(curFile.curLine + 1, curFile.lines.Count);
            curFile.curLine++;
            curFile.lines.Insert(curFile.curLine, 0);
            curFile.data.Insert(curFile.curLine, "");
            curFile.drawFile(curFile.curLine - curFile.offset[1], curFile.lines.Count - curFile.offset[1]);

            if (left != curFile.lines[curFile.curLine - 1])
            {
                string temp = curFile.data[curFile.curLine - 1].Substring(left);
                curFile.data[curFile.curLine] = temp;
                curFile.data[curFile.curLine - 1] = curFile.data[curFile.curLine - 1].Substring(0, left);

                curFile.lines[curFile.curLine] = curFile.data[curFile.curLine].Length;
                curFile.lines[curFile.curLine - 1] = curFile.data[curFile.curLine - 1].Length;

                clearLine(curFile.curLine - 1 - curFile.offset[1]);
                clearLine(curFile.curLine - curFile.offset[1]);

                Console.SetCursorPosition(0, curFile.curLine - 1 - curFile.offset[1]);
                Console.WriteLine(curFile.data[curFile.curLine - 1]);
                Console.Write(curFile.data[curFile.curLine]);
            }

            if (curFile.curLine - curFile.offset[1] == height + 1)
            {
                curFile.clearFile(0, height + 1);
                curFile.offset[1]++;
                curFile.drawFile(0, height + 1);
                regularBar();
            }

            Console.SetCursorPosition(0, curFile.curLine - curFile.offset[1]);
        }
        else if (curChar == '\b')
        {
            if (left == 0)
            {
                // Start of file
                if (Console.CursorTop == 0) return;

                int tempLeft = curFile.lines[curFile.curLine - 1];

                curFile.lines[curFile.curLine - 1] += curFile.lines[curFile.curLine];
                curFile.data[curFile.curLine - 1] += curFile.data[curFile.curLine];

                if (curFile.offset[1] > 0)
                {
                    curFile.clearFile(0, curFile.lines.Count);
                    curFile.offset[1]--;

                    curFile.lines.RemoveAt(curFile.curLine);
                    curFile.data.RemoveAt(curFile.curLine);

                    curFile.drawFile(0, height);
                }
                else
                {
                    curFile.clearFile(curFile.curLine, curFile.lines.Count);

                    curFile.lines.RemoveAt(curFile.curLine);
                    curFile.data.RemoveAt(curFile.curLine);

                    curFile.drawFile(curFile.curLine - 1 - curFile.offset[1], height - endFilePos);
                }

                curFile.curLine--;

                Console.SetCursorPosition(tempLeft, curFile.curLine - curFile.offset[1]);
                return;
            }

            curFile.lines[curFile.curLine]--;
            curFile.data[curFile.curLine] = curFile.data[curFile.curLine].Substring(0, left - 1) + curFile.data[curFile.curLine].Substring(left);

            clearLine(curFile.curLine - curFile.offset[1]);
            Console.Write(curFile.data[curFile.curLine]);

            Console.SetCursorPosition(left - 1, Console.CursorTop);
        }
        else if (curChar == '(')
        {
            if (left != curFile.lines[curFile.curLine])
            {
                curFile.data[curFile.curLine] = curFile.data[curFile.curLine].Substring(0, left) + "()" + curFile.data[curFile.curLine].Substring(left);
                Console.SetCursorPosition(0, curFile.curLine);
                Console.Write(curFile.data[curFile.curLine]);
                Console.SetCursorPosition(left + 1, curFile.curLine);
            }
            else
            {
                curFile.data[curFile.curLine] += "()";
                Console.Write("()");
                Console.SetCursorPosition(left + 1, top);
            }
            curFile.lines[curFile.curLine] += 2;
        }
        else if (curChar == '[')
        {
            if (left != curFile.lines[curFile.curLine])
            {
                curFile.data[curFile.curLine] = curFile.data[curFile.curLine].Substring(0, left) + "[]" + curFile.data[curFile.curLine].Substring(left);
                Console.SetCursorPosition(0, curFile.curLine);
                Console.Write(curFile.data[curFile.curLine]);
                Console.SetCursorPosition(left + 1, curFile.curLine);
            }
            else
            {
                curFile.data[curFile.curLine] += "[]";
                Console.Write("[]");
                Console.SetCursorPosition(left + 1, top);
            }
            curFile.lines[curFile.curLine] += 2;
        }
        else if (curChar == '{')
        {
            if (left != curFile.lines[curFile.curLine])
            {
                curFile.data[curFile.curLine] = curFile.data[curFile.curLine].Substring(0, left) + "{}" + curFile.data[curFile.curLine].Substring(left);
                Console.SetCursorPosition(0, curFile.curLine);
                Console.Write(curFile.data[curFile.curLine]);
                Console.SetCursorPosition(left + 1, curFile.curLine);
            }
            else
            {
                curFile.data[curFile.curLine] += "{}";
                Console.Write("{}");
                Console.SetCursorPosition(left + 1, top);
            }
            curFile.lines[curFile.curLine] += 2;
        }
        else if (curChar == '\t')
        {
            if (left != curFile.lines[curFile.curLine])
            {
                curFile.data[curFile.curLine] = curFile.data[curFile.curLine].Substring(0, left) + "    " + curFile.data[curFile.curLine].Substring(left);
                Console.SetCursorPosition(0, curFile.curLine - curFile.offset[1]);
                Console.Write(curFile.data[curFile.curLine]);
                Console.SetCursorPosition(left + 4, curFile.curLine - curFile.offset[1]);
            }
            else
            {
                curFile.data[curFile.curLine] += "    ";
                Console.SetCursorPosition(left + 4, curFile.curLine - curFile.offset[1]);
            }
            curFile.lines[curFile.curLine] += 4;
        }
        else if (curChar == '\u0011') // ^q
        {
            curMode = Modes.Commands;
            Console.SetCursorPosition(0, Console.WindowHeight - 2);
        }
        else if (curChar == '\u0013') // ^s
        {
            if (curFile.selection == null)
            {
                curFile.selection = new int[2] { left, curFile.curLine };
            }
            else
            {
                int[] curPos = new int[2] { left, curFile.curLine };
                Console.BackgroundColor = Configure.selectionColor;

                clearLine(curFile.selection[1]);
                //Console.Write

                Console.BackgroundColor = Configure.selectionColor;

                for (int i = curFile.selection[1] + 1; i < curPos[1] - 1; i++)
                {
                    clearLine(i);
                    Console.Write(curFile.data[i]);
                }

                clearLine(curPos[1]);

                Console.BackgroundColor = ConsoleColor.Black;
                Console.SetCursorPosition(left, top);
            }
        }
        else if (curKeyInfo.Key == ConsoleKey.Delete)
        {
            if (left == curFile.lines[curFile.curLine])
            {
                if (curFile.lines.Count - 1 == curFile.curLine)
                {
                    return;
                }

                curFile.clearFile(curFile.curLine, curFile.lines.Count);

                curFile.data[curFile.curLine] += curFile.data[curFile.curLine + 1];
                curFile.data.RemoveAt(curFile.curLine + 1);
                curFile.lines[curFile.curLine] += curFile.lines[curFile.curLine + 1];
                curFile.lines.RemoveAt(curFile.curLine + 1);

                
                curFile.drawFile(curFile.curLine, curFile.lines.Count);
                clearLine(curFile.lines.Count);

                Console.SetCursorPosition(left, curFile.curLine);
            }
            else
            {
                curFile.data[curFile.curLine] = curFile.data[curFile.curLine].Remove(left, 1);
                curFile.lines[curFile.curLine]--;

                Console.SetCursorPosition(0, Console.CursorTop);
                clearLine(Console.CursorTop);
                Console.Write(curFile.data[curFile.curLine]);
                Console.SetCursorPosition(left, Console.CursorTop);
            }
        }
        else if (curKeyInfo.Key == ConsoleKey.Insert)
        {

        }
        else if (curKeyInfo.Key == ConsoleKey.End)
        {
            if (curKeyInfo.Modifiers == ConsoleModifiers.Control)
            {
                if (curFile.offset[1] < curFile.lines.Count - (height - endFilePos) - 1)
                {
                    curFile.clearFile(0, curFile.lines.Count);
                    curFile.offset[1] = curFile.lines.Count - (height - endFilePos) - 1;
                    curFile.drawFile(0, curFile.lines.Count);
                }

                curFile.curLine = curFile.lines.Count-1;
                Console.SetCursorPosition(curFile.lines[curFile.curLine], curFile.curLine - curFile.offset[1]);
            }
            else
            {
                Console.SetCursorPosition(curFile.lines[curFile.curLine], Console.CursorTop);
            }
        }
        else if (curKeyInfo.Key == ConsoleKey.Home)
        {
            if (curKeyInfo.Modifiers == ConsoleModifiers.Control)
            {
                if (curFile.offset[1] > 0)
                {
                    curFile.clearFile(0, curFile.lines.Count);
                    curFile.offset[1] = 0;
                    curFile.drawFile(0, curFile.lines.Count);
                }

                curFile.curLine = 0;
                Console.SetCursorPosition(0, 0);
            }
            else
            {
                Console.SetCursorPosition(0, Console.CursorTop);
            }
        }
        else if (curKeyInfo.Key == ConsoleKey.PageUp)
        {

        }
        else if (curKeyInfo.Key == ConsoleKey.PageDown)
        {

        }
        else if (curKeyInfo.Key == ConsoleKey.UpArrow)
        {
            if (curFile.curLine == 0) return;

            height -= endFilePos;
            curFile.curLine--;
            left = Console.CursorLeft;

            if (curFile.curLine - curFile.offset[1] <= -1)
            {
                curFile.clearFile(0, height + 1);
                curFile.offset[1]--;
                curFile.drawFile(0, height + 1);
                Console.SetCursorPosition(left, 0);
            }
            else
            {
                // Moving cursor to the correct position
                if (Console.CursorLeft > curFile.lines[curFile.curLine])
                {
                    Console.SetCursorPosition(curFile.lines[curFile.curLine], Console.CursorTop - 1);
                }
                else Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
            }
        }
        else if (curKeyInfo.Key == ConsoleKey.DownArrow)
        {
            if (curFile.curLine == curFile.lines.Count - 1) return;

            height = Console.WindowHeight - endFilePos + 1;
            curFile.curLine++;
            left = Console.CursorLeft;

            // Get the correct position
            if (Console.CursorLeft > curFile.lines[curFile.curLine])
            {
                Console.SetCursorPosition(curFile.lines[curFile.curLine], Console.CursorTop + 1);
            }
            else Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);

            if (Console.CursorTop >= height)
            {
                curFile.clearFile(0, height);
                curFile.offset[1]++;
                curFile.drawFile(0, height);
                Console.SetCursorPosition(left, height - 1);
            }
        }
        else if (curKeyInfo.Key == ConsoleKey.LeftArrow)
        {
            if (Console.CursorLeft == 0) return;

            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
        else if (curKeyInfo.Key == ConsoleKey.RightArrow)
        {
            if (Console.CursorLeft + curFile.offset[0] == curFile.lines[curFile.curLine]) return;

            if (Console.CursorLeft == width - 1)
            {
                curFile.offset[0]++;
                height -= endFilePos;
                Console.SetCursorPosition(0, 0);

                string line;
                for (int i = curFile.offset[1]; i < curFile.data.Count && i - curFile.offset[1] <= height; i++)
                {
                    clearLine(i - curFile.offset[1], curFile.lines[i]);
                    line = curFile.data[i].Substring(curFile.offset[0]);
                    if (line.Length > width) line = line.Substring(0, width);
                    Console.WriteLine(line);
                }

                Console.SetCursorPosition(left, top);
                return;
            }

            Console.SetCursorPosition(left + 1, top);
        }
        else
        {
            if (left != curFile.lines[curFile.curLine])
            {
                curFile.data[curFile.curLine] = curFile.data[curFile.curLine].Substring(0, left) + curChar + curFile.data[curFile.curLine].Substring(left);
                Console.SetCursorPosition(0, curFile.curLine - curFile.offset[1]);
                string line = curFile.data[curFile.curLine];
                if (line.Length > width) line = line.Substring(0, width);
                Console.Write(line);
                Console.SetCursorPosition(left+1, curFile.curLine - curFile.offset[1]);
            }
            else
            {
                curFile.data[curFile.curLine] += curChar;
                Console.Write(curChar);
            }
            curFile.lines[curFile.curLine]++;
        }
    }

    private static void ExploringInstruct()
    {
        ConsoleKeyInfo curKeyInfo = Console.ReadKey(true);
        char curChar = curKeyInfo.KeyChar;
        int height = Console.WindowHeight;
        int width = Console.WindowWidth;
        if (curChar == '\u0011') // ^q
        {
            curMode = Modes.Commands;
            Console.SetCursorPosition(0, Console.WindowHeight - commandPos);
        }
        else if (curChar == 'a')
        {
            if (curExplorerFiles.Length == 0) return;

            // Flip between 0 and 1
            curExplorerType--;
            if (curExplorerType < 0) curExplorerType = 1;

            curExplorerInd = 0;
            Console.SetCursorPosition(dirIndent * curExplorerType, 0);
        }
        else if (curChar == 'f')
        {
            curExplorerInd++;
            int length = curExplorerType == 0 ? curExplorerDirs.Length : curExplorerFiles.Length;

            if (curExplorerInd == length)
            {
                curExplorerInd = 0;
                if (expOffset != 0)
                {
                    expOffset = 0;
                    displayDir();
                }
            }
            if (curExplorerInd - expOffset == height - endFilePos)
            {
                expOffset++;
                displayDir();
            }
            Console.SetCursorPosition(dirIndent * curExplorerType, curExplorerInd-expOffset);
        }
        else if (curChar == 'e')
        {
            curExplorerInd--;
            int length = curExplorerType == 0 ? curExplorerDirs.Length : curExplorerFiles.Length;
            
            if (curExplorerInd == -1)
            {
                curExplorerInd = length - 1;
                if (curExplorerInd >= height - endFilePos)
                {
                    expOffset = curExplorerInd - height + endFilePos + 1;
                    displayDir();
                }
            }
            if (curExplorerInd - expOffset == -1)
            {
                expOffset--;
                displayDir();
            }
            Console.SetCursorPosition(dirIndent * curExplorerType, curExplorerInd-expOffset);
        }
        else if (curChar == '\r')
        {
            if (curExplorerType == 0)
            {
                if (curExplorerDirs[curExplorerInd] == "../")
                {
                    curDir = Directory.GetParent(curDir).FullName;
                }
                else
                {
                    curDir += "\\" + curExplorerDirs[curExplorerInd];
                }
                int start = curDir == "C:\\" ? 0 : 1;

                if (start == 1)
                {
                    curExplorerDirs = Directory.GetDirectories(curDir).Prepend("../").ToArray();
                }
                else
                {
                    curExplorerDirs = Directory.GetDirectories(curDir);
                }
                
                curExplorerFiles = Directory.GetFiles(curDir).ToArray();
                curExplorerInd = 0;

                for (int i = start; i < curExplorerDirs.Length; i++)
                {
                    curExplorerDirs[i] = curExplorerDirs[i].Substring(curDir.Length + start);
                }

                for (int i = 0; i < curExplorerFiles.Length; i++)
                {
                    curExplorerFiles[i] = curExplorerFiles[i].Substring(curDir.Length + start);
                }

                displayDir();
            }
            else if (curExplorerType == 1)
            {
                string fileName = curExplorerFiles[curExplorerInd];

                curScreen = Screens.File_Editing;
                validCommands = fileCommands;
                curMode = Modes.Commands;

                curFiles[fileName] = new FileTemp(fileName, curDir + "\\" + fileName);
                curFileName = fileName;
                curFiles[fileName].data = File.ReadAllLines(curDir + "\\" + fileName).ToList();
                curFiles[fileName].lines.Clear();

                Console.Clear();

                for (int i = 0; i < curFiles[fileName].data.Count; i++)
                {
                    curFiles[fileName].lines.Add(curFiles[fileName].data[i].Length);
                }

                FileTemp curFile = curFiles[fileName];
                curFile.drawFile(0, height);

                regularBar();
                infoBarValues.Add(fileName);
                infoBar();
            }
        }
        else if (curChar == 's')
        {
            if (curExplorerType == 0)
            {
                selectedItem = curExplorerDirs[curExplorerInd];
            }
            if (curExplorerType == 1)
            {
                selectedItem = curExplorerFiles[curExplorerInd];

                Console.Clear();

                int ind = curExplorerFiles[curExplorerInd].IndexOf(".");
                if (ind == -1)
                {
                    new Panic("This file doesn't have a file extension.");
                    return;
                }

                string extension = curExplorerFiles[curExplorerInd].Substring(ind);
                if (extension != ".exe")
                {
                    new Panic("File must be an exe.");
                    return;
                }

                string strCmdText = curDir + "\\" + curExplorerFiles[curExplorerInd];

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = strCmdText,
                        RedirectStandardInput = false,
                        UseShellExecute = true,
                        CreateNoWindow = false,
                    }
                };

                process.Start();
                process.WaitForExit();

                Console.WriteLine("Press any key to return.");
                Console.ReadKey(true);

                Console.Clear();
                infoBar();
                curMode = Modes.Commands;
                Console.SetCursorPosition(0, Console.WindowHeight - commandPos);
            }
        }
    }

    private static void CommandsInstruct()
    {
        ConsoleKeyInfo curKeyInfo = Console.ReadKey();
        char curChar = curKeyInfo.KeyChar;

        if (curChar == '\r')
        {
            string[] splitCommand = curCommand.Split(' ');

            if (!validCommands.Contains(splitCommand[0]))
            {
                new Panic("Invalid Command.");
                curCommand = "";
                return;
            }

            Commands.runCommand(splitCommand[0], splitCommand.Skip(1).ToArray());
            curCommand = "";
        }
        else if (curChar == '\b')
        {
            if (curCommand.Length == 0) return;

            curCommand = curCommand.Substring(0, curCommand.Length - 1);
            Console.Write(' ');
            Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop);
        }
        else curCommand += curChar;
    }

    public static void getConfig()
    {
        string[] files = Directory.GetFiles(".\\");

        if (!files.Contains(".\\config.txt")) return;

        string[] config = File.ReadAllLines("config.txt");

        Configure.Config(config);
    }

    private static void Main(string[] args)
    {
        getConfig();
        Console.Clear();
        disStartScreen();

        // Event loop
        bool running = true;
        while (running)
        {
            switch (curMode)
            {
                case Modes.Commands:
                    CommandsInstruct();
                    break;
                case Modes.Writing:
                    WritingInstruct();
                    break;
                case Modes.Exploring:
                    ExploringInstruct();
                    break;
            }
        }
    }

    public enum Modes
    {
        Writing,
        Commands,
        Exploring,
        Running
    }

    public enum Screens
    {
        Starting,
        File_Explorer,
        File_Editing,
        Running
    }
}