import os

path = 'FOC-Reborn-Mobile/UI_UX/FOC_Reborn_3D_Interactive_Showcase.html'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

saber_lee_entries = '''  {
    id: "saber",
    name: "Saber (Artoria)",
    title: "King of Knights",
    franchise: "Fate",
    attr: "STR",
    color: "#ffd700",
    secondaryColor: "#1e3799",
    icon: "🗡️",
    modelType: "saber",
    skills: [
      { name: "Invisible Air", key: "Q", icon: "💨", desc: "Releases pressurized wind blast dealing 95/145/195/245/295 damage." },
      { name: "Mana Burst", key: "W", icon: "⚡", desc: "Infuses weapon with extreme magical energy, boosting attack damage by 45%." },
      { name: "Strike Air", key: "E", icon: "🌪️", desc: "Unleashes hammer of wind knocking foes back 500 units with 1.2s stun." },
      { name: "Excalibur", key: "R", icon: "✨", desc: "Unleashes the Sword of Promised Victory dealing 650/950/1250 holy damage in a colossal beam." }
    ]
  },
  {
    id: "rock_lee",
    name: "Rock Lee",
    title: "Green Beast of the Leaf",
    franchise: "Naruto",
    attr: "AGI",
    color: "#2ed573",
    secondaryColor: "#ff4757",
    icon: "🍃",
    modelType: "rock_lee",
    skills: [
      { name: "Leaf Whirlwind", key: "Q", icon: "🌪️", desc: "High and low spinning kicks dealing 100/150/200/250/300 damage." },
      { name: "Primary Lotus", key: "W", icon: "🥋", desc: "Launches target airborne and pile-drives them into the earth for 180/260/340/420 damage." },
      { name: "Eight Gates Release", key: "E", icon: "🔥", desc: "Opens gates of limiters, gaining +80% Attack Speed and +40% Move Speed." },
      { name: "Hidden Lotus", key: "R", icon: "💥", desc: "High-speed aerial onslaught striking the opponent 6 times at supersonic velocity for 600/850/1100 damage." }
    ]
  },
'''

if 'id: "saber"' not in content:
    target = 'const HERO_DATABASE = ['
    replacement = 'const HERO_DATABASE = [\n' + saber_lee_entries
    content = content.replace(target, replacement, 1)
    with open(path, 'w', encoding='utf-8') as f:
        f.write(content)
    print("Added Saber and Rock Lee to HERO_DATABASE successfully!")
else:
    print("Saber already in HERO_DATABASE.")
