using System;
using System.Windows;

namespace ReadVolumeFile
{
    class App : Application
    {

        [STAThread]
        static void Main(string[] args)
        {
            App app = new App();

            MainWindow window = new MainWindow();
            window.DataContext = new MainViewModel();

            app.Run(window);

        }

    }
}
