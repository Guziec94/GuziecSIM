using System.Text;
using System.Security.Cryptography;
using System.Linq;
using Microsoft.Win32;
using System.IO;
using System.Xml;
using System;
using System.Windows;

namespace klasa_zabezpieczen
{
    public class klucze
    {
        public RSACryptoServiceProvider rsa;
        public string klucz_prywatny;
        public string klucz_publiczny;

        public void generuj_klucze()
        {
            rsa = new RSACryptoServiceProvider(512);
            klucz_prywatny = rsa.ToXmlString(true);
            klucz_publiczny = rsa.ToXmlString(false);
            RSAParameters rsapar = rsa.ExportParameters(true);
        }

        public void zaladuj_z_pliku()
        {
            try
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                if (openFileDialog.ShowDialog() == true)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(openFileDialog.FileName);
                    rsa = new RSACryptoServiceProvider(512);
                    rsa.FromXmlString(doc.InnerXml);
                    RSAParameters rsapar = new RSAParameters();
                    rsapar.Modulus = Encoding.Default.GetBytes(doc.DocumentElement.ChildNodes[0].InnerText);
                    rsapar.Exponent = Encoding.Default.GetBytes(doc.DocumentElement.ChildNodes[1].InnerText);
                    rsapar.P = Encoding.Default.GetBytes(doc.DocumentElement.ChildNodes[2].InnerText);
                    rsapar.Q = Encoding.Default.GetBytes(doc.DocumentElement.ChildNodes[3].InnerText);
                    rsapar.DP = Encoding.Default.GetBytes(doc.DocumentElement.ChildNodes[4].InnerText);
                    rsapar.DQ = Encoding.Default.GetBytes(doc.DocumentElement.ChildNodes[5].InnerText);
                    rsapar.InverseQ = Encoding.Default.GetBytes(doc.DocumentElement.ChildNodes[6].InnerText);
                    rsapar.D = Encoding.Default.GetBytes(doc.DocumentElement.ChildNodes[7].InnerText);
                    klucz_prywatny = rsa.ToXmlString(true);
                    klucz_publiczny = rsa.ToXmlString(false);
                }
            }
            catch(Exception)
            {
                MessageBox.Show("Nie udało się załadować pliku z kluczem.");
            }
        }

        public bool zapisz_do_pliku(string login = null)
        {
            var dialog = new SaveFileDialog();
            dialog.Filter = "Plik XML(.xml)|*.xml|Wszystkie pliki(*.*)|*.*";
            dialog.FileName = login == null ? "klucz prywatny.xml" : "klucz uzytkownika " + login + ".xml";
            if (dialog.ShowDialog() == true)
            {
                File.WriteAllText(dialog.FileName, klucz_prywatny);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    public static class klasa_rozszerzen
    {
        public static UnicodeEncoding _encoder = new UnicodeEncoding();

        public static string hashuj(this string dane)//"tekst do zahashowania".hashuj();
        {
            MD5 md5Hash = MD5.Create();
            byte[] daneBajtowo = System.Text.Encoding.Default.GetBytes(dane);//konwersja stringa na tablicę bajtów
            byte[] data = md5Hash.ComputeHash(daneBajtowo);//hash w postacji bajtów
            StringBuilder sBuilder = new StringBuilder();// stringbuilder, ktory bedzie przechowywac skrot funkcji
            for (int i = 0; i < data.Length; i++)// pętla po zahashowanych bajtowych danych konwertująca na zapis szesnatkowy
            {
                sBuilder.Append(data[i].ToString("x2"));
            }
            return sBuilder.ToString();//zwraca skrót w postaci szesnastkowej
        }

        public static string deszyfruj(this string dane, string klucz_prywatny)//"tekst do zdeszyfrowania".deszyfruj(klucz_publiczny);
        {
            var rsa = new RSACryptoServiceProvider();

            /*var dataArray = data.Split(new char[] { ',' });
            byte[] dataByte = new byte[dataArray.Length];
            for (int i = 0; i < dataArray.Length; i++)
            {
                dataByte[i] = Convert.ToByte(dataArray[i]);
            }*/

            byte[] daneBajtowo = System.Text.Encoding.Default.GetBytes(dane);//praca na ascii
            rsa.FromXmlString(klucz_prywatny);
            var odszyfrowaneBajty = rsa.Decrypt(daneBajtowo, false);
            return _encoder.GetString(odszyfrowaneBajty);
        }

        public static string szyfruj(this string dane, string klucz_publiczny)//"tekst do zaszyfrowania".szyfruj(klucz_publiczny);
        {
            var rsa = new RSACryptoServiceProvider();
            rsa.FromXmlString(klucz_publiczny);
            var dataToEncrypt = _encoder.GetBytes(dane);
            var encryptedByteArray = rsa.Encrypt(dataToEncrypt, false).ToArray();

            /*var length = encryptedByteArray.Count();
            var item = 0;
            var sb = new StringBuilder();
            foreach (var x in encryptedByteArray)
            {
                item++;
                sb.Append(x);

                if (item < length)
                    sb.Append(",");
            }
            szyfrowany.Text = sb.ToString();*/

            return System.Text.Encoding.Default.GetString(encryptedByteArray);//praca na ascii
        }
    }
}