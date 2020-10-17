using System.Collections.Generic;
namespace LZW_structures
{
    interface ILZW
    {
        public byte[] Compression(string path, string originalName);
        public List<char> Decompression(List<byte> bytes);
    }
}
