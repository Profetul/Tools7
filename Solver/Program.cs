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

namespace Solver
{
    public class Result
    {
        public int SectionIndex { get; set; }
        public string PatterName { get; set; }
        public int OeisId { get; set; }
        public int InStreamIndex { get; set; }
        public int InSectionIndex { get; set; }
        public string RuneWord { get; set; }
        public string CribWord { get; set; }

    }


    class Program
    {
        private static object syncRoot = new object();
        private static OEIS.OeisRow[] sequences;
        private static WordDictionary dictionary = new WordDictionary();
        private static Book book = new Book();
        private static List<Result> resultStore = new List<Result>();
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

            dictionary.LoadFromFile(@"..\DataSources\CicadaSentences.txt");
            dictionary.LoadFromFile(@"..\DataSources\Titles.txt");
            //dictionary.LoadFromFile(@"..\DataSources\MasterMind.txt");

        }

        private static void Processor()
        {
            for (int sectionIndex = 9; sectionIndex < book.Sections.Count - 2; sectionIndex++)
            {
                resultStore = new List<Result>();
                long counter = 0;
                GC.Collect();
                var section = book.Sections[sectionIndex];
                var stringSectionCharacters = String.Join("", section.Characters);
                var sizeLimit = section.Characters.Count + stringSectionCharacters.Count(w => w == 'F') + 10;
                var wordLength = 4;// section.Words.Max(w => w.Count);
                var runeWords = section.Words.Where(w => w.Count == wordLength).ToList();
                var cribWords = dictionary.Where(w => w.Key.Count == wordLength).Select(w => w.Key).ToList();

                foreach (var cribWord in cribWords)
                {
                    foreach (var runeWord in runeWords)
                    {
                        var characterIndex = stringSectionCharacters.IndexOf(String.Join("", runeWord));
                        var maxDelta = stringSectionCharacters.Take(characterIndex).Count(w => w == 'F') + 10;

                        var x1 = (runeWord | cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX1 = BitConverter.ToString(x1);
                        var x2 = (runeWord | !cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX2 = BitConverter.ToString(x2);

                        Parallel.ForEach(sequences,
                        sequence =>
                        {
                            CheckSequence(sequence, strX1, sizeLimit, characterIndex, maxDelta, sectionIndex, runeWord, cribWord, "runeWord_cribWord");
                            CheckSequence(sequence, strX2, sizeLimit, characterIndex, maxDelta, sectionIndex, runeWord, cribWord, "runeWord_not_cribWord");
                        });

                        counter++;
                        if (counter % 622 == 0)
                        {
                            counter = 0;
                            Task.Factory.StartNew(() =>
                            {
                                lock (syncRoot)
                                {
                                    List<IGrouping<string, Result>> temp1 = resultStore.GroupBy(g => String.Format("{0}-{1}", g.OeisId, g.PatterName)).Where(k => k.Select(d => d.RuneWord).Distinct().Count() > (runeWords.Count * 0.33)).ToList();
                                    if (temp1.Count > 0)
                                    {
                                        File.WriteAllText(@"..\Results\" + sectionIndex + "_" + wordLength + "_light.json", JsonConvert.SerializeObject(temp1));
                                    }
                                }
                            });
                        }
                    }

                }

                List<IGrouping<string, Result>> temp = resultStore.GroupBy(g => String.Format("{0}-{1}", g.OeisId, g.PatterName)).Where(k => k.Select(d => d.RuneWord).Distinct().Count() > (runeWords.Count * 0.33)).ToList();
                if (temp.Count > 0)
                {
                    File.WriteAllText(@"..\Results\" + sectionIndex + "_" + wordLength + "_light.json", JsonConvert.SerializeObject(temp));
                }
                File.WriteAllText(@"..\Results\" + sectionIndex + "_" + wordLength + ".json", JsonConvert.SerializeObject(resultStore));
            }
        }



        private static void CheckSequence(OeisRow sequence, string stringPattern, int sizeLimit, int characterIndex, int maxDelta, int sectionIndex, Word runeWord, Word cribWord, string patternName)
        {
            int resultIndex = sequence.FindPattern(stringPattern, sizeLimit);
            if (resultIndex > 0 && resultIndex - maxDelta <= characterIndex)
            {
                lock (syncRoot)
                {
                    resultStore.Add(new Result
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
