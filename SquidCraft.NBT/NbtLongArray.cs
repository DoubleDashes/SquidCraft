﻿using System.IO;

namespace SquidCraft.NBT
{
    public class NbtLongArray : NbtTag<long[]>
    {
        public override byte Id { get; } = 12;

        public NbtLongArray(long[] value = null) : base(value ?? new long[0])
        {
        }

        public override void Serialize(BinaryWriter writer)
        {
            writer.Write(Value.Length);
            foreach (var l in Value)
                writer.Write(l);
        }
    }
}