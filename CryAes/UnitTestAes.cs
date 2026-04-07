using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CryAes.Tests
{
    [TestClass]
    public class UnitTestAes
    {
        // ============ FONCTIONS UTILITAIRES ============
        private byte[] HexToBytes(string hex)
        {
            hex = hex.Replace(" ", "").Replace("-", "");
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private string BytesToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        // ============ TEST 1 : VECTEUR NIST AES-128 ============
        [TestMethod]
        public void TestNISTVector128()
        {
            Console.WriteLine("=== TEST NIST AES-128 ===");
            
            byte[] key = HexToBytes("2b7e151628aed2a6abf7158809cf4f3c");
            byte[] plaintext = HexToBytes("6bc1bee22e409f96e93d7e117393172a");
            byte[] expected = HexToBytes("3ad77bb40d7a3660a89ecaf32466ef97");
            
            AesCore aes = new AesCore(key);
            byte[] ciphertext = aes.EncryptBlock(plaintext);
            
            Console.WriteLine($"Clé       : {BytesToHex(key)}");
            Console.WriteLine($"Plaintext : {BytesToHex(plaintext)}");
            Console.WriteLine($"Attendu   : {BytesToHex(expected)}");
            Console.WriteLine($"Obtenu    : {BytesToHex(ciphertext)}");
            
            CollectionAssert.AreEqual(expected, ciphertext, "Le chiffrement ne correspond pas au vecteur NIST");
            
            // Vérifier le déchiffrement
            byte[] decrypted = aes.DecryptBlock(ciphertext);
            CollectionAssert.AreEqual(plaintext, decrypted, "Le déchiffrement ne restitue pas le texte original");
            
            Console.WriteLine("✅ Test NIST AES-128 réussi !\n");
        }

        // ============ TEST 2 : AES-192 ============
        [TestMethod]
        public void TestAES192()
        {
            Console.WriteLine("=== TEST AES-192 ===");
            
            byte[] key = HexToBytes("8e73b0f7da0e6452c810f32b809079e562f8ead2522c6b7b");
            byte[] plaintext = HexToBytes("6bc1bee22e409f96e93d7e117393172a");
            byte[] expected = HexToBytes("bd334f1d6e45f25ff712a214571fa5cc");
            
            AesCore aes = new AesCore(key);
            byte[] ciphertext = aes.EncryptBlock(plaintext);
            
            Console.WriteLine($"Clé       : {BytesToHex(key)}");
            Console.WriteLine($"Plaintext : {BytesToHex(plaintext)}");
            Console.WriteLine($"Attendu   : {BytesToHex(expected)}");
            Console.WriteLine($"Obtenu    : {BytesToHex(ciphertext)}");
            
            CollectionAssert.AreEqual(expected, ciphertext);
            
            // Vérifier déchiffrement
            byte[] decrypted = aes.DecryptBlock(ciphertext);
            CollectionAssert.AreEqual(plaintext, decrypted);
            
            Console.WriteLine("✅ AES-192 réussi !\n");
        }

        // ============ TEST 3 : AES-256 ============
        [TestMethod]
        public void TestAES256()
        {
            Console.WriteLine("=== TEST AES-256 ===");
            
            byte[] key = HexToBytes("603deb1015ca71be2b73aef0857d77811f352c073b6108d72d9810a30914dff4");
            byte[] plaintext = HexToBytes("6bc1bee22e409f96e93d7e117393172a");
            byte[] expected = HexToBytes("f3eed1bdb5d2a03c064b5a7e3db181f8");
            
            AesCore aes = new AesCore(key);
            byte[] ciphertext = aes.EncryptBlock(plaintext);
            
            Console.WriteLine($"Clé       : {BytesToHex(key)}");
            Console.WriteLine($"Plaintext : {BytesToHex(plaintext)}");
            Console.WriteLine($"Attendu   : {BytesToHex(expected)}");
            Console.WriteLine($"Obtenu    : {BytesToHex(ciphertext)}");
            
            CollectionAssert.AreEqual(expected, ciphertext);
            
            byte[] decrypted = aes.DecryptBlock(ciphertext);
            CollectionAssert.AreEqual(plaintext, decrypted);
            
            Console.WriteLine("✅ AES-256 réussi !\n");
        }

        // ============ TEST 4 : MODE CBC ============
        [TestMethod]
        public void TestCBCMode()
        {
            Console.WriteLine("=== TEST MODE CBC ===");
            
            byte[] key = HexToBytes("2b7e151628aed2a6abf7158809cf4f3c");
            byte[] iv = new byte[16]; // IV initialisé à zéro
            string plaintext = "Hello UNH! This is a test message for CBC mode.";
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            
            Console.WriteLine($"Texte original : {plaintext}");
            Console.WriteLine($"Taille : {plaintextBytes.Length} bytes");
            
            AesCore aes = new AesCore(key);
            
            // Chiffrement
            byte[] ciphertext = aes.EncryptCBC(plaintextBytes, iv);
            Console.WriteLine($"Texte chiffré (hex) : {BytesToHex(ciphertext).Substring(0, 50)}...");
            
            // Déchiffrement
            byte[] decrypted = aes.DecryptCBC(ciphertext, iv);
            string result = Encoding.UTF8.GetString(decrypted);
            
            Console.WriteLine($"Texte déchiffré : {result}");
            
            Assert.AreEqual(plaintext, result, "Le texte déchiffré ne correspond pas à l'original");
            
            Console.WriteLine("✅ Mode CBC réussi !\n");
        }

        // ============ TEST 5 : MODE CTR ============
        [TestMethod]
        public void TestCTRMode()
        {
            Console.WriteLine("=== TEST MODE CTR ===");
            
            byte[] key = HexToBytes("2b7e151628aed2a6abf7158809cf4f3c");
            byte[] nonce = new byte[8]; // Nonce initialisé à zéro
            long counter = 0;
            string plaintext = "CTR mode allows parallel encryption! This is a test.";
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            
            Console.WriteLine($"Texte original : {plaintext}");
            Console.WriteLine($"Taille : {plaintextBytes.Length} bytes");
            
            AesCore aes = new AesCore(key);
            
            // Chiffrement (identique au déchiffrement pour CTR)
            byte[] ciphertext = aes.EncryptCTR(plaintextBytes, nonce, counter);
            Console.WriteLine($"Texte chiffré (hex) : {BytesToHex(ciphertext).Substring(0, 50)}...");
            
            // Déchiffrement
            byte[] decrypted = aes.DecryptCTR(ciphertext, nonce, counter);
            string result = Encoding.UTF8.GetString(decrypted);
            
            Console.WriteLine($"Texte déchiffré : {result}");
            
            Assert.AreEqual(plaintext, result, "Le texte déchiffré ne correspond pas à l'original");
            
            Console.WriteLine("✅ Mode CTR réussi !\n");
        }

        // ============ TEST 6 : MODE ECB MULTI-BLOC ============
        [TestMethod]
        public void TestECBMultiBlock()
        {
            Console.WriteLine("=== TEST ECB MULTI-BLOC ===");
            
            byte[] key = HexToBytes("2b7e151628aed2a6abf7158809cf4f3c");
            string plaintext = "This is a longer message that spans multiple 16-byte blocks for testing ECB mode encryption and decryption.";
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            
            Console.WriteLine($"Texte original : {plaintext}");
            Console.WriteLine($"Taille : {plaintextBytes.Length} bytes ({Math.Ceiling(plaintextBytes.Length / 16.0)} blocs)");
            
            AesCore aes = new AesCore(key);
            
            // Chiffrement ECB
            byte[] ciphertext = aes.EncryptECB(plaintextBytes);
            Console.WriteLine($"Texte chiffré taille : {ciphertext.Length} bytes");
            
            // Déchiffrement ECB
            byte[] decrypted = aes.DecryptECB(ciphertext);
            string result = Encoding.UTF8.GetString(decrypted);
            
            Console.WriteLine($"Texte déchiffré : {result}");
            
            Assert.AreEqual(plaintext, result);
            
            Console.WriteLine("✅ ECB multi-bloc réussi !\n");
        }

        // ============ TEST 7 : CHIFFREMENT FICHIER ============
        [TestMethod]
        public void TestFileEncryption()
        {
            Console.WriteLine("=== TEST CHIFFREMENT FICHIER ===");
            
            // Créer un fichier test temporaire
            string testFile = "test.txt";
            string encryptedFile = "test.enc";
            string decryptedFile = "test.dec.txt";
            
            string content = "This is a test file content for AES encryption!";
            System.IO.File.WriteAllText(testFile, content);
            
            byte[] key = HexToBytes("2b7e151628aed2a6abf7158809cf4f3c");
            byte[] fileData = System.IO.File.ReadAllBytes(testFile);
            
            AesCore aes = new AesCore(key);
            
            // Chiffrer
            byte[] encrypted = aes.EncryptECB(fileData);
            System.IO.File.WriteAllBytes(encryptedFile, encrypted);
            
            // Déchiffrer
            byte[] decrypted = aes.DecryptECB(encrypted);
            System.IO.File.WriteAllBytes(decryptedFile, decrypted);
            
            string result = System.IO.File.ReadAllText(decryptedFile);
            
            Console.WriteLine($"Contenu original : {content}");
            Console.WriteLine($"Contenu déchiffré : {result}");
            
            Assert.AreEqual(content, result);
            
            // Nettoyer
            System.IO.File.Delete(testFile);
            System.IO.File.Delete(encryptedFile);
            System.IO.File.Delete(decryptedFile);
            
            Console.WriteLine("✅ Chiffrement fichier réussi !\n");
        }

        // ============ TEST 8 : COMPARAISON DES MODES ============
        [TestMethod]
        public void CompareModes()
        {
            Console.WriteLine("\n=== COMPARAISON DES MODES ===");
            
            byte[] key = HexToBytes("2b7e151628aed2a6abf7158809cf4f3c");
            string plaintext = "AAAAAAAABBBBBBBB"; // 16 bytes, bloc identique
            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
            byte[] iv = new byte[16];
            byte[] nonce = new byte[8];
            
            AesCore aes = new AesCore(key);
            
            // ECB - blocs identiques donnent chiffrés identiques
            byte[] ecbCipher = aes.EncryptECB(plaintextBytes);
            
            // CBC - blocs identiques donnent chiffrés différents
            byte[] cbcCipher = aes.EncryptCBC(plaintextBytes, iv);
            
            // CTR - mode compteur
            byte[] ctrCipher = aes.EncryptCTR(plaintextBytes, nonce, 0);
            
            Console.WriteLine("AVANTAGES ET INCONVÉNIENTS :");
            Console.WriteLine("┌─────────┬────────────────────────────┬────────────────────────────┐");
            Console.WriteLine("│ Mode    │ Avantages                   │ Inconvénients              │");
            Console.WriteLine("├─────────┼────────────────────────────┼────────────────────────────┤");
            Console.WriteLine("│ ECB     │ Simple, parallélisable     │ Identique pour blocs ident.│");
            Console.WriteLine("│ CBC     │ Plus sécurisé              │ Séquentiel, besoin IV      │");
            Console.WriteLine("│ CTR     │ Parallélisable, random acc.│ Besoin nonce unique        │");
            Console.WriteLine("└─────────┴────────────────────────────┴────────────────────────────┘");
            
            Console.WriteLine("\n✅ Comparaison terminée !\n");
        }
    }
}