using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public enum CharacterType
    {
        Unspecified = -1,
        Rune = 1,
        Space = 3 ^ 5,
        Number = 5 ^ 3,
        Dot = 7 ^ 2,
        SingleQuote = 11 ^ 2,
        DoubleQuote = 13 ^ 2,
        Colon = 17 ^ 2,
        Delimiter = 23 ^ 2,
    }
}
