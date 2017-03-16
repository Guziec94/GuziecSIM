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

        public Uzytkownik(string login, string kluczPub)
        {
            this.login = login;
            this.kluczPub = kluczPub;
        }
    }
}