using System;
using System.Data.SqlClient;
using System.Windows;
using klasa_zabezpieczen;
using System.Collections.Generic;
using GuziecSIM;
using System.Xml;
using System.Linq;
using System.Xml.Linq;
using System.Data;
using System.Threading.Tasks;

namespace api_baza_danych
{
    public class baza_danych
    {
        public static SqlConnection cnn;
        public static string connectionString = "Data Source=tcp:LENOVOY510P;Initial Catalog = GuziecSIMDB; User ID = Guziec94; Password=P@ssw0rd";
        //public static string connectionString = "Data Source=tcp:217.170.173.210,1433;Initial Catalog = GuziecSIMDB; User ID = Guziec94; Password=P@ssw0rd";

        public static void polacz_z_baza()
        {
            cnn = new SqlConnection(connectionString);
            try
            {
                cnn.Open();
            }
            catch (Exception)
            {
                klasa_rozszerzen.balloon_tip("", "Nie można nawiązać połączenia!");
            }
        }

        static SqlDependency dependency;
        public static async void broker()
        {
            SqlDependency.Start(connectionString);
            var _connection = new SqlConnection(connectionString);
            _connection.Open();
            SqlCommand _sqlCommand = new SqlCommand("SELECT [nowa_wiadomosc],[nowy_oczekujacy],[przeladuj_kontakty],[sprawdz_logowanie] FROM dbo.lista_zdarzen where login = @login", _connection);
            _sqlCommand.Parameters.AddWithValue("login", Logowanie._login);
            _sqlCommand.Notification = null;
            dependency = new SqlDependency(_sqlCommand);
            dependency.OnChange += broker_wykryl_zmiane;
            await _sqlCommand.ExecuteReaderAsync();
        }

        private static void broker_wykryl_zmiane(object sender, SqlNotificationEventArgs eventArgs)
        {
            if (eventArgs.Info == SqlNotificationInfo.Invalid)
            {
                Console.WriteLine("The above notification query is not valid.");
            }
            else
            {
                if (eventArgs.Info.ToString() == "Update")
                {
                    string query = "SELECT [nowa_wiadomosc],[nowy_oczekujacy],[przeladuj_kontakty],[sprawdz_logowanie] FROM dbo.lista_zdarzen where login = @login";
                    SqlCommand executeQuery = new SqlCommand(query, cnn);
                    executeQuery.Parameters.AddWithValue("login", Logowanie._login);
                    using (executeQuery)
                    {
                        using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                        {
                            if (readerQuery.Read())
                            {
                                bool if1 = readerQuery.GetSqlBoolean(0) == true ? true : false;
                                bool if2 = readerQuery.GetSqlBoolean(1) == true ? true : false;
                                bool if3 = readerQuery.GetSqlBoolean(2) == true ? true : false;
                                bool if4 = readerQuery.GetSqlBoolean(3) == true ? true : false;
                                readerQuery.Close();
                                if (if1)//nowa wiadomosc
                                {
                                    System.Media.SystemSounds.Beep.Play();
                                    Logowanie.cos.wczytaj_wiadomosci();//wywolanie funkcji wczytujacej wiadomosci
                                    query = "update lista_zdarzen set nowa_wiadomosc=0 where login = @login";//wyzerowanie eventu
                                    SqlCommand updateQuery = new SqlCommand(query, cnn);
                                    updateQuery.Parameters.AddWithValue("login", Logowanie._login);
                                    updateQuery.ExecuteNonQuery();
                                }
                                if (if2)//oczekujaca prosba o dodanie do znajomych
                                {
                                    int status = 0;
                                    query = "SELECT status FROM oczekujacy_znajomi WHERE (login_dodawanego = @login AND status = 1) OR (login_dodajacego = @login AND (status = 3 OR status = 4))";
                                    SqlCommand execute = new SqlCommand(query, cnn);
                                    execute.Parameters.AddWithValue("login", Logowanie._login);
                                    execute.ExecuteNonQuery();
                                    using (execute)
                                    {
                                        using (SqlDataReader readerStatus = execute.ExecuteReader())
                                        {
                                            if (readerStatus.Read())
                                            {
                                                status = readerStatus.GetInt32(0);
                                                readerStatus.Close();
                                            }
                                        }
                                    }
                                    if (status == 1)
                                    {
                                        czyKtosChceDodacDoListy(Logowanie._login);
                                    }
                                    else if (status == 3 || status == 4)
                                    {
                                        powiadomOStatusieDodawania(Logowanie._login);
                                    }
                                    query = "update lista_zdarzen set nowy_oczekujacy=0 where login = @login";//wyzerowanie eventu
                                    SqlCommand updateQuery = new SqlCommand(query, cnn);
                                    updateQuery.Parameters.AddWithValue("login", Logowanie._login);
                                    updateQuery.ExecuteNonQuery();
                                }
                                if (if3)//ktos zniknal z listy kontaktow lub zmiana statusu dostepnosci z listy kontaktow
                                {
                                    Logowanie.cos.pokazListeKontaktow();
                                    query = "update lista_zdarzen set przeladuj_kontakty=0 where login = @login";//wyzerowanie eventu
                                    SqlCommand updateQuery = new SqlCommand(query, cnn);
                                    updateQuery.Parameters.AddWithValue("login", Logowanie._login);
                                    updateQuery.ExecuteNonQuery();
                                }
                                if (if4)//sprawdzenie czy użytkownik jest zalogowany
                                {
                                    query = "update lista_zdarzen set sprawdz_logowanie=0 where login = @login";//wyzerowanie eventu
                                    SqlCommand updateQuery = new SqlCommand(query, cnn);
                                    updateQuery.Parameters.AddWithValue("login", Logowanie._login);
                                    updateQuery.ExecuteNonQuery();
                                }
                            }
                        }
                    }
                    broker();
                }
            }
        }

        public async static Task<bool> czy_zalogowany()
        {
            bool czy_dostepny = true;
            string select_query = "select czy_zalogowany from uzytkownicy where login = @login";
            SqlCommand selectQuery = new SqlCommand(select_query, cnn);
            selectQuery.Parameters.AddWithValue("login", Logowanie._login);
            czy_dostepny = (bool)selectQuery.ExecuteScalar();
            if (czy_dostepny == false)
            {
                return false;//status w bazie danych był ustawiony na niedostępny - wszystko prawidłowo
            }
            else
            {
                bool wynik = true;
                string query = "update lista_zdarzen set sprawdz_logowanie=1 where login = @login";//ustawienie eventu
                SqlCommand updateQuery = new SqlCommand(query, cnn);
                updateQuery.Parameters.AddWithValue("login", Logowanie._login);
                updateQuery.ExecuteNonQuery();
                //System.Threading.Thread.Sleep(2000);
                await Task.Delay(2000);//czekamy 2 sekundy na zmianę - jeśli ktoś jest zalogowany do aplikacji to zdąży zresetować event
                query = "select sprawdz_logowanie from lista_zdarzen where login = @login";//odczytanie eventu
                SqlCommand executeQuery = new SqlCommand(query, cnn);
                executeQuery.Parameters.AddWithValue("login", Logowanie._login);
                wynik = (bool)executeQuery.ExecuteScalar();//zmienna wynik daje nam prawdziwy status uzytkownika, ktory moze byc inny niz ten przechowywany w bazie danych (np. po zerwanym połączeniu lub wyłączeniu aplikacji na skutek błędu)
                return !wynik;//status w bazie danych był ustawiony na dostepny, a zmienna wynik przechowuje informację czy użytkownik nadal jest poprawnie zalogowany
            }
        }

        public static void broker_stop()
        {
            SqlDependency.Stop(connectionString);
            dependency.OnChange -= broker_wykryl_zmiane;
            dependency = null;
        }

        public static bool sprawdz_dane(string login, klucze key)
        {
            string hashKlucza = key.klucz_prywatny.hashuj();
            string queryResult = null;
            string query = "select skrot_klucz_prywatny from uzytkownicy where login = @login";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            try
            {
                queryResult = (string)executeQuery.ExecuteScalar();
                if (!hashKlucza.Equals(queryResult))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd! Spróbuj ponownie.\n" + ex.Message);
                return false;
            }
        }

        public static bool zarejestruj_uzytkownika(string login, string imie, string opis)
        {
            klucze nowy_klucz = new klucze();
            nowy_klucz.generuj_klucze();
            string query = "INSERT INTO uzytkownicy (login,klucz_publiczny,skrot_klucz_prywatny,imie,opis) VALUES(@login, @klucz_publiczny, @skrot_klucz_prywatny, @imie, @opis);insert into lista_kontaktow values (@login, '<lista_kontaktow></lista_kontaktow>'); insert into lista_zdarzen (login) values (@login)";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            executeQuery.Parameters.AddWithValue("imie", imie);
            executeQuery.Parameters.AddWithValue("opis", opis);
            executeQuery.Parameters.AddWithValue("klucz_publiczny", nowy_klucz.klucz_publiczny);
            executeQuery.Parameters.AddWithValue("skrot_klucz_prywatny", nowy_klucz.klucz_prywatny.hashuj());
            try
            {
                executeQuery.ExecuteNonQuery();
                klasa_rozszerzen.balloon_tip("", "Zapisz plik przechowujący klucz w bezpiecznym miejscu. Będziesz używać go do logowania.");
                if (nowy_klucz.zapisz_do_pliku(login) == false)
                {
                    usun_konto(login, nowy_klucz);
                    klasa_rozszerzen.balloon_tip("", "Konto nie zostało utworzone (nie zapisano klucza).");
                    return false;
                }
                else
                {
                    klasa_rozszerzen.balloon_tip("", "Konto zostało utworzone poprawnie. Możesz teraz się zalogować.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd! Użytkownik nie został utworzony, spróbuj ponownie." + ex.Message);
                return false;
            }
        }

        public static bool usun_konto(string login, klucze klucz)
        {
            if (sprawdz_dane(login, klucz))
            {
                try
                {
                    string query = "delete from uzytkownicy where login = @login;delete from lista_kontaktow where login=@login;delete from wiadomosci where login_odbiorcy = @login or login_wysylajacego = @login; delete from lista_zdarzen where login = @login;";
                    SqlCommand executeQuery = new SqlCommand(query, cnn);
                    executeQuery.Parameters.AddWithValue("login", login);
                    executeQuery.ExecuteNonQuery();
                    ustaw_status(login, false);
                    return true;
                }
                catch (Exception ex)
                {
                    klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd! Spróbuj ponownie.\n" + ex.Message);
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static bool czyLoginIstnieje(string login)
        {
            string query = "select count(*) from uzytkownicy where login = @login";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            try
            {
                if((int)executeQuery.ExecuteScalar() > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd!\n" + ex.Message);
                return true;
            }
        }

        public static List<Wiadomosc> sprawdzWiadomosci(string login, klucze klucz_odbierajacego)
        {
            List<Wiadomosc> wiadomosci = new List<Wiadomosc>();
            string query = "select login_wysylajacego, tresc, czas_wyslania from wiadomosci where login_odbiorcy = @login";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            using (executeQuery)
            {
                try
                {
                    using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                    {
                        while (readerQuery.Read())
                        {
                            var czas = DateTime.Now;
                            wiadomosci.Add(new Wiadomosc(readerQuery.GetString(0), login, readerQuery.GetDateTime(2), readerQuery.GetString(1).deszyfruj(klucz_odbierajacego.klucz_prywatny)));
                        }
                        readerQuery.Close();
                        if (wiadomosci.Count > 0)
                        {
                            return wiadomosci;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd!\n" + ex.Message);
                    return null;
                }
            }
        }

        public static void usunWiadomosci(string login)
        {
            try
            {
                string query = "delete from wiadomosci where login_odbiorcy=@login";
                SqlCommand executeQuery = new SqlCommand(query, cnn);
                executeQuery.Parameters.AddWithValue("login", login);
                executeQuery.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd! Spróbuj ponownie." + ex.Message);
            }
        }

        public static void wyslij_wiadomosc(string login_wysylajacego, string login_odbiorcy, string klucz_publiczny, string tresc)
        {
            DateTime termin_waznosci = DateTime.Now.AddDays(3);
            string query = "INSERT INTO wiadomosci (login_wysylajacego,login_odbiorcy,tresc,termin_waznosci,czas_wyslania) VALUES(@login_wysylajacego, @login_odbiorcy, @tresc, @termin_waznosci,@czas_wyslania);UPDATE lista_zdarzen SET nowa_wiadomosc = 1 WHERE login = @login_odbiorcy";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login_wysylajacego", login_wysylajacego);
            executeQuery.Parameters.AddWithValue("login_odbiorcy", login_odbiorcy);
            executeQuery.Parameters.AddWithValue("tresc", klasa_rozszerzen.szyfruj(tresc, klucz_publiczny));//szyfrowanie wiadomosci kluczem publicznym odbiorcy wiadomosci
            executeQuery.Parameters.AddWithValue("termin_waznosci", termin_waznosci);
            executeQuery.Parameters.AddWithValue("czas_wyslania", DateTime.Now);
            executeQuery.ExecuteNonQuery();
        }

        public static void ustaw_status(string login, bool czy_dostepny)//DO SPRAWDZENIA CZY NIE POWODUJE BŁĘDÓW PRZY NP. OCZEKUJĄCYCH KONTAKTACH (OGÓLNIE EVENTACH USTAWIONYCH GDY UŻYTKOWNIK BYŁ OFFLINE)
        {
            string query;
            if (czy_dostepny)
            {
                query = "UPDATE uzytkownicy set czy_zalogowany=1 where login = @login";
                //query = "UPDATE uzytkownicy set czy_zalogowany=1 where login = @login;update lista_zdarzen set nowa_wiadomosc = 0, nowy_oczekujacy = 0, przeladuj_kontakty = 0, sprawdz_logowanie = 0 where login = @login";
            }
            else
            {
                query = "UPDATE uzytkownicy set czy_zalogowany=0 where login = @login";
                broker_stop();
            }
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            executeQuery.ExecuteNonQuery();
        }

        public static List<Uzytkownik> pobierz_liste_kontaktow(string login)
        {
            string queryResult = null;
            XmlDocument lista_kontaktow = new XmlDocument();
            string query = "select * from lista_kontaktow where login=@login";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            using (executeQuery)
            {
                try
                {
                    using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                    {
                        if (readerQuery.Read())
                        {
                            queryResult = readerQuery.GetString(1);
                            readerQuery.Close();
                        }
                        if (queryResult != null)
                        {
                            lista_kontaktow.LoadXml(queryResult);
                            List<Uzytkownik> lista_uzytkownikow = new List<Uzytkownik>();
                            foreach (XmlNode node in lista_kontaktow)
                            {
                                foreach (XmlNode childnode in node)
                                {
                                    lista_uzytkownikow.Add(new Uzytkownik(childnode.FirstChild.InnerText, childnode.FirstChild.NextSibling.InnerXml, childnode.FirstChild.NextSibling.NextSibling.InnerText, childnode.LastChild.InnerText));
                                }
                            }
                            if (lista_uzytkownikow.Count > 0)
                            {
                                return lista_uzytkownikow;
                            }
                            else
                            {
                                return null;
                            }
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd!\n" +ex);
                    return null;
                }
            }
        }

        public static void lista_kontaktow_do_xml(List<Uzytkownik> lista_kontaktow, string login, bool powiadomienie)
        {
            XmlDocument temp = new XmlDocument();
            var xml = new XElement("Lista_kontaktow", lista_kontaktow.Select(x => new XElement("kontakt", new XElement("login", x.login), new XElement("klucz_publiczny", XElement.Parse(x.kluczPub)), new XElement("imie", x.imie), new XElement("opis", x.opis))));
            string query = "";
            if (powiadomienie == true)
            {
                query = "UPDATE lista_kontaktow SET kontakty = @kontakty WHERE login=@login; UPDATE lista_zdarzen SET przeladuj_kontakty = 1 WHERE login = @login";
            }
            else
            {
                query = "UPDATE lista_kontaktow SET kontakty = @kontakty WHERE login=@login";
            }
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            executeQuery.Parameters.AddWithValue("kontakty", xml.ToString());
            executeQuery.ExecuteNonQuery();
        }

        public static void dodajDoListyOczekujacych(string loginDodajacego, string loginDodawanego)
        {
            string query = "INSERT INTO oczekujacy_znajomi VALUES(@loginDodajacego,@loginDodawanego,1); UPDATE lista_zdarzen SET nowy_oczekujacy = 1 WHERE login = @loginDodawanego";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("loginDodajacego", loginDodajacego);
            executeQuery.Parameters.AddWithValue("loginDodawanego", loginDodawanego);
            executeQuery.ExecuteNonQuery();
        }

        public static void czyKtosChceDodacDoListy(string login)
        {
            string queryResult = null;
            string query = "SELECT login_dodajacego from oczekujacy_znajomi WHERE login_dodawanego = @login and status = 1";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            using (executeQuery)
            {
                try
                {
                    using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                    {
                        if (readerQuery.Read())
                        {
                            queryResult = readerQuery.GetString(0);
                            readerQuery.Close();
                        }
                        if (queryResult != null)
                        {
                            odczytanoProsbeDodania(queryResult, login);
                        }
                    }
                }
                catch (Exception ex)
                {
                    klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd!\n" + ex.Message);
                }
            }
        }

        public static void odczytanoProsbeDodania(string loginDodajacego, string loginDodawanego)
        {
            string query = "";
            if (MessageBox.Show("Czy chcesz dodać użytkownika " + loginDodajacego + " do listy znajomych?", "Dodawanie użytkownika " + loginDodajacego, MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
            {
                try
                {
                    query = "UPDATE oczekujacy_znajomi SET status = 3 WHERE login_dodajacego = @loginDodajacego AND login_dodawanego = @loginDodawanego;UPDATE lista_zdarzen SET nowy_oczekujacy = 1 WHERE login = @loginDodajacego";
                    SqlCommand executeQuery2 = new SqlCommand(query, cnn);
                    executeQuery2.Parameters.AddWithValue("loginDodajacego", loginDodajacego);
                    executeQuery2.Parameters.AddWithValue("loginDodawanego", loginDodawanego);
                    executeQuery2.ExecuteNonQuery();
                    dodajDoListyKontaktow(loginDodajacego, loginDodawanego);
                    dodajDoListyKontaktow(loginDodawanego, loginDodajacego);
                    Logowanie.cos.pokazListeKontaktow();
                }
                catch (Exception ex)
                {
                    klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd!\n" +ex.Message);
                }
            }
            else
            {
                try
                {
                    query = "UPDATE oczekujacy_znajomi SET status = 4 WHERE login_dodajacego = @loginDodajacego AND login_dodawanego = @loginDodawanego;UPDATE lista_zdarzen SET nowy_oczekujacy = 1 WHERE login = @loginDodajacego";
                    SqlCommand executeQuery2 = new SqlCommand(query, cnn);
                    executeQuery2.Parameters.AddWithValue("loginDodajacego", loginDodajacego);
                    executeQuery2.Parameters.AddWithValue("loginDodawanego", loginDodawanego);
                    executeQuery2.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd!\n" +ex.Message);
                }
            }
        }

        public static void powiadomOStatusieDodawania(string login)
        {
            string queryResult = null;
            int queryResult2 = 0;
            string query = "SELECT login_dodawanego, status from oczekujacy_znajomi WHERE login_dodajacego = @login and (status = 3 OR status = 4)";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            using (executeQuery)
            {
                try
                {
                    using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                    {
                        if (readerQuery.Read())
                        {
                            queryResult = readerQuery.GetString(0);
                            queryResult2 = readerQuery.GetInt32(1);
                            readerQuery.Close();
                        }
                        if (queryResult != null && queryResult2 == 3)
                        {
                            query = "DELETE FROM oczekujacy_znajomi WHERE login_dodajacego = @loginDodajacego AND login_dodawanego = @loginDodawanego";
                            SqlCommand executeQuery3 = new SqlCommand(query, cnn);
                            executeQuery3.Parameters.AddWithValue("loginDodajacego", login);
                            executeQuery3.Parameters.AddWithValue("loginDodawanego", queryResult);
                            executeQuery3.ExecuteNonQuery();
                            Logowanie.cos.pokazListeKontaktow();
                            klasa_rozszerzen.balloon_tip("", queryResult + " zaakceptował Twoją prośbę o dodanie do znajomych.");
                        }
                        else if (queryResult != null && queryResult2 == 4)
                        {
                            readerQuery.Close();
                            query = "DELETE FROM oczekujacy_znajomi WHERE login_dodajacego = @loginDodajacego AND login_dodawanego = @loginDodawanego";
                            SqlCommand executeQuery3 = new SqlCommand(query, cnn);
                            executeQuery3.Parameters.AddWithValue("loginDodajacego", login);
                            executeQuery3.Parameters.AddWithValue("loginDodawanego", queryResult);
                            executeQuery3.ExecuteNonQuery();
                            klasa_rozszerzen.balloon_tip("", queryResult + " odrzucił Twoją prośbę o dodanie do znajomych.");
                        }
                    }
                }
                catch (Exception ex)
                {
                    klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd!\n"+ex.Message);
                }
            }
        }

        public static void dodajDoListyKontaktow(string loginDodajacego, string loginDodawanego)
        {
            var temp_lista = pobierz_liste_kontaktow(loginDodajacego);
            string login = null;
            string klucz_pub = null;
            string imie = null;
            string opis = null;
            string query = "SELECT login, klucz_publiczny, imie, opis FROM uzytkownicy WHERE login = @loginDodawanego";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("loginDodawanego", loginDodawanego);
            using (executeQuery)
            {
                try
                {
                    using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                    {
                        if (readerQuery.Read())
                        {
                            login = readerQuery.GetString(0);
                            klucz_pub = readerQuery.GetString(1);
                            imie = readerQuery.GetString(2);
                            opis = readerQuery.GetString(3);
                            readerQuery.Close();
                        }
                        if (login != null && klucz_pub != null && imie != null && opis != null)
                        {
                            Uzytkownik dodaj = new Uzytkownik(login, klucz_pub, imie, opis);
                            if (temp_lista != null)
                            {
                                temp_lista.Add(dodaj);
                            }
                            else
                            {
                                temp_lista = new List<Uzytkownik>();
                                temp_lista.Add(dodaj);
                            }
                            lista_kontaktow_do_xml(temp_lista, loginDodajacego, true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd!\n" +ex.Message);
                }
            }
        }

        public static List<string> dostepni_uzytkownicy()
        {
            List<string> wszyscy_dostepni = new List<string>();
            string query = "declare @XML xml; SELECT @XML = kontakty FROM lista_kontaktow where login = @login; select login from uzytkownicy where login in (select T.N.value('login[1]', 'varchar(max)') as login from @XML.nodes('/Lista_kontaktow/kontakt') as T(N)) and czy_zalogowany = 1";
            SqlCommand readQuery = new SqlCommand(query, cnn);
            readQuery.Parameters.AddWithValue("login", Logowanie._login);
            using (SqlDataReader loginReaderQuery = readQuery.ExecuteReader())
            {
                while (loginReaderQuery.Read())
                {
                    wszyscy_dostepni.Add(loginReaderQuery.GetString(0));
                }
            }
            return wszyscy_dostepni;
        }

        public static void rozglos_logowanie()
        {
                string query = "declare @XML xml; SELECT @XML = kontakty FROM lista_kontaktow where login = @login; update lista_zdarzen set przeladuj_kontakty = 1 where login in ((select login from uzytkownicy where login in (select T.N.value('login[1]', 'varchar(max)') as login from @XML.nodes('/Lista_kontaktow/kontakt') as T(N)) and czy_zalogowany = 1))";//jedna komenda zalatwia wszystko
                SqlCommand update = new SqlCommand(query, cnn);
                update.Parameters.AddWithValue("login", Logowanie._login);
                update.ExecuteNonQuery();
        }

        public static bool sprawdzListeKontaktow(string loginDodajacego, string loginDodawanego)
        {
            if (Logowanie.cos.lista != null)
            {
                string query = "SELECT login_dodawanego FROM oczekujacy_znajomi WHERE login_dodajacego = @loginDodajacego AND login_dodawanego = @loginDodawanego";
                SqlCommand executeQuery = new SqlCommand(query, cnn);
                executeQuery.Parameters.AddWithValue("loginDodajacego", loginDodajacego);
                executeQuery.Parameters.AddWithValue("loginDodawanego", loginDodawanego);
                try
                {
                    string queryResult = (string)executeQuery.ExecuteScalar();
                    if (queryResult != null)
                    {
                        return false;
                    }
                }
                catch (Exception ex)
                {
                    klasa_rozszerzen.balloon_tip("", "Wystąpił nieoczekiwany błąd!\n" + ex.Message);
                    return false;
                }
                Uzytkownik sprawdz = Logowanie.cos.lista.Find(x => x.login == loginDodawanego);
                if (sprawdz != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else//jeśli lista jest pusta to z powodzeniem można dodać jakikolwiek kontakt
            {
                return true;
            }
        }
    }
}