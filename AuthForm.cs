using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MissileSimulator
{
    /// <summary>
    /// Authentication form demonstrating secure login with PBKDF2 password hashing.
    /// Educational only - shows proper authentication flow patterns.
    /// </summary>
    public partial class AuthForm : Form
    {
        // Pre-configured demo credentials (in real app, these would be in a secure database)
        // Stored as (operatorId, passwordHash, salt) tuples
        private static readonly Dictionary<string, (byte[] hash, byte[] salt)> ValidCredentials;
        
        public string SessionToken { get; private set; }
        public string OperatorId { get; private set; }
        
        private TextBox txtOperatorId;
        private TextBox txtPassphrase;
        private Button btnLogin;
        private Button btnCancel;
        private Label lblTitle;
        private Label lblOperatorId;
        private Label lblPassphrase;
        private Label lblInfo;
        
        static AuthForm()
        {
            // Pre-hash the demo passwords (in real app, done during user registration)
            ValidCredentials = new Dictionary<string, (byte[] hash, byte[] salt)>();
            
            // Demo credential: O5-X / HandofDemocracy
            var (hash1, salt1) = CryptoHelper.HashPassword("HandofDemocracy");
            ValidCredentials["O5-X"] = (hash1, salt1);
            
            // Demo credential: Ethics Committee / SafeSimulation
            var (hash2, salt2) = CryptoHelper.HashPassword("SafeSimulation");
            ValidCredentials["Ethics Committee"] = (hash2, salt2);
        }
        
        public AuthForm()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Authentication - Missile Simulator";
            this.Size = new Size(420, 300);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(30, 30, 40);
            
            // Title with modern styling
            lblTitle = new Label
            {
                Text = "🔐 SECURE AUTHENTICATION",
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                Location = new Point(20, 25),
                Size = new Size(380, 35),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(100, 200, 255)
            };
            
            // Info label - removed educational mention
            lblInfo = new Label
            {
                Text = "PBKDF2 password verification (100k iterations)",
                Font = new Font("Segoe UI", 8, FontStyle.Italic),
                Location = new Point(20, 60),
                Size = new Size(380, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(150, 150, 170)
            };
            
            // Operator ID
            lblOperatorId = new Label
            {
                Text = "Operator ID:",
                Location = new Point(50, 105),
                Size = new Size(100, 20),
                ForeColor = Color.FromArgb(200, 200, 220),
                Font = new Font("Segoe UI", 10)
            };
            
            txtOperatorId = new TextBox
            {
                Location = new Point(160, 102),
                Size = new Size(210, 25),
                Font = new Font("Consolas", 10),
                BackColor = Color.FromArgb(50, 50, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Passphrase
            lblPassphrase = new Label
            {
                Text = "Passphrase:",
                Location = new Point(50, 145),
                Size = new Size(100, 20),
                ForeColor = Color.FromArgb(200, 200, 220),
                Font = new Font("Segoe UI", 10)
            };
            
            txtPassphrase = new TextBox
            {
                Location = new Point(160, 142),
                Size = new Size(210, 25),
                UseSystemPasswordChar = true,
                Font = new Font("Consolas", 10),
                BackColor = Color.FromArgb(50, 50, 60),
                ForeColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle
            };
            
            // Login button with modern flat design
            btnLogin = new Button
            {
                Text = "🔓 Login",
                Location = new Point(160, 190),
                Size = new Size(110, 35),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnLogin.FlatAppearance.BorderSize = 0;
            btnLogin.Click += BtnLogin_Click;
            
            // Cancel button
            btnCancel = new Button
            {
                Text = "✖ Cancel",
                Location = new Point(280, 190),
                Size = new Size(90, 35),
                BackColor = Color.FromArgb(150, 50, 50),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;
            
            // Add controls
            this.Controls.AddRange(new Control[] {
                lblTitle, lblInfo, lblOperatorId, txtOperatorId,
                lblPassphrase, txtPassphrase, btnLogin, btnCancel
            });
            
            this.AcceptButton = btnLogin;
            this.CancelButton = btnCancel;
        }
        
        private void BtnLogin_Click(object sender, EventArgs e)
        {
            string operatorId = txtOperatorId.Text.Trim();
            string passphrase = txtPassphrase.Text;
            
            if (string.IsNullOrEmpty(operatorId) || string.IsNullOrEmpty(passphrase))
            {
                MessageBox.Show("Please enter both Operator ID and Passphrase.",
                    "Authentication Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Verify credentials
            if (ValidCredentials.TryGetValue(operatorId, out var credentials))
            {
                if (CryptoHelper.VerifyPassword(passphrase, credentials.hash, credentials.salt))
                {
                    // Authentication successful
                    OperatorId = operatorId;
                    SessionToken = Guid.NewGuid().ToString(); // Generate session token
                    
                    MessageBox.Show($"Authentication successful!\n\nOperator: {operatorId}\nSession Token: {SessionToken.Substring(0, 8)}...",
                        "Login Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                    return;
                }
            }
            
            // Authentication failed
            MessageBox.Show("Invalid Operator ID or Passphrase.\n\nDemo credentials:\n- O5-X / HandofDemocracy\n- Ethics Committee / SafeSimulation",
                "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
