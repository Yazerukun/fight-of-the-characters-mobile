import os
import sys
import winreg
import shutil

print("=== 1. SYSTEM ENVIRONMENT & HARDWARE ===")
print(f"Platform: {sys.platform}")
print(f"Python: {sys.version}")

# Search PATH for key executables
execs = ['Unity.exe', 'UnityHub.exe', 'javac.exe', 'java.exe', 'adb.exe', 'ndk-build.cmd']
for exe in execs:
    found = shutil.which(exe)
    print(f"PATH lookup for {exe}: {found}")

# Environment variables
env_vars = ['UNITY_HOME', 'UNITY_PATH', 'ANDROID_HOME', 'ANDROID_SDK_ROOT', 'ANDROID_NDK_ROOT', 'ANDROID_NDK_HOME', 'JAVA_HOME', 'JDK_HOME']
for ev in env_vars:
    print(f"Env {ev}: {os.environ.get(ev)}")

print("\n=== 2. WINDOWS REGISTRY CHECK FOR UNITY ===")
reg_roots = [
    (winreg.HKEY_LOCAL_MACHINE, r"Software\Unity Technologies"),
    (winreg.HKEY_CURRENT_USER, r"Software\Unity Technologies"),
    (winreg.HKEY_LOCAL_MACHINE, r"Software\Microsoft\Windows\CurrentVersion\Uninstall"),
    (winreg.HKEY_LOCAL_MACHINE, r"Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
    (winreg.HKEY_CURRENT_USER, r"Software\Microsoft\Windows\CurrentVersion\Uninstall")
]

for hkey, subkey in reg_roots:
    try:
        with winreg.OpenKey(hkey, subkey) as key:
            print(f"Found Registry key: {subkey}")
            # Enumerate subkeys
            num_subkeys, num_vals, _ = winreg.QueryInfoKey(key)
            for i in range(num_subkeys):
                try:
                    sname = winreg.EnumKey(key, i)
                    if 'unity' in sname.lower() or 'editor' in sname.lower():
                        print(f"  -> Subkey: {sname}")
                        with winreg.OpenKey(key, sname) as skey:
                            for j in range(winreg.QueryInfoKey(skey)[1]):
                                vname, vval, _ = winreg.EnumValue(skey, j)
                                if vname in ('DisplayName', 'DisplayVersion', 'InstallLocation', 'Location'):
                                    print(f"     {vname}: {vval}")
                except:
                    pass
    except FileNotFoundError:
        pass
    except Exception as e:
        print(f"Error checking {subkey}: {e}")

print("\n=== 3. DISK SEARCH IN COMMON INSTALL LOCATIONS ===")
candidates = [
    r"C:\Program Files\Unity",
    r"C:\Program Files\Unity Hub",
    r"C:\Program Files\Unity\Hub\Editor",
    r"C:\Program Files (x86)\Unity",
    r"C:\Program Files (x86)\Unity Hub",
    r"C:\Unity",
    r"D:\Unity",
    r"C:\Users\Technical\AppData\Local\Programs\Unity",
    r"C:\Users\Technical\AppData\Local\Programs\Unity Hub",
    r"C:\Program Files\Android",
    r"C:\Users\Technical\AppData\Local\Android\Sdk",
    r"C:\Program Files\Java",
    r"C:\Program Files\Eclipse Adoptium",
    r"C:\Program Files\Microsoft\jdk"
]

for c in candidates:
    exists = os.path.exists(c)
    print(f"Candidate path: {c} -> Exists: {exists}")
    if exists:
        try:
            items = os.listdir(c)
            print(f"   Contents: {items[:10]}")
        except Exception as e:
            print(f"   Error listing: {e}")
