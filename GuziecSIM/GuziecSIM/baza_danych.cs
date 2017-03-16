using System;
using System.Data.SqlClient;
using System.Windows;
using klasa_zabezpieczen;
using System.Collections.Generic;
using GuziecSIM;
using System.Xml;
using System.Linq;
using System.Xml.Linq;

namespace baza_danych_azure
{
    public class baza_danych
    {
        public static SqlConnection cnn;

        public static void polacz_z_baza()
        {
            string connetionString = "Data Source=tcp:LENOVOY510P;Initial Catalog = GuziecSIMDB; User ID = Guziec94; Password=P@ssw0rd";
            cnn = new SqlConnection(connetionString);
            try
            {
                cnn.Open();
            }
            catch (Exception)
            {
                MessageBox.Show("Nie można nawiązać połączenia!");
            }
        }

        public static void rozlacz_z_baza()
        {
            try
            {
                cnn.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("Nie udało się rozłączyć.");
            }
        }

        public static bool sprawdz_dane(string login, klucze key)
        {
            string hashKlucza = key.klucz_prywatny.hashuj();
            string queryResult = null;
            string query = "select skrot_klucz_prywatny from uzytkownicy where login = @login";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            using (executeQuery)
                try
                {
                    using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                    {
                        if (readerQuery.Read())
                        {
                            queryResult = readerQuery.GetString(0);
                        }
                    }
                    if (!hashKlucza.Equals(queryResult))
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Wystąpił nieoczekiwany błąd! Spróbuj ponownie.");
                    return false;
                }
        }

        public static bool zarejestruj_uzytkownika(string login, string imie, string opis)
        {
            klucze nowy_klucz = new klucze();
            nowy_klucz.generuj_klucze();

            string query = "INSERT INTO uzytkownicy (login,klucz_publiczny,skrot_klucz_prywatny,imie,opis) VALUES(@login, @klucz_publiczny, @skrot_klucz_prywatny, @imie, @opis);insert into lista_kontaktow values (@login, '<lista_kontaktow></lista_kontaktow>')";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            executeQuery.Parameters.AddWithValue("imie", imie);
            executeQuery.Parameters.AddWithValue("opis", opis);
            executeQuery.Parameters.AddWithValue("klucz_publiczny", nowy_klucz.klucz_publiczny);
            executeQuery.Parameters.AddWithValue("skrot_klucz_prywatny", nowy_klucz.klucz_prywatny.hashuj());
            try
            {
                executeQuery.ExecuteNonQuery();
                MessageBox.Show("Zapisz plik przechowujący klucz w bezpiecznym miejscu. Będziesz używać go do logowania.");
                if (nowy_klucz.zapisz_do_pliku(login) == false)
                {
                    usun_konto(login, nowy_klucz);
                    MessageBox.Show("Konto nie zostało utworzone (nie zapisano klucza).");
                    return false;
                }
                else
                {
                    MessageBox.Show("Konto zostało utworzone poprawnie. Możesz teraz się zalogować.");
                    return true;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Wystąpił nieoczekiwany błąd! Użytkownik nie został utworzony, spróbuj ponownie."+ex.Message);
                return false;
            }
        }

        public static bool usun_konto(string login, klucze klucz)
        {
            if (sprawdz_dane(login, klucz))
            {
                try
                {
                    string query = "delete from uzytkownicy where login = @login;delete from lista_kontaktow where login=@login;delete from krotkie_wiadomosci where login_odbiorcy = @login or login_wysylajacego = @login";
                    SqlCommand executeQuery = new SqlCommand(query, cnn);
                    executeQuery.Parameters.AddWithValue("login", login);
                    executeQuery.ExecuteNonQuery();
                    usunAdresIP(login);
                    return true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Wystąpił nieoczekiwany błąd! Spróbuj ponownie.");
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
            string queryResult = null;
            string query = "select login from uzytkownicy where login = @login";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            using (executeQuery)
                try
                {
                    using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                    {
                        if (readerQuery.Read())
                        {
                            queryResult = readerQuery.GetString(0);
                        }
                        if (queryResult != null)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Wystąpił nieoczekiwany błąd!");
                    return true;
                }
        }

        public static List<string> sprawdzKrotkieWiadomosci(string login, klucze klucz_odbierajacego)
        {
            List<string> wiadomosci = new List<string>();
            string query = "select login_wysylajacego, tresc from krotkie_wiadomosci where login_odbiorcy = @login";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            using (executeQuery)
                try
                {
                    using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                    {
                        while (readerQuery.Read())
                        {
                            wiadomosci.Add(readerQuery.GetString(0) + ": " + readerQuery.GetString(1).deszyfruj(klucz_odbierajacego.klucz_prywatny));
                        }
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
                catch (Exception)
                {
                    MessageBox.Show("Wystąpił nieoczekiwany błąd!");
                    return null;
                }
        }

        public static void usunKrotkieWiadomosci(string login)
        {
            try
            {
                string query = "delete from krotkie_wiadomosci where login_odbiorcy=@login";
                SqlCommand executeQuery = new SqlCommand(query, cnn);
                executeQuery.Parameters.AddWithValue("login", login);
                executeQuery.ExecuteNonQuery();
            }
            catch (Exception)
            {
                MessageBox.Show("Wystąpił nieoczekiwany błąd! Spróbuj ponownie.");
            }
        }

        public static void wyslij_krotka_wiadomosc(string login_wysylajacego, string login_odbiorcy, string klucz_publiczny, string tresc, DateTime? termin_waznosci)
        {
            if (termin_waznosci == null)
            {
                termin_waznosci = DateTime.Now.AddDays(3);
            }

            string query = "INSERT INTO krotkie_wiadomosci (login_wysylajacego,login_odbiorcy,tresc,termin_waznosci) VALUES(@login_wysylajacego, @login_odbiorcy, @tresc, @termin_waznosci)";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login_wysylajacego", login_wysylajacego);
            executeQuery.Parameters.AddWithValue("login_odbiorcy", login_odbiorcy);
            executeQuery.Parameters.AddWithValue("tresc", klasa_rozszerzen.szyfruj(tresc, klucz_publiczny));//szyfrowanie wiadomosci kluczem publicznym odbiorcy wiadomosci
            executeQuery.Parameters.AddWithValue("termin_waznosci", termin_waznosci);
            executeQuery.ExecuteNonQuery();
        }

        public static void wprowadzAdresIP(string login, string externalIP)
        {
            string queryResult = null;
            string query = "SELECT login FROM dostepni_uzytkownicy WHERE login = @login";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            using (executeQuery)
                try
                {
                    using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                    {
                        if (readerQuery.Read())
                        {
                            queryResult = readerQuery.GetString(0);
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Wystąpił nieoczekiwany błąd! Spróbuj ponownie.");
                }

            if (queryResult == null)
            {
                query = "INSERT INTO dostepni_uzytkownicy VALUES(@login,@externalIP,@data)";
            }
            else
            {
                query = "UPDATE dostepni_uzytkownicy SET ostatnio_online = @data, adres_ip = @externalIP WHERE login = @login";
            }
            executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            executeQuery.Parameters.AddWithValue("data", DateTime.Now);
            executeQuery.Parameters.AddWithValue("externalIP", externalIP);
            executeQuery.ExecuteNonQuery();
        }

        public static void usunAdresIP(string login)
        {
            string query = "DELETE FROM dostepni_uzytkownicy WHERE login=@login";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery = new SqlCommand(query, cnn);
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
                try
                {
                    using (SqlDataReader readerQuery = executeQuery.ExecuteReader())
                    {
                        if (readerQuery.Read())
                        {
                            queryResult = readerQuery.GetString(1);
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
                            return lista_uzytkownikow;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Wystąpił nieoczekiwany błąd!");
                    return null;
                }
        }

        public static void lista_kontaktow_do_xml(List<Uzytkownik> lista_kontaktow, string login)
        {
            XmlDocument temp = new XmlDocument();
            var xml = new XElement("Lista_kontaktow", lista_kontaktow.Select(x => new XElement("kontakt", new XElement("login", x.login), new XElement("klucz_publiczny", XElement.Parse(x.kluczPub)), new XElement("imie", x.imie), new XElement("opis", x.opis))));
            string query = "UPDATE lista_kontaktow SET kontakty = @kontakty WHERE login=@login";
            SqlCommand executeQuery = new SqlCommand(query, cnn);
            executeQuery.Parameters.AddWithValue("login", login);
            executeQuery.Parameters.AddWithValue("kontakty", xml.ToString());
            executeQuery.ExecuteNonQuery();
        }
    }
}