using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Web.Exceptions;

namespace Web.Utils
{
    public static class FileExtensionUtil
    {
        public static string GetFileExtensionOrThrow400(string fileName)
        {
            var arr = fileName.Split('.', StringSplitOptions.RemoveEmptyEntries);
            if (arr.Length == 0)
            {
                throw new ApiException(HttpStatusCode.BadRequest, "Invalid file extension");
            }
            else
            {
                return arr.Last();
            }
        }
    }
}
