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
            var sqlParam = new SqlParameter("login", login);
            sqlParam.Value = login;
            executeQuery.Parameters.Add(sqlParam);
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
                catch(Exception)
                {
                    MessageBox.Show("Wystąpił nieoczekiwany błąd! Spróbuj ponownie.");
                }
        }
    }
}
