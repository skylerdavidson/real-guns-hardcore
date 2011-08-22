namespace ZMP.Decorate
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class DecorateFileInfo
    {
        private FileInfo fileInfo;
        private DecorateFileType origin;

        public DecorateFileInfo(FileInfo fileInfo, DecorateFileType origin)
        {
            this.fileInfo = fileInfo;
            this.origin = origin;
        }

        public FileInfo FileInfo
        {
            get
            {
                return this.fileInfo;
            }
        }

        public DecorateFileType Origin
        {
            get
            {
                return this.origin;
            }
        }
    }
}
