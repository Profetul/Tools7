using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataModels
{
    public class Line : List<Character>
    {
        private string _inRunes;
        public string Runes
        {
            get
            {
                if (String.IsNullOrWhiteSpace(_inRunes))
                {
                    _inRunes = String.Join("", this.Select(c => c.Rune));
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
                    _inLatin = String.Join("ˌ", this.Select(c => c.Type == CharacterType.Rune ? Alphabets.GEMATRIA[c.Rune] :
                    c.Type != CharacterType.Space ? c.Rune.ToString() + " " : c.Rune.ToString()));
                }
                return _inLatin;
            }
        }

        public long RuneSum
        {
            get
            {
                return this.Select(c => c.PrimeValue).Sum();
            }
        }
        public override string ToString()
        {
            return Latin;
        }
    }
}
