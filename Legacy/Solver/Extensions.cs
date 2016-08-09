using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public static class Extensions
    {
        public static IEnumerable<int> PatternAt(this byte[] source, byte[] pattern)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    yield return i;
                }
            }
        }

        public static bool HasPattern(this byte[] source, byte[] pattern)
        {
            return BitConverter.ToString(source).Contains(BitConverter.ToString(pattern));
        }

        public static bool HasPattern(this byte[] source, byte[] x1, byte[] x2, byte[] x3, byte[] x4)
        {
            var test = BitConverter.ToString(source);
            return test.Contains(BitConverter.ToString(x1)) || test.Contains(BitConverter.ToString(x2))
                || test.Contains(BitConverter.ToString(x3)) || test.Contains(BitConverter.ToString(x4));
        }

        public static byte[] HexToByteArray(this string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
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

                if (Constants.GEMATRIA.Any(g => g.Value == letter.ToString()))
                {
                    result.Add(new Character { Type = CharacterType.Rune, Rune = Constants.GEMATRIA.FirstOrDefault(g => g.Value == letter.ToString()).Key });
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
                result.Add(new Character { Type = CharacterType.Rune, Rune = Constants.INDEXED_RUNES[(29 + key[i]) % 29] });
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

        public static double[] ToIoC(this List<Paragraph> section, int interval = 1)
        {
            if (interval < 1)
            {
                return new double[0];
            }

            List<Word> paragraphs = section.Aggregate(new List<Word>(), (a, p) =>
            {
                a.AddRange(p.Words);
                return a;
            });
            return paragraphs.ToIoC(interval);
        }

        public static Dictionary<Character, int>[] ToFrequency(this List<Paragraph> section, int interval = 1)
        {
            if (interval < 1)
            {
                return new Dictionary<Character, int>[0];
            }

            List<Word> paragraphs = section.Aggregate(new List<Word>(), (a, p) =>
            {
                a.AddRange(p.Words);
                return a;
            });
            return paragraphs.ToFrequency(interval);
        }


        public static double[] ToIoC(this List<Sentence> paragraph, int interval = 1)
        {

            if (interval < 1)
            {
                return new double[0];
            }

            List<Word> words = paragraph.Aggregate(new List<Word>(), (a, w) =>
            {
                a.AddRange(w);
                return a;
            });
            return words.ToIoC(interval);
        }

        public static Dictionary<Character, int>[] ToFrequency(this List<Sentence> paragraph, int interval = 1)
        {

            if (interval < 1)
            {
                return new Dictionary<Character, int>[0];
            }

            List<Word> words = paragraph.Aggregate(new List<Word>(), (a, w) =>
            {
                a.AddRange(w);
                return a;
            });
            return words.ToFrequency(interval);
        }



        public static double[] ToIoC(this List<Word> sentence, int interval = 1)
        {
            if (interval < 1)
            {
                return new double[0];
            }

            List<Character> characters = sentence.Aggregate(new List<Character>(), (a, w) =>
            {
                a.AddRange(w);
                return a;
            });
            return characters.ToIoC(interval);
        }

        public static Dictionary<Character, int>[] ToFrequency(this List<Word> sentence, int interval = 1)
        {
            if (interval < 1)
            {
                return new Dictionary<Character, int>[0];
            }

            List<Character> characters = sentence.Aggregate(new List<Character>(), (a, w) =>
            {
                a.AddRange(w);
                return a;
            });
            return characters.ToFrequency(interval);
        }

        public static double[] ToIoC(this List<Character> characters, int interval = 1)
        {
            if (interval < 1)
            {
                return new double[0];
            }



            return characters.ToFrequency(interval).ToIoC();
        }

        public static double[] ToIoC(this Dictionary<Character, int>[] characterFrequencies)
        {

            double[] results = new double[characterFrequencies.Length];
            for (int i = 0; i < results.Length; i++)
            {
                double n = (double)characterFrequencies[i].Values.Sum();
                double phiN = (1.0 / 29.0) * n * (n - 1);
                double phiR = characterFrequencies[i].Values.Select(v => (double)(v * (v - 1))).Sum();
                results[i] = phiR / phiN;
            }
            return results;
        }


        public static Dictionary<Character, int>[] ToFrequency(this List<Character> characters, int interval = 1)
        {

            if (interval < 1)
            {
                return new Dictionary<Character, int>[0];
            }

            Dictionary<Character, int>[] characterFrequency = new Dictionary<Character, int>[interval];
            for (int i = 0; i < interval; i++)
            {
                characterFrequency[i] = new Dictionary<Character, int>();
            }

            for (int i = 0; i < characters.Count; i++)
            {
                var intervalIndex = (i + 1) % interval;
                var character = characters[i];
                if (character.Type != CharacterType.Rune)
                {
                    continue;
                }

                if (!characterFrequency[intervalIndex].ContainsKey(character))
                {
                    characterFrequency[intervalIndex].Add(character, 0);
                }
                characterFrequency[intervalIndex][character] = characterFrequency[intervalIndex][character] + 1;
            }

            for (int i = 0; i < interval; i++)
            {
                characterFrequency[i] = characterFrequency[i].OrderByDescending(c => c.Value).ToDictionary(c => c.Key, v => v.Value);
            }

            return characterFrequency;
        }

        public static Dictionary<Word, List<int>> NGramOffsets(this List<Character> characters, int nGrams = 2)
        {
            if (nGrams < 2)
            {
                return new Dictionary<Word, List<int>>();
            }

            Dictionary<Word, List<int>> counter = new Dictionary<Word, List<int>>();
            for (int i = 0; i < characters.Count; i++)
            {
                if (i + nGrams > characters.Count)
                {
                    break;
                }
                Word key = characters.Skip(i).Take(nGrams).AsWord();
                if (!counter.ContainsKey(key))
                {
                    counter.Add(key, new List<int>() { i });
                }
                else
                {
                    counter[key].Add(i);
                }

            }

            return counter.Where(n => n.Value.Count > 1).ToDictionary(k => k.Key, v => v.Value);
        }

        public static Dictionary<Word, List<int>> RevNGramOffsets(this List<Character> characters, int nGrams = 2)
        {
            if (nGrams < 2)
            {
                return new Dictionary<Word, List<int>>();
            }

            Dictionary<Word, List<int>> counter = new Dictionary<Word, List<int>>();
            for (int i = 0; i < characters.Count; i++)
            {
                if (i + nGrams > characters.Count)
                {
                    break;
                }

                Word key = characters.Skip(i).Take(nGrams).AsWord();
                Word revKey = key.Reverced;

                if (!counter.ContainsKey(key))
                {
                    counter.Add(key, new List<int>() { i });
                }

                if (counter.ContainsKey(revKey) && !key.Equals(revKey))
                {
                    counter[revKey].Add(i);
                }
            }

            return counter.Where(n => n.Value.Count > 1).ToDictionary(k => k.Key, v => v.Value);
        }



    }
}
