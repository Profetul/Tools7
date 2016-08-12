using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class Paragraph : List<Sentence>
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
                    _inLatin = String.Join(" * ", this.Select(c => c.Latin));
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

        private List<Word> _words;
        public List<Word> Words
        {
            get
            {
                if (_words == null)
                {
                    _words = this.Aggregate(new List<Word>(), (a, s) =>
                    {
                        a.AddRange(s.Where(w => w.Any(c => c.Type == CharacterType.Rune)));
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
                    _reversedWords = this.Aggregate(new List<Word>(), (a, s) =>
                    {
                        a.AddRange(s.ReversedWords.Where(w => w.Any(c => c.Type == CharacterType.Rune)));
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
            return String.Join("\r\n", this.Select(s => s.ToPrimeSumString())) + "\r\nParagraph Total: " + PrimeSum + "\r\n";
        }

    }
}
