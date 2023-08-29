using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ASS_2025
{
    public partial class wordItem : UserControl
    {
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

            private string name;
            public string Name
            {
                get { return name; }
                set { SetField(ref name, value, "Name"); }
            }         
        }
        public wordItem(string inputName)
        {
            InitializeComponent();
            var model = new Data()
            {
                Name = inputName,
            };
            this.DataContext = model;
        }

        private void foundCheckbox_Checked()
        {

        }

        private void foundCheckbox_Checked_1(object sender, System.Windows.RoutedEventArgs e)
        {

        }
    }
}