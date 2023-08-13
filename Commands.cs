using static Program;
using System.Diagnostics;

namespace fix
{
    internal class Commands
    {
        private static Dictionary<string, Func<string[], Panic>> allCommands = new Dictionary<string, Func<string[], Panic>>
        {
            { "fe" , openFileExplorer },
            { "nf" , createNewFile },
            { "nd" , createNewDirectory },
            { "q" , quitProgram },
            { "mw" , writeMode },
            { "me" , exploreMode },
            { "fs", saveFile },
            { "s", startScreen },
            { "fsa", saveAllFiles },
            { "lf", listFiles },
            { "gf", gotoFile },
            { "rf", removeFile },
            { "help", help },
            { "cr", runConfig },
            { "run", runRunner },
            { "build", runBuilder },
            { "br", buildRun },
        };

        public static void runCommand(string command, string[] args)
        {
            allCommands[command].Invoke(args);
        }

        private static Panic buildRun(string[] args)
        {
            Panic err = saveFile(args);
            if (err != null) return err;

            Console.Clear();

            int ind = curFileName.IndexOf(".");
            if (ind == -1)
            {
                return new Panic("This file doesn't have a file extension.");
            }

            string extension = curFileName.Substring(ind);

            if (!Configure.validRunners.ContainsKey(extension))
            {
                return new Panic("This filetype does not have a runner associated with it.");
            }

            return null;
        }

        private static Panic runBuilder(string[] args)
        {
            Panic err = saveFile(args);
            if (err != null) return err;

            Console.Clear();

            int ind = curFileName.IndexOf(".");
            if (ind == -1)
            {
                return new Panic("This file doesn't have a file extension.");
            }

            string extension = curFileName.Substring(ind);

            if (!Configure.validBuilders.ContainsKey(extension))
            {
                return new Panic("This filetype does not have a builder associated with it.");
            }

            string builder = Configure.validBuilders[extension][2];

            string strCmdText = builder + " " + curFiles[curFileName].fullName;

            if (Configure.validBuilders[extension][2] != "null")
            {
                string outName;
                int length = curFileName.Length - curFileName.IndexOf(".");

                outName = curFiles[curFileName].fullName.Substring(0, curFiles[curFileName].fullName.Length - length) + ".exe";

                strCmdText += " " + Configure.validBuilders[extension][2] + outName;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = Convert.ToBoolean(Configure.validBuilders[extension][1])
                }
            };

            process.Start();

            if (Configure.validBuilders[extension][0] == "dev")
            {
                process.StandardInput.WriteLine("\"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvarsall.bat\" amd64");
            }

            process.StandardInput.WriteLine(strCmdText);
            process.StandardInput.WriteLine("exit");

            process.WaitForExit();

            Console.WriteLine("Press any key to return.");
            Console.ReadKey(true);

            err = gotoFile(new string[] { curFileName });
            if (err != null) return err;

            return null;
        }

        private static Panic runRunner(string[] args)
        {
            Panic err = saveFile(args);
            if (err != null) return err;

            Console.Clear();

            int ind = curFileName.IndexOf(".");
            if (ind == -1)
            {
                return new Panic("This file doesn't have a file extension.");
            }

            string extension = curFileName.Substring(ind);

            if (!Configure.validRunners.ContainsKey(extension))
            {
                return new Panic("This filetype does not have a builder associated with it.");
            }

            string runner = Configure.validRunners[extension][2];

            string strCmdText = runner + " " + curFiles[curFileName].fullName;

            if (Configure.validRunners[extension][2] != "null")
            {
                string outName;
                int length = curFileName.Length - curFileName.IndexOf(".");

                outName = curFiles[curFileName].fullName.Substring(0, curFiles[curFileName].fullName.Length - length) + ".exe";

                strCmdText += " " + Configure.validRunners[extension][2] + outName;
            }

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = Convert.ToBoolean(Configure.validRunners[extension][1])
                }
            };

            process.Start();

            if (Configure.validRunners[extension][0] == "dev")
            {
                process.StandardInput.WriteLine("\"C:\\Program Files\\Microsoft Visual Studio\\2022\\Community\\VC\\Auxiliary\\Build\\vcvarsall.bat\" amd64");
            }

            process.StandardInput.WriteLine(strCmdText);
            process.StandardInput.WriteLine("exit");

            process.WaitForExit();

            Console.WriteLine("Press any key to return.");
            Console.ReadKey(true);

            err = gotoFile(new string[] { curFileName });
            if (err != null) return err;

            return null;
        }

        private static Panic runConfig(string[] args)
        {
            getConfig();
            clearLine(-commandPos);

            return null;
        }

        private static Panic help(string[] args)
        {
            Console.SetCursorPosition(0, 0);
            Console.Clear();
            Console.WriteLine(File.ReadAllText("help.txt"));
            Console.ReadKey();

            if (curScreen == Screens.File_Explorer)
            {
                displayDir();
                Console.SetCursorPosition(0, Console.WindowHeight - 2);
            }
            else if (curScreen == Screens.Starting)
            {
                Console.Clear();
                disStartScreen();
            }

            return null;
        }

        private static Panic listFiles(string[] args)
        {
            infoBarValues = new List<string>();
            foreach (KeyValuePair<string, FileTemp> item in curFiles)
            {
                infoBarValues.Add(item.Key);
            }
            clearLine(-commandPos);
            clearLine(-infoPos);
            infoBar();
            return null;
        }

        private static Panic removeFile(string[] args)
        {
            if (args.Length != 1)
            {
                return new Panic("1 argument is needed to find the file.");
            }

            bool isIndex = int.TryParse(args[0], out int index);

            if (isIndex)
            {
                if (index >= curFiles.Count || index < 0)
                {
                    return new Panic("Index was out of bounds.");
                }

                if (curFiles[curFileName] == curFiles.ElementAt(index).Value && curScreen == Screens.File_Editing)
                {
                    return new Panic("This is the currently open file.");
                }

                curFiles.Remove(curFiles.ElementAt(index).Key);

                listFiles(args);
                clearLine(-commandPos);

                return null;
            }

            else if (curFiles.ContainsKey(args[0]))
            {
                if (curFiles[curFileName] == curFiles[args[0]] && curMode == Modes.Writing)
                {
                    return new Panic("This is the currently open file.");
                }

                curFiles.Remove(args[0]);

                listFiles(args);
                clearLine(-commandPos);

                return null;
            }

            return new Panic("The argument given was not a valid int or not found in current files.");
        }

        private static Panic gotoFile(string[] args)
        {
            if (args.Length != 1)
            {
                return new Panic("1 argument is needed to find the file.");
            }

            bool isIndex = int.TryParse(args[0], out int index);

            string fileName;
            if (isIndex)
            {
                if (curFiles.Count < index)
                {
                    return new Panic("Index out of range.");
                }
                fileName = curFiles.ElementAt(index).Key;
            }
            else if (curFiles.ContainsKey(args[0]))
            {
                fileName = args[0];
            }
            else return new Panic("The argument given was not a valid int or not found in current files.");

            curScreen = Screens.File_Editing;
            validCommands = fileCommands;

            curFileName = fileName;
            FileTemp curFile = curFiles[fileName];

            Console.Clear();

            int height = Console.WindowHeight - endFilePos;
            curFile.drawFile(0, height);

            regularBar();
            infoBar();

            return null;
        }

        private static Panic openFileExplorer(string[] args)
        {
            curScreen = Screens.File_Explorer;
            curMode = Modes.Exploring;

            validCommands = explorerCommands;

            curExplorerDirs = Directory.GetDirectories(curDir).Prepend("../").ToArray();
            curExplorerFiles = Directory.GetFiles(curDir).ToArray();
            curExplorerInd = 0;
            curExplorerType = 0;

            for (int i = 1; i < curExplorerDirs.Length; i++)
            {
                curExplorerDirs[i] = curExplorerDirs[i].Substring(curDir.Length + 1);
            }
            for (int i = 0; i < curExplorerFiles.Length; i++)
            {
                curExplorerFiles[i] = curExplorerFiles[i].Substring(curDir.Length + 1);
            }

            displayDir();

            return null;
        }

        private static Panic createNewFile(string[] args)
        {
            if (args.Length != 1)
            {
                return new Panic("Either there was no filename given or there were too many arguments.");
            }

            string fileName = args[0];

            if (curFiles.ContainsKey(fileName) || curExplorerFiles.Contains(fileName))
            {
                return new Panic("Filename already exists.");
            }

            curScreen = Screens.File_Editing;
            validCommands = fileCommands;

            curFiles[fileName] = new FileTemp(fileName, curDir + "\\" + fileName);

            curFileName = fileName;

            Console.Clear();
            regularBar();
            infoBarValues.Add(fileName);
            infoBar();
            return null;
        }

        private static Panic createNewDirectory(string[] args)
        {

            Directory.CreateDirectory(curDir + "\\" + args[0]);

            curExplorerDirs = Directory.GetDirectories(curDir).Prepend("../").ToArray();
            curExplorerFiles = Directory.GetFiles(curDir).ToArray();

            for (int i = 1; i < curExplorerDirs.Length; i++)
            {
                curExplorerDirs[i] = curExplorerDirs[i].Substring(curDir.Length + 1);
            }
            for (int i = 0; i < curExplorerFiles.Length; i++)
            {
                curExplorerFiles[i] = curExplorerFiles[i].Substring(curDir.Length + 1);
            }

            clearLine(-infoPos);
            infoBar();
            displayDir();

            Console.SetCursorPosition(0, Console.WindowHeight - 2);

            return null;
        }

        private static Panic quitProgram(string[] args)
        {
            Console.Clear();
            Environment.Exit(0);
            return null;
        }

        private static Panic writeMode(string[] args)
        {
            int height = Console.WindowHeight;
            curMode = Modes.Writing;
            clearLine(-commandPos);

            if (curFiles[curFileName].curLine > height - endFilePos)
            {
                curFiles[curFileName].offset[1] = 
            }

            Console.SetCursorPosition(0, curFiles[curFileName].curLine);
            return null;
        }

        private static Panic exploreMode(string[] args)
        {
            curMode = Modes.Exploring;
            clearLine(-commandPos);
            Console.SetCursorPosition(0, 0);
            return null;
        }

        private static Panic saveFile(string[] args)
        {
            FileTemp file = curFiles[curFileName];

            using (StreamWriter sw = new StreamWriter(file.fullName))
            {
                for (int i = 0; i < file.data.Count; i++)
                {
                    sw.WriteLine(file.data[i]);
                }
            }

            clearLine(-commandPos);
            Console.SetCursorPosition(0, Console.WindowHeight - commandPos);

            return null;
        }

        private static Panic saveAllFiles(string[] args)
        {
            FileTemp file;
            foreach (var item in curFiles)
            {
                file = item.Value;
                using (StreamWriter sw = new StreamWriter(file.fullName))
                {
                    for (int i = 0; i < file.data.Count; i++)
                    {
                        sw.WriteLine(file.data[i]);
                    }
                }
            }

            clearLine(-commandPos);
            Console.SetCursorPosition(0, Console.WindowHeight - commandPos);

            return null;
        }

        private static Panic startScreen(string[] args)
        {
            Console.Clear();
            disStartScreen();
            return null;
        }
    }
}
