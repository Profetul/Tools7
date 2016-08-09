using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class Character
    {
        public CharacterType Type { get; set; }

        public char Rune { get; set; }

        public string Latin
        {
            get
            {
                return Type == CharacterType.Rune ? Alphabets.GEMATRIA[Rune] : Rune.ToString();
            }
        }

        public int GematriaIndex
        {
            get
            {
                return Type == CharacterType.Rune ? Alphabets.RUNE_INDEX[Rune] : (int)Type;
            }
        }

        public int PrimeValue
        {
            get
            {
                return Type == CharacterType.Rune ? Alphabets.RUNE_PRIME[Rune] : 0;
            }
        }

        public static bool operator ==(Character o1, Character o2)
        {
            return o1?.Equals(o2) == true || (o1 == null && o2 == null);
        }

        public static bool operator !=(Character o1, Character o2)
        {
            return !(o1?.Equals(o2) == true);
        }

        public static Character operator +(Character o1, int shift)
        {
            if (o1 == null)
            {
                return null;
            }

            if (o1.Type != CharacterType.Rune)
            {
                return o1;
            }

            var newIndex = Math.Abs(29 + o1.GematriaIndex + shift) % 29;
            return new Character { Type = CharacterType.Rune, Rune = Alphabets.GEMATRIA.Keys.ToArray()[newIndex] };
        }


        public static int operator -(Character o1, Character o2)
        {
            if (o1 == null)
            {
                return -o2.GematriaIndex;
            }

            if (o2 == null)
            {
                return o1.GematriaIndex;
            }

            return o1.GematriaIndex - o2.GematriaIndex;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is Character)
            {
                return this.Rune == ((Character)obj).Rune;
            }

            return false;
        }

        public override int GetHashCode()
        {
            return Rune.GetHashCode();
        }

        public override string ToString()
        {
            return Latin;
        }
    }
}
