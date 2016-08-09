using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Solver
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
                        Rune = Constants.INDEXED_RUNES[(int)BigInteger.Remainder(resultVector[column], 29)]
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
                Rune = Constants.INDEXED_RUNES[(input[0].GematriaIndex + offset) % 29]
            });


            for (int i = 1; i < input.Count; i++)
            {
                output.Add(new Character
                {
                    Type = CharacterType.Rune,
                    Rune = Constants.INDEXED_RUNES[(input[i].GematriaIndex + input[i - 1].GematriaIndex) % 29]
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
                Rune = Constants.INDEXED_RUNES[(input[0].GematriaIndex + offset) % 29]
            });


            for (int i = 1; i < input.Count; i++)
            {
                output.Add(new Character
                {
                    Type = CharacterType.Rune,
                    Rune = Constants.INDEXED_RUNES[(input[i].GematriaIndex + output[i - 1].GematriaIndex) % 29]
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
                Rune = Constants.INDEXED_RUNES[(29 + input[0].GematriaIndex - offset) % 29]
            });


            for (int i = 1; i < input.Count; i++)
            {
                output.Add(new Character
                {
                    Type = CharacterType.Rune,
                    Rune = Constants.INDEXED_RUNES[(29 + input[i].GematriaIndex - output[i - 1].GematriaIndex) % 29]
                });
            }
            return output;
        }
    }
}
