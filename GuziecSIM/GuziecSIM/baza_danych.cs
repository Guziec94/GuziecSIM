using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GuziecSIM
{
    public static class baza_danych
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
    }
}
