namespace fix
{
    internal class Configure
    {
        public static string[] validInstructions = new string[]
        {
            "color",
            "fileColor",
            "expGap",
            "showSize",
            "runners",
            "selectColor",
            "//",
        };

        public static Dictionary<string, int> fileColors = new Dictionary<string, int>();
        public static Dictionary<string, string[]> validRunners = new Dictionary<string, string[]>();
        public static Dictionary<string, string[]> validBuilders = new Dictionary<string, string[]>();

        public static int explorerGap = 8;
        public static int explorerSubGap = 4;
        public static bool showSizeFile = false;
        public static ConsoleColor selectionColor = ConsoleColor.DarkGray;

        public static void Config(string[] instructs)
        {
            fileColors = new Dictionary<string, int>();
            validRunners = new Dictionary<string, string[]>();
            validBuilders = new Dictionary<string, string[]>();

            Panic err;
            for (int i = 0; i < instructs.Length; i++)
            {
                err = Execute(instructs[i]);
            }
        }

        public static Panic Execute(string instruct)
        {
            if (instruct == "") return null;

            string[] temp = instruct.Split(' ');
            string start = temp[0];
            string[] args = temp.Skip(1).ToArray();
            bool result;
            int val, color;

            if (!validInstructions.Contains(start))
            {
                return new Panic("There was an invalid instruction in the config.");
            }

            switch (start)
            {
                case "color":
                    if (args.Length != 3)
                    {
                        return new Panic("Color arguement did not have enough parameters");
                    }
                    int r, g, b;
                    result = !int.TryParse(args[0], out r) || !int.TryParse(args[1], out g) || !int.TryParse(args[2], out b);
                    if (result)
                    {
                        return new Panic("One of the arguements was not a valid int.");
                    }
                    //Console.BackgroundColor = ConsoleColor.Green;
                    break;
                case "fileColor":
                    if (args.Length != 2)
                    {
                        return new Panic("Incorrect number of parameters.");
                    }

                    if (args[0][0] != '.')
                    {
                        return new Panic("Incorrect file extention.");
                    }

                    result = int.TryParse(args[1], out val);

                    if (!result)
                    {
                        return new Panic("Color arguement was not valid int.");
                    }

                    fileColors.Add(args[0], val);

                    break;
                case "expGap":
                    if (args.Length != 1)
                    {
                        return new Panic("Incorrect number of parameters.");
                    }

                    result = int.TryParse(args[0], out val);

                    if (!result)
                    {
                        return new Panic("Spacing arguement was not valid int.");
                    }

                    explorerGap = val;

                    break;
                case "showSize":
                    if (args.Length != 1)
                    {
                        return new Panic("Incorrect number of parameters.");
                    }
                    if (args[0] == "File")
                    {
                        showSizeFile = true;
                    }
                    break;
                case "runners":
                    if (args.Length != 1)
                    {
                        return new Panic("Incorrect number of parameters.");
                    }

                    if (!File.Exists(args[0]))
                    {
                        return new Panic("File path does not exist.");
                    }

                    return getRunners(args[0]);
                case "selectColor":
                    if (args.Length != 1)
                    {
                        return new Panic("Incorrect number of parameters");
                    }

                    result = int.TryParse(args[0], out color);

                    if (!result)
                    {
                        return new Panic("Color arguement was not valid int.");
                    }

                    selectionColor = (ConsoleColor)color;

                    break;
            }

            return null;
        }

        public static void setColor(string[] args)
        {

        }

        public static Panic getRunners(string fileName)
        {
            string[] runners = File.ReadAllLines(fileName);
            string[] runner;
            string cmd;

            /*
                runner[0] = run | build
                runner[1] = extension type
                runner[2] = command terminal type
                runner[3] = new terminal
                runner[4] = output flag
                runner[5] = command
             */

            for (int i = 0; i < runners.Length; i++)
            {
                runner = runners[i].Split(' ');

                if (runner.Length < 3)
                {
                    return new Panic("Incorrect number of arguements while loading runners.");
                }

                if (runner[1].IndexOf(".") != 0)
                {
                    return new Panic("File extension did not lead with \".\".");
                }

                switch (runner[2])
                {
                    case "cmd":
                        break;
                    case "dev":
                        break;
                    default:
                        return new Panic("Command prompter not valid.");
                }

                if (runner[3] != "true" && runner[3] != "false")
                {
                    return new Panic("New terminal prompt was not true or false.");
                }

                string outFlag = runner[4];
                outFlag = outFlag.Replace("<space>", " ");

                cmd = runner[5];
                for (int j = 6; j < runner.Length; j++)
                {
                    cmd += " " + runner[j];
                }

                if (runner[0] == "run")
                {
                    validRunners.Add(runner[1], new string[] { runner[2], runner[3], outFlag, cmd });
                }
                else if (runner[0] == "build")
                {
                    validBuilders.Add(runner[1], new string[] { runner[2], runner[3], outFlag, cmd });
                }
                else
                {
                    return new Panic("Need either keyword \"build\" or \"run\".");
                }
            }

            return null;
        }
    }
}
