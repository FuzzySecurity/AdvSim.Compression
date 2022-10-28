using System;

namespace SevenZip.Compression.RangeCoder
{
	struct BitEncoder
	{
		public const Int32 kNumBitModelTotalBits = 11;
		public const UInt32 kBitModelTotal = (1 << kNumBitModelTotalBits);
		const Int32 kNumMoveBits = 5;
		const Int32 kNumMoveReducingBits = 2;
		public const Int32 kNumBitPriceShiftBits = 6;

		UInt32 Prob;

		public void Init() { Prob = kBitModelTotal >> 1; }

		public void UpdateModel(UInt32 symbol)
		{
			if (symbol == 0)
				Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
			else
				Prob -= (Prob) >> kNumMoveBits;
		}

		public void Encode(Encoder encoder, UInt32 symbol)
		{
			// encoder.EncodeBit(Prob, kNumBitModelTotalBits, symbol);
			// UpdateModel(symbol);
			UInt32 newBound = (encoder.Range >> kNumBitModelTotalBits) * Prob;
			if (symbol == 0)
			{
				encoder.Range = newBound;
				Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
			}
			else
			{
				encoder.Low += newBound;
				encoder.Range -= newBound;
				Prob -= (Prob) >> kNumMoveBits;
			}
			if (encoder.Range < Encoder.kTopValue)
			{
				encoder.Range <<= 8;
				encoder.ShiftLow();
			}
		}

		private static UInt32[] ProbPrices = new UInt32[kBitModelTotal >> kNumMoveReducingBits];

		static BitEncoder()
		{
			const Int32 kNumBits = (kNumBitModelTotalBits - kNumMoveReducingBits);
			for (Int32 i = kNumBits - 1; i >= 0; i--)
			{
				UInt32 start = (UInt32)1 << (kNumBits - i - 1);
				UInt32 end = (UInt32)1 << (kNumBits - i);
				for (UInt32 j = start; j < end; j++)
					ProbPrices[j] = ((UInt32)i << kNumBitPriceShiftBits) +
						(((end - j) << kNumBitPriceShiftBits) >> (kNumBits - i - 1));
			}
		}

		public UInt32 GetPrice(UInt32 symbol)
		{
			return ProbPrices[(((Prob - symbol) ^ ((-(Int32)symbol))) & (kBitModelTotal - 1)) >> kNumMoveReducingBits];
		}
	  public UInt32 GetPrice0() { return ProbPrices[Prob >> kNumMoveReducingBits]; }
		public UInt32 GetPrice1() { return ProbPrices[(kBitModelTotal - Prob) >> kNumMoveReducingBits]; }
	}

	struct BitDecoder
	{
		public const Int32 kNumBitModelTotalBits = 11;
		public const UInt32 kBitModelTotal = (1 << kNumBitModelTotalBits);
		const Int32 kNumMoveBits = 5;

		UInt32 Prob;

		public void UpdateModel(Int32 numMoveBits, UInt32 symbol)
		{
			if (symbol == 0)
				Prob += (kBitModelTotal - Prob) >> numMoveBits;
			else
				Prob -= (Prob) >> numMoveBits;
		}

		public void Init() { Prob = kBitModelTotal >> 1; }

		public UInt32 Decode(RangeCoder.Decoder rangeDecoder)
		{
			UInt32 newBound = (UInt32)(rangeDecoder.Range >> kNumBitModelTotalBits) * (UInt32)Prob;
			if (rangeDecoder.Code < newBound)
			{
				rangeDecoder.Range = newBound;
				Prob += (kBitModelTotal - Prob) >> kNumMoveBits;
				if (rangeDecoder.Range < Decoder.kTopValue)
				{
					rangeDecoder.Code = (rangeDecoder.Code << 8) | (Byte)rangeDecoder.Stream.ReadByte();
					rangeDecoder.Range <<= 8;
				}
				return 0;
			}
			else
			{
				rangeDecoder.Range -= newBound;
				rangeDecoder.Code -= newBound;
				Prob -= (Prob) >> kNumMoveBits;
				if (rangeDecoder.Range < Decoder.kTopValue)
				{
					rangeDecoder.Code = (rangeDecoder.Code << 8) | (Byte)rangeDecoder.Stream.ReadByte();
					rangeDecoder.Range <<= 8;
				}
				return 1;
			}
		}
	}
}
