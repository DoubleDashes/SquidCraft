﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SquidCraft.NBT
{
    public class NbtCompound : NbtTag<IDictionary<string, INbtTag>>, IDictionary<string, INbtTag>,
        IReadOnlyDictionary<string, INbtTag>
    {
        private static readonly UTF8Encoding Utf8Encoding = new UTF8Encoding(false, true);

        public override byte Id { get; } = 10;

        public int Count => Value.Count;
        public ICollection<string> Keys => Value.Keys;
        public ICollection<INbtTag> Values => Value.Values;

        IEnumerable<INbtTag> IReadOnlyDictionary<string, INbtTag>.Values => Values;
        IEnumerable<string> IReadOnlyDictionary<string, INbtTag>.Keys => Keys;

        bool ICollection<KeyValuePair<string, INbtTag>>.IsReadOnly => false;

        public NbtCompound() : base(new Dictionary<string, INbtTag>())
        {
        }

        public override void Serialize(BinaryWriter writer)
        {
            foreach (var (name, tag) in Value)
            {
                writer.Write(tag.Id);

                var nameByteCount = (short) Utf8Encoding.GetByteCount(name);
                writer.Write(nameByteCount);

                var nameBytes = Utf8Encoding.GetBytes(name);
                writer.Write(nameBytes);

                tag.Serialize(writer);
            }

            writer.Write(byte.MinValue);
        }

        public IEnumerator<KeyValuePair<string, INbtTag>> GetEnumerator()
        {
            return Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        void ICollection<KeyValuePair<string, INbtTag>>.Add(KeyValuePair<string, INbtTag> item)
        {
            Value.Add(item);
        }

        bool ICollection<KeyValuePair<string, INbtTag>>.Contains(KeyValuePair<string, INbtTag> item)
        {
            return Value.Contains(item);
        }

        void ICollection<KeyValuePair<string, INbtTag>>.CopyTo(KeyValuePair<string, INbtTag>[] array,
            int arrayIndex)
        {
            Value.CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, INbtTag>>.Remove(KeyValuePair<string, INbtTag> item)
        {
            return Value.Remove(item);
        }

        public void Clear()
        {
            Value.Clear();
        }

        public void Add(string key, INbtTag tag)
        {
            Value[key] = tag;
        }

        public bool ContainsKey(string key)
        {
            return Value.ContainsKey(key);
        }

        public bool Remove(string key)
        {
            return Value.Remove(key);
        }

        public bool TryGetValue(string key, out INbtTag tag)
        {
            return Value.TryGetValue(key, out tag);
        }

        public INbtTag this[string key]
        {
            get => Value[key];
            set => Value[key] = value;
        }
    }
}