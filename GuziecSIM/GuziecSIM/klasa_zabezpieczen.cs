using System.Text;
using System.Security.Cryptography;
using System.Linq;

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
