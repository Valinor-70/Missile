using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MissileSimulator
{
    public partial class AuthCodeForm : Form
    {
        private TextBox txtAuthCode;
        private Button btnVerify;
        private RichTextBox txtConsole;
        private Label lblMissileInfo;
        
        public string RequiredAuthCode { get; set; }
        public string MissileId { get; set; }
        public string MissileName { get; set; }
        
        public AuthCodeForm()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "STAGE 3: AUTHORIZATION";
            this.Size = new Size(600, 500);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 30);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            int y = 20;
            
            // Header
            var lblTitle = new Label
            {
                Text = "SCP FOUNDATION - AUTHORIZATION",
                Font = new Font("Courier New", 12, FontStyle.Bold),
                Location = new Point(20, y),
                Size = new Size(560, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.Black
            };
            this.Controls.Add(lblTitle);
            y += 40;
            
            var lblStage = new Label
            {
                Text = "STAGE 3: ENTER AUTHORIZATION CODE",
                Font = new Font("Courier New", 9, FontStyle.Bold),
                Location = new Point(20, y),
                Size = new Size(560, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(255, 200, 0)
            };
            this.Controls.Add(lblStage);
            y += 35;
            
            // Missile info
            lblMissileInfo = new Label
            {
                Text = "MISSILE: Awaiting data...",
                Font = new Font("Consolas", 9, FontStyle.Bold),
                Location = new Point(40, y),
                Size = new Size(520, 40),
                ForeColor = Color.FromArgb(255, 180, 0)
            };
            this.Controls.Add(lblMissileInfo);
            y += 50;
            
            // Auth code input
            var grpAuth = new GroupBox
            {
                Text = "━━━ AUTHORIZATION CODE ━━━",
                Location = new Point(40, y),
                Size = new Size(520, 110),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            var lblInfo = new Label
            {
                Text = "Enter the authorization code from AuthCodes.txt\nFormat: XXX-XX-XXX",
                Location = new Point(15, 25),
                Size = new Size(490, 35),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Consolas", 8)
            };
            grpAuth.Controls.Add(lblInfo);
            
            txtAuthCode = new TextBox
            {
                Location = new Point(15, 65),
                Size = new Size(350, 23),
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(255, 255, 0),
                Font = new Font("Consolas", 12, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                MaxLength = 11 // XXX-XX-XXX = 11 chars with dashes
            };
            txtAuthCode.TextChanged += TxtAuthCode_TextChanged;
            grpAuth.Controls.Add(txtAuthCode);
            
            btnVerify = new Button
            {
                Text = "[VERIFY]",
                Location = new Point(375, 63),
                Size = new Size(130, 27),
                BackColor = Color.FromArgb(0, 100, 0),
                ForeColor = Color.FromArgb(200, 255, 200),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 9, FontStyle.Bold)
            };
            btnVerify.FlatAppearance.BorderColor = Color.FromArgb(0, 200, 0);
            btnVerify.Click += BtnVerify_Click;
            grpAuth.Controls.Add(btnVerify);
            
            this.Controls.Add(grpAuth);
            y += 120;
            
            // Console
            var grpConsole = new GroupBox
            {
                Text = "━━━ AUTHORIZATION LOG ━━━",
                Location = new Point(40, y),
                Size = new Size(520, 200),
                ForeColor = Color.FromArgb(200, 200, 220),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            txtConsole = new RichTextBox
            {
                Location = new Point(10, 22),
                Size = new Size(500, 170),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 100),
                Font = new Font("Consolas", 8),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpConsole.Controls.Add(txtConsole);
            
            this.Controls.Add(grpConsole);
            
            LogToConsole(">>> AUTHORIZATION STAGE INITIALIZED");
            LogToConsole(">>> AWAITING CODE ENTRY...");
        }
        
        public void SetMissileInfo()
        {
            lblMissileInfo.Text = $"MISSILE: {MissileName}\nID: {MissileId}";
            LogToConsole($">>> MISSILE: {MissileName}");
            LogToConsole($">>> REQUIRED CODE: {RequiredAuthCode}");
            LogToConsole($">>> (Check AuthCodes.txt)");
        }
        
        private void BtnVerify_Click(object sender, EventArgs e)
        {
            string entered = txtAuthCode.Text.Trim();
            
            if (entered == RequiredAuthCode)
            {
                LogToConsole($">>> CODE VERIFIED: {entered}");
                LogToConsole($">>> AUTHORIZATION GRANTED");
                LogToConsole($">>> PROCEEDING TO LAUNCH CONTROL...");
                
                MessageBox.Show("AUTHORIZATION VERIFIED\n\nProceeding to launch control.",
                    "AUTHORIZED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                LogToConsole($">>> VERIFICATION FAILED");
                LogToConsole($">>> EXPECTED: {RequiredAuthCode}");
                LogToConsole($">>> ENTERED: {entered}");
                
                MessageBox.Show($"AUTHORIZATION DENIED\n\nExpected: {RequiredAuthCode}\nEntered: {entered}",
                    "ACCESS DENIED", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void TxtAuthCode_TextChanged(object sender, EventArgs e)
        {
            // Auto-format with dashes: XXX-XX-XXX
            string text = txtAuthCode.Text.Replace("-", ""); // Remove existing dashes
            
            // Only allow digits
            text = new string(text.Where(char.IsDigit).ToArray());
            
            // Add dashes at appropriate positions
            if (text.Length > 5)
            {
                text = text.Substring(0, 3) + "-" + text.Substring(3, 2) + "-" + text.Substring(5);
            }
            else if (text.Length > 3)
            {
                text = text.Substring(0, 3) + "-" + text.Substring(3);
            }
            
            // Update textbox if changed
            if (txtAuthCode.Text != text)
            {
                int cursorPos = txtAuthCode.SelectionStart;
                txtAuthCode.TextChanged -= TxtAuthCode_TextChanged; // Prevent recursion
                txtAuthCode.Text = text;
                txtAuthCode.SelectionStart = Math.Min(cursorPos + (text.Length > txtAuthCode.Text.Length ? 1 : 0), text.Length);
                txtAuthCode.TextChanged += TxtAuthCode_TextChanged;
            }
        }
        
        private void LogToConsole(string message)
        {
            if (txtConsole != null)
            {
                txtConsole.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                txtConsole.ScrollToCaret();
            }
        }
    }
}
