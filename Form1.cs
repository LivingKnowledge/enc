using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Security.Cryptography;

namespace encryptDecrypt
{

	public partial class Form1 : Form
	{
         public static class GlobalVar
         {
             static string _crypt;


             public static string CryptString
             {
                 get
                 {
                     return _crypt;    
                 }
                 set
                 {
                     _crypt = value;

                 }

             }

         }

        public static class StringCipher
        {
        // This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        // This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        // 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        private static readonly byte[] initVectorBytes = Encoding.ASCII.GetBytes("tu89geji340t89u2");

        // This constant is used to determine the keysize of the encryption algorithm.
        private const int keysize = 256;

        public static string Encrypt(string plainText, string passPhrase)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using (RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (ICryptoTransform encryptor = symmetricKey.CreateEncryptor(keyBytes, initVectorBytes))
                    {
                        using (MemoryStream memoryStream = new MemoryStream())
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                byte[] cipherTextBytes = memoryStream.ToArray();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
            using(PasswordDeriveBytes password = new PasswordDeriveBytes(passPhrase, null))
            {
                byte[] keyBytes = password.GetBytes(keysize / 8);
                using(RijndaelManaged symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using(ICryptoTransform decryptor = symmetricKey.CreateDecryptor(keyBytes, initVectorBytes))
                    {
                        using(MemoryStream memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using(CryptoStream cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                byte[] plainTextBytes = new byte[cipherTextBytes.Length];
                                int decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }
        }

		// Path variables for source, encryption, and
		// decryption folders. Must end with a backslash.
        const string EncrFolder = @"C:\Users\Shaun\Documents\Encrypt";
        const string DecrFolder = @"C:\Users\Shaun\Documents\Decrypt\";
        const string SrcFolder = @"C:\Users\Shaun\Documents\test";

		// Public key file
        const string PubKeyFile = @"C:\Users\Shaun\Documents\test\mytest.txt";

		// Key container name for
		// private/public key value pair.
		const string keyName = "MySecretKey";

		CspParameters cspp;
		RSACryptoServiceProvider rsa;

		OpenFileDialog openFileDialog1, openFileDialog2;

		public Form1()
		{
			// Declare CspParmeters and RsaCryptoServiceProvider
			// objects with global scope of your Form class.
			cspp = new CspParameters();

			InitializeComponent();
		}

		private void buttonCreateAsmKeys_Click(object sender, System.EventArgs e)
		{
			// Stores a key pair in the key container.
			cspp.KeyContainerName = keyName;
			rsa = new RSACryptoServiceProvider(cspp);
			rsa.PersistKeyInCsp = true;
			if (rsa.PublicOnly == true)
				label5.Text = "Key: " + cspp.KeyContainerName + " - Public Only";
			else
				label5.Text = "Key: " + cspp.KeyContainerName + " - Full Key Pair";
		}

		private void buttonEncryptFile_Click(object sender, EventArgs e)
		{
			if (rsa == null)
				MessageBox.Show("Key not set.");
			else
			{
				// Display a dialog box to select a file to encrypt.
				openFileDialog1 = new OpenFileDialog();
				openFileDialog1.InitialDirectory = SrcFolder;

				if (openFileDialog1.ShowDialog() == DialogResult.OK)
				{
					string fName = openFileDialog1.FileName;
					if (fName != null)
					{
						FileInfo fInfo = new FileInfo(fName);
                        string f = fInfo.DirectoryName;
						// Pass the file name without the path.
						string name = fInfo.FullName;
                        DirectoryInfo dinfo = new DirectoryInfo(f);
						EncryptFile(name);
					}
				}
			}

		}

        private void ProcessDirectory(DirectoryInfo directoryInfo)
        {
            
        }

        private void ProcessFile(FileInfo info)
        {
           
        }


		private void EncryptFile(string inFile)
		{
			// Create instance of Rijndael for
			// symetric encryption of the data.
			RijndaelManaged rjndl = new RijndaelManaged();
			rjndl.KeySize = 192;
			rjndl.BlockSize = 512;
			rjndl.Mode = CipherMode.CFB;
			ICryptoTransform transform = rjndl.CreateEncryptor();

			// Use RSACryptoServiceProvider to
			// enrypt the Rijndael key.
			// rsa is previously instantiated: 
			//    rsa = new RSACryptoServiceProvider(cspp);
			byte[] keyEncrypted = rsa.Encrypt(rjndl.Key, false);

			// Create byte arrays to contain
			// the length values of the key and IV.
			byte[] LenK = new byte[2];
			byte[] LenIV = new byte[2];

			int lKey = keyEncrypted.Length;
			LenK = BitConverter.GetBytes(lKey);
			int lIV = rjndl.IV.Length;
			LenIV = BitConverter.GetBytes(lIV);

			// Write the following to the FileStream
			// for the encrypted file (outFs):
			// - length of the key
			// - length of the IV
			// - ecrypted key
			// - the IV
			// - the encrypted cipher content

			int startFileName = inFile.LastIndexOf("\\") + 1;
			// Change the file's extension to ".enc"
			string outFile = EncrFolder + inFile.Substring(startFileName, inFile.LastIndexOf(".") - startFileName) + ".enc";

			using (FileStream outFs = new FileStream(outFile, FileMode.Create))
			{

				outFs.Write(LenK, 0, 4);
				outFs.Write(LenIV, 0, 4);
				outFs.Write(keyEncrypted, 0, lKey);
				outFs.Write(rjndl.IV, 0, lIV);

				// Now write the cipher text using
				// a CryptoStream for encrypting.
				using (CryptoStream outStreamEncrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
				{

					// By encrypting a chunk at
					// a time, you can save memory
					// and accommodate large files.
					int count = 0;
					int offset = 0;

					// blockSizeBytes can be any arbitrary size.
					int blockSizeBytes = rjndl.BlockSize / 8;
					byte[] data = new byte[blockSizeBytes];
					int bytesRead = 0;

					using (FileStream inFs = new FileStream(inFile, FileMode.Open))
					{
						do
						{
							count = inFs.Read(data, 0, blockSizeBytes);
							offset += count;
							outStreamEncrypted.Write(data, 0, count);
							bytesRead += blockSizeBytes;
						}
						while (count > 0);
						inFs.Close();
					}
					outStreamEncrypted.FlushFinalBlock();
					outStreamEncrypted.Close();
				}
				outFs.Close();
			}

		}

		private void buttonDecryptFile_Click(object sender, EventArgs e)
		{
			if (rsa == null)
				MessageBox.Show("Key not set.");
			else
			{
				// Display a dialog box to select the encrypted file.
				openFileDialog2 = new OpenFileDialog();
				openFileDialog2.InitialDirectory = EncrFolder;
				if (openFileDialog2.ShowDialog() == DialogResult.OK)
				{
					string fName = openFileDialog2.FileName;
					if (fName != null)
					{
						FileInfo fi = new FileInfo(fName);
						string name = fi.Name;
						DecryptFile(name);
					}
				}
			}
		}

		private void DecryptFile(string inFile)
		{
			// Create instance of Rijndael for
			// symetric decryption of the data.
			RijndaelManaged rjndl = new RijndaelManaged();
			rjndl.KeySize = 192;
			rjndl.BlockSize = 512;
			rjndl.Mode = CipherMode.CFB;

			// Create byte arrays to get the length of
			// the encrypted key and IV.
			// These values were stored as 4 bytes each
			// at the beginning of the encrypted package.
			byte[] LenK = new byte[2];
			byte[] LenIV = new byte[2];

			// Consruct the file name for the decrypted file.
			string outFile = DecrFolder + inFile.Substring(0, inFile.LastIndexOf(".")) + ".txt";

			// Use FileStream objects to read the encrypted
			// file (inFs) and save the decrypted file (outFs).
			using (FileStream inFs = new FileStream(EncrFolder + inFile, FileMode.Open))
			{

				inFs.Seek(0, SeekOrigin.Begin);
				inFs.Seek(0, SeekOrigin.Begin);
				inFs.Read(LenK, 0, 3);
				inFs.Seek(4, SeekOrigin.Begin);
				inFs.Read(LenIV, 0, 3);

				// Convert the lengths to integer values.
				int lenK = BitConverter.ToInt32(LenK, 0);
				int lenIV = BitConverter.ToInt32(LenIV, 0);

				// Determine the start postition of
				// the ciphter text (startC)
				// and its length(lenC).
				int startC = lenK + lenIV + 8;
				int lenC = (int)inFs.Length - startC;

				// Create the byte arrays for
				// the encrypted Rijndael key,
				// the IV, and the cipher text.
				byte[] KeyEncrypted = new byte[lenK];
				byte[] IV = new byte[lenIV];

				// Extract the key and IV
				// starting from index 8
				// after the length values.
				inFs.Seek(8, SeekOrigin.Begin);
				inFs.Read(KeyEncrypted, 0, lenK);
				inFs.Seek(8 + lenK, SeekOrigin.Begin);
				inFs.Read(IV, 0, lenIV);
				Directory.CreateDirectory(DecrFolder);
				// Use RSACryptoServiceProvider
				// to decrypt the Rijndael key.
				byte[] KeyDecrypted = rsa.Decrypt(KeyEncrypted, false);

				// Decrypt the key.
				ICryptoTransform transform = rjndl.CreateDecryptor(KeyDecrypted, IV);

				// Decrypt the cipher text from
				// from the FileSteam of the encrypted
				// file (inFs) into the FileStream
				// for the decrypted file (outFs).
				using (FileStream outFs = new FileStream(outFile, FileMode.Create))
				{
					int count = 0;
					int offset = 0;

					// blockSizeBytes can be any arbitrary size.
					int blockSizeBytes = rjndl.BlockSize / 8;
					byte[] data = new byte[blockSizeBytes];


					// By decrypting a chunk a time,
					// you can save memory and
					// accommodate large files.

					// Start at the beginning
					// of the cipher text.
					inFs.Seek(startC, SeekOrigin.Begin);
					using (CryptoStream outStreamDecrypted = new CryptoStream(outFs, transform, CryptoStreamMode.Write))
					{
						do
						{
							count = inFs.Read(data, 0, blockSizeBytes);
							offset += count;
							outStreamDecrypted.Write(data, 0, count);

						}
						while (count > 0);

						outStreamDecrypted.FlushFinalBlock();
						outStreamDecrypted.Close();
					}
					outFs.Close();
				}
				inFs.Close();
			}
		}

		private void buttonExportPublicKey_Click(object sender, EventArgs e)
		{
			// Save the public key created by the RSA
			// to a file. Caution, persisting the
			// key to a file is a security risk.
			Directory.CreateDirectory(EncrFolder);
			StreamWriter sw = new StreamWriter(PubKeyFile, false);
			sw.Write(rsa.ToXmlString(false));
			sw.Close();

		}

		private void buttonImportPublicKey_Click(object sender, EventArgs e)
		{
			StreamReader sr = new StreamReader(PubKeyFile);
			cspp.KeyContainerName = keyName;
			rsa = new RSACryptoServiceProvider(cspp);
			string keytxt = sr.ReadToEnd();
			rsa.FromXmlString(keytxt);
			rsa.PersistKeyInCsp = true;
			if (rsa.PublicOnly == true)
				label5.Text = "Key: " + cspp.KeyContainerName + " - Public Only";
			else
				label5.Text = "Key: " + cspp.KeyContainerName + " - Full Key Pair";
			sr.Close();

		}

		private void buttonGetPrivateKey_Click(object sender, EventArgs e)
		{
			cspp.KeyContainerName = keyName;

			rsa = new RSACryptoServiceProvider(cspp);
			rsa.PersistKeyInCsp = true;

			if (rsa.PublicOnly == true)
				label5.Text = "Key: " + cspp.KeyContainerName + " - Public Only";
			else
				label5.Text = "Key: " + cspp.KeyContainerName + " - Full Key Pair";

		}

        private void EncryptFolder(object sender, EventArgs e)
        {
            if (rsa == null)
                MessageBox.Show("Key not set.");
            else
            {
                // Display a dialog box to select a file to encrypt.
                openFileDialog1 = new OpenFileDialog();
                openFileDialog1.InitialDirectory = SrcFolder;

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    string fName = openFileDialog1.FileName;
                    if (fName != null)
                    {
                        DirectoryInfo fInfo = new DirectoryInfo(fName);
                        // Pass the file name without the path.
                        string name = fInfo.FullName;
                        EncryptFile(name);
                    }
                }
            }
        }

        private void DecryptFolder(object sender, EventArgs e)
        {
            if (rsa == null)
                MessageBox.Show("Key not set.");
            else
            {
                // Display a dialog box to select the encrypted file.
                openFileDialog2 = new OpenFileDialog();
                openFileDialog2.InitialDirectory = EncrFolder;
                if (openFileDialog2.ShowDialog() == DialogResult.OK)
                {
                    string fName = openFileDialog2.FileName;
                    if (fName != null)
                    {
                        DirectoryInfo fi = new DirectoryInfo(fName);
                        string name = fi.Name;
                        DecryptFile(name);
                    }
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == null)
            {
                textBox2.AppendText(GlobalVar.CryptString);
                //MessageBox.Show("No text set");
                return;
            }
            string clearText = textBox1.Text.Trim();
            label3.Visible = false;
            textBox2.Text = "";
            button1.Enabled = true;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (textBox2.Text == null)
            {
                textBox1.AppendText(GlobalVar.CryptString);
                //MessageBox.Show("No text set");
                return;
            }
            string cipherText = textBox2.Text.Trim();
            button2.Text = "";
            button2.Visible = true;
            label3.Visible = true;  
        }


	}


}
