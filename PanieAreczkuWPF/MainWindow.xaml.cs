using System;
using System.Drawing.Imaging;
using System.Reflection;
using System.Timers;
using System.Windows;
using System.Windows.Threading;
using System.IO;

namespace PanieAreczkuWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Timer timer;

        public MainWindow()
        {



            InitializeComponent();

            Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/ScreenShoots");
            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(MakeScreenShot);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 20);
            dispatcherTimer.Start();




        }


        private void MakeScreenShot(object sender, EventArgs e)
        {
            var image = ScreenCapture.CaptureDesktop();
            image.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $@"/ScreenShoots/{DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")}.jpg", ImageFormat.Jpeg);

        }
    }
}

