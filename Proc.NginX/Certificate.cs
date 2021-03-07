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
        public CertificateClass(EnvironmentClass env, bool live)
            : base(env)
        {
            //
            this.Live = live;
            this.CertificatePath = this.Domain.StoragePath(this.Live, true);

            this.Parse();

                // 
            if (!this.Live)
            {
                // Do we need a new one?
                if (!this.IsValid && !(this.Domain.IsIPV4() || this.Domain.IsIPV6()))
                {
                    // Delete current
                    this.CertificatePath.DeleteFile();
                    this.KeyPath.DeleteFile();

                    // And make
                    this.CreateSelfSignedCertificate(2048);
                }
            }
        }
        public CertificateClass(EnvironmentClass env, string path): base(env)
        {
            //
            this.CertificatePath = path;

            this.Parse();
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
        /// Is it the live certificate
        /// 
        /// </summary>
        public bool Live { get; private set; }

        /// <summary>
        /// 
        /// The path of the certificate
        /// 
        /// </summary>
        public string CertificatePath { get; private set; } 

        /// <summary>
        /// 
        /// The path to the keys
        /// 
        /// </summary>
        public string KeyPath { get { return this.Domain.StoragePath(this.Live, false); } }

        /// <summary>
        /// 
        /// The common name of the current certificate
        /// 
        /// </summary>
        public string CommonName { get; private set; }

        /// <summary>
        /// 
        /// The epiration date
        /// 
        /// </summary>
        public DateTime Expiration { get; private set; }

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
                return this.CommonName.IsSameValue(this.Domain) &&
                                this.Expiration>= DateTime.Today;
            }
        }
        #endregion

        #region Methods
        private void Parse()
        {
            // Tell the world
            //this.Parent.LogInfo("Certificate path is {0}".FormatString(this.CertificatePath));

            try
            {
                if (this.CertificatePath.FileExists())
                {
                    using (X509Certificate2 c_Cert = new X509Certificate2(this.CertificatePath.ReadFileAsBytes()))
                    {
                        // Get items
                        this.CommonName = c_Cert.GetNameInfo(X509NameType.SimpleName, false);
                        this.Expiration = c_Cert.GetExpirationDateString().FromDBDate();
                    }
                }
            }
            catch (Exception e)
            {
                this.CommonName = "";
                this.Expiration = DateTime.MinValue;

                this.Parent.LogException("While parsing the certificate", e);
            }
        }

        /// <summary>
        /// 
        /// Create a self signed certificaate
        /// 
        /// </summary>
        /// <param name="keysize"></param>
        private void CreateSelfSignedCertificate(int keysize)
        {
            // Generate key pair
            AsymmetricCipherKeyPair c_KP = this.GenerateKeys(keysize);

            // Generating Random Numbers
            CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();
            SecureRandom random = new SecureRandom(randomGenerator);
            ISignatureFactory signatureFactory = new Asn1SignatureFactory("SHA512WITHRSA", c_KP.Private, random);

            // The Certificate Generator
            X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
            certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage.Id, true, new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth));

            // Serial Number
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);

            // Issuer and Subject Name
            certificateGenerator.SetIssuerDN(new X509Name(this.Domain));
            certificateGenerator.SetSubjectDN(new X509Name(this.Domain));

            // Valid For
            DateTime notBefore = DateTime.UtcNow.Date;
            DateTime notAfter = notBefore.AddYears(2);
            certificateGenerator.SetNotBefore(notBefore);
            certificateGenerator.SetNotAfter(notAfter);

            //certificateGenerator.SetPublicKey(subjectKeyPair.Public);
            certificateGenerator.SetPublicKey(c_KP.Public);

            // selfsign certificate
            Org.BouncyCastle.X509.X509Certificate certificate = certificateGenerator.Generate(signatureFactory);
            var dotNetPrivateKey = this.ToDotNetKey((RsaPrivateCrtKeyParameters)c_KP.Private);

            // merge into X509Certificate2
            X509Certificate2 x509 = new X509Certificate2(DotNetUtilities.ToX509Certificate(certificate));
            x509.PrivateKey = dotNetPrivateKey;
            x509.FriendlyName = this.Domain;

            // Save
            try
            {
                this.CertificatePath.WriteFile(this.PEM("CERTIFICATE", x509.Export(X509ContentType.Cert)));
            }
            catch
            {
                this.Parent.LogError("Unable to save self signed certificate, state is unknown!");
            }

            //
            this.Parent.LogInfo("Created self signed certificate");
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