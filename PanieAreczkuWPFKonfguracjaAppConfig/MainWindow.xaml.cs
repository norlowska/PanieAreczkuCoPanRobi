using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PanieAreczkuWPFKonfguracjaAppConfig
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void SaveBtn_Click(object sender, RoutedEventArgs e)
        {
            string fullPath = System.IO.Path.GetDirectoryName(path: Assembly.GetExecutingAssembly().Location);
            List<string> listPath = fullPath.Split(System.IO.Path.DirectorySeparatorChar).ToList<string>();
            listPath[1] = "\\" + listPath[1];
            string path = System.IO.Path.Combine(listPath.GetRange(0, listPath.Count - 3).ToArray());

            path = System.IO.Path.Combine(path, "PanieAreczkuWPF\\App.config");

            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap();
            fileMap.ExeConfigFilename = path;

            Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            string emali = EmailTextBox.Text;
            string password = PasswordTextBox.Password;
            int min;
            int s;


            if (IsValidEmail(emali) && emali != "")
            {
                configuration.AppSettings.Settings["FromMail"].Value = emali;
                EmailTextBox.Background = Brushes.White;
            }
            else
            {
                EmailTextBox.Background = Brushes.Red;
                return;
            }

            if (password != "")
            {
                configuration.AppSettings.Settings["Password"].Value = password;
                PasswordTextBox.Background = Brushes.White;
            }
            else
            {
                PasswordTextBox.Background = Brushes.Red;
                return;
            }

            bool isNumericMin = int.TryParse(DelayMinTextbox.Text, out min);
            bool isNumericS = int.TryParse(DelaySTextbox.Text, out s);
            if (isNumericMin && isNumericS)
            {
                int value = min * 60 + s;
                if (value > 0)
                {
                    configuration.AppSettings.Settings["ScreenShotInterval"].Value = value.ToString();
                    DelayMinTextbox.Background = Brushes.White;
                    DelaySTextbox.Background = Brushes.White;
                }
                else
                {
                    DelayMinTextbox.Background = Brushes.Red;
                    DelaySTextbox.Background = Brushes.Red;
                    return;
                }
            }
            else
            {
                DelayMinTextbox.Background = Brushes.Red;
                DelaySTextbox.Background = Brushes.Red;
                return;
            }

            if ((bool)(SoundCheckBox.IsChecked))
            {
                configuration.AppSettings.Settings["MakeSound"].Value = "true";
            }
            else
            {
                configuration.AppSettings.Settings["MakeSound"].Value = "false";
            }

            configuration.Save(ConfigurationSaveMode.Modified);

            System.Windows.Application.Current.Shutdown();
        }

        bool IsValidEmail(string email)
        {
            if (email.Trim().EndsWith("."))
            {
                return false;
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

    }
}
