using System;

namespace SevenZip.Compression.RangeCoder
{
	struct BitTreeEncoder
	{
		BitEncoder[] Models;
		Int32 NumBitLevels;

		public BitTreeEncoder(Int32 numBitLevels)
		{
			NumBitLevels = numBitLevels;
			Models = new BitEncoder[1 << numBitLevels];
		}

		public void Init()
		{
			for (UInt32 i = 1; i < (1 << NumBitLevels); i++)
				Models[i].Init();
		}

		public void Encode(Encoder rangeEncoder, UInt32 symbol)
		{
			UInt32 m = 1;
			for (Int32 bitIndex = NumBitLevels; bitIndex > 0; )
			{
				bitIndex--;
				UInt32 bit = (symbol >> bitIndex) & 1;
				Models[m].Encode(rangeEncoder, bit);
				m = (m << 1) | bit;
			}
		}

		public void ReverseEncode(Encoder rangeEncoder, UInt32 symbol)
		{
			UInt32 m = 1;
			for (UInt32 i = 0; i < NumBitLevels; i++)
			{
				UInt32 bit = symbol & 1;
				Models[m].Encode(rangeEncoder, bit);
				m = (m << 1) | bit;
				symbol >>= 1;
			}
		}

		public UInt32 GetPrice(UInt32 symbol)
		{
			UInt32 price = 0;
			UInt32 m = 1;
			for (Int32 bitIndex = NumBitLevels; bitIndex > 0; )
			{
				bitIndex--;
				UInt32 bit = (symbol >> bitIndex) & 1;
				price += Models[m].GetPrice(bit);
				m = (m << 1) + bit;
			}
			return price;
		}

		public UInt32 ReverseGetPrice(UInt32 symbol)
		{
			UInt32 price = 0;
			UInt32 m = 1;
			for (Int32 i = NumBitLevels; i > 0; i--)
			{
				UInt32 bit = symbol & 1;
				symbol >>= 1;
				price += Models[m].GetPrice(bit);
				m = (m << 1) | bit;
			}
			return price;
		}

		public static UInt32 ReverseGetPrice(BitEncoder[] Models, UInt32 startIndex,
			Int32 NumBitLevels, UInt32 symbol)
		{
			UInt32 price = 0;
			UInt32 m = 1;
			for (Int32 i = NumBitLevels; i > 0; i--)
			{
				UInt32 bit = symbol & 1;
				symbol >>= 1;
				price += Models[startIndex + m].GetPrice(bit);
				m = (m << 1) | bit;
			}
			return price;
		}

		public static void ReverseEncode(BitEncoder[] Models, UInt32 startIndex,
			Encoder rangeEncoder, Int32 NumBitLevels, UInt32 symbol)
		{
			UInt32 m = 1;
			for (Int32 i = 0; i < NumBitLevels; i++)
			{
				UInt32 bit = symbol & 1;
				Models[startIndex + m].Encode(rangeEncoder, bit);
				m = (m << 1) | bit;
				symbol >>= 1;
			}
		}
	}

	struct BitTreeDecoder
	{
		BitDecoder[] Models;
		Int32 NumBitLevels;

		public BitTreeDecoder(Int32 numBitLevels)
		{
			NumBitLevels = numBitLevels;
			Models = new BitDecoder[1 << numBitLevels];
		}

		public void Init()
		{
			for (UInt32 i = 1; i < (1 << NumBitLevels); i++)
				Models[i].Init();
		}

		public UInt32 Decode(RangeCoder.Decoder rangeDecoder)
		{
			UInt32 m = 1;
			for (Int32 bitIndex = NumBitLevels; bitIndex > 0; bitIndex--)
				m = (m << 1) + Models[m].Decode(rangeDecoder);
			return m - ((UInt32)1 << NumBitLevels);
		}

		public UInt32 ReverseDecode(RangeCoder.Decoder rangeDecoder)
		{
			UInt32 m = 1;
			UInt32 symbol = 0;
			for (Int32 bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
			{
				UInt32 bit = Models[m].Decode(rangeDecoder);
				m <<= 1;
				m += bit;
				symbol |= (bit << bitIndex);
			}
			return symbol;
		}

		public static UInt32 ReverseDecode(BitDecoder[] Models, UInt32 startIndex,
			RangeCoder.Decoder rangeDecoder, Int32 NumBitLevels)
		{
			UInt32 m = 1;
			UInt32 symbol = 0;
			for (Int32 bitIndex = 0; bitIndex < NumBitLevels; bitIndex++)
			{
				UInt32 bit = Models[startIndex + m].Decode(rangeDecoder);
				m <<= 1;
				m += bit;
				symbol |= (bit << bitIndex);
			}
			return symbol;
		}
	}
}
