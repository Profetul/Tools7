using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEIS
{
    public class OeisSearchResult
    {
        private string _id;

        [PrimaryKey]
        [AutoIncrement]
        public int ID
        {
            get; set;
        }

        [Indexed]
        public int RefIndex { get; set; }

        [Indexed]
        public string PatterName { get; set; }

        [Indexed]
        public int OeisId { get; set; }

        public string RuneWord { get; set; }

        public string CribWord { get; set; }

        public string Pattern { get; set; }


    }
}
