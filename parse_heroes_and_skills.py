import re
import os
import json

def parse_foc():
    with open("war3map.j", "r", encoding="utf-8", errors="ignore") as f:
        content = f.read()

    print(f"Read war3map.j ({len(content)} bytes)")

    # Find hero unit types: in WC3 typically 'H000', 'O000', 'E000', 'U000', etc.
    # Often created or referenced in hero selection triggers.
    # Let's search for unit create patterns or hero pick triggers
    unit_ids = set(re.findall(r"'([A-Za-z0-9]{4})'", content))
    print(f"Total 4-char raw IDs found in script: {len(unit_ids)}")

    # Search for hero strings in war3map.wts
    wts_dict = {}
    if os.path.exists("war3map.wts"):
        with open("war3map.wts", "r", encoding="utf-8", errors="ignore") as wf:
            wts_raw = wf.read()
        entries = re.findall(r"STRING\s+(\d+)\s*\{([^}]+)\}", wts_raw)
        for num, text in entries:
            wts_dict[f"TRIGSTR_{num.zfill(3)}"] = text.strip()
            wts_dict[num] = text.strip()
        print(f"Parsed {len(entries)} strings from war3map.wts")

    # Let's search for character names in JASS: e.g. Ichigo, Naruto, Luffy, Sasuke, etc.
    anime_names = [
        "Ichigo", "Luffy", "Naruto", "Sasuke", "Zoro", "Sanji", "Ace", "Whitebeard",
        "Byakuya", "Hitsugaya", "Kenpachi", "Ishida", "Inuyasha", "Goku", "Archer",
        "Saber", "Lucci", "Gaara", "Rock Lee", "Neji", "Hiruzen", "Madara", "Obito",
        "Iori", "Kyo", "Roger", "Metal Bat", "Alucard", "Enel", "Orochimaru", "Itachi",
        "Kakashi", "Riku", "Renji", "Ulquiorra", "Grimmjow", "Aizen", "Gilgamesh"
    ]

    found_heroes = []
    for name in anime_names:
        matches = len(re.findall(rf"\b{name}\b", content, re.IGNORECASE))
        if matches > 0:
            found_heroes.append((name, matches))

    print("Found anime character references in war3map.j:")
    for h, m in sorted(found_heroes, key=lambda x: -x[1]):
        print(f"  {h}: {m} occurrences")

    # Let's inspect hero selection trigger
    print("\n--- Searching for hero selection / tavern / pick functions ---")
    pick_funcs = re.findall(r"function\s+([A-Za-z0-9_]*Pick[A-Za-z0-9_]*|[A-Za-z0-9_]*Hero[A-Za-z0-9_]*|[A-Za-z0-9_]*Select[A-Za-z0-9_]*)\s+takes", content, re.IGNORECASE)
    print("Candidate hero pick functions:", pick_funcs[:25])

if __name__ == "__main__":
    parse_foc()
