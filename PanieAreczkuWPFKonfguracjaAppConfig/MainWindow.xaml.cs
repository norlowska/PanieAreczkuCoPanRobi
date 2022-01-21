using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

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

            string email = EmailTextBox.Text;
            string password = PasswordTextBox.Password;
            int min, s;
            bool isInvalid = false;

            if (IsValidEmail(email) && email != "")
            {                
                AddOrReplaceConfigValue(ref configuration, "FromMail", email);
                EmailTextBox.Background = Brushes.White;
            }
            else
            {
                EmailTextBox.Background = Brushes.Red;
                isInvalid = true;
            }

            if (password != "")
            {
                AddOrReplaceConfigValue(ref configuration, "Password", password);
                PasswordTextBox.Background = Brushes.White;
            }
            else
            {
                PasswordTextBox.Background = Brushes.Red;
                isInvalid = true;
            }

            bool isNumericMin = int.TryParse(DelayMinuteTextBox.Text, out min);
            bool isNumericS = int.TryParse(DelaySecondTextBox.Text, out s);
            if (isNumericMin && isNumericS)
            {
                int value = min * 60 + s;
                if (value > 0)
                {
                    AddOrReplaceConfigValue(ref configuration, "ScreenShotInterval", value.ToString());
                    DelayMinuteTextBox.Background = Brushes.White;
                    DelaySecondTextBox.Background = Brushes.White;
                }
                else
                {
                    DelayMinuteTextBox.Background = Brushes.Red;
                    DelaySecondTextBox.Background = Brushes.Red;
                    isInvalid = true;
                }
            }
            else
            {
                DelayMinuteTextBox.Background = Brushes.Red;
                DelaySecondTextBox.Background = Brushes.Red;
                isInvalid = true;
            }


            if (SoundEffectCheckBox.IsChecked.HasValue && SoundEffectCheckBox.IsChecked.Value)
            {
                AddOrReplaceConfigValue(ref configuration, "MakeSound", "true");
            }
            else
            {
                AddOrReplaceConfigValue(ref configuration, "MakeSound", "false");
            }

            if (EnableWorkHoursCheckbox.IsChecked.HasValue && EnableWorkHoursCheckbox.IsChecked.Value)
            {
                AddOrReplaceConfigValue(ref configuration, "WorkHours", "true");
                if (IsValidWorkHours(out DateTime? startTime, out DateTime? endTime))
                {
                    AddOrReplaceConfigValue(ref configuration, "StartTime", startTime?.ToString("yyyy-MM-ddTHH:mm"));
                    AddOrReplaceConfigValue(ref configuration, "EndTime", endTime?.ToString("yyyy-MM-ddTHH:mm"));
                    WorkHoursStartHourTextBox.Background = WorkHoursStartMinuteTextBox.Background =
                        WorkHoursEndHourTextBox.Background = WorkHoursEndMinuteTextBox.Background = Brushes.White;
                }
                else
                {
                    WorkHoursStartHourTextBox.Background = WorkHoursStartMinuteTextBox.Background =
                        WorkHoursEndHourTextBox.Background = WorkHoursEndMinuteTextBox.Background = Brushes.Red;
                    isInvalid = true;
                }
            } else
                AddOrReplaceConfigValue(ref configuration, "WorkHours", "false");

            if (!isInvalid)
            {
                configuration.Save(ConfigurationSaveMode.Modified);
                System.Windows.Application.Current.Shutdown();
            }
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

        void HandleWorkHoursChange(object sender, TextChangedEventArgs e)
        {
            System.Windows.Controls.TextBox senderTB = sender as System.Windows.Controls.TextBox;
            if (senderTB.Name.Contains("Minute") && !string.IsNullOrEmpty(senderTB.Text))
            {
                string resultMinute = senderTB.Text;
                if (!int.TryParse(resultMinute, out int minutesValue))
                    resultMinute = Regex.Replace(senderTB.Text, "[^\\d]+", "");
                else if (minutesValue > 59)
                    resultMinute = "59";
                else if (minutesValue < 0)
                    resultMinute = "00";
                if (senderTB.Name.Contains("Start"))
                    WorkHoursStartMinuteTextBox.Text = resultMinute;
                else
                    WorkHoursEndMinuteTextBox.Text = resultMinute;
            }
            else if (!string.IsNullOrEmpty(senderTB.Text))
            {
                string resultHour = senderTB.Text;
                if (!int.TryParse(resultHour, out int hourValue))
                    resultHour = Regex.Replace(senderTB.Text, "[^\\d]+", "");
                else if (hourValue > 23)
                    resultHour = "23";
                else if (hourValue < 0)
                    resultHour = "00";
                if (senderTB.Name.Contains("Start"))
                    WorkHoursStartHourTextBox.Text = resultHour;
                else
                    WorkHoursEndHourTextBox.Text = resultHour;
            }
        }

        bool IsValidWorkHours(out DateTime? startTime, out DateTime? endTime)
        {
            startTime = null;
            endTime = null;
            if (string.IsNullOrEmpty(WorkHoursStartHourTextBox.Text) || string.IsNullOrEmpty(WorkHoursStartMinuteTextBox.Text) ||
                string.IsNullOrEmpty(WorkHoursEndHourTextBox.Text) || string.IsNullOrEmpty(WorkHoursEndMinuteTextBox.Text))
                return false;

            if (!int.TryParse(WorkHoursStartHourTextBox.Text, out int startHour) || !int.TryParse(WorkHoursStartMinuteTextBox.Text, out int startMinute) ||
                 !int.TryParse(WorkHoursEndHourTextBox.Text, out int endHour) || !int.TryParse(WorkHoursEndMinuteTextBox.Text, out int endMinute))
                return false;

            if (startHour > endHour || (startHour == endHour && startMinute >= endMinute))
                return false;

            startTime = new DateTime(1, 1, 1, startHour, startMinute, 0);
            endTime = new DateTime(1, 1, 1, endHour, endMinute, 0);
            return true;
        }

        void AddOrReplaceConfigValue(ref Configuration configuration, string key, string value)
        {
            if (configuration.AppSettings.Settings.AllKeys.Contains(key))
                configuration.AppSettings.Settings[key].Value = value;
            else
                configuration.AppSettings.Settings.Add(key, value);
        }
    }
}
