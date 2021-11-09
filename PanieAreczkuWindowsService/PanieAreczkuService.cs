using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;

using System.Threading.Tasks;
using System.Timers;

namespace PanieAreczkuWindowsService
{
    public partial class PanieAreczkuService : ServiceBase
    {
        Timer timer;
        public PanieAreczkuService()
        {

            InitializeComponent();
          
        }

        protected override void OnStart(string[] args)
        {

            MakeScreenShot(null,null);
          timer = new Timer(double.Parse(ConfigurationManager.AppSettings["ScreenShotInterval"]) * 1000);

            timer.Elapsed += MakeScreenShot;
        }

        protected override void OnStop()
        {
        }

        private void MakeScreenShot(object sender, ElapsedEventArgs e)
        {
            try
            {
          var image = ScreenCapture.CaptureDesktop();
                image.Save(@"C:\temp\snippetsource.jpg", ImageFormat.Jpeg);
            }
            catch (Exception ex)
            {


                string filePath = @"C:\temp\Error.txt";

      

using (StreamWriter writer = new StreamWriter(filePath, true))
                {
                    writer.WriteLine("-----------------------------------------------------------------------------");
                    writer.WriteLine("Date : " + DateTime.Now.ToString());
                    writer.WriteLine();

                    while (ex != null)
                    {
                        writer.WriteLine(ex.GetType().FullName);
                        writer.WriteLine("Message : " + ex.Message);
                        writer.WriteLine("StackTrace : " + ex.StackTrace);

                        ex = ex.InnerException;
                    }
                }

            }
      
        }
    }
}
