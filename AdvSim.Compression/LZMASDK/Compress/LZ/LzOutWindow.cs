// LzOutWindow.cs

using System;

namespace SevenZip.Compression.LZ
{
	public class OutWindow
	{
		Byte[] _buffer = null;
		UInt32 _pos;
		UInt32 _windowSize = 0;
		UInt32 _streamPos;
		System.IO.Stream _stream;

		public UInt32 TrainSize = 0;

		public void Create(UInt32 windowSize)
		{
			if (_windowSize != windowSize)
			{
				// System.GC.Collect();
				_buffer = new Byte[windowSize];
			}
			_windowSize = windowSize;
			_pos = 0;
			_streamPos = 0;
		}

		public void Init(System.IO.Stream stream, Boolean solid)
		{
			ReleaseStream();
			_stream = stream;
			if (!solid)
			{
				_streamPos = 0;
				_pos = 0;
				TrainSize = 0;
			}
		}
	
		public Boolean Train(System.IO.Stream stream)
		{
			Int64 len = stream.Length;
			UInt32 size = (len < _windowSize) ? (UInt32)len : _windowSize;
			TrainSize = size;
			stream.Position = len - size;
			_streamPos = _pos = 0;
			while (size > 0)
			{
				UInt32 curSize = _windowSize - _pos;
				if (size < curSize)
					curSize = size;
				Int32 numReadBytes = stream.Read(_buffer, (Int32)_pos, (Int32)curSize);
				if (numReadBytes == 0)
					return false;
				size -= (UInt32)numReadBytes;
				_pos += (UInt32)numReadBytes;
				_streamPos += (UInt32)numReadBytes;
				if (_pos == _windowSize)
					_streamPos = _pos = 0;
			}
			return true;
		}

		public void ReleaseStream()
		{
			Flush();
			_stream = null;
		}

		public void Flush()
		{
			UInt32 size = _pos - _streamPos;
			if (size == 0)
				return;
			_stream.Write(_buffer, (Int32)_streamPos, (Int32)size);
			if (_pos >= _windowSize)
				_pos = 0;
			_streamPos = _pos;
		}

		public void CopyBlock(UInt32 distance, UInt32 len)
		{
			UInt32 pos = _pos - distance - 1;
			if (pos >= _windowSize)
				pos += _windowSize;
			for (; len > 0; len--)
			{
				if (pos >= _windowSize)
					pos = 0;
				_buffer[_pos++] = _buffer[pos++];
				if (_pos >= _windowSize)
					Flush();
			}
		}

		public void PutByte(Byte b)
		{
			_buffer[_pos++] = b;
			if (_pos >= _windowSize)
				Flush();
		}

		public Byte GetByte(UInt32 distance)
		{
			UInt32 pos = _pos - distance - 1;
			if (pos >= _windowSize)
				pos += _windowSize;
			return _buffer[pos];
		}
	}
}
