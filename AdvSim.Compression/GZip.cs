using System;
using System.IO;
using System.IO.Compression;

namespace AdvSim.Compression
{
    public class GZip
    {
        /// <summary>
        /// Use Gzip to compress a byte array to a byte array
        /// </summary>
        /// <param name="bUncompressedBuffer">Byte array containing the uncompressed buffer</param>
        /// <returns>Compressed Byte array</returns>
        public static Byte[] GzipCompress(Byte[] bUncompressedBuffer)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                using (GZipStream gzip = new GZipStream(ms, CompressionMode.Compress, true))
                {
                    gzip.Write(bUncompressedBuffer, 0, bUncompressedBuffer.Length);
                }
                return ms.ToArray();
            }
        }
        
        /// <summary>
        /// Use Gzip to compress a file to a byte array
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be compressed</param>
        /// <returns>Compressed Byte array</returns>
        public static Byte[] GzipCompress(String sFilePath)
        {
            // Read file to byte array
            Byte[] bUncompressedBuffer = File.ReadAllBytes(sFilePath);
            
            // Compress byte array
            return GzipCompress(bUncompressedBuffer);
        }
        
        /// <summary>
        /// Use Gzip to compress a byte array to a file
        /// </summary>
        /// <param name="bUncompressedBuffer">Byte array containing the uncompressed buffer</param>
        /// <param name="sFilePath">Full path to the file on disk where the compressed data will be written</param>
        public static void GzipCompress(Byte[] bUncompressedBuffer, String sFilePath)
        {
            // Compress byte array
            Byte[] bCompressedBuffer = GzipCompress(bUncompressedBuffer);
            
            // Write compressed byte array to file
            File.WriteAllBytes(sFilePath, bCompressedBuffer);
        }
        
        /// <summary>
        /// Use Gzip to compress a file to a file
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be compressed</param>
        /// <param name="sOutputFilePath">Full path to the file on disk where the compressed data will be written</param>
        public static void GzipCompress(String sFilePath, String sOutputFilePath)
        {
            // Compress byte array
            Byte[] bCompressedBuffer = GzipCompress(sFilePath);
            
            // Write compressed byte array to file
            File.WriteAllBytes(sOutputFilePath, bCompressedBuffer);
        }

        /// <summary>
        /// Use GZip to decompress a byte array to a byte array
        /// </summary>
        /// <param name="bCompressedBuffer">Byte array containing the compressed buffer</param>
        /// <returns>Decompressed Byte array</returns>
        public static Byte[] GzipDecompress(Byte[] bCompressedBuffer)
        {
            using (MemoryStream src = new MemoryStream(bCompressedBuffer))
            {
                using (MemoryStream dst = new MemoryStream())
                {
                    using (GZipStream gZipStream = new GZipStream(src, CompressionMode.Decompress))
                    {
                        Byte[] buffer = new Byte[4096];
                        Int32 n;
                        while ((n = gZipStream.Read(buffer, 0, buffer.Length)) != 0)
                        {
                            dst.Write(buffer, 0, n);
                        }
                    }
                    return dst.ToArray();
                }
            }
        }
        
        /// <summary>
        /// Use GZip to decompress a file to a byte array
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be decompressed</param>
        /// <returns>Decompressed Byte array</returns>
        public static Byte[] GzipDecompress(String sFilePath)
        {
            // Read file to byte array
            Byte[] bCompressedBuffer = File.ReadAllBytes(sFilePath);
            
            // Decompress byte array
            return GzipDecompress(bCompressedBuffer);
        }
        
        /// <summary>
        /// Use GZip to decompress a byte array to a file
        /// </summary>
        /// <param name="bCompressedBuffer">Byte array containing the compressed buffer</param>
        /// <param name="sFilePath">Full path to the file on disk where the decompressed data will be written</param>
        public static void GzipDecompress(Byte[] bCompressedBuffer, String sFilePath)
        {
            // Decompress byte array
            Byte[] bUncompressedBuffer = GzipDecompress(bCompressedBuffer);
            
            // Write uncompressed byte array to file
            File.WriteAllBytes(sFilePath, bUncompressedBuffer);
        }

        /// <summary>
        /// Use GZip to decompress a file to a file
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be decompressed</param>
        /// <param name="sOutputFilePath">Full path to the file on disk where the decompressed data will be written</param>
        public static void GzipDecompress(String sFilePath, String sOutputFilePath)
        {
            // Decompress byte array
            Byte[] bUncompressedBuffer = GzipDecompress(sFilePath);
            
            // Write uncompressed byte array to file
            File.WriteAllBytes(sOutputFilePath, bUncompressedBuffer);
        }
    }
}