using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class Page : List<Line>
    {
        private string _inRunes;
        public string Runes
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_inRunes))
                {
                    _inRunes = String.Join("\r\n", this.Select(c => c.Runes));
                }
                return _inRunes;
            }
        }

        private string _inLatin;
        public string Latin
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_inLatin))
                {
                    _inLatin = String.Join("\r\n", this.Select(c => c.Latin));
                }
                return _inLatin;
            }
        }

        public long PrimeSum
        {
            get
            {
                return this.Select(c => c.PrimeSum).Sum();
            }
        }

        public BigInteger PrimeMult
        {
            get
            {
                return this.Aggregate((BigInteger)1, (a, c) => BigInteger.Multiply(a, c.PrimeSum));
            }
        }

        public override string ToString()
        {
            return Latin;
        }

        public string ToPrimeSumString()
        {
            return String.Join("\r\n", this.Select(l => l.PrimeSum)) + "\r\nPage Total (sum): " + PrimeSum + "\r\nPage Total (mult): " + PrimeMult;
        }
    }

}
