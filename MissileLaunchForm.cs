using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace MissileSimulator
{
    public partial class MissileLaunchForm : Form
    {
        private GMapControl mapControl;
        private RichTextBox txtConsole;
        private GMapOverlay markersOverlay;
        private GMapOverlay circlesOverlay;
        
        private System.Windows.Forms.Timer missileAnimationTimer;
        private System.Windows.Forms.Timer detonationTimer;
        
        private int missileAnimationStep = 0;
        private const int MISSILE_ANIMATION_STEPS = 50;
        private int detonationZoneIndex = 0;
        
        public double TargetLat { get; set; }
        public double TargetLng { get; set; }
        public string LaunchBase { get; set; } = string.Empty;
        public string MissileId { get; set; } = string.Empty;
        public string MissileName { get; set; } = string.Empty;
        public List<DamageZone> DamageZones { get; set; } = new List<DamageZone>();
        public double WeaponYield { get; set; }
        
        private PointLatLng baseLoc;
        private PointLatLng targetLoc;
        
        public MissileLaunchForm()
        {
            InitializeComponent();
            InitializeMap();
            
            // Apply modern composition effects
            this.Load += (s, e) => CompositionHelper.ApplyModernStyle(this);
        }
        
        private void InitializeComponent()
        {
            this.Text = "STAGE 5: MISSILE LAUNCH & DETONATION";
            this.Size = new Size(1400, 900);
            this.MinimumSize = new Size(1200, 800);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(10, 10, 15);
            this.FormClosing += MissileLaunchForm_FormClosing;
            
            // Map control
            mapControl = new GMapControl
            {
                Dock = DockStyle.Left,
                Size = new Size(1000, 850)
            };
            
            // Modern console panel with glow
            var consolePanel = CompositionHelper.CreateModernPanel(Color.FromArgb(20, 20, 30), withGlow: true);
            consolePanel.Dock = DockStyle.Right;
            consolePanel.Size = new Size(384, 850);
            
            var pnlHeader = CompositionHelper.CreateModernPanel(Color.FromArgb(5, 5, 10), withGlow: false);
            pnlHeader.Location = new Point(0, 10);
            pnlHeader.Size = new Size(384, 70);
            
            var lblTitle = CompositionHelper.CreateAnimatedLabel(
                "███ SCP FOUNDATION ███",
                new Font("Courier New", 11, FontStyle.Bold),
                Color.FromArgb(255, 0, 0),
                0
            );
            lblTitle.Location = new Point(10, 8);
            lblTitle.Size = new Size(364, 20);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            pnlHeader.Controls.Add(lblTitle);
            
            var lblStage = CompositionHelper.CreateAnimatedLabel(
                "STAGE 5: MISSILE FLIGHT & DETONATION",
                new Font("Courier New", 8, FontStyle.Bold),
                Color.FromArgb(255, 200, 0),
                200
            );
            lblStage.Location = new Point(10, 32);
            lblStage.Size = new Size(364, 18);
            lblStage.TextAlign = ContentAlignment.MiddleCenter;
            pnlHeader.Controls.Add(lblStage);
            
            var lblClass = CompositionHelper.CreateAnimatedLabel(
                "▸▸ SEQUENTIAL DETONATION IN PROGRESS ◂◂",
                new Font("Courier New", 7, FontStyle.Bold),
                Color.FromArgb(255, 100, 100),
                400
            );
            lblClass.Location = new Point(10, 50);
            lblClass.Size = new Size(364, 12);
            lblClass.TextAlign = ContentAlignment.MiddleCenter;
            pnlHeader.Controls.Add(lblClass);
            CompositionHelper.ApplyGlitchEffect(lblClass);
            
            consolePanel.Controls.Add(pnlHeader);
            
            var grpConsole = new GroupBox
            {
                Text = "━━━ LAUNCH LOG ━━━",
                Location = new Point(10, 90),
                Size = new Size(360, 750),
                ForeColor = Color.FromArgb(255, 100, 100),
                Font = new Font("Courier New", 8, FontStyle.Bold)
            };
            
            txtConsole = new RichTextBox
            {
                Location = new Point(10, 22),
                Size = new Size(340, 750),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.FromArgb(0, 255, 100),
                Font = new Font("Consolas", 8),
                BorderStyle = BorderStyle.FixedSingle
            };
            grpConsole.Controls.Add(txtConsole);
            
            consolePanel.Controls.Add(grpConsole);
            
            this.Controls.Add(consolePanel);
            this.Controls.Add(mapControl);
            
            LogToConsole(">>> LAUNCH STAGE INITIALIZED");
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
                
                markersOverlay = new GMapOverlay("markers");
                circlesOverlay = new GMapOverlay("circles");
                
                mapControl.Overlays.Add(circlesOverlay);
                mapControl.Overlays.Add(markersOverlay);
                
                // Missile animation timer
                missileAnimationTimer = new System.Windows.Forms.Timer { Interval = 40 }; // 25 FPS
                missileAnimationTimer.Tick += MissileAnimationTimer_Tick;
                
                // Detonation timer
                detonationTimer = new System.Windows.Forms.Timer { Interval = 300 }; // 300ms between zones
                detonationTimer.Tick += DetonationTimer_Tick;
                
                LogToConsole(">>> SATELLITE MAP LOADED");
            }
            catch (Exception ex)
            {
                LogToConsole($">>> MAP WARNING: {ex.Message}");
            }
        }
        
        public void StartLaunchSequence()
        {
            targetLoc = new PointLatLng(TargetLat, TargetLng);
            
            // Calculate base location
            string[] bases = { "ALPHA", "BRAVO", "CHARLIE", "DELTA", "ECHO", "FOXTROT", "GOLF", "HOTEL", "INDIA", "JULIET" };
            int baseIndex = Array.IndexOf(bases, LaunchBase);
            double baseLat = 40.0 + (baseIndex * 2);
            double baseLng = -100.0 + (baseIndex * 3);
            baseLoc = new PointLatLng(baseLat, baseLng);
            
            // Add base marker
            var baseMarker = new GMarkerGoogle(baseLoc, GMarkerGoogleType.green_big_go);
            markersOverlay.Markers.Add(baseMarker);
            
            // Add target marker
            var targetMarker = new GMarkerGoogle(targetLoc, GMarkerGoogleType.red_pushpin);
            markersOverlay.Markers.Add(targetMarker);
            
            mapControl.Position = baseLoc;
            mapControl.Zoom = 5;
            mapControl.Refresh();
            
            LogToConsole($">>> MISSILE: {MissileName}");
            LogToConsole($">>> ID: {MissileId}");
            LogToConsole($">>> LAUNCH BASE: {LaunchBase}");
            LogToConsole($">>> TARGET: {TargetLat:F6}, {TargetLng:F6}");
            LogToConsole($">>> YIELD: {WeaponYield} MT");
            LogToConsole($">>> LAUNCHING IN 3...");
            
            System.Threading.Thread.Sleep(1000);
            LogToConsole($">>> 2...");
            System.Threading.Thread.Sleep(1000);
            LogToConsole($">>> 1...");
            System.Threading.Thread.Sleep(1000);
            LogToConsole($">>> LAUNCH!");
            
            missileAnimationStep = 0;
            missileAnimationTimer.Start();
        }
        
        private void MissileAnimationTimer_Tick(object sender, EventArgs e)
        {
            missileAnimationStep++;
            
            if (missileAnimationStep >= MISSILE_ANIMATION_STEPS)
            {
                missileAnimationTimer.Stop();
                DetonateAtTarget();
                return;
            }
            
            // Calculate missile position
            double t = (double)missileAnimationStep / MISSILE_ANIMATION_STEPS;
            double lat = baseLoc.Lat + (targetLoc.Lat - baseLoc.Lat) * t;
            double lng = baseLoc.Lng + (targetLoc.Lng - baseLoc.Lng) * t;
            
            // Remove old missile marker
            var missileMarkers = markersOverlay.Markers.Where(m => m.ToolTipText == "MISSILE").ToList();
            foreach (var m in missileMarkers)
            {
                markersOverlay.Markers.Remove(m);
            }
            
            // Add new missile marker
            var missileMarker = new GMarkerGoogle(new PointLatLng(lat, lng), GMarkerGoogleType.arrow)
            {
                ToolTipText = "MISSILE"
            };
            markersOverlay.Markers.Add(missileMarker);
            
            // Follow missile
            mapControl.Position = new PointLatLng(lat, lng);
            mapControl.Refresh();
            
            if (missileAnimationStep % 10 == 0)
            {
                LogToConsole($">>> MISSILE EN ROUTE: {(t * 100):F0}%");
            }
        }
        
        private void DetonateAtTarget()
        {
            LogToConsole($">>> IMPACT AT TARGET");
            LogToConsole($">>> DETONATION INITIATED");
            LogToConsole($">>> YIELD: {WeaponYield} MT");
            LogToConsole($">>> ");
            
            // Center on target
            mapControl.Position = targetLoc;
            mapControl.Zoom = 8;
            
            // Remove missile marker
            var missileMarkers = markersOverlay.Markers.Where(m => m.ToolTipText == "MISSILE").ToList();
            foreach (var m in missileMarkers)
            {
                markersOverlay.Markers.Remove(m);
            }
            
            mapControl.Refresh();
            
            // Start zone detonation animation
            detonationZoneIndex = 0;
            detonationTimer.Start();
        }
        
        private void DetonationTimer_Tick(object sender, EventArgs e)
        {
            if (DamageZones != null && detonationZoneIndex < DamageZones.Count)
            {
                var zone = DamageZones[detonationZoneIndex];
                DrawGeodesicCircle(targetLoc.Lat, targetLoc.Lng, zone.Radius, zone.Color);
                LogToConsole($">>> {zone.Label}: {zone.Radius:F2} km");
                mapControl.Refresh();
                detonationZoneIndex++;
            }
            else
            {
                detonationTimer.Stop();
                LogToConsole($">>> ");
                LogToConsole($">>> DETONATION COMPLETE");
                LogToConsole($">>> MISSION ACCOMPLISHED");
                LogToConsole($">>> ALL ZONES RENDERED");
                
                MessageBox.Show($"DETONATION COMPLETE\n\nMISSILE: {MissileName}\nYIELD: {WeaponYield} MT\n\nAll {DamageZones.Count} damage zones rendered.",
                    "MISSION COMPLETE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        
        private void DrawGeodesicCircle(double centerLat, double centerLng, double radiusKm, Color color)
        {
            List<PointLatLng> points = new List<PointLatLng>();
            int numPoints = 64;
            
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
            const double R = 6371.0;
            double lat1Rad = lat1 * Math.PI / 180.0;
            double lon1Rad = lon1 * Math.PI / 180.0;
            double bearingRad = bearing * Math.PI / 180.0;
            
            double lat2Rad = Math.Asin(Math.Sin(lat1Rad) * Math.Cos(distanceKm / R) +
                                       Math.Cos(lat1Rad) * Math.Sin(distanceKm / R) * Math.Cos(bearingRad));
            
            double lon2Rad = lon1Rad + Math.Atan2(Math.Sin(bearingRad) * Math.Sin(distanceKm / R) * Math.Cos(lat1Rad),
                                                   Math.Cos(distanceKm / R) - Math.Sin(lat1Rad) * Math.Sin(lat2Rad));
            
            double lat2 = lat2Rad * 180.0 / Math.PI;
            double lon2 = lon2Rad * 180.0 / Math.PI;
            
            return new PointLatLng(lat2, lon2);
        }
        
        private void MissileLaunchForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (missileAnimationTimer != null)
                missileAnimationTimer.Stop();
            if (detonationTimer != null)
                detonationTimer.Stop();
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
    
    public class DamageZone
    {
        public double Radius { get; set; }
        public Color Color { get; set; }
        public string Label { get; set; }
    }
}
