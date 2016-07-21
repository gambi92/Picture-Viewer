using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace kepnezegeto
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        public static List<FileInfo> mainArgs = new List<FileInfo>();

        [STAThread]
        static void Main(string[] args)
        {
            for (int i = 0; i < args.Count(); i++) mainArgs.Add(new FileInfo(args[i]));
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
