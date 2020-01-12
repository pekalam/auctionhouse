using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Core.Common;

namespace Infrastructure.Services
{
    class TempFileService : ITempFileService
    {
        public string SaveAsTempFile(Stream stream)
        {
            var tempFile = Path.GetTempFileName();
            using (var fs = File.OpenWrite(tempFile))
            {
                stream.CopyTo(fs);
            }
            return tempFile;
        }
    }
}
