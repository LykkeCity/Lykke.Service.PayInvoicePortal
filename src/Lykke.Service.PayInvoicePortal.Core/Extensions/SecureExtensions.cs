using System;
using System.IO;
using System.Security.Cryptography;

namespace Lykke.Service.PayInvoicePortal.Core.Extensions
{
    public static class SecureExtensions
    {
        public static RSA CreateRsa(this string src)
        {
            var privateKey = src
                .Replace("-----BEGIN RSA PRIVATE KEY-----", string.Empty)
                .Replace("-----END RSA PRIVATE KEY-----", string.Empty)
                .Replace("\n", string.Empty).Replace("\r", string.Empty);

            var rsaParams = new RSAParameters();

            using (var binr = new BinaryReader(new MemoryStream(Convert.FromBase64String(privateKey))))
            {
                byte bt = 0;
                ushort twobytes = 0;
                twobytes = binr.ReadUInt16();
                if (twobytes == 0x8130)
                    binr.ReadByte();
                else if (twobytes == 0x8230)
                    binr.ReadInt16();
                else
                    throw new Exception("Unexpected value read binr.ReadUInt16()");

                twobytes = binr.ReadUInt16();
                if (twobytes != 0x0102)
                    throw new Exception("Unexpected version");

                bt = binr.ReadByte();
                if (bt != 0x00)
                    throw new Exception("Unexpected value read binr.ReadByte()");

                rsaParams.Modulus = binr.ReadBytes(binr.GetIntegerSize());
                rsaParams.Exponent = binr.ReadBytes(binr.GetIntegerSize());
                rsaParams.D = binr.ReadBytes(binr.GetIntegerSize());
                rsaParams.P = binr.ReadBytes(binr.GetIntegerSize());
                rsaParams.Q = binr.ReadBytes(binr.GetIntegerSize());
                rsaParams.DP = binr.ReadBytes(binr.GetIntegerSize());
                rsaParams.DQ = binr.ReadBytes(binr.GetIntegerSize());
                rsaParams.InverseQ = binr.ReadBytes(binr.GetIntegerSize());
            }

            return RSA.Create(rsaParams);
        }

        public static int GetIntegerSize(this BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();
            else
            if (bt == 0x82)
            {
                highbyte = binr.ReadByte();
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;
            }

            while (binr.ReadByte() == 0x00)
            {
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);
            return count;
        }
    }
}
