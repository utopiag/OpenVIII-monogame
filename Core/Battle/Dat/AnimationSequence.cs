using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenVIII.Battle.Dat
{
    public struct AnimationSequence : IReadOnlyList<byte>
    {
        #region Fields

        private readonly IReadOnlyList<byte> _data;
        public readonly int ID;
        public readonly uint Offset;

        #endregion Fields

        #region Constructors

        private AnimationSequence(BinaryReader br, uint start, uint end, int id) : this()
        {
            br.BaseStream.Seek(start, SeekOrigin.Begin);

            ID = id;
            Offset = start;
            _data = br.ReadBytes(((int)Math.Abs(end - start)));
        }

        #endregion Constructors

        #region Methods

        public static IReadOnlyList<AnimationSequence> CreateInstances(BinaryReader br, uint start, uint end)
        {
            // nothing final in here just was trying to dump data to see what was there.
            br.BaseStream.Seek(start, SeekOrigin.Begin);
            uint[] offsets = new uint[br.ReadUInt16()];
            for (ushort i = 0; i < offsets.Length; i++)
            {
                ushort offset = br.ReadUInt16();
                if (offset == 0)
                    continue;
                offsets[i] = offset + start;
            }

            IReadOnlyList<uint> orderedEnumerable = offsets.Where(x => x > 0).Distinct().OrderBy(x => x).ToList().AsReadOnly();
            return orderedEnumerable.Select((x, i) => new AnimationSequence(br, x, orderedEnumerable.Count > i + 1 ? orderedEnumerable[i + 1] : end, i))
                .ToList().AsReadOnly();
        }

        #endregion Methods

        public IEnumerator<byte> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public int Count => _data.Count;

        public byte this[int index] => _data[index];
    }
}