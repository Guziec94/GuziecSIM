using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using klasa_zabezpieczen;
using baza_danych_azure;

namespace GuziecSIM
{
    /// <summary>
    /// Logika interakcji dla klasy Logowanie.xaml
    /// </summary>
    public partial class Logowanie : Page
    {
        private string _login;
        klucze _klucz = new klucze();

        public Logowanie()
        {
            InitializeComponent();

            label.ToolTip = "np. Guziec94";
            label1.ToolTip = "Klucz prywatny przypisany do konta";
        }

        /* [WCZYTYWANIE PLIKU Z KLUCZEM PRYWATNYM DO ZALOGOWANIA] */
        private void button_Click(object sender, RoutedEventArgs e)
        {
            _klucz.zaladuj_z_pliku();
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
                    MessageBox.Show("... Rozpoczynamy proces logowania");
                    baza_danych.polacz_z_baza();
                    baza_danych.sprawdzDaneLogowania(_login, _klucz);
                }
                else button.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)242, (byte)202, (byte)202));
            }
            else textBox.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)242, (byte)202, (byte)202));
        }

        private void label3_MouseDown(object sender, MouseButtonEventArgs e)
        {  
            Rejestracja rejestracja = new Rejestracja();

            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(rejestracja);
        }
    }
}
