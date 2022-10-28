// OutBuffer.cs

using System;

namespace SevenZip.Buffer
{
	public class OutBuffer
	{
		Byte[] m_Buffer;
		UInt32 m_Pos;
		UInt32 m_BufferSize;
		System.IO.Stream m_Stream;
		UInt64 m_ProcessedSize;

		public OutBuffer(UInt32 bufferSize)
		{
			m_Buffer = new Byte[bufferSize];
			m_BufferSize = bufferSize;
		}

		public void SetStream(System.IO.Stream stream) { m_Stream = stream; }
		public void FlushStream() { m_Stream.Flush(); }
		public void CloseStream() { m_Stream.Close(); }
		public void ReleaseStream() { m_Stream = null; }

		public void Init()
		{
			m_ProcessedSize = 0;
			m_Pos = 0;
		}

		public void WriteByte(Byte b)
		{
			m_Buffer[m_Pos++] = b;
			if (m_Pos >= m_BufferSize)
				FlushData();
		}

		public void FlushData()
		{
			if (m_Pos == 0)
				return;
			m_Stream.Write(m_Buffer, 0, (Int32)m_Pos);
			m_Pos = 0;
		}

		public UInt64 GetProcessedSize() { return m_ProcessedSize + m_Pos; }
	}
}
