using System.Windows;

namespace WinGUI
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Exit(object sender, ExitEventArgs e)
        {
            if (Config.Document != null)
                Config.Document.Dispose();
        }
    }
}
