using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
namespace API_LZW.Models
{
    public class JsonFile
    {
        public void WriteInJson(LZW_Compressions newCompression, string pathToWrite)
        {
            pathToWrite += "/Compressions.json";
            //Creamos una lista de objetos "Archive"
            List<LZW_Compressions> listAux = new List<LZW_Compressions>();
            //Si el archivo ya existe, entonces lo leemos
            if (File.Exists(pathToWrite))
            {
                using FileStream fileRead = File.OpenRead(pathToWrite);
                string result = "";
                MemoryStream memory = new MemoryStream();
                fileRead.CopyTo(memory);
                //Le asignamos al string "result" lo que va a leer...
                result = Encoding.ASCII.GetString(memory.ToArray());
                //Agregamos a la lista todos los objetos que ya contiene:
                listAux = Deselearize(result);
            }
            //Si no existe, solo agregamos por primera vez el objeto:
            listAux.Add(newCompression);
            //Mandamos a escribir nuevamente en el jason:
            using StreamWriter toWrite = new StreamWriter(pathToWrite);
            toWrite.Write(Serialize(listAux));
        }
        public List<LZW_Compressions> Deselearize(string objects)
        {
            return JsonSerializer.Deserialize<List<LZW_Compressions>>(objects, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        public string Serialize(List<LZW_Compressions> objects)
        {
            return JsonSerializer.Serialize(objects, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
    }
}
