#!/usr/bin/env python3
"""
Quick build script for GameCube Controller Enabler
Simple, fast building without extra dependencies or virtual environments
"""

import subprocess
import sys
import os

def main():
    print("Quick Build - GameCube Controller Enabler")
    print("=" * 45)
    
    # Change to script directory
    os.chdir(os.path.dirname(os.path.abspath(__file__)))
    
    # Simple PyInstaller command with better error handling
    cmd = [
        sys.executable, "-m", "PyInstaller",
        "--onefile",
        "--windowed", 
        "--name", "GC-Controller-Enabler",
        "--hidden-import", "hid",
        "--hidden-import", "usb.core",
        "--hidden-import", "usb.util",
        "--collect-all", "vgamepad",  # Include all vgamepad files and DLLs
        "gc_controller_enabler.py"
    ]
    
    print("Building executable...")
    print(f"Command: {' '.join(cmd)}")
    
    try:
        result = subprocess.run(cmd, check=True)
        print("\n✅ Build completed successfully!")
        print("📁 Executable location: dist/GC-Controller-Enabler.exe")
        print("\n📝 Next steps:")
        print("1. Test the executable")
        print("2. Install ViGEmBus driver for Xbox 360 emulation")
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