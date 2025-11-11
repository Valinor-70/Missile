using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace MissileSimulator
{
    /// <summary>
    /// Authentication form for operator login
    /// Demonstrates secure authentication with PBKDF2 password hashing
    /// </summary>
    public class AuthForm : Form
    {
        private TextBox operatorIdTextBox;
        private TextBox passphraseTextBox;
        private Button loginButton;
        private Button cancelButton;
        private Label statusLabel;
        
        // Store valid credentials (in real app, this would be in a secure database)
        // Educational purpose: demonstrating password verification
        private static Dictionary<string, (string hash, string salt)> validCredentials;
        
        public string OperatorId { get; private set; }
        
        static AuthForm()
        {
            // Initialize demo credentials
            validCredentials = new Dictionary<string, (string, string)>();
            
            // O5-X with passphrase "HandofDemocracy"
            string salt1;
            string hash1 = CryptoHelper.HashPassword("HandofDemocracy", out salt1);
            validCredentials["O5-X"] = (hash1, salt1);
            
            // Ethics Committee
            string salt2;
            string hash2 = CryptoHelper.HashPassword("HandofDemocracy", out salt2);
            validCredentials["Ethics Committee"] = (hash2, salt2);
        }
        
        public AuthForm()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Operator Authentication";
            this.Size = new Size(500, 350);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 240, 240);
            
            // High-quality rendering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);
            this.UpdateStyles();
            
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(40, 30, 40, 30)
            };
            
            var titleLabel = new Label
            {
                Text = "🔐 Operator Authentication",
                Font = new Font("Segoe UI", 16F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215),
                AutoSize = true,
                Location = new Point(0, 0)
            };
            
            var operatorLabel = new Label
            {
                Text = "Operator ID:",
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(0, 60)
            };
            
            operatorIdTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, 85),
                Width = 400,
                Height = 30
            };
            
            var passphraseLabel = new Label
            {
                Text = "Passphrase:",
                Font = new Font("Segoe UI", 10F),
                AutoSize = true,
                Location = new Point(0, 130)
            };
            
            passphraseTextBox = new TextBox
            {
                Font = new Font("Segoe UI", 11F),
                Location = new Point(0, 155),
                Width = 400,
                Height = 30,
                UseSystemPasswordChar = true
            };
            passphraseTextBox.KeyPress += (s, e) =>
            {
                if (e.KeyChar == (char)Keys.Return)
                {
                    LoginButton_Click(s, e);
                    e.Handled = true;
                }
            };
            
            statusLabel = new Label
            {
                Text = "",
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.Red,
                AutoSize = false,
                Width = 400,
                Height = 20,
                Location = new Point(0, 195),
                TextAlign = ContentAlignment.MiddleLeft
            };
            
            var buttonPanel = new FlowLayoutPanel
            {
                Location = new Point(0, 225),
                Width = 400,
                Height = 40,
                FlowDirection = FlowDirection.RightToLeft
            };
            
            loginButton = new Button
            {
                Text = "Login",
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            loginButton.FlatAppearance.BorderSize = 0;
            loginButton.Click += LoginButton_Click;
            
            cancelButton = new Button
            {
                Text = "Cancel",
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
            };
            cancelButton.FlatAppearance.BorderSize = 0;
            cancelButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            
            buttonPanel.Controls.Add(loginButton);
            buttonPanel.Controls.Add(cancelButton);
            
            mainPanel.Controls.Add(titleLabel);
            mainPanel.Controls.Add(operatorLabel);
            mainPanel.Controls.Add(operatorIdTextBox);
            mainPanel.Controls.Add(passphraseLabel);
            mainPanel.Controls.Add(passphraseTextBox);
            mainPanel.Controls.Add(statusLabel);
            mainPanel.Controls.Add(buttonPanel);
            
            this.Controls.Add(mainPanel);
        }
        
        private void LoginButton_Click(object sender, EventArgs e)
        {
            string operatorId = operatorIdTextBox.Text.Trim();
            string passphrase = passphraseTextBox.Text;
            
            if (string.IsNullOrEmpty(operatorId) || string.IsNullOrEmpty(passphrase))
            {
                statusLabel.Text = "Please enter both Operator ID and Passphrase";
                return;
            }
            
            // Verify credentials using PBKDF2 hash verification
            if (validCredentials.ContainsKey(operatorId))
            {
                var (storedHash, storedSalt) = validCredentials[operatorId];
                if (CryptoHelper.VerifyPassword(passphrase, storedHash, storedSalt))
                {
                    OperatorId = operatorId;
                    DialogResult = DialogResult.OK;
                    Close();
                    return;
                }
            }
            
            statusLabel.Text = "Invalid credentials. Try: O5-X / HandofDemocracy";
        }
    }
}
