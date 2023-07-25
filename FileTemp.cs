namespace fix
{
    internal class FileTemp
    {
        public int curLine;
        public string name;
        public string fullName;
        public int offset;

        public List<int> lines;
        public List<string> data;
        

        public FileTemp(string sName, string sFullName)
        {
            curLine = 0;
            offset = 0;
            lines = new List<int>() { 0 };
            data = new List<string>() { "" };
            name = sName;
            fullName = sFullName;
        }
    }
}
