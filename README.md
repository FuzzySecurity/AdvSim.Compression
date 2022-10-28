
![Release](https://badgen.net/badge/AdvSim.Compression/v1.0.0/cyan?icon=github)
# AdvSim.Compression

The `AdvSim.Compression` NuGet contains a set of compression wrapper functions which are reusable, configured with sane defaults and are easy to use. Further details are available under the different subheadings below.

- [GZip](#gzip)
- [LZMA](#lzma)
- [LZNT1](#lznt1)
- [Direct3D](#direct3d)
- [Cabinet](#cabinet)

#### NuGet Compatibility

The `AdvSim.Compression` NuGet supports a wide variety of .Net versions. Due to native API usage, a number of the modules are available only on `NETFRAMEWORK`, these have been highlighted in the usage documentation below.

**NuGet URL**: https://www.nuget.org/packages/AdvSim.Compression

# Usage

### GZip

![Availability](https://badgen.net/badge/Availability/All/green)

`GZip` is a standard wrapper around `System.IO.Compression`.

```c#
// Compression
//====================

// Compress Byte[] to Byte[]
Byte[] bCompressed = GZip.GzipCompress(bUncompressedBuffer);
// Compress File to Byte[]
Byte[] bCompressed = GZip.GzipCompress(sFilePath);
// Compress Byte[] to File
GZip.GzipCompress(bUncompressedBuffer, sFilePath);
// Compress File to File
GZip.GzipCompress(sFilePath, sOutputFilePath);

// Decompression
//====================

// Decompress Byte[] to Byte[]
Byte[] bDecompressed = GZip.GzipDecompress(bCompressedBuffer);
// Decompress File to Byte[]
Byte[] bDecompressed = GZip.GzipDecompress(sFilePath);
// Decompress Byte[] to File
GZip.GzipDecompress(bUncompressedBuffer, sFilePath);
// Decompress File to File
GZip.GzipDecompress(sFilePath, sOutputFilePath);
```

### LZMA

![Availability](https://badgen.net/badge/Availability/All/green)

`LZMA` is a wrapper built on the `7z` sdk provided [here](https://7-zip.org/sdk.html) under public domain licencing.

```c#
// Compression
//====================

// Compress Byte[] to Byte[]
Byte[] bCompressed = LZMA.LZMACompress(bUncompressedBuffer);
// Compress File to Byte[]
Byte[] bCompressed = LZMA.LZMACompress(sFilePath);
// Compress Byte[] to File
LZMA.LZMACompress(bUncompressedBuffer, sFilePath);
// Compress File to File
LZMA.LZMACompress(sFilePath, sOutputFilePath);

// Decompression
//====================

// Decompress Byte[] to Byte[]
Byte[] bDecompressed = LZMA.LZMACompress(bCompressedBuffer);
// Decompress File to Byte[]
Byte[] bDecompressed = LZMA.LZMACompress(sFilePath);
// Decompress Byte[] to File
LZMA.LZMACompress(bUncompressedBuffer, sFilePath);
// Decompress File to File
LZMA.LZMACompress(sFilePath, sOutputFilePath);
```

### LZNT1

![Availability](https://badgen.net/badge/Availability/NETFRAMEWORK/green)

`LZNT` is a wrapper around `ntdll!RtlCompressBuffer` and `ntdll!RtlDecompressBuffer`. More details about `LZNT1` are available [here](https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-xca/5655f4a3-6ba4-489b-959f-e1f407c52f15). The compression workspace here is using the `Standard Engine`.

```c#
// Compression
//====================

// Compress Byte[] to Byte[]
Byte[] bCompressed = LZNT.RtlCompressBuffer(bUncompressedBuffer);
// Compress File to Byte[]
Byte[] bCompressed = LZNT.RtlCompressBuffer(sFilePath);
// Compress Byte[] to File
LZNT.RtlCompressBuffer(bUncompressedBuffer, sFilePath);
// Compress File to File
LZNT.RtlCompressBuffer(sFilePath, sOutputFilePath);

// Decompression
//====================

// Decompress Byte[] to Byte[]
Byte[] bDecompressed = LZNT.RtlDecompressBuffer(bCompressedBuffer);
// Decompress File to Byte[]
Byte[] bDecompressed = LZNT.RtlDecompressBuffer(sFilePath);
// Decompress Byte[] to File
LZNT.RtlDecompressBuffer(bUncompressedBuffer, sFilePath);
// Decompress File to File
LZNT.RtlDecompressBuffer(sFilePath, sOutputFilePath);
```

### Direct3D

![Availability](https://badgen.net/badge/Availability/NETFRAMEWORK/green)

`Direct3D` is using shader compression from `D3DCompiler_47!D3DCompressShaders` and `D3DCompiler_47!D3DDecompressShaders`. Compression here is really good and I want to give a shout-out to [@modexpblog](https://twitter.com/modexpblog) for documenting this [here](https://modexp.wordpress.com/2019/12/08/shellcode-compression/) and helping me do some debugging <3!

```c#
// Compression
//====================

// Compress Byte[] to Byte[]
Byte[] bCompressed = Direct3D.D3DCompressShaders(bUncompressedBuffer);
// Compress File to Byte[]
Byte[] bCompressed = Direct3D.D3DCompressShaders(sFilePath);
// Compress Byte[] to File
Direct3D.D3DCompressShaders(bUncompressedBuffer, sFilePath);
// Compress File to File
Direct3D.D3DCompressShaders(sFilePath, sOutputFilePath);

// Decompression
//====================

// Decompress Byte[] to Byte[]
Byte[] bDecompressed = Direct3D.D3DDecompressShaders(bCompressedBuffer);
// Decompress File to Byte[]
Byte[] bDecompressed = Direct3D.D3DDecompressShaders(sFilePath);
// Decompress Byte[] to File
Direct3D.D3DDecompressShaders(bUncompressedBuffer, sFilePath);
// Decompress File to File
Direct3D.D3DDecompressShaders(sFilePath, sOutputFilePath);
```

### Cabinet

![Availability](https://badgen.net/badge/Availability/NETFRAMEWORK/green)

`Cabinet` is built mainly around `cabinet!Compress` and `cabinet!Decompress`. These functions are actually exposed through managed code as well, within [Windows.Storage.Compression](https://learn.microsoft.com/en-us/uwp/api/windows.storage.compression?view=winrt-22621), however this namespace is tied to the Universal Windows Platform (UWP) making it not practicable.

The `cabinet` wrapper functions are able to use various types of compression. The `Enum` below lists the supported compression types.

```c#
public enum CompressionAlgorithm : UInt32  
{  
  COMPRESS_ALGORITHM_MSZIP = 2,  
  COMPRESS_ALGORITHM_XPRESS = 3,  
  COMPRESS_ALGORITHM_XPRESS_HUFF = 4,  
  COMPRESS_ALGORITHM_LZMS = 5  
}
```

Usage is very similar to the other wrappers in this library.

```c#
// Compression
//====================

// Compress Byte[] to Byte[]
Byte[] bCompressed = Cabinet.CompressStorage(bUncompressedBuffer, eAlgorithm);
// Compress File to Byte[]
Byte[] bCompressed = Cabinet.CompressStorage(sFilePath, eAlgorithm);
// Compress Byte[] to File
Cabinet.CompressStorage(bUncompressedBuffer, sFilePath, eAlgorithm);
// Compress File to File
Cabinet.CompressStorage(sFilePath, sOutputFilePath, eAlgorithm);

// Decompression
//====================

// Decompress Byte[] to Byte[]
Byte[] bDecompressed = Cabinet.DecompressStorage(bCompressedBuffer, eAlgorithm);
// Decompress File to Byte[]
Byte[] bDecompressed = Cabinet.DecompressStorage(sFilePath, eAlgorithm);
// Decompress Byte[] to File
Cabinet.DecompressStorage(bUncompressedBuffer, sFilePath, eAlgorithm);
// Decompress File to File
Cabinet.DecompressStorage(sFilePath, sOutputFilePath, eAlgorithm);
```
