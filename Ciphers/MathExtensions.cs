using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Cryptanalysis
{
    public static class MatrixExtensions
    {
        public static BigInteger Determinant(this BigInteger[,] input)
        {
            int order = (int)Math.Sqrt(input.Length);
            if (order > 2)
            {
                BigInteger value = 0;
                for (int j = 0; j < order; j++)
                {
                    BigInteger[,] Temp = input.Minor(0, j);
                    value = value + input[0, j] * (SignOfElement(0, j) * Determinant(Temp));
                }
                return value;
            }
            else if (order == 2)
            {
                return ((input[0, 0] * input[1, 1]) - (input[1, 0] * input[0, 1]));
            }
            else
            {
                return (input[0, 0]);
            }
        }

        static int SignOfElement(int i, int j)
        {
            if ((i + j) % 2 == 0)
            {
                return 1;
            }
            else
            {
                return -1;
            }
        }

        public static BigInteger[,] Minor(this BigInteger[,] input, int i, int j)
        {
            int order = (int)Math.Sqrt(input.Length);
            BigInteger[,] output = new BigInteger[order - 1, order - 1];
            int x = 0, y = 0;
            for (int m = 0; m < order; m++, x++)
            {
                if (m != i)
                {
                    y = 0;
                    for (int n = 0; n < order; n++)
                    {
                        if (n != j)
                        {
                            output[x, y] = input[m, n];
                            y++;
                        }
                    }
                }
                else
                {
                    x--;
                }
            }

            return output;
        }

        public static BigInteger[,] Transpose(this BigInteger[,] input)
        {
            int size = (int)Math.Sqrt(input.Length);
            BigInteger[,] output = new BigInteger[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    output[column, row] = input[row, column];
                }
            }
            return output;
        }

        public static BigInteger[,] Mod(this BigInteger[,] input, BigInteger modulo)
        {
            int size = (int)Math.Sqrt(input.Length);
            BigInteger[,] output = new BigInteger[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    output[row, column] = BigInteger.Remainder(input[row, column], modulo);
                }
            }
            return output;
        }

        public static BigInteger[,] MultiplyAndMod(this BigInteger[,] input, BigInteger multiplicator, BigInteger modulo)
        {
            int size = (int)Math.Sqrt(input.Length);
            BigInteger[,] output = new BigInteger[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    output[row, column] = BigInteger.Remainder(modulo + BigInteger.Remainder(BigInteger.Multiply(input[row, column], multiplicator), modulo), modulo);
                }
            }
            return output;
        }

        public static BigInteger[] Multiply(this BigInteger[,] input, BigInteger[] vector)
        {
            int size = (int)Math.Sqrt(input.Length);
            if (vector.Length < size)
            {
                return vector;
            }

            BigInteger[] output = new BigInteger[size];
            for (int row = 0; row < size; row++)
            {
                for (int colum = 0; colum < size; colum++)
                {
                    output[row] = output[row] + input[row, colum] * vector[colum];
                }
            }
            return output;
        }


        public static BigInteger[,] ModInverse(this BigInteger[,] input, int modulo = 29)
        {
            var size = (int)Math.Sqrt(input.Length);
            var adjoint = new BigInteger[size, size];
            for (int row = 0; row < size; row++)
            {
                for (int column = 0; column < size; column++)
                {
                    adjoint[row, column] = input.Minor(row, column).Determinant();
                    if ((row + column) % 2 == 1)
                    {
                        adjoint[row, column] = -adjoint[row, column];
                    }
                }
            }

            var determinant = input.Determinant();
            var multInv = 0;

            for (int index = 0; index < modulo; index++)
            {
                if ((index * determinant * determinant.Sign) % modulo == 1)
                {
                    if (determinant.Sign < 0)
                    {
                        multInv = modulo - index;
                    }
                    else
                    {
                        multInv = index;
                    }
                    break;
                }
            }

            return adjoint.Transpose().MultiplyAndMod(multInv, modulo);
        }
    }
}
