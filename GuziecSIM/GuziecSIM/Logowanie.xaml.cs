using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using klasa_zabezpieczen;
using api_baza_danych;

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
            textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)230, (byte)230, (byte)230));
        }

        /* [ZAPOCZĄTKOWANIE PRÓBY LOGOWANIA] */
        private async void button1_Click(object sender, RoutedEventArgs e)
        {
            _login = textBox.Text;
            if (!string.IsNullOrEmpty(_login))
            {
                if (!string.IsNullOrEmpty(_klucz.klucz_prywatny))
                {
                    button1.IsEnabled = false;
                    if (baza_danych.sprawdz_dane(_login, _klucz))
                    {
                        bool czy_zalogowany = await baza_danych.czy_zalogowany();
                        if (czy_zalogowany == false)
                        {
                            baza_danych.ustaw_status(_login, true);//zmiana statusu uzytkownika na zalogowany
                            cos = new PanelGlowny();
                            NavigationService nav = NavigationService.GetNavigationService(this);
                            nav.Navigate(cos);
                        }
                        else
                        {
                            klasa_rozszerzen.balloon_tip("", "Użytkownik jest już zalogowany w systemie.");
                            button1.IsEnabled = true;
                        }
                    }
                    else
                    {
                        klasa_rozszerzen.balloon_tip("", "Błąd logowania. Sprawdź dane!");
                        button1.IsEnabled = true;
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
