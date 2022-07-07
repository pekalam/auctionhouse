using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Core.Common
{
    public interface IFileStreamAccessor
    {
        Stream GetStream();
    }
}
