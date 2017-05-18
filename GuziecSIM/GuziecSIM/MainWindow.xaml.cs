using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace GuziecSIM
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon = null;

        public MainWindow()
        {
            InitializeComponent();
            notifyIcon = new NotifyIcon();
            notifyIcon.Icon = new Icon(@"../../ikona.ico");
            notifyIcon.Visible = true;
            System.Windows.Application.Current.Resources["notifyIcon"] = notifyIcon;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(Logowanie._login != null)
            {
                api_baza_danych.baza_danych.ustaw_status(Logowanie._login, false);
                api_baza_danych.baza_danych.rozglos_logowanie();
            }
            notifyIcon.Visible = false;
        }
    }
}
