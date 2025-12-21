using System;
using System.Windows.Forms;

namespace VietSportSystem
{
    static class Progra
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}