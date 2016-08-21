using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptanalysis
{
    public static class Transformation
    {
        public static Character[,] AsMatrix(this List<Character> input, int m, int n)
        {
            Character[,] output = new Character[m, n];
            Parallel.For(0, m, row =>
            {
                for (int column = 0; column < n; column++)
                {
                    output[row, column] = input[row * n + column];
                }
            });
            return output;
        }

        public static List<Character> AsColumnsToRows(this List<Character> input, int m, int n)
        {
            List<Character> output = new List<Character>();
            for (int column = 0; column < n; column++)
            {
                for (int row = 0; row < m; row++)
                {
                    output.Add(input[row * n + column]);
                }
            }
            return output;
        }

        public static List<Character> AsSpiral(this List<Character> input, int m, int n)
        {
            Character[,] a = input.AsMatrix(m, n);
            int i, k = 0, l = 0;
            List<Character> output = new List<Character>();

            /*  k - starting row index
                m - ending row index
                l - starting column index
                n - ending column index
                i - iterator
            */

            while (k < m && l < n)
            {
                /* Print the first row from the remaining rows */
                for (i = l; i < n; ++i)
                {
                    output.Add(a[k, i]);
                }
                k++;

                /* Print the last column from the remaining columns */
                for (i = k; i < m; ++i)
                {
                    output.Add(a[i, n - 1]);
                }
                n--;

                /* Print the last row from the remaining rows */
                if (k < m)
                {
                    for (i = n - 1; i >= l; --i)
                    {
                        output.Add(a[m - 1, i]);
                    }
                    m--;
                }

                /* Print the first column from the remaining columns */
                if (l < n)
                {
                    for (i = m - 1; i >= k; --i)
                    {
                        output.Add(a[i, l]);
                    }
                    l++;
                }

            }
            return output;
        }

    }

}
