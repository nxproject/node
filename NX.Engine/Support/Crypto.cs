///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2021 Jose E. Gonzalez (nxoffice2021@gmail.com) - All Rights Reserved
/// 
/// Permission is hereby granted, free of charge, to any person obtaining a copy
/// of this software and associated documentation files (the "Software"), to deal
/// in the Software without restriction, including without limitation the rights
/// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
/// copies of the Software, and to permit persons to whom the Software is
/// furnished to do so, subject to the following conditions:
/// 
/// The above copyright notice and this permission notice shall be included in all
/// copies or substantial portions of the Software.
/// 
/// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
/// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
/// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
/// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
/// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
/// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
/// SOFTWARE.
/// 
///--------------------------------------------------------------------------------

using System;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Security.Cryptography;

using NX.Shared;

namespace NX.Engine
{
    /// <summary>
    /// 
    /// A general cryptography class
    /// 
    /// It is not kept as part of the extensions as many
    /// of its functions are OS dependent
    /// 
    /// As the function title explains the use of each call,
    /// I will not describe them in detail.
    /// 
    /// </summary>
    public static class CryptoClass
    {
        #region Enums
        public enum CryptoAlgorithm
        {
            DES,
            TripleDES,
            RC2,
            Rijiandel
        }

        public enum CryptoHashAlgorithm
        {
            MD5,
            SHA1
        }
        #endregion Enums

        #region Hash
        public static byte[] MD5Hash(this byte[] value)
        {
            return value.Hash(CryptoHashAlgorithm.MD5);
        }

        public static byte[] SHA1Hash(this byte[] value)
        {
            return value.Hash(CryptoHashAlgorithm.SHA1);
        }

        private static byte[] Hash(this byte[] value, CryptoHashAlgorithm algorithm)
        {
            HashAlgorithm c_Provider = null;

            switch (algorithm)
            {
                case CryptoHashAlgorithm.MD5:

                    c_Provider = new MD5CryptoServiceProvider();
                    break;

                case CryptoHashAlgorithm.SHA1:

                    c_Provider = new SHA1CryptoServiceProvider();
                    break;

            }

            return c_Provider.ComputeHash(value);
        }

        public static string MD5HashString(this string value)
        {
            return value.ToBytes().MD5HashString();
        }

        public static string MD5HashString(this byte[] value)
        {
            byte[] baWkg = MD5Hash(value);

            return BitConverter.ToString(baWkg).Replace("-", "").Replace(":", "");
        }

        public static string SHA1HashString(this string value, string addition = "")
        {
            byte[] baWkg = SHA1Hash((value + addition).ToBytes()); ;

            return BitConverter.ToString(baWkg).Replace("-", "");
        }

        public static bool IsMD5Hash(this string value)
        {
            return value.Length == 32 && Regex.Replace(value, @"[0-9a-fA-F]", "").Length == 0;
        }

        public static bool IsSHA1Hash(this string value)
        {
            return value.Length == 40 && Regex.Replace(value, @"[0-9a-fA-F]", "").Length == 0;
        }

        public static string MakePwd(this string value)
        {
            return value.MD5HashString().Substring(17, 6);
        }
        #endregion

        #region Encode/Decode
        public static byte[] Encode(this byte[] value, string password, CryptoAlgorithm algorithm)
        {
            byte[] baAns = null;

            SymmetricAlgorithm c_Provider = algorithm.GetAlgorithm();

            if (value != null && value.Length > 0)
            {
                try
                {
                    ComputeKey(c_Provider, password);

                    MemoryStream c_Mem = new MemoryStream();
                    CryptoStream c_Crypto = new CryptoStream(c_Mem, c_Provider.CreateEncryptor(), CryptoStreamMode.Write);
                    c_Crypto.Write(value, 0, value.Length);
                    c_Crypto.Flush();
                    c_Crypto.Close();

                    baAns = c_Mem.ToArray();
                }
                catch { }
            }

            return baAns;
        }

        public static byte[] Encode(this byte[] value, string password)
        {
            return value.Encode(password, CryptoAlgorithm.TripleDES);
        }

        public static byte[] Decode(this byte[] value, string password, CryptoAlgorithm algorithm)
        {
            byte[] baAns = null;

            SymmetricAlgorithm c_Provider = algorithm.GetAlgorithm();

            if (value != null && value.Length > 0)
            {
                try
                {
                    ComputeKey(c_Provider, password);

                    MemoryStream c_Mem = new MemoryStream();
                    CryptoStream c_Crypto = new CryptoStream(c_Mem, c_Provider.CreateDecryptor(), CryptoStreamMode.Write);
                    c_Crypto.Write(value, 0, value.Length);
                    c_Crypto.Flush();
                    c_Crypto.Close();

                    baAns = c_Mem.ToArray();
                }
                catch (Exception e)
                {
                    var a = e;
                }
            }

            return baAns;
        }

        public static byte[] Decode(this byte[] value, string password)
        {
            return value.Decode(password, CryptoAlgorithm.TripleDES);
        }

        private static SymmetricAlgorithm GetAlgorithm(this CryptoAlgorithm algorithm)
        {
            SymmetricAlgorithm c_Ans = null;

            switch (algorithm)
            {
                case CryptoAlgorithm.DES:

                    c_Ans = new DESCryptoServiceProvider();
                    break;

                case CryptoAlgorithm.TripleDES:

                    c_Ans = new TripleDESCryptoServiceProvider();
                    break;

                case CryptoAlgorithm.RC2:

                    c_Ans = new RC2CryptoServiceProvider();
                    break;

                case CryptoAlgorithm.Rijiandel:

                    c_Ans = new RijndaelManaged();
                    break;

            }

            return c_Ans;
        }

        private static void ComputeKey(this SymmetricAlgorithm provider, string password)
        {
            SHA256Managed c_Hash = new SHA256Managed();
            System.Security.Cryptography.KeySizes[] baSizes = provider.LegalKeySizes;
            int iSIV = provider.IV.Length;
            int iSize = 0;
            //provider.Padding = PaddingMode.ANSIX923;

            foreach (System.Security.Cryptography.KeySizes c_KS in baSizes)
            {
                int iNew = (int)(c_KS.MaxSize / 8);
                if (c_KS.MaxSize == (8 * iNew))
                {
                    if (iNew > iSize) iSize = iNew;
                }
            }

            int iMin = iSize + iSIV;
            string sKey = password;

            while (sKey.Length < iMin) sKey += sKey;

            byte[] baPwd = Encoding.UTF8.GetBytes(sKey.Substring(0, iSize));
            byte[] baIV = Encoding.UTF8.GetBytes(sKey.Substring(iSize, iSIV));

            provider.Key = baPwd;
            provider.IV = baIV;
        }
        #endregion

        #region String
        public static string EncodeString(this string value, string pwd)
        {
            return Convert.ToBase64String(Encode(UTF32Encoding.UTF32.GetBytes(value), pwd));
        }

        public static string DecodeString(this string value, string pwd)
        {
            return UTF32Encoding.UTF32.GetString(Decode(Convert.FromBase64String(value), pwd));
        }
        #endregion

        #region eCandidus
        public static string DecodeFromBase64(this string value, string password, CryptoAlgorithm algorithm)
        {
            string sAns = null;
            SymmetricAlgorithm c_Provider = GetAlgorithm(algorithm);

            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    ComputeKey(c_Provider, password);

                    MemoryStream c_Mem = new MemoryStream();
                    CryptoStream c_Crypto = new CryptoStream(c_Mem, c_Provider.CreateDecryptor(), CryptoStreamMode.Write);
                    byte[] baValue = Convert.FromBase64String(value);
                    c_Crypto.Write(baValue, 0, baValue.Length);
                    c_Crypto.Flush();
                    c_Crypto.Close();

                    sAns = c_Mem.ToArray().FromASCIIBytes();
                }
                catch { }
            }

            return sAns;
        }

        public static string DecodeFromBase64(this string value, string password, bool nospace = false)
        {
            if (nospace) value = value.Replace(" ", "+");
            return DecodeFromBase64(value, password, CryptoAlgorithm.RC2);
        }

        public static string EncodeToBase64(this string value, string password, CryptoAlgorithm algorithm)
        {
            string sAns = null;
            SymmetricAlgorithm c_Provider = GetAlgorithm(algorithm);

            if (!string.IsNullOrEmpty(value))
            {
                try
                {
                    ComputeKey(c_Provider, password);

                    MemoryStream c_Mem = new MemoryStream();
                    CryptoStream c_Crypto = new CryptoStream(c_Mem, c_Provider.CreateEncryptor(), CryptoStreamMode.Write);
                    byte[] baValue = value.ToASCIIBytes();
                    c_Crypto.Write(baValue, 0, baValue.Length);
                    c_Crypto.Flush();
                    c_Crypto.Close();

                    sAns = Convert.ToBase64String(c_Mem.ToArray());
                }
                catch { }
            }

            return sAns;
        }

        public static string EncodeToBase64(this string value, string password)
        {
            return EncodeToBase64(value, password, CryptoAlgorithm.RC2);
        }
        #endregion

        #region Hex
        public static string DecodeFromHex(this string value, string password, CryptoAlgorithm algorithm)
        {
            string sAns = null;
            SymmetricAlgorithm c_Provider = GetAlgorithm(algorithm);

            try
            {
                ComputeKey(c_Provider, password);

                MemoryStream c_Mem = new MemoryStream();
                CryptoStream c_Crypto = new CryptoStream(c_Mem, c_Provider.CreateDecryptor(), CryptoStreamMode.Write);
                byte[] baValue = value.HexStringToByteArray();
                c_Crypto.Write(baValue, 0, baValue.Length);
                c_Crypto.Flush();
                c_Crypto.Close();

                sAns = c_Mem.ToArray().FromASCIIBytes();
            }
            catch
            {
            }

            return sAns;
        }

        public static string DecodeFromHex(this string value, string password)
        {
            return DecodeFromHex(value, password, CryptoAlgorithm.RC2);
        }

        public static string EncodeToHex(this string value, string password, CryptoAlgorithm algorithm)
        {
            string sAns = null;
            SymmetricAlgorithm c_Provider = GetAlgorithm(algorithm);

            try
            {
                ComputeKey(c_Provider, password);

                MemoryStream c_Mem = new MemoryStream();
                CryptoStream c_Crypto = new CryptoStream(c_Mem, c_Provider.CreateEncryptor(), CryptoStreamMode.Write);
                byte[] baValue = value.ToASCIIBytes();
                c_Crypto.Write(baValue, 0, baValue.Length);
                c_Crypto.Flush();
                c_Crypto.Close();

                sAns = c_Mem.ToArray().ByteArrayToHexString();
            }
            catch { }

            return sAns;
        }

        public static string EncodeToHex(this string value, string password)
        {
            return EncodeToHex(value, password, CryptoAlgorithm.RC2);
        }

        public static int HexToInt(this string hex, int defaultvalue)
        {
            int iAns = defaultvalue;

            try
            {
                iAns = Int32.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            }
            catch { }

            return iAns;
        }

        public static byte HexToByte(this string hex, byte defaultvalue)
        {
            byte iAns = defaultvalue;

            try
            {
                iAns = Byte.Parse(hex, System.Globalization.NumberStyles.HexNumber);
            }
            catch
            {
            }

            return iAns;
        }

        public static string ByteToHex(this byte value)
        {
            string sWkg = string.Format("{0:X2}", value);

            while (sWkg.Length < 2) sWkg = "0" + sWkg;

            return sWkg;
        }

        public static string ByteArrayToHexString(this byte[] value)
        {
            string sAns = string.Empty;

            for (int iLoop = 0; iLoop < value.Length; iLoop++)
            {
                sAns += ByteToHex(value[iLoop]);
            }

            return sAns;
        }

        public static byte[] HexStringToByteArray(this string value)
        {
            int iLen = value.Length / 2;

            byte[] baAns = new byte[iLen];

            int iPos = 0;
            for (int iLoop = 0; iLoop < value.Length; iLoop += 2)
            {
                baAns[iPos++] = HexToByte(value.Substring(iLoop, 2), 0);
            }

            return baAns;
        }
        #endregion

        #region Compression
        public static byte[] CompressArray(this byte[] buffer)
        {
            MemoryStream c_MS = new MemoryStream();
            System.IO.Stream c_Stream = new ICSharpCode.SharpZipLib.GZip.GZipOutputStream(c_MS);

            c_Stream.Write(buffer, 0, buffer.Length);
            c_Stream.Flush();
            c_Stream.Close();

            return c_MS.ToArray();
        }

        public static byte[] DecompressArray(this byte[] buffer)
        {
            byte[] baAns = null;

            try
            {
                using (System.IO.MemoryStream c_Result = new MemoryStream())
                {
                    using (System.IO.MemoryStream c_Source = new MemoryStream(buffer, 0, buffer.Length))
                    {
                        using (ICSharpCode.SharpZipLib.GZip.GZipInputStream c_Stream = new ICSharpCode.SharpZipLib.GZip.GZipInputStream(c_Source))
                        {
                            byte[] abBuffer = new byte[1024];

                            int iSize = c_Stream.Read(abBuffer, 0, abBuffer.Length);
                            while (iSize > 0)
                            {
                                c_Result.Write(abBuffer, 0, iSize);

                                iSize = c_Stream.Read(abBuffer, 0, abBuffer.Length);
                            }
                        }
                    }

                    baAns = c_Result.ToArray();
                }
            }
            catch
            {
            }

            return baAns;
        }

        public static string Compress(this string value)
        {
            return CompressArray(value.ToBytes()).ToBase64().Base64Secure();
        }

        public static string Decompress(this string value)
        {
            return DecompressArray(value.Base64Unsecure().FromBase64Bytes()).FromBytes();
        }
        #endregion

        #region Base36
        public static string base36 = "01234567898ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string base62 = "01234567898ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

        public static string ToBase36(this long value, int length = -1)
        {
            return base36.ToBase(value, length);
        }

        public static long FromBase36(this string value)
        {
            return base36.FromBase(value);
        }

        public static string ToBase62(this long value, int length = -1)
        {
            return base62.ToBase(value, length);
        }

        public static long FromBase62(this string value)
        {
            return base62.FromBase(value);
        }

        private static string ToBase(this string basec, long value, int length)
        {
            string sAns = string.Empty;

            while (length > sAns.Length || (length ==-1 && value > 0))
            {
                byte bVal = (byte)(value % basec.Length);
                sAns = basec[bVal] + sAns;

                value /= basec.Length;
            }

            return sAns;
        }

        private static long FromBase(this string basec, string value)
        {
            long lAns = 0;

            while (value.Length > 0)
            {
                lAns *= basec.Length;

                byte bWkg = (byte)basec.IndexOf(value[0]);
                lAns += bWkg;

                value = value.Substring(1);
            }

            return lAns;
        }
        #endregion
    }
}