namespace GuziecSIM
{
    public class Uzytkownik
    {
        public string login;
        public string kluczPub;

        public string imie;
        public string opis;

        public string ip;
        public int port;

        public Uzytkownik(string login, string kluczPub, string imie, string opis)
        {
            this.login = login;
            this.kluczPub = kluczPub;
            this.imie = imie;
            this.opis = opis;
        }
    }
}