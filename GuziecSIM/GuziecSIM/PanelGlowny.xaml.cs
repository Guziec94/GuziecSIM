﻿using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using baza_danych_azure;
using klasa_zabezpieczen;

namespace GuziecSIM
{
    /// <summary>
    /// Logika interakcji dla klasy PanelGlowny.xaml
    /// </summary>
    public partial class PanelGlowny : Page
    {
        public string _login;
        public klucze _klucz;

        public List<Uzytkownik> lista = new List<Uzytkownik>();
        public List<Wiadomosc> archiwum = new List<Wiadomosc>();
        private Wiadomosc nowa = null;

        public PanelGlowny()
        {
            InitializeComponent();

            _login = Logowanie._login; // <-- Pobieram info ze strony Logowania
            _klucz = Logowanie._klucz;

            Application.Current.MainWindow.Width = 548;
            Application.Current.MainWindow.Title = "GuziecSIM - " + _login;

            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.AcceptsReturn = true;

            button1_Copy.ToolTip = "Zminimalizuj konwersację";
            button1_Copy1.ToolTip = "Zamknij konwersację";
            btnWyl.ToolTip = "Wyloguj się";
            btnUsuw.ToolTip = "Usuń konto";
            btnDod.ToolTip = "Dodaj kontakt";

            lista = baza_danych.pobierz_liste_kontaktow(_login);
            if (lista != null) pokazListeKontaktow(lista);

            wczytaj_wiadomosci();
            baza_danych.broker();
        }

        public void wczytaj_wiadomosci()
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                List<Wiadomosc> wiadomosci = baza_danych.sprawdzKrotkieWiadomosci(_login, _klucz);
                if (wiadomosci != null)
                {
                    foreach (Wiadomosc w in wiadomosci)
                    {
                        archiwum.Add(w);
                        foreach (var kontakt in kontakty.Items)
                        {
                            TextBlock login = ((kontakt as GroupBox).Content as ListBox).Items.GetItemAt(0) as TextBlock;
                            if (login.Text == w.nadawca)
                            {
                                login.Foreground = Brushes.Red;
                                pokazWiadom(w.nadawca);
                                break;
                            }
                        }
                    }
                    baza_danych.usunKrotkieWiadomosci(_login);
                }
            });
        }

        /* [FUNKCJA UKAZUJACA LISTĘ KONTAKTÓW OTRZYMANA W POSTACI LISTY] */
        public void pokazListeKontaktow(List<Uzytkownik> lista)
        {
            foreach (var kontakt in lista)
            {
                GroupBox group = new GroupBox();
                ListBox list = new ListBox();
                group.Content = list;

                list.BorderThickness = new Thickness(0);
                list.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);

                group.Width = 220;
                group.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)230, (byte)230, (byte)230));
                group.BorderThickness = new Thickness(0, 0, 0, 1);
                group.ToolTip = "Dwukrotne kliknięcie LPM rozpocznie konwersację";

                group.MouseLeave += Group_MouseLeave;

                TextBlock login = new TextBlock();

                login.Foreground = Brushes.Black;
                login.FontSize = 12;
                login.Text = kontakt.login;

                TextBlock imie = new TextBlock();

                imie.Foreground = Brushes.Gray;
                imie.FontSize = 10;
                imie.Text = kontakt.imie;

                TextBlock opis = new TextBlock();

                opis.Foreground = Brushes.LightGray;
                opis.FontSize = 10;
                opis.Text = kontakt.opis;
                opis.TextWrapping = TextWrapping.WrapWithOverflow;

                Button btnUsun = new Button();
                btnUsun.Content = "Usuń znajomego";
                btnUsun.Name = login.Text;
                btnUsun.MouseDoubleClick += BtnUsun_MouseDoubleClick;

                list.Items.Add(login);
                list.Items.Add(imie);
                list.Items.Add(opis);
                list.Items.Add(btnUsun);

                group.Name = login.Text;
                group.MouseDoubleClick += Group_MouseDoubleClick;

                kontakty.Items.Add(group);
            }
        }

        private void BtnUsun_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            string uzytkownikDoUsuniecia = (sender as Button).Name;

            MessageBox.Show("Usuńcie tego użytkownika ze znajomych zalogowanego");
        }

        /* [FUNKCJA KASUJĄCA NIEBIESKIE PODŚWIETLENIE Z ELEMENTÓW LISTY PO ZJECHANIU Z NICH KURSOREM] */
        private void Group_MouseLeave(object sender, EventArgs e)
        {
            ListBox lista = (sender as GroupBox).Content as ListBox;
            lista.UnselectAll();
        }

        private void pokazWiadom(string osoba)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate {
                okno.Items.Clear();
                foreach (var wiadomosc in archiwum)
                {
                    if (
                        (wiadomosc.nadawca == _login && wiadomosc.odbiorca == osoba) ||
                        (wiadomosc.odbiorca == _login && wiadomosc.nadawca == osoba)
                        )
                    {
                        GroupBox group = new GroupBox();
                        ListBox list = new ListBox();
                        group.Content = list;

                        list.BorderThickness = new Thickness(0);
                        list.Background = new SolidColorBrush(Color.FromArgb(0, (byte)0, (byte)0, (byte)0));

                        list.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);

                        group.Width = 220;
                        group.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)230, (byte)230, (byte)230));
                        group.BorderThickness = new Thickness(0);
                        group.Background = wiadomosc.nadawca == _login ? new SolidColorBrush(Color.FromArgb(255, (byte)111, (byte)163, (byte)99)) : new SolidColorBrush(Color.FromArgb(255, (byte)101, (byte)140, (byte)183));
                        group.MouseLeave += Group_MouseLeave;
                        group.Margin = new Thickness(0, 6, 0, 0);

                        TextBlock nadawca = new TextBlock();

                        nadawca.Foreground = Brushes.White;
                        nadawca.FontSize = 12;
                        nadawca.Text = wiadomosc.nadawca;

                        TextBlock czas = new TextBlock();

                        czas.Foreground = Brushes.White;
                        czas.FontSize = 10;
                        czas.Text = wiadomosc.czas.ToString();
                        czas.TextWrapping = TextWrapping.WrapWithOverflow;

                        TextBlock text = new TextBlock();

                        text.Foreground = Brushes.LightGray;
                        text.FontSize = 10;
                        text.Text = wiadomosc.Text;
                        text.TextWrapping = TextWrapping.WrapWithOverflow;

                        list.Items.Add(nadawca);
                        list.Items.Add(czas);
                        list.Items.Add(text);

                        okno.Items.Add(group);
                    }
                }
            });
        }

        /* [OTWARCIE OKNA KONWERSACJI Z INNYM UŻYTKOWNIKIEM] */
        private void Group_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            nowa = new Wiadomosc() { nadawca = _login, odbiorca = ((GroupBox)sender).Name };

            kontakty.UnselectAll();
            pokazWiadom(nowa.odbiorca);

            infoKonf.Content = nowa.odbiorca;

            if ((sender as GroupBox).Content != null)
            {
                TextBlock login = ((sender as GroupBox).Content as ListBox).Items.GetItemAt(0) as TextBlock;
                login.Foreground = new SolidColorBrush(Color.FromArgb(255, (byte)111, (byte)163, (byte)99));
            }

            textBox.IsEnabled = true;
            button1.IsEnabled = true;

            button1_Copy.Visibility = Visibility.Visible;
            button1_Copy1.Visibility = Visibility.Visible;
        }

        /* [PRÓBA WYSŁANIA WIADOMOŚCI] */
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            nowa.czas = DateTime.Now;
            nowa.Text = textBox.Text;

            baza_danych.wyslij_krotka_wiadomosc(nowa.nadawca, nowa.odbiorca, lista.Find(x => x.login == nowa.odbiorca).kluczPub, nowa.Text, DateTime.Now.AddDays(3));
            archiwum.Add(nowa);

            textBox.Clear();

            pokazWiadom(nowa.odbiorca);
        }

        /* [ZMINIMALIZOWANIE OKNA ROZMOWY] */
        private void button1_Copy_Click(object sender, RoutedEventArgs e)
        {
            infoKonf.Content = string.Empty;
            okno.Items.Clear();
            textBox.Clear();

            textBox.IsEnabled = false;
            button1.IsEnabled = false;

            nowa = null;

            button1_Copy.Visibility = Visibility.Hidden;
            button1_Copy1.Visibility = Visibility.Hidden;
        }

        /* [ZAMKNIECIE OKNA ROZMOWY] */
        private void button1_Copy1_Click(object sender, RoutedEventArgs e)
        {
            foreach (var kontakt in kontakty.Items)
            {
                TextBlock login = ((kontakt as GroupBox).Content as ListBox).Items.GetItemAt(0) as TextBlock;
                if (login.Text == infoKonf.Content.ToString()) { login.Foreground = Brushes.Black; break; }
            }

            for (int i = 0; i < archiwum.Count; i++)
            {
                if (archiwum[i].odbiorca == infoKonf.Content.ToString() || archiwum[i].nadawca == infoKonf.Content.ToString())
                {
                    archiwum.Remove(archiwum[i--]);
                    if (i < -1) i = -1;
                }
            }

            nowa = null;

            textBox.IsEnabled = false;
            button1.IsEnabled = false;

            button1_Copy.Visibility = Visibility.Hidden;
            button1_Copy1.Visibility = Visibility.Hidden;

            button1_Copy_Click(null, null);
        }

        /* [WYBRANO OPCJĘ WYLOGOWANIA SIĘ] */
        private void btnWyl_Click(object sender, RoutedEventArgs e)
        {
            baza_danych.usunAdresIP(_login);
            baza_danych.broker_stop();
            Application.Current.MainWindow.Title = "GuziecSIM";
            Logowanie logowanie = new Logowanie();
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(logowanie);
        }

        /* [WYBRANO OPCJĘ USUWANIA KONTA] */
        private void btnUsuw_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Czy na pewno chcesz usunąć swojek konto? Ta operacja jest nieodwracalna. Operację należy potwierdzić kluczem.", "Usuwanie konta - " + _login, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                klucze temp = new klucze();
                temp.zaladuj_z_pliku();
                if (temp.klucz_prywatny != null)
                {
                    if (baza_danych.sprawdz_dane(_login, temp))
                    {
                        baza_danych.usun_konto(_login, _klucz);
                        Application.Current.MainWindow.Title = "GuziecSIM";
                        Logowanie logowanie = new Logowanie();
                        NavigationService nav = NavigationService.GetNavigationService(this);
                        nav.Navigate(logowanie);
                    }
                    else
                    {
                        MessageBox.Show("Podano nie poprawny klucz, konto nie zostało usunięte.");
                    }
                }
            }
        }

        /* [WYBRANO OPCJĘ DODAWANIA KONTAKTU] */
        private void btnDod_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tutaj dodamy kontakt");
        }
    }
}