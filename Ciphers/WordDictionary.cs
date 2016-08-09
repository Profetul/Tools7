using DataModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cryptanalysis
{
    public class WordDictionary : Dictionary<Word, string>
    {
        public WordDictionary()
        {

        }

        public void LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            File.ReadAllLines(filePath).Select(line =>
            {
                string[] words = line.Replace("'", "").Split(new char[] { ' ', '.', ',', ';', '-', '"' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var word in words)
                {
                    var runeWord = word.ToWord();
                    if (!this.ContainsKey(runeWord))
                    {
                        this.Add(runeWord, word.ToUpper());
                    }
                }
                return 0;
            }).ToList();
            var wrongWords = this.Where(w => w.Key.Count == 1 && !(w.Value == "A" || w.Value == "I")).Select(w => w.Key).ToList();
            foreach (var wrongWord in wrongWords)
            {
                this.Remove(wrongWord);
            }
        }
    }
}
