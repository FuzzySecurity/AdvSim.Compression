// LzmaDecoder.cs

using System;

namespace SevenZip.Compression.LZMA
{
	using RangeCoder;

	public class Decoder : ICoder, ISetDecoderProperties // ,System.IO.Stream
	{
		class LenDecoder
		{
			BitDecoder m_Choice = new BitDecoder();
			BitDecoder m_Choice2 = new BitDecoder();
			BitTreeDecoder[] m_LowCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
			BitTreeDecoder[] m_MidCoder = new BitTreeDecoder[Base.kNumPosStatesMax];
			BitTreeDecoder m_HighCoder = new BitTreeDecoder(Base.kNumHighLenBits);
			UInt32 m_NumPosStates = 0;

			public void Create(UInt32 numPosStates)
			{
				for (UInt32 posState = m_NumPosStates; posState < numPosStates; posState++)
				{
					m_LowCoder[posState] = new BitTreeDecoder(Base.kNumLowLenBits);
					m_MidCoder[posState] = new BitTreeDecoder(Base.kNumMidLenBits);
				}
				m_NumPosStates = numPosStates;
			}

			public void Init()
			{
				m_Choice.Init();
				for (UInt32 posState = 0; posState < m_NumPosStates; posState++)
				{
					m_LowCoder[posState].Init();
					m_MidCoder[posState].Init();
				}
				m_Choice2.Init();
				m_HighCoder.Init();
			}

			public UInt32 Decode(RangeCoder.Decoder rangeDecoder, UInt32 posState)
			{
				if (m_Choice.Decode(rangeDecoder) == 0)
					return m_LowCoder[posState].Decode(rangeDecoder);
				else
				{
					UInt32 symbol = Base.kNumLowLenSymbols;
					if (m_Choice2.Decode(rangeDecoder) == 0)
						symbol += m_MidCoder[posState].Decode(rangeDecoder);
					else
					{
						symbol += Base.kNumMidLenSymbols;
						symbol += m_HighCoder.Decode(rangeDecoder);
					}
					return symbol;
				}
			}
		}

		class LiteralDecoder
		{
			struct Decoder2
			{
				BitDecoder[] m_Decoders;
				public void Create() { m_Decoders = new BitDecoder[0x300]; }
				public void Init() { for (Int32 i = 0; i < 0x300; i++) m_Decoders[i].Init(); }

				public Byte DecodeNormal(RangeCoder.Decoder rangeDecoder)
				{
					UInt32 symbol = 1;
					do
						symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
					while (symbol < 0x100);
					return (Byte)symbol;
				}

				public Byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, Byte matchByte)
				{
					UInt32 symbol = 1;
					do
					{
						UInt32 matchBit = (UInt32)(matchByte >> 7) & 1;
						matchByte <<= 1;
						UInt32 bit = m_Decoders[((1 + matchBit) << 8) + symbol].Decode(rangeDecoder);
						symbol = (symbol << 1) | bit;
						if (matchBit != bit)
						{
							while (symbol < 0x100)
								symbol = (symbol << 1) | m_Decoders[symbol].Decode(rangeDecoder);
							break;
						}
					}
					while (symbol < 0x100);
					return (Byte)symbol;
				}
			}

			Decoder2[] m_Coders;
			Int32 m_NumPrevBits;
			Int32 m_NumPosBits;
			UInt32 m_PosMask;

			public void Create(Int32 numPosBits, Int32 numPrevBits)
			{
				if (m_Coders != null && m_NumPrevBits == numPrevBits &&
					m_NumPosBits == numPosBits)
					return;
				m_NumPosBits = numPosBits;
				m_PosMask = ((UInt32)1 << numPosBits) - 1;
				m_NumPrevBits = numPrevBits;
				UInt32 numStates = (UInt32)1 << (m_NumPrevBits + m_NumPosBits);
				m_Coders = new Decoder2[numStates];
				for (UInt32 i = 0; i < numStates; i++)
					m_Coders[i].Create();
			}

			public void Init()
			{
				UInt32 numStates = (UInt32)1 << (m_NumPrevBits + m_NumPosBits);
				for (UInt32 i = 0; i < numStates; i++)
					m_Coders[i].Init();
			}

			UInt32 GetState(UInt32 pos, Byte prevByte)
			{ return ((pos & m_PosMask) << m_NumPrevBits) + (UInt32)(prevByte >> (8 - m_NumPrevBits)); }

			public Byte DecodeNormal(RangeCoder.Decoder rangeDecoder, UInt32 pos, Byte prevByte)
			{ return m_Coders[GetState(pos, prevByte)].DecodeNormal(rangeDecoder); }

			public Byte DecodeWithMatchByte(RangeCoder.Decoder rangeDecoder, UInt32 pos, Byte prevByte, Byte matchByte)
			{ return m_Coders[GetState(pos, prevByte)].DecodeWithMatchByte(rangeDecoder, matchByte); }
		};

		LZ.OutWindow m_OutWindow = new LZ.OutWindow();
		RangeCoder.Decoder m_RangeDecoder = new RangeCoder.Decoder();

		BitDecoder[] m_IsMatchDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];
		BitDecoder[] m_IsRepDecoders = new BitDecoder[Base.kNumStates];
		BitDecoder[] m_IsRepG0Decoders = new BitDecoder[Base.kNumStates];
		BitDecoder[] m_IsRepG1Decoders = new BitDecoder[Base.kNumStates];
		BitDecoder[] m_IsRepG2Decoders = new BitDecoder[Base.kNumStates];
		BitDecoder[] m_IsRep0LongDecoders = new BitDecoder[Base.kNumStates << Base.kNumPosStatesBitsMax];

		BitTreeDecoder[] m_PosSlotDecoder = new BitTreeDecoder[Base.kNumLenToPosStates];
		BitDecoder[] m_PosDecoders = new BitDecoder[Base.kNumFullDistances - Base.kEndPosModelIndex];

		BitTreeDecoder m_PosAlignDecoder = new BitTreeDecoder(Base.kNumAlignBits);

		LenDecoder m_LenDecoder = new LenDecoder();
		LenDecoder m_RepLenDecoder = new LenDecoder();

		LiteralDecoder m_LiteralDecoder = new LiteralDecoder();

		UInt32 m_DictionarySize;
		UInt32 m_DictionarySizeCheck;

		UInt32 m_PosStateMask;

		public Decoder()
		{
			m_DictionarySize = 0xFFFFFFFF;
			for (Int32 i = 0; i < Base.kNumLenToPosStates; i++)
				m_PosSlotDecoder[i] = new BitTreeDecoder(Base.kNumPosSlotBits);
		}

		void SetDictionarySize(UInt32 dictionarySize)
		{
			if (m_DictionarySize != dictionarySize)
			{
				m_DictionarySize = dictionarySize;
				m_DictionarySizeCheck = Math.Max(m_DictionarySize, 1);
				UInt32 blockSize = Math.Max(m_DictionarySizeCheck, (1 << 12));
				m_OutWindow.Create(blockSize);
			}
		}

		void SetLiteralProperties(Int32 lp, Int32 lc)
		{
			if (lp > 8)
				throw new InvalidParamException();
			if (lc > 8)
				throw new InvalidParamException();
			m_LiteralDecoder.Create(lp, lc);
		}

		void SetPosBitsProperties(Int32 pb)
		{
			if (pb > Base.kNumPosStatesBitsMax)
				throw new InvalidParamException();
			UInt32 numPosStates = (UInt32)1 << pb;
			m_LenDecoder.Create(numPosStates);
			m_RepLenDecoder.Create(numPosStates);
			m_PosStateMask = numPosStates - 1;
		}

		Boolean _solid = false;
		void Init(System.IO.Stream inStream, System.IO.Stream outStream)
		{
			m_RangeDecoder.Init(inStream);
			m_OutWindow.Init(outStream, _solid);

			UInt32 i;
			for (i = 0; i < Base.kNumStates; i++)
			{
				for (UInt32 j = 0; j <= m_PosStateMask; j++)
				{
					UInt32 index = (i << Base.kNumPosStatesBitsMax) + j;
					m_IsMatchDecoders[index].Init();
					m_IsRep0LongDecoders[index].Init();
				}
				m_IsRepDecoders[i].Init();
				m_IsRepG0Decoders[i].Init();
				m_IsRepG1Decoders[i].Init();
				m_IsRepG2Decoders[i].Init();
			}

			m_LiteralDecoder.Init();
			for (i = 0; i < Base.kNumLenToPosStates; i++)
				m_PosSlotDecoder[i].Init();
			// m_PosSpecDecoder.Init();
			for (i = 0; i < Base.kNumFullDistances - Base.kEndPosModelIndex; i++)
				m_PosDecoders[i].Init();

			m_LenDecoder.Init();
			m_RepLenDecoder.Init();
			m_PosAlignDecoder.Init();
		}

		public void Code(System.IO.Stream inStream, System.IO.Stream outStream,
			Int64 inSize, Int64 outSize, ICodeProgress progress)
		{
			Init(inStream, outStream);

			Base.State state = new Base.State();
			state.Init();
			UInt32 rep0 = 0, rep1 = 0, rep2 = 0, rep3 = 0;

			UInt64 nowPos64 = 0;
			UInt64 outSize64 = (UInt64)outSize;
			if (nowPos64 < outSize64)
			{
				if (m_IsMatchDecoders[state.Index << Base.kNumPosStatesBitsMax].Decode(m_RangeDecoder) != 0)
					throw new DataErrorException();
				state.UpdateChar();
				Byte b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, 0, 0);
				m_OutWindow.PutByte(b);
				nowPos64++;
			}
			while (nowPos64 < outSize64)
			{
				// UInt64 next = Math.Min(nowPos64 + (1 << 18), outSize64);
					// while(nowPos64 < next)
				{
					UInt32 posState = (UInt32)nowPos64 & m_PosStateMask;
					if (m_IsMatchDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(m_RangeDecoder) == 0)
					{
						Byte b;
						Byte prevByte = m_OutWindow.GetByte(0);
						if (!state.IsCharState())
							b = m_LiteralDecoder.DecodeWithMatchByte(m_RangeDecoder,
								(UInt32)nowPos64, prevByte, m_OutWindow.GetByte(rep0));
						else
							b = m_LiteralDecoder.DecodeNormal(m_RangeDecoder, (UInt32)nowPos64, prevByte);
						m_OutWindow.PutByte(b);
						state.UpdateChar();
						nowPos64++;
					}
					else
					{
						UInt32 len;
						if (m_IsRepDecoders[state.Index].Decode(m_RangeDecoder) == 1)
						{
							if (m_IsRepG0Decoders[state.Index].Decode(m_RangeDecoder) == 0)
							{
								if (m_IsRep0LongDecoders[(state.Index << Base.kNumPosStatesBitsMax) + posState].Decode(m_RangeDecoder) == 0)
								{
									state.UpdateShortRep();
									m_OutWindow.PutByte(m_OutWindow.GetByte(rep0));
									nowPos64++;
									continue;
								}
							}
							else
							{
								UInt32 distance;
								if (m_IsRepG1Decoders[state.Index].Decode(m_RangeDecoder) == 0)
								{
									distance = rep1;
								}
								else
								{
									if (m_IsRepG2Decoders[state.Index].Decode(m_RangeDecoder) == 0)
										distance = rep2;
									else
									{
										distance = rep3;
										rep3 = rep2;
									}
									rep2 = rep1;
								}
								rep1 = rep0;
								rep0 = distance;
							}
							len = m_RepLenDecoder.Decode(m_RangeDecoder, posState) + Base.kMatchMinLen;
							state.UpdateRep();
						}
						else
						{
							rep3 = rep2;
							rep2 = rep1;
							rep1 = rep0;
							len = Base.kMatchMinLen + m_LenDecoder.Decode(m_RangeDecoder, posState);
							state.UpdateMatch();
							UInt32 posSlot = m_PosSlotDecoder[Base.GetLenToPosState(len)].Decode(m_RangeDecoder);
							if (posSlot >= Base.kStartPosModelIndex)
							{
								Int32 numDirectBits = (Int32)((posSlot >> 1) - 1);
								rep0 = ((2 | (posSlot & 1)) << numDirectBits);
								if (posSlot < Base.kEndPosModelIndex)
									rep0 += BitTreeDecoder.ReverseDecode(m_PosDecoders,
											rep0 - posSlot - 1, m_RangeDecoder, numDirectBits);
								else
								{
									rep0 += (m_RangeDecoder.DecodeDirectBits(
										numDirectBits - Base.kNumAlignBits) << Base.kNumAlignBits);
									rep0 += m_PosAlignDecoder.ReverseDecode(m_RangeDecoder);
								}
							}
							else
								rep0 = posSlot;
						}
						if (rep0 >= m_OutWindow.TrainSize + nowPos64 || rep0 >= m_DictionarySizeCheck)
						{
							if (rep0 == 0xFFFFFFFF)
								break;
							throw new DataErrorException();
						}
						m_OutWindow.CopyBlock(rep0, len);
						nowPos64 += len;
					}
				}
			}
			m_OutWindow.Flush();
			m_OutWindow.ReleaseStream();
			m_RangeDecoder.ReleaseStream();
		}

		public void SetDecoderProperties(Byte[] properties)
		{
			if (properties.Length < 5)
				throw new InvalidParamException();
			Int32 lc = properties[0] % 9;
			Int32 remainder = properties[0] / 9;
			Int32 lp = remainder % 5;
			Int32 pb = remainder / 5;
			if (pb > Base.kNumPosStatesBitsMax)
				throw new InvalidParamException();
			UInt32 dictionarySize = 0;
			for (Int32 i = 0; i < 4; i++)
				dictionarySize += ((UInt32)(properties[1 + i])) << (i * 8);
			SetDictionarySize(dictionarySize);
			SetLiteralProperties(lp, lc);
			SetPosBitsProperties(pb);
		}

		public Boolean Train(System.IO.Stream stream)
		{
			_solid = true;
			return m_OutWindow.Train(stream);
		}

		/*
		public override bool CanRead { get { return true; }}
		public override bool CanWrite { get { return true; }}
		public override bool CanSeek { get { return true; }}
		public override long Length { get { return 0; }}
		public override long Position
		{
			get { return 0;	}
			set { }
		}
		public override void Flush() { }
		public override int Read(byte[] buffer, int offset, int count) 
		{
			return 0;
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
		}
		public override long Seek(long offset, System.IO.SeekOrigin origin)
		{
			return 0;
		}
		public override void SetLength(long value) {}
		*/
	}
}
