using System;
using System.Collections.Generic;
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
using klasa_zabezpieczen;
using Microsoft.Win32;
using System.IO;

namespace GuziecSIM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _login;
        private string _klucz;     

        public MainWindow()
        {
            InitializeComponent();

            label.ToolTip = "np. Guziec94";
            label1.ToolTip = "Klucz prywatny przypisany do konta";
        }

        /* [WCZYTYWANIE PLIKU Z KLUCZEM PRYWATNYM DO ZALOGOWANIA] */
        private void button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            if (openFileDialog.ShowDialog() == true)
            {
                label2.Content = openFileDialog.SafeFileName;
                button.Content = "Zmień";

                _klucz = File.ReadAllText(openFileDialog.FileName);
                button.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)230, (byte)230, (byte)230));
            }
        }

        /* [WYKRYTO WPROWADZANIE ZMIAN W POLU LOGINU] */
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _login = textBox.Text;
            textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)230, (byte)230, (byte)230));
        }

        /* [ZAPOCZĄTKOWANIE PRÓBY LOGOWANIA] */
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_login))
            {
                if (!string.IsNullOrEmpty(_klucz))
                {
                    MessageBox.Show("... Rozpoczynamy proces logowania");
                }
                else button.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)242, (byte)202, (byte)202));
            }
            else textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)242, (byte)202, (byte)202));
        }

        private void label3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("... Proces tworzenia konta");
        }
    }
}
