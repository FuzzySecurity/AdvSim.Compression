using System;
using System.Security.Cryptography;
using NUnit.Framework;
using AdvSim.Compression;

namespace AdvSim.Compression.UnitTest
{
    [TestFixture]
    public class Tests
    {
        // Unit test globals
        //===================================
        
        private static Byte[] bTest = GenerateRandomByteArray(0x1000);
        private static String sTestHash = GenerateByteHash(bTest);
        
        // Helper functions
        //===================================
        
        // Generate random byte array of given size
        private static Byte[] GenerateRandomByteArray(Int32 size)
        {
            Byte[] data = new Byte[size];
            Random random = new Random();
            random.NextBytes(data);
            return data;
        }
        
        // Hash byte array
        public static String GenerateByteHash(Byte[] bArray)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            Byte[] bHash = sha1.ComputeHash(bArray);
            return BitConverter.ToString(bHash).Replace("-", "");
        }
        
        // Tests
        //===================================

        [Test]
        public void Test_Gzip()
        {
            // Compress
            Byte[] bCompressed = GZip.GzipCompress(bTest);
            // Decompress
            Byte[] bDecompressed = GZip.GzipDecompress(bCompressed);
            
            // Check that the decompressed data is the same as the original
            Assert.AreEqual(sTestHash, GenerateByteHash(bDecompressed));
        }
        
        [Test]
        public void Test_LZMA()
        {
            // Compress
            Byte[] bCompressed = LZMA.LZMACompress(bTest);
            // Decompress
            Byte[] bDecompressed = LZMA.LZMADecompress(bCompressed);
            
            // Check that the decompressed data is the same as the original
            Assert.AreEqual(sTestHash, GenerateByteHash(bDecompressed));
        }
        
        [Test]
        public void Test_LZNT()
        {
            // Compress
            Byte[] bCompressed = LZNT.RtlCompressBuffer(bTest);
            // Decompress
            Byte[] bDecompressed = LZNT.RtlDecompressBuffer(bCompressed);
            
            // Check that the decompressed data is the same as the original
            Assert.AreEqual(sTestHash, GenerateByteHash(bDecompressed));
        }
        
        [Test]
        public void Test_Direct3D()
        {
            // Compress
            Byte[] bCompressed = Direct3D.D3DCompressShaders(bTest);
            // Decompress
            Byte[] bDecompressed = Direct3D.D3DDecompressShaders(bCompressed);
            
            // Check that the decompressed data is the same as the original
            Assert.AreEqual(sTestHash, GenerateByteHash(bDecompressed));
        }
        
        [Test]
        public void Test_LZMS()
        {
            // Compress
            Byte[] bCompressed = Cabinet.CompressStorage(bTest, Cabinet.CompressionAlgorithm.COMPRESS_ALGORITHM_LZMS);
            // Decompress
            Byte[] bDecompressed = Cabinet.DecompressStorage(bCompressed, Cabinet.CompressionAlgorithm.COMPRESS_ALGORITHM_LZMS);
            
            // Check that the decompressed data is the same as the original
            Assert.AreEqual(sTestHash, GenerateByteHash(bDecompressed));
        }
        
        [Test]
        public void Test_MSZIP()
        {
            // Compress
            Byte[] bCompressed = Cabinet.CompressStorage(bTest, Cabinet.CompressionAlgorithm.COMPRESS_ALGORITHM_MSZIP);
            // Decompress
            Byte[] bDecompressed = Cabinet.DecompressStorage(bCompressed, Cabinet.CompressionAlgorithm.COMPRESS_ALGORITHM_MSZIP);
            
            // Check that the decompressed data is the same as the original
            Assert.AreEqual(sTestHash, GenerateByteHash(bDecompressed));
        }
        
        [Test]
        public void Test_XPRESS()
        {
            // Compress
            Byte[] bCompressed = Cabinet.CompressStorage(bTest, Cabinet.CompressionAlgorithm.COMPRESS_ALGORITHM_XPRESS);
            // Decompress
            Byte[] bDecompressed = Cabinet.DecompressStorage(bCompressed, Cabinet.CompressionAlgorithm.COMPRESS_ALGORITHM_XPRESS);
            
            // Check that the decompressed data is the same as the original
            Assert.AreEqual(sTestHash, GenerateByteHash(bDecompressed));
        }
        
        [Test]
        public void Test_HUFF()
        {
            // Compress
            Byte[] bCompressed = Cabinet.CompressStorage(bTest, Cabinet.CompressionAlgorithm.COMPRESS_ALGORITHM_XPRESS_HUFF);
            // Decompress
            Byte[] bDecompressed = Cabinet.DecompressStorage(bCompressed, Cabinet.CompressionAlgorithm.COMPRESS_ALGORITHM_XPRESS_HUFF);
            
            // Check that the decompressed data is the same as the original
            Assert.AreEqual(sTestHash, GenerateByteHash(bDecompressed));
        }
    }
}