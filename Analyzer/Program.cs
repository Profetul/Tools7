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

namespace Analyzer
{
    class Program
    {
        static Book book = new Book();
        static void Main(string[] args)
        {
            book.LoadFromFile(@"..\DataSources\liber-master");
            WordDictionary dictionary = new WordDictionary();
            //dictionary.LoadFromFile(@"..\DataSources\CicadaSentences.txt");
            //dictionary.LoadFromFile(@"..\DataSources\Titles.txt");
            //dictionary.LoadFromFile(@"..\DataSources\WordList.txt");
            StringBuilder sb = new StringBuilder();
            for (int secIndex = 7; secIndex < book.Sections.Count - 2; secIndex++)
            {
                //var section = book.Sections[secIndex];
                //foreach (Word word in section.Words)
                //{
                //    var cribs = dictionary.Where(d => d.Key.PrimeSum == word.PrimeSum).ToList();
                //}
                sb.AppendLine("--- Start Page " + (secIndex - 7) + " --- ");
                sb.AppendLine(book.Sections[secIndex].ToPrimeSumString());
                sb.AppendLine(" --- End Page " + (secIndex - 7) + " --- \r\n");
            }
        }
    }
}
