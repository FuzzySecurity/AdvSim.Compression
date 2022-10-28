#if NETFRAMEWORK

using System;
using System.Runtime.InteropServices;

namespace AdvSim.Compression
{
    public class LZNT
    {
        // NT Native API's
        //===================================
        
        [DllImport("ntdll.dll")]
        internal static extern UInt32 RtlCompressBuffer(
            COMPRESSION_FLAGS iCompressionFormatAndEngine,
            Byte[] bUncompressedBuffer,
            UInt32 iUncompressedBufferSize,
            Byte[] bCompressedBuffer,
            UInt32 iCompressedBufferSize,
            UInt32 iUncompressedChunkSize,
            ref UInt32 iFinalCompressedSize,
            IntPtr pWorkSpace);
        
        [DllImport("ntdll.dll")]
        internal static extern UInt32 RtlGetCompressionWorkSpaceSize(
            COMPRESSION_FLAGS iCompressionFormatAndEngine,
            ref UInt32 iCompressBufferWorkSpaceSize,
            ref UInt32 iCompressFragmentWorkSpaceSize);
        
        [DllImport("ntdll.dll")]
        internal static extern UInt32 RtlDecompressBuffer(
            COMPRESSION_FLAGS iCompressionFormatAndEngine,
            Byte[] bUncompressedBuffer,
            UInt32 iUncompressedBufferSize,
            Byte[] bCompressedBuffer,
            UInt32 iCompressedBufferSize,
            ref UInt32 iFinalUncompressedSize);

        // Enums
        //===================================
        
        internal enum COMPRESSION_FORMAT : UInt16
        {
            COMPRESSION_FORMAT_NONE = 0x0000,
            COMPRESSION_FORMAT_DEFAULT = 0x0001,
            COMPRESSION_FORMAT_LZNT1 = 0x0002
        }
        
        internal enum COMPRESSION_ENGINE : UInt16
        {
            COMPRESSION_ENGINE_STANDARD = 0x0000,
            COMPRESSION_ENGINE_MAXIMUM = 0x0100,
            COMPRESSION_ENGINE_HIBER = 0x0200
        }
        
        internal enum COMPRESSION_FLAGS : UInt16
        {
            COMPRESSION_FORMAT_LZNT1_STD = COMPRESSION_FORMAT.COMPRESSION_FORMAT_LZNT1 | COMPRESSION_ENGINE.COMPRESSION_ENGINE_STANDARD,
            COMPRESSION_FORMAT_LZNT1_MAX = COMPRESSION_FORMAT.COMPRESSION_FORMAT_LZNT1 | COMPRESSION_ENGINE.COMPRESSION_ENGINE_MAXIMUM,
            // Not supported, can be made to work on Ex version
            COMPRESSION_FORMAT_LZNT1_XPRESS_HUFF_HIBER = COMPRESSION_FORMAT.COMPRESSION_FORMAT_LZNT1 | COMPRESSION_ENGINE.COMPRESSION_ENGINE_HIBER
        }

        // Methods
        //===================================
        
        /// <summary>
        /// Use LZNT1 to compress a byte array to a byte array
        /// </summary>
        /// <param name="bUncompressedBuffer">Byte array containing the uncompressed buffer</param>
        /// <returns>Compressed Byte array</returns>
        public static Byte[] RtlCompressBuffer(Byte[] bUncompressedBuffer)
        {
            // Set standard compression format and engine
            COMPRESSION_FLAGS iCompressionFormatAndEngine = COMPRESSION_FLAGS.COMPRESSION_FORMAT_LZNT1_STD;
            
            // Get the size of the compression workspace.
            UInt32 iCompressBufferWorkSpaceSize = 0;
            UInt32 iCompressFragmentWorkSpaceSize = 0;
            RtlGetCompressionWorkSpaceSize(
                iCompressionFormatAndEngine,
                ref iCompressBufferWorkSpaceSize,
                ref iCompressFragmentWorkSpaceSize);
            
            // "Allocate" the compression workspace.
            Byte[] bCompressBufferWorkSpace = new Byte[iCompressBufferWorkSpaceSize];
            
            // "Allocate" the compressed buffer. We just add a page to the uncompressed buffer size.
            // |_ We will resize the array later.
            Byte[] bCompressedBuffer = new Byte[bUncompressedBuffer.Length +  0x1000];
            
            // Compress the buffer.
            UInt32 iFinalCompressedSize = 0;
            RtlCompressBuffer(
                iCompressionFormatAndEngine,
                bUncompressedBuffer,
                (UInt32)bUncompressedBuffer.Length,
                bCompressedBuffer,
                (UInt32)bCompressedBuffer.Length,
                0,
                ref iFinalCompressedSize,
                Marshal.UnsafeAddrOfPinnedArrayElement(bCompressBufferWorkSpace, 0));

            // Resize the compressed buffer to the actual size.
            Array.Resize(ref bCompressedBuffer, (Int32)iFinalCompressedSize);
            
            // Create return buffer -> (UInt32) original size + compressed buffer.
            Byte[] bReturnBuffer = new Byte[bCompressedBuffer.Length + 4];
            
            // Copy the original size to the return buffer.
            Array.Copy(BitConverter.GetBytes(bUncompressedBuffer.Length), 0, bReturnBuffer, 0, 4);
            
            // Copy the compressed buffer to the return buffer.
            Array.Copy(bCompressedBuffer, 0, bReturnBuffer, 4, bCompressedBuffer.Length);
            
            // Return the compressed buffer.
            return bReturnBuffer;
        }
        
        /// <summary>
        /// Use LZNT1 to compress a file to a byte array
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be compressed</param>
        /// <returns>Compressed Byte array</returns>
        public static Byte[] RtlCompressBuffer(String sFilePath)
        {
            // Read the file to a byte array.
            Byte[] bUncompressedBuffer = System.IO.File.ReadAllBytes(sFilePath);
            
            // Compress the byte array.
            return RtlCompressBuffer(bUncompressedBuffer);
        }
        
        /// <summary>
        /// Use LZNT1 to compress a byte array to a file
        /// </summary>
        /// <param name="bUncompressedBuffer">Byte array containing the uncompressed buffer</param>
        /// <param name="sFilePath">Full path to the file on disk where the compressed data will be written</param>
        public static void RtlCompressBuffer(Byte[] bUncompressedBuffer, String sFilePath)
        {
            // Compress the byte array.
            Byte[] bCompressedBuffer = RtlCompressBuffer(bUncompressedBuffer);
            
            // Write the compressed byte array to a file.
            System.IO.File.WriteAllBytes(sFilePath, bCompressedBuffer);
        }

        /// <summary>
        /// Use LZNT1 to compress a file to a file
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be compressed</param>
        /// <param name="sOutputFilePath">Full path to the file on disk where the compressed data will be written</param>
        public static void RtlCompressBuffer(String sFilePath, String sOutputFilePath)
        {
            // Compress the file to a byte array.
            Byte[] bCompressedBuffer = RtlCompressBuffer(sFilePath);
            
            // Write the byte array to a file.
            System.IO.File.WriteAllBytes(sOutputFilePath, bCompressedBuffer);
        }
        
        /// <summary>
        /// Use LZNT1 to decompress a byte array to a byte array
        /// </summary>
        /// <param name="bCompressedBuffer">Byte array containing the compressed buffer</param>
        /// <returns>Decompressed Byte array</returns>
        public static Byte[] RtlDecompressBuffer(Byte[] bCompressedBuffer)
        {
            // Set standard compression format and engine
            COMPRESSION_FLAGS iCompressionFormatAndEngine = COMPRESSION_FLAGS.COMPRESSION_FORMAT_LZNT1_STD;
            
            // Get the original size of the uncompressed buffer.
            Byte[] bOriginalSize = new Byte[4];
            Array.Copy(bCompressedBuffer, bOriginalSize, 4);

            // Allocate the uncompressed buffer.
            Byte[] bUncompressedBuffer = new Byte[BitConverter.ToUInt32(bOriginalSize, 0)];
            
            // Remove the original size from the compressed buffer.
            Byte[] bCompressedBufferWithoutOriginalSize = new Byte[bCompressedBuffer.Length - 4];
            Array.Copy(bCompressedBuffer, 4, bCompressedBufferWithoutOriginalSize, 0, bCompressedBufferWithoutOriginalSize.Length);
            
            // Decompress the buffer.
            UInt32 iFinalUncompressedSize = 0;
            RtlDecompressBuffer(
                iCompressionFormatAndEngine,
                bUncompressedBuffer,
                (UInt32)bUncompressedBuffer.Length,
                bCompressedBufferWithoutOriginalSize,
                (UInt32)bCompressedBufferWithoutOriginalSize.Length,
                ref iFinalUncompressedSize);
            
            // Return the uncompressed buffer.
            return bUncompressedBuffer;
        }
        
        /// <summary>
        /// Use LZNT1 to decompress a file to a byte array
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be decompressed</param>
        /// <returns>Decompressed Byte array</returns>
        public static Byte[] RtlDecompressBuffer(String sFilePath)
        {
            // Read the file to a byte array.
            Byte[] bCompressedBuffer = System.IO.File.ReadAllBytes(sFilePath);
            
            // Decompress the byte array.
            return RtlDecompressBuffer(bCompressedBuffer);
        }
        
        /// <summary>
        /// Use LZNT1 to decompress a byte array to a file
        /// </summary>
        /// <param name="bCompressedBuffer">Byte array containing the compressed buffer</param>
        /// <param name="sFilePath">Full path to the file on disk where the decompressed data will be written</param>
        public static void RtlDecompressBuffer(Byte[] bCompressedBuffer, String sFilePath)
        {
            // Decompress the byte array.
            Byte[] bUncompressedBuffer = RtlDecompressBuffer(bCompressedBuffer);
            
            // Write the uncompressed byte array to a file.
            System.IO.File.WriteAllBytes(sFilePath, bUncompressedBuffer);
        }
        
        /// <summary>
        /// Use LZNT1 to decompress a file to a file
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be decompressed</param>
        /// <param name="sOutputFilePath">Full path to the file on disk where the decompressed data will be written</param>
        public static void RtlDecompressBuffer(String sFilePath, String sOutputFilePath)
        {
            // Decompress the file to a byte array.
            Byte[] bUncompressedBuffer = RtlDecompressBuffer(sFilePath);
            
            // Write the byte array to a file.
            System.IO.File.WriteAllBytes(sOutputFilePath, bUncompressedBuffer);
        }
    }
}

#endif