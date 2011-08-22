namespace ZMP.Utilities.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;    

    public static class DirectoryInfoExtensions
    {
        public static IEnumerable<FileInfo> GetFilesRecursive(this DirectoryInfo dirInfo)
        {
            return GetFilesRecursive(dirInfo, "*.*");
        }

        public static IEnumerable<FileInfo> GetFilesRecursive(this DirectoryInfo dirInfo, string searchPattern)
        {
            // ignore hidden directories (these are usually owned by version control software)
            if ((dirInfo.Attributes & FileAttributes.Hidden) == 0)
            {
                foreach (DirectoryInfo di in dirInfo.GetDirectories())
                {
                    foreach (FileInfo fi in GetFilesRecursive(di, searchPattern))
                    {
                        yield return fi;
                    }
                }

                foreach (FileInfo fi in dirInfo.GetFiles(searchPattern))
                {
                    yield return fi;
                }
            }
        }
    }
}
