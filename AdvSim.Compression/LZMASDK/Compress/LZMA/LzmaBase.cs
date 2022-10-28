// LzmaBase.cs

using System;

namespace SevenZip.Compression.LZMA
{
	internal abstract class Base
	{
		public const UInt32 kNumRepDistances = 4;
		public const UInt32 kNumStates = 12;

		// static byte []kLiteralNextStates  = {0, 0, 0, 0, 1, 2, 3, 4,  5,  6,   4, 5};
		// static byte []kMatchNextStates    = {7, 7, 7, 7, 7, 7, 7, 10, 10, 10, 10, 10};
		// static byte []kRepNextStates      = {8, 8, 8, 8, 8, 8, 8, 11, 11, 11, 11, 11};
		// static byte []kShortRepNextStates = {9, 9, 9, 9, 9, 9, 9, 11, 11, 11, 11, 11};

		public struct State
		{
			public UInt32 Index;
			public void Init() { Index = 0; }
			public void UpdateChar()
			{
				if (Index < 4) Index = 0;
				else if (Index < 10) Index -= 3;
				else Index -= 6;
			}
			public void UpdateMatch() { Index = (UInt32)(Index < 7 ? 7 : 10); }
			public void UpdateRep() { Index = (UInt32)(Index < 7 ? 8 : 11); }
			public void UpdateShortRep() { Index = (UInt32)(Index < 7 ? 9 : 11); }
			public Boolean IsCharState() { return Index < 7; }
		}

		public const Int32 kNumPosSlotBits = 6;
		public const Int32 kDicLogSizeMin = 0;
		// public const int kDicLogSizeMax = 30;
		// public const uint kDistTableSizeMax = kDicLogSizeMax * 2;

		public const Int32 kNumLenToPosStatesBits = 2; // it's for speed optimization
		public const UInt32 kNumLenToPosStates = 1 << kNumLenToPosStatesBits;

		public const UInt32 kMatchMinLen = 2;

		public static UInt32 GetLenToPosState(UInt32 len)
		{
			len -= kMatchMinLen;
			if (len < kNumLenToPosStates)
				return len;
			return (UInt32)(kNumLenToPosStates - 1);
		}

		public const Int32 kNumAlignBits = 4;
		public const UInt32 kAlignTableSize = 1 << kNumAlignBits;
		public const UInt32 kAlignMask = (kAlignTableSize - 1);

		public const UInt32 kStartPosModelIndex = 4;
		public const UInt32 kEndPosModelIndex = 14;
		public const UInt32 kNumPosModels = kEndPosModelIndex - kStartPosModelIndex;

		public const UInt32 kNumFullDistances = 1 << ((Int32)kEndPosModelIndex / 2);

		public const UInt32 kNumLitPosStatesBitsEncodingMax = 4;
		public const UInt32 kNumLitContextBitsMax = 8;

		public const Int32 kNumPosStatesBitsMax = 4;
		public const UInt32 kNumPosStatesMax = (1 << kNumPosStatesBitsMax);
		public const Int32 kNumPosStatesBitsEncodingMax = 4;
		public const UInt32 kNumPosStatesEncodingMax = (1 << kNumPosStatesBitsEncodingMax);

		public const Int32 kNumLowLenBits = 3;
		public const Int32 kNumMidLenBits = 3;
		public const Int32 kNumHighLenBits = 8;
		public const UInt32 kNumLowLenSymbols = 1 << kNumLowLenBits;
		public const UInt32 kNumMidLenSymbols = 1 << kNumMidLenBits;
		public const UInt32 kNumLenSymbols = kNumLowLenSymbols + kNumMidLenSymbols +
                                             (1 << kNumHighLenBits);
		public const UInt32 kMatchMaxLen = kMatchMinLen + kNumLenSymbols - 1;
	}
}
