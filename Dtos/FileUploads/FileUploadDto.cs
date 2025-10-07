using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dtos.FileUploads
{
    public class FileUploadDto
    {
        public IFormFile File { get; set; }
    }
}
