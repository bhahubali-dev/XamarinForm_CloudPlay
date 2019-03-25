using System;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities.Encoders;

namespace MediaMallTechnologies
{
	public class AccessInfoEncryptor : AccessInfoCryptoBase
	{
		protected static ECPublicKeyParameters publicKey;

		static AccessInfoEncryptor()
		{
			publicKey = new ECPublicKeyParameters(
				domainParameters.Curve.DecodePoint(
					Hex.Decode(
						"04" +
						"96099060a1a4b35f753f55bd71d1cb369ce8828af94892235e79565372ef69bb370f78edf81939f6d7db5264ca7b796d" +
						"09878c5b4a13aaa7c836fe7f5da189dc390e49a99752c6e849481f6ba552dfdc0a72e6f51cd5a14c4419bc640dbf4f95"
					)
				), domainParameters);
		}

		public static byte[] Encrypt(string input)
		{
			var keyGenerationParameters = new ECKeyGenerationParameters(domainParameters, new SecureRandom());

			var ecKeyPairGenerator = new ECKeyPairGenerator();
			ecKeyPairGenerator.Init(keyGenerationParameters);

			AsymmetricCipherKeyPair ephemeralKeyPair = ecKeyPairGenerator.GenerateKeyPair();

			byte[] plaintext = Encoding.UTF8.GetBytes(input);

			var engine = CreateCipherEngine(true, ephemeralKeyPair.Private, publicKey);
			byte[] ciphertext = engine.ProcessBlock(plaintext, 0, plaintext.Length);

			var ecPublicKeyParameters = ephemeralKeyPair.Public as ECPublicKeyParameters;
			var ecPublicKey = ecPublicKeyParameters.Q.GetEncoded();

			byte[] UcatTAGcatC = new byte[ecPublicKey.Length + ciphertext.Length];
			Buffer.BlockCopy(ecPublicKey, 0, UcatTAGcatC, 0, ecPublicKey.Length);
			Buffer.BlockCopy(ciphertext, 0, UcatTAGcatC, ecPublicKey.Length, ciphertext.Length);
			return UcatTAGcatC;
		}
	}
}
