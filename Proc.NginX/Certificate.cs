///--------------------------------------------------------------------------------
/// 
/// Copyright (C) 2020-2024 Jose E. Gonzalez (nx.jegbhe@gmail.com) - All Rights Reserved
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
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using Org.BouncyCastle.Crypto.Operators;

using NX.Engine;
using NX.Engine.NginX;
using NX.Shared;

namespace Proc.NginX
{
    public class CertificateClass : ChildOfClass<EnvironmentClass>
    {
        #region Constructor
        public CertificateClass(EnvironmentClass env)
            : base(env)
        {
            //
            this.Path = this.Domain.StoragePath(true, true);
        }

        public CertificateClass(EnvironmentClass env, string path)
            : base(env)
        {
            //
            this.Path = path;
        }
        #endregion

        #region Properties
        /// <summary>
        /// 
        /// The domain of the certificate
        /// 
        /// </summary>
        public string Domain { get { return this.Parent.Domain; } }

        /// <summary>
        /// 
        /// The path of the certificate
        /// 
        /// </summary>
        public string Path { get; private set; } 

        /// <summary>
        /// 
        /// The path to the keys
        /// 
        /// </summary>
        public string KeyPath { get { return this.Domain.StoragePath(true, false); } }

        /// <summary>
        /// 
        /// Is the certificate valid?
        /// 
        /// </summary>
        public bool IsValid
        {
            get
            {
                // 
                return this.Path.FileExists();
            }
        }
        #endregion

        #region Statics
        private string PEM(string type, byte[] value)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(this.LINE("BEGIN", type));
            builder.AppendLine(Convert.ToBase64String(value, Base64FormattingOptions.InsertLineBreaks));
            builder.AppendLine(this.LINE("END", type));

            return builder.ToString();
        }

        private string LINE(string se, string type)
        {
            return "-----" + se + " " + type + "-----";
        }

        private AsymmetricCipherKeyPair GenerateKeys(int keysize)
        {
            RsaKeyPairGenerator r = new RsaKeyPairGenerator();
            r.Init(new KeyGenerationParameters(new SecureRandom(), keysize));
            return r.GenerateKeyPair();
        }

        private AsymmetricAlgorithm ToDotNetKey(RsaPrivateCrtKeyParameters privateKey)
        {
            var cspParams = new CspParameters()
            {
                KeyContainerName = Guid.NewGuid().ToString(),
                KeyNumber = (int)KeyNumber.Exchange,
                Flags = CspProviderFlags.UseMachineKeyStore
            };

            var rsaProvider = new RSACryptoServiceProvider(cspParams);
            var parameters = new RSAParameters()
            {
                Modulus = privateKey.Modulus.ToByteArrayUnsigned(),
                P = privateKey.P.ToByteArrayUnsigned(),
                Q = privateKey.Q.ToByteArrayUnsigned(),
                DP = privateKey.DP.ToByteArrayUnsigned(),
                DQ = privateKey.DQ.ToByteArrayUnsigned(),
                InverseQ = privateKey.QInv.ToByteArrayUnsigned(),
                D = privateKey.Exponent.ToByteArrayUnsigned(),
                Exponent = privateKey.PublicExponent.ToByteArrayUnsigned()
            };

            rsaProvider.ImportParameters(parameters);

            return rsaProvider;
        }
        #endregion
    }
}