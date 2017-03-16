using System;
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
        private Wiadomosc nowa;

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
            if(lista.Count>0)
            {
                pokazListeKontaktow(lista);
            }
            List<string> wiadomosci = baza_danych.sprawdzKrotkieWiadomosci(_login, _klucz);
            if (wiadomosci != null)
            {
                foreach (string w in wiadomosci)
                {
                    MessageBox.Show(w);
                }
                baza_danych.usunKrotkieWiadomosci(_login);
            }
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

                list.Items.Add(login);
                list.Items.Add(imie);
                list.Items.Add(opis);

                group.Name = login.Text;
                group.MouseDoubleClick += Group_MouseDoubleClick;

                kontakty.Items.Add(group);
            }
        }

        /* [FUNKCJA KASUJĄCA NIEBIESKIE PODŚWIETLENIE Z ELEMENTÓW LISTY PO ZJECHANIU Z NICH KURSOREM] */
        private void Group_MouseLeave(object sender, EventArgs e)
        {
            ListBox lista = (sender as GroupBox).Content as ListBox;
            lista.UnselectAll();
        }

        /* [OTWARCIE OKNA KONWERSACJI Z INNYM UŻYTKOWNIKIEM] */
        private void Group_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            nowa = new Wiadomosc { nadawca = _login, odbiorca = ((GroupBox)sender).Name };

            okno.Items.Clear();
            kontakty.UnselectAll();
            foreach (var wiadomosc in archiwum)
            {
                if (
                    (wiadomosc.nadawca == _login && wiadomosc.odbiorca == nowa.odbiorca) ||
                    (wiadomosc.odbiorca == _login && wiadomosc.nadawca == nowa.odbiorca)
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

                    MessageBox.Show("Tutaj usuwamy z bazy danch aktualnie rozpatrywaną wiadomość ponieważ po otworzeniu konwersacji z użytkownikiem zostały one odczytane");
                }
            }

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

            if (true) // <-- Może warunek i dodanie wiadomosci do lokalnej listy archiwum po poprawnym dodaniu przez baze?
                MessageBox.Show("Tu wywołajcie metodę wysylajaca wiadomosc zapisana w zmiennej o nazwie nowa");

            nowa.czas = DateTime.Now;
            nowa.Text = textBox.Text;
            archiwum.Add(nowa);

            okno.Items.Clear();
            textBox.Clear();

            GroupBox g = new GroupBox(); g.Name = nowa.odbiorca;
            Group_MouseDoubleClick(g, null);
        }

        /* [ZMINIMALIZOWANIE OKNA ROZMOWY] */
        private void button1_Copy_Click(object sender, RoutedEventArgs e)
        {
            infoKonf.Content = string.Empty;
            okno.Items.Clear();
            textBox.Clear();

            textBox.IsEnabled = false;
            button1.IsEnabled = false;

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
                _klucz.zaladuj_z_pliku();
                if (baza_danych.sprawdz_dane(_login, _klucz))
                {
                    baza_danych.usun_konto(_login,_klucz);
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

        /* [WYBRANO OPCJĘ DODAWANIA KONTAKTU] */
        private void btnDod_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Tutaj dodamy kontakt");
        }
    }
}