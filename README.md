# Educational Missile Simulator

## Overview
This is a safe, educational C# Windows Forms application that demonstrates:
- **High-detail, non-pixelated UI** with DPI awareness
- Mapping and geodesic mathematics
- AES encryption and PBKDF2 password hashing
- Authentication workflows
- Animated UI effects including missile explosion animations
- Secure launch code input with proper cursor positioning

## Key Features Implemented

### 1. High-Quality, Non-Pixelated UI
- **DPI Awareness**: The application uses `SetProcessDPIAware()` and includes a manifest file with `PerMonitorV2` DPI awareness
- **Double Buffering**: All controls use `OptimizedDoubleBuffer` to prevent flickering
- **AntiAliasing**: Graphics are rendered with `SmoothingMode.AntiAlias` and `TextRenderingHint.ClearTypeGridFit`
- **Modern UI**: Uses Segoe UI font family and proper styling for crisp rendering

### 2. Fixed Launch Code Auto-Dash with Correct Cursor Positioning
- **Auto-formatting**: Automatically formats input as XXX-XX-XXX
- **Cursor Fix**: The cursor now appears AFTER the last digit, not behind it
- **Smart Input**: Only accepts digits and auto-inserts dashes at correct positions
- Implementation in `LaunchCode_TextChanged()` and `LaunchCode_KeyPress()` methods

### 3. Missile Explosion Animation
- **Multi-stage Animation**: Shows blast wave, fireball, and core
- **Smooth 60 FPS**: Uses a timer running at 16ms intervals (~60 FPS)
- **Fade Effect**: Alpha channel decreases as explosion expands
- **Color Gradient**: White core → Yellow/orange fireball → Orange blast wave
- Implementation in `ExplosionAnimation` class

### 4. Other Features
- **Authentication**: PBKDF2-based password hashing with demo credentials
- **Encryption**: AES-256 encryption with HMAC authentication
- **Control Key Widget**: Interactive rotation widget that must be turned to unlock
- **Event Logging**: Console-style log with timestamps
- **Missile Types**: 4 different types with different visual parameters

## How to Build and Run

### Prerequisites
- .NET 6.0 or later SDK
- Windows OS (required for Windows Forms)

### Build Instructions

```bash
# Navigate to the project directory
cd /path/to/Missile

# Build the project
dotnet build

# Run the application
dotnet run
```

### Alternative: Direct Compilation
```bash
# Compile all files together
csc /target:winexe /out:MissileSimulator.exe /r:System.Windows.Forms.dll /r:System.Drawing.dll Program.cs DisclaimerForm.cs AuthForm.cs CryptoHelper.cs MainForm.cs
```

## Usage Instructions

1. **Accept Disclaimer**: Read and accept the educational disclaimer
2. **Login**: Use credentials `O5-X` / `HandofDemocracy`
3. **Plot Event**: 
   - Select missile type
   - Enter coordinates (latitude, longitude)
   - Enter radius in kilometers
   - Click "Plot Event Point"
4. **Encrypt Data**:
   - Click "Encrypt Activation Data"
   - Save the encryption key displayed
5. **Decrypt & Verify**:
   - Click "Decrypt & Verify"
   - Enter the encryption key when prompted
6. **Activate Simulation**:
   - Enter the required launch code (displayed above the input field)
   - Turn the control key by clicking and dragging
   - Click "🚀 ACTIVATE SIMULATION"
   - Watch the explosion animation on the map!

## Demo Credentials
- **Operator ID**: `O5-X` or `Ethics Committee`
- **Passphrase**: `HandofDemocracy`

## Technical Implementation Details

### UI Quality Improvements
```csharp
// DPI awareness for high-detail rendering
SetProcessDPIAware();

// Double buffering for smooth rendering
this.SetStyle(ControlStyles.OptimizedDoubleBuffer | 
             ControlStyles.AllPaintingInWmPaint |
             ControlStyles.UserPaint, true);

// Anti-aliased graphics
g.SmoothingMode = SmoothingMode.AntiAlias;
g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
```

### Launch Code Cursor Fix
```csharp
// Properly format and position cursor AFTER last digit
private void LaunchCode_TextChanged(object sender, EventArgs e)
{
    // Remove non-digits, format with dashes
    string digitsOnly = new string(launchCodeTextBox.Text.Where(char.IsDigit).ToArray());
    string formatted = FormatWithDashes(digitsOnly);
    
    // Set cursor at END of text (after last character)
    launchCodeTextBox.Text = formatted;
    launchCodeTextBox.SelectionStart = formatted.Length;
}
```

### Explosion Animation
```csharp
// Multi-layer explosion with fade effect
public class ExplosionAnimation
{
    private float animationProgress = 0;
    
    public void Draw(Graphics g, int width, int height)
    {
        float radius = baseRadius * animationProgress * 3;
        int alpha = (int)(255 * (1 - animationProgress));
        
        // Draw blast wave, fireball, and core with decreasing alpha
        DrawLayer(g, radius, alpha / 2, Color.FromArgb(255, 100, 0));
        DrawLayer(g, radius * 0.6f, alpha, Color.FromArgb(255, 200, 0));
        DrawLayer(g, radius * 0.3f, alpha, Color.White);
    }
}
```

## Educational Value

This application teaches:
1. **Cryptography**: AES encryption, PBKDF2 key derivation, HMAC authentication
2. **Security**: Secure password handling, authentication flows
3. **Graphics**: High-DPI rendering, animations, anti-aliasing
4. **UI/UX**: Proper input handling, cursor positioning, user feedback
5. **Event-driven Programming**: Timers, event handlers, state management

## Safety Notice

⚠️ **EDUCATIONAL PURPOSE ONLY** ⚠️

This application is designed solely for educational purposes to demonstrate:
- Software development techniques
- Cryptographic implementations
- User interface design
- Animation and graphics programming

It is NOT intended for, and should NOT be used for, any harmful activities.

## License
Educational demonstration software - 2025
