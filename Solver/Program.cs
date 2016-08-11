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
            resultStore = resultsDB.Table<OeisSearchResult>().Where(s => s.SectionIndex == 9).ToList();
            dictionary.LoadFromFile(@"..\DataSources\CicadaSentences.txt");
            dictionary.LoadFromFile(@"..\DataSources\Titles.txt");
            dictionary.LoadFromFile(@"..\DataSources\MasterMind.txt");
            dictionary.LoadFromFile(@"..\DataSources\WordList.txt");

        }

        private static void Processor()
        {
            int paragraphIndex = book.Paragraphs.IndexOf(book.Sections[7][0]);
            int maxIndex = book.Paragraphs.IndexOf(book.Sections[book.Sections.Count - 3][0]) - 1;
            for(; paragraphIndex < maxIndex; paragraphIndex++)
            {

            }
            for (int sectionIndex = 7; sectionIndex < book.Sections.Count - 2; sectionIndex++)
            {
                //long counter = 7;
                GC.Collect();
                var section = book.Sections[sectionIndex];
                var stringSectionCharacters = String.Join("", section.Characters);
                var sizeLimit = (section.Characters.Count + stringSectionCharacters.Count(w => w == 'F') + 10) * 3;
                var wordLength = section.Words.Max(w => w.Count);
                var runeWords = section.Words.Where(w => w.Count == wordLength).ToList();
                var cribWords = dictionary.Where(w => w.Key.Count == wordLength).Select(w => w.Key).ToList();
                var cribIndex = 0;

                for (; cribIndex < cribWords.Count; cribIndex++)
                {
                    var cribWord = cribWords[cribIndex];
                    foreach (var runeWord in runeWords)
                    {
                        var characterIndex = stringSectionCharacters.IndexOf(String.Join("", runeWord));
                        var maxDelta = stringSectionCharacters.Take(characterIndex).Count(w => w == 'F') + 10;

                        var x1 = (runeWord | cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX1 = BitConverter.ToString(x1);
                        //var x2 = (runeWord | !cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        //var strX2 = BitConverter.ToString(x2);

                        Parallel.ForEach(sequences,
                        sequence =>
                        {
                            CheckSequence(sequence, strX1, sizeLimit, characterIndex, maxDelta, sectionIndex, runeWord, cribWord, "runeWord_cribWord");
                            //CheckSequence(sequence, strX2, sizeLimit, characterIndex, maxDelta, sectionIndex, runeWord, cribWord, "runeWord_not_cribWord");
                        });
                        //counter++;
                        //if (counter % 622 == 0)
                        //{
                        //    counter = 0;
                        //    Task.Factory.StartNew(() =>
                        //    {
                        //        lock (syncRoot)
                        //        {
                        //            List<IGrouping<string, OeisSearchResult>> temp1 = resultStore.GroupBy(g => String.Format("{0}-{1}", g.OeisId, g.PatterName)).Where(k => k.Select(d => d.RuneWord).Distinct().Count() > (runeWords.Count * 0.33)).ToList();
                        //            if (temp1.Count > 0)
                        //            {
                        //                File.WriteAllText(@"..\Results\" + sectionIndex + "_" + wordLength + "_light.json", JsonConvert.SerializeObject(temp1));
                        //            }
                        //        }
                        //    });
                        //}
                    }

                }


                //List<IGrouping<string, OeisSearchResult>> temp = resultStore.GroupBy(g => String.Format("{0}-{1}", g.OeisId, g.PatterName)).Where(k => k.Select(d => d.RuneWord).Distinct().Count() > (runeWords.Count * 0.33)).ToList();
                //if (temp.Count > 0)
                //{
                //    var rows = temp.Aggregate(new List<OeisSearchResult>(), (a, n) =>
                //    {
                //        a.AddRange(n);
                //        return a;
                //    });

                //    resultsDB.InsertOrIgnoreAll(rows);
                //}
                //resultStore = new List<OeisSearchResult>();

            }

        }



        private static void CheckSequence(OeisRow sequence, string stringPattern, int sizeLimit, int characterIndex, int maxDelta, int sectionIndex, Word runeWord, Word cribWord, string patternName)
        {
            int resultIndex = sequence.FindPattern(stringPattern, sizeLimit);
            if (resultIndex > 0 /*&& resultIndex - maxDelta <= characterIndex*/)
            {
                //lock (syncRoot)
                //{
                //    resultStore.Add(new OeisSearchResult
                //    {
                //        SectionIndex = sectionIndex,
                //        PatterName = patternName,
                //        OeisId = sequence.OeisId,
                //        InSectionIndex = characterIndex,
                //        InStreamIndex = resultIndex,
                //        RuneWord = runeWord.ToString(),
                //        CribWord = cribWord.ToString()
                //    });
                //}
                using (resultsDB.Lock())
                {
                    resultsDB.InsertOrIgnore(new OeisSearchResult
                    {
                        SectionIndex = sectionIndex,
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
