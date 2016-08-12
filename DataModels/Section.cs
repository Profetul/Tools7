using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class Section : List<Paragraph>
    {
        private string _inRunes;
        public string Runes
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_inRunes))
                {
                    _inRunes = String.Join("\r\n\r\n", this.Select(c => c.Runes));
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
                    _inLatin = String.Join("\r\n\r\n", this.Select(c => c.Latin));
                }
                return _inLatin;
            }
        }

        public long PrimeSum
        {
            get
            {
                return this.Select(c => c.PrimeSum).Sum();
            }
        }

        public BigInteger PrimeMultSum
        {
            get
            {
                return this.Where(p => p.PrimeMult > 0).Aggregate((BigInteger)0, (a, c) => a + c.PrimeMult);

            }
        }

        private List<Word> _words;
        public List<Word> Words
        {
            get
            {
                if (_words == null)
                {
                    _words = this.Aggregate(new List<Word>(), (a, p) =>
                    {
                        a.AddRange(p.Words.Where(w => w.Any(c => c.Type == CharacterType.Rune)));
                        return a;
                    });
                }
                return _words;
            }
        }

        private List<Word> _reversedWords;
        public List<Word> ReversedWords
        {
            get
            {
                if (_reversedWords == null)
                {
                    _reversedWords = this.Aggregate(new List<Word>(), (a, p) =>
                    {
                        a.AddRange(p.ReversedWords.Where(w => w.Any(c => c.Type == CharacterType.Rune)));
                        return a;
                    });
                }
                return _reversedWords;
            }
        }

        private List<Character> _characters;
        public List<Character> Characters
        {
            get
            {
                if (_characters == null)
                {
                    _characters = Words.Aggregate(new List<Character>(), (a, w) =>
                    {
                        if (w.Any(c => c.Type == CharacterType.Rune))
                        {
                            a.AddRange(w);
                        }
                        return a;
                    });
                }
                return _characters;
            }
        }

        public override string ToString()
        {
            return Latin;
        }

        public string ToPrimeSumString()
        {
            return String.Join("\r\n", this.Select(w => w.ToPrimeSumString())) + "\r\nSection Total (sums): " + PrimeSum + "\r\nSection Total (mult-sums): " + PrimeMultSum + "\r\n";
        }

        public string ToPrimesString()
        {
            return String.Join("\r\n", this.Select(c => c.ToPrimesString())) + "\r\n";
        }
    }

}
