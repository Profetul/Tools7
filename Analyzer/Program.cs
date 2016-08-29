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
using SQLite.Net.Attributes;

namespace Analyzer
{
    class Program
    {
        static List<NGramStat> stats = new List<NGramStat>();
        private static WordDictionary dictionary = new WordDictionary();
        private static double nGramsTotal = 0.0;
        private static double nGramsFloor = 0.0;

        public class StartNGram : NGram { }
        public class EndNGram : NGram { }
        public class NGram
        {
            [PrimaryKey]
            public string Sentence { get; set; }

            [Indexed]
            public string Pattern { get; set; }

            public int Total { get; set; }
        }


        public class NGramView
        {
            private string _sentence;
            public string Sentence
            {
                get
                {
                    return _sentence;
                }
                set
                {
                    _sentence = value;
                }
            }

            private Sentence _runeSentence;
            public Sentence RuneSentence
            {
                get
                {
                    if (_runeSentence == null)
                    {
                        _runeSentence = Sentence.ToSentence();
                    }
                    return _runeSentence;
                }
            }

            public string Pattern { get; set; }

            public int Total { get; set; }

            public int? N1 { get; set; }

            public int? N2 { get; set; }

            public int? N3 { get; set; }

            public int? N4 { get; set; }

            public int? N5 { get; set; }

            public override string ToString()
            {
                return Sentence;
            }
        }


        static void Main(string[] args)
        {
            SQLiteConnectionWithLock db = new SQLiteConnectionWithLock(
                new SQLitePlatformWin32(),
                new SQLiteConnectionString(@"c:\temp\ngrams.db", true));
            db.CreateTable<StartNGram>();
            db.CreateTable<EndNGram>();
            //var part1 = db.Table<NGramView>().Where(v => v.N1 == 3 && v.N2 == 3 && v.N3 == 11 && v.N4 == 5).ToList();
            //var res = part1.Select(p1 => p1.RuneSentence[2].Latin).Distinct().OrderBy(r => r).ToList();
            for (int i = 1; i < 56; i++)
            {
                ParseNGrams(db, @"c:\temp\" + i);
            }

        }

        private static void ParseNGrams(SQLiteConnectionWithLock db, string filePath)
        {
            try
            {
                ConcurrentDictionary<Sentence, int> results = new ConcurrentDictionary<Sentence, int>(1024, 10000000);
                ConcurrentDictionary<Sentence, int> start = new ConcurrentDictionary<Sentence, int>(1024, 1000000);
                ConcurrentDictionary<Sentence, int> end = new ConcurrentDictionary<Sentence, int>(1024, 1000000);
                Parallel.ForEach(File.ReadLines(filePath), line =>
                {
                    var comp = line
                    .Replace("_ADJ", "")
                    .Replace("_ADP", "")
                    .Replace("ADJ_ADP", "")
                    .Replace("_ADV", "")
                    .Replace("_CONJ", "")
                    .Replace("_DET", "")
                    .Replace("_NOUN", "")
                    .Replace("_NUM", "")
                    .Replace("_PRON", "")
                    .Replace("_PRT", "")
                    .Replace("_VERB", "")
                    .Replace("_END", "")
                    .Replace("START_", "")
                    .Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    Sentence result = new Sentence();
                    var words = comp[0].Replace(".", "")
                    .Replace("'", "")
                    .Replace("\"", "")
                    .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(w => w.ToWord()).Where(w => w.Count > 0);
                    result.AddRange(words);
                    if (result.Count > 1)
                    {
                        results.AddOrUpdate(result, int.Parse(comp[2]), (s, t) => t + int.Parse(comp[2]));
                        if (line.Contains("_START_"))
                        {
                            start.AddOrUpdate(result, int.Parse(comp[2]), (s, t) => t + int.Parse(comp[2]));
                        }
                        if (line.Contains("_END"))
                        {
                            end.AddOrUpdate(result, int.Parse(comp[2]), (s, t) => t + int.Parse(comp[2]));
                        }
                    }
                });

                db.InsertOrIgnoreAll
                (
                    results.Select(r => new NGram
                    {
                        Sentence = r.Key.ToString(),
                        Pattern = String.Join("_", r.Key.Select(v => v.Count)),
                        Total = r.Value
                    })
                );

                db.InsertOrIgnoreAll
                (
                    start.Select(r => new StartNGram
                    {
                        Sentence = r.Key.ToString(),
                        Pattern = String.Join("_", r.Key.Select(v => v.Count)),
                        Total = r.Value
                    })
                );
                Console.WriteLine("Done with: {0}", filePath);
                File.Delete(filePath);
            }
            catch
            { }

            GC.Collect();
        }

        private static void GapExtractor()
        {
            Book book = new Book();
            book.LoadFromFile(@"..\DataSources\liber-work");
            var words = book.Sections[0].Words.Where(w => w.Count == 1).ToList();
            var word = words[1];
            var prevWord = book.Sections[0].Words[book.Sections[0].Words.IndexOf(word) - 1];
            var nextWord = book.Sections[0].Words[book.Sections[0].Words.IndexOf(word) + 1];
            //int nStart = 28;
            //for (; nStart > -1; nStart--)
            //{
            //    int nGap = 1;
            //    var rez = Ciphers.DecodeVigenere((new int[] {0, nStart, nStart - nGap, nStart - 2 * nGap, nStart - 3 * nGap, nStart - 4 * nGap}).AsWord(), nextWord);
            //}
            Sentence testS = new Sentence();
            testS.Add(prevWord);
            testS.Add(word);
            List<Sentence> sentences = @"..\Results\a7_1.txt".SentencesFromFile();
            ConcurrentDictionary<Sentence, string> nSentence = new ConcurrentDictionary<Sentence, string>();
            Parallel.ForEach(sentences, sentence =>
            {
                byte[] shifts = (testS.Characters.AsWord() - sentence.Characters.AsWord()).Select(i => i < 0 ? (byte)((29 + i % 29) % 29) : (byte)(i % 29)).ToArray();
                List<int> gaps = new List<int>();
                for (int i = 0; i < shifts.Length - 1; i++)
                {
                    gaps.Add(shifts[i + 1] - shifts[i]);
                }
                nSentence.AddOrUpdate(sentence, String.Join(",", gaps) + "|" + String.Join(",", shifts), (a, n) => n);
            });

            File.WriteAllText(@"..\Results\rez7_1.txt", String.Join("\r\n", nSentence.Select(l => l.Key.ToString() + "|" + l.Value).ToList()));
        }

        private static void NGramExtractor()
        {
            List<Sentence> sentences = @"..\DataSources\atlas.txt".SentencesFromFile();

            ConcurrentDictionary<Sentence, int> nSentence = new ConcurrentDictionary<Sentence, int>();
            Parallel.ForEach(sentences, sentence =>
            {
                if (sentence.Count < 2)
                    return;
                for (int index = 0; index < sentence.Count - 2; index++)
                {
                    //var index = 0;
                    if (sentence[index].Count == 7
                        && sentence[index + 1].Count == 1
                        //&& sentence[index + 2].Count == 6
                        //&& sentence[index + 3].Count == 6
                        )
                    {
                        nSentence.AddOrUpdate(new Sentence
                        {
                            sentence[index],
                            sentence[index + 1],
                            //sentence[index + 2],
                            //sentence[index + 3]
                        }, 1, (k, v) => v + 1);
                    }
                }
            });

            sentences = @"..\DataSources\geb.txt".SentencesFromFile();

            Parallel.ForEach(sentences, sentence =>
            {
                if (sentence.Count < 2)
                    return;
                for (int index = 0; index < sentence.Count - 2; index++)
                {
                    //var index = 0;
                    if (sentence[index].Count == 7
                        && sentence[index + 1].Count == 1
                        //&& sentence[index + 2].Count == 6
                        //&& sentence[index + 3].Count == 6
                        )
                    {
                        nSentence.AddOrUpdate(new Sentence
                        {
                            sentence[index],
                            sentence[index + 1],
                            //sentence[index + 2],
                            //sentence[index + 3]
                        }, 1, (k, v) => v + 1);
                    }
                }
            });

            sentences = @"..\DataSources\bible.txt".SentencesFromFile();

            Parallel.ForEach(sentences, sentence =>
            {
                if (sentence.Count < 2)
                    return;
                for (int index = 0; index < sentence.Count - 2; index++)
                {
                    //var index = 0;
                    if (sentence[index].Count == 7
                        && sentence[index + 1].Count == 1
                        //&& sentence[index + 2].Count == 6
                        //&& sentence[index + 3].Count == 6
                        )
                    {
                        nSentence.AddOrUpdate(new Sentence
                        {
                            sentence[index],
                            sentence[index + 1],
                            //sentence[index + 2],
                            //sentence[index + 3]
                        }, 1, (k, v) => v + 1);
                    }
                }
            });


            sentences = @"..\DataSources\mastermind.txt".SentencesFromFile();

            Parallel.ForEach(sentences, sentence =>
            {
                if (sentence.Count < 2)
                    return;
                for (int index = 0; index < sentence.Count - 2; index++)
                {
                    //var index = 0;
                    if (sentence[index].Count == 7
                        && sentence[index + 1].Count == 1
                        //&& sentence[index + 2].Count == 6
                        //&& sentence[index + 3].Count == 6
                        )
                    {
                        nSentence.AddOrUpdate(new Sentence
                        {
                            sentence[index],
                            sentence[index + 1],
                            //sentence[index + 2],
                            //sentence[index + 3]
                        }, 1, (k, v) => v + 1);
                    }
                }
            });

            sentences = @"..\DataSources\LordOfRings.txt".SentencesFromFile();

            Parallel.ForEach(sentences, sentence =>
            {
                if (sentence.Count < 2)
                    return;
                for (int index = 0; index < sentence.Count - 2; index++)
                {
                    //var index = 0;
                    if (sentence[index].Count == 7
                        && sentence[index + 1].Count == 1
                        //&& sentence[index + 2].Count == 6
                        //&& sentence[index + 3].Count == 6
                        )
                    {
                        nSentence.AddOrUpdate(new Sentence
                        {
                            sentence[index],
                            sentence[index + 1],
                            //sentence[index + 2],
                            //sentence[index + 3]
                        }, 1, (k, v) => v + 1);
                    }
                }
            });

            sentences = @"..\DataSources\CicadaSentences.txt".SentencesFromFile();

            Parallel.ForEach(sentences, sentence =>
            {
                if (sentence.Count < 2)
                    return;
                for (int index = 0; index < sentence.Count - 2; index++)
                {
                    //var index = 0;
                    if (sentence[index].Count == 7
                        && sentence[index + 1].Count == 1
                        //&& sentence[index + 2].Count == 6
                        //&& sentence[index + 3].Count == 6
                        )
                    {
                        nSentence.AddOrUpdate(new Sentence
                        {
                            sentence[index],
                            sentence[index + 1],
                            //sentence[index + 2],
                            //sentence[index + 3]
                        }, 1, (k, v) => v + 1);
                    }
                }
            });

            sentences = @"..\DataSources\Koans.txt".SentencesFromFile();

            Parallel.ForEach(sentences, sentence =>
            {
                if (sentence.Count < 2)
                    return;
                for (int index = 0; index < sentence.Count - 2; index++)
                {
                    //var index = 0;
                    if (sentence[index].Count == 7
                        && sentence[index + 1].Count == 1
                        //&& sentence[index + 2].Count == 6
                        //&& sentence[index + 3].Count == 6
                        )
                    {
                        nSentence.AddOrUpdate(new Sentence
                        {
                            sentence[index],
                            sentence[index + 1],
                            //sentence[index + 2],
                            //sentence[index + 3]
                        }, 1, (k, v) => v + 1);
                    }
                }
            });

            sentences = @"..\DataSources\selfrel.txt".SentencesFromFile();

            Parallel.ForEach(sentences, sentence =>
            {
                if (sentence.Count < 2)
                    return;
                for (int index = 0; index < sentence.Count - 2; index++)
                {
                    //var index = 0;
                    if (sentence[index].Count == 7
                        && sentence[index + 1].Count == 1
                        //&& sentence[index + 2].Count == 6
                        //&& sentence[index + 3].Count == 6
                        )
                    {
                        nSentence.AddOrUpdate(new Sentence
                        {
                            sentence[index],
                            sentence[index + 1],
                            //sentence[index + 2],
                            //sentence[index + 3]
                        }, 1, (k, v) => v + 1);
                    }
                }
            });

            File.WriteAllText(@"..\Results\a7_1.txt", String.Join("\r\n", nSentence.OrderByDescending(l => l.Value).Select(l => l.Key.ToString())));
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
