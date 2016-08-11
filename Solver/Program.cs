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
        private static List<OeisSearchResult> resultStore = new List<OeisSearchResult>();
        private static SQLiteConnectionWithLock resultsDB;
        static void Main(string[] args)
        {
            Initialize();
            Processor();
            System.Diagnostics.Debugger.Break();
        }

        private static void Initialize()
        {
            book.LoadFromFile(@"..\DataSources\liber-master");

            SQLiteConnectionWithLock oeisDB = new SQLiteConnectionWithLock(
                new SQLitePlatformWin32(),
                new SQLiteConnectionString(@"c:\temp\oeisMod29.db", true)
                );

            sequences = OEIS.Parser.FromDatabase(oeisDB);
            oeisDB.Close();
            resultsDB = new SQLiteConnectionWithLock(
                new SQLitePlatformWin32(),
                new SQLiteConnectionString(@"c:\temp\results.db", true)
                );
            resultsDB.CreateTable<OeisSearchResult>();
            resultStore = resultsDB.Table<OeisSearchResult>().Where(s => s.RefIndex == 9).ToList();
            dictionary.LoadFromFile(@"..\DataSources\CicadaSentences.txt");
            dictionary.LoadFromFile(@"..\DataSources\Titles.txt");
            dictionary.LoadFromFile(@"..\DataSources\MasterMind.txt");
            //dictionary.LoadFromFile(@"..\DataSources\WordList.txt");

        }

        private static void Processor()
        {
            int paragraphIndex = book.Paragraphs.IndexOf(book.Sections[7][0]);
            int maxIndex = book.Paragraphs.IndexOf(book.Sections[book.Sections.Count - 3][0]) - 1;
            int wordIndex = 0;
            int cribIndex = 0;
            int sizeLimit = 13000;

            while (paragraphIndex < maxIndex)
            {
                var paragraph = book.Paragraphs[paragraphIndex];
                var stringCharacters = String.Join("", paragraph.Characters);
                var wordLength = paragraph.Words.Max(w => w.Count);
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
                        var x1 = (runeWord | cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX1 = BitConverter.ToString(x1);

                        var x2 = (runeWord | !cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX2 = BitConverter.ToString(x2);


                        var x3 = (runeWord | cribWord.Reverced).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX3 = BitConverter.ToString(x1);

                        var x4 = (runeWord | !(cribWord.Reverced)).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX4 = BitConverter.ToString(x2);

                        Parallel.ForEach(sequences,
                        sequence =>
                        {
                            CheckSequence(sequence, strX1, sizeLimit, characterIndex, paragraphIndex, runeWordString, cribWordString, "runeWord_cribWord");
                            CheckSequence(sequence, strX2, sizeLimit, characterIndex, paragraphIndex, runeWordString, cribWordString, "runeWord_not_cribWord");
                            CheckSequence(sequence, strX1, sizeLimit, characterIndex, paragraphIndex, runeWordString, cribWordString, "runeWord_rev_cribWord");
                            CheckSequence(sequence, strX2, sizeLimit, characterIndex, paragraphIndex, runeWordString, cribWordString, "runeWord_not_rev_cribWord");
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


        private static void CheckSequence(OeisRow sequence, string stringPattern, int sizeLimit, int characterIndex, int refIndex, string runeWord, string cribWord, string patternName)
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
                        InSectionIndex = characterIndex,
                        InStreamIndex = resultIndex,
                        RuneWord = runeWord.ToString(),
                        CribWord = cribWord.ToString()
                    });
                }
            }
        }
    }
}
