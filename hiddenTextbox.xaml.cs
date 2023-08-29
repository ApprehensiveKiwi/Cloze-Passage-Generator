using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ASS_2025
{
    public partial class hiddenTextbox : UserControl
    {
        public int Counter = -1;
        WrapPanel Container;
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
            private string text;
            public string Text
            {
                get { return text; }
                set { SetField(ref text, value, "Text"); }
            }
        }
        public hiddenTextbox(string inputText,int counter, WrapPanel container)
        {
            InitializeComponent();
            Foreground = new SolidColorBrush(Colors.Red);
            var model = new Data()
            {
                Text = inputText,
            };
            this.DataContext = model;
            internalItem.Text = new string(' ', Name.Length);
            Counter = counter;
            Container = container;
        }
        void internalItem_TextChanged(object sender, TextChangedEventArgs e)
        {
            internalItem.IsEnabled = (internalItem.Text.ToUpper().Trim() == internalItem.Tag.ToString().ToUpper()) ? false : true;
            if (!internalItem.IsEnabled)
            {
                Foreground = new SolidColorBrush(Colors.Green);
                clozeSettings.removeWordBankValue(Tag.ToString(), Container);             
            }
        }
        private void TextBox_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(internalItem.Text == new string(' ', Name.Length))
            {
                internalItem.Text = "";
            }
        }
    }  
}
