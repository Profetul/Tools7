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

namespace Solver
{
    class Program
    {
        private static object syncRoot = new object();
        private static OEIS.OeisRow[] sequences;
        private static WordDictionary dictionary = new WordDictionary();
        private static Book book = new Book();
        private static SQLiteConnectionWithLock resultsDB = null;
        static void Main(string[] args)
        {
            Initialize();
            System.Diagnostics.Debugger.Break();
        }

        private static void Initialize()
        {
            book.LoadFromFile(@"..\DataSources\liber-master");

            SQLiteConnectionWithLock oeisDB = new SQLiteConnectionWithLock(
                new SQLitePlatformWin32(),
                new SQLiteConnectionString(@"c:\temp\oeisMod29.db", true)
                );

            //sequences = OEIS.Parser.FromDatabase(oeisDB);
            oeisDB.CreateTable<OeisRow>();
            oeisDB.CreateTable<NGramStat>();
            //oeisDB.Close();
            //dictionary.LoadFromFile(@"..\DataSources\CicadaSentences.txt");
            //dictionary.LoadFromFile(@"..\DataSources\MasterMind.txt");
            //dictionary.LoadFromFile(@"..\DataSources\Koans.txt");
            //List<Character> stats = new List<Character>();
            //File.ReadAllLines(@"..\DataSources\CicadaSentences.txt").Select(line =>
            //{
            //    string[] words = line.Replace("'", "").Split(new char[] { ' ', '.', ',', ';', '-', '"' }, StringSplitOptions.RemoveEmptyEntries);
            //    foreach (var word in words)
            //    {
            //        var runeWord = word.ToWord();
            //        stats.AddRange(runeWord);
            //    }
            //    return 0;
            //}).ToList();

            //File.ReadAllLines(@"..\DataSources\MasterMind.txt").Select(line =>
            //{
            //    string[] words = line.Replace("'", "").Split(new char[] { ' ', '.', ',', ';', '-', '"' }, StringSplitOptions.RemoveEmptyEntries);
            //    foreach (var word in words)
            //    {
            //        var runeWord = word.ToWord();
            //        stats.AddRange(runeWord);
            //    }
            //    return 0;
            //}).ToList();

            //File.ReadAllLines(@"..\DataSources\Koans.txt").Select(line =>
            //{
            //    string[] words = line.Replace("'", "").Split(new char[] { ' ', '.', ',', ';', '-', '"' }, StringSplitOptions.RemoveEmptyEntries);
            //    foreach (var word in words)
            //    {
            //        var runeWord = word.ToWord();
            //        stats.AddRange(runeWord);
            //    }
            //    return 0;
            //}).ToList();

            //var bigrams = stats.NGramOffsets(2).Select(v => new NGramStat { Word = v.Key, Counts = v.Value.Count }).ToList();
            //var trigrams = stats.NGramOffsets(3).Select(v => new NGramStat { Word = v.Key, Counts = v.Value.Count }).ToList();
            //var quadgrams = stats.NGramOffsets(4).Select(v => new NGramStat { Word = v.Key, Counts = v.Value.Count }).ToList();

            //var nBigrams = (double)bigrams.Select(v => v.Counts).Sum();
            //Parallel.ForEach(bigrams, b => b.Log10 = Math.Log10((double)b.Counts / nBigrams));

            //var nTrigrams = (double)trigrams.Select(v => v.Counts).Sum();
            //Parallel.ForEach(trigrams, b => b.Log10 = Math.Log10((double)b.Counts / nTrigrams));

            //var nQuadgrams = (double)quadgrams.Select(v => v.Counts).Sum();
            //Parallel.ForEach(quadgrams, b => b.Log10 = Math.Log10((double)b.Counts / nQuadgrams));

            
            //oeisDB.InsertOrIgnore(bigrams);
            //oeisDB.InsertOrIgnore(trigrams);
            //oeisDB.InsertOrIgnore(quadgrams);

            //int i = 0;

            //resultsDB = new SQLiteConnectionWithLock(
            //    new SQLitePlatformWin32(),
            //    new SQLiteConnectionString(@"c:\temp\results.db", true)
            //    );
            //resultsDB.CreateTable<OeisSearchResult>();


            //dictionary.LoadFromFile(@"..\DataSources\Titles.txt");

            //dictionary.LoadFromFile(@"..\DataSources\WordList.txt");

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
