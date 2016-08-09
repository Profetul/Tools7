using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class Word : List<Character>
    {
        private string _inRunes;
        public string Runes
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_inRunes))
                {
                    _inRunes = String.Join("", this.Select(c => c.Rune));
                }
                return _inRunes;
            }
        }

        private string _inLatin;
        public string Latin
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_inLatin))
                {
                    _inLatin = String.Join("-", this.Select(c => c.Latin));
                }
                return _inLatin;
            }
        }

        public long RunSum
        {
            get
            {
                return this.Select(c => c.PrimeValue).Sum();
            }
        }

        public BigInteger GodelNumber
        {
            get
            {
                BigInteger result = 1;
                for (int i = 0; i < Count; i++)
                {
                    result = result * BigInteger.Pow(Alphabets.PRIMES[i], (this[i].GematriaIndex + 1));
                }
                return result;
            }
        }

        private Word _reverced;
        public Word Reverced
        {
            get
            {
                if (_reverced == null)
                {
                    _reverced = new Word();
                    List<Character> reverchedValue = this.ToList();
                    reverchedValue.Reverse();
                    foreach (var c in reverchedValue)
                    {
                        _reverced.Add(c);
                    }
                }
                return _reverced;
            }
        }

        public static Word operator !(Word o1)
        {
            if (o1 == null)
            {
                return null;
            }
            int[] result = new int[o1.Count];
            for (int i = 0; i < o1.Count; i++)
            {
                result[i] = 28 - o1[i].GematriaIndex;
            }
            return result.AsWord();
        }

        public static int[] operator |(Word o1, Word o2)
        {
            if (o1 == null)
            {
                return o2.Select(o => o.GematriaIndex).ToArray();
            }

            if (o2 == null)
            {
                return o1.Select(o => o.GematriaIndex).ToArray();
            }

            var maxCount = Math.Max(o1.Count, o2.Count);
            var minCount = Math.Min(o1.Count, o2.Count);

            int[] result = new int[o1.Count];

            for (int i = 0; i < o1.Count; i++)
            {
                if (i >= o2.Count)
                {
                    result[i] = o1[i].GematriaIndex;
                }
                else
                {
                    result[i] = o1[i].GematriaIndex - o2[i].GematriaIndex;
                }
            }
            return result;
        }


        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is List<Character> && ((List<Character>)obj).Count == this.Count)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    if (!this[i].Equals(((List<Character>)obj)[i]))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {

            return Runes.GetHashCode();
        }

        public override string ToString()
        {
            return Latin;
        }
    }

}
