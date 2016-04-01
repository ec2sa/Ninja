using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChatWinTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            testc();
            Console.ReadKey();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        //0:XX:61:2121XX20XX//CQ
        private static string Old2New(string old)
        {
            var p = old.Split(':');
            string n = p[0] + ":XX:" + p[1] + ":";
            n += p[2].Substring(0, 4)+"XX"+p[2].Substring(4,2)+"XX";

            if (p.Length > 3)
            {
                n += "/";
                if (p[3].Contains('M')) n += "m/";
                if (p[3].Contains('F')) n += "f/";
                if (p[3].Contains('I')) n += "i/";

                if (p[3].Contains('C')) n += "C";
                if (p[3].Contains('Q')) n += "Q";
            }

            return n;
            
        }

        private static void testc()
        {
            string old="0:60:202020:QCA";

            string news = Old2New(old);

        }
    }
}
