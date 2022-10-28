#if NETFRAMEWORK

using System;
using System.Runtime.InteropServices;

namespace AdvSim.Compression
{
    public class Cabinet
    {
        // Cabinet.dll API's
        //===================================
        
        [DllImport("Cabinet.dll")]
        internal static extern Boolean CreateCompressor(
            CompressionAlgorithm iAlgorithm,
            IntPtr pAllocationRoutines,
            ref IntPtr hCompressorHandle);
        
        [DllImport("Cabinet.dll")]
        internal static extern Boolean CreateDecompressor(
            CompressionAlgorithm iAlgorithm,
            IntPtr pAllocationRoutines,
            ref IntPtr hDecompressorHandle);

        [DllImport("Cabinet.dll", SetLastError = true)]
        internal static extern Boolean Compress(
            IntPtr hCompressorHandle,
            Byte[] bUncompressedData,
            UInt32 iUncompressedDataSize,
            Byte[] bCompressedBuffer,
            UInt32 iCompressedBufferSize,
            ref UInt32 iCompressedDataSize);
        
        [DllImport("Cabinet.dll")]
        internal static extern Boolean Decompress(
            IntPtr hDecompressorHandle,
            Byte[] bCompressedData,
            UInt32 iCompressedDataSize,
            Byte[] bUncompressedBuffer,
            UInt32 iUncompressedBufferSize,
            ref UInt32 iUncompressedDataSize);
        
        [DllImport("Cabinet.dll")]
        internal static extern Boolean CloseCompressor(
            IntPtr hCompressHandle);
        
        [DllImport("Cabinet.dll")]
        internal static extern Boolean CloseDecompressor(
            IntPtr hCompressHandle);

        // Cabinet Enums
        //===================================
        
        /// <summary>
        /// Enum defining supported Windows Storage compression types
        /// </summary>
        public enum CompressionAlgorithm : UInt32
        {
            COMPRESS_ALGORITHM_MSZIP = 2,
            COMPRESS_ALGORITHM_XPRESS = 3,
            COMPRESS_ALGORITHM_XPRESS_HUFF = 4,
            COMPRESS_ALGORITHM_LZMS = 5
        }
        
        // Methods
        //===================================

        /// <summary>
        /// Use native Windows.Storage.Compression (Cabinet.dll) to compress a byte array to a byte array
        /// </summary>
        /// <param name="bUncompressedBuffer">Byte array containing the uncompressed buffer</param>
        /// <param name="eAlgorithm">Type of compressor to create (CompressionAlgorithm)</param>
        /// <returns>Compressed Byte array</returns>
        public static Byte[] CompressStorage(Byte[] bUncompressedBuffer, CompressionAlgorithm eAlgorithm)
        {
            // Create a new compressor
            IntPtr hCompressorHandle = IntPtr.Zero;
            CreateCompressor(
                eAlgorithm,
                IntPtr.Zero,
                ref hCompressorHandle);

            // Find the correct compression buffer size
            UInt32 iCompressedDataSize = 0;
            Compress(
                hCompressorHandle,
                bUncompressedBuffer,
                (UInt32)bUncompressedBuffer.Length,
                null,
                0,
                ref iCompressedDataSize);
            
            // Create the compressed buffer
            Byte[] bCompressedBuffer = new Byte[iCompressedDataSize];
            
            // Compress the data
            Compress(
                hCompressorHandle,
                bUncompressedBuffer,
                (UInt32)bUncompressedBuffer.Length,
                bCompressedBuffer,
                (UInt32)bCompressedBuffer.Length,
                ref iCompressedDataSize);

            // Close the compressor
            CloseCompressor(hCompressorHandle);

            // Return the compressed data
            return bCompressedBuffer;
        }

        /// <summary>
        /// Use native Windows.Storage.Compression (Cabinet.dll) to compress a file to a byte array
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be compressed</param>
        /// <param name="eAlgorithm">Type of compressor to create (CompressionAlgorithm)</param>
        /// <returns>Compressed Byte array</returns>
        public static Byte[] CompressStorage(String sFilePath, CompressionAlgorithm eAlgorithm)
        {
            // Read the file into a byte array
            Byte[] bUncompressedBuffer = System.IO.File.ReadAllBytes(sFilePath);

            // Compress the byte array
            return CompressStorage(bUncompressedBuffer, eAlgorithm);
        }
        
        /// <summary>
        /// Use native Windows.Storage.Compression (Cabinet.dll) to compress a byte array to a file
        /// </summary>
        /// <param name="bUncompressedBuffer">Byte array containing the uncompressed buffer</param>
        /// <param name="sFilePath">Full path to the file on disk where the compressed data will be written</param>
        /// <param name="eAlgorithm">Type of compressor to create (CompressionAlgorithm)</param>
        public static void CompressStorage(Byte[] bUncompressedBuffer, String sFilePath, CompressionAlgorithm eAlgorithm)
        {
            // Compress the byte array
            Byte[] bCompressedBuffer = CompressStorage(bUncompressedBuffer, eAlgorithm);

            // Write the compressed data to disk
            System.IO.File.WriteAllBytes(sFilePath, bCompressedBuffer);
        }
        
        /// <summary>
        /// Use native Windows.Storage.Compression (Cabinet.dll) to compress a file to a file
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be compressed</param>
        /// <param name="sOutputFilePath">Full path to the file on disk where the compressed data will be written</param>
        /// <param name="eAlgorithm">Type of compressor to create (CompressionAlgorithm)</param>
        public static void CompressStorage(String sFilePath, String sOutputFilePath, CompressionAlgorithm eAlgorithm)
        {
            // Read the file into a byte array
            Byte[] bUncompressedBuffer = System.IO.File.ReadAllBytes(sFilePath);

            // Compress the byte array
            CompressStorage(bUncompressedBuffer, sOutputFilePath, eAlgorithm);
        }

        /// <summary>
        /// Use native Windows.Storage.Compression (Cabinet.dll) to decompress a byte array to a byte array
        /// </summary>
        /// <param name="bCompressedBuffer">Byte array containing the compressed buffer</param>
        /// <param name="eAlgorithm">Type of decompressor to create (CompressionAlgorithm)</param>
        /// <returns>Decompressed Byte array</returns>
        public static Byte[] DecompressStorage(Byte[] bCompressedBuffer, CompressionAlgorithm eAlgorithm)
        {
            // Create a new decompressor
            IntPtr hDecompressorHandle = IntPtr.Zero;
            CreateDecompressor(
                eAlgorithm,
                IntPtr.Zero,
                ref hDecompressorHandle);

            // Find the correct uncompressed size
            UInt32 iUncompressedDataSize = 0;
            Decompress(
                hDecompressorHandle,
                bCompressedBuffer,
                (UInt32)bCompressedBuffer.Length,
                null,
                0,
                ref iUncompressedDataSize);

            // Create the uncompressed buffer
            Byte[] bUncompressedBuffer = new Byte[iUncompressedDataSize];

            // Decompress the data
            Decompress(
                hDecompressorHandle,
                bCompressedBuffer,
                (UInt32)bCompressedBuffer.Length,
                bUncompressedBuffer,
                (UInt32)bUncompressedBuffer.Length,
                ref iUncompressedDataSize);

            // Close the decompressor
            CloseDecompressor(hDecompressorHandle);
            
            // Return the uncompressed data
            return bUncompressedBuffer;
        }
        
        /// <summary>
        /// Use native Windows.Storage.Compression (Cabinet.dll) to decompress a file to a byte array
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be decompressed</param>
        /// <param name="eAlgorithm">Type of decompressor to create (CompressionAlgorithm)</param>
        /// <returns>Decompressed Byte array</returns>
        public static Byte[] DecompressStorage(String sFilePath, CompressionAlgorithm eAlgorithm)
        {
            // Read the file into a byte array
            Byte[] bCompressedBuffer = System.IO.File.ReadAllBytes(sFilePath);

            // Decompress the byte array
            return DecompressStorage(bCompressedBuffer, eAlgorithm);
        }
        
        /// <summary>
        /// Use native Windows.Storage.Compression (Cabinet.dll) to decompress a byte array to a file
        /// </summary>
        /// <param name="bCompressedBuffer">Byte array containing the compressed buffer</param>
        /// <param name="sFilePath">Full path to the file on disk where the decompressed data will be written</param>
        /// <param name="eAlgorithm">Type of decompressor to create (CompressionAlgorithm)</param>
        public static void DecompressStorage(Byte[] bCompressedBuffer, String sFilePath, CompressionAlgorithm eAlgorithm)
        {
            // Decompress the byte array
            Byte[] bUncompressedBuffer = DecompressStorage(bCompressedBuffer, eAlgorithm);

            // Write the uncompressed data to disk
            System.IO.File.WriteAllBytes(sFilePath, bUncompressedBuffer);
        }
        
        /// <summary>
        /// Use native Windows.Storage.Compression (Cabinet.dll) to decompress a file to a file
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be decompressed</param>
        /// <param name="sOutputFilePath">Full path to the file on disk where the decompressed data will be written</param>
        /// <param name="eAlgorithm">Type of decompressor to create (CompressionAlgorithm)</param>
        public static void DecompressStorage(String sFilePath, String sOutputFilePath, CompressionAlgorithm eAlgorithm)
        {
            // Read the file into a byte array
            Byte[] bCompressedBuffer = System.IO.File.ReadAllBytes(sFilePath);

            // Decompress the byte array
            DecompressStorage(bCompressedBuffer, sOutputFilePath, eAlgorithm);
        }
    }
}

#endif