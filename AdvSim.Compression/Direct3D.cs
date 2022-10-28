#if NETFRAMEWORK

using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AdvSim.Compression
{
    public class Direct3D
    {
        // Direct3D API's
        // |_ https://modexp.wordpress.com/2019/12/08/shellcode-compression/#d3d
        //===================================

        [DllImport("D3DCompiler_47.dll")]
        internal static extern UInt32 D3DCompressShaders(
            UInt32 uuNumShaders,
            ref D3D_SHADER_DATA pShaderData,
            UInt32 uFlags,
            ref ID3DBlob ppCompressedData);
        
        [DllImport("D3DCompiler_47.dll")]
        internal static extern UInt32 D3DDecompressShaders(
            IntPtr pSrcData,
            UIntPtr iSrcDataSize,
            UInt32 uNumShaders,
            UInt32 uStartIndex,
            ref UInt32 pIndices,
            UInt32 uFlags,
            ref ID3DBlob ppShaders,
            ref UInt32 pTotalShaders);

        // Structs
        //===================================
        
        [StructLayout(LayoutKind.Sequential)]
        internal struct D3D_SHADER_DATA
        {
            public IntPtr pBytecode;
            public UInt32 BytecodeLength;
        }
        
        // Interfaces
        //===================================
        
        [Guid("8BA5FB08-5195-40e2-AC58-0D989C3A0102")]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        internal interface ID3DBlob
        {
            [PreserveSig]
            IntPtr GetBufferPointer();

            [PreserveSig]
            UInt32 GetBufferSize();
        }
        
        // Constants
        //===================================
        
        internal const UInt32 D3DCOMPRESS_SHADER_KEEP_ALL_PARTS = 0x00000001;
        
        // Methods
        //===================================
        
        /// <summary>
        /// Use Direct3d shaders to compress a byte array to a byte array
        /// </summary>
        /// <param name="bUncompressedBuffer">Byte array containing the uncompressed buffer</param>
        /// <returns>Compressed Byte array</returns>
        public static Byte[] D3DCompressShaders(Byte[] bUncompressedBuffer)
        {
            // We must marshall an unmanaged pointer to the byte array
            IntPtr pUncompressedBuffer = Marshal.AllocHGlobal(bUncompressedBuffer.Length);
            Marshal.Copy(bUncompressedBuffer, 0, pUncompressedBuffer, bUncompressedBuffer.Length);

            // Create D3D_SHADER_DATA struct
            D3D_SHADER_DATA d3dShaderData = new D3D_SHADER_DATA();
            d3dShaderData.pBytecode = pUncompressedBuffer;
            d3dShaderData.BytecodeLength = (UInt32)bUncompressedBuffer.Length;
            
            // Compress
            ID3DBlob oID3DBlob = null;
            UInt32 iCallRes = D3DCompressShaders(
                1,
                ref d3dShaderData,
                D3DCOMPRESS_SHADER_KEEP_ALL_PARTS,
                ref oID3DBlob);

            // Call shader interface
            UInt32 iSize = oID3DBlob.GetBufferSize();
            IntPtr pBlob = oID3DBlob.GetBufferPointer();

            // Copy to byte[]
            Byte[] bCompressedBuffer = new Byte[iSize];
            Marshal.Copy(pBlob, bCompressedBuffer, 0, (Int32)iSize);
            
            // Free blob
            Marshal.ReleaseComObject(oID3DBlob);
            Marshal.FreeHGlobal(pUncompressedBuffer);
            
            // Return compressed buffer
            return bCompressedBuffer;
        }
        
        /// <summary>
        /// Use Direct3d shaders to compress a file to a byte array
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be compressed</param>
        /// <returns>Compressed Byte array</returns>
        public static Byte[] D3DCompressShaders(String sFilePath)
        {
            // Read file
            Byte[] bUncompressedBuffer = File.ReadAllBytes(sFilePath);
            
            // Compress
            return D3DCompressShaders(bUncompressedBuffer);
        }
        
        /// <summary>
        /// Use Direct3d shaders to compress a byte array to a file
        /// </summary>
        /// <param name="bUncompressedBuffer">Byte array containing the uncompressed buffer</param>
        /// <param name="sFilePath">Full path to the file on disk where the compressed data will be written</param>
        public static void D3DCompressShaders(Byte[] bUncompressedBuffer, String sFilePath)
        {
            // Compress
            Byte[] bCompressedBuffer = D3DCompressShaders(bUncompressedBuffer);
            
            // Write file
            File.WriteAllBytes(sFilePath, bCompressedBuffer);
        }
        
        /// <summary>
        /// Use Direct3d shaders to compress a file to a file
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be compressed</param>
        /// <param name="sOutputFilePath">Full path to the file on disk where the compressed data will be written</param>
        public static void D3DCompressShaders(String sFilePath, String sOutputFilePath)
        {
            // Read file
            Byte[] bUncompressedBuffer = File.ReadAllBytes(sFilePath);
            
            // Compress
            D3DCompressShaders(bUncompressedBuffer, sOutputFilePath);
        }
        
        /// <summary>
        /// Use Direct3d shaders to decompress a byte array to a byte array
        /// </summary>
        /// <param name="bCompressedBuffer">Byte array containing the compressed buffer</param>
        /// <returns>Decompressed Byte array</returns>
        public static Byte[] D3DDecompressShaders(Byte[] bCompressedBuffer)
        {
            // We must marshall an unmanaged pointer to the byte array
            IntPtr pCompressedBuffer = Marshal.AllocHGlobal(bCompressedBuffer.Length);
            Marshal.Copy(bCompressedBuffer, 0, pCompressedBuffer, bCompressedBuffer.Length);
            
            UInt32 iTotalShaders = 0;
            UInt32 iIndices = 0;
            ID3DBlob oID3DBlob = null;
            
            UInt32 iCallRes = D3DDecompressShaders(
                pCompressedBuffer,
                new UIntPtr((UInt32)bCompressedBuffer.Length),
                1,
                0,
                ref iIndices,
                0,
                ref oID3DBlob,
                ref iTotalShaders);

            // Call shader interface
            UInt32 iSize = oID3DBlob.GetBufferSize();
            IntPtr pBlob = oID3DBlob.GetBufferPointer();

            // Copy to byte[]
            Byte[] bUnCompressedBuffer = new Byte[iSize];
            Marshal.Copy(pBlob, bUnCompressedBuffer, 0, (Int32)iSize);
            
            // Free blob
            Marshal.ReleaseComObject(oID3DBlob);
            Marshal.FreeHGlobal(pCompressedBuffer);
            
            // Return decompressed buffer
            return bUnCompressedBuffer;
        }
        
        /// <summary>
        /// Use Direct3d shaders to decompress a file to a byte array
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be decompressed</param>
        /// <returns>Decompressed Byte array</returns>
        public static Byte[] D3DDecompressShaders(String sFilePath)
        {
            // Read file
            Byte[] bCompressedBuffer = File.ReadAllBytes(sFilePath);
            
            // Decompress
            return D3DDecompressShaders(bCompressedBuffer);
        }
        
        /// <summary>
        /// Use Direct3d shaders to decompress a byte array to a file
        /// </summary>
        /// <param name="bCompressedBuffer">Byte array containing the compressed buffer</param>
        /// <param name="sFilePath">Full path to the file on disk where the decompressed data will be written</param>
        public static void D3DDecompressShaders(Byte[] bCompressedBuffer, String sFilePath)
        {
            // Decompress
            Byte[] bUnCompressedBuffer = D3DDecompressShaders(bCompressedBuffer);
            
            // Write file
            File.WriteAllBytes(sFilePath, bUnCompressedBuffer);
        }
        
        /// <summary>
        /// Use Direct3d shaders to decompress a file to a file
        /// </summary>
        /// <param name="sFilePath">Full path to the file on disk which will be decompressed</param>
        /// <param name="sOutputFilePath">Full path to the file on disk where the decompressed data will be written</param>
        public static void D3DDecompressShaders(String sFilePath, String sOutputFilePath)
        {
            // Read file
            Byte[] bCompressedBuffer = File.ReadAllBytes(sFilePath);
            
            // Decompress
            D3DDecompressShaders(bCompressedBuffer, sOutputFilePath);
        }
    }
}

#endif