using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.IO;
using System.Linq;

namespace MissileSimulator
{
    /// <summary>
    /// Main application form with map, controls, and animations
    /// High-quality rendering with DPI awareness for crisp, non-pixelated UI
    /// </summary>
    public class MainForm : Form
    {
        private string operatorId;
        private Panel mapPanel;
        private Panel controlPanel;
        private ComboBox missileTypeCombo;
        private TextBox latTextBox;
        private TextBox lngTextBox;
        private TextBox radiusTextBox;
        private Button plotButton;
        private TextBox encryptedTextBox;
        private TextBox launchCodeTextBox; // Fixed cursor positioning
        private Button encryptButton;
        private Button decryptButton;
        private Button activateButton;
        private TextBox consoleTextBox;
        private ControlKeyWidget controlKey;
        private Label missileIdLabel;
        private Label requiredCodeLabel;
        private Timer animationTimer;
        private string currentMissileId = "";
        private string requiredLaunchCode = "";
        private string encryptionKey = "";
        private List<EventPoint> eventPoints = new List<EventPoint>();
        private ExplosionAnimation explosionAnimation;
        
        public MainForm(string operatorId)
        {
            this.operatorId = operatorId;
            InitializeComponent();
            LogEvent($"Operator {operatorId} logged in");
        }
        
        private void InitializeComponent()
        {
            this.Text = "Educational Missile Simulator - SAFE DEMONSTRATION";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            
            // Enable high-quality, non-pixelated rendering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint |
                         ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();
            
            // Animation timer for smooth effects
            animationTimer = new Timer { Interval = 16 }; // ~60 FPS
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
            
            InitializeMapPanel();
            InitializeControlPanel();
        }
        
        private void InitializeMapPanel()
        {
            mapPanel = new Panel
            {
                Location = new Point(0, 0),
                Size = new Size(900, 900),
                BackColor = Color.FromArgb(230, 240, 250),
                BorderStyle = BorderStyle.FixedSingle
            };
            mapPanel.Paint += MapPanel_Paint;
            
            // Control key widget in center
            controlKey = new ControlKeyWidget
            {
                Location = new Point(350, 350),
                Size = new Size(200, 200)
            };
            controlKey.KeyTurned += ControlKey_Turned;
            
            mapPanel.Controls.Add(controlKey);
            this.Controls.Add(mapPanel);
        }
        
        private void InitializeControlPanel()
        {
            controlPanel = new Panel
            {
                Location = new Point(900, 0),
                Size = new Size(500, 900),
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                AutoScroll = true
            };
            
            int yPos = 20;
            
            // Title
            var titleLabel = new Label
            {
                Text = "Control Panel",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                Location = new Point(20, yPos),
                Size = new Size(460, 30),
                ForeColor = Color.FromArgb(0, 120, 215)
            };
            controlPanel.Controls.Add(titleLabel);
            yPos += 40;
            
            // Missile Type Selection
            AddLabel("Missile Type:", 20, yPos);
            yPos += 25;
            missileTypeCombo = new ComboBox
            {
                Location = new Point(20, yPos),
                Width = 460,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 10F)
            };
            missileTypeCombo.Items.AddRange(new object[] 
            { 
                "Conventional", 
                "Nuclear", 
                "Thermonuclear", 
                "Thaumonuclear" 
            });
            missileTypeCombo.SelectedIndex = 0;
            missileTypeCombo.SelectedIndexChanged += MissileType_Changed;
            controlPanel.Controls.Add(missileTypeCombo);
            yPos += 40;
            
            // Coordinates
            AddLabel("Latitude:", 20, yPos);
            yPos += 25;
            latTextBox = CreateTextBox(20, yPos, "40.7128");
            yPos += 40;
            
            AddLabel("Longitude:", 20, yPos);
            yPos += 25;
            lngTextBox = CreateTextBox(20, yPos, "-74.0060");
            yPos += 40;
            
            AddLabel("Radius (km):", 20, yPos);
            yPos += 25;
            radiusTextBox = CreateTextBox(20, yPos, "5");
            yPos += 40;
            
            plotButton = CreateButton("Plot Event Point", 20, yPos);
            plotButton.Click += PlotButton_Click;
            yPos += 50;
            
            // Missile ID and Launch Code
            AddLabel("Missile ID:", 20, yPos);
            yPos += 25;
            missileIdLabel = new Label
            {
                Location = new Point(20, yPos),
                Size = new Size(460, 25),
                Font = new Font("Consolas", 10F, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Text = "None"
            };
            controlPanel.Controls.Add(missileIdLabel);
            yPos += 35;
            
            AddLabel("Required Launch Code:", 20, yPos);
            yPos += 25;
            requiredCodeLabel = new Label
            {
                Location = new Point(20, yPos),
                Size = new Size(460, 25),
                Font = new Font("Consolas", 12F, FontStyle.Bold),
                ForeColor = Color.Red,
                Text = "XXX-XX-XXX"
            };
            controlPanel.Controls.Add(requiredCodeLabel);
            yPos += 35;
            
            // Launch Code Input with FIXED cursor positioning
            AddLabel("Enter Launch Code (XXX-XX-XXX):", 20, yPos);
            yPos += 25;
            launchCodeTextBox = new TextBox
            {
                Location = new Point(20, yPos),
                Width = 460,
                Font = new Font("Consolas", 14F, FontStyle.Bold),
                MaxLength = 10,
                ForeColor = Color.Black,
                BackColor = Color.White
            };
            // Fix cursor positioning by handling text changes properly
            launchCodeTextBox.TextChanged += LaunchCode_TextChanged;
            launchCodeTextBox.KeyPress += LaunchCode_KeyPress;
            controlPanel.Controls.Add(launchCodeTextBox);
            yPos += 40;
            
            // Encryption Section
            AddSectionHeader("Encryption Demo", 20, yPos);
            yPos += 35;
            
            AddLabel("Encrypted Payload:", 20, yPos);
            yPos += 25;
            encryptedTextBox = new TextBox
            {
                Location = new Point(20, yPos),
                Width = 460,
                Height = 80,
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Consolas", 9F),
                ScrollBars = ScrollBars.Vertical
            };
            controlPanel.Controls.Add(encryptedTextBox);
            yPos += 90;
            
            encryptButton = CreateButton("Encrypt Activation Data", 20, yPos);
            encryptButton.Click += EncryptButton_Click;
            yPos += 50;
            
            decryptButton = CreateButton("Decrypt & Verify", 20, yPos);
            decryptButton.Click += DecryptButton_Click;
            yPos += 50;
            
            activateButton = CreateButton("🚀 ACTIVATE SIMULATION", 20, yPos);
            activateButton.BackColor = Color.FromArgb(220, 53, 69);
            activateButton.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            activateButton.Click += ActivateButton_Click;
            activateButton.Enabled = false;
            yPos += 60;
            
            // Console
            AddSectionHeader("Event Log", 20, yPos);
            yPos += 35;
            consoleTextBox = new TextBox
            {
                Location = new Point(20, yPos),
                Width = 460,
                Height = 150,
                Multiline = true,
                ReadOnly = true,
                ScrollBars = ScrollBars.Vertical,
                Font = new Font("Consolas", 9F),
                BackColor = Color.Black,
                ForeColor = Color.Lime
            };
            controlPanel.Controls.Add(consoleTextBox);
            
            this.Controls.Add(controlPanel);
        }
        
        private void AddLabel(string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F)
            };
            controlPanel.Controls.Add(label);
        }
        
        private void AddSectionHeader(string text, int x, int y)
        {
            var label = new Label
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(460, 25),
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(0, 120, 215)
            };
            controlPanel.Controls.Add(label);
        }
        
        private TextBox CreateTextBox(int x, int y, string defaultValue = "")
        {
            var textBox = new TextBox
            {
                Location = new Point(x, y),
                Width = 460,
                Font = new Font("Segoe UI", 10F),
                Text = defaultValue
            };
            controlPanel.Controls.Add(textBox);
            return textBox;
        }
        
        private Button CreateButton(string text, int x, int y)
        {
            var button = new Button
            {
                Text = text,
                Location = new Point(x, y),
                Size = new Size(460, 40),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            button.FlatAppearance.BorderSize = 0;
            controlPanel.Controls.Add(button);
            return button;
        }
        
        // FIX: Proper launch code input with auto-dash and correct cursor positioning
        private bool isUpdatingLaunchCode = false;
        
        private void LaunchCode_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Only allow digits and control keys
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }
        
        private void LaunchCode_TextChanged(object sender, EventArgs e)
        {
            if (isUpdatingLaunchCode) return;
            
            isUpdatingLaunchCode = true;
            
            // Remove all non-digits
            string digitsOnly = new string(launchCodeTextBox.Text.Where(char.IsDigit).ToArray());
            
            // Limit to 8 digits
            if (digitsOnly.Length > 8)
                digitsOnly = digitsOnly.Substring(0, 8);
            
            // Format with dashes: XXX-XX-XXX
            string formatted = "";
            
            for (int i = 0; i < digitsOnly.Length; i++)
            {
                if (i == 3 || i == 5)
                {
                    formatted += "-";
                }
                formatted += digitsOnly[i];
            }
            
            // Calculate correct cursor position AFTER the last digit
            int selectionStart = formatted.Length;
            
            launchCodeTextBox.Text = formatted;
            
            // Set cursor position AFTER the last character (fixes cursor behind number issue)
            launchCodeTextBox.SelectionStart = selectionStart;
            launchCodeTextBox.SelectionLength = 0;
            
            isUpdatingLaunchCode = false;
        }
        
        private void MissileType_Changed(object sender, EventArgs e)
        {
            LogEvent($"Missile type changed to: {missileTypeCombo.SelectedItem}");
        }
        
        private void PlotButton_Click(object sender, EventArgs e)
        {
            try
            {
                double lat = double.Parse(latTextBox.Text);
                double lng = double.Parse(lngTextBox.Text);
                double radius = double.Parse(radiusTextBox.Text);
                string missileType = missileTypeCombo.SelectedItem.ToString();
                
                // Generate missile ID
                currentMissileId = $"{missileType.ToUpper()}-{Math.Abs(lat * lng):F2}";
                missileIdLabel.Text = currentMissileId;
                
                // Generate required launch code
                requiredLaunchCode = CryptoHelper.GenerateLaunchCode();
                requiredCodeLabel.Text = requiredLaunchCode;
                
                // Add event point
                var point = new EventPoint
                {
                    Name = missileType,
                    Lat = lat,
                    Lng = lng,
                    Radius = radius,
                    EventType = missileType,
                    Color = GetColorForMissileType(missileType)
                };
                eventPoints.Add(point);
                
                mapPanel.Invalidate();
                
                LogEvent($"Event point plotted: {missileType} at ({lat}, {lng}), radius {radius}km");
                LogEvent($"Missile ID: {currentMissileId}");
                LogEvent($"Launch code required: {requiredLaunchCode}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Invalid input: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void EncryptButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentMissileId))
            {
                MessageBox.Show("Please plot an event point first", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // Create payload
            string payload = $"{{\"operator\":\"{operatorId}\",\"event\":\"{missileTypeCombo.SelectedItem}\",\"coords\":[{latTextBox.Text},{lngTextBox.Text}],\"radius_km\":{radiusTextBox.Text}}}";
            
            // Generate encryption key
            encryptionKey = CryptoHelper.GenerateLaunchCode();
            
            try
            {
                string encrypted = CryptoHelper.EncryptAES(payload, encryptionKey);
                encryptedTextBox.Text = encrypted;
                
                LogEvent($"Payload encrypted with key: {encryptionKey}");
                MessageBox.Show($"Encryption successful!\n\nEncryption Key: {encryptionKey}\n\nStore this key for decryption.", 
                    "Encryption Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Encryption failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void DecryptButton_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(encryptedTextBox.Text))
            {
                MessageBox.Show("No encrypted data available", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            
            // Create custom input dialog
            using (var inputDialog = new Form())
            {
                inputDialog.Text = "Decrypt";
                inputDialog.Size = new Size(400, 150);
                inputDialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                inputDialog.StartPosition = FormStartPosition.CenterParent;
                inputDialog.MaximizeBox = false;
                inputDialog.MinimizeBox = false;
                
                var label = new Label { Text = "Enter decryption key:", Left = 20, Top = 20, Width = 340 };
                var textBox = new TextBox { Left = 20, Top = 45, Width = 340, Text = encryptionKey };
                var okButton = new Button { Text = "OK", Left = 220, Top = 80, DialogResult = DialogResult.OK };
                var cancelButton = new Button { Text = "Cancel", Left = 300, Top = 80, DialogResult = DialogResult.Cancel };
                
                okButton.Click += (s, ev) => inputDialog.Close();
                cancelButton.Click += (s, ev) => inputDialog.Close();
                
                inputDialog.Controls.Add(label);
                inputDialog.Controls.Add(textBox);
                inputDialog.Controls.Add(okButton);
                inputDialog.Controls.Add(cancelButton);
                inputDialog.AcceptButton = okButton;
                inputDialog.CancelButton = cancelButton;
                
                if (inputDialog.ShowDialog() != DialogResult.OK)
                    return;
                
                string key = textBox.Text;
                
                if (string.IsNullOrEmpty(key)) return;
                
                try
                {
                    string decrypted = CryptoHelper.DecryptAES(encryptedTextBox.Text, key);
                    LogEvent("Decryption successful");
                    LogEvent($"Payload: {decrypted}");
                    MessageBox.Show($"Decrypted payload:\n\n{decrypted}", "Decryption Successful", 
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    
                    activateButton.Enabled = true;
                }
                catch (Exception ex)
                {
                    LogEvent("Decryption failed: " + ex.Message);
                    MessageBox.Show("Decryption failed: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        
        private void ActivateButton_Click(object sender, EventArgs e)
        {
            // Verify launch code
            string enteredCode = launchCodeTextBox.Text.Trim();
            
            if (enteredCode != requiredLaunchCode)
            {
                MessageBox.Show($"Invalid launch code!\n\nRequired: {requiredLaunchCode}\nEntered: {enteredCode}", 
                    "Authentication Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogEvent("Activation failed: Invalid launch code");
                return;
            }
            
            if (!controlKey.IsUnlocked)
            {
                MessageBox.Show("Control key must be turned first!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Trigger explosion animation
            if (eventPoints.Count > 0)
            {
                var lastPoint = eventPoints[eventPoints.Count - 1];
                explosionAnimation = new ExplosionAnimation(lastPoint);
                LogEvent($"SIMULATION ACTIVATED - {lastPoint.EventType} event at ({lastPoint.Lat}, {lastPoint.Lng})");
                MessageBox.Show("Simulation Activated!\n\nWatch the map for the explosion animation.", 
                    "Activation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private void ControlKey_Turned(object sender, EventArgs e)
        {
            LogEvent("Control key turned - system unlocked");
        }
        
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            // Update explosion animation
            if (explosionAnimation != null && !explosionAnimation.IsComplete)
            {
                explosionAnimation.Update();
                mapPanel.Invalidate();
            }
            
            // Update control key animation
            if (controlKey.IsAnimating)
            {
                mapPanel.Invalidate();
            }
        }
        
        private void MapPanel_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            
            // Draw background grid
            using (Pen gridPen = new Pen(Color.FromArgb(200, 220, 230), 1))
            {
                for (int i = 0; i < mapPanel.Width; i += 50)
                    g.DrawLine(gridPen, i, 0, i, mapPanel.Height);
                for (int i = 0; i < mapPanel.Height; i += 50)
                    g.DrawLine(gridPen, 0, i, mapPanel.Width, i);
            }
            
            // Draw event points
            foreach (var point in eventPoints)
            {
                DrawEventPoint(g, point);
            }
            
            // Draw explosion animation
            if (explosionAnimation != null && !explosionAnimation.IsComplete)
            {
                explosionAnimation.Draw(g, mapPanel.Width, mapPanel.Height);
            }
        }
        
        private void DrawEventPoint(Graphics g, EventPoint point)
        {
            // Convert lat/lng to screen coordinates (simple projection for demo)
            float x = mapPanel.Width / 2 + (float)(point.Lng * 3);
            float y = mapPanel.Height / 2 - (float)(point.Lat * 3);
            
            // Draw outer effect radius
            float outerRadius = (float)(point.Radius * 5);
            using (Brush brush = new SolidBrush(Color.FromArgb(30, point.Color)))
            {
                g.FillEllipse(brush, x - outerRadius, y - outerRadius, outerRadius * 2, outerRadius * 2);
            }
            
            // Draw inner impact radius
            float innerRadius = outerRadius * 0.5f;
            using (Brush brush = new SolidBrush(Color.FromArgb(60, point.Color)))
            {
                g.FillEllipse(brush, x - innerRadius, y - innerRadius, innerRadius * 2, innerRadius * 2);
            }
            
            // Draw center marker
            using (Brush brush = new SolidBrush(point.Color))
            {
                g.FillEllipse(brush, x - 5, y - 5, 10, 10);
            }
            
            // Draw label
            using (Font font = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(Color.Black))
            {
                g.DrawString(point.Name, font, textBrush, x + 10, y - 10);
            }
        }
        
        private Color GetColorForMissileType(string type)
        {
            switch (type)
            {
                case "Conventional": return Color.FromArgb(255, 165, 0);
                case "Nuclear": return Color.FromArgb(255, 0, 0);
                case "Thermonuclear": return Color.FromArgb(255, 0, 255);
                case "Thaumonuclear": return Color.FromArgb(0, 255, 255);
                default: return Color.Gray;
            }
        }
        
        private void LogEvent(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            consoleTextBox.AppendText($"[{timestamp}] {message}\r\n");
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            animationTimer?.Stop();
            animationTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
    
    // Event point data structure
    public class EventPoint
    {
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double Radius { get; set; }
        public string EventType { get; set; }
        public Color Color { get; set; }
    }
    
    // Explosion animation class
    public class ExplosionAnimation
    {
        private EventPoint point;
        private float animationProgress = 0;
        private const float AnimationSpeed = 0.02f;
        public bool IsComplete => animationProgress >= 1.0f;
        
        public ExplosionAnimation(EventPoint point)
        {
            this.point = point;
        }
        
        public void Update()
        {
            animationProgress += AnimationSpeed;
        }
        
        public void Draw(Graphics g, int width, int height)
        {
            float x = width / 2 + (float)(point.Lng * 3);
            float y = height / 2 - (float)(point.Lat * 3);
            
            // Multi-stage explosion animation
            float radius = (float)(point.Radius * 5 * animationProgress * 3);
            int alpha = (int)(255 * (1 - animationProgress));
            
            // Outer blast wave
            using (Brush brush = new SolidBrush(Color.FromArgb(Math.Max(0, alpha / 2), 255, 100, 0)))
            {
                g.FillEllipse(brush, x - radius, y - radius, radius * 2, radius * 2);
            }
            
            // Inner fireball
            float innerRadius = radius * 0.6f;
            using (Brush brush = new SolidBrush(Color.FromArgb(Math.Max(0, alpha), 255, 200, 0)))
            {
                g.FillEllipse(brush, x - innerRadius, y - innerRadius, innerRadius * 2, innerRadius * 2);
            }
            
            // Core
            float coreRadius = radius * 0.3f;
            using (Brush brush = new SolidBrush(Color.FromArgb(Math.Max(0, alpha), 255, 255, 255)))
            {
                g.FillEllipse(brush, x - coreRadius, y - coreRadius, coreRadius * 2, coreRadius * 2);
            }
        }
    }
    
    // Control key widget with rotation animation
    public class ControlKeyWidget : Control
    {
        private float rotationAngle = 0;
        private bool isDragging = false;
        private Point lastMousePos;
        public bool IsUnlocked => rotationAngle >= 90;
        public bool IsAnimating => isDragging;
        
        public event EventHandler KeyTurned;
        
        public ControlKeyWidget()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);
        }
        
        protected override void OnMouseDown(MouseEventArgs e)
        {
            isDragging = true;
            lastMousePos = e.Location;
            base.OnMouseDown(e);
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (isDragging)
            {
                int dx = e.X - lastMousePos.X;
                rotationAngle += dx * 0.5f;
                rotationAngle = Math.Max(0, Math.Min(90, rotationAngle));
                
                if (rotationAngle >= 90 && KeyTurned != null)
                {
                    KeyTurned(this, EventArgs.Empty);
                }
                
                lastMousePos = e.Location;
                Invalidate();
            }
            base.OnMouseMove(e);
        }
        
        protected override void OnMouseUp(MouseEventArgs e)
        {
            isDragging = false;
            base.OnMouseUp(e);
        }
        
        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int centerX = Width / 2;
            int centerY = Height / 2;
            
            // Draw key housing
            using (Brush brush = new SolidBrush(Color.FromArgb(50, 50, 50)))
            {
                g.FillEllipse(brush, 20, 20, Width - 40, Height - 40);
            }
            
            // Draw key
            g.TranslateTransform(centerX, centerY);
            g.RotateTransform(rotationAngle);
            
            using (Brush keyBrush = new SolidBrush(IsUnlocked ? Color.Green : Color.Gold))
            {
                g.FillRectangle(keyBrush, -10, -60, 20, 80);
                g.FillEllipse(keyBrush, -15, -70, 30, 30);
            }
            
            g.ResetTransform();
            
            // Draw label
            string label = IsUnlocked ? "UNLOCKED" : "LOCKED";
            using (Font font = new Font("Segoe UI", 10F, FontStyle.Bold))
            using (Brush textBrush = new SolidBrush(IsUnlocked ? Color.Green : Color.Red))
            {
                SizeF textSize = g.MeasureString(label, font);
                g.DrawString(label, font, textBrush, 
                    centerX - textSize.Width / 2, Height - 30);
            }
        }
    }
}
