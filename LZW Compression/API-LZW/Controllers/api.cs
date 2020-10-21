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
using API_LZW.Models;

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
            try
            {
                #region "Compression"
                string path = _env.ContentRootPath;
                byte[] result = null;
                byte[] copy = null;
                using (var memory = new MemoryStream())
                {
                    await file.CopyToAsync(memory);
                    copy = memory.ToArray();
                    using (FileStream fStream = System.IO.File.Create(path + "/Copy/" + file.FileName))
                    {
                        fStream.Write(memory.ToArray());
                    }
                    result = lzw.Compression(path + "/Copy/" + file.FileName, file.FileName);
                }
                //System.IO.File.Delete(path + "/Copy/" + file.FileName);
                Archive response = new Archive
                {
                    Content = result,
                    ContentType = "compressedFile / huff",
                    FileName = name
                };
                #endregion
                #region "Json"
                LZW_Compressions jsonValues = new LZW_Compressions
                {
                    OriginalName = file.FileName,
                    CompressedFilePath = _env.ContentRootPath + "\\Compressions",
                    CompressionRatio = (double)result.Length / (double)copy.Length,
                    CompressionFactor = (double)copy.Length / (double)result.Length,
                    ReductionPorcentage = 1 - ((double)result.Length / (double)copy.Length)
                };
                JsonFile addToJson = new JsonFile();
                addToJson.WriteInJson(jsonValues, _env.ContentRootPath);
                #endregion
                #region "Path"
                string compressionPath = _env.ContentRootPath + "/Compressions/" + name + ".huff";
                using (FileStream fs = System.IO.File.Create(compressionPath))
                {
                    fs.Write(result);
                }
                #endregion
                return File(response.Content, response.ContentType, response.FileName + ".huff");
            }
            catch (System.Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [Route("decompress")]
        public async Task<ActionResult> Decompression([FromForm] IFormFile file)
        {
            string path = _env.ContentRootPath;
            byte[] result = null;
            string originalName = "";
            using (var memory = new MemoryStream())
            {
                await file.CopyToAsync(memory);
                byte[] bytes = memory.ToArray();
                using (FileStream fStream = System.IO.File.Create(path + "/Copy/" + file.FileName))
                {
                    fStream.Write(memory.ToArray());
                }
                //originalName = lzw.GetOriginalName(path + "/Copy/" + file.FileName);
                result = lzw.Decompression(path + "/Copy/" + file.FileName);
            }

            return Ok();
        }

        [HttpGet]
        [Route("compressions")]
        public ActionResult Compressions()
        {
            List<LZW_Compressions> list = new List<LZW_Compressions>();
            JsonFile addToJson = new JsonFile();
            if (System.IO.File.Exists(_env.ContentRootPath + "/Compressions.json"))
            {
                using (FileStream fileRead = System.IO.File.OpenRead(_env.ContentRootPath + "/Compressions.json"))
                {
                    string result = "";
                    MemoryStream memory = new MemoryStream();
                    fileRead.CopyTo(memory);
                    result = Encoding.ASCII.GetString(memory.ToArray());
                    list = addToJson.Deselearize(result);
                }
                return Ok(list);
            }
            else
            {
                return StatusCode(500);
            }
        }
    }
}
