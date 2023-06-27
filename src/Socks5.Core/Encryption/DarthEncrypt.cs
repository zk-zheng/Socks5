/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.ComponentModel;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

namespace Socks5.Core.Encryption;

public class DarthEncrypt
{
    // Nested Types
    public enum DcHashTypes
    {
        // Fields
        Sha1 = 0,
        Sha256 = 1,
        Sha384 = 2,
        Sha512 = 3
    }

    // Fields
    //private string _fileDecryptExtension;
    //private string _fileEncryptExtension;
    private string _initVector;
    private int _passPhraseStrength;

    public DarthEncrypt()
    {
        InitializeComponent();
        if (PassPhrase == null) 
            PassPhrase = "Z29sZGZpc2ggYm93bA==";
        if (SaltValue == null) 
            SaltValue = "ZGlhbW9uZCByaW5n";
        HashType = DcHashTypes.Sha1;
        if (_initVector == null) 
            _initVector = "@1B2c3D4e5F6g7H8";
        _passPhraseStrength = 2;
    }

    [Category("Encryption Options")]
    [Description("The type of HASH you want to use to aid RijndaelManaged transformations")]
    public DcHashTypes HashType { get; set; }

    [Category("Encryption Options")]
    [Description("The initialization vector to use (must be 16 chars)")]
    public string InitVector
    {
        get => _initVector;
        set
        {
            if (value.Length != 0x10)
                _initVector = "@1B2c3D4e5F6g7H8";
            else
                _initVector = value;
        }
    }

    [Description("The secret pass phrase to use for encryption and decryption")]
    [Category("Encryption Options")]
    public string PassPhrase { get; set; }

    [Category("Encryption Options")]
    [Description("The Pass Phrase strength (5 high, 1 low)")]
    public int PassPhraseStrength
    {
        get => _passPhraseStrength;
        set
        {
            if (value > 5)
                _passPhraseStrength = 2;
            else
                _passPhraseStrength = value;
        }
    }

    [Category("Encryption Options")]
    [Description("The salt value used to foil hackers attempting to crack the encryption")]
    public string SaltValue { get; set; }

    public byte[] CompressBytes(byte[] bytes, int offset, int count)
    {
        using (var memory = new MemoryStream())
        {
            using (var gzip = new GZipStream(memory, CompressionMode.Compress, true))
            {
                gzip.Write(bytes, offset, count);
            }

            return memory.ToArray();
        }
    }

    public byte[] DecompressBytes(byte[] compressed)
    {
        byte[] buffer2;
        using (var stream = new GZipStream(new MemoryStream(compressed), CompressionMode.Decompress))
        {
            var buffer = new byte[0x1000];
            using (var stream2 = new MemoryStream())
            {
                var count = 0;
                do
                {
                    count = stream.Read(buffer, 0, 0x1000);
                    if (count > 0) stream2.Write(buffer, 0, count);
                } while (count > 0);

                buffer2 = stream2.ToArray();
            }
        }

        return buffer2;
    }

    public byte[] DecryptBytes(byte[] encryptedBytes)
    {
        var initVector = InitVector;
        var num = 0x100;
        var bytes = Encoding.ASCII.GetBytes(initVector);
        var rgbSalt = Encoding.ASCII.GetBytes(SaltValue);
        var buffer = encryptedBytes;
        var strHashName = "SHA1";
        if (HashType == DcHashTypes.Sha1) strHashName = "SHA1";
        if (HashType == DcHashTypes.Sha256) strHashName = "SHA256";
        if (HashType == DcHashTypes.Sha384) strHashName = "SHA384";
        if (HashType == DcHashTypes.Sha512) strHashName = "SHA512";
        var rgbKey = new PasswordDeriveBytes(PassPhrase, rgbSalt, strHashName, PassPhraseStrength).GetBytes(num / 8);
        var managed = new RijndaelManaged();
        managed.Mode = CipherMode.CBC;
        managed.Padding = PaddingMode.Zeros;
        var transform = managed.CreateDecryptor(rgbKey, bytes);
        var stream = new MemoryStream(buffer);
        var stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Read);
        var buffer5 = new byte[buffer.Length];
        var count = stream2.Read(buffer5, 0, buffer5.Length);
        stream.Close();
        stream2.Close();
        return buffer5;
    }

    public byte[] EncryptBytes(byte[] bytearray)
    {
        var initVector = InitVector;
        var num = 0x100;
        var bytes = Encoding.ASCII.GetBytes(initVector);
        var rgbSalt = Encoding.ASCII.GetBytes(SaltValue);
        var buffer = bytearray;
        var strHashName = "SHA1";
        if (HashType == DcHashTypes.Sha1) strHashName = "SHA1";
        if (HashType == DcHashTypes.Sha256) strHashName = "SHA256";
        if (HashType == DcHashTypes.Sha384) strHashName = "SHA384";
        if (HashType == DcHashTypes.Sha512) strHashName = "SHA512";
        var rgbKey = new PasswordDeriveBytes(PassPhrase, rgbSalt, strHashName, PassPhraseStrength).GetBytes(num / 8);
        var managed = new RijndaelManaged();
        managed.Mode = CipherMode.CBC;
        managed.Padding = PaddingMode.Zeros;
        var transform = managed.CreateEncryptor(rgbKey, bytes);
        var stream = new MemoryStream();
        var stream2 = new CryptoStream(stream, transform, CryptoStreamMode.Write);
        stream2.Write(buffer, 0, buffer.Length);
        stream2.FlushFinalBlock();
        var inArray = stream.ToArray();
        stream.Close();
        stream2.Close();
        return inArray;
    }

    private void InitializeComponent()
    {
    }
}