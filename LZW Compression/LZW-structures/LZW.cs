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

        public byte[] CompressedText(int maxValue, string originalName)
        {
            string binaryMajor = ConvertDecimalToBinary(maxValue);
            StringBuilder binaries = new StringBuilder();
            for (int i = 0; i < textInNumbers.Count; i++)
            {
                StringBuilder binary = new StringBuilder();
                binary.Append(ConvertDecimalToBinary(textInNumbers[i]));
                if (binary.Length < binaryMajor.Length)
                {
                    for (int j = binary.Length; j < binaryMajor.Length; j++)
                    {
                        binary.Insert(0,"0");
                    }                    
                }
                binaries.Append(binary.ToString());
            }
            //Con el string "binaries" individuales de cada número, dividimos de 8 en 8 bits: 
            List<string> bytesToCompressedText = SeparateBytes(binaries.ToString());
            //Convertimos los bytes (en binario) a decimales y los agregamos a otra lista:
            List<int> bytesCompressedTex = new List<int>();
            for (int i = 0; i < bytesToCompressedText.Count; i++)
            {
                bytesCompressedTex.Add(ConvertBinaryToDecimal(bytesToCompressedText[i]));
            }
            //Obtenemos la tabla original en bytes:
            List<int> originalTableToBytes = OriginalTableToBytes();
            //Agregamos todos los bytes al arreglo resultante:
            List<int> auxResult = new List<int>();

            List<byte> oName = BytesToOriginalName(originalName);
            for (int i = 0; i < oName.Count; i++)
            {
                auxResult.Add((int)oName[i]);
            }

            auxResult.Add(binaryMajor.Length);
            auxResult.Add(posOriginalData);
            for (int i = 0; i < originalTableToBytes.Count; i++)
            {
                auxResult.Add(originalTableToBytes[i]);
            }
            for (int i = 0; i < bytesCompressedTex.Count; i++)
            {
                auxResult.Add(bytesCompressedTex[i]);
            }           
            byte[] result = new byte[oName.Count + 2 + originalTableToBytes.Count + bytesCompressedTex.Count];
            for (int i = 0; i < auxResult.Count; i++)
            {
                result[i] = (byte)auxResult[i];
            }
            return result;
        }

        public List<int> OriginalTableToBytes()
        {
            List<string> characters = new List<string>();
            List<int> result = new List<int>();
            int counter = 0;
            foreach (KeyValuePair<string, int> symbol in symbols)
            {
                while (counter < posOriginalData)
                {
                    characters.Add(symbol.Key);
                    counter++;
                }
            }
            //
            for (int i = 0; i < characters.Count; i++)
            {
                char[] oka = characters[i].ToCharArray();
                result.Add((byte)oka[0]);
            }
            return result;
        }

        public List<string> SeparateBytes(string largeBinary)
        {
            StringBuilder copy = new StringBuilder();
            copy.Append(largeBinary);
            List<string> result = new List<string>();
            bool OK = false;
            while (!OK)
            {
                if (copy.Length >= 8)
                {
                    result.Add(copy.ToString(0, 8));
                    copy.Remove(0, 8);
                }
                else
                {
                    if (copy.Length > 0)
                    {
                        for (int i = copy.Length; i < 8; i++)
                        {
                            copy.Append("0");
                        }
                        result.Add(copy.ToString());
                    }                   
                    OK = true;
                }
            }
            return result;
        }

        public List<byte> BytesToOriginalName(string originalName)
        {
            List<byte> listAux = new List<byte>();
            for (int i = 0; i < originalName.Length; i++)
            {
                char ok = (char)originalName[i];
                listAux.Add((byte)ok);
            }
            listAux.Add(10);
            return listAux;
        }


        public byte[] Compression(string path, string originalName)
        {
            OriginalTable(path);
            TextInNumbers(path);
            int maxNumber = textInNumbers.Max();
            byte[] compressedText = CompressedText(maxNumber, originalName);
            return compressedText;
        }

        public List<char> Decompression(List<byte> bytes)
        {
            return null;
        }

        #region "Auxiliaries"
        public int ConvertBinaryToDecimal(string binary)
        {
            int exponent = binary.Length - 1;
            int decimalNumber = 0;

            for (int i = 0; i < binary.Length; i++)
            {
                if (int.Parse(binary.Substring(i, 1)) == 1)
                {
                    decimalNumber += int.Parse(System.Math.Pow(2, double.Parse(exponent.ToString())).ToString());
                }
                exponent--;
            }
            return decimalNumber;
        }
        public string ConvertDecimalToBinary(int number)
        {
            string result = "";
            while (number > 0)
            {
                if (number % 2 == 0)
                {
                    result = "0" + result;
                }
                else
                {
                    result = "1" + result;
                }
                number = (int)(number / 2);
            }
            return result;
        }
        #endregion
    }
}
