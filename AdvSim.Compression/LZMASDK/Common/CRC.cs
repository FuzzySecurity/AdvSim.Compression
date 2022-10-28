// Common/CRC.cs

using System;

namespace SevenZip
{
	class CRC
	{
		public static readonly UInt32[] Table;

		static CRC()
		{
			Table = new UInt32[256];
			const UInt32 kPoly = 0xEDB88320;
			for (UInt32 i = 0; i < 256; i++)
			{
				UInt32 r = i;
				for (Int32 j = 0; j < 8; j++)
					if ((r & 1) != 0)
						r = (r >> 1) ^ kPoly;
					else
						r >>= 1;
				Table[i] = r;
			}
		}

		UInt32 _value = 0xFFFFFFFF;

		public void Init() { _value = 0xFFFFFFFF; }

		public void UpdateByte(Byte b)
		{
			_value = Table[(((Byte)(_value)) ^ b)] ^ (_value >> 8);
		}

		public void Update(Byte[] data, UInt32 offset, UInt32 size)
		{
			for (UInt32 i = 0; i < size; i++)
				_value = Table[(((Byte)(_value)) ^ data[offset + i])] ^ (_value >> 8);
		}

		public UInt32 GetDigest() { return _value ^ 0xFFFFFFFF; }

		static UInt32 CalculateDigest(Byte[] data, UInt32 offset, UInt32 size)
		{
			CRC crc = new CRC();
			// crc.Init();
			crc.Update(data, offset, size);
			return crc.GetDigest();
		}

		static Boolean VerifyDigest(UInt32 digest, Byte[] data, UInt32 offset, UInt32 size)
		{
			return (CalculateDigest(data, offset, size) == digest);
		}
	}
}
