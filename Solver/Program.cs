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
        private static SQLiteConnectionWithLock resultsDB = null;
        static void Main(string[] args)
        {
            Initialize();
            System.Diagnostics.Debugger.Break();
        }

        private static void Initialize()
        {
            book.LoadFromFile(@"..\DataSources\liber-work");
            for (int paragraphIndex = 0; paragraphIndex < book.Paragraphs.Count - 2; paragraphIndex++)
            {
                List<Character> paragraphCharacters = book.Paragraphs[paragraphIndex].Characters;
                string values = String.Join(" ",paragraphCharacters.Select(s => s.GematriaIndex.ToString("00")));
                for (int nCount = 2; nCount < paragraphCharacters.Count; nCount++)
                {
                    if (paragraphCharacters.Count % nCount != 0)
                        continue;

                    List<Character> newCharacters = new List<Character>();
                    for (int col = 0; col < nCount; col++)
                    {
                        for (int row = 0; row < paragraphCharacters.Count / nCount; row++)
                        {
                            var c = paragraphCharacters[row * nCount + col];
                            newCharacters.Add(c);
                        }
                    }
                    var orgBiGrams = paragraphCharacters.NGramCount(2).Where(k => k.Key[0].GematriaIndex == k.Key[1].GematriaIndex).Select(v => v.Value).Sum();
                    var biGrams = newCharacters.NGramCount(2).Where(k => k.Key[0].GematriaIndex == k.Key[1].GematriaIndex).Select(v => v.Value).Sum();
                    var triGrams = newCharacters.NGramCount(3).Where(k => k.Key[0].GematriaIndex == k.Key[1].GematriaIndex && k.Key[0].GematriaIndex == k.Key[2].GematriaIndex).Select(v => v.Value).Sum();
                    var quadGrams = newCharacters.NGramCount(4).Where(k => k.Key[0].GematriaIndex == k.Key[1].GematriaIndex && k.Key[0].GematriaIndex == k.Key[2].GematriaIndex && k.Key[0].GematriaIndex == k.Key[3].GematriaIndex).Select(v => v.Value).Sum();
                }
            }
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
