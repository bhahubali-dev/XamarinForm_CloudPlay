using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Agreement;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Paddings;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Utilities.Encoders;

namespace MediaMallTechnologies
{
	public class AccessInfoCryptoBase
	{
		protected static byte[] derivation;
		protected static byte[] encoding;
		protected static ECDomainParameters domainParameters;

		static AccessInfoCryptoBase()
		{
			derivation = Hex.Decode("ce5e47e807b1166ce81d04e15b3fb36a37c364893d21248296528026c4d7a7c5");
			encoding = Hex.Decode("9bb579bd4856cd1a37310d295f473339ba7e6134f5851db0a8ba07e11493a08c");

			var curveParameters = NistNamedCurves.GetByName("P-384");
			domainParameters = new ECDomainParameters(curveParameters.Curve, curveParameters.G, curveParameters.N, curveParameters.H, curveParameters.GetSeed());
		}

		protected static IesEngine CreateCipherEngine(bool encrypt, ICipherParameters privParameters, ICipherParameters pubParameters)
		{
			var engine = new IesEngine(
				new ECDHBasicAgreement(),
				new Kdf2BytesGenerator(new Sha256Digest()),
				new HMac(new Sha256Digest()),
				new PaddedBufferedBlockCipher(new CbcBlockCipher(new AesFastEngine())));

			var parameterSpec = new IesWithCipherParameters(derivation, encoding, 256, 256);
			engine.Init(encrypt, privParameters, pubParameters, parameterSpec);
			return engine;
		}
	}
}
