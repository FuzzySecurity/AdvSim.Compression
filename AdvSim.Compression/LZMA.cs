using System;
using System.IO;

namespace AdvSim.Compression
{
    public class LZMA
    {
        /// <summary>
        /// Use LZMA to compress a byte array to a byte array
        /// </summary>
        /// <param name="bUncompressedBuffer">Byte array containing the uncompressed buffer</param>
        /// <returns>Compressed Byte array</returns>
        public static Byte[] LZMACompress(Byte[] bUncompressedBuffer)
        {
            using (MemoryStream ms = new MemoryStream(bUncompressedBuffer))
            {
                using (MemoryStream msLZMA = new MemoryStream())
                {
                    SevenZip.Compression.LZMA.Encoder encoder = new SevenZip.Compression.LZMA.Encoder();
                    encoder.WriteCoderProperties(msLZMA);
                    msLZMA.Write(BitConverter.GetBytes(ms.Length), 0, 8);
                    encoder.Code(ms, msLZMA, ms.Length, -1, null);
                    
                    return msLZMA.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Use LZMA to compress a file to a byte array
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be compressed</param>
        /// <returns>Compressed Byte array</returns>
        public static Byte[] LZMACompress(String sFilePath)
        {
            // Read file to byte array
            Byte[] bUncompressedBuffer = File.ReadAllBytes(sFilePath);
            
            // Compress byte array
            return LZMACompress(bUncompressedBuffer);
        }
        
        /// <summary>
        /// Use LZMA to compress a byte array to a file
        /// </summary>
        /// <param name="bUncompressedBuffer">Byte array containing the uncompressed buffer</param>
        /// <param name="sFilePath">Full path to the file on disk where the compressed data will be written</param>
        public static void LZMACompress(Byte[] bUncompressedBuffer, String sFilePath)
        {
            // Compress byte array
            Byte[] bCompressedBuffer = LZMACompress(bUncompressedBuffer);
            
            // Write byte array to file
            File.WriteAllBytes(sFilePath, bCompressedBuffer);
        }

        /// <summary>
        /// Use LZMA to compress a file to a file
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be compressed</param>
        /// <param name="sOutputFilePath">Full path to the file on disk where the compressed data will be written</param>
        public static void LZMACompress(String sFilePath, String sOutputFilePath)
        {
            // Compress byte array
            Byte[] bCompressedBuffer = LZMACompress(sFilePath);
            
            // Write byte array to file
            File.WriteAllBytes(sOutputFilePath, bCompressedBuffer);
        }

        /// <summary>
        /// Use LZMA to decompress a byte array to a byte array
        /// </summary>
        /// <param name="bCompressedBuffer">Byte array containing the compressed buffer</param>
        /// <returns>Decompressed Byte array</returns>
        public static Byte[] LZMADecompress(Byte[] bCompressedBuffer)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (MemoryStream msLZMA = new MemoryStream(bCompressedBuffer))
                {
                    SevenZip.Compression.LZMA.Decoder decoder = new SevenZip.Compression.LZMA.Decoder();
                    Byte[] properties = new Byte[5];
                    msLZMA.Read(properties, 0, 5);
                    decoder.SetDecoderProperties(properties);
                    
                    Byte[] fileLengthBytes = new Byte[8];
                    msLZMA.Read(fileLengthBytes, 0, 8);
                    Int64 fileLength = BitConverter.ToInt64(fileLengthBytes, 0);
                    
                    decoder.Code(msLZMA, ms, msLZMA.Length, fileLength, null);
                    return ms.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Use LZMA to decompress a file to a byte array
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be decompressed</param>
        /// <returns>Decompressed Byte array</returns>
        public static Byte[] LZMADecompress(String sFilePath)
        {
            // Read file to byte array
            Byte[] bCompressedBuffer = File.ReadAllBytes(sFilePath);
            
            // Decompress byte array
            return LZMADecompress(bCompressedBuffer);
        }
        
        /// <summary>
        /// Use LZMA to decompress a byte array to a file
        /// </summary>
        /// <param name="bCompressedBuffer">Byte array containing the compressed buffer</param>
        /// <param name="sFilePath">Full path to the file on disk where the decompressed data will be written</param>
        public static void LZMADecompress(Byte[] bCompressedBuffer, String sFilePath)
        {
            // Decompress byte array
            Byte[] bUncompressedBuffer = LZMADecompress(bCompressedBuffer);
            
            // Write byte array to file
            File.WriteAllBytes(sFilePath, bUncompressedBuffer);
        }

        /// <summary>
        /// Use LZMA to decompress a file to a file
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be decompressed</param>
        /// <param name="sOutputFilePath">Full path to the file on disk where the decompressed data will be written</param>
        public static void LZMADecompress(String sFilePath, String sOutputFilePath)
        {
            // Decompress byte array
            Byte[] bUncompressedBuffer = LZMADecompress(sFilePath);
            
            // Write byte array to file
            File.WriteAllBytes(sOutputFilePath, bUncompressedBuffer);
        }
    }
}