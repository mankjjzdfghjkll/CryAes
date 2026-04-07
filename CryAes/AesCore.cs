using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryAes
{
    public class AesCore
    {
        private Key key;
        private int Nr; // Nombre de rounds

        public AesCore(byte[] keyBytes)
        {
            key = new Key(keyBytes);
            Nr = key.Nr;
        }

        // Chiffrement d'un bloc
        public byte[] EncryptBlock(byte[] plaintext)
        {
            if (plaintext.Length != 16) throw new ArgumentException("Plaintext must be 16 bytes");

            State state = new State(plaintext);
            
            // Initial round
            state.AddRoundKey(key.GetRoundKey(0));
            
            // Rounds principaux
            for (int round = 1; round < Nr; round++)
            {
                state.SubBytes();
                state.ShiftRows();
                state.MixColumns();
                state.AddRoundKey(key.GetRoundKey(round));
            }
            
            // Final round
            state.SubBytes();
            state.ShiftRows();
            state.AddRoundKey(key.GetRoundKey(Nr));
            
            return state.ToBytes();
        }

        // Déchiffrement d'un bloc
        public byte[] DecryptBlock(byte[] ciphertext)
        {
            if (ciphertext.Length != 16) throw new ArgumentException("Ciphertext must be 16 bytes");

            State state = new State(ciphertext);
            
            // Initial round
            state.AddRoundKey(key.GetRoundKey(Nr));
            
            // Rounds principaux
            for (int round = Nr - 1; round > 0; round--)
            {
                state.InvShiftRows();
                state.InvSubBytes();
                state.AddRoundKey(key.GetRoundKey(round));
                state.InvMixColumns();
            }
            
            // Final round
            state.InvShiftRows();
            state.InvSubBytes();
            state.AddRoundKey(key.GetRoundKey(0));
            
            return state.ToBytes();
        }

        // Mode ECB pour plusieurs blocs
        public byte[] EncryptECB(byte[] plaintext)
        {
            int padding = 16 - (plaintext.Length % 16);
            byte[] padded = new byte[plaintext.Length + padding];
            Array.Copy(plaintext, padded, plaintext.Length);
            for (int i = plaintext.Length; i < padded.Length; i++)
                padded[i] = (byte)padding;

            byte[] ciphertext = new byte[padded.Length];
            for (int i = 0; i < padded.Length; i += 16)
            {
                byte[] block = new byte[16];
                Array.Copy(padded, i, block, 0, 16);
                byte[] encrypted = EncryptBlock(block);
                Array.Copy(encrypted, 0, ciphertext, i, 16);
            }
            return ciphertext;
        }

        public byte[] DecryptECB(byte[] ciphertext)
        {
            byte[] plaintext = new byte[ciphertext.Length];
            for (int i = 0; i < ciphertext.Length; i += 16)
            {
                byte[] block = new byte[16];
                Array.Copy(ciphertext, i, block, 0, 16);
                byte[] decrypted = DecryptBlock(block);
                Array.Copy(decrypted, 0, plaintext, i, 16);
            }
            
            // Remove padding
            int padding = plaintext[plaintext.Length - 1];
            if (padding > 0 && padding <= 16)
            {
                byte[] result = new byte[plaintext.Length - padding];
                Array.Copy(plaintext, result, result.Length);
                return result;
            }
            return plaintext;
        }

        // Mode CBC
        public byte[] EncryptCBC(byte[] plaintext, byte[] iv)
        {
            int padding = 16 - (plaintext.Length % 16);
            byte[] padded = new byte[plaintext.Length + padding];
            Array.Copy(plaintext, padded, plaintext.Length);
            for (int i = plaintext.Length; i < padded.Length; i++)
                padded[i] = (byte)padding;

            byte[] ciphertext = new byte[padded.Length];
            byte[] previous = (byte[])iv.Clone();

            for (int i = 0; i < padded.Length; i += 16)
            {
                byte[] block = new byte[16];
                Array.Copy(padded, i, block, 0, 16);
                
                // XOR with previous ciphertext
                for (int j = 0; j < 16; j++)
                    block[j] ^= previous[j];
                
                byte[] encrypted = EncryptBlock(block);
                Array.Copy(encrypted, 0, ciphertext, i, 16);
                previous = encrypted;
            }
            return ciphertext;
        }

        public byte[] DecryptCBC(byte[] ciphertext, byte[] iv)
        {
            byte[] plaintext = new byte[ciphertext.Length];
            byte[] previous = (byte[])iv.Clone();

            for (int i = 0; i < ciphertext.Length; i += 16)
            {
                byte[] block = new byte[16];
                Array.Copy(ciphertext, i, block, 0, 16);
                
                byte[] decrypted = DecryptBlock(block);
                
                // XOR with previous ciphertext
                for (int j = 0; j < 16; j++)
                    decrypted[j] ^= previous[j];
                
                Array.Copy(decrypted, 0, plaintext, i, 16);
                previous = block;
            }
            
            int padding = plaintext[plaintext.Length - 1];
            if (padding > 0 && padding <= 16)
            {
                byte[] result = new byte[plaintext.Length - padding];
                Array.Copy(plaintext, result, result.Length);
                return result;
            }
            return plaintext;
        }

        // Mode CTR
        public byte[] EncryptCTR(byte[] plaintext, byte[] nonce, long counter)
        {
            byte[] ciphertext = new byte[plaintext.Length];
            byte[] counterBlock = new byte[16];
            
            for (int i = 0; i < plaintext.Length; i++)
            {
                if (i % 16 == 0)
                {
                    Array.Copy(nonce, 0, counterBlock, 0, nonce.Length);
                    byte[] counterBytes = BitConverter.GetBytes(counter + (i / 16));
                    Array.Copy(counterBytes, 0, counterBlock, nonce.Length, counterBytes.Length);
                    
                    byte[] keystream = EncryptBlock(counterBlock);
                    Array.Copy(keystream, 0, counterBlock, 0, 16);
                }
                ciphertext[i] = (byte)(plaintext[i] ^ counterBlock[i % 16]);
            }
            return ciphertext;
        }

        public byte[] DecryptCTR(byte[] ciphertext, byte[] nonce, long counter)
        {
            // CTR mode uses the same operation for encryption and decryption
            return EncryptCTR(ciphertext, nonce, counter);
        }
    }
}