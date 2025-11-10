using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace MissileSimulator
{
    /// <summary>
    /// Main application form demonstrating mapping, geodesic calculations, encryption, and UI effects.
    /// Educational demonstration only - all event types are safe and neutral.
    /// </summary>
    public partial class MainForm : Form
    {
        // Authentication
        private readonly string sessionToken;
        private readonly string operatorId;
        
        // Map control
        private GMapControl mapControl;
        private GMapOverlay markersOverlay;
        private GMapOverlay circlesOverlay;
        
        // Control panel components
        private Panel controlPanel;
        private TextBox txtLatitude;
        private TextBox txtLongitude;
        private TextBox txtRadius;
        private ComboBox cmbEventType;
        private Button btnPlotEvent;
        private Button btnLoadCSV;
        private Button btnClearMap;
        
        // Encryption components
        private TextBox txtEncryptionCode;
        private TextBox txtDecryptionCode;
        private TextBox txtEncryptedData;
        private Button btnEncrypt;
        private Button btnDecrypt;
        private Label lblMissileId;
        private TextBox txtAuthCode;
        private Button btnVerifyAuth;
        
        // Control key widget
        private Panel pnlControlKey;
        private float keyRotation = 0f;
        private bool keyEnabled = false;
        private bool isDraggingKey = false;
        private Point lastMousePos;
        
        // Timeline/Console
        private RichTextBox txtConsole;
        
        // Animation
        private System.Windows.Forms.Timer animationTimer;
        private List<AnimatedCircle> animatedCircles;
        
        // Event profiles
        private Dictionary<string, EventProfile> eventProfiles;
        private EventProfile currentProfile;
        
        // Encryption state
        private byte[] encryptionKey;
        private byte[] decryptionKey;
        private string encryptedPayload;
        private string currentMissileId;
        private HashSet<string> validAuthCodes;
        private string requiredAuthCode;
        
        public MainForm(string sessionToken, string operatorId)
        {
            this.sessionToken = sessionToken;
            this.operatorId = operatorId;
            
            animatedCircles = new List<AnimatedCircle>();
            InitializeEventProfiles();
            LoadAuthCodes();
            
            InitializeComponent();
            InitializeMap();
            
            // Log after all UI components are initialized
            LogToConsole($"System initialized. Operator: {operatorId}");
            LogToConsole($"Session: {sessionToken.Substring(0, 8)}...");
            LogToConsole("Authentication codes generated: AuthCodes.txt");
        }
        
        private void InitializeEventProfiles()
        {
            // Define event profiles with multiple realistic damage/fallout zones
            eventProfiles = new Dictionary<string, EventProfile>
            {
                ["Conventional"] = new EventProfile
                {
                    Name = "Conventional",
                    Zones = new List<DamageZone>
                    {
                        new DamageZone { Radius = 0.3, Color = Color.FromArgb(200, 255, 0, 0), Label = "Direct Impact" },
                        new DamageZone { Radius = 0.8, Color = Color.FromArgb(150, 255, 100, 0), Label = "Blast Radius" },
                        new DamageZone { Radius = 2.0, Color = Color.FromArgb(100, 255, 165, 0), Label = "Shrapnel Zone" }
                    },
                    InnerRadius = 0.3,
                    OuterRadius = 2.0,
                    InnerColor = Color.FromArgb(200, 255, 0, 0),
                    OuterColor = Color.FromArgb(100, 255, 165, 0),
                    Label = "Direct Impact / Blast / Shrapnel"
                },
                ["Nuclear"] = new EventProfile
                {
                    Name = "Nuclear",
                    Zones = new List<DamageZone>
                    {
                        new DamageZone { Radius = 0.8, Color = Color.FromArgb(220, 255, 255, 0), Label = "Fireball" },
                        new DamageZone { Radius = 2.5, Color = Color.FromArgb(200, 255, 0, 0), Label = "20 PSI Zone - Total Destruction" },
                        new DamageZone { Radius = 4.5, Color = Color.FromArgb(150, 255, 100, 0), Label = "10 PSI Zone - Heavy Damage" },
                        new DamageZone { Radius = 8.0, Color = Color.FromArgb(120, 255, 150, 0), Label = "5 PSI Zone - Moderate Damage" },
                        new DamageZone { Radius = 12.0, Color = Color.FromArgb(80, 200, 200, 0), Label = "Thermal Radiation" }
                    },
                    InnerRadius = 0.8,
                    OuterRadius = 12.0,
                    InnerColor = Color.FromArgb(220, 255, 255, 0),
                    OuterColor = Color.FromArgb(80, 200, 200, 0),
                    Label = "Fireball / PSI Zones / Thermal"
                },
                ["Thermonuclear"] = new EventProfile
                {
                    Name = "Thermonuclear",
                    Zones = new List<DamageZone>
                    {
                        new DamageZone { Radius = 2.0, Color = Color.FromArgb(240, 255, 255, 100), Label = "Fireball" },
                        new DamageZone { Radius = 6.0, Color = Color.FromArgb(220, 255, 0, 0), Label = "20 PSI - Vaporization" },
                        new DamageZone { Radius = 10.0, Color = Color.FromArgb(180, 255, 50, 0), Label = "10 PSI - Total Destruction" },
                        new DamageZone { Radius = 16.0, Color = Color.FromArgb(140, 255, 100, 0), Label = "5 PSI - Heavy Damage" },
                        new DamageZone { Radius = 25.0, Color = Color.FromArgb(100, 255, 150, 50), Label = "Thermal Burns" },
                        new DamageZone { Radius = 40.0, Color = Color.FromArgb(70, 150, 150, 100), Label = "Fallout Zone" }
                    },
                    InnerRadius = 2.0,
                    OuterRadius = 40.0,
                    InnerColor = Color.FromArgb(240, 255, 255, 100),
                    OuterColor = Color.FromArgb(70, 150, 150, 100),
                    Label = "Fireball / PSI Zones / Thermal / Fallout"
                },
                ["Thaumonuclear"] = new EventProfile
                {
                    Name = "Thaumonuclear",
                    Zones = new List<DamageZone>
                    {
                        new DamageZone { Radius = 5.0, Color = Color.FromArgb(240, 200, 0, 255), Label = "Reality Breach" },
                        new DamageZone { Radius = 12.0, Color = Color.FromArgb(200, 150, 0, 200), Label = "Exotic Energy" },
                        new DamageZone { Radius = 25.0, Color = Color.FromArgb(160, 100, 0, 150), Label = "Anomalous Effects" },
                        new DamageZone { Radius = 50.0, Color = Color.FromArgb(120, 75, 0, 130), Label = "Extended Influence" },
                        new DamageZone { Radius = 80.0, Color = Color.FromArgb(80, 50, 0, 100), Label = "Residual Anomalies" }
                    },
                    InnerRadius = 5.0,
                    OuterRadius = 80.0,
                    InnerColor = Color.FromArgb(240, 200, 0, 255),
                    OuterColor = Color.FromArgb(80, 50, 0, 100),
                    Label = "Reality Breach / Exotic Energy / Anomalies"
                }
            };
            
            currentProfile = eventProfiles["Conventional"];
        }
        
        private void LoadAuthCodes()
        {
            // Generate authentication codes and save to file
            validAuthCodes = new HashSet<string>();
            for (int i = 0; i < 100; i++)
            {
                validAuthCodes.Add(CryptoHelper.GenerateAuthCode());
            }
            
            // Save to file (logging happens after UI initialization)
            try
            {
                File.WriteAllLines("AuthCodes.txt", validAuthCodes);
            }
            catch (Exception)
            {
                // Will log error after UI is initialized
            }
        }
        
        private void InitializeComponent()
        {
            this.Text = "Missile Simulator";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(20, 20, 30); // Dark theme
            
            // Map control (fills most of the window)
            mapControl = new GMapControl
            {
                Location = new Point(0, 0),
                Size = new Size(1000, 850),
                Dock = DockStyle.None
            };
            
            // Control panel on the right with modern dark theme
            controlPanel = new Panel
            {
                Location = new Point(1000, 0),
                Size = new Size(384, 850),
                BackColor = Color.FromArgb(30, 30, 40),
                BorderStyle = BorderStyle.None,
                AutoScroll = true
            };
            
            InitializeControlPanel();
            
            // Add to form
            this.Controls.Add(mapControl);
            this.Controls.Add(controlPanel);
            
            // Animation timer
            animationTimer = new System.Windows.Forms.Timer { Interval = 50 }; // 20 FPS
            animationTimer.Tick += AnimationTimer_Tick;
            animationTimer.Start();
        }
        
        private void InitializeControlPanel()
        {
            int y = 5;
            
            // SCP Foundation style header
            var pnlHeader = new Panel
            {
                Location = new Point(0, y),
                Size = new Size(384, 60),
                BackColor = Color.Black
            };
            
            var lblSCPTitle = new Label
            {
                Text = "SCP FOUNDATION",
                Font = new Font("Courier New", 11, FontStyle.Bold),
                Location = new Point(10, 5),
                Size = new Size(364, 20),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White
            };
            pnlHeader.Controls.Add(lblSCPTitle);
            
            var lblSubtitle = new Label
            {
                Text = "STRATEGIC COMMAND PROTOCOL",
                Font = new Font("Courier New", 8, FontStyle.Regular),
                Location = new Point(10, 25),
                Size = new Size(364, 15),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(200, 200, 200)
            };
            pnlHeader.Controls.Add(lblSubtitle);
            
            var lblClass = new Label
            {
                Text = "■ CLEARANCE LEVEL: O5 ■",
                Font = new Font("Courier New", 7, FontStyle.Bold),
                Location = new Point(10, 42),
                Size = new Size(364, 12),
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.FromArgb(255, 50, 50)
            };
            pnlHeader.Controls.Add(lblClass);
            
            controlPanel.Controls.Add(pnlHeader);
            y += 65;
            
            // Authentication section
            var grpAuth = new GroupBox
            {
                Text = "━━━ AUTHENTICATION REQUIRED ━━━",
                Location = new Point(10, y),
                Size = new Size(360, 100),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            var lblAuthStatus = new Label
            {
                Text = $"OPERATOR: {operatorId}",
                Font = new Font("Consolas", 9, FontStyle.Bold),
                Location = new Point(10, 25),
                Size = new Size(340, 20),
                ForeColor = Color.FromArgb(100, 255, 100)
            };
            grpAuth.Controls.Add(lblAuthStatus);
            
            var lblSession = new Label
            {
                Text = $"SESSION: {sessionToken.Substring(0, 8).ToUpper()}",
                Font = new Font("Consolas", 9),
                Location = new Point(10, 45),
                Size = new Size(340, 20),
                ForeColor = Color.FromArgb(150, 150, 200)
            };
            grpAuth.Controls.Add(lblSession);
            
            var lblClearance = new Label
            {
                Text = "STATUS: AUTHORIZED FOR WEAPONS DEPLOYMENT",
                Font = new Font("Consolas", 8, FontStyle.Bold),
                Location = new Point(10, 65),
                Size = new Size(340, 20),
                ForeColor = Color.FromArgb(255, 200, 0)
            };
            grpAuth.Controls.Add(lblClearance);
            
            controlPanel.Controls.Add(grpAuth);
            y += 105;
            
            // Control key below authentication
            var grpControlKey = new GroupBox
            {
                Text = "━━━ LAUNCH AUTHORIZATION ━━━",
                Location = new Point(10, y),
                Size = new Size(360, 130),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            // Control key panel (moved here from map)
            pnlControlKey = new Panel
            {
                Location = new Point(130, 25), // Centered in group
                Size = new Size(100, 100),
                BackColor = Color.Transparent
            };
            pnlControlKey.Paint += PnlControlKey_Paint;
            pnlControlKey.MouseDown += PnlControlKey_MouseDown;
            pnlControlKey.MouseMove += PnlControlKey_MouseMove;
            pnlControlKey.MouseUp += PnlControlKey_MouseUp;
            
            grpControlKey.Controls.Add(pnlControlKey);
            controlPanel.Controls.Add(grpControlKey);
            y += 135;
            
            // Target coordinates
            var grpTarget = new GroupBox
            {
                Text = "━━━ TARGET COORDINATES ━━━",
                Location = new Point(10, y),
                Size = new Size(360, 140),
                ForeColor = Color.FromArgb(200, 200, 220),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            grpTarget.Controls.Add(new Label 
            { 
                Text = "LAT:", 
                Location = new Point(10, 25), 
                Size = new Size(40, 20),
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Consolas", 9)
            });
            txtLatitude = new TextBox 
            { 
                Location = new Point(55, 23), 
                Size = new Size(285, 20), 
                Text = "40.7128",
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 0),
                Font = new Font("Consolas", 10, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpTarget.Controls.Add(txtLatitude);
            
            grpTarget.Controls.Add(new Label 
            { 
                Text = "LONG:", 
                Location = new Point(10, 55), 
                Size = new Size(40, 20),
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Consolas", 9)
            });
            txtLongitude = new TextBox 
            { 
                Location = new Point(55, 53), 
                Size = new Size(285, 20), 
                Text = "-74.0060",
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 0),
                Font = new Font("Consolas", 10, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpTarget.Controls.Add(txtLongitude);
            
            grpTarget.Controls.Add(new Label 
            { 
                Text = "SCALE:", 
                Location = new Point(10, 85), 
                Size = new Size(50, 20),
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Consolas", 9)
            });
            txtRadius = new TextBox 
            { 
                Location = new Point(65, 83), 
                Size = new Size(60, 20), 
                Text = "5",
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 0),
                Font = new Font("Consolas", 10, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpTarget.Controls.Add(txtRadius);
            
            grpTarget.Controls.Add(new Label 
            { 
                Text = "km", 
                Location = new Point(130, 85), 
                Size = new Size(30, 20),
                ForeColor = Color.FromArgb(150, 150, 170),
                Font = new Font("Consolas", 9)
            });
            
            btnPlotEvent = new Button 
            { 
                Text = "[PLOT TARGET]", 
                Location = new Point(10, 110), 
                Size = new Size(110, 25),
                BackColor = Color.FromArgb(100, 0, 0),
                ForeColor = Color.FromArgb(255, 200, 200),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            btnPlotEvent.FlatAppearance.BorderColor = Color.FromArgb(200, 0, 0);
            btnPlotEvent.Click += BtnPlotEvent_Click;
            grpTarget.Controls.Add(btnPlotEvent);
            
            btnLoadCSV = new Button 
            { 
                Text = "[LOAD DATA]", 
                Location = new Point(125, 110), 
                Size = new Size(110, 25),
                BackColor = Color.FromArgb(0, 50, 100),
                ForeColor = Color.FromArgb(200, 220, 255),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            btnLoadCSV.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 200);
            btnLoadCSV.Click += BtnLoadCSV_Click;
            grpTarget.Controls.Add(btnLoadCSV);
            
            btnClearMap = new Button 
            { 
                Text = "[CLEAR]", 
                Location = new Point(240, 110), 
                Size = new Size(100, 25),
                BackColor = Color.FromArgb(60, 60, 60),
                ForeColor = Color.FromArgb(180, 180, 180),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            btnClearMap.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
            btnClearMap.Click += BtnClearMap_Click;
            grpTarget.Controls.Add(btnClearMap);
            
            controlPanel.Controls.Add(grpTarget);
            y += 145;
            
            // Weapon selection in SCP style
            var grpWeapon = new GroupBox
            {
                Text = "━━━ WEAPON SELECTION ━━━",
                Location = new Point(10, y),
                Size = new Size(360, 90),
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
            cmbEventType.SelectedIndex = 0;
            cmbEventType.SelectedIndexChanged += CmbEventType_SelectedIndexChanged;
            grpWeapon.Controls.Add(cmbEventType);
            
            var lblZoneInfo = new Label
            {
                Location = new Point(10, 55),
                Size = new Size(340, 25),
                Text = "⚠ Multiple damage zones will be plotted",
                Font = new Font("Consolas", 8),
                ForeColor = Color.FromArgb(255, 150, 0)
            };
            grpWeapon.Controls.Add(lblZoneInfo);
            
            controlPanel.Controls.Add(grpWeapon);
            y += 95;
            
            // Encryption in SCP style
            var grpEncryption = new GroupBox
            {
                Text = "━━━ ENCRYPTION PROTOCOL ━━━",
                Location = new Point(10, y),
                Size = new Size(360, 230),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            grpEncryption.Controls.Add(new Label 
            { 
                Text = "KEY:", 
                Location = new Point(10, 28), 
                Size = new Size(50, 20),
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Consolas", 9)
            });
            txtEncryptionCode = new TextBox 
            { 
                Location = new Point(65, 26), 
                Size = new Size(275, 20),
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 0),
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpEncryption.Controls.Add(txtEncryptionCode);
            
            btnEncrypt = new Button 
            { 
                Text = "[ENCRYPT & GENERATE ID]", 
                Location = new Point(10, 55), 
                Size = new Size(330, 28),
                BackColor = Color.FromArgb(80, 0, 80),
                ForeColor = Color.FromArgb(200, 150, 255),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            btnEncrypt.FlatAppearance.BorderColor = Color.FromArgb(150, 0, 150);
            btnEncrypt.Click += BtnEncrypt_Click;
            grpEncryption.Controls.Add(btnEncrypt);
            
            lblMissileId = new Label
            {
                Location = new Point(10, 90),
                Size = new Size(330, 20),
                Text = "ID: [AWAITING GENERATION]",
                Font = new Font("Consolas", 9, FontStyle.Bold),
                ForeColor = Color.FromArgb(255, 200, 0)
            };
            grpEncryption.Controls.Add(lblMissileId);
            
            grpEncryption.Controls.Add(new Label 
            { 
                Text = "AUTH:", 
                Location = new Point(10, 118), 
                Size = new Size(50, 20),
                ForeColor = Color.FromArgb(180, 180, 200),
                Font = new Font("Consolas", 9)
            });
            txtAuthCode = new TextBox 
            { 
                Location = new Point(65, 116), 
                Size = new Size(190, 20),
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(255, 255, 0),
                Font = new Font("Consolas", 9, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpEncryption.Controls.Add(txtAuthCode);
            
            btnVerifyAuth = new Button 
            { 
                Text = "[VERIFY]", 
                Location = new Point(260, 115), 
                Size = new Size(80, 23),
                BackColor = Color.FromArgb(0, 80, 0),
                ForeColor = Color.FromArgb(100, 255, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            btnVerifyAuth.FlatAppearance.BorderColor = Color.FromArgb(0, 150, 0);
            btnVerifyAuth.Click += BtnVerifyAuth_Click;
            grpEncryption.Controls.Add(btnVerifyAuth);
            
            grpEncryption.Controls.Add(new Label 
            { 
                Text = "ENCRYPTED DATA:", 
                Location = new Point(10, 148), 
                Size = new Size(130, 15),
                ForeColor = Color.FromArgb(150, 150, 170),
                Font = new Font("Consolas", 8)
            });
            txtEncryptedData = new TextBox
            {
                Location = new Point(10, 165),
                Size = new Size(330, 20),
                ReadOnly = true,
                BackColor = Color.FromArgb(5, 5, 10),
                ForeColor = Color.FromArgb(0, 200, 255),
                Font = new Font("Consolas", 7),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpEncryption.Controls.Add(txtEncryptedData);
            
            grpEncryption.Controls.Add(new Label 
            { 
                Text = "DECRYPT:", 
                Location = new Point(10, 193), 
                Size = new Size(70, 15),
                ForeColor = Color.FromArgb(150, 150, 170),
                Font = new Font("Consolas", 8)
            });
            txtDecryptionCode = new TextBox 
            { 
                Location = new Point(85, 191), 
                Size = new Size(180, 20),
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Consolas", 9),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpEncryption.Controls.Add(txtDecryptionCode);
            
            btnDecrypt = new Button 
            { 
                Text = "[DECRYPT]", 
                Location = new Point(270, 190), 
                Size = new Size(70, 23),
                BackColor = Color.FromArgb(80, 40, 0),
                ForeColor = Color.FromArgb(255, 200, 100),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            btnDecrypt.FlatAppearance.BorderColor = Color.FromArgb(150, 80, 0);
            btnDecrypt.Click += BtnDecrypt_Click;
            grpEncryption.Controls.Add(btnDecrypt);
            
            controlPanel.Controls.Add(grpEncryption);
            y += 235;
            
            // Mission log in SCP style
            var grpConsole = new GroupBox
            {
                Text = "━━━ MISSION LOG ━━━",
                Location = new Point(10, y),
                Size = new Size(360, 195),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            txtConsole = new RichTextBox
            {
                Location = new Point(10, 22),
                Size = new Size(340, 165),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 100),
                Font = new Font("Consolas", 8),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpConsole.Controls.Add(txtConsole);
            
            controlPanel.Controls.Add(grpConsole);
        }
        
        private void InitializeMap()
        {
            try
            {
                // Configure GMap.NET with better settings
                GMapProvider.WebProxy = null; // Disable proxy
                mapControl.MapProvider = GMapProviders.OpenStreetMap;
                mapControl.Position = new PointLatLng(40.7128, -74.0060); // New York
                mapControl.MinZoom = 2;
                mapControl.MaxZoom = 18;
                mapControl.Zoom = 6; // Start more zoomed out to avoid imagery issues
                mapControl.ShowCenter = false;
                mapControl.DragButton = MouseButtons.Left;
                mapControl.CanDragMap = true;
                mapControl.MouseWheelZoomEnabled = true;
                mapControl.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionWithoutCenter;
                
                // Performance settings
                mapControl.MaxZoom = 18;
                mapControl.MinZoom = 2;
                
                // Create overlays
                markersOverlay = new GMapOverlay("markers");
                circlesOverlay = new GMapOverlay("circles");
                
                mapControl.Overlays.Add(circlesOverlay);
                mapControl.Overlays.Add(markersOverlay);
                
                LogToConsole("Map initialized successfully");
            }
            catch (Exception ex)
            {
                LogToConsole($"Map initialization warning: {ex.Message}");
                // Continue anyway - map may still work
            }
        }
        
        private void CmbEventType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selected = cmbEventType.SelectedItem?.ToString();
            if (!string.IsNullOrEmpty(selected) && eventProfiles.ContainsKey(selected))
            {
                currentProfile = eventProfiles[selected];
                LogToConsole($"Event profile changed to: {selected}");
            }
        }
        
        private void BtnPlotEvent_Click(object sender, EventArgs e)
        {
            try
            {
                double lat = double.Parse(txtLatitude.Text);
                double lng = double.Parse(txtLongitude.Text);
                double radius = double.Parse(txtRadius.Text);
                
                PlotEvent(lat, lng, radius, currentProfile);
                LogToConsole($"Event plotted at ({lat}, {lng}) with radius {radius} km");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error plotting event: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void PlotEvent(double lat, double lng, double radius, EventProfile profile)
        {
            // Add marker
            var marker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.red_dot);
            markersOverlay.Markers.Add(marker);
            
            // Draw all damage zones if available
            if (profile.Zones != null && profile.Zones.Count > 0)
            {
                // Draw zones from largest to smallest for proper layering
                var sortedZones = profile.Zones.OrderByDescending(z => z.Radius).ToList();
                foreach (var zone in sortedZones)
                {
                    DrawGeodesicCircle(lat, lng, zone.Radius, zone.Color);
                    LogToConsole($"  Zone: {zone.Label} ({zone.Radius:F1} km)");
                }
            }
            else
            {
                // Fallback to old style
                DrawGeodesicCircle(lat, lng, profile.InnerRadius, profile.InnerColor);
                DrawGeodesicCircle(lat, lng, profile.OuterRadius, profile.OuterColor);
            }
            
            // Center map on the new point
            mapControl.Position = new PointLatLng(lat, lng);
            mapControl.Refresh();
        }
        
        private void DrawGeodesicCircle(double centerLat, double centerLng, double radiusKm, Color color)
        {
            // Use destination point formula for geodesic circle
            // This properly accounts for Earth's curvature
            List<PointLatLng> points = new List<PointLatLng>();
            int numPoints = 64; // Smooth circle
            
            for (int i = 0; i <= numPoints; i++)
            {
                double bearing = (360.0 / numPoints) * i;
                var point = CalculateDestinationPoint(centerLat, centerLng, radiusKm, bearing);
                points.Add(point);
            }
            
            var polygon = new GMapPolygon(points, "circle")
            {
                Fill = new SolidBrush(color),
                Stroke = new Pen(Color.FromArgb(200, color), 2)
            };
            
            circlesOverlay.Polygons.Add(polygon);
        }
        
        private PointLatLng CalculateDestinationPoint(double lat1, double lon1, double distanceKm, double bearing)
        {
            // Geodesic destination point calculation (Haversine formula)
            const double R = 6371.0; // Earth's radius in km
            double lat1Rad = lat1 * Math.PI / 180.0;
            double lon1Rad = lon1 * Math.PI / 180.0;
            double bearingRad = bearing * Math.PI / 180.0;
            
            double lat2Rad = Math.Asin(
                Math.Sin(lat1Rad) * Math.Cos(distanceKm / R) +
                Math.Cos(lat1Rad) * Math.Sin(distanceKm / R) * Math.Cos(bearingRad)
            );
            
            double lon2Rad = lon1Rad + Math.Atan2(
                Math.Sin(bearingRad) * Math.Sin(distanceKm / R) * Math.Cos(lat1Rad),
                Math.Cos(distanceKm / R) - Math.Sin(lat1Rad) * Math.Sin(lat2Rad)
            );
            
            return new PointLatLng(lat2Rad * 180.0 / Math.PI, lon2Rad * 180.0 / Math.PI);
        }
        
        private void BtnLoadCSV_Click(object sender, EventArgs e)
        {
            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                openFileDialog.Title = "Load Event Data";
                
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        LoadEventsFromCSV(openFileDialog.FileName);
                        LogToConsole($"Loaded events from: {Path.GetFileName(openFileDialog.FileName)}");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading CSV: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        
        private void LoadEventsFromCSV(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            int loaded = 0;
            
            for (int i = 1; i < lines.Length; i++) // Skip header
            {
                var parts = lines[i].Split(',');
                if (parts.Length >= 5)
                {
                    string name = parts[0].Trim();
                    double lat = double.Parse(parts[1].Trim());
                    double lng = double.Parse(parts[2].Trim());
                    double radius = double.Parse(parts[3].Trim());
                    string eventType = parts[4].Trim();
                    
                    EventProfile profile = eventProfiles.ContainsKey(eventType) 
                        ? eventProfiles[eventType] 
                        : currentProfile;
                    
                    PlotEvent(lat, lng, radius, profile);
                    loaded++;
                }
            }
            
            MessageBox.Show($"Loaded {loaded} events from CSV.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        
        private void BtnClearMap_Click(object sender, EventArgs e)
        {
            markersOverlay.Markers.Clear();
            circlesOverlay.Polygons.Clear();
            animatedCircles.Clear();
            mapControl.Refresh();
            LogToConsole("Map cleared");
        }
        
        private void BtnEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtEncryptionCode.Text))
                {
                    MessageBox.Show("Please enter an encryption code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                double lat = double.Parse(txtLatitude.Text);
                double lng = double.Parse(txtLongitude.Text);
                
                // Generate Missile ID
                currentMissileId = $"{currentProfile.Name.ToUpper()}-{Math.Abs(lat):F2}*{Math.Abs(lng):F2}";
                lblMissileId.Text = $"ID: [{currentMissileId}]";
                
                // Select random auth code
                requiredAuthCode = validAuthCodes.ElementAt(new Random().Next(validAuthCodes.Count));
                
                // Create payload
                var payload = new
                {
                    @operator = operatorId,
                    @event = currentProfile.Name,
                    coords = new[] { lat, lng },
                    radius_km = double.Parse(txtRadius.Text),
                    missile_id = currentMissileId,
                    auth_code = requiredAuthCode,
                    timestamp = DateTime.UtcNow.ToString("o")
                };
                
                string json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = false });
                
                // Derive encryption key from code
                byte[] salt = Encoding.UTF8.GetBytes("MissileSimSalt2024"); // Fixed salt for demo
                encryptionKey = CryptoHelper.DeriveKey(txtEncryptionCode.Text, salt);
                
                // Encrypt
                byte[] encrypted = CryptoHelper.EncryptAES(json, encryptionKey);
                encryptedPayload = Convert.ToBase64String(encrypted);
                txtEncryptedData.Text = encryptedPayload.Substring(0, Math.Min(50, encryptedPayload.Length)) + "...";
                
                LogToConsole($">>> ENCRYPTED: ID [{currentMissileId}]");
                LogToConsole($">>> REQUIRE AUTH: {requiredAuthCode}");
                LogToConsole($">>> TIMESTAMP: {DateTime.Now:HH:mm:ss}");
                
                MessageBox.Show($"ENCRYPTION SUCCESSFUL\n\nID: [{currentMissileId}]\n\nAUTH CODE REQUIRED:\n{requiredAuthCode}\n\n(See AuthCodes.txt)",
                    "SCP PROTOCOL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Encryption error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogToConsole($"Encryption error: {ex.Message}");
            }
        }
        
        private void BtnVerifyAuth_Click(object sender, EventArgs e)
        {
            string entered = txtAuthCode.Text.Trim();
            
            if (string.IsNullOrEmpty(requiredAuthCode))
            {
                MessageBox.Show("Please encrypt an event first to generate an auth code requirement.",
                    "No Auth Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            if (entered == requiredAuthCode)
            {
                MessageBox.Show("Authentication code verified successfully!\nControl key is now enabled.",
                    "Auth Verified", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LogToConsole($"Auth code verified: {entered}");
                
                // Enable control key after auth
                keyEnabled = true;
                pnlControlKey.Invalidate();
            }
            else
            {
                MessageBox.Show($"Invalid authentication code.\n\nExpected: {requiredAuthCode}\nEntered: {entered}",
                    "Auth Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogToConsole($"Auth verification failed");
            }
        }
        
        private void BtnDecrypt_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrEmpty(encryptedPayload))
                {
                    MessageBox.Show("No encrypted data available. Please encrypt an event first.",
                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                if (string.IsNullOrWhiteSpace(txtDecryptionCode.Text))
                {
                    MessageBox.Show("Please enter a decryption code.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Derive decryption key
                byte[] salt = Encoding.UTF8.GetBytes("MissileSimSalt2024");
                decryptionKey = CryptoHelper.DeriveKey(txtDecryptionCode.Text, salt);
                
                // Decrypt
                byte[] encrypted = Convert.FromBase64String(encryptedPayload);
                string decrypted = CryptoHelper.DecryptAES(encrypted, decryptionKey);
                
                MessageBox.Show($"Decryption successful!\n\nPayload:\n{decrypted}",
                    "Decryption Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                LogToConsole($"Decryption successful at {DateTime.Now:HH:mm:ss}");
            }
            catch (CryptographicException ex)
            {
                MessageBox.Show($"Decryption failed: {ex.Message}\n\nThis usually means:\n- Wrong decryption code\n- Data was tampered with",
                    "Decryption Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogToConsole($"Decryption failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Decryption error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                LogToConsole($"Decryption error: {ex.Message}");
            }
        }
        
        private void PnlControlKey_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            
            int centerX = pnlControlKey.Width / 2;
            int centerY = pnlControlKey.Height / 2;
            
            // Draw outer glow effect
            if (keyEnabled)
            {
                using (var path = new GraphicsPath())
                {
                    path.AddEllipse(centerX - 48, centerY - 48, 96, 96);
                    using (var pgb = new PathGradientBrush(path))
                    {
                        pgb.CenterPoint = new PointF(centerX, centerY);
                        pgb.CenterColor = Color.FromArgb(80, 0, 255, 0);
                        pgb.SurroundColors = new[] { Color.Transparent };
                        g.FillPath(pgb, path);
                    }
                }
            }
            
            // Save state
            var state = g.Save();
            g.TranslateTransform(centerX, centerY);
            g.RotateTransform(keyRotation);
            
            // Draw key base (circle) with gradient
            Color keyColor = keyEnabled ? Color.FromArgb(0, 220, 0) : Color.FromArgb(80, 80, 80);
            Color keyDark = keyEnabled ? Color.FromArgb(0, 150, 0) : Color.FromArgb(50, 50, 50);
            
            using (var brush = new LinearGradientBrush(
                new Rectangle(-40, -40, 80, 80),
                keyColor,
                keyDark,
                LinearGradientMode.Vertical))
            {
                g.FillEllipse(brush, -40, -40, 80, 80);
            }
            
            // Draw metallic border
            using (var pen = new Pen(Color.FromArgb(200, 200, 200), 3))
            {
                g.DrawEllipse(pen, -40, -40, 80, 80);
            }
            
            // Draw inner circle for depth
            using (var pen = new Pen(Color.FromArgb(150, 150, 150), 1))
            {
                g.DrawEllipse(pen, -35, -35, 70, 70);
            }
            
            // Draw key handle with gradient
            using (var brush = new LinearGradientBrush(
                new Rectangle(-5, -30, 10, 60),
                keyColor,
                keyDark,
                LinearGradientMode.Horizontal))
            {
                g.FillRectangle(brush, -5, -30, 10, 60);
            }
            
            // Draw key teeth
            using (var brush = new SolidBrush(keyColor))
            {
                g.FillRectangle(brush, -15, 25, 10, 5);
                g.FillRectangle(brush, 5, 25, 10, 5);
                g.FillRectangle(brush, -10, 30, 6, 3);
            }
            
            g.Restore(state);
            
            // Draw label with shadow
            using (var font = new Font("Segoe UI", 9, FontStyle.Bold))
            {
                string text = keyEnabled ? "ARMED" : "LOCKED";
                var size = g.MeasureString(text, font);
                
                // Shadow
                using (var brush = new SolidBrush(Color.FromArgb(100, 0, 0, 0)))
                {
                    g.DrawString(text, font, brush, centerX - size.Width / 2 + 1, centerY - size.Height / 2 + 1);
                }
                
                // Text
                Color textColor = keyEnabled ? Color.FromArgb(255, 255, 255) : Color.FromArgb(150, 150, 150);
                using (var brush = new SolidBrush(textColor))
                {
                    g.DrawString(text, font, brush, centerX - size.Width / 2, centerY - size.Height / 2);
                }
            }
            
            // Draw rotation indicator
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
            if (!keyEnabled)
            {
                MessageBox.Show("Control key is locked.\n\nRequirements:\n1. Encrypt an event\n2. Verify authentication code",
                    "Key Locked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            isDraggingKey = true;
            lastMousePos = e.Location;
        }
        
        private void PnlControlKey_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isDraggingKey) return;
            
            // Calculate rotation based on mouse movement
            int centerX = pnlControlKey.Width / 2;
            int centerY = pnlControlKey.Height / 2;
            
            double angle1 = Math.Atan2(lastMousePos.Y - centerY, lastMousePos.X - centerX);
            double angle2 = Math.Atan2(e.Y - centerY, e.X - centerX);
            
            double deltaAngle = (angle2 - angle1) * 180.0 / Math.PI;
            keyRotation += (float)deltaAngle;
            
            // Normalize rotation
            while (keyRotation < 0) keyRotation += 360;
            while (keyRotation >= 360) keyRotation -= 360;
            
            lastMousePos = e.Location;
            pnlControlKey.Invalidate();
            
            // Check if key has been turned enough (90 degrees)
            if (keyRotation >= 85 && keyRotation <= 95)
            {
                ActivateEvent();
                isDraggingKey = false;
                keyRotation = 0;
                pnlControlKey.Invalidate();
            }
        }
        
        private void PnlControlKey_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingKey = false;
        }
        
        private void ActivateEvent()
        {
            try
            {
                double lat = double.Parse(txtLatitude.Text);
                double lng = double.Parse(txtLongitude.Text);
                double radius = currentProfile.OuterRadius;
                
                // Create animated circle
                var animCircle = new AnimatedCircle
                {
                    CenterLat = lat,
                    CenterLng = lng,
                    MaxRadius = radius,
                    CurrentRadius = 0,
                    Color = currentProfile.InnerColor,
                    StartTime = DateTime.Now
                };
                animatedCircles.Add(animCircle);
                
                LogToConsole($"=== EVENT ACTIVATED ===");
                LogToConsole($"Time: {DateTime.Now:HH:mm:ss}");
                LogToConsole($"Location: ({lat}, {lng})");
                LogToConsole($"Profile: {currentProfile.Name}");
                LogToConsole($"Operator: {operatorId}");
                LogToConsole($"======================");
                
                MessageBox.Show($"Simulation Event Activated!\n\nType: {currentProfile.Name}\nLocation: ({lat}, {lng})\n\nActivation logged to timeline.",
                    "Activation Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Activation error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private void AnimationTimer_Tick(object sender, EventArgs e)
        {
            bool needsRefresh = false;
            
            // Update animated circles
            for (int i = animatedCircles.Count - 1; i >= 0; i--)
            {
                var circle = animatedCircles[i];
                double elapsed = (DateTime.Now - circle.StartTime).TotalSeconds;
                
                if (elapsed > 3.0) // Animation duration
                {
                    animatedCircles.RemoveAt(i);
                    continue;
                }
                
                // Expand and fade
                circle.CurrentRadius = circle.MaxRadius * (elapsed / 3.0);
                int alpha = (int)(200 * (1.0 - elapsed / 3.0));
                circle.Color = Color.FromArgb(Math.Max(0, alpha), circle.Color);
                
                needsRefresh = true;
            }
            
            if (needsRefresh)
            {
                // Redraw animated circles
                circlesOverlay.Polygons.Clear();
                
                foreach (var circle in animatedCircles)
                {
                    DrawGeodesicCircle(circle.CenterLat, circle.CenterLng, circle.CurrentRadius, circle.Color);
                }
                
                // Redraw static elements
                mapControl.Refresh();
            }
        }
        
        private void LogToConsole(string message)
        {
            // Null check to prevent NullReferenceException
            if (txtConsole == null)
                return;
                
            if (txtConsole.InvokeRequired)
            {
                txtConsole.Invoke(new Action(() => LogToConsole(message)));
                return;
            }
            
            string timestamp = DateTime.Now.ToString("HH:mm:ss");
            txtConsole.AppendText($"[{timestamp}] {message}\n");
            txtConsole.SelectionStart = txtConsole.Text.Length;
            txtConsole.ScrollToCaret();
        }
    }
    
    // Damage zone for realistic fallout/blast modeling
    public class DamageZone
    {
        public double Radius { get; set; } // km
        public Color Color { get; set; }
        public string Label { get; set; }
    }
    
    // Event profile data class
    public class EventProfile
    {
        public string Name { get; set; }
        public double InnerRadius { get; set; } // km
        public double OuterRadius { get; set; } // km
        public Color InnerColor { get; set; }
        public Color OuterColor { get; set; }
        public string Label { get; set; }
        public List<DamageZone> Zones { get; set; }
    }
    
    // Animated circle for visual effects
    public class AnimatedCircle
    {
        public double CenterLat { get; set; }
        public double CenterLng { get; set; }
        public double MaxRadius { get; set; }
        public double CurrentRadius { get; set; }
        public Color Color { get; set; }
        public DateTime StartTime { get; set; }
    }
}
