using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ASS_2025
{
    public partial class MainWindow : Window
    {
        int timeLeft = 0;
        public Data model = new();
        clozeSettings clozeSettingsInstance;
        public MainWindow(clozeSettings clozeSettings, List<(string, bool)> wordList,int timeAllowedSec,string userName, int wordBankSortMethod)
        { 
            InitializeComponent();
            clozeSettingsInstance = clozeSettings;

            nameBlock.Text = userName;
            timeLeft = timeAllowedSec;
            dateTextblock.Text = DateTime.Now.ToString("dd/MM/yyyy");
            clozeSettings.inputClozeValues(wordList, generationPreviewTextboxStudent, this,wordBankPreview);
            clozeSettings.inputWordBankValues(wordList, wordBankPreview, wordBankSortMethod);
            
            model = new Data()
            {
                TimeLeft = TimeSpan.FromSeconds(timeLeft).ToString("hh\\:mm\\:ss")
            };
            this.DataContext = model;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += timer_Tick;
            timer.Start();
        }
        public class Data : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected virtual void OnPropertyChanged(string propertyName)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            }
            protected bool SetField<T>(ref T field, T value, string propertyName)
           {
                if (EqualityComparer<T>.Default.Equals(field, value)) return false;
                field = value;
                OnPropertyChanged(propertyName);
                return true;
            }
            private string timeLeft;
            public string TimeLeft
            {
                get { return timeLeft; }
                set { SetField(ref timeLeft, value, "TimeLeft"); }
            }
        }
        void timer_Tick(object sender, EventArgs e)
        {
            timeLeft--;
            model.TimeLeft = ((timeLeft < 0) ? "Time Expired" : TimeSpan.FromSeconds(timeLeft).ToString("hh\\:mm\\:ss"));
        }
        private void exitToSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            clozeSettingsInstance.Show();
            this.Close();
        }
    }
}
