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

namespace Solver
{
    class Program
    {
        private static object syncRoot = new object();
        private static OEIS.OeisRow[] sequences;
        private static WordDictionary dictionary = new WordDictionary();
        private static Book book = new Book();
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
            //dictionary.LoadFromFile(@"..\DataSources\MasterMind.txt");

        }

        private static void Processor()
        {
            for (int sectionIndex = 7; sectionIndex < book.Sections.Count - 2; sectionIndex++)
            {
                var section = book.Sections[sectionIndex];
                var sizeLimit = section.Characters.Count;
                var wordLength = 4;// section.Words.Max(w => w.Count);
                var runeWords = section.Words.Where(w => w.Count == wordLength).ToList();
                var cribWords = dictionary.Where(w => w.Key.Count == wordLength).Select(w => w.Key).ToList();
                foreach (var cribWord in cribWords)
                {
                    foreach (var runeWord in runeWords)
                    {
                        //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                        //sw.Start();

                        var x1 = (runeWord | cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX1 = BitConverter.ToString(x1);
                        var x2 = (runeWord | !cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX2 = BitConverter.ToString(x2);
                        var x3 = (!runeWord | cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX3 = BitConverter.ToString(x3);
                        var x4 = (!runeWord | !cribWord).Select(b => (sbyte)b < 0 ? (byte)((29 + (sbyte)b) % 29) : (byte)b).ToArray();
                        var strX4 = BitConverter.ToString(x4);

                        Parallel.ForEach(sequences,
                        sequence =>
                        {
                            CheckSequence(sequence, strX1, x1, sizeLimit, sectionIndex, runeWord, cribWord, "runeWord_cribWord");
                            CheckSequence(sequence, strX2, x2, sizeLimit, sectionIndex, runeWord, cribWord, "runeWord_not_cribWord");
                            CheckSequence(sequence, strX3, x3, sizeLimit, sectionIndex, runeWord, cribWord, "not_runeWord_cribWord");
                            CheckSequence(sequence, strX4, x4, sizeLimit, sectionIndex, runeWord, cribWord, "not_runeWord_not_cribWord");
                        });

                        //sw.Stop();
                    }
                }
            }
        }

        private static void CheckSequence(OeisRow sequence, string stringPattern, byte[] bytePattern, int sizeLimit, int sectionIndex, Word runeWord, Word cribWord, string patternName)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            int result = sequence.FindPattern(stringPattern, bytePattern, sizeLimit * 2);
            if (result > 0)
            {
                lock (syncRoot)
                {
                    var outputData = String.Format("{0},{1},{2},{3},{4},{5},{6},{7}\r\n",
                        sectionIndex,
                        patternName,
                        sequence.OeisId,
                        result,
                        runeWord.Count,
                        runeWord.ToString(),
                        cribWord.ToString(),
                        String.Join("-", bytePattern));
                    File.AppendAllText(String.Format("..\\Results\\{0}_{1}.txt", sectionIndex, patternName), outputData);
                }
            }
            //sw.Stop();
        }
    }
}
