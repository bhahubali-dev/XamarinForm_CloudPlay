using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PlayOnCloud.Droid
{
    internal static class Logger
    {
        private static object syncRoot = new object();

        internal static void Log(string format, params object[] args)
        {
            Log(string.Format(format, args));
        }

        internal static void Log(string message)
        {
            try
            {
#if DEBUG
                //XXX: Because of a dumbass Xamarin bug we need to escape % or else it will interpret it as a format specifier and crash
                //Remove the escaping when C9 is released and we update to it - https://bugzilla.xamarin.com/show_bug.cgi?id=45046
                Console.WriteLine(message.Replace("%", "%%"));
#endif
                var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(libraryPath, "log.log");
                lock (syncRoot)
                {
                    using (StreamWriter streamWriter = File.AppendText(filePath))
                        streamWriter.Write(DateTime.UtcNow.ToString() + ": " + message + Environment.NewLine);

                    FileInfo fileInfo = new FileInfo(filePath);
                    if (File.Exists(filePath) && (fileInfo.Length > 1048576))
                    {
                        File.Copy(filePath, Path.Combine(fileInfo.DirectoryName, DateTime.UtcNow.ToString("yyyyMMddHHmm") + "log.log"));
                        File.Delete(filePath);
                        deleteOldFiles();
                    }
                }
            }
            catch
            {
            }
        }

        private static void deleteOldFiles()
        {
            try
            {
                var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                string[] files = Directory.GetFiles(libraryPath, "*.log", SearchOption.TopDirectoryOnly);
                if ((files != null) && files.Any())
                {
                    List<FileInfo> fileInfos = new List<FileInfo>();
                    foreach (var file in files)
                        fileInfos.Add(new FileInfo(file));

                    var filesToDelte = fileInfos.Where(f => f.LastWriteTimeUtc < DateTime.UtcNow.Subtract(TimeSpan.FromDays(30)));
                    if (filesToDelte.Any())
                        foreach (var file in filesToDelte)
                            file.Delete();
                }
            }
            catch
            {
            }
        }
    }
}