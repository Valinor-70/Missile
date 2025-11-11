using System;
using System.Drawing;
using System.Windows.Forms;

namespace MissileSimulator
{
    /// <summary>
    /// Disclaimer form shown at application startup
    /// </summary>
    public class DisclaimerForm : Form
    {
        private TextBox disclaimerText;
        private Button acceptButton;
        private Button declineButton;
        
        public DisclaimerForm()
        {
            InitializeComponent();
        }
        
        private void InitializeComponent()
        {
            this.Text = "Educational Disclaimer";
            this.Size = new Size(600, 400);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            
            // High-quality rendering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.UserPaint, true);
            this.UpdateStyles();
            
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(20)
            };
            
            disclaimerText = new TextBox
            {
                Multiline = true,
                ReadOnly = true,
                Dock = DockStyle.Top,
                Height = 280,
                Font = new Font("Segoe UI", 10F, FontStyle.Regular),
                BackColor = Color.White,
                Text = @"EDUCATIONAL DISCLAIMER

This is a safe simulation for learning mapping, cryptography, and user interface design.

This application demonstrates:
• Geodesic mathematics and mapping with GMap.NET
• AES encryption and secure password handling with PBKDF2
• Authentication workflows and session management
• Animated UI effects and visual feedback
• CSV data loading and visualization

NOT INTENDED FOR:
This application is NOT intended for planning or enabling any harmful activity. 
All simulated events are purely educational examples.

By clicking 'Accept', you acknowledge that this is an educational tool only.

© 2025 Educational Software - Safe Simulation Demo"
            };
            
            var buttonPanel = new FlowLayoutPanel
            {
                Dock = DockStyle.Bottom,
                Height = 50,
                FlowDirection = FlowDirection.RightToLeft,
                Padding = new Padding(0, 10, 0, 0)
            };
            
            acceptButton = new Button
            {
                Text = "Accept",
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            acceptButton.FlatAppearance.BorderSize = 0;
            acceptButton.Click += (s, e) => { DialogResult = DialogResult.OK; Close(); };
            
            declineButton = new Button
            {
                Text = "Decline",
                Size = new Size(100, 35),
                Font = new Font("Segoe UI", 10F),
                BackColor = Color.Gray,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 10, 0)
            };
            declineButton.FlatAppearance.BorderSize = 0;
            declineButton.Click += (s, e) => { DialogResult = DialogResult.Cancel; Close(); };
            
            buttonPanel.Controls.Add(acceptButton);
            buttonPanel.Controls.Add(declineButton);
            
            panel.Controls.Add(disclaimerText);
            panel.Controls.Add(buttonPanel);
            
            this.Controls.Add(panel);
        }
    }
}
