using fix;
using System.Data;

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

    public static string[] validCommands;

    public static string[] startCommands = new string[]
    {
        "fe",
        //"nf",
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
        //"nf",
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
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;

        Console.SetCursorPosition(Console.WindowWidth / 2 - 7, 1);
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
        Console.SetCursorPosition(0, height - 2);
    }

    public static void regularBar()
    {
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        for (int x = 0; x < width; x++)
        {
            Console.SetCursorPosition(x, height - 3);
            Console.Write("=");
        }
    }

    public static void infoBar()
    {
        int index;
        string extension;
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        Console.SetCursorPosition(0, height - 1);
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
        Console.SetCursorPosition(0, height - 2);
    }

    public static void clearCommandBar()
    {
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        for (int x = 0; x < width; x++)
        {
            Console.SetCursorPosition(x, height - 2);
            Console.Write(" ");
        }
        Console.SetCursorPosition(0, height - 2);
    }

    public static void clearLine(int line)
    {
        int width = Console.WindowWidth;
        for (int x = 0; x < width; x++)
        {
            Console.SetCursorPosition(x, line);
            Console.Write(" ");
        }
        Console.SetCursorPosition(0, line);
    }

    public static void clearLine(int line, int amt)
    {
        for (int x = 0; x < amt + 1 && x < Console.WindowWidth; x++)
        {
            Console.SetCursorPosition(x, line);
            Console.Write(" ");
        }
        Console.SetCursorPosition(0, line);
    }

    public static void clearInfoBar()
    {
        int width = Console.WindowWidth;
        int height = Console.WindowHeight;
        for (int x = 0; x < width; x++)
        {
            Console.SetCursorPosition(x, height - 1);
            Console.Write(" ");
        }
        Console.SetCursorPosition(0, height - 1);
    }

    public static void displayDir()
    {
        Console.Clear();
        regularBar();

        clearInfoBar();
        infoBarValues = new List<string>() { curDir };
        infoBar();

        Console.SetCursorPosition(0, 0);

        int max = 0;

        int height = Console.WindowHeight - 4;
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
        ConsoleKeyInfo curKeyInfo = Console.ReadKey(true);
        char curChar = curKeyInfo.KeyChar;
        FileTemp curFile = curFiles[curFileName];
        int width = Console.WindowWidth;

        if (curChar == '\r')
        {
            int height = Console.WindowHeight - 4;

            Console.Write('\n');
            curFile.curLine++;
            curFile.lines.Insert(curFile.curLine, 0);
            curFile.data.Insert(curFile.curLine, "");

            clearLine(curFile.curLine - curFile.offset[1]);
            for (int i = curFile.curLine + 1; i < curFile.lines.Count; i++)
            {
                if (i - curFile.offset[1] >  height) break;

                clearLine(i - curFile.offset[1], curFile.lines[i]);
                Console.Write(curFile.data[i]);
            }

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
                Console.SetCursorPosition(0, 0);
                curFile.offset[1]++;

                for (int i = curFile.offset[1]; i < curFile.data.Count && i - curFile.offset[1] <= height; i++)
                {
                    clearLine(i - curFile.offset[1]);
                    Console.WriteLine(curFile.data[i]);
                }
            }

            Console.SetCursorPosition(curFile.lines[curFile.curLine], curFile.curLine - curFile.offset[1]);
        }
        else if (curChar == '\b')
        {
            if (left == 0)
            {
                // Start of file
                if (Console.CursorTop == 0)
                {
                    Console.SetCursorPosition(0, 0);
                    return;
                }

                curFile.lines[curFile.curLine - 1] += curFile.lines[curFile.curLine];
                curFile.data[curFile.curLine - 1] += curFile.data[curFile.curLine];

                curFile.lines.RemoveAt(curFile.curLine);
                curFile.data.RemoveAt(curFile.curLine);

                for (int i = curFile.curLine - 1; i < curFile.lines.Count; i++)
                {
                    clearLine(i);
                    Console.Write(curFile.data[i]);
                }
                clearLine(curFile.lines.Count);

                curFile.curLine--;

                Console.SetCursorPosition(curFile.lines[curFile.curLine], curFile.curLine);
                return;
            }

            curFile.lines[curFile.curLine]--;
            curFile.data[curFile.curLine] = curFile.data[curFile.curLine].Substring(0, left - 1) + curFile.data[curFile.curLine].Substring(left);

            clearLine(curFile.curLine);
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
                Console.Write("{}");
                Console.SetCursorPosition(left + 1, top);
                curFile.data[curFile.curLine] += "{}";
            }
            curFile.lines[curFile.curLine] += 2;
        }
        else if (curChar == '\t')
        {
            if (left != curFile.lines[curFile.curLine])
            {
                curFile.data[curFile.curLine] = curFile.data[curFile.curLine].Substring(0, left) + "    " + curFile.data[curFile.curLine].Substring(left);
                Console.SetCursorPosition(0, curFile.curLine);
                Console.Write(curFile.data[curFile.curLine]);
                Console.SetCursorPosition(left + 4, curFile.curLine);
            }
            else
            {
                curFile.data[curFile.curLine] += "    ";
                Console.SetCursorPosition(left + 4, curFile.curLine);
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
                curFile.data[curFile.curLine] += curFile.data[curFile.curLine + 1];
                curFile.data.RemoveAt(curFile.curLine + 1);
                curFile.lines[curFile.curLine] += curFile.lines[curFile.curLine + 1];
                curFile.lines.RemoveAt(curFile.curLine + 1);

                for (int i = curFile.curLine; i < curFile.lines.Count; i++)
                {
                    clearLine(i);
                    Console.Write(curFile.data[i]);
                }
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
                curFile.curLine = curFile.lines.Count-1;
                Console.SetCursorPosition(curFile.lines[curFile.curLine], curFile.curLine);
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
            int height = Console.WindowHeight - 4;

            if (curFile.curLine == 0) return;

            curFile.curLine--;

            left = Console.CursorLeft;

            if (Console.CursorLeft > curFile.lines[curFile.curLine])
            {
                Console.SetCursorPosition(curFile.lines[curFile.curLine], Console.CursorTop - 1);
            }
            else if (Console.CursorTop == 0) ;
            else Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);

            if (curFile.curLine - curFile.offset[1] < 0)
            {
                Console.SetCursorPosition(0, 0);
                curFile.offset[1]--;

                for (int i = curFile.offset[1]; i < curFile.data.Count && i - curFile.offset[1] <= height; i++)
                {
                    clearLine(i - curFile.offset[1], curFile.lines[i + 1]);
                    Console.WriteLine(curFile.data[i]);
                }
                Console.SetCursorPosition(left, 0);
            }
        }
        else if (curKeyInfo.Key == ConsoleKey.DownArrow)
        {
            int height = Console.WindowHeight - 4;

            if (curFile.curLine == curFile.lines.Count-1) return;

            curFile.curLine++;

            left = Console.CursorLeft;

            if (Console.CursorLeft > curFile.lines[curFile.curLine])
            {
                Console.SetCursorPosition(curFile.lines[curFile.curLine], Console.CursorTop + 1);
            }
            else Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop + 1);

            if (Console.CursorTop > height)
            {
                Console.SetCursorPosition(0, 0);
                curFile.offset[1]++;

                for (int i = curFile.offset[1]; i < curFile.data.Count && i - curFile.offset[1] <= height; i++)
                {
                    clearLine(i - curFile.offset[1], curFile.lines[i - 1]);
                    Console.WriteLine(curFile.data[i]);
                }
                Console.SetCursorPosition(left, height);
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

                int height = Console.WindowHeight - 4;
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
                Console.SetCursorPosition(0, curFile.curLine);
                string line = curFile.data[curFile.curLine];
                if (line.Length > width) line = line.Substring(0, width);
                Console.Write(line);
                Console.SetCursorPosition(left+1, curFile.curLine);
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
        if (curChar == '\u0011')
        {
            curMode = Modes.Commands;
            Console.SetCursorPosition(0, Console.WindowHeight - 2);
        }
        else if (curChar == 'a')
        {
            if (curExplorerFiles.Length == 0)
            {
                return;
            }
            curExplorerType--;
            if (curExplorerType < 0)
            {
                curExplorerType = 1;
            }
            curExplorerInd = 0;
            Console.SetCursorPosition(dirIndent * curExplorerType, 0);
        }
        else if (curChar == 'f')
        {
            curExplorerInd++;
            if (curExplorerType == 0)
            {
                if (curExplorerInd == curExplorerDirs.Length)
                {
                    curExplorerInd = 0;
                    if (expOffset != 0)
                    {
                        expOffset = 0;
                        displayDir();
                    }
                }
                if (curExplorerInd - expOffset == height - 4)
                {
                    expOffset++;
                    displayDir();
                }
            }
            else if (curExplorerType == 1)
            {
                if (curExplorerInd == curExplorerFiles.Length)
                {
                    curExplorerInd = 0;
                    if (expOffset != 0)
                    {
                        expOffset = 0;
                        displayDir();
                    }
                }
                if (curExplorerInd - expOffset == height - 4)
                {
                    expOffset++;
                    displayDir();
                }
            }
            Console.SetCursorPosition(dirIndent * curExplorerType, curExplorerInd-expOffset);
        }
        else if (curChar == 'e')
        {
            curExplorerInd--;
            
            if (curExplorerType == 0)
            {
                if (curExplorerInd == -1)
                {
                    curExplorerInd = curExplorerDirs.Length - 1;
                    if (curExplorerInd >= height - 4)
                    {
                        expOffset = curExplorerInd - height + 5;
                        displayDir();
                    }
                }
                if (curExplorerInd - expOffset == -1)
                {
                    expOffset--;
                    displayDir();
                }
            }
            else if (curExplorerType == 1)
            {
                if (curExplorerInd == -1)
                {
                    curExplorerInd = curExplorerFiles.Length - 1;
                    if (curExplorerInd >= height - 4)
                    {
                        expOffset = curExplorerInd - height + 5;
                        displayDir();
                    }
                }
                if (curExplorerInd - expOffset == -1)
                {
                    expOffset--;
                    displayDir();
                }
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

                string line;
                for (int i = 0; i < curFiles[fileName].data.Count && i < height - 5; i++)
                {
                    line = curFiles[fileName].data[i];
                    if (line.Length > width)
                    {
                        line = line.Substring(0, width);
                    }
                    Console.WriteLine(line);
                }

                regularBar();
                infoBarValues.Add(fileName);
                infoBar();
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