using System;
using System.Windows.Forms;

namespace MissileSimulator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Educational C# Windows Forms application demonstrating mapping, geodesic math,
        /// AES encryption, authentication, and animated UI effects.
        /// 
        /// EDUCATIONAL PURPOSE ONLY - This is a safe simulation for learning.
        /// Not intended for planning or enabling any harmful activity.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Enable high-quality visual styles for non-pixelated UI
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Enable high DPI awareness for crisp, high-detail rendering
            if (Environment.OSVersion.Version.Major >= 6)
            {
                SetProcessDPIAware();
            }
            
            // Show disclaimer first
            var disclaimer = new DisclaimerForm();
            if (disclaimer.ShowDialog() == DialogResult.OK)
            {
                // Show authentication dialog
                var authForm = new AuthForm();
                if (authForm.ShowDialog() == DialogResult.OK)
                {
                    Application.Run(new MainForm(authForm.OperatorId));
                }
            }
        }
        
        // Import SetProcessDPIAware for high DPI support
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
