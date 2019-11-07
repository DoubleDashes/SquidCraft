﻿﻿using System;
 using DotNetty.Buffers;
 using SquidCraft.API.Blocks;
 using SquidCraft.API.Blocks.Palette;
 using SquidCraft.Blocks.Palette;
 using SquidCraft.Collections;
 using SquidCraft.Extensions;

 namespace SquidCraft.Blocks
{
    public class BlockStorage : IBlockStorage
    {
        private const byte MinBitsPerBlock = 4;
        private const byte Nil = 0;

        public IBlockPalette BlockPalette { get; private set; }

        public ushort BlockCount
        {
            get
            {
                if (_blockCount != ushort.MaxValue)
                    return _blockCount;

                _blockCount = 0;
                for (var i = 0; i < _nBitsArray.Capacity; i++)
                {
                    if (_nBitsArray[i] != 0)
                        _blockCount++;
                }

                return _blockCount;
            }
        }

        private readonly IBlockPalette _globalPalette;

        private NBitsArray _nBitsArray;
        private ushort _blockCount = ushort.MaxValue;

        public BlockStorage(IBlockPalette globalPalette, byte bitsPerBlock = MinBitsPerBlock)
        {
            _globalPalette = globalPalette;
            BlockPalette = _globalPalette;

            UpdatePalette(bitsPerBlock);
        }

        /// <summary>
        /// Resize the storage to match the bitsPerBlock
        /// </summary>
        /// <param name="bitsPerBlock">The number of bits used to store a block state</param>
        private void Resize(byte bitsPerBlock)
        {
            var previousDataBits = _nBitsArray;
            var previousBlockPalette = BlockPalette;

            if (!UpdatePalette(bitsPerBlock))
                return;

            for (var i = 0; i < previousDataBits.Capacity; i++)
            {
                var blockStateId = previousDataBits[i];
                if (blockStateId == 0)
                    continue;

                var blockState = previousBlockPalette.GetBlockState(blockStateId);
                _nBitsArray[i] = BlockPalette.GetId(blockState);
            }
        }

        /// <summary>
        /// Update the internal palette to match the bitsPerBlock
        /// </summary>
        /// <remarks>This should never be called directly, please use <see cref="Resize"/></remarks>
        /// <param name="bitsPerBlock">The number of bits used to store a block state</param>
        /// <returns>true if the internal array must be updated, false otherwise</returns>
        private bool UpdatePalette(byte bitsPerBlock)
        {
            bitsPerBlock = System.Math.Clamp(bitsPerBlock, MinBitsPerBlock, _globalPalette.BitsPerBlock);

            if (BlockPalette.BitsPerBlock == bitsPerBlock)
                return false;

            var flag = true;
            if (bitsPerBlock <= 8)
            {
                if (BlockPalette is LinearBlockPalette blockPalette)
                {
                    blockPalette.Resize(bitsPerBlock);
                    flag = false;
                }
                else
                    BlockPalette = new LinearBlockPalette(_globalPalette, bitsPerBlock);
            }
            else
                BlockPalette = _globalPalette;

            _nBitsArray = NBitsArray.Create(bitsPerBlock, 4096); // TODO replace that by chunk section block count
            return flag;
        }

        public bool HasBlock(int x, int y, int z)
        {
            var index = Index(x, y, z);
            return _nBitsArray[index] != Nil;
        }

        public IBlockState GetBlock(int x, int y, int z)
        {
            var index = Index(x, y, z);
            var id = _nBitsArray[index];
            return id == Nil ? null : BlockPalette.GetBlockState(id);
        }

        public void SetBlock(int x, int y, int z, IBlockState blockState)
        {
            var id = blockState != null ? BlockPalette.GetId(blockState) : Nil;
            if (id == -1) // block not registered
            {
                Resize((byte) (BlockPalette.BitsPerBlock + 1));
                id = BlockPalette.GetId(blockState);
                if (id == -1)
                    throw new InvalidOperationException("Invalid block state palette id");
            }

            var index = Index(x, y, z);
            _nBitsArray[index] = id;
            _blockCount = ushort.MaxValue;
        }

        public void Serialize(IByteBuffer buffer)
        {
            var backing = _nBitsArray.Backing;
            buffer.WriteVarInt32(backing.Length);
            foreach (var l in backing)
                buffer.WriteLong(l);
        }

        private static int Index(int x, int y, int z)
        {
            return (y & 15) << 8 | (z & 15) << 4 | x & 15;
        }
    }
}