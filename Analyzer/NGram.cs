using DataModels;
using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Analyzer
{
    public class NGramStat
    {
        [PrimaryKey]
        public string ID
        {
            get
            {
                return Word.ToString();
            }
            set
            {
                Word = value.ToWord();
            }
        }

        [Ignore]
        public Word Word
        {
            get; set;
        }

        public long Counts
        {
            get;
            set;
        }

        public double Log10
        {
            get; set;
        }
    }
}
