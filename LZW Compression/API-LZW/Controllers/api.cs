using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.Extensions.Localization.Internal;
using LZW_structures;

namespace API_LZW.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class api : ControllerBase
    {
        private IWebHostEnvironment _env;
        public api (IWebHostEnvironment env)
        {
            _env = env;
        }

        LZW lzw = new LZW();

        [HttpPost]
        [Route("compress/{name}")]
        public async Task<ActionResult> Compression([FromForm] IFormFile file, string name)
        {
            string path = _env.ContentRootPath;
            byte[] result = null;
            using (var memory = new MemoryStream())
            {
                await file.CopyToAsync(memory);
                using (FileStream fStream = System.IO.File.Create(path + "/Copy/" + file.FileName))
                {
                    fStream.Write(memory.ToArray());
                }
                result = lzw.Compression(path + "/Copy/" + file.FileName, file.FileName);
            }
            return Ok();
        }
    }
}
