using System;
using System.Windows.Forms;

namespace MissileSimulator
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// Educational simulation demonstrating mapping, cryptography, and UI design.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            
            // Show disclaimer first
            DialogResult result = MessageBox.Show(
                "EDUCATIONAL DISCLAIMER\n\n" +
                "This is a safe simulation for learning mapping, cryptography, and UI design.\n" +
                "This application is NOT intended for planning or enabling any harmful activity.\n\n" +
                "The application demonstrates:\n" +
                "- Geographic mapping and geodesic mathematics\n" +
                "- AES encryption and secure key derivation (PBKDF2)\n" +
                "- Authentication flows and session management\n" +
                "- Windows Forms UI animations and effects\n\n" +
                "All event types (meteor, volcanic eruption, emergency drill, fireworks) are neutral and safe.\n\n" +
                "Do you understand and agree to use this for educational purposes only?",
                "Educational Simulation - Disclaimer",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Information);
            
            if (result != DialogResult.Yes)
            {
                return;
            }
            
            // Show authentication form first
            using (var authForm = new AuthForm())
            {
                if (authForm.ShowDialog() == DialogResult.OK)
                {
                    // Authentication successful, show main form
                    Application.Run(new MainForm(authForm.SessionToken, authForm.OperatorId));
                }
            }
        }
    }
}
