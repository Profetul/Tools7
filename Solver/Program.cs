using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataModels;
using SQLite.Net;
using SQLite.Net.Platform.Win32;
using Cryptanalysis;
using OEIS;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Diagnostics;
using SQLite.Net.Attributes;
using System.Numerics;

namespace Solver
{
    class Program
    {
        private static object syncRoot = new object();
        private static OEIS.OeisRow[] sequences;
        private static WordDictionary dictionary = new WordDictionary();
        private static Book book = new Book();
        private static SQLiteConnectionWithLock resultsDB = new SQLiteConnectionWithLock(new SQLitePlatformWin32(), new SQLiteConnectionString(@"c:\temp\results.db", true));
        private static SQLiteConnectionWithLock oeisDB = new SQLiteConnectionWithLock(new SQLitePlatformWin32(), new SQLiteConnectionString(@"c:\temp\oeisMod29.db", true));
        static void Main(string[] args)
        {
            Initialize();
            ProcessStats();
            System.Diagnostics.Debugger.Break();
        }

        private static void Initialize()
        {

            sequences = oeisDB.Table<OeisRow>().ToArray();
            book.LoadFromFile(@"..\DataSources\liber-work");
        }

        private class TestResult
        {
            public int OeisId { get; set; }
            public int KeySize { get; set; }
            public string Key { get; set; }
            public int TextSize { get; set; }
            public string Text { get; set; }
            public int DoublesCount { get; set; }
            public string IoCs { get; set; }
        }
        private static void ProcessStats()
        {
            List<Character> source = @"..\DataSources\geb.txt".CharactersFromFile();
            ConcurrentBag<TestResult> results = new ConcurrentBag<TestResult>();
            int sectionIndex = 0;
            for (; sectionIndex < book.Sections.Count - 2; sectionIndex++)
            {
                int charactersCount = book.Sections[sectionIndex].Characters.Count;
                int doublesCount = book.Sections[sectionIndex].Characters.DoublesCount();
                int offset = 0;
                while (offset < source.Count - charactersCount)
                {
                    var sampleCharacters = source.Skip(offset).Take(charactersCount).ToList();
                    Parallel.For(0, sequences.Length, sequenceIndex =>
                    {
                        var sequence = sequences[sequenceIndex];
                        var key = sequence.CacheValueOfSize(charactersCount);
                        var sampleResult = Ciphers.EncodeVigenereSerial(key, sampleCharacters);
                        var sampleDoublesCount = sampleResult.DoublesCount();
                        var testIoc = sampleResult.ToIoC()[0];
                        if (sampleDoublesCount <= doublesCount && testIoc < 1.1)
                        {
                            var iocsTable = sampleResult.ToIoCTable();
                            if (!iocsTable.Any(v => v.Value > 1.2))
                            {
                                var iocs = iocsTable.Select(r => "(" + r.Key + "=" + r.Value + ")").ToList();
                                results.Add(new TestResult
                                {
                                    OeisId = sequence.OeisId,
                                    KeySize = key.Length,
                                    Key = String.Join(",", key),
                                    TextSize = charactersCount,
                                    Text = sampleCharacters.AsWord().Runes,
                                    DoublesCount = sampleDoublesCount,
                                    IoCs = String.Join(",", iocs)
                                });
                            }
                        }
                    });
                    offset += charactersCount;
                }
            }

            var res = results.OrderByDescending(r => r.DoublesCount).ToList();
            File.WriteAllText(@"..\Results\TestForDoublesGEB.txt", JsonConvert.SerializeObject(res));
        }

        private static void Processor()
        {
            int paragraphIndex = 27;//book.Paragraphs.IndexOf(book.Sections[7][0]);
            int maxIndex = book.Paragraphs.IndexOf(book.Sections[book.Sections.Count - 3][0]) - 1;
            int wordIndex = 0;
            int cribIndex = 451;
            int sizeLimit = 3500;

            while (paragraphIndex < maxIndex)
            {
                var paragraph = book.Paragraphs[paragraphIndex];
                var stringCharacters = String.Join("", paragraph.Characters);
                var wordLength = paragraph.Words.Any(w => w.Count > 11) ? paragraph.Words.Select(w => w.Count).Max() : 0;
                var runeWords = paragraph.Words.Where(w => w.Count == wordLength).ToList();
                var cribWords = dictionary.Where(w => w.Key.Count == wordLength).Select(w => w.Key).ToList();
                while (wordIndex < runeWords.Count)
                {
                    var runeWord = runeWords[wordIndex];
                    var runeWordString = runeWord.ToString();
                    var characterIndex = stringCharacters.IndexOf(String.Join("", runeWord));

                    while (cribIndex < cribWords.Count)
                    {
                        var cribWord = cribWords[cribIndex];
                        var cribWordString = cribWord.ToString();
                        var x1 = (runeWord - cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX1 = "-" + String.Join("-", x1) + "-";
                        Parallel.ForEach(sequences,
                        sequence =>
                        {
                            CheckSequence(sequence, strX1, sizeLimit, paragraphIndex, runeWordString, cribWordString, "runeWord_cribWord");
                        });

                        cribIndex++;
                    }
                    wordIndex++;
                    cribIndex = 0;
                }
                paragraphIndex++;
                wordIndex = 0;
            }

        }

        private static void CheckSequence(OeisRow sequence, string stringPattern, int sizeLimit, int refIndex, string runeWord, string cribWord, string patternName)
        {
            int resultIndex = sequence.FindPattern(stringPattern, sizeLimit);
            if (resultIndex > 0)
            {
                using (resultsDB.Lock())
                {
                    resultsDB.InsertOrIgnore(new OeisSearchResult
                    {
                        RefIndex = refIndex,
                        PatterName = patternName,
                        OeisId = sequence.OeisId,
                        RuneWord = runeWord.ToString(),
                        CribWord = cribWord.ToString(),
                        Pattern = stringPattern
                    });
                }
            }
        }
    }
}
