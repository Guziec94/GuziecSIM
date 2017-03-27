using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using klasa_zabezpieczen;
using baza_danych_azure;
using System.Collections.Generic;
using System.Net;

namespace GuziecSIM
{
    /// <summary>
    /// Logika interakcji dla klasy Logowanie.xaml
    /// </summary>
    public partial class Logowanie : Page
    {
        public static string _login;
        public static klucze _klucz;
        public static PanelGlowny cos;
        public Logowanie()
        {
            InitializeComponent(); 

            label.ToolTip = "np. Guziec94";
            label1.ToolTip = "Klucz prywatny przypisany do konta";
            _login = null;
            _klucz = new klucze();

            baza_danych.polacz_z_baza();
        }

        /* [WCZYTYWANIE PLIKU Z KLUCZEM PRYWATNYM DO ZALOGOWANIA] */
        private void button_Click(object sender, RoutedEventArgs e)
        {
            _klucz.zaladuj_z_pliku();
            string fragmentKlucza = _klucz.klucz_prywatny;
            if (fragmentKlucza != null && fragmentKlucza.Length > 45)
            {
                label2.Content = fragmentKlucza.Substring(22, 23) + "...";
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
                if (!string.IsNullOrEmpty(_klucz.klucz_prywatny))
                {
                    if (baza_danych.sprawdz_dane(_login, _klucz))
                    {
                        baza_danych.wprowadzAdresIP(_login);
                        cos = new PanelGlowny();
                        NavigationService nav = NavigationService.GetNavigationService(this);
                        nav.Navigate(cos);
                    }
                    else
                    {
                        MessageBox.Show("Błąd logowania. Sprawdź dane!");
                    }
                }
                else button.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)242, (byte)202, (byte)202));
            }
            else textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)242, (byte)202, (byte)202));
        }

        /* [PRZEKIEROWANIE DO STRONY REJESTRACJI] */
        private void label3_MouseDown(object sender, MouseButtonEventArgs e)
        {
            Rejestracja rejestracja = new Rejestracja();

            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(rejestracja);
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

        /* [ZABLOKOWANIE MOŻLIWOŚCI WKLEJANIA DANYCH DO POLA LOGINU] */
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }

        /* [SPRAWDZANIE POPRAWNOŚCI WPROWADZANYCH ZNAKOW W POLU LOGINU] */
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((!dozwolone(e.Text, true) && textBox.Text.Length == 0) || (!dozwolone(e.Text, true) && textBox.SelectedText == textBox.Text) || znakiSpecjalne(e.Text)) e.Handled = true;
        }
    }
}
