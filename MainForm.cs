using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace MissileSimulator
{
    public partial class MainForm : Form
    {
        private readonly string sessionToken;
        private readonly string operatorId;
        private HashSet<string> validAuthCodes;
        private Dictionary<string, EventProfile> eventProfiles;
        
        public MainForm(string sessionToken, string operatorId)
        {
            this.sessionToken = sessionToken;
            this.operatorId = operatorId;
            
            InitializeEventProfiles();
            LoadAuthCodes();
            
            // Start workflow
            this.Load += MainForm_Load;
        }
        
        private void MainForm_Load(object sender, EventArgs e)
        {
            this.Hide();
            StartWorkflow();
        }
        
        private void StartWorkflow()
        {
            // Stage 1: Target Selection
            using (var targetForm = new TargetSelectionForm())
            {
                if (targetForm.ShowDialog() != DialogResult.OK)
                {
                    Application.Exit();
                    return;
                }
                
                double targetLat = targetForm.TargetLat;
                double targetLng = targetForm.TargetLng;
                string weaponType = targetForm.SelectedWeaponType;
                string launchBase = targetForm.SelectedBase;
                
                EventProfile profile = eventProfiles[weaponType];
                
                // Stage 2: Encryption
                using (var encryptForm = new EncryptionForm(operatorId, validAuthCodes))
                {
                    encryptForm.TargetLat = targetLat;
                    encryptForm.TargetLng = targetLng;
                    encryptForm.WeaponType = weaponType;
                    encryptForm.LaunchBase = launchBase;
                    encryptForm.WeaponYield = profile.YieldMT;
                    encryptForm.SetTargetInfo();
                    
                    if (encryptForm.ShowDialog() != DialogResult.OK)
                    {
                        Application.Exit();
                        return;
                    }
                    
                    string missileId = encryptForm.MissileId;
                    string missileName = encryptForm.MissileName;
                    string authCode = encryptForm.AuthCode;
                    
                    // Stage 3: Authorization Code
                    using (var authForm = new AuthCodeForm())
                    {
                        authForm.RequiredAuthCode = authCode;
                        authForm.MissileId = missileId;
                        authForm.MissileName = missileName;
                        authForm.SetMissileInfo();
                        
                        if (authForm.ShowDialog() != DialogResult.OK)
                        {
                            Application.Exit();
                            return;
                        }
                        
                        // Stage 4: Launch Control
                        using (var launchControl = new LaunchControlForm())
                        {
                            launchControl.MissileId = missileId;
                            launchControl.MissileName = missileName;
                            launchControl.LaunchBase = launchBase;
                            launchControl.SetMissileInfo();
                            
                            var result = launchControl.ShowDialog();
                            
                            if (result == DialogResult.Cancel)
                            {
                                MessageBox.Show("Mission aborted. Returning to target selection.",
                                    "ABORTED", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                StartWorkflow(); // Restart from beginning
                                return;
                            }
                            
                            if (result == DialogResult.OK && launchControl.LaunchConfirmed)
                            {
                                // Stage 5: Missile Launch & Detonation
                                using (var launchForm = new MissileLaunchForm())
                                {
                                    launchForm.TargetLat = targetLat;
                                    launchForm.TargetLng = targetLng;
                                    launchForm.LaunchBase = launchBase;
                                    launchForm.MissileId = missileId;
                                    launchForm.MissileName = missileName;
                                    launchForm.DamageZones = profile.Zones;
                                    launchForm.WeaponYield = profile.YieldMT;
                                    
                                    launchForm.StartLaunchSequence();
                                    launchForm.ShowDialog();
                                }
                                
                                // Mission complete, ask if want to run another
                                var again = MessageBox.Show("Mission complete. Start new mission?",
                                    "COMPLETE", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                
                                if (again == DialogResult.Yes)
                                {
                                    StartWorkflow();
                                }
                                else
                                {
                                    Application.Exit();
                                }
                                return;
                            }
                        }
                    }
                }
            }
            
            Application.Exit();
        }
        
        private void InitializeEventProfiles()
        {
            eventProfiles = new Dictionary<string, EventProfile>
            {
                ["Conventional"] = new EventProfile
                {
                    Name = "Conventional",
                    YieldMT = 0.0001,
                    Zones = new List<DamageZone>
                    {
                        new DamageZone { Radius = 0.3, Color = Color.FromArgb(200, 255, 0, 0), Label = "Direct Impact" },
                        new DamageZone { Radius = 0.8, Color = Color.FromArgb(150, 255, 100, 0), Label = "Blast Radius" },
                        new DamageZone { Radius = 2.0, Color = Color.FromArgb(100, 255, 165, 0), Label = "Shrapnel Zone" }
                    }
                },
                ["Nuclear"] = new EventProfile
                {
                    Name = "Nuclear",
                    YieldMT = 0.5,
                    Zones = new List<DamageZone>
                    {
                        new DamageZone { Radius = 0.8, Color = Color.FromArgb(220, 255, 255, 0), Label = "Fireball" },
                        new DamageZone { Radius = 2.5, Color = Color.FromArgb(200, 255, 0, 0), Label = "20 PSI - Total Destruction" },
                        new DamageZone { Radius = 4.5, Color = Color.FromArgb(150, 255, 100, 0), Label = "10 PSI - Heavy Damage" },
                        new DamageZone { Radius = 8.0, Color = Color.FromArgb(120, 255, 150, 0), Label = "5 PSI - Moderate Damage" },
                        new DamageZone { Radius = 12.0, Color = Color.FromArgb(80, 200, 200, 0), Label = "1 PSI - Light Damage" }
                    }
                },
                ["Thermonuclear"] = new EventProfile
                {
                    Name = "Thermonuclear",
                    YieldMT = 10.4,
                    Zones = new List<DamageZone>
                    {
                        new DamageZone { Radius = 0.421, Color = Color.FromArgb(240, 150, 0, 0), Label = "Crater (421m)" },
                        new DamageZone { Radius = 0.72, Color = Color.FromArgb(230, 200, 0, 0), Label = "3000 PSI - Silo Hardened" },
                        new DamageZone { Radius = 1.76, Color = Color.FromArgb(220, 255, 0, 0), Label = "200 PSI - Extreme Damage" },
                        new DamageZone { Radius = 3.22, Color = Color.FromArgb(200, 255, 100, 255), Label = "1000 rem - Fatal Radiation" },
                        new DamageZone { Radius = 3.4, Color = Color.FromArgb(180, 200, 150, 255), Label = "500 rem - Likely Fatal" },
                        new DamageZone { Radius = 3.57, Color = Color.FromArgb(240, 255, 255, 100), Label = "Fireball - Vaporization" },
                        new DamageZone { Radius = 3.78, Color = Color.FromArgb(160, 150, 200, 255), Label = "100 rem - Radiation Sickness" },
                        new DamageZone { Radius = 4.75, Color = Color.FromArgb(200, 255, 50, 0), Label = "20 PSI - Heavy Blast Damage" },
                        new DamageZone { Radius = 9.99, Color = Color.FromArgb(150, 255, 100, 0), Label = "5 PSI - Moderate Blast Damage" },
                        new DamageZone { Radius = 25.7, Color = Color.FromArgb(120, 255, 150, 50), Label = "1 PSI - Light Blast Damage" },
                        new DamageZone { Radius = 29.1, Color = Color.FromArgb(100, 255, 200, 100), Label = "3rd Degree Burns" },
                        new DamageZone { Radius = 63.1, Color = Color.FromArgb(60, 200, 200, 150), Label = "Thermal Radiation Boundary" }
                    }
                },
                ["Thaumonuclear"] = new EventProfile
                {
                    Name = "Thaumonuclear",
                    YieldMT = 50.0,
                    Zones = new List<DamageZone>
                    {
                        new DamageZone { Radius = 5.0, Color = Color.FromArgb(240, 200, 0, 255), Label = "Reality Breach" },
                        new DamageZone { Radius = 12.0, Color = Color.FromArgb(200, 150, 0, 200), Label = "Exotic Energy" },
                        new DamageZone { Radius = 25.0, Color = Color.FromArgb(160, 100, 0, 150), Label = "Anomalous Effects" },
                        new DamageZone { Radius = 50.0, Color = Color.FromArgb(120, 75, 0, 130), Label = "Extended Influence" },
                        new DamageZone { Radius = 80.0, Color = Color.FromArgb(80, 50, 0, 100), Label = "Residual Anomalies" }
                    }
                }
            };
        }
        
        private void LoadAuthCodes()
        {
            validAuthCodes = new HashSet<string>();
            string authFile = "AuthCodes.txt";
            
            try
            {
                if (!File.Exists(authFile))
                {
                    // Generate sample auth codes
                    for (int i = 0; i < 20; i++)
                    {
                        string code = GenerateAuthCode();
                        validAuthCodes.Add(code);
                    }
                    
                    File.WriteAllLines(authFile, validAuthCodes);
                }
                else
                {
                    var lines = File.ReadAllLines(authFile);
                    foreach (var line in lines)
                    {
                        string code = line.Trim();
                        if (!string.IsNullOrEmpty(code))
                        {
                            validAuthCodes.Add(code);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading auth codes: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        
        private string GenerateAuthCode()
        {
            var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[6];
            rng.GetBytes(bytes);
            
            int part1 = (bytes[0] % 10) * 100 + (bytes[1] % 10) * 10 + (bytes[2] % 10);
            int part2 = (bytes[3] % 10) * 10 + (bytes[4] % 10);
            int part3 = (bytes[5] % 10) * 100 + (bytes[0] % 10) * 10 + (bytes[1] % 10);
            
            return $"{part1:D3}-{part2:D2}-{part3:D3}";
        }
    }
    
    public class EventProfile
    {
        public string Name { get; set; }
        public double YieldMT { get; set; }
        public List<DamageZone> Zones { get; set; }
    }
}
