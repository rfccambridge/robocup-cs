using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;


namespace RFC.SerialControl
{
    static class Program
    {        
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);            
            Application.Run(new SerialControl());
        }
    }
}
