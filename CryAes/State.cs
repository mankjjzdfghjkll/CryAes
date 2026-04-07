using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryAes
{
    public class State
    {
        public byte[,] state = new byte[4, 4];

        // Constructeurs
        public State() { }

        public State(byte[] data)
        {
            if (data.Length != 16) throw new ArgumentException("Data must be 16 bytes");
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    state[j, i] = data[i * 4 + j];
        }

        public byte[] ToBytes()
        {
            byte[] result = new byte[16];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    result[i * 4 + j] = state[j, i];
            return result;
        }

        // Chiffrement: SubBytes
        public void SubBytes()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    state[i, j] = Sbox.SubByte(state[i, j]);
        }

        // Déchiffrement: InvSubBytes
        public void InvSubBytes()
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    state[i, j] = Sbox.InvSubByte(state[i, j]);
        }

        // Chiffrement: ShiftRows
        public void ShiftRows()
        {
            byte[] temp = new byte[4];
            // Ligne 1: décalage de 1
            for (int i = 0; i < 4; i++) temp[i] = state[1, (i + 1) % 4];
            for (int i = 0; i < 4; i++) state[1, i] = temp[i];
            // Ligne 2: décalage de 2
            for (int i = 0; i < 4; i++) temp[i] = state[2, (i + 2) % 4];
            for (int i = 0; i < 4; i++) state[2, i] = temp[i];
            // Ligne 3: décalage de 3
            for (int i = 0; i < 4; i++) temp[i] = state[3, (i + 3) % 4];
            for (int i = 0; i < 4; i++) state[3, i] = temp[i];
        }

        // Déchiffrement: InvShiftRows
        public void InvShiftRows()
        {
            byte[] temp = new byte[4];
            // Ligne 1: décalage inverse de 1
            for (int i = 0; i < 4; i++) temp[(i + 1) % 4] = state[1, i];
            for (int i = 0; i < 4; i++) state[1, i] = temp[i];
            // Ligne 2: décalage inverse de 2
            for (int i = 0; i < 4; i++) temp[(i + 2) % 4] = state[2, i];
            for (int i = 0; i < 4; i++) state[2, i] = temp[i];
            // Ligne 3: décalage inverse de 3
            for (int i = 0; i < 4; i++) temp[(i + 3) % 4] = state[3, i];
            for (int i = 0; i < 4; i++) state[3, i] = temp[i];
        }

        // Multiplication dans GF(2^8)
        private byte GMul(byte a, byte b)
        {
            byte p = 0;
            for (int i = 0; i < 8; i++)
            {
                if ((b & 1) != 0) p ^= a;
                bool hiBitSet = (a & 0x80) != 0;
                a <<= 1;
                if (hiBitSet) a ^= 0x1B;
                b >>= 1;
            }
            return p;
        }

        // Chiffrement: MixColumns
        public void MixColumns()
        {
            for (int c = 0; c < 4; c++)
            {
                byte[] col = new byte[4];
                for (int r = 0; r < 4; r++)
                    col[r] = state[r, c];

                state[0, c] = (byte)(GMul(0x02, col[0]) ^ GMul(0x03, col[1]) ^ col[2] ^ col[3]);
                state[1, c] = (byte)(col[0] ^ GMul(0x02, col[1]) ^ GMul(0x03, col[2]) ^ col[3]);
                state[2, c] = (byte)(col[0] ^ col[1] ^ GMul(0x02, col[2]) ^ GMul(0x03, col[3]));
                state[3, c] = (byte)(GMul(0x03, col[0]) ^ col[1] ^ col[2] ^ GMul(0x02, col[3]));
            }
        }

        // Déchiffrement: InvMixColumns
        public void InvMixColumns()
        {
            for (int c = 0; c < 4; c++)
            {
                byte[] col = new byte[4];
                for (int r = 0; r < 4; r++)
                    col[r] = state[r, c];

                state[0, c] = (byte)(GMul(0x0E, col[0]) ^ GMul(0x0B, col[1]) ^ GMul(0x0D, col[2]) ^ GMul(0x09, col[3]));
                state[1, c] = (byte)(GMul(0x09, col[0]) ^ GMul(0x0E, col[1]) ^ GMul(0x0B, col[2]) ^ GMul(0x0D, col[3]));
                state[2, c] = (byte)(GMul(0x0D, col[0]) ^ GMul(0x09, col[1]) ^ GMul(0x0E, col[2]) ^ GMul(0x0B, col[3]));
                state[3, c] = (byte)(GMul(0x0B, col[0]) ^ GMul(0x0D, col[1]) ^ GMul(0x09, col[2]) ^ GMul(0x0E, col[3]));
            }
        }

        // AddRoundKey
        public void AddRoundKey(byte[,] roundKey)
        {
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    state[i, j] ^= roundKey[i, j];
        }
    }
}