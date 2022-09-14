using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backup
{
  public class Utils
  {


    public static string GetZipFileName(string outPath)
    {
      int count = 0;
      while(count < 1000)
      {
        string path = System.IO.Path.Combine(outPath, String.Format("{0}{1}.zip", DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss"), count == 0 ? "" : "_"+count.ToString()));
        count++;
        if(!File.Exists(path))
          return path;
      }
      return System.IO.Path.Combine(outPath, String.Format("{0}{1}.zip", DateTime.Now.ToString("yyyy_MM_dd HH_mm_ss"), "last"));
    }

    public static bool NeedBackup(string src,string dst, List<string> excludedDirs)
    {
      var files = GetRelativeFileList(src, excludedDirs);
      foreach(string file in files)
      {
        string srcFile = Path.Combine(src, file);
        string dstFile = Path.Combine(dst, file);
        if (!SameFile(srcFile, dstFile))
          return true;
      }
      return false;
    }

    public static bool SameFile(string srcFile,string dstFile)
    {
      var src = File.Exists(srcFile) ? new FileInfo(srcFile) : null;
      var dst = File.Exists(dstFile) ? new FileInfo(dstFile) : null;
      if (src ==null && dst==null)
        return true;
      if (src == null && dst != null)
        return false;
      if (src != null && dst == null)
        return false;

      if (src.Length != src.Length)
        return false;
       if (src.LastWriteTime != dst.LastWriteTime)
        return false;
      

       return true;
    }

    public static void EmptyDirectory(string dirPath)
    {
      var di = new DirectoryInfo(dirPath);
      foreach (FileInfo file in di.GetFiles())
      {
        file.Delete();
      }
      foreach (DirectoryInfo dir in di.GetDirectories())
      {
        dir.Delete(true);
      }
    }

    public static void EraseOlderFiles(string path,int numToKeep = 10)
    {
      var sortedFiles = new DirectoryInfo(path).GetFiles()
                                                        .OrderBy(f => f.LastWriteTime)
                                                        .ToList();
      for (int i = 0; i < sortedFiles.Count - Math.Max(1,numToKeep); i++)
        sortedFiles[i].Delete();
    }

    public static void CreateDirectory(string path)
    {
      if (!Directory.Exists(path))
        Directory.CreateDirectory(path);
    }

    public static string GetRelativePath(string basePath, string file)
    {
      try
      {
        Uri pathUri = new Uri(file);
        // Folders must end in a slash
        if (!basePath.EndsWith(Path.DirectorySeparatorChar.ToString()))
        {
          basePath += Path.DirectorySeparatorChar;
        }
        Uri folderUri = new Uri(basePath);
        return CleanPath(Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar)));
      }
      catch
      {
        return file;
      }
    }

    public static string CleanPath(string path)
    {
      var parts = path.Split("\\".ToCharArray());
      List<string> ret = new List<string>();
      for (int i = 0; i < parts.Length; i++)
      {
        if (parts[i] == ".." && ret.Count > 0)
          ret.RemoveAt(ret.Count - 1);
        else
          ret.Add(parts[i]);
      }
      return String.Join("\\", ret.ToArray());
    }

    public static void CopyDirectory(string src,string dst)
    {
      //Now Create all of the directories
      foreach (string dirPath in Directory.GetDirectories(src, "*", SearchOption.AllDirectories))
        Directory.CreateDirectory(dirPath.Replace(src, dst));

      //Copy all the files & Replaces any files with the same name
      foreach (string newPath in Directory.GetFiles(src, "*.*", SearchOption.AllDirectories))
        File.Copy(newPath, newPath.Replace(src, dst), true);
    }

    public static void ZipIt(string path,List<string> excludedDirs, string outPathAndZipFile, string password)
    {
      string OutPath = outPathAndZipFile;
                                             // find number of chars to remove from orginal file path
      int TrimLength = (path).ToString().Length;
      if (!path.EndsWith("\\"))
        TrimLength += 1; //remove '\'

      var files = GetRelativeFileList(path, excludedDirs);
      if (files.Count <= 0)
        return;
      FileStream ostream;
      byte[] obuffer;
      ZipOutputStream oZipStream = new ZipOutputStream(System.IO.File.Create(OutPath)); // create zip stream
      if (password != String.Empty) oZipStream.Password = password;
      oZipStream.SetLevel(9); // 9 = maximum compression level
      ZipEntry oZipEntry;
      foreach (string entrypath in files) // for each file, generate a zipentry
      {
        oZipEntry = new ZipEntry(entrypath);
        oZipStream.PutNextEntry(oZipEntry);

        if (!entrypath.EndsWith(@"/")) // if a file ends with '/' its a directory
        {
          ostream = File.OpenRead(Path.Combine(path, entrypath));
          obuffer = new byte[ostream.Length]; // byte buffer
          ostream.Read(obuffer, 0, obuffer.Length);
          oZipStream.Write(obuffer, 0, obuffer.Length);
          Console.Write(".");
          ostream.Close();
        }
      }
      oZipStream.Finish();
      oZipStream.Close();
    }

    public static List<String> GetRelativeFileList(string dir, List<string> excludedDirs)
    {
      var files = GetFileList(dir, excludedDirs);
      var ret = new List<string>();
      foreach(var file in files)
      {
        string f = GetRelativePath(dir, file);
        if(!string.IsNullOrEmpty(f))
          ret.Add(f);
      }
      return ret;
    }

    public static List<String> GetFileList(string dir,List<string> excludedDirs)
    {
      var mid = new List<String>();
      bool Empty = true;
      foreach (string file in Directory.GetFiles(dir)) // add each file in directory
      {
        mid.Add(file);
        Empty = false;
      }

      if (Empty)
      {
        if (Directory.GetDirectories(dir).Length == 0) // if directory is completely empty, add it
        {
          mid.Add(dir + @"/");
        }
      }
      foreach (string dirs in Directory.GetDirectories(dir)) // do this recursively
      {
        // set up the excludeDir test
        string testDir = dirs.Substring(dirs.LastIndexOf(@"\") + 1).ToUpper();
        if (excludedDirs!=null && excludedDirs.Contains(testDir))
          continue;
        foreach (var obj in GetFileList(dirs, excludedDirs))
        {
          mid.Add(obj);
        }
      }
      return mid; // return file list
    }

  }
}
