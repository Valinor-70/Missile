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
            this.Size = new Size(400, 280);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // Title
            lblTitle = new Label
            {
                Text = "SECURE AUTHENTICATION",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(20, 20),
                Size = new Size(360, 30),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.DarkBlue
            };
            
            // Info label
            lblInfo = new Label
            {
                Text = "Educational demo - PBKDF2 password verification",
                Font = new Font("Arial", 8, FontStyle.Italic),
                Location = new Point(20, 50),
                Size = new Size(360, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.Gray
            };
            
            // Operator ID
            lblOperatorId = new Label
            {
                Text = "Operator ID:",
                Location = new Point(40, 90),
                Size = new Size(100, 20)
            };
            
            txtOperatorId = new TextBox
            {
                Location = new Point(150, 87),
                Size = new Size(200, 20),
                Font = new Font("Arial", 10)
            };
            
            // Passphrase
            lblPassphrase = new Label
            {
                Text = "Passphrase:",
                Location = new Point(40, 130),
                Size = new Size(100, 20)
            };
            
            txtPassphrase = new TextBox
            {
                Location = new Point(150, 127),
                Size = new Size(200, 20),
                UseSystemPasswordChar = true,
                Font = new Font("Arial", 10)
            };
            
            // Login button
            btnLogin = new Button
            {
                Text = "Login",
                Location = new Point(150, 170),
                Size = new Size(100, 30),
                BackColor = Color.Green,
                ForeColor = Color.White,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnLogin.Click += BtnLogin_Click;
            
            // Cancel button
            btnCancel = new Button
            {
                Text = "Cancel",
                Location = new Point(260, 170),
                Size = new Size(90, 30),
                DialogResult = DialogResult.Cancel
            };
            
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
