# Educational Missile Simulator — Qt C++ / GCC / Linux

> **EDUCATIONAL PURPOSE ONLY** — a safe simulation for learning mapping,
> cryptography, and UI programming. Not intended for planning or enabling any
> harmful activity.

## Features

| Feature | Implementation |
|---------|---------------|
| **High-detail, crisp UI** | Qt5 `Fusion` style, `AA_EnableHighDpiScaling`, `QPainter::Antialiasing` |
| **Launch code auto-dash + correct cursor** | `QLineEdit::textEdited` + `setCursorPosition(formatted.size())` |
| **Missile explosion animation** | 60 FPS `QTimer` driving `QPainter` multi-layer ellipses |
| **AES-256-CBC + HMAC-SHA256 encrypt/decrypt** | OpenSSL `EVP_*` APIs |
| **PBKDF2-SHA256 password hashing** | OpenSSL `PKCS5_PBKDF2_HMAC` (100 000 iterations) |
| **Interactive control key widget** | Drag-to-rotate `QWidget` with `QPainter` |
| **Event log console** | Black terminal–style `QTextEdit` |
| **Authentication dialog** | PBKDF2-verified credentials |

## Building

### Prerequisites

```bash
sudo apt-get install qtbase5-dev libssl-dev cmake g++
```

### Compile

```bash
mkdir build && cd build
cmake .. -DCMAKE_BUILD_TYPE=Release
make -j$(nproc)
./MissileSimulator
```

## Demo credentials

| Operator ID       | Passphrase       |
|-------------------|------------------|
| `O5-X`            | `HandofDemocracy` |
| `Ethics Committee`| `HandofDemocracy` |

## Usage walkthrough

1. **Accept** the educational disclaimer  
2. **Login** with the demo credentials above  
3. Select a **missile type**, enter coordinates, click **Plot Event Point** →
   a coloured circle appears on the map and a random launch code is generated  
4. Click **Encrypt Activation Data** → the payload is AES-encrypted; note the
   key shown in the pop-up  
5. Click **Decrypt & Verify** → enter the key; on success the *Activate* button
   is enabled  
6. Enter the **launch code** in the `XXX-XX-XXX` field (cursor always stays
   after the last digit — no more "cursor behind number" bug)  
7. **Drag** the control key widget on the map until it reads `UNLOCKED`  
8. Click **🚀 ACTIVATE SIMULATION** → watch the 3-layer explosion animation

## Architecture

```
src/
├── main.cpp              Entry point; sets AA_EnableHighDpiScaling + Fusion style
├── CryptoHelper.h/.cpp   AES-256-CBC, HMAC-SHA256, PBKDF2 (OpenSSL)
├── DisclaimerDialog.h/.cpp
├── AuthDialog.h/.cpp     PBKDF2 credential verification
├── MapWidget.h/.cpp      Custom QWidget: grid, event points, explosions
├── ControlKeyWidget.h/.cpp Drag-to-rotate interactive key
├── ExplosionAnimation.h/.cpp Multi-layer expanding animation
└── MainWindow.h/.cpp     Main window; control panel; launch code fix
CMakeLists.txt
```
