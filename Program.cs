using System;
using System.Windows.Forms;

namespace ProjetParc;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new WelcomePage());
    }
}