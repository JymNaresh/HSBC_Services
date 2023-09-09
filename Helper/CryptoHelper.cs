using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
// using Bouncy Castle library: http://www.bouncycastle.org/csharp/
using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using System.Text;


namespace EncryptionSample
{
    public static class CryptoHelper
    {
        private static readonly string Password = ConfigurationManager.AppSettings["Password"];
        public static readonly string PublicKey = ConfigurationManager.AppSettings["PublicKey"];
        public static readonly string PublicKeyPath = PublicKey;
        // note: this should be changed if the private key is not located in the current executables path
        private static readonly string PrivateKeyOnly = ConfigurationManager.AppSettings["PrivateKeyOnly"];
        private static readonly string PrivateKeyOnlyPath = PrivateKeyOnly;

        // The majority of the functionality encrypting/decrypting came from this answer: http://stackoverflow.com/a/10210465

        private static PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] pass)
        {
            PgpSecretKey pgpSecKey = pgpSec.GetSecretKey(keyId);
            if (pgpSecKey == null)
            {
                return null;
            }

            return pgpSecKey.ExtractPrivateKey(pass);
        }

        public static string DecryptPgpData(string inputData)
        {
            string output;
            //string PGPDATA = File.OpenRead(inputData).ToString();
            string PGPDATA = System.IO.File.ReadAllText(inputData);
            using (Stream inputStream = IoHelper.GetStream(PGPDATA))
            {
                using (Stream keyIn = File.OpenRead(PrivateKeyOnlyPath))
                {
                    output = DecryptPgpData(inputStream, keyIn, Password);
                }
            }
            return output;
        }

        public static string DecryptPgpData(Stream inputStream, Stream privateKeyStream, string passPhrase)
        {
            string output;

            PgpObjectFactory pgpFactory = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));
            // find secret key
            PgpSecretKeyRingBundle pgpKeyRing = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));

            PgpObject pgp = null;
            if (pgpFactory != null)
            {
                pgp = pgpFactory.NextPgpObject();
            }

            // the first object might be a PGP marker packet.
            PgpEncryptedDataList encryptedData = null;
            if (pgp is PgpEncryptedDataList)
            {
                encryptedData = (PgpEncryptedDataList)pgp;
            }
            else
            {
                encryptedData = (PgpEncryptedDataList)pgpFactory.NextPgpObject();
            }

            // decrypt
            PgpPrivateKey privateKey = null;
            PgpPublicKeyEncryptedData pubKeyData = null;
            foreach (PgpPublicKeyEncryptedData pubKeyDataItem in encryptedData.GetEncryptedDataObjects())
            {
                privateKey = FindSecretKey(pgpKeyRing, pubKeyDataItem.KeyId, passPhrase.ToCharArray());

                if (privateKey != null)
                {
                    pubKeyData = pubKeyDataItem;
                    break;
                }
            }

            if (privateKey == null)
            {
                throw new ArgumentException("Secret key for message not found.");
            }

            PgpObjectFactory plainFact = null;
            using (Stream clear = pubKeyData.GetDataStream(privateKey))
            {
                plainFact = new PgpObjectFactory(clear);
            }

            PgpObject message = plainFact.NextPgpObject();

            if (message is PgpCompressedData)
            {
                PgpCompressedData compressedData = (PgpCompressedData)message;
                PgpObjectFactory pgpCompressedFactory = null;

                using (Stream compDataIn = compressedData.GetDataStream())
                {
                    pgpCompressedFactory = new PgpObjectFactory(compDataIn);
                }

                message = pgpCompressedFactory.NextPgpObject();
                PgpLiteralData literalData = null;
                if (message is PgpOnePassSignatureList)
                {
                    message = pgpCompressedFactory.NextPgpObject();
                }

                literalData = (PgpLiteralData)message;
                using (Stream unc = literalData.GetInputStream())
                {
                    output = IoHelper.GetString(unc);
                }

            }
            else if (message is PgpLiteralData)
            {
                PgpLiteralData literalData = (PgpLiteralData)message;
                using (Stream unc = literalData.GetInputStream())
                {
                    output = IoHelper.GetString(unc);
                }
            }
            else if (message is PgpOnePassSignatureList)
            {
                throw new PgpException("Encrypted message contains a signed message - not literal data.");
            }
            else
            {
                throw new PgpException("Message is not a simple encrypted file - type unknown.");
            }

            return output;
        }

        private static PgpPublicKey ReadPublicKey(Stream inputStream)
        {
            inputStream = PgpUtilities.GetDecoderStream(inputStream);
            PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(inputStream);

            foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
            {
                foreach (PgpPublicKey key in keyRing.GetPublicKeys())
                {
                    if (key.IsEncryptionKey)
                    {
                        return key;
                    }
                }
            }

            throw new ArgumentException("Can't find encryption key in key ring.");
        }

        public static void EncryptPgpFile(string inputFile, string outputFile)
        {
            // use armor: yes, use integrity check? yes?
            EncryptPgpFile(inputFile, outputFile, PublicKeyPath, true, true);
        }

        public static void EncryptPgpFile(string inputFile, string outputFile, string publicKeyFile, bool armor, bool withIntegrityCheck)
        {
            using (Stream publicKeyStream = File.OpenRead(publicKeyFile))
            {
                PgpPublicKey pubKey = ReadPublicKey(publicKeyStream);

                using (MemoryStream outputBytes = new MemoryStream())
                {
                    PgpCompressedDataGenerator dataCompressor = new PgpCompressedDataGenerator(CompressionAlgorithmTag.Uncompressed);
                    PgpUtilities.WriteFileToLiteralData(dataCompressor.Open(outputBytes), PgpLiteralData.Binary, new FileInfo(inputFile));

                    dataCompressor.Close();
                    PgpEncryptedDataGenerator dataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256, withIntegrityCheck, new SecureRandom());
                  

                    dataGenerator.AddMethod(pubKey);
                    byte[] dataBytes = outputBytes.ToArray();

                    using (Stream outputStream = File.Create(outputFile))
                    {
                        if (armor)
                        {
                            using (ArmoredOutputStream armoredStream = new ArmoredOutputStream(outputStream))
                            {
                                IoHelper.WriteStream(dataGenerator.Open(armoredStream, dataBytes.Length), ref dataBytes);
                            }
                        }
                        else
                        {
                            IoHelper.WriteStream(dataGenerator.Open(outputStream, dataBytes.Length), ref dataBytes);
                        }
                    }
                }
            }
        }

        // Note: I was able to extract the private key into xml format .Net expecs with this
        public static string GetPrivateKeyXml(string inputData)
        {
            Stream inputStream = IoHelper.GetStream(inputData);
            PgpObjectFactory pgpFactory = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));
            PgpObject pgp = null;
            if (pgpFactory != null)
            {
                pgp = pgpFactory.NextPgpObject();
            }

            PgpEncryptedDataList encryptedData = null;
            if (pgp is PgpEncryptedDataList)
            {
                encryptedData = (PgpEncryptedDataList)pgp;
            }
            else
            {
                encryptedData = (PgpEncryptedDataList)pgpFactory.NextPgpObject();
            }

            Stream privateKeyStream = File.OpenRead(PrivateKeyOnlyPath);

            // find secret key
            PgpSecretKeyRingBundle pgpKeyRing = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));
            PgpPrivateKey privateKey = null;

            foreach (PgpPublicKeyEncryptedData pked in encryptedData.GetEncryptedDataObjects())
            {
                privateKey = FindSecretKey(pgpKeyRing, pked.KeyId, Password.ToCharArray());
                if (privateKey != null)
                {
                    //pubKeyData = pked;
                    break;
                }
            }

            // get xml:
            RsaPrivateCrtKeyParameters rpckp = ((RsaPrivateCrtKeyParameters)privateKey.Key);
            RSAParameters dotNetParams = DotNetUtilities.ToRSAParameters(rpckp);
            RSA rsa = RSA.Create();
            rsa.ImportParameters(dotNetParams);
            string xmlPrivate = rsa.ToXmlString(true);

            return xmlPrivate;
        }

        public static string Base64Encode(string plainTextfile)
        {
            // Provide correct url
            string plainText = System.IO.File.ReadAllText(plainTextfile);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }
        public static string Base64Encode(string plainTextfile,string outputpath)
        {
            // Provide correct url
            string plainText = System.IO.File.ReadAllText(plainTextfile);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            StringBuilder sb1 = new StringBuilder();

            string strPath1 = outputpath;
            //sb1.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb1.Append(System.Convert.ToBase64String(plainTextBytes).ToString() + Environment.NewLine);
            //sb1.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw1 = new StreamWriter(strPath1, true);
            //sw1.Flush();
            sw1.Write(sb1.ToString());
            sw1.Close();

            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string plainTextfile)
        {
            string plainText = System.IO.File.ReadAllText(plainTextfile);
            byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            byte[] base64EncodedBytes = System.Convert.FromBase64String(plainText);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
        public static string Base64Decode(string plainTextfile, string outputpath)
        {
            // Provide correct url
            string plainText = System.IO.File.ReadAllText(plainTextfile);
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);

            var base64EncodedBytes = System.Convert.FromBase64String(System.Convert.ToBase64String(plainTextBytes));
            StringBuilder sb1 = new StringBuilder();

            string strPath1 = outputpath;
            //sb1.Append(System.DateTime.Now.ToString() + Environment.NewLine);
            sb1.Append(System.Convert.FromBase64String(System.Convert.ToBase64String(plainTextBytes)).ToString() + Environment.NewLine);
            //sb1.Append("-------------------------------------------------------" + Environment.NewLine);
            StreamWriter sw1 = new StreamWriter(strPath1, true);
            //sw1.Flush();
            sw1.Write(sb1.ToString());
            sw1.Close();

            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        
    }
}
