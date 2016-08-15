using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Cryptanalysis
{
    public static class Ciphers
    {
        public static List<Character> HillCipher(BigInteger[,] keyMatrix, List<Character> input)
        {
            int size = (int)Math.Sqrt(keyMatrix.Length);
            int offset = 0;
            List<Character> output = new List<Character>();
            while (offset < input.Count)
            {
                var plainVector = input.Skip(offset).Take(size).Select(c => (BigInteger)c.GematriaIndex).ToArray();
                var resultVector = keyMatrix.Multiply(plainVector);
                for (int column = 0; column < size; column++)
                {
                    output.Add(new Character
                    {
                        Type = CharacterType.Rune,
                        Rune = DataModels.Alphabets.INDEXED_RUNES[(int)BigInteger.Remainder(resultVector[column], 29)]
                    });
                }
                offset = offset + size;
            }
            return output;
        }
        public static List<Character> EncodeCondi(List<Character> input, int offset = 10)
        {
            List<Character> output = new List<Character>();
            output.Add(new Character
            {
                Type = CharacterType.Rune,
                Rune = DataModels.Alphabets.INDEXED_RUNES[(input[0].GematriaIndex + offset) % 29]
            });


            for (int i = 1; i < input.Count; i++)
            {
                output.Add(new Character
                {
                    Type = CharacterType.Rune,
                    Rune = DataModels.Alphabets.INDEXED_RUNES[(input[i].GematriaIndex + input[i - 1].GematriaIndex) % 29]
                });
            }
            return output;
        }

        public static List<Character> EncodeCondiNew(List<Character> input, int offset = 10)
        {
            List<Character> output = new List<Character>();
            output.Add(new Character
            {
                Type = CharacterType.Rune,
                Rune = DataModels.Alphabets.INDEXED_RUNES[(input[0].GematriaIndex + offset) % 29]
            });


            for (int i = 1; i < input.Count; i++)
            {
                output.Add(new Character
                {
                    Type = CharacterType.Rune,
                    Rune = DataModels.Alphabets.INDEXED_RUNES[(input[i].GematriaIndex + output[i - 1].GematriaIndex) % 29]
                });
            }
            return output;
        }

        public static List<Character> DecodeNew(List<Character> input, int offset = 10)
        {
            List<Character> output = new List<Character>();
            output.Add(new Character
            {
                Type = CharacterType.Rune,
                Rune = DataModels.Alphabets.INDEXED_RUNES[(29 + input[0].GematriaIndex - offset) % 29]
            });


            for (int i = 1; i < input.Count; i++)
            {
                output.Add(new Character
                {
                    Type = CharacterType.Rune,
                    Rune = DataModels.Alphabets.INDEXED_RUNES[(29 + input[i].GematriaIndex - output[i - 1].GematriaIndex) % 29]
                });
            }
            return output;
        }

        public static List<Character> EncodeVigenere(List<Character> key, List<Character> input)
        {
            List<Character> output = new List<Character>(input.Count);
            output.AddRange(input);
            Parallel.For(0, input.Count, (index) =>
            {
                var newIndex = (key[index % key.Count].GematriaIndex + input[index].GematriaIndex) % 29;
                output[index] = new Character { Rune = Alphabets.INDEXED_RUNES[newIndex] };
            });
            return output;
        }

        public static List<Character> DecodeVigenere(List<Character> key, List<Character> input)
        {
            List<Character> output = new List<Character>();
            output.AddRange(input);
            Parallel.For(0, input.Count, (index) =>
            {
                var newIndex = (input[index].GematriaIndex - key[index % key.Count].GematriaIndex) % 29;
                output[index] = new Character { Rune = Alphabets.INDEXED_RUNES[newIndex < 0 ? 29 + newIndex : newIndex] };
            });
            return output;
        }

        public static List<Character> EncodeAutokey(List<Character> key, List<Character> input)
        {
            List<Character> output = new List<Character>(input.Count);
            output.AddRange(input);

            List<Character> autokey = new List<Character>(key.Count + input.Count);
            autokey.AddRange(key);
            autokey.AddRange(input);
            return EncodeVigenere(autokey, input);
        }

        public static List<Character> DecodeAutokey(List<Character> key, List<Character> input)
        {
            var newKey = key;
            int offset = 0;
            List<Character> output = new List<Character>(input.Count);
            while (offset < input.Count)
            {
                newKey = DecodeVigenere(newKey, input.Skip(offset).Take(key.Count).ToList());
                output.AddRange(newKey);
                offset += newKey.Count;
            }
            return output;
        }
        public static List<Character> DecodeAntiAutokey(List<Character> key, List<Character> input)
        {
            var newKey = key;
            int offset = 0;
            List<Character> output = new List<Character>(input.Count);
            while (offset < input.Count)
            {
                newKey = EncodeVigenere(newKey, input.Skip(offset).Take(key.Count).ToList());
                output.AddRange(newKey);
                offset += newKey.Count;
            }
            return output;
        }
    }
}

