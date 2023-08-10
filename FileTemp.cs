namespace fix
{
    internal class FileTemp
    {
        public int curLine;
        public string name;
        public string fullName;
        public int[] offset;
        public int[] selection;

        public List<int> lines;
        public List<string> data;
        

        public FileTemp(string sName, string sFullName)
        {
            curLine = 0;
            offset = new int[2] { 0, 0 };
            lines = new List<int>() { 0 };
            data = new List<string>() { "" };
            name = sName;
            fullName = sFullName;
            selection = new int[] { 0, 0 };
        }

        public void drawFile(int start, int end)
        {
            int height = Console.WindowHeight - Program.endFilePos + 1;
            int width = Console.WindowWidth;

            Console.SetCursorPosition(0, start);

            end = end < data.Count ? end : data.Count;
            end = end < height ? end : height;

            string line;
            for (int i = offset[1] + start; i < end + offset[1]; i++)
            {
                line = data[i];
                if (line.Length > width)
                {
                    line = line.Substring(0, width);
                }
                Console.WriteLine(line);
            }
        }

        public void clearFile(int start, int end)
        {
            int height = Console.WindowHeight - Program.endFilePos + 1;

            end = end < data.Count ? end : data.Count;
            end = end < height ? end : height;

            for (int i = offset[1] + start; i < end + offset[1]; i++)
            {
                Program.clearLine(i - offset[1], lines[i]);
            }
        }
    }
}
