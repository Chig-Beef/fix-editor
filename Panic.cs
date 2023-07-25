namespace fix
{
    internal class Panic
    {
        public Panic(string message)
        {
            Console.Write(message + " (Press any key to continue)");
            Console.ReadKey();
            Program.clearCommandBar();
        }
    }
}
