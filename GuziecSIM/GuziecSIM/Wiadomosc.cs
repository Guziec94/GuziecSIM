using System;

namespace GuziecSIM
{
    public class Wiadomosc
    {
        public string nadawca;
        public string odbiorca;

        public DateTime czas;
        public string Text;
        public Wiadomosc(string nadawca, string odbiorca, DateTime czas, string Text)
        {
            this.nadawca = nadawca;
            this.odbiorca = odbiorca;
            this.czas = czas;
            this.Text = Text;
        }

        public Wiadomosc()
        {
        }
    }
}