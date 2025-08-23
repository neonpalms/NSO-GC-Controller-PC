#!/usr/bin/env python3
"""
Build script that creates executable even when vgamepad is not installed
The executable will still work with vgamepad if it's available at runtime
"""

import subprocess
import sys
import os

def main():
    print("Building GC Controller Enabler (vgamepad-optional)")
    print("=" * 55)
    
    # Change to script directory
    os.chdir(os.path.dirname(os.path.abspath(__file__)))
    
    # Build command without vgamepad dependencies
    cmd = [
        sys.executable, "-m", "PyInstaller",
        "--onefile",
        "--windowed",
        "--name", "GC-Controller-Enabler",
        "--hidden-import", "hid", 
        "--hidden-import", "usb.core",
        "--hidden-import", "usb.util",
        "--hidden-import", "tkinter",
        "--hidden-import", "tkinter.ttk",
        # Don't include vgamepad - it will be optional at runtime
        "gc_controller_enabler.py"
    ]
    
    print("Building executable...")
    print(f"Command: {' '.join(cmd)}")
    print("\nNote: Xbox 360 emulation requires vgamepad to be installed separately")
    
    try:
        result = subprocess.run(cmd, check=True)
        print("\nBuild completed successfully!")
        print("Executable location: dist/GC-Controller-Enabler.exe")
        print("\nNext steps:")
        print("1. Test the executable")
        print("2. For Xbox 360 emulation, install:")
        print("   - ViGEmBus driver: https://github.com/nefarius/ViGEmBus")
        print("   - vgamepad: pip install vgamepad")
        print("3. Connect your GameCube controller and test!")
        
    except subprocess.CalledProcessError as e:
        print(f"\n❌ Build failed with error code: {e.returncode}")
        return 1
    except FileNotFoundError:
        print("\n❌ PyInstaller not found. Install with: pip install pyinstaller")
        return 1
    
    return 0

if __name__ == "__main__":
    sys.exit(main())