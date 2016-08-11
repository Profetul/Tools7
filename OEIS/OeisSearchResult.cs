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
        public string ID
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_id))
                {
                    _id = String.Format("{0},{1},{2},{3},{4}",
                        SectionIndex,
                        PatterName,
                        OeisId,
                        InStreamIndex,
                        InSectionIndex);
                }
                return _id;
            }
            set
            {
                _id = value;
            }
        }

        [Indexed]
        public int SectionIndex { get; set; }

        [Indexed]
        public string PatterName { get; set; }

        [Indexed]
        public int OeisId { get; set; }

        [Indexed]
        public int InStreamIndex { get; set; }

        [Indexed]
        public int InSectionIndex { get; set; }

        public string RuneWord { get; set; }
        public string CribWord { get; set; }

    }
}
