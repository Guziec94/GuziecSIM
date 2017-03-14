using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using baza_danych_azure;

namespace GuziecSIM
{
    /// <summary>
    /// Logika interakcji dla klasy Rejestracja.xaml
    /// </summary>
    public partial class Rejestracja : Page
    {
        private string _login;
        private string _imie;
        private string _opis;

        public Rejestracja()
        {
            InitializeComponent();

            textBox2.TextWrapping = TextWrapping.Wrap;
            textBox2.AcceptsReturn = true;

            label.ToolTip = "np. Guziec94";
            label1.ToolTip = "Przedstaw się np. \"Jan\"";
            label2.ToolTip = "Napisz coś o sobie";
        }

        /* [PRZEKIEROWANIE DO STRONY LOGOWANIA] */
        private void label3_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Logowanie logowanie = new Logowanie();

            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(logowanie);
        }

        /* [ZAPOCZĄTKOWANIE PRÓBY REJESTRACJI] */
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(_login))
            {
                if (!string.IsNullOrEmpty(_imie))
                {
                    if (!string.IsNullOrEmpty(_opis))
                    {
                        if (baza_danych.zarejestruj_uzytkownika(textBox.Text, textBox1.Text, textBox2.Text))
                        {
                            Logowanie logowanie = new Logowanie();

                            NavigationService nav = NavigationService.GetNavigationService(this);
                            nav.Navigate(logowanie);
                        }
                    }
                    else textBox2.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)242, (byte)202, (byte)202));
                }
                else textBox1.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)242, (byte)202, (byte)202));
            }
            else textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)242, (byte)202, (byte)202));
        }

        /* [WYKRYTO WPROWADZANIE ZMIAN W POLU LOGINU] */
        private void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            _login = textBox.Text;
            textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)230, (byte)230, (byte)230));
        }

        /* [WYKRYTO WPROWADZANIE ZMIAN W POLU IMIENIA] */
        private void textBox1_TextChanged(object sender, TextChangedEventArgs e)
        {
            _imie = textBox1.Text;
            textBox1.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)230, (byte)230, (byte)230));
        }

        /* [WYKRYTO WPROWADZANIE ZMIAN W POLU OPISU] */
        private void textBox2_TextChanged(object sender, TextChangedEventArgs e)
        {
            _opis = textBox2.Text;
            textBox2.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)230, (byte)230, (byte)230));
        }

        /* [REGUŁY SPRAWDZAJĄCE CZY WPROWADZANE DANE TEKSTOWE SĄ DOZWOLONE] */
        private bool dozwolone(string text, bool cyfry = false)
        {
            string z = "żŻóÓłŁćĆęĘśŚąĄźŹńŃ ";
            string c = "0123456789";

            return !cyfry ? !(z.Contains(text) || znakiSpecjalne(text)) : !(z.Contains(text) || c.Contains(text) || znakiSpecjalne(text));
        }
        private bool znakiSpecjalne(string text)
        {
            string s = "!@#$%^&*()_-+={[}]|\\:;\"'<,>.?/";
            return s.Contains(text);
        }

        /* [SPRAWDZANIE POPRAWNOŚCI WPROWADZANYCH ZNAKOW W POLU LOGINU] */
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((!dozwolone(e.Text, true) && textBox.Text.Length == 0) || (!dozwolone(e.Text, true) && textBox.SelectedText == textBox.Text) || znakiSpecjalne(e.Text)) e.Handled = true;
        }

        /* [SPRAWDZANIE POPRAWNOŚCI WPROWADZANYCH ZNAKOW W POLU IMIENIA] */
        private void textBox1_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!dozwolone(e.Text, true)) e.Handled = true;
        }

        /* [ZABLOKOWANIE MOŻLIWOŚCI WKLEJANIA DANYCH DO POL LOGINU I IMIENIA] */
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}