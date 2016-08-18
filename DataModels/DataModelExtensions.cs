using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public static class DataModelExtensions
    {

        public static List<Sentence> SentencesFromFile(this string path)
        {
            return File.ReadAllLines(path).Select(line =>
            {
                Sentence sentence = new Sentence();
                string[] words = line.ToUpper().Replace("'", "").Split(new char[] { ' ', '.', ',', ';', '-', '"', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    if (word.Length == 1 && !(word == "A" || word == "I"))
                    {
                        continue;
                    }
                    sentence.Add(word.ToWord());
                }
                return sentence;
            }).ToList();
        }

        public static List<Word> WordsFromFile(this string path)
        {
            List<Word> output = new List<Word>();
            File.ReadAllLines(path).Select(line =>
            {
                string[] words = line.ToUpper().Replace("'", "").Split(new char[] { ' ', '.', ',', ';', '-', '"', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    if (word.Length == 1 && !(word == "A" || word == "I"))
                    {
                        continue;
                    }
                    output.Add(word.ToWord());
                }
                return 0;
            }).ToList();
            return output;
        }

        public static List<Character> CharactersFromFile(this string path)
        {
            List<Character> output = new List<Character>();
            File.ReadAllLines(path).Select(line =>
            {
                string[] words = line.ToUpper().Replace("'", "").Split(new char[] { ' ', '.', ',', ';', '-', '"', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    if (word.Length == 1 && !(word == "A" || word == "I"))
                    {
                        continue;
                    }
                    output.AddRange(word.ToWord());
                }
                return 0;
            }).ToList();
            return output;
        }


        public static Word ToWord(this string word)
        {
            Word result = new Word();
            string tempWord = word.ToUpper();
            for (int i = 0; i < tempWord.Length; i++)
            {
                var letter = tempWord[i];

                if (letter == 'K')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᚳ' });
                    continue;
                }

                if (letter == 'V')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᚢ' });
                    continue;
                }

                if (letter == 'Z')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᛋ' });
                    continue;
                }

                if (letter == 'Q')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᚳ' });
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᚹ' });
                    continue;
                }

                if (i + 1 < tempWord.Length && letter == 'T' && tempWord[i + 1] == 'H')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᚦ' });
                    i = i + 1;
                    continue;
                }

                if (i + 1 < tempWord.Length && letter == 'E' && tempWord[i + 1] == 'O')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᛇ' });
                    i = i + 1;
                    continue;
                }


                if (i + 2 < tempWord.Length && letter == 'I' && tempWord[i + 1] == 'N' && tempWord[i + 2] == 'G')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᛝ' });
                    i = i + 2;
                    continue;
                }

                if (i + 1 < tempWord.Length && letter == 'N' && tempWord[i + 1] == 'G')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᛝ' });
                    i = i + 1;
                    continue;
                }

                if (i + 1 < tempWord.Length && letter == 'O' && tempWord[i + 1] == 'E')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᛟ' });
                    i = i + 1;
                    continue;
                }

                if (i + 1 < tempWord.Length && letter == 'A' && tempWord[i + 1] == 'E')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᚫ' });
                    i = i + 1;
                    continue;
                }

                if (i + 1 < tempWord.Length && letter == 'I' && (tempWord[i + 1] == 'A' || tempWord[i + 1] == 'O'))
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᛡ' });
                    i = i + 1;
                    continue;
                }


                if (i + 1 < tempWord.Length && letter == 'E' && tempWord[i + 1] == 'A')
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = 'ᛠ' });
                    i = i + 1;
                    continue;
                }

                if (Alphabets.GEMATRIA.Any(g => g.Value == letter.ToString()))
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = Alphabets.GEMATRIA.FirstOrDefault(g => g.Value == letter.ToString()).Key });
                }

            }
            return result;
        }

        public static Word AsWord(this IEnumerable<Character> c)
        {
            Word result = new Word();
            result.AddRange(c);
            return result;
        }

        public static Word AsWord(this int[] key)
        {
            Word result = new Word();
            for (int i = 0; i < key.Length; i++)
            {
                result.Add(new Character { Type = CharacterType.Rune, Rune = Alphabets.INDEXED_RUNES[(29 + key[i]) % 29] });
            }
            return result;
        }

        public static bool IsOverlap(this Word word1, Word word2)
        {
            for (int i = 0; i < word1.Count; i++)
            {
                if (i >= word2.Count)
                {
                    break;
                }

                if (word1[i] != word2[i])
                {
                    return false;
                }
            }
            return true;
        }
    }
}
