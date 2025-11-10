using System;
using System.Drawing;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace MissileSimulator
{
    public partial class EncryptionForm : Form
    {
        private TextBox txtClearanceCode;
        private Button btnEncrypt;
        private RichTextBox txtConsole;
        private Label lblTargetInfo;
        private Label lblMissileId;
        
        public string OperatorId { get; set; }
        public double TargetLat { get; set; }
        public double TargetLng { get; set; }
        public string WeaponType { get; set; }
        public string LaunchBase { get; set; }
        public double WeaponYield { get; set; }
        
        public string MissileId { get; private set; }
        public string MissileName { get; private set; }
        public string LaunchCode { get; private set; }
        public string AuthCode { get; private set; }
        public string ClearanceLevel { get; private set; }
        
        private System.Collections.Generic.HashSet<string> validAuthCodes;
        
        public EncryptionForm(string operatorId, System.Collections.Generic.HashSet<string> authCodes)
        {
            this.OperatorId = operatorId;
            this.validAuthCodes = authCodes;
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "STAGE 2: ENCRYPTION";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 30);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            int y = 20;
            
            // Header
            var lblTitle = new Label
            {
                Text = "SCP FOUNDATION - ENCRYPTION PROTOCOL",
                Font = new Font("Courier New", 12, FontStyle.Bold),
                Location = new Point(20, y),
                Size = new Size(660, 25),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.Black
            };
            this.Controls.Add(lblTitle);
            y += 40;
            
            var lblStage = new Label
            {
                Text = "STAGE 2: CLEARANCE VERIFICATION",
                Font = new Font("Courier New", 9, FontStyle.Bold),
                Location = new Point(20, y),
                Size = new Size(660, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(255, 200, 0)
            };
            this.Controls.Add(lblStage);
            y += 35;
            
            // Target info
            lblTargetInfo = new Label
            {
                Text = "TARGET: Awaiting data...",
                Font = new Font("Consolas", 9),
                Location = new Point(40, y),
                Size = new Size(620, 60),
                ForeColor = Color.FromArgb(100, 255, 100)
            };
            this.Controls.Add(lblTargetInfo);
            y += 70;
            
            // Clearance input
            var grpClearance = new GroupBox
            {
                Text = "━━━ CLEARANCE CODE ━━━",
                Location = new Point(40, y),
                Size = new Size(620, 100),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            var lblInfo = new Label
            {
                Text = "Enter your clearance code:\n  L6-O5-X (O5 Council)\n  L5-EC-LAW (Ethics Committee)",
                Location = new Point(15, 25),
                Size = new Size(590, 45),
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Consolas", 8)
            };
            grpClearance.Controls.Add(lblInfo);
            
            txtClearanceCode = new TextBox
            {
                Location = new Point(15, 70),
                Size = new Size(450, 23),
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(255, 255, 0),
                Font = new Font("Consolas", 10, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpClearance.Controls.Add(txtClearanceCode);
            
            btnEncrypt = new Button
            {
                Text = "[ENCRYPT]",
                Location = new Point(475, 68),
                Size = new Size(130, 27),
                BackColor = Color.FromArgb(100, 0, 100),
                ForeColor = Color.FromArgb(255, 200, 255),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 9, FontStyle.Bold)
            };
            btnEncrypt.FlatAppearance.BorderColor = Color.FromArgb(200, 0, 200);
            btnEncrypt.Click += BtnEncrypt_Click;
            grpClearance.Controls.Add(btnEncrypt);
            
            this.Controls.Add(grpClearance);
            y += 110;
            
            // Missile ID
            lblMissileId = new Label
            {
                Text = "MISSILE ID: [AWAITING ENCRYPTION]",
                Font = new Font("Consolas", 9, FontStyle.Bold),
                Location = new Point(40, y),
                Size = new Size(620, 20),
                ForeColor = Color.FromArgb(255, 180, 0)
            };
            this.Controls.Add(lblMissileId);
            y += 30;
            
            // Console
            var grpConsole = new GroupBox
            {
                Text = "━━━ ENCRYPTION LOG ━━━",
                Location = new Point(40, y),
                Size = new Size(620, 200),
                ForeColor = Color.FromArgb(200, 200, 220),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            txtConsole = new RichTextBox
            {
                Location = new Point(10, 22),
                Size = new Size(600, 170),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 100),
                Font = new Font("Consolas", 8),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpConsole.Controls.Add(txtConsole);
            
            this.Controls.Add(grpConsole);
            
            LogToConsole(">>> ENCRYPTION STAGE INITIALIZED");
            LogToConsole(">>> AWAITING CLEARANCE CODE...");
        }
        
        public void SetTargetInfo()
        {
            lblTargetInfo.Text = $"TARGET: {TargetLat:F6}, {TargetLng:F6}\n" +
                                $"WEAPON: {WeaponType} ({WeaponYield} MT)\n" +
                                $"BASE: {LaunchBase}";
            LogToConsole($">>> TARGET: {TargetLat:F6}, {TargetLng:F6}");
            LogToConsole($">>> WEAPON: {WeaponType}");
        }
        
        private void BtnEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                string encKey = txtClearanceCode.Text.Trim();
                
                // Check clearance level format
                if (encKey.StartsWith("L6-O5-") || OperatorId.Contains("O5"))
                {
                    ClearanceLevel = "L6-O5";
                }
                else if (encKey.StartsWith("L5-EC-") || OperatorId.Contains("Ethics"))
                {
                    ClearanceLevel = "L5-EC";
                }
                else
                {
                    MessageBox.Show("CLEARANCE DENIED\n\nRequired formats:\n- L6-O5-X (for O5 Council)\n- L5-EC-LAW (for Ethics Committee)", 
                        "INVALID CLEARANCE", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                
                // Generate missile name
                string[] prefixes = { "EAGLE", "FALCON", "HAWK", "RAVEN", "PHOENIX", "CONDOR", "OSPREY", "SPARROW", "VIPER", "COBRA" };
                string[] suffixes = { "ALPHA", "BETA", "GAMMA", "DELTA", "SIGMA", "OMEGA", "PRIME", "ULTIMA", "NOVA", "APEX" };
                var rand = new Random();
                MissileName = $"{prefixes[rand.Next(prefixes.Length)]}-{suffixes[rand.Next(suffixes.Length)]}";
                
                // Generate Missile ID
                long coordProduct = (long)(Math.Abs(TargetLat * TargetLng * 1000));
                MissileId = $"{LaunchBase}-0001-{WeaponType.ToUpper()}-{coordProduct}-0001";
                
                // Generate launch code
                int part1 = rand.Next(100, 1000);
                int part2 = rand.Next(1000, 10000);
                int part3 = rand.Next(100, 1000);
                LaunchCode = $"{LaunchBase}-{part1:D3}-{part2:D4}-{part3:D3}";
                
                // Select random auth code
                AuthCode = validAuthCodes.ToArray()[rand.Next(validAuthCodes.Count)];
                
                // Create payload and encrypt
                var payload = new
                {
                    @operator = OperatorId,
                    clearance = ClearanceLevel,
                    @event = WeaponType,
                    launch_base = LaunchBase,
                    missile_name = MissileName,
                    missile_id = MissileId,
                    launch_code = LaunchCode,
                    coords = new[] { TargetLat, TargetLng },
                    yield_mt = WeaponYield,
                    auth_code = AuthCode,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
                byte[] salt = Encoding.UTF8.GetBytes("MissileSimSalt2024");
                byte[] key = CryptoHelper.DeriveKey(encKey, salt);
                byte[] encrypted = CryptoHelper.EncryptAES(json, key);
                string encryptedPayload = Convert.ToBase64String(encrypted);
                
                lblMissileId.Text = $"MISSILE ID: [{MissileId}]";
                
                LogToConsole($">>> CLEARANCE: {ClearanceLevel}");
                LogToConsole($">>> MISSILE: {MissileName}");
                LogToConsole($">>> ID: {MissileId}");
                LogToConsole($">>> LAUNCH CODE: {LaunchCode}");
                LogToConsole($">>> PAYLOAD ENCRYPTED");
                LogToConsole($">>> PROCEEDING TO AUTHORIZATION...");
                
                MessageBox.Show($"ENCRYPTION SUCCESSFUL\n\nCLEARANCE: {ClearanceLevel}\nMISSILE: {MissileName}\nID: {MissileId}\nLAUNCH CODE: {LaunchCode}\n\nProceed to authorization.",
                    "ENCRYPTED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Encryption error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
