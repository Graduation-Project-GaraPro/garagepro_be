using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Dtos.FileUploads
{
    public class MultipleFileUploadDto
    {
        public List<IFormFile> Files { get; set; }
    }
}
