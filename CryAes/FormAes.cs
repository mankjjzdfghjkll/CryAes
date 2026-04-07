using System;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace CryAes
{
    public partial class FormAes : Form
    {
        // Contrôles de l'interface
        private TextBox txtKey;
        private TextBox txtPlaintext;
        private TextBox txtCiphertext;
        private ComboBox cmbMode;
        private ComboBox cmbKeySize;
        private Button btnEncrypt;
        private Button btnDecrypt;
        private Button btnClear;
        private Button btnFileEncrypt;
        private Button btnFileDecrypt;
        private Label lblStatus;
        private ProgressBar progressBar;
        private CheckBox chkParallel;
        private Label lblModeInfo;
        private GroupBox grpInput;
        private GroupBox grpOutput;
        private GroupBox grpOptions;
        private GroupBox grpFile;
        private RadioButton rdoText;
        private RadioButton rdoHex;
        private TextBox txtIV;
        private Label lblIV;
        private NumericUpDown nudParallelism;

        public FormAes()
        {
            InitializeComponent();
            InitializeCustomComponents();
        }

        private void InitializeCustomComponents()
        {
            // Configuration de la fenêtre principale
            this.Text = "AES Encryption Tool - UNH Genie Logiciel";
            this.Size = new Size(900, 700);
            this.BackColor = Color.FromArgb(240, 242, 245);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Icon = null; // Optionnel: ajouter une icône

            // ============ GROUP BOX OPTIONS ============
            grpOptions = new GroupBox()
            {
                Text = "🔐 Configuration AES",
                Location = new Point(12, 12),
                Size = new Size(860, 120),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Clé (hex)
            Label lblKey = new Label()
            {
                Text = "Clé (hexadécimale) :",
                Location = new Point(15, 30),
                Size = new Size(130, 25),
                Font = new Font("Segoe UI", 9)
            };
            txtKey = new TextBox()
            {
                Location = new Point(150, 28),
                Size = new Size(350, 25),
                Font = new Font("Courier New", 9),
                Text = "2b7e151628aed2a6abf7158809cf4f3c"
            };

            // Taille de la clé
            Label lblKeySize = new Label()
            {
                Text = "Taille clé :",
                Location = new Point(520, 30),
                Size = new Size(70, 25),
                Font = new Font("Segoe UI", 9)
            };
            cmbKeySize = new ComboBox()
            {
                Location = new Point(595, 28),
                Size = new Size(80, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbKeySize.Items.AddRange(new object[] { "128 bits", "192 bits", "256 bits" });
            cmbKeySize.SelectedIndex = 0;
            cmbKeySize.SelectedIndexChanged += CmbKeySize_SelectedIndexChanged;

            // Mode de chiffrement
            Label lblMode = new Label()
            {
                Text = "Mode :",
                Location = new Point(15, 65),
                Size = new Size(50, 25),
                Font = new Font("Segoe UI", 9)
            };
            cmbMode = new ComboBox()
            {
                Location = new Point(70, 63),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbMode.Items.AddRange(new object[] { "ECB", "CBC", "CTR" });
            cmbMode.SelectedIndex = 0;
            cmbMode.SelectedIndexChanged += CmbMode_SelectedIndexChanged;

            // IV pour CBC
            lblIV = new Label()
            {
                Text = "IV/Nonce :",
                Location = new Point(190, 65),
                Size = new Size(70, 25),
                Font = new Font("Segoe UI", 9),
                Visible = false
            };
            txtIV = new TextBox()
            {
                Location = new Point(265, 63),
                Size = new Size(200, 25),
                Font = new Font("Courier New", 9),
                Text = "00000000000000000000000000000000",
                Visible = false
            };

            // Parallélisme
            chkParallel = new CheckBox()
            {
                Text = "Mode parallèle (Task-based)",
                Location = new Point(490, 63),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 9),
                Checked = true
            };
            Label lblParallel = new Label()
            {
                Text = "Threads:",
                Location = new Point(680, 65),
                Size = new Size(50, 25),
                Font = new Font("Segoe UI", 8)
            };
            nudParallelism = new NumericUpDown()
            {
                Location = new Point(735, 63),
                Size = new Size(50, 25),
                Minimum = 1,
                Maximum = 8,
                Value = 4,
                Font = new Font("Segoe UI", 9)
            };

            grpOptions.Controls.AddRange(new Control[] { lblKey, txtKey, lblKeySize, cmbKeySize, 
                lblMode, cmbMode, lblIV, txtIV, chkParallel, lblParallel, nudParallelism });

            // ============ GROUP BOX INPUT ============
            grpInput = new GroupBox()
            {
                Text = "📝 Texte à chiffrer",
                Location = new Point(12, 140),
                Size = new Size(860, 150),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            rdoText = new RadioButton()
            {
                Text = "Texte (ASCII/UTF-8)",
                Location = new Point(15, 25),
                Size = new Size(150, 25),
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };
            rdoHex = new RadioButton()
            {
                Text = "Hexadécimal",
                Location = new Point(180, 25),
                Size = new Size(120, 25),
                Font = new Font("Segoe UI", 9)
            };
            rdoText.CheckedChanged += RdoInput_CheckedChanged;
            rdoHex.CheckedChanged += RdoInput_CheckedChanged;

            txtPlaintext = new TextBox()
            {
                Location = new Point(15, 55),
                Size = new Size(830, 80),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 10),
                Text = "The quick brown fox jumps over the lazy dog. This is a test message for AES encryption! This demonstrates the full functionality of our implementation."
            };

            grpInput.Controls.AddRange(new Control[] { rdoText, rdoHex, txtPlaintext });

            // ============ BOUTONS D'ACTION ============
            btnEncrypt = new Button()
            {
                Text = "🔒 CHIFFRER",
                Location = new Point(12, 300),
                Size = new Size(130, 45),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnEncrypt.Click += BtnEncrypt_Click;

            btnDecrypt = new Button()
            {
                Text = "🔓 DÉCHIFFRER",
                Location = new Point(150, 300),
                Size = new Size(130, 45),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnDecrypt.Click += BtnDecrypt_Click;

            btnClear = new Button()
            {
                Text = "🗑️ EFFACER",
                Location = new Point(290, 300),
                Size = new Size(100, 45),
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnClear.Click += (s, e) => { txtPlaintext.Clear(); txtCiphertext.Clear(); };

            // ============ GROUP BOX FILE ============
            grpFile = new GroupBox()
            {
                Text = "📁 Chiffrement de fichiers",
                Location = new Point(410, 300),
                Size = new Size(460, 70),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            btnFileEncrypt = new Button()
            {
                Text = "📂 Chiffrer un fichier",
                Location = new Point(15, 25),
                Size = new Size(140, 35),
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat
            };
            btnFileEncrypt.Click += BtnFileEncrypt_Click;

            btnFileDecrypt = new Button()
            {
                Text = "📂 Déchiffrer un fichier",
                Location = new Point(170, 25),
                Size = new Size(140, 35),
                Font = new Font("Segoe UI", 9),
                FlatStyle = FlatStyle.Flat
            };
            btnFileDecrypt.Click += BtnFileDecrypt_Click;

            grpFile.Controls.AddRange(new Control[] { btnFileEncrypt, btnFileDecrypt });

            // ============ GROUP BOX OUTPUT ============
            grpOutput = new GroupBox()
            {
                Text = "📤 Résultat (hexadécimal)",
                Location = new Point(12, 360),
                Size = new Size(860, 150),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            txtCiphertext = new TextBox()
            {
                Location = new Point(15, 30),
                Size = new Size(830, 105),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Courier New", 9),
                ReadOnly = false
            };

            grpOutput.Controls.Add(txtCiphertext);

            // ============ STATUS BAR ============
            lblStatus = new Label()
            {
                Text = "✅ Prêt - Entrez une clé et un texte à chiffrer",
                Location = new Point(12, 520),
                Size = new Size(700, 30),
                Font = new Font("Segoe UI", 9),
                ForeColor = Color.Green
            };

            progressBar = new ProgressBar()
            {
                Location = new Point(12, 555),
                Size = new Size(860, 20),
                Visible = false,
                Style = ProgressBarStyle.Marquee
            };

            // ============ MODE INFO LABEL ============
            lblModeInfo = new Label()
            {
                Text = "ℹ️ Mode ECB : Simple mais peu sécurisé (blocs identiques = chiffrés identiques)",
                Location = new Point(12, 585),
                Size = new Size(860, 25),
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                ForeColor = Color.Gray
            };

            // Ajout de tous les contrôles
            this.Controls.AddRange(new Control[] { 
                grpOptions, grpInput, btnEncrypt, btnDecrypt, btnClear, 
                grpFile, grpOutput, lblStatus, progressBar, lblModeInfo 
            });
        }

        private void CmbKeySize_SelectedIndexChanged(object sender, EventArgs e)
        {
            int size = cmbKeySize.SelectedIndex;
            if (size == 0) // 128 bits
                txtKey.Text = "2b7e151628aed2a6abf7158809cf4f3c";
            else if (size == 1) // 192 bits
                txtKey.Text = "8e73b0f7da0e6452c810f32b809079e562f8ead2522c6b7b";
            else // 256 bits
                txtKey.Text = "603deb1015ca71be2b73aef0857d77811f352c073b6108d72d9810a30914dff4";
        }

        private void CmbMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            string mode = cmbMode.SelectedItem.ToString();
            lblIV.Visible = (mode != "ECB");
            txtIV.Visible = (mode != "ECB");

            if (mode == "ECB")
            {
                lblModeInfo.Text = "ℹ️ Mode ECB : Simple mais peu sécurisé (blocs identiques = chiffrés identiques)";
                lblModeInfo.ForeColor = Color.Orange;
            }
            else if (mode == "CBC")
            {
                lblModeInfo.Text = "ℹ️ Mode CBC : Plus sécurisé, nécessite un IV. Blocs identiques = chiffrés différents.";
                lblModeInfo.ForeColor = Color.Green;
            }
            else if (mode == "CTR")
            {
                lblModeInfo.Text = "ℹ️ Mode CTR : Parallélisable, accès aléatoire. Idéal pour gros volumes.";
                lblModeInfo.ForeColor = Color.Blue;
            }
        }

        private void RdoInput_CheckedChanged(object sender, EventArgs e)
        {
            if (rdoHex.Checked)
            {
                // Convertir le texte en hex si nécessaire
                try
                {
                    string currentText = txtPlaintext.Text;
                    byte[] bytes = Encoding.UTF8.GetBytes(currentText);
                    txtPlaintext.Text = BitConverter.ToString(bytes).Replace("-", " ");
                }
                catch { }
            }
        }

        private byte[] StringToHex(string hex)
        {
            hex = hex.Replace(" ", "").Replace("-", "");
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < hex.Length; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        private string HexToString(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", " ");
        }

        private byte[] GetKeyFromInput()
        {
            string hexKey = txtKey.Text.Replace(" ", "").Replace("-", "");
            byte[] key = new byte[hexKey.Length / 2];
            for (int i = 0; i < hexKey.Length; i += 2)
                key[i / 2] = Convert.ToByte(hexKey.Substring(i, 2), 16);
            return key;
        }

        private async void BtnEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                btnEncrypt.Enabled = false;
                lblStatus.Text = "⏳ Chiffrement en cours...";
                progressBar.Visible = true;

                byte[] key = GetKeyFromInput();
                byte[] plaintext;

                if (rdoHex.Checked)
                {
                    plaintext = StringToHex(txtPlaintext.Text);
                }
                else
                {
                    plaintext = Encoding.UTF8.GetBytes(txtPlaintext.Text);
                }

                byte[] ciphertext = await Task.Run(() =>
                {
                    AesCore aes = new AesCore(key);
                    string mode = cmbMode.SelectedItem.ToString();

                    if (mode == "ECB")
                        return aes.EncryptECB(plaintext);
                    else if (mode == "CBC")
                    {
                        byte[] iv = StringToHex(txtIV.Text);
                        return aes.EncryptCBC(plaintext, iv);
                    }
                    else
                    {
                        byte[] nonce = StringToHex(txtIV.Text.Substring(0, 16));
                        return aes.EncryptCTR(plaintext, nonce, 0);
                    }
                });

                txtCiphertext.Text = HexToString(ciphertext);
                lblStatus.Text = "✅ Chiffrement terminé avec succès !";
                lblStatus.ForeColor = Color.Green;
                progressBar.Visible = false;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌ Erreur : {ex.Message}";
                lblStatus.ForeColor = Color.Red;
                progressBar.Visible = false;
                MessageBox.Show($"Erreur lors du chiffrement : {ex.Message}", "Erreur", 
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnEncrypt.Enabled = true;
            }
        }

        private async void BtnDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                btnDecrypt.Enabled = false;
                lblStatus.Text = "⏳ Déchiffrement en cours...";
                progressBar.Visible = true;

                byte[] key = GetKeyFromInput();
                string[] hexValues = txtCiphertext.Text.Split(new char[] { ' ', '\t', '\n' }, 
                    StringSplitOptions.RemoveEmptyEntries);
                byte[] ciphertext = new byte[hexValues.Length];
                for (int i = 0; i < hexValues.Length; i++)
                    ciphertext[i] = Convert.ToByte(hexValues[i], 16);

                byte[] plaintext = await Task.Run(() =>
                {
                    AesCore aes = new AesCore(key);
                    string mode = cmbMode.SelectedItem.ToString();

                    if (mode == "ECB")
                        return aes.DecryptECB(ciphertext);
                    else if (mode == "CBC")
                    {
                        byte[] iv = StringToHex(txtIV.Text);
                        return aes.DecryptCBC(ciphertext, iv);
                    }
                    else
                    {
                        byte[] nonce = StringToHex(txtIV.Text.Substring(0, 16));
                        return aes.DecryptCTR(ciphertext, nonce, 0);
                    }
                });

                if (rdoHex.Checked)
                    txtPlaintext.Text = HexToString(plaintext);
                else
                    txtPlaintext.Text = Encoding.UTF8.GetString(plaintext);

                lblStatus.Text = "✅ Déchiffrement terminé avec succès !";
                lblStatus.ForeColor = Color.Green;
                progressBar.Visible = false;
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"❌ Erreur : {ex.Message}";
                lblStatus.ForeColor = Color.Red;
                progressBar.Visible = false;
                MessageBox.Show($"Erreur lors du déchiffrement : {ex.Message}", "Erreur",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnDecrypt.Enabled = true;
            }
        }

        private async void BtnFileEncrypt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Sélectionner un fichier à chiffrer";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Title = "Enregistrer le fichier chiffré";
                        sfd.Filter = "Fichiers AES|*.aes|Tous les fichiers|*.*";
                        sfd.FileName = ofd.SafeFileName + ".aes";

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                progressBar.Visible = true;
                                lblStatus.Text = $"⏳ Chiffrement du fichier {ofd.SafeFileName}...";

                                byte[] key = GetKeyFromInput();
                                byte[] fileData = await Task.Run(() => File.ReadAllBytes(ofd.FileName));

                                byte[] encrypted = await Task.Run(() =>
                                {
                                    AesCore aes = new AesCore(key);
                                    return aes.EncryptECB(fileData);
                                });

                                await Task.Run(() => File.WriteAllBytes(sfd.FileName, encrypted));

                                progressBar.Visible = false;
                                lblStatus.Text = $"✅ Fichier chiffré : {sfd.FileName}";
                                MessageBox.Show($"Fichier chiffré avec succès !\nTaille originale : {fileData.Length} bytes\nTaille chiffrée : {encrypted.Length} bytes",
                                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                progressBar.Visible = false;
                                MessageBox.Show($"Erreur : {ex.Message}", "Erreur", 
                                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private async void BtnFileDecrypt_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Sélectionner un fichier à déchiffrer";
                ofd.Filter = "Fichiers AES|*.aes|Tous les fichiers|*.*";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    using (SaveFileDialog sfd = new SaveFileDialog())
                    {
                        sfd.Title = "Enregistrer le fichier déchiffré";
                        sfd.FileName = ofd.SafeFileName.Replace(".aes", "");

                        if (sfd.ShowDialog() == DialogResult.OK)
                        {
                            try
                            {
                                progressBar.Visible = true;
                                lblStatus.Text = $"⏳ Déchiffrement du fichier {ofd.SafeFileName}...";

                                byte[] key = GetKeyFromInput();
                                byte[] encrypted = await Task.Run(() => File.ReadAllBytes(ofd.FileName));

                                byte[] decrypted = await Task.Run(() =>
                                {
                                    AesCore aes = new AesCore(key);
                                    return aes.DecryptECB(encrypted);
                                });

                                await Task.Run(() => File.WriteAllBytes(sfd.FileName, decrypted));

                                progressBar.Visible = false;
                                lblStatus.Text = $"✅ Fichier déchiffré : {sfd.FileName}";
                                MessageBox.Show($"Fichier déchiffré avec succès !\nTaille : {decrypted.Length} bytes",
                                    "Succès", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            }
                            catch (Exception ex)
                            {
                                progressBar.Visible = false;
                                MessageBox.Show($"Erreur : {ex.Message}\nVérifiez que la clé est correcte.", 
                                    "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }
    }
}