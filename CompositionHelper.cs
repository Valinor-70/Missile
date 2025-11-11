using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace MissileSimulator
{
    /// <summary>
    /// Composition API helper for modern UI effects including acrylic blur, shadows, and animations
    /// </summary>
    public static class CompositionHelper
    {
        // Windows DWM API for composition effects
        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        [DllImport("dwmapi.dll")]
        private static extern int DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct DWM_BLURBEHIND
        {
            public int dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;
        }

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        private const int DWMWA_BORDER_COLOR = 34;
        private const int DWMWA_CAPTION_COLOR = 35;
        private const int DWMWCP_ROUND = 2;

        /// <summary>
        /// Applies modern Windows 11 style rounded corners and dark mode to a form
        /// </summary>
        public static void ApplyModernStyle(Form form)
        {
            if (Environment.OSVersion.Version.Major >= 10)
            {
                try
                {
                    // Enable dark mode
                    int useDarkMode = 1;
                    DwmSetWindowAttribute(form.Handle, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useDarkMode, sizeof(int));

                    // Set rounded corners
                    int cornerPreference = DWMWCP_ROUND;
                    DwmSetWindowAttribute(form.Handle, DWMWA_WINDOW_CORNER_PREFERENCE, ref cornerPreference, sizeof(int));

                    // Set border color (SCP Foundation red)
                    int borderColor = ColorTranslator.ToWin32(Color.FromArgb(180, 0, 0));
                    DwmSetWindowAttribute(form.Handle, DWMWA_BORDER_COLOR, ref borderColor, sizeof(int));

                    // Set caption color (dark black)
                    int captionColor = ColorTranslator.ToWin32(Color.FromArgb(10, 10, 10));
                    DwmSetWindowAttribute(form.Handle, DWMWA_CAPTION_COLOR, ref captionColor, sizeof(int));
                }
                catch { /* Silently fail on older Windows versions */ }
            }
        }

        /// <summary>
        /// Applies acrylic blur effect behind a form (Windows 10+)
        /// </summary>
        public static void ApplyAcrylicBlur(Form form)
        {
            if (Environment.OSVersion.Version.Major >= 10)
            {
                try
                {
                    var blurBehind = new DWM_BLURBEHIND
                    {
                        dwFlags = 1,
                        fEnable = true,
                        hRgnBlur = IntPtr.Zero,
                        fTransitionOnMaximized = false
                    };
                    DwmEnableBlurBehindWindow(form.Handle, ref blurBehind);
                }
                catch { /* Silently fail */ }
            }
        }

        /// <summary>
        /// Creates a modern panel with gradient and shadow effects
        /// </summary>
        public static Panel CreateModernPanel(Color baseColor, bool withGlow = false)
        {
            var panel = new Panel
            {
                BackColor = baseColor,
                ForeColor = Color.White
            };

            panel.Paint += (s, e) =>
            {
                using (var brush = new LinearGradientBrush(
                    panel.ClientRectangle,
                    Color.FromArgb(255, baseColor),
                    Color.FromArgb(230, baseColor.R / 2, baseColor.G / 2, baseColor.B / 2),
                    LinearGradientMode.Vertical))
                {
                    e.Graphics.FillRectangle(brush, panel.ClientRectangle);
                }

                if (withGlow)
                {
                    // Draw inner glow
                    using (var pen = new Pen(Color.FromArgb(40, 255, 0, 0), 2))
                    {
                        e.Graphics.DrawRectangle(pen, new Rectangle(1, 1, panel.Width - 3, panel.Height - 3));
                    }
                }

                // Draw border
                using (var pen = new Pen(Color.FromArgb(80, 255, 255, 255), 1))
                {
                    e.Graphics.DrawRectangle(pen, new Rectangle(0, 0, panel.Width - 1, panel.Height - 1));
                }
            };

            return panel;
        }

        /// <summary>
        /// Creates a glowing button with SCP Foundation styling
        /// </summary>
        public static Button CreateGlowButton(string text, Color glowColor)
        {
            var button = new Button
            {
                Text = text,
                Font = new Font("Courier New", 10, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(40, 40, 50),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };

            button.FlatAppearance.BorderSize = 2;
            button.FlatAppearance.BorderColor = glowColor;
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(60, 60, 70);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 30, 40);

            button.Paint += (s, e) =>
            {
                // Draw glow effect
                using (var path = new GraphicsPath())
                {
                    path.AddRectangle(new Rectangle(0, 0, button.Width, button.Height));
                    using (var brush = new PathGradientBrush(path))
                    {
                        brush.CenterColor = Color.FromArgb(30, glowColor);
                        brush.SurroundColors = new[] { Color.Transparent };
                        e.Graphics.FillPath(brush, path);
                    }
                }
            };

            return button;
        }

        /// <summary>
        /// Creates a modern textbox with SCP Foundation terminal styling
        /// </summary>
        public static TextBox CreateTerminalTextBox(Color textColor)
        {
            return new TextBox
            {
                Font = new Font("Consolas", 10),
                BackColor = Color.FromArgb(15, 15, 20),
                ForeColor = textColor,
                BorderStyle = BorderStyle.FixedSingle
            };
        }

        /// <summary>
        /// Creates an animated label with fade-in effect
        /// </summary>
        public static Label CreateAnimatedLabel(string text, Font font, Color color, int delay = 0)
        {
            var label = new Label
            {
                Text = text,
                Font = font,
                ForeColor = Color.FromArgb(0, color), // Start transparent
                AutoSize = true
            };

            // Fade in animation
            var timer = new System.Windows.Forms.Timer { Interval = 50 };
            int alpha = 0;
            int startDelay = delay;

            timer.Tick += (s, e) =>
            {
                if (startDelay > 0)
                {
                    startDelay -= 50;
                    return;
                }

                if (alpha < 255)
                {
                    alpha = Math.Min(255, alpha + 15);
                    label.ForeColor = Color.FromArgb(alpha, color);
                }
                else
                {
                    timer.Stop();
                    timer.Dispose();
                }
            };

            timer.Start();
            return label;
        }

        /// <summary>
        /// Applies a pulsing glow animation to a control
        /// </summary>
        public static void ApplyPulsingGlow(Control control, Color glowColor)
        {
            var timer = new System.Windows.Forms.Timer { Interval = 30 };
            float alpha = 0f;
            float direction = 1f;

            timer.Tick += (s, e) =>
            {
                alpha += direction * 0.05f;
                if (alpha >= 1f)
                {
                    alpha = 1f;
                    direction = -1f;
                }
                else if (alpha <= 0.3f)
                {
                    alpha = 0.3f;
                    direction = 1f;
                }

                control.Invalidate();
            };

            control.Paint += (s, ev) =>
            {
                int glowAlpha = (int)(alpha * 80);
                using (var pen = new Pen(Color.FromArgb(glowAlpha, glowColor), 3))
                {
                    ev.Graphics.DrawRectangle(pen,
                        new Rectangle(2, 2, control.Width - 5, control.Height - 5));
                }
            };

            timer.Start();
        }

        /// <summary>
        /// Creates a scanline effect overlay for terminal aesthetics
        /// </summary>
        public static Panel CreateScanlineOverlay()
        {
            var overlay = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };

            overlay.Paint += (s, e) =>
            {
                // Draw scanlines
                using (var pen = new Pen(Color.FromArgb(8, 0, 255, 0), 1))
                {
                    for (int y = 0; y < overlay.Height; y += 4)
                    {
                        e.Graphics.DrawLine(pen, 0, y, overlay.Width, y);
                    }
                }
            };

            return overlay;
        }

        /// <summary>
        /// Creates a modern progress bar with SCP styling
        /// </summary>
        public static ProgressBar CreateModernProgressBar(Color fillColor)
        {
            var progressBar = new ProgressBar
            {
                Style = ProgressBarStyle.Continuous,
                Height = 25
            };

            return progressBar;
        }

        /// <summary>
        /// Applies a glitch effect to a label (cyberpunk/SCP aesthetic)
        /// </summary>
        public static void ApplyGlitchEffect(Label label)
        {
            var timer = new System.Windows.Forms.Timer { Interval = 3000 + new Random().Next(2000) };
            var originalText = label.Text;
            var random = new Random();

            timer.Tick += (s, e) =>
            {
                // Briefly glitch the text
                var chars = originalText.ToCharArray();
                for (int i = 0; i < Math.Min(3, chars.Length); i++)
                {
                    int pos = random.Next(chars.Length);
                    chars[pos] = (char)random.Next(33, 127);
                }
                label.Text = new string(chars);

                // Restore after 100ms
                var restoreTimer = new System.Windows.Forms.Timer { Interval = 100 };
                restoreTimer.Tick += (s2, e2) =>
                {
                    label.Text = originalText;
                    restoreTimer.Stop();
                    restoreTimer.Dispose();
                };
                restoreTimer.Start();

                timer.Interval = 3000 + random.Next(4000);
            };

            timer.Start();
        }
    }
}
