using System;
using System.Data.SqlClient;
using System.Windows;
using klasa_zabezpieczen;

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

        public static void sprawdzDaneLogowania(string login, klucze key)
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
                        MessageBox.Show("Błąd logowania. Sprawdź dane!");
                    }
                    else
                    {
                        MessageBox.Show("Sukces!");
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("Wystąpił nieoczekiwany błąd! Spróbuj ponownie.");
                }
        }
        public static void zarejestruj_uzytkownika(string login, string imie, string opis)
        {
            klucze nowy_klucz = new klucze();
            nowy_klucz.generuj_klucze();

            string query = "INSERT INTO uzytkownicy (login,klucz_publiczny,skrot_klucz_prywatny,imie,opis) VALUES(@login, @klucz_publiczny, @skrot_klucz_prywatny, @imie, @opis)";
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
                if(nowy_klucz.zapisz_do_pliku(login)==false)
                {
                    query = "delete from uzytkownicy where login = @login";
                    executeQuery = new SqlCommand(query, cnn);
                    executeQuery.Parameters.AddWithValue("login", login);
                    executeQuery.ExecuteNonQuery();
                }
                else
                {
                    MessageBox.Show("Konto zostało utworzone poprawnie. Możesz teraz się zalogować.");
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Wystąpił nieoczekiwany błąd! Użytkownik nie został utworzony, spróbuj ponownie.");
            }
        }
    }
}
