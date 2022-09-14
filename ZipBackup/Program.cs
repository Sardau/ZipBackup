using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Backup
{
  static class Program
  {
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main(string[] args)
    {
        string src = args[0];
        string dst = args[1];

        bool force = args.Length > 2 ? args[2].ToLower() == "f" : false;


			Console.WriteLine("___________ " + DateTime.Now.ToUniversalTime() + " ___________");

			List<string> excludedDirs = new List<string>();

        string mirrorDir = Path.Combine(dst, "Mirror");
        string historyDir = Path.Combine(dst, "History");
        Console.WriteLine("Creating " + mirrorDir);
        Utils.CreateDirectory(mirrorDir);
        Console.WriteLine("Creating " + historyDir);
        Utils.CreateDirectory(historyDir);


        if (force)
        {
          Console.WriteLine("Forcing backup of " + src);
        }
        if (force || Utils.NeedBackup(src, mirrorDir, excludedDirs))
        {
          Console.WriteLine("Creating history");
          var outZip = Utils.GetZipFileName(historyDir);
          if (File.Exists(outZip))
            File.Delete(outZip);
          Console.WriteLine("zipping old backup to : " + outZip);
          Utils.ZipIt(mirrorDir, excludedDirs, outZip, "");
          Console.WriteLine("Cleaning mirror");
          Utils.EmptyDirectory(mirrorDir);
          Console.WriteLine("Copying files");
          Utils.CopyDirectory(src, mirrorDir);
          Console.WriteLine("Erasing older history files");
          Utils.EraseOlderFiles(historyDir);
        }
        else
        {
          Console.WriteLine("Up to date: no need to backup.");
        }

        
 



      /*
      Application.EnableVisualStyles();
      Application.SetCompatibleTextRenderingDefault(false);
      Application.Run(new Form1());
      */
    }
  }
}
