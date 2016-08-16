using Newtonsoft.Json;
using OEIS;
using DataModels;
using Cryptanalysis;
using SQLite.Net;
using SQLite.Net.Platform.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Analyzer
{
    class Program
    {
        static List<NGramStat> stats = new List<NGramStat>();
        private static WordDictionary dictionary = new WordDictionary();
        private static double nGramsTotal = 0.0;
        private static double nGramsFloor = 0.0;
        static void Main(string[] args)
        {
            HashSet<Sentence> sentences = new HashSet<Sentence>();
            File.ReadAllLines(@"..\Results\18.txt").Select(line =>
            {
                string[] words = line.Replace("'", "").Split(new char[] { ' ', '.', ',', ';', '"', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                if(words.Length > 1 && words[0].Length == 1)
                {
                    Sentence s = new Sentence
                    {
                        words[0].ToWord(),
                        words[1].ToWord()
                    };
                    if (!sentences.Any(sa => sa[1].Equals(s[1])))
                    {
                        sentences.Add(s);
                    }
                }
                return 0;
            }).ToList();

            File.AppendAllText(@"..\Results\1_8.txt", String.Join("\r\n", sentences.Select(s => s[0].ToString() + " " + s[1].ToString())));

            List<Word> nGrams = new List<Word>();
            File.ReadAllLines(@"..\DataSources\Koans.txt").Select(line =>
            {
                string[] words = line.Replace("'", "").Split(new char[] { ' ', '.', ',', ';', '-', '"', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    nGrams.Add(word.ToWord());
                }
                return 0;
            }).ToList();

            BlockingCollection<Sentence> results = new BlockingCollection<Sentence>();
            Parallel.For(0, nGrams.Count - 1, index =>
            {
                if (nGrams[index].Count == 1)
                {
                    results.Add(new Sentence
                    {
                        nGrams[index],
                        nGrams[index + 1]
                    });
                }
            });

            HashSet<Sentence> oneToMany = results.Where(s => s[1].Count == 8).Aggregate(new HashSet<Sentence>(), (a, n) =>
            {
                if (!a.Contains(n))
                {
                    a.Add(n);
                }
                return a;
            });

            File.AppendAllText(@"..\Results\18.txt", String.Join("\r\n", oneToMany.Select(s => s[0].ToString() + " " + s[1].ToString())));
        }

        private static void AutokeyBreaker()
        {
            var section = "A-P-IA-M-T-E-A-H-P-G-OE-AE-P-U-U-B-E-Y-E-S-M-Y-EO-W-H-H-S-EA-M-ING-W-I-J-R-C-B-Y-M-T-ING-J-T-G-TH-L-P-D-H-L-ING-G-R-ING-EO-E-I-ING-U-S-A-TH-O-J-U-D-X-IA-R-O-OE-G-Y-R-S-E-EA-G-AE-D-B-L-C-S-M-D-W-H-E-J-U-U-J-IA-OE-OE-Y-EA-H-A-IA-EO-E-W-A-O-M-TH-EA-EO-EO-EO-C-IA-A-I-O-EA-X-O-X-L-G-T-J-G-EO-EO-OE-P-X-O-C-T-I-AE-F-M-E-TH-X-TH-M-C-T-E-D-J-A-P-S-ING-N-AE-N-N-EO-R-W-EO-I-TH-W-F-X-S-EA-TH-D-X-T-AE-W-C-W-IA-C-EO-M-I-L-W-P-G-Y-D-C-OE-M-X-S-IA-I-D-E-E-J-I-A-P-O-N-H-EA-W-I-P-M-U-X-T-O-M-Y-G-H-X-E-B-A-M-IA-I-S-U-M-ING-AE-F-EA-B-I-IA-TH-J-ING-X-S-EA-E-C-X-F-O-J-EA-X-B-U-P-G-TH-B-IA-ING-G-Y-C-ING-L-M-R-OE-Y-B-D-Y-L-OE-D-D-OE-M-R-M-C-C-F-X-OE-E-D-TH-O-U-EA-F-G-O-Y-D-A-H-C-B-M-TH-ING-R-B-E-G-S-U-P-B-G-C-I".ToWord();

            Dictionary<Word, int> nGrams = new Dictionary<Word, int>();
            File.ReadAllLines(@"..\DataSources\Quads.txt").Select(l =>
            {
                string[] split = l.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                Word word = split[0].ToWord();
                if (!nGrams.ContainsKey(word))
                {
                    nGrams.Add(word, 0);
                }
                nGrams[word] = nGrams[word] + Convert.ToInt32(split[1]);
                return 0;
            }).ToList();

            stats = nGrams.Select(k => new NGramStat
            {
                Word = k.Key,
                Counts = k.Value
            }).ToList();

            //nGramExtractor(nGrams);

            nGramsTotal = (double)stats.Select(s => s.Counts).Sum();
            nGramsFloor = Math.Log10((double)0.0001 / nGramsTotal);
            Parallel.ForEach(stats, s => s.Log10 = Math.Log10((double)s.Counts / nGramsTotal));

            //File.WriteAllText(@"c:\temp\quards2.txt", String.Join("\r\n", stats.Select(k => k.Word.ToString() + "," + k.Counts)));
            double[] results = new double[29];
            for (int keySize = 2; keySize < 30; keySize++)
            {
                int[] keyShift = new int[keySize];
                double prevMax = double.MinValue;
                while (true)
                {
                    double maxValue = double.MinValue;
                    for (int inKey = 0; inKey < keySize; inKey++)
                    {
                        Parallel.For(0, 29, (index) =>
                        {
                            keyShift[inKey] = index;
                            List<int> key = keyShift.ToList();
                            key.Insert(0, 0);
                            var decoded = Ciphers.DecodeAutokey(key.ToArray().AsWord(), section);
                            results[index] = decoded.NGramCount(4).Select(k =>
                            {
                                var stat = stats.FirstOrDefault(n => n.Word.Equals(k.Key));
                                if (stat == null)
                                {
                                    return nGramsFloor * k.Value;
                                }
                                return stat.Log10 * k.Value;
                            }).Sum();
                        }
                        );

                        maxValue = results.Max();
                        var newIndex = results.ToList().IndexOf(maxValue);
                        keyShift[inKey] = newIndex;
                    }

                    List<int> testKey = keyShift.ToList();
                    testKey.Insert(0, 0);
                    var rez = Ciphers.DecodeAutokey(testKey.ToArray().AsWord(), section).Take(30).AsWord().ToString();
                    Console.WriteLine("{0} => {1}", String.Join(",", testKey), rez);
                    File.AppendAllText(@"..\Results\AutoKey.txt", String.Format("\r\n{0} => {1}", String.Join(",", testKey), rez));
                    if (prevMax >= maxValue)
                    {
                        break;
                    }
                    prevMax = maxValue;
                }
            }
        }

        private static double Fitness(List<Character> text)
        {
            return text.NGramCount(4).Select(k =>
            {
                var stat = stats.FirstOrDefault(n => n.Word.Equals(k.Key));
                if (stat == null)
                {
                    return nGramsFloor * k.Value;
                }
                return stat.Log10 * k.Value;
            }).Sum();
        }

        private static void nGramExtractor(Dictionary<Word, int> nGrams)
        {
            List<Character> characters = new List<Character>();
            File.ReadAllLines(@"..\DataSources\SongsOfCicada.txt").Select(line =>
            {
                string[] words = line.Replace("'", "").Split(new char[] { ' ', '.', ',', ';', '-', '"', '(', ')', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    var runeWord = word.ToWord();
                    characters.AddRange(runeWord);
                }
                return 0;
            }).ToList();

            characters.NGramOffsets(4).Select(k =>
            {
                if (!nGrams.ContainsKey(k.Key))
                {
                    nGrams.Add(k.Key, 0);
                }
                nGrams[k.Key] = nGrams[k.Key] + k.Value.Count;
                return 0;
            }).ToArray();

            File.WriteAllText(@"..\DataSources\Quads.txt", String.Join("\r\n", nGrams.Select(k => k.Key + " " + k.Value)));
        }
    }
}
