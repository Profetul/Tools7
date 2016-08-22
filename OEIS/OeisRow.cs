using SQLite.Net.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OEIS
{
    public class OeisRow
    {
        [PrimaryKey]
        public int OeisId { get; set; }

        public byte[] Mod29 { get; set; }

        private bool _initialized;

        private object _syncRoot = new object();

        private byte[] _cachedMod29;

        private string _cachedValue;

        private int _cachedLength;


        public void ComputeMod29()
        {
            Mod29 = Mod29.Select(m => (sbyte)m < 0 ? (byte)((29 + (sbyte)m) % 29) : m).ToArray();
        }

        public int FindPattern(string stringPattern, int sizeLimit = 150)
        {
            CacheValueOfSize(sizeLimit, true);
            var resultIndex = _cachedValue.IndexOf(stringPattern);
            return resultIndex;
        }


        public byte[] CacheValueOfSize(int sizeLimit, bool andString = false)
        {
            lock (_syncRoot)
            {
                if (sizeLimit != _cachedLength)
                {
                    _initialized = false;
                }

                if (!_initialized)
                {
                    _cachedLength = sizeLimit;
                    _cachedMod29 = Mod29.Take(sizeLimit).Select(val => (sbyte)val < 0 ? (byte)((29 + (sbyte)val) % 29) : val).ToArray();
                    if (andString)
                    {
                        _cachedValue = "-" + String.Join("-", _cachedMod29) + "-";
                        _cachedMod29 = null;
                    }
                    _initialized = true;
                }
            }
            return _cachedMod29;
        }

        private static IEnumerable<int> PatternAt(byte[] source, byte[] pattern)
        {
            for (int i = 0; i < source.Length; i++)
            {
                if (source.Skip(i).Take(pattern.Length).SequenceEqual(pattern))
                {
                    yield return i;
                }
            }
        }

        public int FirstIndexOf(byte[] pattern, bool skipZero = false, int maxOffset = -1)
        {
            var max = maxOffset > 0 ? Math.Min(maxOffset, Mod29.Length - pattern.Length) : Mod29.Length - pattern.Length;
            for (int index = 0; index < max; index++)
            {
                bool found = true;
                int indexInPattern = 0;
                int offset = 0;
                while (indexInPattern < pattern.Length)
                {
                    found = found && Mod29[index + offset] == pattern[indexInPattern];
                    if (!found && pattern[indexInPattern] != 0 && skipZero)
                    {
                        break;
                    }
                    found = true;
                    if (pattern[indexInPattern] != 0 || !skipZero)
                    {
                        offset++;
                    }
                    indexInPattern++;
                }
                if (found)
                {
                    return index;
                }
            }
            return -1;
        }
    }
}
