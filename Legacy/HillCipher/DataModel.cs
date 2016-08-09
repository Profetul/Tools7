using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Solver
{
    public class DataModel
    {
        private bool _isInitialized = false;
        public List<Page> Pages { get; set; }

        public List<Section> Sections { get; set; }

        public DataModel()
        {
            Pages = new List<Page>();
            Sections = new List<Section>();
        }

        private string _inRunes;
        public string Runes
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_inRunes))
                {
                    _inRunes = String.Join("\r\n\r\n", this.Sections.Select(c => c.Runes));
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
                    _inLatin = String.Join("\r\n\r\n", this.Sections.Select(c => c.Latin));
                }
                return _inLatin;
            }
        }

        public long RuneSum
        {
            get
            {
                return this.Sections.Select(c => c.RuneSum).Sum();
            }
        }

        private List<Word> _words;
        public List<Word> Words
        {
            get
            {
                if (_words == null)
                {
                    _words = this.Sections.Aggregate(new List<Word>(), (a, s) =>
                    {
                        a.AddRange(s.Words);
                        return a;
                    });
                }
                return _words;
            }
        }


        private List<Paragraph> _paragraphs;
        public List<Paragraph> Paragraphs
        {
            get
            {
                if (_paragraphs == null)
                {
                    _paragraphs = Sections.Aggregate(new List<Paragraph>(), (a, p) =>
                    {
                        a.AddRange(p);
                        return a;
                    });
                }
                return _paragraphs;
            }
        }

        public void Initialize(string filePath)
        {
            if (_isInitialized)
            {
                return;
            }
            Page currentPage = new Page();
            Line currentLine = new Line();
            Section currentSection = new Section();
            Paragraph currentParagraph = new Paragraph();
            Sentence currentSentance = new Sentence();
            Word currentWord = new Word();
            Character character;

            using (StreamReader reader = File.OpenText(filePath))
            {
                string source = reader.ReadToEnd();
                reader.Close();
                foreach (char c in source)
                {
                    switch (c)
                    {

                        case '-':
                            if (currentWord.Count > 0)
                                currentSentance.Add(currentWord);
                            currentWord = new Word();
                            character = new Character { Type = CharacterType.Space, Rune = '·' };
                            currentWord.Add(character);
                            currentLine.Add(character);
                            currentSentance.Add(currentWord);
                            currentWord = new Word();
                            break;

                        case '#':
                            if (currentWord.Count > 0)
                                currentSentance.Add(currentWord);
                            currentWord = new Word();
                            character = new Character { Type = CharacterType.Delimiter, Rune = '#' };
                            currentWord.Add(character);
                            currentLine.Add(character);
                            currentSentance.Add(currentWord);
                            currentParagraph.Add(currentSentance);
                            currentWord = new Word();
                            currentSentance = new Sentence();
                            break;

                        case '.':
                            if (currentWord.Count > 0)
                                currentSentance.Add(currentWord);
                            currentWord = new Word();
                            character = new Character { Type = CharacterType.Dot, Rune = '¤' };
                            currentWord.Add(character);
                            currentLine.Add(character);
                            currentSentance.Add(currentWord);
                            currentParagraph.Add(currentSentance);
                            currentWord = new Word();
                            currentSentance = new Sentence();
                            break;

                        case ':':
                            if (currentWord.Count > 0)
                                currentSentance.Add(currentWord);

                            currentWord = new Word();
                            character = new Character { Type = CharacterType.Colon, Rune = ':' };
                            currentWord.Add(character);
                            currentLine.Add(character);
                            currentSentance.Add(currentWord);
                            currentWord = new Word();
                            break;

                        case '\'':

                            /*character = new Character { Type = CharacterType.Rune, Rune = c };
                            currentLine.Add(character);
                            currentWord.Add(character);*/
                            /*
                            if (currentWord.Count > 0)
                                currentSentance.Add(currentWord);

                            currentWord = new Word();
                            character = new Character { Type = CharacterType.DoubleQuote, Rune = '"' };
                            currentWord.Add(character);
                            currentLine.Add(character);
                            currentSentance.Add(currentWord);
                            currentWord = new Word();*/
                            break;

                        case '"':

                            if (currentWord.Count > 0)
                                currentSentance.Add(currentWord);

                            currentWord = new Word();
                            character = new Character { Type = CharacterType.DoubleQuote, Rune = '"' };
                            currentWord.Add(character);
                            currentLine.Add(character);
                            currentSentance.Add(currentWord);
                            currentWord = new Word();
                            break;

                        case '/':
                            currentPage.Add(currentLine);
                            currentLine = new Line();
                            break;

                        case '\r':
                        case '\n':
                            break;

                        case '%':
                            this.Pages.Add(currentPage);
                            currentPage = new Page();
                            break;

                        case '&':
                            currentSection.Add(currentParagraph);
                            currentParagraph = new Paragraph();
                            break;

                        case '$':
                            this.Sections.Add(currentSection);
                            currentSection = new Section();
                            break;

                        case '§':
                            currentSection.Add(currentParagraph);
                            this.Sections.Add(currentSection);
                            currentSentance = null;
                            currentWord = null;
                            currentParagraph = null;
                            currentSection = null;
                            currentPage = null;
                            currentLine = null;
                            break;

                        case '1':
                        case '2':
                        case '3':
                        case '4':
                        case '5':
                            currentSection.Add(currentParagraph);
                            currentParagraph = new Paragraph();
                            currentWord = new Word();
                            character = new Character { Type = CharacterType.Number, Rune = c };
                            currentWord.Add(character);
                            currentLine.Add(character);
                            currentSentance.Add(currentWord);
                            currentWord = new Word();
                            break;

                        case '7':
                            if (currentWord.Count > 0)
                                currentSentance.Add(currentWord);
                            currentWord = new Word();
                            character = new Character { Type = CharacterType.Number, Rune = c };
                            currentWord.Add(character);
                            currentLine.Add(character);
                            currentSentance.Add(currentWord);
                            currentWord = new Word();
                            break;

                        default:
                            character = new Character { Type = CharacterType.Rune, Rune = c };
                            currentLine.Add(character);
                            currentWord.Add(character);
                            break;
                    }

                }
            }
            _isInitialized = true;
        }

        public override string ToString()
        {
            return Latin;
        }
    }

    public class Page : List<Line>
    {
        private string _inRunes;
        public string Runes
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_inRunes))
                {
                    _inRunes = String.Join("\r\n", this.Select(c => c.Runes));
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
                    _inLatin = String.Join("\r\n", this.Select(c => c.Latin));
                }
                return _inLatin;
            }
        }

        public long RuneSum
        {
            get
            {
                return this.Select(c => c.RuneSum).Sum();
            }
        }

        public override string ToString()
        {
            return Latin;
        }
    }

    public class Line : List<Character>
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
                    _inLatin = String.Join("ˌ", this.Select(c => c.Type == CharacterType.Rune ? Constants.GEMATRIA[c.Rune] :
                    c.Type != CharacterType.Space ? c.Rune.ToString() + " " : c.Rune.ToString()));
                }
                return _inLatin;
            }
        }

        public long RuneSum
        {
            get
            {
                return this.Select(c => c.PrimeValue).Sum();
            }
        }
        public override string ToString()
        {
            return Latin;
        }
    }


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

        public long RuneSum
        {
            get
            {
                return this.Select(c => c.RuneSum).Sum();
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

    }


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

        public long RuneSum
        {
            get
            {
                return this.Select(c => c.RuneSum).Sum();
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

    }

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

        public long RuneSum
        {
            get
            {
                return this.Select(c => c.RunSum).Sum();
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

        public override int GetHashCode()
        {
            return String.Join(" ", Words.ToString()).GetHashCode();
        }
    }

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
                    _inLatin = String.Join(".", this.Select(c => c.Latin));
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
                    result = result * BigInteger.Pow(Constants.PRIMES[i], (this[i].GematriaIndex + 1));
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
            if(o1 == null)
            {
                return null;
            }
            int[] result = new int[o1.Count];
            for(int i = 0; i< o1.Count; i++)
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

    public class Character
    {
        public CharacterType Type { get; set; }

        public char Rune { get; set; }

        public string Latin
        {
            get
            {
                return Type == CharacterType.Rune ? Constants.GEMATRIA[Rune] : Rune.ToString();
            }
        }

        public int GematriaIndex
        {
            get
            {
                return Type == CharacterType.Rune ? Constants.RUNE_INDEX[Rune] : (int)Type;
            }
        }

        public int PrimeValue
        {
            get
            {
                return Type == CharacterType.Rune ? Constants.RUNE_PRIME[Rune] : 0;
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
            return new Character { Type = CharacterType.Rune, Rune = Constants.GEMATRIA.Keys.ToArray()[newIndex] };
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
