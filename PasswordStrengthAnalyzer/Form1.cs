using System;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace PasswordStrengthAnalyzer
{
    public partial class Form1 : Form
    {
        // UI Controls
        private TextBox txtPassword;
        private CheckBox chkShowPassword;
        private Label lblStrength;
        private Panel pnlStrengthBackground;
        private Panel pnlStrengthBar; // Visual Strength Bar
        private TextBox txtAnalysis;
        private Button btnGenerate;
        private Button btnCopy;
        private TextBox txtGeneratedPassword;

        public Form1()
        {
            InitializeComponent();
            SetupUI();
        }

        private void SetupUI()
        {
            // --- Form Styling: Dark Mode ---
            this.Text = "Advanced Password Strength Analyzer";
            this.Size = new Size(450, 530);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 30);      // Dark background
            this.ForeColor = Color.White;                     // White text globally
            this.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point); // Modern Font

            Label lblTitle = new Label { Text = "Enter a password to analyze:", Location = new Point(20, 20), AutoSize = true, ForeColor = Color.LightGray };
            this.Controls.Add(lblTitle);

            // --- Password Input Field ---
            txtPassword = new TextBox
            {
                Location = new Point(20, 45),
                Width = 390,
                PasswordChar = '•', // Modern bullet instead of asterisk
                Font = new Font("Segoe UI", 12F),
                BackColor = Color.FromArgb(45, 45, 48), // Darker text box
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            txtPassword.TextChanged += TxtPassword_TextChanged;
            this.Controls.Add(txtPassword);

            // --- Visual Strength Bar ---
            pnlStrengthBackground = new Panel { Location = new Point(20, 78), Width = 390, Height = 6, BackColor = Color.FromArgb(60, 60, 60) };
            pnlStrengthBar = new Panel { Location = new Point(0, 0), Width = 0, Height = 6, BackColor = Color.Transparent };
            pnlStrengthBackground.Controls.Add(pnlStrengthBar);
            this.Controls.Add(pnlStrengthBackground);

            chkShowPassword = new CheckBox { Text = "Reveal Password", Location = new Point(20, 95), AutoSize = true, ForeColor = Color.LightGray };
            chkShowPassword.CheckedChanged += ChkShowPassword_CheckedChanged;
            this.Controls.Add(chkShowPassword);

            lblStrength = new Label { Text = "Strength: None", Location = new Point(20, 125), AutoSize = true, Font = new Font("Segoe UI", 11F, FontStyle.Bold) };
            this.Controls.Add(lblStrength);

            // --- Analysis Output Field ---
            txtAnalysis = new TextBox
            {
                Location = new Point(20, 155),
                Width = 390,
                Height = 170,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.LightGray,
                BorderStyle = BorderStyle.None, // Borderless for a cleaner modern look
                Font = new Font("Segoe UI", 9F)
            };
            this.Controls.Add(txtAnalysis);

            // --- Flat Modern Buttons ---
            btnGenerate = CreateFlatButton("Generate Password", new Point(20, 345), 190);
            btnGenerate.Click += BtnGenerate_Click;
            this.Controls.Add(btnGenerate);

            btnCopy = CreateFlatButton("Copy to Clipboard", new Point(220, 345), 190);
            btnCopy.Click += BtnCopy_Click;
            this.Controls.Add(btnCopy);

            txtGeneratedPassword = new TextBox
            {
                Location = new Point(20, 395),
                Width = 390,
                ReadOnly = true,
                Font = new Font("Segoe UI", 12F),
                BackColor = Color.FromArgb(45, 45, 48),
                ForeColor = Color.PaleGreen,
                BorderStyle = BorderStyle.FixedSingle
            };
            this.Controls.Add(txtGeneratedPassword);
        }

        // Helper to create consistent, modern Flat Buttons
        private Button CreateFlatButton(string text, Point location, int width)
        {
            Button btn = new Button
            {
                Text = text,
                Location = location,
                Width = width,
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(0, 122, 204), // VS Blue Accent
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0; // Remove 3D border
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(28, 151, 234); // Lighter blue on hover
            return btn;
        }

        private void ChkShowPassword_CheckedChanged(object? sender, EventArgs e)
        {
            txtPassword.PasswordChar = chkShowPassword.Checked ? '\0' : '•';
        }

        private void BtnCopy_Click(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtGeneratedPassword.Text))
            {
                Clipboard.SetText(txtGeneratedPassword.Text);
                MessageBox.Show("Password copied to clipboard!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TxtPassword_TextChanged(object? sender, EventArgs e)
        {
            AnalyzePassword(txtPassword.Text);
        }

        private void BtnGenerate_Click(object? sender, EventArgs e)
        {
            string securePass = GenerateStrongPassword(16);
            txtGeneratedPassword.Text = securePass;
        }

        private void AnalyzePassword(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                UpdateStrengthUI(0, "None", Color.LightGray);
                txtAnalysis.Text = "";
                return;
            }

            if (IsCommonPassword(password))
            {
                UpdateStrengthUI(1, "VERY WEAK (Commonly used!)", Color.Red);
                txtAnalysis.Text = "- Suggestion: Use a unique combination of words or characters.\r\n";
                return;
            }

            bool hasUpper = password.Any(char.IsUpper);
            bool hasLower = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            int charsetSize = 0;
            if (hasLower) charsetSize += 26;
            if (hasUpper) charsetSize += 26;
            if (hasDigit) charsetSize += 10;
            if (hasSpecial) charsetSize += 32;

            int score = 0;
            if (password.Length >= 8) score++;
            if (hasUpper) score++;
            if (hasLower) score++;
            if (hasDigit) score++;
            if (hasSpecial) score++;

            StringBuilder analysis = new StringBuilder();
            analysis.AppendLine("--- Analysis ---");

            if (password.Length < 8) analysis.AppendLine("• Add more characters (min 8).");
            if (!hasUpper) analysis.AppendLine("• Add an uppercase letter.");
            if (!hasLower) analysis.AppendLine("• Add a lowercase letter.");
            if (!hasDigit) analysis.AppendLine("• Add a number.");
            if (!hasSpecial) analysis.AppendLine("• Add a special character (e.g., !, @, #).");

            double entropy = password.Length * Math.Log2(charsetSize == 0 ? 1 : charsetSize);
            analysis.AppendLine();
            analysis.AppendLine($"• Complexity Score (Entropy): {(int)entropy} bits");
            analysis.AppendLine($"• Time to crack: {EstimateTimeToCrack(entropy)}");

            txtAnalysis.Text = analysis.ToString();

            // Evaluate overall strength logic (0 to 5)
            if (score <= 2 || password.Length < 6)
                UpdateStrengthUI(score == 0 ? 1 : score, "WEAK", Color.Tomato);
            else if (score < 5 || password.Length < 8)
                UpdateStrengthUI(score, "MEDIUM", Color.Orange); // Medium 3-4
            else
                UpdateStrengthUI(5, "STRONG", Color.LimeGreen);  // Strong 5
        }

        // Dedicated method to update the labels and colorized visual strength bar
        private void UpdateStrengthUI(int score, string label, Color color)
        {
            lblStrength.Text = "Strength: " + label;
            lblStrength.ForeColor = color;

            // Cap score at 5 for math purposes
            score = Math.Max(0, Math.Min(5, score));

            // Set width of the visual bar based on the 1-5 score, using 390 as max width
            pnlStrengthBar.Width = (int)((score / 5.0) * pnlStrengthBackground.Width);
            pnlStrengthBar.BackColor = color;
        }

        private bool IsCommonPassword(string password)
        {
            string[] commonPasswords = { "password", "123456", "12345678", "admin", "qwerty", "iloveyou" };
            return commonPasswords.Contains(password);
        }

        private string EstimateTimeToCrack(double entropy)
        {
            double combinations = Math.Pow(2, entropy);
            double guessesPerSecond = 100e9; // 100 Billion/sec offline attack
            double seconds = combinations / guessesPerSecond;

            if (seconds < 1) return "Instantly";
            if (seconds < 60) return $"{(int)seconds} seconds";
            if (seconds < 3600) return $"{(int)(seconds / 60)} minutes";
            if (seconds < 86400) return $"{(int)(seconds / 3600)} hours";
            if (seconds < 31536000) return $"{(int)(seconds / 86400)} days";
            if (seconds < 3153600000) return $"{(int)(seconds / 31536000)} years";
            return "Centuries (Highly Secure)";
        }

        private string GenerateStrongPassword(int length = 16)
        {
            const string allChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()-_=+";

            using (var rng = RandomNumberGenerator.Create())
            {
                byte[] uintBuffer = new byte[sizeof(uint)];
                char[] newPassword = new char[length];

                for (int i = 0; i < length; i++)
                {
                    rng.GetBytes(uintBuffer);
                    uint num = BitConverter.ToUInt32(uintBuffer, 0);
                    newPassword[i] = allChars[(int)(num % (uint)allChars.Length)];
                }

                return new string(newPassword);
            }
        }
    }
}