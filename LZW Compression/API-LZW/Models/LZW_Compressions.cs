namespace API_LZW.Models
{
    public class LZW_Compressions
    {
        public string OriginalName { get; set; }
        public string CompressedFilePath { get; set; }
        public double CompressionRatio { get; set; }
        public double CompressionFactor { get; set; }
        public double ReductionPorcentage { get; set; }
    }
}
