using System;

namespace SevenZip.Compression.RangeCoder
{
	class Encoder
	{
		public const UInt32 kTopValue = (1 << 24);

		System.IO.Stream Stream;

		public UInt64 Low;
		public UInt32 Range;
		UInt32 _cacheSize;
		Byte _cache;

		Int64 StartPosition;

		public void SetStream(System.IO.Stream stream)
		{
			Stream = stream;
		}

		public void ReleaseStream()
		{
			Stream = null;
		}

		public void Init()
		{
			StartPosition = Stream.Position;

			Low = 0;
			Range = 0xFFFFFFFF;
			_cacheSize = 1;
			_cache = 0;
		}

		public void FlushData()
		{
			for (Int32 i = 0; i < 5; i++)
				ShiftLow();
		}

		public void FlushStream()
		{
			Stream.Flush();
		}

		public void CloseStream()
		{
			Stream.Close();
		}

		public void Encode(UInt32 start, UInt32 size, UInt32 total)
		{
			Low += start * (Range /= total);
			Range *= size;
			while (Range < kTopValue)
			{
				Range <<= 8;
				ShiftLow();
			}
		}

		public void ShiftLow()
		{
			if ((UInt32)Low < (UInt32)0xFF000000 || (UInt32)(Low >> 32) == 1)
			{
				Byte temp = _cache;
				do
				{
					Stream.WriteByte((Byte)(temp + (Low >> 32)));
					temp = 0xFF;
				}
				while (--_cacheSize != 0);
				_cache = (Byte)(((UInt32)Low) >> 24);
			}
			_cacheSize++;
			Low = ((UInt32)Low) << 8;
		}

		public void EncodeDirectBits(UInt32 v, Int32 numTotalBits)
		{
			for (Int32 i = numTotalBits - 1; i >= 0; i--)
			{
				Range >>= 1;
				if (((v >> i) & 1) == 1)
					Low += Range;
				if (Range < kTopValue)
				{
					Range <<= 8;
					ShiftLow();
				}
			}
		}

		public void EncodeBit(UInt32 size0, Int32 numTotalBits, UInt32 symbol)
		{
			UInt32 newBound = (Range >> numTotalBits) * size0;
			if (symbol == 0)
				Range = newBound;
			else
			{
				Low += newBound;
				Range -= newBound;
			}
			while (Range < kTopValue)
			{
				Range <<= 8;
				ShiftLow();
			}
		}

		public Int64 GetProcessedSizeAdd()
		{
			return _cacheSize +
				Stream.Position - StartPosition + 4;
			// (long)Stream.GetProcessedSize();
		}
	}

	class Decoder
	{
		public const UInt32 kTopValue = (1 << 24);
		public UInt32 Range;
		public UInt32 Code;
		// public Buffer.InBuffer Stream = new Buffer.InBuffer(1 << 16);
		public System.IO.Stream Stream;

		public void Init(System.IO.Stream stream)
		{
			// Stream.Init(stream);
			Stream = stream;

			Code = 0;
			Range = 0xFFFFFFFF;
			for (Int32 i = 0; i < 5; i++)
				Code = (Code << 8) | (Byte)Stream.ReadByte();
		}

		public void ReleaseStream()
		{
			// Stream.ReleaseStream();
			Stream = null;
		}

		public void CloseStream()
		{
			Stream.Close();
		}

		public void Normalize()
		{
			while (Range < kTopValue)
			{
				Code = (Code << 8) | (Byte)Stream.ReadByte();
				Range <<= 8;
			}
		}

		public void Normalize2()
		{
			if (Range < kTopValue)
			{
				Code = (Code << 8) | (Byte)Stream.ReadByte();
				Range <<= 8;
			}
		}

		public UInt32 GetThreshold(UInt32 total)
		{
			return Code / (Range /= total);
		}

		public void Decode(UInt32 start, UInt32 size, UInt32 total)
		{
			Code -= start * Range;
			Range *= size;
			Normalize();
		}

		public UInt32 DecodeDirectBits(Int32 numTotalBits)
		{
			UInt32 range = Range;
			UInt32 code = Code;
			UInt32 result = 0;
			for (Int32 i = numTotalBits; i > 0; i--)
			{
				range >>= 1;
				/*
				result <<= 1;
				if (code >= range)
				{
					code -= range;
					result |= 1;
				}
				*/
				UInt32 t = (code - range) >> 31;
				code -= range & (t - 1);
				result = (result << 1) | (1 - t);

				if (range < kTopValue)
				{
					code = (code << 8) | (Byte)Stream.ReadByte();
					range <<= 8;
				}
			}
			Range = range;
			Code = code;
			return result;
		}

		public UInt32 DecodeBit(UInt32 size0, Int32 numTotalBits)
		{
			UInt32 newBound = (Range >> numTotalBits) * size0;
			UInt32 symbol;
			if (Code < newBound)
			{
				symbol = 0;
				Range = newBound;
			}
			else
			{
				symbol = 1;
				Code -= newBound;
				Range -= newBound;
			}
			Normalize();
			return symbol;
		}

		// ulong GetProcessedSize() {return Stream.GetProcessedSize(); }
	}
}
