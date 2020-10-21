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
        public Dictionary<int, string> symbolsAux = new Dictionary<int, string>();
        public List<int> textInNumbers = new List<int>();
        public int index = 1;
        public int posOriginalData = 0;
        #endregion
        #region "Compression"
        public byte[] Compression(string path, string originalName)
        {
            OriginalTable(path);
            TextInNumbers(path);
            int maxNumber = textInNumbers.Max();
            byte[] compressedText = CompressedText(maxNumber, originalName);
            return compressedText;
        }
        public void OriginalTable(string path)
        {
            int counter = 0;
            FileStream fs = File.OpenRead(path);
            BinaryReader reader = new BinaryReader(fs);

            while (fs.Length - (8 * counter) > 0)
            {
                byte[] buffer = reader.ReadBytes(8);
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (!symbols.ContainsKey(((char)buffer[i]).ToString()))
                    {
                        char character = (char)buffer[i];
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
                byte[] buffer = reader.ReadBytes(8);
                int counterTwo = 0;

                while (counterTwo < buffer.Length)
                {
                    longesChainPlusOne.Append(((char)buffer[counterTwo]).ToString());
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
                        longesChain.Append(((char)buffer[counterTwo]).ToString());
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
            List<string> bytesToCompressedText = SeparateBytes(binaries.ToString(),8);
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
                if(counter < posOriginalData)
                {
                    characters.Add(symbol.Key);
                    counter++;
                }
            }            
            for (int i = 0; i < characters.Count; i++)
            {
                char[] oka = characters[i].ToCharArray();
                result.Add((byte)oka[0]);
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
        #endregion
        #region "Descompression"
        public byte[] Decompression(string path)
        {
            FormOriginalTable(path);
            byte[] result = GetOriginalText(path);           
            return result;
        }
        public void FormOriginalTable(string path)
        {
            int metaDataLength = GetOriginalName(path).Length + 2;
            int numberOfCharacters = 0;

            FileStream fs = File.OpenRead(path);
            BinaryReader reader = new BinaryReader(fs);

            bool matchNumberOfCharacters = false;
            int counter = 0;

            while (!matchNumberOfCharacters)
            {
                byte[] buffer = reader.ReadBytes(8);
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (counter == metaDataLength)
                    {
                        numberOfCharacters = buffer[i];
                        matchNumberOfCharacters = true;
                    }
                    counter++;
                }
            }

            FileStream fs1 = File.OpenRead(path);
            BinaryReader reader1 = new BinaryReader(fs1);

            bool allInserted = false;
            counter = 0;

            while (!allInserted)
            {
                byte[] buffer = reader1.ReadBytes(8);
                for (int i = 0; i < buffer.Length; i++)
                {
                    if ((counter >= metaDataLength + 1) && (index <= numberOfCharacters))
                    {
                        char character = (char)buffer[i];
                        symbolsAux.Add(index, character.ToString());
                        index++;
                    }
                    counter++;
                }
                if (index == numberOfCharacters + 1)
                {
                    allInserted = true;
                }
            }
        }
        public string GetOriginalName(string path)
        {
            FileStream fs = File.OpenRead(path);
            BinaryReader reader = new BinaryReader(fs);

            StringBuilder result = new StringBuilder();
            bool matchTen = false;

            while (!matchTen)
            {
                byte[] buffer = reader.ReadBytes(8);
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (buffer[i] == 10)
                    {
                        matchTen = true;
                    }
                    else
                    {
                        if (!matchTen)
                        {
                            char aux = (char)buffer[i];
                            result.Append(aux.ToString());
                        }
                    }
                }
            }
            return result.ToString();
        }
        public byte[] GetOriginalText(string path)
        {
            FillList(path);
            GetNFCT(path);

            StringBuilder auxResult = new StringBuilder();            
            StringBuilder previusString = new StringBuilder();
            StringBuilder currentString = new StringBuilder();
            StringBuilder previousPlusFirst = new StringBuilder();

            for (int i = 0; i < textInNumbers.Count; i++)
            {
                if (i == 0)
                {
                    currentString.Append(symbolsAux[textInNumbers[i]]);
                }
                else
                {
                    if (textInNumbers[i] != 0)
                    {
                        previusString.Clear();
                        previusString.Append(currentString.ToString());
                        currentString.Clear();
                        currentString.Append(symbolsAux[textInNumbers[i]]);
                        previousPlusFirst.Clear();
                        previousPlusFirst.Append(previusString.ToString() + currentString.ToString(0,1));

                        symbolsAux.Add(index, previousPlusFirst.ToString());
                        index++;
                    }
                }
                if (textInNumbers[i] != 0)
                {
                    auxResult.Append(currentString.ToString());
                }
            }

            byte[] result = Encoding.ASCII.GetBytes(auxResult.ToString());
            return result;
        }
        public void FillList(string path)
        {
            int counter = 0;
            int counterAux = 0;
            int metaDataLength = GetOriginalName(path).Length + 3 + symbolsAux.Count;

            FileStream fs = File.OpenRead(path);
            BinaryReader reader = new BinaryReader(fs);

            while (fs.Length - (8 * counter) > 0)
            {
                byte[] buffer = reader.ReadBytes(8);

                for (int i = 0; i < buffer.Length; i++)
                {
                    if (counterAux >= metaDataLength)
                    {
                        textInNumbers.Add(buffer[i]);
                    }
                    counterAux++;
                }
                counter++;
            }
        }
        public void GetNFCT(string path)
        {
            int numberOfBytes = GetNumberOfBits(path);
            StringBuilder bytes = new StringBuilder();
            for (int i = 0; i < textInNumbers.Count; i++)
            {
                StringBuilder aux = new StringBuilder();
                aux.Append(ConvertDecimalToBinary(textInNumbers[i]));
                if (aux.Length < 8)
                {
                    int lenght = aux.Length;
                    for (int j = 0; j < 8 - lenght; j++)
                    {
                        aux.Insert(0, "0");
                    }
                }
                bytes.Append(aux.ToString());
            }
            List<string> binaryNumbers = SeparateBytes(bytes.ToString(), numberOfBytes);
            textInNumbers.Clear();
            for (int i = 0; i < binaryNumbers.Count; i++)
            {
                textInNumbers.Add(ConvertBinaryToDecimal(binaryNumbers[i]));
            }
        }
        public int GetNumberOfBits(string path)
        {
            int metaDataLength = GetOriginalName(path).Length + 1;
            int result = 0;

            FileStream fs = File.OpenRead(path);
            BinaryReader reader = new BinaryReader(fs);

            bool matchNumberOfBits = false;
            int counter = 0;

            while (!matchNumberOfBits)
            {
                byte[] buffer = reader.ReadBytes(8);
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (counter == metaDataLength)
                    {
                        result = buffer[i];
                        matchNumberOfBits = true;
                    }
                    counter++;
                }
            }

            return result;
        }
        #endregion
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
        public List<string> SeparateBytes(string largeBinary, int length)
        {
            StringBuilder copy = new StringBuilder();
            copy.Append(largeBinary);
            List<string> result = new List<string>();
            bool OK = false;
            while (!OK)
            {
                if (copy.Length >= length)
                {
                    result.Add(copy.ToString(0, length));
                    copy.Remove(0, length);
                }
                else
                {
                    if (copy.Length > 0)
                    {
                        for (int i = copy.Length; i < length; i++)
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
        #endregion
    }
}