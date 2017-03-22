using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GuziecSIM
{
    /// <summary>
    /// Logika interakcji dla klasy PanelDodawaniaZnajomego.xaml
    /// </summary>
    public partial class PanelDodawaniaZnajomego : Window
    {
        public PanelDodawaniaZnajomego()
        {
            InitializeComponent();
        }

        public string Znajomy
        {
            get { return login.Text; }
        }

        private void btnPotwiedz_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }

        /* [REGUŁY SPRAWDZAJĄCE CZY WPROWADZANE DANE TEKSTOWE SĄ DOZWOLONE] */
        private bool dozwolone(string text, bool cyfry = false)
        {
            string z = "żŻóÓłŁćĆęĘśŚąĄźŹńŃ ";
            string c = "0123456789";

            return !cyfry ? !(z.Contains(text) || znakiSpecjalne(text)) : !(z.Contains(text) || c.Contains(text) || znakiSpecjalne(text));
        }
        private bool znakiSpecjalne(string text)
        {
            string s = "!@#$%^&*()_-+={[}]|\\:;\"'<,>.?/";
            return s.Contains(text);
        }

        /* [ZABEZPIECZENIE POLA INPUT PRZED WPROWADZANIEM NIEDOZWOLONYCH ZNAKOW] */
        private void textBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if ((!dozwolone(e.Text, true) && login.Text.Length == 0) || (!dozwolone(e.Text, true) && login.SelectedText == login.Text) || znakiSpecjalne(e.Text)) e.Handled = true;
        }

        /* [ZABLOKOWANIE MOŻLIWOŚCI WKLEJANIA DANYCH DO POLA LOGINU] */
        private void textBox_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}
