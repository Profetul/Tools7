using DataModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptanalysis
{
    public static class Statistics
    {
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
            return counter.Where(n => n.Value.Count > 1).OrderByDescending(n => n.Value.Count).ToDictionary(k => k.Key, v => v.Value);
        }
        public static Dictionary<Word, List<int>> NGrams(this List<Character> characters, int nGrams = 2)
        {
            if (nGrams < 2)
            {
                return new Dictionary<Word, List<int>>();
            }

            Dictionary<Word, List<int>> counter = new Dictionary<Word, List<int>>();
            for (int i = 0; i < characters.Count; i = i + nGrams)
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

            return counter.OrderByDescending(n => n.Value.Count).ToDictionary(k => k.Key, v => v.Value);
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

            return counter.OrderByDescending(n => n.Value.Count).ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
