using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Solver
{
    public class OeisRow
    {
        [PrimaryKey]
        public int OeisId { get; set; }

        public byte[] Mod29 { get; set; }

        public byte[] Mod29Rev { get; set; }

        public byte[] Diff1 { get; set; }

        public byte[] Diff2 { get; set; }

        public byte[] Diff3 { get; set; }

        private string _mod29Str;

        public string Mod29Str
        {
            get
            {
                if (_mod29Str == null)
                {
                    _mod29Str = String.Join(",", Mod29);
                }
                return _mod29Str;
            }
        }
    }
}
