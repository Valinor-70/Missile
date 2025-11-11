using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace MissileSimulator
{
    public partial class LaunchControlForm : Form
    {
        private Panel pnlControlKey;
        private Button btnAbort;
        private RichTextBox txtConsole;
        private Label lblMissileInfo;
        
        private float keyRotation = 0f;
        private bool isDraggingKey = false;
        private Point lastMousePos;
        
        public string MissileId { get; set; } = string.Empty;
        public string MissileName { get; set; } = string.Empty;
        public string LaunchBase { get; set; } = string.Empty;
        
        public bool LaunchConfirmed { get; private set; } = false;
        
        public LaunchControlForm()
        {
            InitializeComponent();
            
            // Apply modern composition effects
            this.Load += (s, e) => CompositionHelper.ApplyModernStyle(this);
        }
        
        private void InitializeComponent()
        {
            this.Text = "STAGE 4: LAUNCH CONTROL";
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 10, 15);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            
            int y = 20;
            
            // Modern Header with intense glow
            var pnlHeader = CompositionHelper.CreateModernPanel(Color.FromArgb(5, 5, 10), withGlow: true);
            pnlHeader.Location = new Point(20, y);
            pnlHeader.Size = new Size(660, 60);
            
            var lblTitle = CompositionHelper.CreateAnimatedLabel(
                "███ SCP FOUNDATION - LAUNCH CONTROL ███",
                new Font("Courier New", 11, FontStyle.Bold),
                Color.FromArgb(255, 0, 0),
                0
            );
            lblTitle.Location = new Point(10, 8);
            lblTitle.Size = new Size(640, 22);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            pnlHeader.Controls.Add(lblTitle);
            CompositionHelper.ApplyGlitchEffect(lblTitle);
            
            var lblStage = CompositionHelper.CreateAnimatedLabel(
                "STAGE 4: TURN CONTROL KEY 90° TO INITIATE LAUNCH",
                new Font("Courier New", 9, FontStyle.Bold),
                Color.FromArgb(255, 200, 0),
                200
            );
            lblStage.Location = new Point(10, 33);
            lblStage.Size = new Size(640, 20);
            lblStage.TextAlign = ContentAlignment.MiddleCenter;
            pnlHeader.Controls.Add(lblStage);
            
            this.Controls.Add(pnlHeader);
            y += 80;
            
            // Missile info
            lblMissileInfo = new Label
            {
                Text = "MISSILE: Awaiting data...",
                Font = new Font("Consolas", 9, FontStyle.Bold),
                Location = new Point(40, y),
                Size = new Size(620, 40),
                ForeColor = Color.FromArgb(255, 180, 0),
                TextAlign = ContentAlignment.TopCenter
            };
            this.Controls.Add(lblMissileInfo);
            y += 50;
            
            // Control key group
            var grpKey = new GroupBox
            {
                Text = "━━━ LAUNCH AUTHORIZATION KEY ━━━",
                Location = new Point(40, y),
                Size = new Size(620, 200),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            var lblInstruction = new Label
            {
                Text = "DRAG KEY TO ROTATE 90° CLOCKWISE TO ARM LAUNCH",
                Location = new Point(15, 25),
                Size = new Size(590, 20),
                ForeColor = Color.FromArgb(255, 255, 100),
                Font = new Font("Consolas", 8, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleCenter
            };
            grpKey.Controls.Add(lblInstruction);
            
            // Control key panel
            pnlControlKey = new Panel
            {
                Location = new Point(260, 55),
                Size = new Size(100, 100),
                BackColor = Color.Transparent
            };
            pnlControlKey.Paint += PnlControlKey_Paint;
            pnlControlKey.MouseDown += PnlControlKey_MouseDown;
            pnlControlKey.MouseMove += PnlControlKey_MouseMove;
            pnlControlKey.MouseUp += PnlControlKey_MouseUp;
            grpKey.Controls.Add(pnlControlKey);
            
            // Abort button
            btnAbort = new Button
            {
                Text = "[ABORT MISSION]",
                Location = new Point(215, 165),
                Size = new Size(190, 25),
                BackColor = Color.FromArgb(100, 100, 0),
                ForeColor = Color.FromArgb(255, 255, 200),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 9, FontStyle.Bold)
            };
            btnAbort.FlatAppearance.BorderColor = Color.FromArgb(200, 200, 0);
            btnAbort.Click += BtnAbort_Click;
            grpKey.Controls.Add(btnAbort);
            
            this.Controls.Add(grpKey);
            y += 210;
            
            // Console
            var grpConsole = new GroupBox
            {
                Text = "━━━ LAUNCH LOG ━━━",
                Location = new Point(40, y),
                Size = new Size(620, 180),
                ForeColor = Color.FromArgb(200, 200, 220),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            txtConsole = new RichTextBox
            {
                Location = new Point(10, 22),
                Size = new Size(600, 150),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 100),
                Font = new Font("Consolas", 8),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpConsole.Controls.Add(txtConsole);
            
            this.Controls.Add(grpConsole);
            
            LogToConsole(">>> LAUNCH CONTROL INITIALIZED");
            LogToConsole(">>> LAUNCH KEY ENABLED");
            LogToConsole(">>> TURN KEY 90° TO INITIATE LAUNCH");
        }
        
        public void SetMissileInfo()
        {
            lblMissileInfo.Text = $"MISSILE: {MissileName}\nID: {MissileId}\nBASE: {LaunchBase}";
            LogToConsole($">>> MISSILE: {MissileName}");
            LogToConsole($">>> BASE: {LaunchBase}");
        }
        
        private void PnlControlKey_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int centerX = pnlControlKey.Width / 2;
            int centerY = pnlControlKey.Height / 2;
            
            // Outer glow
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(centerX - 48, centerY - 48, 96, 96);
                using (var pgb = new PathGradientBrush(path))
                {
                    pgb.CenterPoint = new PointF(centerX, centerY);
                    pgb.CenterColor = Color.FromArgb(80, 255, 0, 0);
                    pgb.SurroundColors = new[] { Color.Transparent };
                    g.FillPath(pgb, path);
                }
            }
            
            var state = g.Save();
            g.TranslateTransform(centerX, centerY);
            g.RotateTransform(keyRotation);
            
            // Key base with gradient
            Color keyColor = Color.FromArgb(220, 0, 0);
            Color keyDark = Color.FromArgb(150, 0, 0);
            
            using (var brush = new LinearGradientBrush(
                new Rectangle(-40, -40, 80, 80),
                keyColor,
                keyDark,
                LinearGradientMode.Vertical))
            {
                g.FillEllipse(brush, -40, -40, 80, 80);
            }
            
            // Border
            using (var pen = new Pen(Color.FromArgb(200, 200, 200), 3))
            {
                g.DrawEllipse(pen, -40, -40, 80, 80);
            }
            
            // Key handle
            using (var brush = new LinearGradientBrush(
                new Rectangle(-5, -30, 10, 60),
                keyColor,
                keyDark,
                LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, -5, -30, 10, 60);
            }
            
            // Key teeth
            using (var brush = new SolidBrush(keyColor))
            {
                g.FillRectangle(brush, -15, 25, 10, 5);
                g.FillRectangle(brush, 5, 25, 10, 5);
                g.FillRectangle(brush, -10, 30, 6, 3);
            }
            
            g.Restore(state);
            
            // Label
            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                string text = "ARMED";
                var size = g.MeasureString(text, font);
                
                using (var brush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                {
                    g.DrawString(text, font, brush, centerX - size.Width / 2 + 1, centerY - size.Height / 2 + 1);
                }
                
                using (var brush = new SolidBrush(Color.White))
                {
                    g.DrawString(text, font, brush, centerX - size.Width / 2, centerY - size.Height / 2);
                }
            }
            
            // Rotation indicator
            using (var pen = new Pen(Color.FromArgb(200, 255, 255, 0), 2))
            {
                float angle = keyRotation * (float)Math.PI / 180f;
                float x = centerX + 35 * (float)Math.Cos(angle - Math.PI / 2);
                float y = centerY + 35 * (float)Math.Sin(angle - Math.PI / 2);
                g.DrawLine(pen, centerX, centerY, x, y);
            }
        }
        
        private void PnlControlKey_MouseDown(object sender, MouseEventArgs e)
        {
            isDraggingKey = true;
            lastMousePos = e.Location;
        }
        
        private void PnlControlKey_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingKey)
            {
                int centerX = pnlControlKey.Width / 2;
                int centerY = pnlControlKey.Height / 2;
                
                double angle1 = Math.Atan2(lastMousePos.Y - centerY, lastMousePos.X - centerX);
                double angle2 = Math.Atan2(e.Y - centerY, e.X - centerX);
                double deltaAngle = (angle2 - angle1) * (180.0 / Math.PI);
                
                keyRotation += (float)deltaAngle;
                keyRotation = Math.Max(0, Math.Min(90, keyRotation));
                
                lastMousePos = e.Location;
                pnlControlKey.Invalidate();
            }
        }
        
        private void PnlControlKey_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingKey = false;
            
            if (Math.Abs(keyRotation - 90) < 15)
            {
                keyRotation = 90;
                pnlControlKey.Invalidate();
                
                LogToConsole($">>> LAUNCH KEY TURNED");
                LogToConsole($">>> LAUNCH SEQUENCE INITIATED");
                LogToConsole($">>> PROCEEDING TO MISSILE LAUNCH...");
                
                MessageBox.Show("LAUNCH KEY TURNED\n\nInitiating launch sequence...",
                    "LAUNCH INITIATED", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                
                LaunchConfirmed = true;
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
        
        private void BtnAbort_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("ABORT LAUNCH SEQUENCE?\n\nThis will terminate the mission.",
                "ABORT CONFIRMATION", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                
            if (result == DialogResult.Yes)
            {
                LogToConsole($">>> LAUNCH ABORTED");
                LogToConsole($">>> MISSION TERMINATED");
                
                this.DialogResult = DialogResult.Cancel;
                this.Close();
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
