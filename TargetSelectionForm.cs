using System;
using System.Drawing;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace MissileSimulator
{
    public partial class TargetSelectionForm : Form
    {
        private GMapControl mapControl;
        private Panel controlPanel;
        private TextBox txtLatitude;
        private TextBox txtLongitude;
        private ComboBox cmbEventType;
        private ComboBox cmbLaunchBase;
        private Button btnConfirmTarget;
        private RichTextBox txtConsole;
        
        public double TargetLat { get; private set; }
        public double TargetLng { get; private set; }
        public string SelectedWeaponType { get; private set; }
        public string SelectedBase { get; private set; }
        
        public TargetSelectionForm()
        {
            InitializeComponent();
            InitializeMap();
            
            // Apply modern composition effects
            this.Load += (s, e) => CompositionHelper.ApplyModernStyle(this);
        }
        
        private void InitializeComponent()
        {
            this.Text = "STAGE 1: TARGET SELECTION";
            this.Size = new Size(1400, 900);
            this.MinimumSize = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 10, 15); // Darker background
            
            // Map control
            mapControl = new GMapControl
            {
                Dock = DockStyle.Left,
                Size = new Size(1000, 850)
            };
            mapControl.MouseClick += MapControl_MouseClick;
            
            // Modern control panel with gradient
            controlPanel = CompositionHelper.CreateModernPanel(Color.FromArgb(20, 20, 30), withGlow: true);
            controlPanel.Dock = DockStyle.Right;
            controlPanel.Size = new Size(384, 850);
            controlPanel.AutoScroll = true;
            
            InitializeControlPanel();
            
            this.Controls.Add(controlPanel);
            this.Controls.Add(mapControl);
            
            // Add scanline overlay for terminal effect
            var scanlines = CompositionHelper.CreateScanlineOverlay();
            this.Controls.Add(scanlines);
            scanlines.BringToFront();
        }
        
        private void InitializeControlPanel()
        {
            int y = 10;
            
            // Modern Header with glow effect
            var pnlHeader = CompositionHelper.CreateModernPanel(Color.FromArgb(10, 10, 10), withGlow: false);
            pnlHeader.Location = new Point(0, y);
            pnlHeader.Size = new Size(384, 80);
            
            var lblTitle = CompositionHelper.CreateAnimatedLabel(
                "███ SCP FOUNDATION ███",
                new Font("Courier New", 12, FontStyle.Bold),
                Color.FromArgb(255, 0, 0),
                0
            );
            lblTitle.Location = new Point(10, 8);
            lblTitle.Size = new Size(364, 22);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            pnlHeader.Controls.Add(lblTitle);
            
            var lblSubtitle = CompositionHelper.CreateAnimatedLabel(
                "STAGE 1: TARGET ACQUISITION PROTOCOL",
                new Font("Courier New", 8, FontStyle.Regular),
                Color.FromArgb(200, 200, 200),
                200
            );
            lblSubtitle.Location = new Point(10, 32);
            lblSubtitle.Size = new Size(364, 15);
            lblSubtitle.TextAlign = ContentAlignment.MiddleCenter;
            pnlHeader.Controls.Add(lblSubtitle);
            
            var lblClass = CompositionHelper.CreateAnimatedLabel(
                "▸▸ RIGHT-CLICK MAP TO SELECT TARGET ◂◂",
                new Font("Courier New", 7, FontStyle.Bold),
                Color.FromArgb(100, 255, 100),
                400
            );
            lblClass.Location = new Point(10, 52);
            lblClass.Size = new Size(364, 12);
            lblClass.TextAlign = ContentAlignment.MiddleCenter;
            pnlHeader.Controls.Add(lblClass);
            CompositionHelper.ApplyGlitchEffect(lblClass);
            
            controlPanel.Controls.Add(pnlHeader);
            y += 90;
            
            // Target coordinates with modern styling
            var grpTarget = new GroupBox
            {
                Text = "━━━━ TARGET COORDINATES ━━━━",
                Location = new Point(10, y),
                Size = new Size(360, 130),
                ForeColor = Color.FromArgb(0, 200, 255),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            grpTarget.Controls.Add(new Label 
            { 
                Text = "LAT:", 
                Location = new Point(10, 28), 
                Size = new Size(40, 20),
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Consolas", 9, FontStyle.Bold)
            });
            
            txtLatitude = CompositionHelper.CreateTerminalTextBox(Color.FromArgb(0, 255, 100));
            txtLatitude.Location = new Point(55, 26);
            txtLatitude.Size = new Size(285, 20);
            txtLatitude.Text = "40.7128";
            grpTarget.Controls.Add(txtLatitude);
            
            grpTarget.Controls.Add(new Label 
            { 
                Text = "LONG:", 
                Location = new Point(10, 58), 
                Size = new Size(40, 20),
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Consolas", 9, FontStyle.Bold)
            });
            
            txtLongitude = CompositionHelper.CreateTerminalTextBox(Color.FromArgb(0, 255, 100));
            txtLongitude.Location = new Point(55, 56);
            txtLongitude.Size = new Size(285, 20);
            txtLongitude.Text = "-74.0060";
            grpTarget.Controls.Add(txtLongitude);
            
            btnConfirmTarget = CompositionHelper.CreateGlowButton("[CONFIRM TARGET]", Color.FromArgb(255, 0, 0));
            btnConfirmTarget.Location = new Point(10, 90);
            btnConfirmTarget.Size = new Size(330, 30);
            btnConfirmTarget.Click += BtnConfirmTarget_Click;
            CompositionHelper.ApplyPulsingGlow(btnConfirmTarget, Color.Red);
            grpTarget.Controls.Add(btnConfirmTarget);
            
            controlPanel.Controls.Add(grpTarget);
            y += 140;
            
            // Weapon selection
            var grpWeapon = new GroupBox
            {
                Text = "━━━ WEAPON SELECTION ━━━",
                Location = new Point(10, y),
                Size = new Size(360, 70),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            grpWeapon.Controls.Add(new Label 
            { 
                Text = "TYPE:", 
                Location = new Point(10, 28), 
                Size = new Size(50, 20),
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Consolas", 9)
            });
            cmbEventType = new ComboBox
            {
                Location = new Point(65, 26),
                Size = new Size(275, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(255, 200, 0),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 9, FontStyle.Bold)
            };
            cmbEventType.Items.AddRange(new object[] { "Conventional", "Nuclear", "Thermonuclear", "Thaumonuclear" });
            cmbEventType.SelectedIndex = 2; // Default to Thermonuclear
            grpWeapon.Controls.Add(cmbEventType);
            
            controlPanel.Controls.Add(grpWeapon);
            y += 80;
            
            // Launch base
            var grpBase = new GroupBox
            {
                Text = "━━━ LAUNCH FACILITY ━━━",
                Location = new Point(10, y),
                Size = new Size(360, 70),
                ForeColor = Color.FromArgb(200, 200, 220),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            grpBase.Controls.Add(new Label
            {
                Text = "BASE:",
                Location = new Point(10, 28),
                Size = new Size(50, 20),
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Consolas", 9)
            });
            
            cmbLaunchBase = new ComboBox
            {
                Location = new Point(65, 26),
                Size = new Size(275, 23),
                DropDownStyle = ComboBoxStyle.DropDownList,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Consolas", 9, FontStyle.Bold)
            };
            string[] bases = { "ALPHA", "BRAVO", "CHARLIE", "DELTA", "ECHO", "FOXTROT", "GOLF", "HOTEL", "INDIA", "JULIET" };
            cmbLaunchBase.Items.AddRange(bases);
            cmbLaunchBase.SelectedIndex = 0;
            grpBase.Controls.Add(cmbLaunchBase);
            
            controlPanel.Controls.Add(grpBase);
            y += 80;
            
            // Console
            var grpConsole = new GroupBox
            {
                Text = "━━━ MISSION LOG ━━━",
                Location = new Point(10, y),
                Size = new Size(360, 400),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            txtConsole = new RichTextBox
            {
                Location = new Point(10, 22),
                Size = new Size(340, 370),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 100),
                Font = new Font("Consolas", 8),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpConsole.Controls.Add(txtConsole);
            
            controlPanel.Controls.Add(grpConsole);
            
            LogToConsole(">>> SYSTEM INITIALIZED");
            LogToConsole(">>> AWAITING TARGET SELECTION");
        }
        
        private void InitializeMap()
        {
            try
            {
                GMapProvider.WebProxy = null;
                mapControl.MapProvider = GMapProviders.GoogleSatelliteMap;
                mapControl.Position = new PointLatLng(40.7128, -74.0060);
                mapControl.MinZoom = 2;
                mapControl.MaxZoom = 18;
                mapControl.Zoom = 6;
                mapControl.ShowCenter = false;
                mapControl.DragButton = MouseButtons.Left;
                mapControl.CanDragMap = true;
                mapControl.MouseWheelZoomEnabled = true;
                mapControl.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
                
                LogToConsole(">>> SATELLITE MAP LOADED");
            }
            catch (Exception ex)
            {
                LogToConsole($">>> MAP WARNING: {ex.Message}");
            }
        }
        
        private void MapControl_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                var point = mapControl.FromLocalToLatLng(e.X, e.Y);
                txtLatitude.Text = point.Lat.ToString("F6");
                txtLongitude.Text = point.Lng.ToString("F6");
                
                // Add marker
                var overlay = new GMapOverlay("markers");
                var marker = new GMarkerGoogle(point, GMarkerGoogleType.red_pushpin);
                overlay.Markers.Add(marker);
                
                mapControl.Overlays.Clear();
                mapControl.Overlays.Add(overlay);
                mapControl.Position = point;
                mapControl.Zoom = 8;
                
                LogToConsole($">>> TARGET: {point.Lat:F6}, {point.Lng:F6}");
            }
        }
        
        private void BtnConfirmTarget_Click(object sender, EventArgs e)
        {
            try
            {
                TargetLat = double.Parse(txtLatitude.Text);
                TargetLng = double.Parse(txtLongitude.Text);
                SelectedWeaponType = cmbEventType.SelectedItem?.ToString() ?? "Thermonuclear";
                SelectedBase = cmbLaunchBase.SelectedItem?.ToString() ?? "ALPHA";
                
                LogToConsole($">>> TARGET CONFIRMED");
                LogToConsole($">>> WEAPON: {SelectedWeaponType}");
                LogToConsole($">>> BASE: {SelectedBase}");
                LogToConsole($">>> PROCEEDING TO ENCRYPTION...");
                
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void LogToConsole(string message)
        {
            if (txtConsole != null && !txtConsole.IsDisposed)
            {
                if (txtConsole.InvokeRequired)
                {
                    txtConsole.Invoke(new Action(() => LogToConsole(message)));
                }
                else
                {
                    txtConsole.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                    txtConsole.ScrollToCaret();
                }
            }
        }
    }
}
