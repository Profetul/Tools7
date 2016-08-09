using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Cryptanalysis
{
    public static class Keys
    {
        public static readonly BigInteger[,] M5x5x1033 = new BigInteger[,]
            {{272,138,341,131,151},
            {366,199,130,320,18},
            {226,245,91,245,226},
            {18,320,130,199,366},
            {151,131,341,138,272}};

        public static readonly Word KeyM51033 = (new int[] {
            2,7,2,1,3,8,3,4,1,1,3,1,1,5,1,
            3,6,6,1,9,9,1,3,0,3,2,0,1,8,
            2,2,6,2,4,5,9,1,2,4,5,2,2,6,
            1,8,3,2,0,1,3,0,1,9,9,3,6,6,
            1,5,1,1,3,1,3,4,1,1,3,8,2,7,2}).AsWord();

        public static readonly BigInteger[,] M295x5x1033 = M5x5x1033.Mod(29);
        public static readonly BigInteger[,] M295x5x1033_I = M295x5x1033.ModInverse(29);


        public static readonly BigInteger[,] M5x5x1033_I = M5x5x1033.ModInverse(29);

        public static readonly BigInteger[,] M5x5x3301 = new BigInteger[,]
            {{434,1311,312,278,966},
            {204,812,934,280,1071},
            {626,620,809,620,626},
            {1071,280,934,812,204},
            {966,278,312,1311,434}};

        public static readonly Word KeyM53301 = (new int[] {
            4,3,4,1,3,1,1,3,1,2,2,7,8,9,6,6,
            2,0,4,8,1,2,9,3,4,2,8,0,1,0,7,1,
            6,2,6,6,2,0,8,0,9,6,2,0,6,2,6,
            1,0,7,1,2,8,0,9,3,4,8,1,2,2,0,4,
            9,6,6,2,7,8,3,1,2,1,3,1,1,4,3,4}).AsWord();

        public static readonly BigInteger[,] M5x5x3301_I = M5x5x3301.ModInverse(29);

        public static readonly BigInteger[,] M7x7x3301 = new BigInteger[,]
            {{7,375,236,190,27,17,181},
            {351,223,14,47,293,98,7},
            {456,232,121,114,72,23,15},
            {16,65,270,331,270,65,16},
            {15,23,72,114,121,232,456},
            {7,98,293,47,14,223,351},
            {181,17,27,190,236,375,7}};

        public static readonly Word KeyM73301 = (new int[] {
            7,3,7,5,2,3,6,1,9,0,2,7,1,7,1,8,1,
            3,5,1,2,2,3,1,4,4,7,2,9,3,9,8,7,
            4,5,6,2,3,2,1,2,1,1,1,4,7,2,2,3,1,5,
            1,6,6,5,2,7,0,3,3,1,2,7,0,6,5,1,6,
            1,5,2,3,7,2,1,1,4,1,2,1,2,3,2,4,5,6,
            7,9,8,2,9,3,4,7,1,4,2,2,3,3,5,1,
            1,8,1,1,7,2,7,1,9,0,2,3,6,3,7,5,7
        }).AsWord();

        public static readonly BigInteger[,] M7x7x3301_I = M7x7x3301.ModInverse(29);
    }
}
