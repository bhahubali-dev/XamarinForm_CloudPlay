using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xamarin.Forms;
using PlayOnCloud.Droid;

[assembly: Dependency(typeof(LoggerService))]
namespace PlayOnCloud.Droid
{
	public class LoggerService : ILogger
	{
		public string GetLog()
		{
            //throw new NotImplementedException();
            string content = string.Empty;
            string fileName = "log.log";
            var libraryPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = Path.Combine(libraryPath, fileName);
            if (File.Exists(filePath))
                content = File.ReadAllText(filePath);

            string[] files = Directory.GetFiles(libraryPath, "*.log", SearchOption.TopDirectoryOnly);
            if ((files != null) && files.Any())
            {
                List<FileInfo> fileInfos = new List<FileInfo>();
                foreach (var file in files)
                    fileInfos.Add(new FileInfo(file));

                fileInfos = fileInfos.OrderByDescending(f => f.LastAccessTime).Where(f => f.Name != "log.log").ToList();
                if (fileInfos.Any())
                    content = File.ReadAllText(fileInfos.First().FullName) + content;
            }

            return content;
        }

		public void Log(string message)
		{
			Android.Util.Log.Info("PlayOnCloud", message);
		}
	}
}