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
    }
}
