using System;
using System.Configuration;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
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
        static bool emailSent = false;
        int defaultHourOfSendingEmail = 18;
        public MainWindow()
        {
            InitializeComponent();
            Directory.CreateDirectory(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/ScreenShoots");
            DispatcherTimer dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += new EventHandler(HandleTickEvent);
            dispatcherTimer.Interval = new TimeSpan(0, 0, int.Parse(ConfigurationManager.AppSettings["ScreenShotInterval"]));
            dispatcherTimer.Start();
        }

        private void HandleTickEvent(object sender, EventArgs e)
        {
            try
            {
                DateTime now = DateTime.Now;
                if (bool.Parse(ConfigurationManager.AppSettings["WorkHours"]))
                {
                    DateTime startTime = DateTime.TryParse(ConfigurationManager.AppSettings["StartTime"], out var st) ? st : DateTime.MinValue.AddHours(9);
                    DateTime endTime = DateTime.TryParse(ConfigurationManager.AppSettings["EndTime"], out var et) ? et : DateTime.MinValue.AddHours(17);
                    if (now.Hour < startTime.Hour || (now.Hour == startTime.Hour && now.Minute < startTime.Minute)) return;
                    else if (now.Hour > endTime.Hour || (now.Hour == endTime.Hour && now.Minute > endTime.Minute))
                    {
                        if(getFilesNumber() > 0 && !emailSent) SendEmail(null, null);
                        return;
                    }
                    if (emailSent && now.Hour == startTime.Hour && now.Minute > startTime.Minute) {
                        emailSent = false;
                    }
                }
                else if (now.Hour == defaultHourOfSendingEmail && getFilesNumber() > 0 && !emailSent) {
                    SendEmail(null, null);
                }
                else if (now.Hour > defaultHourOfSendingEmail) {
                    emailSent = false;
                }
                TakeScreenShot();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"/Logi.txt", ex.ToString());
            }
        }

        private void TakeScreenShot()
        {
            var image = ScreenCapture.CaptureDesktop();
            image.Save(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $@"/ScreenShoots/{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.jpg", ImageFormat.Jpeg);
            bool makeSound = bool.TryParse(ConfigurationManager.AppSettings["MakeSound"], out var tmpSound) ? tmpSound : true;
            if (makeSound)
                PlaySound();
        }

        private void PlaySound()
        {
            var uri = new Uri(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/Resources/DARIO.mp3", UriKind.RelativeOrAbsolute);
            var player = new MediaPlayer();

            player.Open(uri);
            player.Play();
        }

        private static int getFilesNumber()
        {
            return Directory.GetFiles(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $@"/ScreenShoots").Length;
        }


        public static void SendEmail(object sender, EventArgs e)
        {
            try
            {
                string ToEmail = ConfigurationManager.AppSettings["ToMail"];
                string Subj = "Panie Areczku co Pan robi!!!";

                string templatePath = ConfigurationManager.AppSettings["TemplatePath"];
                string Message = "W załączniku znajdują się zrzuty ekranu z ostatniej doby.";
                if (File.Exists(templatePath))
                    Message = File.ReadAllText(templatePath)
                                .Replace(ReportTags.CREATE_DATE, DateTime.Now.ToString("dd.MM.yyyy"))
                                .Replace(ReportTags.SCREENSHOTS_TAKEN, getFilesNumber().ToString())
                                .Replace(ReportTags.WORK_STATION, Environment.MachineName);

                string screenshotsDirPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\ScreenShoots";
                string[] fileNames = Directory.GetFiles(screenshotsDirPath);

                //Reading sender Email credential from web.config file  

                string HostAdd = ConfigurationManager.AppSettings["Host"];
                string FromEmailid = ConfigurationManager.AppSettings["FromMail"];
                string Pass = ConfigurationManager.AppSettings["Password"];

                //creating the object of MailMessage  
                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(FromEmailid); //From Email Id  
                mailMessage.Subject = Subj; //Subject of Email  
                mailMessage.Body = Message; //body or message of Email  
                mailMessage.IsBodyHtml = true;

                string screenshotsZipPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\screenshots.zip";
                ZipFile.CreateFromDirectory(screenshotsDirPath, screenshotsZipPath);
                byte[] file = File.ReadAllBytes(screenshotsZipPath);
                foreach (var fileName in fileNames)
                    File.Delete(fileName);
                MemoryStream stream1 = new MemoryStream(file);
                mailMessage.Attachments.Add(new Attachment(stream1, "screenshots_" + Environment.MachineName + DateTime.Now.ToString("_ddMMyyy") + ".zip"));


                string[] ToMuliId = ToEmail.Split(',');
                foreach (string ToEMailId in ToMuliId)
                {
                    mailMessage.To.Add(new MailAddress(ToEMailId)); //adding multiple TO Email Id  
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

                File.Delete(screenshotsZipPath);
                emailSent = true;
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + $"/Logi.txt", ex.ToString());
            }
        }
    }
}

