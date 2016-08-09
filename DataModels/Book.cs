using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class Book
    {
        private bool _isInitialized = false;
        public List<Page> Pages { get; set; }

        public List<Section> Sections { get; set; }

        public Book()
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

        public void LoadFromFile(string filePath)
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
}
