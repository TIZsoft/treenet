using System;
using System.IO;

namespace Tizsoft.IO
{
    public static class IOUtil
    {
        public static string CombinePathAndFile(string path, string file)
        {
            path = path.Replace("\\", "/");
            return string.Format(path.EndsWith("/") ? "{0}{1}" : "{0}/{1}", path, file);
        }

        public static bool CheckAndCreateDirectory(string path)
        {
            var dirPath = Path.GetDirectoryName(path);
            if (string.IsNullOrWhiteSpace(dirPath))
            {
                return false;
            }

            if (Directory.Exists(dirPath))
            {
                return true;
            }

            try
            {
                Directory.CreateDirectory(dirPath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
