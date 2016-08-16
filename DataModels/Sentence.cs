using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class Sentence : List<Word>
    {
        private string _inRunes;
        public string Runes
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_inRunes))
                {
                    _inRunes = String.Join("", this.Select(c => c.Runes));
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
                    _inLatin = String.Join(" ", this.Where(w => w.Any(c => c.Type == CharacterType.Rune)).Select(c => c.Latin));
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

        private List<Word> _ReversedWords;
        public List<Word> ReversedWords
        {
            get
            {
                if (_ReversedWords == null)
                {
                    _ReversedWords = this.Aggregate(new List<Word>(), (a, w) =>
                    {
                        if (w.Any(r => r.Type == CharacterType.Rune))
                        {
                            a.Add(w.Reverced);
                        }
                        return a;
                    });
                }
                return _ReversedWords;
            }
        }

        public List<Word> _words;
        public List<Word> Words
        {
            get
            {
                if (_words == null)
                {
                    _words = this.Where(w => w.Any(c => c.Type == CharacterType.Rune)).ToList();
                }
                return _words;
            }
        }

        public static Sentence operator -(Sentence o1, int[] o2)
        {
            Sentence result = new Sentence();
            result.AddRange(o1.Words);
            int index = 0;
            foreach (Word w in result)
            {
                if (index >= o2.Length)
                {
                    break;
                }
                foreach (Character c in w)
                {
                    if (index >= o2.Length)
                    {
                        break;
                    }
                    var newResult = c.GematriaIndex - o2[index];
                    c.Rune = Alphabets.INDEXED_RUNES[newResult < 0 ? 29 + newResult : newResult];
                    index++;
                }
            }
            return result;
        }

        public static Sentence operator +(Sentence o1, int[] o2)
        {
            Sentence result = new Sentence();
            result.AddRange(o1.Words);
            int index = 0;
            foreach (Word w in result)
            {
                if (index >= o2.Length)
                {
                    break;
                }
                foreach (Character c in w)
                {
                    if (index >= o2.Length)
                    {
                        break;
                    }
                    var newResult = c.GematriaIndex + o2[index] % 29;
                    c.Rune = Alphabets.INDEXED_RUNES[newResult];
                    index++;
                }
            }
            return result;
        }

        private List<Character> _characters;
        public List<Character> Characters
        {
            get
            {
                if (_characters == null)
                {
                    _characters = this.Aggregate(new List<Character>(), (a, w) =>
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
            return String.Join(" ", this.Where(w => w.Any(c => c.Type == CharacterType.Rune)).Select(w => w.PrimeSum)) + " = " + PrimeSum + ".";
        }

        public string ToPrimesString()
        {
            return String.Join(" ", this.Where(w => w.Any(c => c.Type == CharacterType.Rune)).Select(c => c.ToPrimesString()));
        }

        public override int GetHashCode()
        {
            return Runes.GetHashCode();
        }
    }

}
