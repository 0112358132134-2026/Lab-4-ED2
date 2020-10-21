namespace LZW_structures
{
    interface ILZW
    {
        public byte[] Compression(string path, string originalName);
        public byte[] Decompression(string path);
    }
}
