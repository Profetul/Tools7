using Newtonsoft.Json;
using OEIS;
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
        static void Main(string[] args)
        {
            SQLiteConnectionWithLock resultsDB = new SQLiteConnectionWithLock(
              new SQLitePlatformWin32(),
              new SQLiteConnectionString(@"c:\temp\results.db", true)
              );
            string json = File.ReadAllText(@"C:\Users\xbox1\Documents\GitHub\Tools7\Results\9_4_light.json");
            List<List<OeisSearchResult>> oeisresults = JsonConvert.DeserializeObject<List<List<OeisSearchResult>>>(json);
            List<OeisSearchResult> results = oeisresults.Aggregate(new List<OeisSearchResult>(), (a, n) =>
            {
                a.AddRange(n);
                return a;
            });
            resultsDB.InsertOrIgnoreAll(results);
        }
    }
}
