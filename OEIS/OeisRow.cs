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

        private object _syncRoot;

        private byte[] _cachedMod29;

        private string _cachedValue;

        private int _cachedLength;


        public int FindPattern(int[] pattern, int sizeLimit = 150)
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
                    _cachedValue = BitConverter.ToString(_cachedMod29);
                    _initialized = true;
                }
            }

            byte[] bytesPattern = pattern.Select(val => val < 0 ? (byte)((29 + val) % 29) : (byte)(val % 29)).ToArray();
            var valueToCheck = BitConverter.ToString(bytesPattern);
            if (_cachedValue.Contains(valueToCheck))
            {
                return PatternAt(_cachedMod29, bytesPattern).First();
            }
            return -1;
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
    }
}
