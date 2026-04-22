using System;
using System.Windows.Forms;
using MiniCRM.Client.Controllers;
using MiniCRM.Client.Forms;
using MiniCRM.Client.Infrastructure;

namespace MiniCRM.Client
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var mainForm = new MainForm(new ClientsController(), new Debounce());

            Application.Run(mainForm);
        }
    }
}