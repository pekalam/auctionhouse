using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Core.Common;
using Microsoft.AspNetCore.Http;
using Web.Exceptions;

namespace Web.Adapters
{
    public class FileStreamAccessor : IFileStreamAccessor
    {
        private IFormFile _formFile;

        public FileStreamAccessor(IFormFile formFile)
        {
            this._formFile = formFile ?? throw new ApiException(HttpStatusCode.BadRequest, "Null file");
        }

        public Stream GetStream()
        {
            return _formFile.OpenReadStream();
        }
    }
}
