using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LZW_structures
{
    public class LZW : ILZW
    {
        #region "Global_Variables"
        public Dictionary<string, int> symbols = new Dictionary<string, int>();
        public List<int> textInNumbers = new List<int>();
        public int index = 1;
        public int posOriginalData = 0;
        #endregion

        public void OriginalTable(string path)
        {
            int counter = 0;
            FileStream fs = File.OpenRead(path);
            BinaryReader reader = new BinaryReader(fs);

            while (fs.Length - (8 * counter) > 0)
            {
                byte[] pieceOfText = reader.ReadBytes(8);
                for (int i = 0; i < pieceOfText.Length; i++)
                {
                    if (!symbols.ContainsKey(((char)pieceOfText[i]).ToString()))
                    {
                        char character = (char)pieceOfText[i];
                        symbols.Add(character.ToString(), index);
                        index++;
                    }
                }
                counter++;
            }
            posOriginalData = index - 1;
        }

        public void TextInNumbers(string path)
        {
            int counter = 0;
            FileStream fs = File.OpenRead(path);
            BinaryReader reader = new BinaryReader(fs);

            StringBuilder longesChain = new StringBuilder();
            StringBuilder longesChainPlusOne = new StringBuilder();

            while (fs.Length - (8 * counter) > 0)
            {
                byte[] pieceOfText = reader.ReadBytes(8);
                int counterTwo = 0;

                while (counterTwo < pieceOfText.Length)
                {
                    longesChainPlusOne.Append(((char)pieceOfText[counterTwo]).ToString());
                    if (!symbols.ContainsKey(longesChainPlusOne.ToString()))
                    {
                        textInNumbers.Add(symbols[longesChain.ToString()]);
                        symbols.Add(longesChainPlusOne.ToString(), index);
                        index++;
                        longesChainPlusOne.Clear();
                        longesChain.Clear();
                        counterTwo -= 1;
                    }
                    else
                    {
                        longesChain.Append(((char)pieceOfText[counterTwo]).ToString());
                    }
                    counterTwo++;
                }
                counter++;
            }
            if (longesChain.Length > 0)
            {
                textInNumbers.Add(symbols[longesChain.ToString()]);
            } 
        }

        public byte[] CompressedText()
        {
            return null;
        }

        public byte[] Compression(string path, string originalName)
        {
            OriginalTable(path);
            TextInNumbers(path);
            int maxNumber = textInNumbers.Max();

            return null;
        }


        public List<char> Decompression(List<byte> bytes)
        {
            return null;
        }
    }
}
