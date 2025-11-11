# Missile Simulator - Educational Application

## Overview

This is an **educational Windows Forms application** demonstrating:
- Geographic mapping with GMap.NET and OpenStreetMap
- Geodesic mathematics (calculating circles on Earth's surface)
- AES-256 encryption with PBKDF2 key derivation
- Secure authentication with password hashing
- Animated UI effects and interactive controls

**IMPORTANT: This is a safe simulation for learning purposes only. It does not connect to real systems and is not intended for planning or enabling any harmful activity.**

## Features

### 1. Authentication
- Login with Operator ID and Passphrase
- Password hashing using PBKDF2 with salt
- Demo credentials:
  - Operator ID: `O5-X`, Passphrase: `HandofDemocracy`
  - Operator ID: `Ethics Committee`, Passphrase: `SafeSimulation`

### 2. Map Integration
- OpenStreetMap tiles via GMap.NET
- Plot events with latitude, longitude, and radius
- Geodesic circle rendering (accounts for Earth's curvature)
- Load multiple events from CSV files

### 3. Event Profiles
Four selectable profiles with different visual parameters:
- **Conventional**: 0.5km / 2km radius zones
- **Nuclear**: 2km / 8km radius zones
- **Thermonuclear**: 5km / 20km radius zones
- **Thaumonuclear**: 10km / 50km radius zones (fictional)

Each profile demonstrates drawing concentric circles with different colors and opacity.

### 4. Encryption Demo (AES-256)
- Encrypt event payloads with user-provided encryption code
- Key derivation using PBKDF2
- Display encrypted data as Base64
- Decrypt with separate decryption code
- Demonstrates authenticated encryption with HMAC

### 5. Authentication Code System
- Generate Missile ID from event type and coordinates
- Require authentication code (format: XXX-XX-XXX)
- Codes stored in `AuthCodes.txt`
- Simulate two-factor authentication

### 6. Control Key Widget
- Interactive circular control that can be rotated
- Requires authentication and valid auth code
- Rotate 90 degrees to activate simulation
- Visual feedback (locked/armed states)

### 7. Animated Effects
- Expanding ripple animation on activation
- Pulsing circles with fade-out effect
- Smooth transitions using timer-based animations

### 8. Event Timeline
- Console log showing all user actions
- Timestamps for login, encryption, activation
- Real-time event logging

## Requirements

- .NET 7.0 SDK or later
- Windows operating system
- GMap.NET.Core and GMap.NET.WinForms NuGet packages

## Building and Running

### Option 1: Using dotnet CLI

```bash
# Restore dependencies
dotnet restore

# Build the project
dotnet build

# Run the application
dotnet run
```

### Option 2: Using Visual Studio

1. Open the folder in Visual Studio 2022
2. Wait for NuGet packages to restore
3. Press F5 to build and run

### Option 3: Direct compilation

```bash
# Single command build and run
dotnet run --project MissileSimulator.csproj
```

## Usage Guide

1. **Start Application**
   - Read and accept the educational disclaimer
   - Log in with demo credentials

2. **Plot an Event**
   - Enter latitude, longitude, and radius
   - Select an event profile (Conventional, Nuclear, etc.)
   - Click "Plot Event" to visualize on map

3. **Load Multiple Events**
   - Prepare CSV file with format: `name,lat,lng,radius_km,event_type`
   - Click "Load CSV" and select your file
   - See example in `sample_events.csv`

4. **Encrypt an Event**
   - Enter an encryption code (any text)
   - Click "Encrypt Event"
   - Note the Missile ID and required Auth Code

5. **Activate Simulation**
   - Enter the correct Auth Code and click "Verify"
   - Control Key will turn green (armed)
   - Click and drag the key to rotate it 90 degrees
   - Watch the animated activation effect

6. **Decrypt Data**
   - Enter the same encryption code as decryption code
   - Click "Decrypt" to view the payload

## Educational Value

### Cryptography Concepts
- **PBKDF2**: Key derivation from passwords (prevents rainbow table attacks)
- **Salt**: Random data to ensure unique hashes
- **AES-256**: Industry-standard symmetric encryption
- **HMAC**: Message authentication to detect tampering
- **Constant-time comparison**: Prevents timing attacks

### Geographic Concepts
- **Geodesic circles**: Account for Earth's curvature
- **Haversine formula**: Calculate great-circle distances
- **Coordinate systems**: Latitude/longitude representation

### Security Concepts
- **Password hashing**: Never store plaintext passwords
- **Session tokens**: Manage authenticated sessions
- **Two-factor authentication**: Multiple verification steps
- **Authenticated encryption**: Combine encryption with integrity

### UI/UX Concepts
- **Form validation**: Check user input before processing
- **Visual feedback**: Colors, animations for state changes
- **Event logging**: Audit trail of user actions
- **Interactive controls**: Custom widgets with mouse events

## File Structure

- `MissileSimulator.csproj` - Project configuration
- `Program.cs` - Application entry point and disclaimer
- `AuthForm.cs` - Login dialog with PBKDF2 authentication
- `CryptoHelper.cs` - Encryption and hashing utilities
- `MainForm.cs` - Main application UI and logic
- `AuthCodes.txt` - Generated authentication codes
- `sample_events.csv` - Example event data
- `README.md` - This file

## Safety Notes

This application:
- ✅ Is purely educational
- ✅ Uses neutral event types (meteor, volcanic eruption, drill, fireworks)
- ✅ Does not connect to any real systems
- ✅ Cannot cause any real-world harm
- ✅ Demonstrates proper cryptographic practices
- ❌ Is NOT intended for actual targeting or planning
- ❌ Does NOT model real weapon systems accurately
- ❌ Should NOT be used for any harmful purposes

## License

This is educational software provided as-is for learning purposes.

## Support

For educational questions about cryptography, mapping, or UI design concepts demonstrated in this application, please consult standard computer science resources.
