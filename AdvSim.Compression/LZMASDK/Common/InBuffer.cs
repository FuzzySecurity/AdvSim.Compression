// InBuffer.cs

using System;

namespace SevenZip.Buffer
{
	public class InBuffer
	{
		Byte[] m_Buffer;
		UInt32 m_Pos;
		UInt32 m_Limit;
		UInt32 m_BufferSize;
		System.IO.Stream m_Stream;
		Boolean m_StreamWasExhausted;
		UInt64 m_ProcessedSize;

		public InBuffer(UInt32 bufferSize)
		{
			m_Buffer = new Byte[bufferSize];
			m_BufferSize = bufferSize;
		}

		public void Init(System.IO.Stream stream)
		{
			m_Stream = stream;
			m_ProcessedSize = 0;
			m_Limit = 0;
			m_Pos = 0;
			m_StreamWasExhausted = false;
		}

		public Boolean ReadBlock()
		{
			if (m_StreamWasExhausted)
				return false;
			m_ProcessedSize += m_Pos;
			Int32 aNumProcessedBytes = m_Stream.Read(m_Buffer, 0, (Int32)m_BufferSize);
			m_Pos = 0;
			m_Limit = (UInt32)aNumProcessedBytes;
			m_StreamWasExhausted = (aNumProcessedBytes == 0);
			return (!m_StreamWasExhausted);
		}


		public void ReleaseStream()
		{
			// m_Stream.Close(); 
			m_Stream = null;
		}

		public Boolean ReadByte(Byte b) // check it
		{
			if (m_Pos >= m_Limit)
				if (!ReadBlock())
					return false;
			b = m_Buffer[m_Pos++];
			return true;
		}

		public Byte ReadByte()
		{
			// return (byte)m_Stream.ReadByte();
			if (m_Pos >= m_Limit)
				if (!ReadBlock())
					return 0xFF;
			return m_Buffer[m_Pos++];
		}

		public UInt64 GetProcessedSize()
		{
			return m_ProcessedSize + m_Pos;
		}
	}
}
