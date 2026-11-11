import os
import winreg

print("=== CHECKING REGISTRY ===")
registry_paths = [
    (winreg.HKEY_CURRENT_USER, r"Software\Blizzard Entertainment\Warcraft III"),
    (winreg.HKEY_CURRENT_USER, r"Software\Battle.net\Configuration"),
    (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Blizzard Entertainment\Warcraft III"),
    (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\WOW6432Node\Blizzard Entertainment\Warcraft III"),
    (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
    (winreg.HKEY_LOCAL_MACHINE, r"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
]

for hive, path in registry_paths:
    try:
        k = winreg.OpenKey(hive, path)
        print(f"Found Key: {path}")
        try:
            i = 0
            while True:
                name, val, typ = winreg.EnumValue(k, i)
                if any(x in str(name).lower() or x in str(val).lower() for x in ['war3', 'warcraft', 'install', 'path']):
                    print(f"  {name} = {val}")
                i += 1
        except WindowsError:
            pass
        # Check subkeys if it's Uninstall
        if "Uninstall" in path:
            try:
                j = 0
                while True:
                    subkey_name = winreg.EnumKey(k, j)
                    if any(x in subkey_name.lower() for x in ['warcraft', 'war3', 'blizzard']):
                        sub_k = winreg.OpenKey(k, subkey_name)
                        try:
                            dn, _ = winreg.QueryValueEx(sub_k, "DisplayName")
                            il, _ = winreg.QueryValueEx(sub_k, "InstallLocation")
                            print(f"  Subkey {subkey_name}: {dn} at {il}")
                        except:
                            print(f"  Subkey {subkey_name}")
                    j += 1
            except WindowsError:
                pass
    except Exception as e:
        pass

print("\n=== CHECKING COMMON DIRECTORIES ===")
candidate_dirs = [
    r"C:\Program Files\Warcraft III",
    r"C:\Program Files (x86)\Warcraft III",
    r"C:\Program Files (x86)\Battle.net",
    r"C:\ProgramData\Battle.net",
    r"D:\Program Files\Warcraft III",
    r"D:\Program Files (x86)\Warcraft III",
    r"D:\Games\Warcraft III",
    r"D:\Warcraft III",
    r"D:\Warcraft",
    r"D:\installer\warcraft iii\warcraft iii",
    r"D:\installer",
]

for cd in candidate_dirs:
    exists = os.path.exists(cd)
    print(f"Directory {cd}: {'EXISTS' if exists else 'NOT FOUND'}")
    if exists:
        try:
            print(f"  Contents: {os.listdir(cd)[:10]}")
        except Exception as e:
            print(f"  Error reading: {e}")
