using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using baza_danych_azure;
using klasa_zabezpieczen;
using System.Windows.Media.Imaging;
using System.Windows.Documents;

namespace GuziecSIM
{
    /// <summary>
    /// Logika interakcji dla klasy PanelGlowny.xaml
    /// </summary>
    public partial class PanelGlowny : Page
    {
        // TWORZYMY ZMIENNE W KTORYCH ZAPISANE ZOSTANA DANE AKTUALNIE ZALOGOWANEGO KONTA
        public string _login;
        public klucze _klucz;

        // TWORZYMY LISTY PRZECHOWUJACE KOLEJNO: ZNAJOMYCH ZALOGOWANEGO UZYTKOWNIKA, ARCHIWUM ROZMOW ZALOGOWANEGO UZYTKOWNIKA.
        // TWORZYMY ROWNIEZ ZMIENNA POMOCNICZA UZYWANA W CELU UTWORZENIA NOWEJ WIADOMOSCI
        public List<Uzytkownik> lista = new List<Uzytkownik>();
        public List<Wiadomosc> archiwum = new List<Wiadomosc>();
        private Wiadomosc nowa = null;

        public PanelGlowny()
        {
            InitializeComponent();

            // POBIERAMY DANE KONTA ZE STRONY LOGOWANIA
            _login = Logowanie._login;
            _klucz = Logowanie._klucz;

            // ZMIENIAMY TYTUL NA BELCE GORNEJ APLIKACJI UWZGLEDNIAJAC LOGIN ZALOGOWANEGO UZYTKOWNIKA
            Application.Current.MainWindow.Title = "GuziecSIM - " + _login;

            // DO LISTY KONTAKTOW I WIADOMOSCI DODAJEMY STYLE DZIEKI KTORYM ICH ZAWARTOSC NIE BEDZIE PODSWIETLANA PO NAJECHANIU
            okno.Style = (Style)Application.Current.Resources["listboxBezPodswietlen"];
            kontakty.Style = (Style)Application.Current.Resources["listboxBezPodswietlen"];

            // DO PRZYCISKOW DODANYCH STATYCZNIE DODAJEMY STYLE
            button1_Copy2.Style = (Style)Application.Current.Resources["ladnyPrzyciskStyle"];
            button1.Style = (Style)Application.Current.Resources["ladnyPrzyciskStyle"];

            // UMOZLIWIAMY BY POLE TEKSTOWE NA NOWA WIADOMOSC MOGLO PRZECHOWYWAC TEKST WIELOLINIOWY
            textBox.TextWrapping = TextWrapping.Wrap;
            textBox.AcceptsReturn = true;

            // DEFINIUJEMY WSZYSTKIE MOZLIWE NA TE CHWILE PODPOWIEDZI (DLA STATYCZNIE UTWORZONYCH KONTROLEK)
            button1_Copy.ToolTip = "Zminimalizuj konwersację";
            button1_Copy1.ToolTip = "Zamknij konwersację";
            btnWyl.ToolTip = "Wyloguj się";
            btnUsuw.ToolTip = "Usuń konto";
            btnDod.ToolTip = "Dodaj kontakt";

            baza_danych.powiadomOStatusieDodawania(_login);
            baza_danych.czyKtosChceDodacDoListy(_login);

            // POBIERAMY Z BAZY DANYCH LISTE ZNAJOMYCH AKTUALNIE ZALOGOWANEGO UZYTKOWNIKA
            lista = baza_danych.pobierz_liste_kontaktow(_login);

            // JEZELI UZYTKOWNIK POSIADA JAKICHS ZNAJOMYCH (POBRANA LISTA NIE JEST PUSTA) WYSWIETLAMY ICH
            if (lista != null)
                pokazListeKontaktow(lista, baza_danych.dostepni_uzytkownicy());

            baza_danych.rozglos_logowanie();

            // ROZPOCZYNAMY RAPORTOWANIE BAZY DANYCH O WPROWADZONYCH W NIEJ ZMIANACH
            baza_danych.broker();
            wczytaj_wiadomosci();
            if (archiwum.Count > 0)
            {
                System.Media.SystemSounds.Beep.Play();
            }
            okno.Dispatcher.Invoke(new Action(() => okno.Items.Clear()), System.Windows.Threading.DispatcherPriority.Normal);
        }

        public void wczytaj_wiadomosci()
        {
            // STOSUJEMY INVOKE ZE WZGLEDU NA TO ZE FUNKCJA RAPORTUJACA O ZMIANACH NA BAZIE DANYCH JEST WYWOLYWANA NA INNYM WATKU A MUSI MIEC MOZLIWOSC MODYFIKOWANIA STANU KONTROLEK INTERFEJSU
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                // POBIERAMY NOWYCH WIADOMOSCI ZWIAZANYCH Z ZALOGOWANYM UZYTKOWNIKIEM Z BAZY DANYCH
                List<Wiadomosc> wiadomosci = baza_danych.sprawdzKrotkieWiadomosci(_login, _klucz);
                if (wiadomosci != null)
                {
                    foreach (Wiadomosc w in wiadomosci)
                    {
                        // KAZDA ZE WCZYTANYCH NOWYCH WIADOMOSCI DODAJEMY DO ARCHIWUM (LISTY ZDEFINIOWANEJ U GORY)
                        archiwum.Add(w);

                        // ZNAJDUJEMY NA LISCIE ZNAJOMYCH NADAWCE WIADOMOSCI I JEGO KOLOR JEGO LOGINU ZMIENIAMY NA CZERWONY I ODSWIEZAMY OKNO KONWERSACJI JESLI NOWE WIADOMOSCI NALEZALY DO OSOBY Z KTORA AKTUALNIE ROZMAWIAL UZYTKOWNIK
                        foreach (var kontakt in kontakty.Items)
                        {
                            Run login = (((kontakt as GroupBox).Content as ListBox).Items.GetItemAt(0) as TextBlock).Inlines.FirstInline as Run;
                            pokazWiadom(login.Text);
                            if (login.Text == w.nadawca)
                            {
                                login.Foreground = Brushes.Red;
                                break;
                            }
                        }
                    }

                    // PO POBRANIU WIADOMOSCI Z BAZY DANYCH WYKONUJEMY OPERACJE USUWANIA ICH Z NIEJ
                    baza_danych.usunKrotkieWiadomosci(_login);
                }
            });
        }

        /* [FUNKCJA UKAZUJACA LISTĘ KONTAKTÓW OTRZYMANA W POSTACI LISTY] */
        public void pokazListeKontaktow(List<Uzytkownik> lista, List<string> online)
        {
            Application.Current.Dispatcher.Invoke((Action)delegate
            {
                // CZYSCIMY OKNO Z LISTA ZNAJOMYCH BY MOC JA POTEM UAKTUALNIC
                kontakty.Items.Clear();

                foreach (var kontakt in lista)
                {
                    // KAZDEGO UZYTKOWNIKA NA LISCIE ZNAJOMCH OPISUJE POJEDYNCZY GROUPBOX KTOREGO ZAWARTOSC TO LISTA DANYCH TAKICH JAK: LOGIN, IMIE, OPIS I PRZYCISK USUWAJACY TEGO ZNAJOMEGO Z LISTY
                    GroupBox group = new GroupBox();
                    ListBox list = new ListBox();
                    group.Content = list;

                    // KOLEJNO: USUWAMY OBRAMOWANIE DLA DANYCH UZYTKOWNIKA ZNAJDUJACYCH SIE WEWNATRZ GROUPBOXA, BLOKUJEMY MOZLIWOSC POJAWIENIA SIE POZIOMEGO PASKA PRZEWIJANIA, ORAZ NADAJEMY STYL DZIEKI KTOREMU SKLADOWE DANYCH UZYTKOWNIKA PO NAJECHANIU NIE BEDA PODSWIETLANE
                    list.BorderThickness = new Thickness(0);
                    list.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);
                    list.Style = (Style)Application.Current.Resources["listboxBezPodswietlen"];

                    // KOLEJNO: NADAJEMY SZYROKOSC GENEROWANEGO GROUPBOXA DLA DANEGO ZNAJOMEGO, ZMIENIAMY KOLOR JEGO OBRAMOWANIA NA JASNO-SZARY, USTAWIAMY JEGO OBRAMOWANIE NA TYLKO DOLNE (SEPARUJACE ZNAJOMYCH), DEFINIUJEMY PODPOWIEDZ PO NAJECHANIU NA ELEMENT ZNAJOMEGO
                    group.Width = 220;
                    group.BorderBrush = new SolidColorBrush(Color.FromArgb(255, (byte)230, (byte)230, (byte)230));
                    group.BorderThickness = new Thickness(0, 0, 0, 1);
                    group.ToolTip = "Dwukrotne kliknięcie LPM rozpocznie konwersację";

                    // DODAJEMY I STYLUJEMY ELEMENT BEDACY LOGINEM ZNAJOMEGO
                    TextBlock login = new TextBlock();

                    //login.Text = kontakt.login;
                    login.Foreground = Brushes.Black;
                    login.FontSize = 12;

                    login.Margin = new Thickness(0, 6, 0, 0);

                    //// OBOK LOGINU DODAJEMY KOLOROWE KOLKO SYGNALIZUJACE STATUS ZNAJOMEGO JESLI ZNAJDUJE SIE ON NA LISCIE ONLINE
                    login.Inlines.Add(new Run(kontakt.login));
                    login.Inlines.Add(new Run(" ✩") { Foreground = online.Contains(kontakt.login) ? new SolidColorBrush(Color.FromArgb(255, (byte)167, (byte)207, (byte)118)) : Brushes.DarkOrange, FontSize = 10 });

                    // DODAJEMY I STYLUJEMY ELEMENT BEDACY IMIENIEM ZNAJOMEGO
                    TextBlock imie = new TextBlock();

                    imie.Foreground = Brushes.Gray;
                    imie.FontSize = 10;
                    imie.Text = kontakt.imie;
                    imie.Margin = new Thickness(0, 0, 0, 0);

                    // DODAJEMY I STYLUJEMY ELEMENT BEDACY OPISEM ZNAJOMEGO
                    TextBlock opis = new TextBlock();

                    opis.Foreground = Brushes.LightGray;
                    opis.FontSize = 10;
                    opis.Text = kontakt.opis;
                    opis.TextWrapping = TextWrapping.WrapWithOverflow;
                    opis.Margin = new Thickness(0, 6, 0, 0);

                    // DODAJEMY I STYLUJEMY ELEMENT BEDACY PRZYCISKIEM USUWAJACYM ZNAJOMEGO
                    Button btnUsun = new Button();

                    btnUsun.BorderThickness = new Thickness(1, 0, 0, 1);
                    btnUsun.Margin = new Thickness(0, 6, 0, 0);

                    btnUsun.HorizontalAlignment = HorizontalAlignment.Left;
                    btnUsun.Width = 40;

                    btnUsun.Style = (Style)Application.Current.Resources["ladnyPrzyciskStyle"];
                    btnUsun.Foreground = new SolidColorBrush(Color.FromArgb(255, (byte)80, (byte)80, (byte)80));
                    btnUsun.Cursor = Cursors.Hand;
                    btnUsun.FontSize = 10;

                    btnUsun.Content = " Usuń ";
                    btnUsun.ToolTip = "Kliknięcie spowoduje usunięcie znajomego";

                    btnUsun.Click += BtnUsun_Click;
                    btnUsun.Name = (login.Inlines.FirstInline as Run).Text;

                    // WSZYSTKIE 3 ELEMENTY OPISUJACE ZNAJOMEGO ORAZ BUTTON USUWAJACY GO DODAJEMY DO LISTY BEDACEJ SZKIELETEM GROUPBOXA PRZYPISANEGO DO DANEGO UZYTKOWNIKA
                    list.Items.Add(login);
                    list.Items.Add(imie);
                    list.Items.Add(opis);
                    list.Items.Add(btnUsun);

                    // GENEROWANEMU GROUPBOXOWI NADAJEMY IDENTYFIKATOR BEDACY LOGINEM DANEGO ZNAJOMEGO DZIEKI CZEMU BEDZIEMY MOGLI UZYSKAC INFORMACJE O TYM JAKIEGO ZNAJOMEGO GROUPBOX KLIKNIETO W OBSLUDZE EVENTU ZDEFINIOWANEGO PONIZEJ
                    group.Name = (login.Inlines.FirstInline as Run).Text;
                    group.MouseDoubleClick += Group_MouseDoubleClick;

                    // DODAJEMY GROUPBOX UZUPELNIONY DANYMI ZNAJOMEGO DO LISTY KONTAKTOW
                    kontakty.Items.Add(group);
                }
            });
        }

        /* [FUNKCJA USUWAJACA UZYTKOWNIKA Z LISTY ZAJOMYCH] */
        private void BtnUsun_Click(object sender, RoutedEventArgs e)
        {
            // ODBIERAMY INFORMACJE O LOGINIE UZYTKOWNIKA KTORYCH CHCEMY USUNAC Z LISTY ZNAJOMYCH
            string uzytkownikDoUsuniecia = (sender as Button).Name;
            string tresc_powiadomienia = "Czy na pewno chcesz usunąć użytkownika " + uzytkownikDoUsuniecia + "? Po tej operacji nie będziecie mogli się komunikować.";
            if (MessageBox.Show(tresc_powiadomienia, "Usuwanie użytkownika - " + uzytkownikDoUsuniecia, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                // JESLI AKTUALNIE OTWARTA BYLA KONWERSACJA Z OSOBA KTORA USUWAMY Z LISTY ZNAJOMYCH TO KONWERSACJE ZAMYKAMY KORZYSTAJAC ZE ZDEFINIOWANEGO WCZESNIEJ EVENTU
                if (infoKonf.Content.ToString() == uzytkownikDoUsuniecia)
                    button1_Copy1_Click(null, null);
                else
                {
                    // NA LISCIE ZNAJOMYCH ZNAJDUJEMY USUWANEGO UZYTKOWNIKA I KOLOR JEGO LOGINU ZMIENIAMY SPOWEROTEM NA CZARNY
                    foreach (var kontakt in kontakty.Items)
                    {
                        Run login = (((kontakt as GroupBox).Content as ListBox).Items.GetItemAt(0) as TextBlock).Inlines.FirstInline as Run;
                        if (login.Text == uzytkownikDoUsuniecia) { login.Foreground = Brushes.Black; break; }
                    }

                    // USUWAMY Z ARCHIWUM WIADOMOSCI ZWIAZANE Z UZYTKOWNIKIEM KTOREGO USUWAMY
                    for (int i = 0; i < archiwum.Count; i++)
                    {
                        if (archiwum[i].odbiorca == uzytkownikDoUsuniecia || archiwum[i].nadawca == uzytkownikDoUsuniecia)
                        {
                            archiwum.Remove(archiwum[i--]);
                            if (i < -1) i = -1;
                        }
                    }
                }

                // USUWAMY UZYTKOWNIKA Z LISTY ZAWIERAJACEJ ZNAJOMYCH
                lista.Remove(lista.Find(x => x.login == uzytkownikDoUsuniecia));

                // UZYTKOWNIK O ODEBRANYM LOGINIE JEST USUWANY Z BAZY DANYCH Z LISTY ZNAJOMYCH ZALOGOWANEGO UZYTKOWNIKA
                baza_danych.lista_kontaktow_do_xml(lista, _login, false);
                List<Uzytkownik> lista_usuwanego = new List<Uzytkownik>();
                lista_usuwanego = baza_danych.pobierz_liste_kontaktow(uzytkownikDoUsuniecia);
                lista_usuwanego.Remove(lista_usuwanego.Find(x => x.login == _login));
                baza_danych.lista_kontaktow_do_xml(lista_usuwanego, uzytkownikDoUsuniecia, true);

                // ODSWIEZAMY OKNO ZAWIERAJACE LISTE ZNAJOMYCH
                pokazListeKontaktow(lista, baza_danych.dostepni_uzytkownicy());
            }
        }

        /* [FUNKCJA ODSWIEZAJACA OKNO KONWERSACJI (CZYSCI OKNO I WCZYTUJE WSZYSTKIE WIADOMOSCI KONWERSACJI Z LISTY ARCHIWUM)] */
        private void pokazWiadom(string osoba)
        {
            // STOSUJEMY INVOKE ZE WZGLEDU NA TO ZE FUNKCJA RAPORTUJACA O ZMIANACH NA BAZIE DANYCH JEST WYWOLYWANA NA INNYM WATKU A MUSI MIEC MOZLIWOSC MODYFIKOWANIA STANU KONTROLEK INTERFEJSU
            Application.Current.Dispatcher.Invoke((Action)delegate {

                // CZYSCIMY OKNO KONWERSACJI
                okno.Items.Clear();

                foreach (var wiadomosc in archiwum)
                {
                    // WYSWIETLAMY TYLKO TE WIADOMOSCI Z ARCHIWUM KTORE DOTYCZA ROZMOWY Z WYBRANYM WCZESNIEJ DO KONWERSACJI ZNAJOMYM
                    if (
                        (wiadomosc.nadawca == _login && wiadomosc.odbiorca == osoba) ||
                        (wiadomosc.odbiorca == _login && wiadomosc.nadawca == osoba)
                        )
                    {
                        // TWORZYMY GROUPBOX KTOREGO ZAWARTOSCIA BEDA DANE DOTYCZACE KONKRETNEJ WIADOMOSCI Z ARCHIWUM - JEGO SZKIELETEM BEDZIE LISTA ZAWIERAJACA: NADAWCE WIADOMOSCI, CZAS NADESLANIA, TRESC WIADOMOSCI
                        GroupBox group = new GroupBox();
                        ListBox list = new ListBox();
                        group.Content = list;

                        // KOLEJNO: USTAWIAMY BRAK OBRAMOWANIA DLA SZKIELETU WIADOMOSCI ZNAJDUJACEJ SIE W GROUPBOXIE, KOLOR SZKIELETU USTAWIAMY NA PRZEZROCZYSTY, NADAJEMY MU STYL TAK BY JEGO SKLADOWE NIE PODSWIETLALY SIE PO NAJECHANIU, BLOKUJEMY MOZLIWOSC POJAWIENIA SIE POZIOMEGO PASKA PRZEWIJANIA
                        list.BorderThickness = new Thickness(0);
                        list.Background = new SolidColorBrush(Color.FromArgb(0, (byte)0, (byte)0, (byte)0));
                        list.Style = (Style)Application.Current.Resources["listboxBezPodswietlen"];
                        list.SetValue(ScrollViewer.HorizontalScrollBarVisibilityProperty, ScrollBarVisibility.Disabled);

                        // KOLEJNO: USTAWIAMY SZEROKOSC GROUPBOXA DLA KONKRETNEJ WIADOMOSCI, USUWAM OBRAMOWANIA WIADOMOSCI, USTAWIAMY GORNY MARGINES ODDZIELAJACY WIADOMOSCI O 6PX, NADAJEMY KOLOR WIADOMOSC ZALEZNY OD TEGO CZY WIADOMOSC ZOSTALA WYSLANA CZY ODEBRANA
                        group.Width = 220;
                        group.BorderThickness = new Thickness(0);
                        group.Margin = new Thickness(0, 6, 0, 0);
                        group.Background = wiadomosc.nadawca == _login ? new SolidColorBrush(Color.FromArgb(255, (byte)111, (byte)163, (byte)99)) : new SolidColorBrush(Color.FromArgb(255, (byte)101, (byte)140, (byte)183));

                        // DODAJEMY I STYLUJEMY ELEMENT BEDACY NADAWCA WIADOMOSCI
                        TextBlock nadawca = new TextBlock();

                        nadawca.Foreground = Brushes.White;
                        nadawca.FontSize = 12;
                        nadawca.Text = wiadomosc.nadawca;
                        nadawca.Margin = new Thickness(0, 6, 0, 0);

                        // DODAJEMY I STYLUJEMY ELEMENT BEDACY CZASEM NADESLANIA WIADOMOSCI
                        TextBlock czas = new TextBlock();

                        czas.Foreground = Brushes.White;
                        czas.FontSize = 10;
                        czas.Text = wiadomosc.czas.ToString();
                        czas.TextWrapping = TextWrapping.WrapWithOverflow;

                        // DODAJEMY I STYLUJEMY ELEMENT BEDACY TRESCIA WIADOMOSCI
                        TextBlock text = new TextBlock();

                        text.Foreground = Brushes.LightGray;
                        text.FontSize = 10;
                        text.Text = wiadomosc.Text;
                        text.TextWrapping = TextWrapping.WrapWithOverflow;
                        text.Margin = new Thickness(0, 6, 0, 6);

                        // WSZYSTKIE DANE NA TEMAT WIADOMOSCI ZOSTAJA DODANE DO LISTY STANOWIACEJ SZKIELET GROUPBOXA
                        list.Items.Add(nadawca);
                        list.Items.Add(czas);
                        list.Items.Add(text);

                        // WIADOMOSC ZOSTAJE DODANA DO OKNA KONWERSACJI
                        okno.Items.Add(group);
                    }
                }
            });
        }

        /* [OTWARCIE OKNA KONWERSACJI Z INNYM UŻYTKOWNIKIEM] */
        private void Group_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // TWORZYMY NOWA WIADOMOSC KTOREJ ODBIORCA BEDZIE WYBRANY PRZEZ NAS DO KONWERSACJI ZNAJOMY
            nowa = new Wiadomosc() { nadawca = _login, odbiorca = ((GroupBox)sender).Name };

            // OTWIERAMY OKNO KONWERSACJI DLA WIADOMOSCI WYMIENIONYCH Z WYBRANYM ZNAJOMYM
            pokazWiadom(nowa.odbiorca);

            // ZMIENIAMY INFO O WYBRANEJ DO KONWERSACJI OSOBIE NAD JEJ OKNEM
            infoKonf.Content = nowa.odbiorca;

            // JEZELI FUNKCJA WYWOLANA ZOSTALA POPRZEZ WYBRANIE ZNAJOMEGO DO KONWERSACJI - ZMIENIAMY KOLOR JEGO LOGINU NA LISCIE ZNAJOMYCH NA ZIELONY
            if ((sender as GroupBox).Content != null)
            {
                Run login = (((sender as GroupBox).Content as ListBox).Items.GetItemAt(0) as TextBlock).Inlines.FirstInline as Run;
                login.Foreground = new SolidColorBrush(Color.FromArgb(255, (byte)111, (byte)163, (byte)99));
            }

            // UAKTYWNIAMY POLE TEKSTOWE NA TRESC NOWEJ WIADOMOSCI ORAZ PRZYCISK UMOZLIWIAJACY JEJ WYSLANIE
            textBox.IsEnabled = true;
            button1.IsEnabled = true;
            button1_Copy2.IsEnabled = true;

            // UKAZUJEMY PRZYCISK ZMINIMALIZOWANIA ORAZ ZAMKNIECIA OTWARTEJ KONWERSACJI
            button1_Copy.Visibility = Visibility.Visible;
            button1_Copy1.Visibility = Visibility.Visible;
        }

        /* [PRÓBA WYSŁANIA WIADOMOŚCI] */
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            // NADAJEMY WYSYLANEJ WIADOMOSCI AKTUALNA DATE I CZAS ORAZ TRESC POBRANA Z POLA TEKSTOWEGO
            nowa.czas = DateTime.Now;
            nowa.Text = textBox.Text;

            // WYSYLLAMY NOWA WIADOMOSC DO BAZY DANYCH
            baza_danych.wyslij_krotka_wiadomosc(nowa.nadawca, nowa.odbiorca, lista.Find(x => x.login == nowa.odbiorca).kluczPub, nowa.Text, DateTime.Now.AddDays(3));

            // NOWO WYSLANA WIADOMOSC JEST DODAWANA ROWNIEZ DO ARCHIWUM KONWERSACJI
            archiwum.Add(nowa);

            // CZYSCIMY POLE TEKSTOWE Z TRESCI WYSLANEJ JUZ WIADOMOSCI ORAZ ODSWIEZAMY OKNO KONWERSACJI UWZGLEDNIAJAC JUZ WYSLANA WIADOMOSC DODANA DO ARCHIWUM
            textBox.Clear();
            pokazWiadom(nowa.odbiorca);

            // PO WYSLANIU WIADOMOSCI TWORZYMY OBIEKT NOWEJ BY BYL GOTOWY NA WYSLANIE NASTEPNEJ - ODBIORCA JEST TEN SAM
            nowa = new Wiadomosc() { nadawca = _login, odbiorca = nowa.odbiorca };
        }

        /* [ZMINIMALIZOWANIE OKNA ROZMOWY] */
        private void button1_Copy_Click(object sender, RoutedEventArgs e)
        {
            // CHOWAMY INFORMACJE NA TEMAT OTWARTEJ KONWERSACJI
            infoKonf.Content = string.Empty;

            // CZYWSCIMY OKNO Z OTWARTA KONWERSACJA ORAZ POLE TEKSTOWE NA NOWA WIADOMOSC
            okno.Items.Clear();
            textBox.Clear();

            // BLOKUJEMY MOZLIWOSC WYSLANIA NOWEJ WIADOMOSCI
            textBox.IsEnabled = false;
            button1.IsEnabled = false;
            button1_Copy2.IsEnabled = false;
            nowa = null;

            // CHOWAMY PRZYCISKI ODPOWIEDZIALNE ZA MINIMALIZOWANIE I ZAMYKANIE KONWERSACJI
            button1_Copy.Visibility = Visibility.Hidden;
            button1_Copy1.Visibility = Visibility.Hidden;
        }

        /* [ZAMKNIECIE OKNA ROZMOWY] */
        private void button1_Copy1_Click(object sender, RoutedEventArgs e)
        {
            // NA LISCIE ZNAJOMYCH ZNAJDUJEMY UZYTKOWNIKA Z AKTUALNIE OTWARTEJ KONWERSACJI I KOLOR JEGO LOGINU ZMIENIAMY SPOWEROTEM NA CZARNY
            foreach (var kontakt in kontakty.Items)
            {
                Run login = (((kontakt as GroupBox).Content as ListBox).Items.GetItemAt(0) as TextBlock).Inlines.FirstInline as Run;
                if (login.Text == infoKonf.Content.ToString()) { login.Foreground = Brushes.Black; break; }
            }

            // USUWAMY Z ARCHIWUM WIADOMOSCI ZWIAZANE Z UZYTKOWNIKIEM Z KTORYM ZAMYKAMY KONWERSACJE
            for (int i = 0; i < archiwum.Count; i++)
            {
                if (archiwum[i].odbiorca == infoKonf.Content.ToString() || archiwum[i].nadawca == infoKonf.Content.ToString())
                {
                    archiwum.Remove(archiwum[i--]);
                    if (i < -1) i = -1;
                }
            }

            // CHOWAMY INFORMACJE NA TEMAT OTWARTEJ KONWERSACJI
            infoKonf.Content = string.Empty;

            // CZYSCIMY OKNO Z OTWARTA KONWERSACJA ORAZ POLE TEKSTOWE NA NOWA WIADOMOSC
            okno.Items.Clear();
            textBox.Clear();

            // UNIEMOZLIWIAMY WYSLANIE NOWYCH WIADOMOSCI
            textBox.IsEnabled = false;
            button1.IsEnabled = false;
            button1_Copy2.IsEnabled = false;
            nowa = null;

            // CHOWAMY PRZYCISKI ODPOWIEDZIALNE ZA MINIMALIZOWANIE I ZAMYKANIE KONWERSACJI
            button1_Copy.Visibility = Visibility.Hidden;
            button1_Copy1.Visibility = Visibility.Hidden;
        }

        /* [WYBRANO OPCJĘ WYLOGOWANIA SIĘ] */
        private void btnWyl_Click(object sender, RoutedEventArgs e)
        {
            // ZMIENIAMY SPOWROTEM TYTUL BELKI GORNEJ APLIKACJI NIE UWZGLEDNIAJAC TYM RAZEM ZADNEGO LOGINU
            Application.Current.MainWindow.Title = "GuziecSIM";

            // ZATRZYMUJEMY WYKONYWANE PRZEZ BAZE DANYCH
            baza_danych.rozglos_logowanie();
            baza_danych.usunAdresIP(_login);
            baza_danych.broker_stop();

            // PRZEKIEROWUJEMY UZYTKOWNIKA SPOWROTEM NA STORNE LOGOWANIA
            Logowanie logowanie = new Logowanie();
            NavigationService nav = NavigationService.GetNavigationService(this);
            nav.Navigate(logowanie);
        }

        /* [WYBRANO OPCJĘ USUWANIA KONTA] */
        private void btnUsuw_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Czy na pewno chcesz usunąć swoje konto? Ta operacja jest nieodwracalna. Operację należy potwierdzić kluczem.", "Usuwanie konta - " + _login, MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                // W CELU USUNIECIA KONTA MUSIMY POTWIERDZIC CHEC TEJ OPERACJI PODAJAC KLUCZ PRYWATNY DO KONTA
                klucze temp = new klucze();
                temp.zaladuj_z_pliku();

                if (temp.klucz_prywatny != null)
                {
                    // SPRAWDZAMY CZY PODANY KLUCZ PRYWATNY JEST POPRAWNY
                    if (baza_danych.sprawdz_dane(_login, temp))
                    {
                        // USUWAMY KONTO Z BAZY DANYCH
                        baza_danych.usun_konto(_login, _klucz);

                        // ZMIENIAMY SPOWROTEM TYTUL BELKI GORNEJ APLIKACJI NIE UWZGLEDNIAJAC TYM RAZEM ZADNEGO LOGINU
                        Application.Current.MainWindow.Title = "GuziecSIM";

                        // PRZEKIEROWUJEMY UZYTKOWNIKA SPOWROTEM NA STORNE LOGOWANIA
                        baza_danych.broker_stop();
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
            PanelDodawaniaZnajomego inputDialog = new PanelDodawaniaZnajomego();

            if (inputDialog.ShowDialog() == true)
            {
                if (inputDialog.Znajomy != _login)
                {
                    if (baza_danych.czyLoginIstnieje(inputDialog.Znajomy))
                    {
                        if (baza_danych.sprawdzListeKontaktow(_login, inputDialog.Znajomy, lista))
                        {
                            MessageBox.Show("Zaproszenie zostało wysłane.");
                            baza_danych.dodajDoListyOczekujacych(_login, inputDialog.Znajomy);
                        }
                        else
                        {
                            MessageBox.Show("Masz już użytkownika " + inputDialog.Znajomy + " na liscie kontaktów lub zaproszenie czeka na decyzje użytkownika!");
                        }
                    }
                    else MessageBox.Show("Użytkownik o nazwie '" + inputDialog.Znajomy + "' nie istnieje");
                }
                else MessageBox.Show("Nie możesz dodawać samego siebie.");
            }
        }

        /* [Button do naklejek] */
        private void button1_Copy2_Click(object sender, RoutedEventArgs e)
        {
            button1_Copy2.Content = "Abc";
        }
    }
}
