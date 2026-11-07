/* ============================================================================
 * FOCS3 ANOTHER 9.7 HERO ROSTER REPOSITORY
 * Extracted directly from: FOCS3 Another 9.7 C03.w3x
 * ============================================================================ */

export const HERO_ROSTER = Object.freeze({
  "ichigo": {
    "id": "ichigo",
    "name": "Kurosaki Ichigo",
    "krName": "쿠로사키 이치고",
    "origin": "Bleach",
    "primaryStat": "str",
    "color": "#38bdf8",
    "buffColor": "#dc2626",
    "stats": {
      "str": 24,
      "agi": 20,
      "int": 16
    },
    "skills": {
      "q": {
        "name": "Getsuga Slash",
        "cost": 35,
        "cd": 3.5,
        "desc": "Fires a crescent slash knocking foes back."
      },
      "w": {
        "name": "Shunpo Blink",
        "cost": 45,
        "cd": 5.5,
        "desc": "High-speed teleport in movement direction."
      },
      "e": {
        "name": "Bankai: Tensa Zangetsu",
        "cost": 80,
        "cd": 14.0,
        "desc": "+50% Move speed and +80 bonus damage."
      },
      "r": {
        "name": "Final Getsuga Tensho",
        "cost": 120,
        "cd": 24.0,
        "desc": "Gigantic piercing beam dealing 3.5x damage."
      }
    }
  },
  "naruto": {
    "id": "naruto",
    "name": "Uzumaki Naruto",
    "krName": "우즈마키 나루토",
    "origin": "Naruto",
    "primaryStat": "str",
    "color": "#f97316",
    "buffColor": "#ea580c",
    "stats": {
      "str": 22,
      "agi": 18,
      "int": 20
    },
    "skills": {
      "q": {
        "name": "Rasengan",
        "cost": 40,
        "cd": 4.0,
        "desc": "Spiraling chakra sphere dealing heavy melee impact."
      },
      "w": {
        "name": "Shadow Clones",
        "cost": 50,
        "cd": 6.0,
        "desc": "Blinks away and confuses enemy targeting."
      },
      "e": {
        "name": "Kyuubi Cloak",
        "cost": 75,
        "cd": 15.0,
        "desc": "Red chakra shroud with damage reduction."
      },
      "r": {
        "name": "Rasenshuriken",
        "cost": 130,
        "cd": 26.0,
        "desc": "Detonating wind vortex tearing enemies."
      }
    }
  },
  "luffy": {
    "id": "luffy",
    "name": "Monkey D. Luffy",
    "krName": "몽키 D. 루피",
    "origin": "One Piece",
    "primaryStat": "str",
    "color": "#ef4444",
    "buffColor": "#b91c1c",
    "stats": {
      "str": 26,
      "agi": 22,
      "int": 12
    },
    "skills": {
      "q": {
        "name": "Gomu Gomu Pistol",
        "cost": 30,
        "cd": 3.0,
        "desc": "Long-range stretching punch."
      },
      "w": {
        "name": "Gatling Gun",
        "cost": 45,
        "cd": 5.0,
        "desc": "Rapid flurry of rubber punches in a cone."
      },
      "e": {
        "name": "Gear Second",
        "cost": 70,
        "cd": 12.0,
        "desc": "Boiling blood granting supreme speed."
      },
      "r": {
        "name": "Gear Third: Elephant Gun",
        "cost": 115,
        "cd": 22.0,
        "desc": "Massive giant fist crushing target area."
      }
    }
  },
  "zoro": {
    "id": "zoro",
    "name": "Roronoa Zoro",
    "krName": "로로노아 조로",
    "origin": "One Piece",
    "primaryStat": "agi",
    "color": "#10b981",
    "buffColor": "#047857",
    "stats": {
      "str": 24,
      "agi": 25,
      "int": 12
    },
    "skills": {
      "q": {
        "name": "Oni Giri",
        "cost": 35,
        "cd": 3.5,
        "desc": "Three-blade crossing charge slash."
      },
      "w": {
        "name": "Tatsu Maki",
        "cost": 50,
        "cd": 6.0,
        "desc": "Dragon twister launching enemies."
      },
      "e": {
        "name": "Asura Nine-Blades",
        "cost": 75,
        "cd": 14.0,
        "desc": "Multi-armed spirit avatar increasing damage."
      },
      "r": {
        "name": "Sanzen Sekai",
        "cost": 125,
        "cd": 25.0,
        "desc": "Three Thousand Worlds fatal rotational cut."
      }
    }
  },
  "sasuke": {
    "id": "sasuke",
    "name": "Uchiha Sasuke",
    "krName": "우치하 사스케",
    "origin": "Naruto",
    "primaryStat": "agi",
    "color": "#8b5cf6",
    "buffColor": "#6d28d9",
    "stats": {
      "str": 20,
      "agi": 26,
      "int": 18
    },
    "skills": {
      "q": {
        "name": "Chidori",
        "cost": 40,
        "cd": 4.0,
        "desc": "Lightning thrust piercing defenses."
      },
      "w": {
        "name": "Fireball Technique",
        "cost": 45,
        "cd": 5.0,
        "desc": "Giant flame blast combusting foes."
      },
      "e": {
        "name": "Sharingan",
        "cost": 70,
        "cd": 13.0,
        "desc": "Heightened evasion and critical strike rate."
      },
      "r": {
        "name": "Kirin Lightning",
        "cost": 130,
        "cd": 26.0,
        "desc": "Summons heavenly thunder dragon from skies."
      }
    }
  },
  "goku": {
    "id": "goku",
    "name": "Son Goku",
    "krName": "손오공",
    "origin": "Dragon Ball",
    "primaryStat": "str",
    "color": "#f59e0b",
    "buffColor": "#d97706",
    "stats": {
      "str": 28,
      "agi": 20,
      "int": 14
    },
    "skills": {
      "q": {
        "name": "Kamehameha",
        "cost": 45,
        "cd": 4.0,
        "desc": "Signature energy blast line."
      },
      "w": {
        "name": "Instant Transmission",
        "cost": 40,
        "cd": 4.5,
        "desc": "Blinks directly behind target."
      },
      "e": {
        "name": "Super Saiyan",
        "cost": 85,
        "cd": 15.0,
        "desc": "Golden aura unleashing explosive power."
      },
      "r": {
        "name": "Spirit Bomb",
        "cost": 140,
        "cd": 28.0,
        "desc": "Giant planetary energy sphere devastation."
      }
    }
  },
  "ace": {
    "id": "ace",
    "name": "Portgas D. Ace",
    "krName": "포트거스 D. 에이스",
    "origin": "One Piece",
    "primaryStat": "int",
    "color": "#ea580c",
    "buffColor": "#c2410c",
    "stats": {
      "str": 21,
      "agi": 20,
      "int": 24
    },
    "skills": {
      "q": {
        "name": "Hiken (Fire Fist)",
        "cost": 40,
        "cd": 3.5,
        "desc": "Massive flame pillar burning forward."
      },
      "w": {
        "name": "Higan (Fire Gun)",
        "cost": 35,
        "cd": 4.0,
        "desc": "Rapid fire finger bullets."
      },
      "e": {
        "name": "Flame Body Logia",
        "cost": 65,
        "cd": 12.0,
        "desc": "Intangible flame form reducing incoming hits."
      },
      "r": {
        "name": "Dai Enkai: Entei",
        "cost": 135,
        "cd": 26.0,
        "desc": "Giant sun fireball incinerating the arena."
      }
    }
  },
  "saber": {
    "id": "saber",
    "name": "Artoria (Saber)",
    "krName": "세이버",
    "origin": "Fate",
    "primaryStat": "str",
    "color": "#3b82f6",
    "buffColor": "#1d4ed8",
    "stats": {
      "str": 25,
      "agi": 22,
      "int": 18
    },
    "skills": {
      "q": {
        "name": "Strike Air",
        "cost": 35,
        "cd": 3.5,
        "desc": "Wind blast knocking enemies away."
      },
      "w": {
        "name": "Invisible Air Step",
        "cost": 40,
        "cd": 5.0,
        "desc": "Wind-assisted dash with invulnerability."
      },
      "e": {
        "name": "Mana Burst",
        "cost": 70,
        "cd": 12.0,
        "desc": "Infuses golden holy aura into blade."
      },
      "r": {
        "name": "Excalibur",
        "cost": 130,
        "cd": 25.0,
        "desc": "Holy sword beam disintegrating everything."
      }
    }
  },
  "byakuya": {
    "id": "byakuya",
    "name": "Byakuya Kuchiki",
    "krName": "쿠치키 바쿠야",
    "origin": "Bleach",
    "primaryStat": "int",
    "color": "#ec4899",
    "buffColor": "#be185d",
    "stats": {
      "str": 20,
      "agi": 22,
      "int": 24
    },
    "skills": {
      "q": {
        "name": "Hado #33 Sokatsui",
        "cost": 35,
        "cd": 3.5,
        "desc": "Pale blue burst of spiritual lightning."
      },
      "w": {
        "name": "Utsusemi Step",
        "cost": 45,
        "cd": 5.5,
        "desc": "Leaves shadow coat and teleports."
      },
      "e": {
        "name": "Shikai: Senbonzakura",
        "cost": 65,
        "cd": 12.0,
        "desc": "Scatters thousand cherry petal razor blades."
      },
      "r": {
        "name": "Bankai: Kageyoshi",
        "cost": 120,
        "cd": 24.0,
        "desc": "Wall of countless blades shredding enemies."
      }
    }
  },
  "gilgamesh": {
    "id": "gilgamesh",
    "name": "Gilgamesh",
    "krName": "길가메쉬",
    "origin": "Fate",
    "primaryStat": "int",
    "color": "#eab308",
    "buffColor": "#ca8a04",
    "stats": {
      "str": 22,
      "agi": 18,
      "int": 26
    },
    "skills": {
      "q": {
        "name": "Gate of Babylon",
        "cost": 40,
        "cd": 3.0,
        "desc": "Summons golden ripples firing Noble Phantasms."
      },
      "w": {
        "name": "Enkidu Chains",
        "cost": 50,
        "cd": 6.0,
        "desc": "Chains of heaven immobilizing targets."
      },
      "e": {
        "name": "Golden Armor",
        "cost": 65,
        "cd": 14.0,
        "desc": "Magic resistance and armor buff."
      },
      "r": {
        "name": "Enuma Elish (Ea)",
        "cost": 140,
        "cd": 28.0,
        "desc": "Star of creation tearing spatial fabric."
      }
    }
  },
  "kenpachi": {
    "id": "kenpachi",
    "name": "Zaraki Kenpachi",
    "krName": "자라키 켄파치",
    "origin": "Bleach",
    "primaryStat": "str",
    "color": "#64748b",
    "buffColor": "#334155",
    "stats": {
      "str": 30,
      "agi": 16,
      "int": 10
    },
    "skills": {
      "q": {
        "name": "Brutal Cleave",
        "cost": 25,
        "cd": 2.5,
        "desc": "Fierce sword cut with heavy physical damage."
      },
      "w": {
        "name": "Pressure Dash",
        "cost": 40,
        "cd": 5.0,
        "desc": "Charges forward trampling minion paths."
      },
      "e": {
        "name": "Eyepatch Removal",
        "cost": 60,
        "cd": 15.0,
        "desc": "Unseals monstrous yellow spiritual pressure."
      },
      "r": {
        "name": "Ryodan (Two-Hand Cut)",
        "cost": 100,
        "cd": 20.0,
        "desc": "Two-handed vertical smash splitting earth."
      }
    }
  },
  "ulquiorra": {
    "id": "ulquiorra",
    "name": "Ulquiorra Cifer",
    "krName": "우르키오라",
    "origin": "Bleach",
    "primaryStat": "agi",
    "color": "#22c55e",
    "buffColor": "#15803d",
    "stats": {
      "str": 22,
      "agi": 26,
      "int": 20
    },
    "skills": {
      "q": {
        "name": "Green Cero",
        "cost": 35,
        "cd": 3.5,
        "desc": "High-density spiritual energy flash."
      },
      "w": {
        "name": "Sonido Speed",
        "cost": 40,
        "cd": 4.5,
        "desc": "Instant sound-step relocation."
      },
      "e": {
        "name": "Resurreccion: Murcielago",
        "cost": 80,
        "cd": 14.0,
        "desc": "Black winged form with high regeneration."
      },
      "r": {
        "name": "Lanza del Relampago",
        "cost": 130,
        "cd": 25.0,
        "desc": "Lightning javelin catastrophic detonation."
      }
    }
  },
  "lucci": {
    "id": "lucci",
    "name": "Rob Lucci",
    "krName": "로브 루치",
    "origin": "One Piece",
    "primaryStat": "agi",
    "color": "#a855f7",
    "buffColor": "#7e22ce",
    "stats": {
      "str": 23,
      "agi": 27,
      "int": 14
    },
    "skills": {
      "q": {
        "name": "Shigan (Finger Pistol)",
        "cost": 30,
        "cd": 3.0,
        "desc": "Rapid piercing thrust piercing through armor."
      },
      "w": {
        "name": "Rankyaku (Tempest Kick)",
        "cost": 40,
        "cd": 4.5,
        "desc": "Compressed air blade projectile."
      },
      "e": {
        "name": "Tekkai (Iron Mass)",
        "cost": 50,
        "cd": 10.0,
        "desc": "Hardens body to iron countering blows."
      },
      "r": {
        "name": "Rokuogan (Six King Gun)",
        "cost": 120,
        "cd": 22.0,
        "desc": "Dual-fist shockwave causing internal trauma."
      }
    }
  },
  "gaara": {
    "id": "gaara",
    "name": "Gaara of the Sand",
    "krName": "가아라",
    "origin": "Naruto",
    "primaryStat": "int",
    "color": "#d97706",
    "buffColor": "#b45309",
    "stats": {
      "str": 21,
      "agi": 16,
      "int": 28
    },
    "skills": {
      "q": {
        "name": "Sand Shuriken",
        "cost": 35,
        "cd": 3.0,
        "desc": "Barrage of hardened sand projectiles."
      },
      "w": {
        "name": "Sand Shield",
        "cost": 55,
        "cd": 8.0,
        "desc": "Protective barrier absorbing incoming hits."
      },
      "e": {
        "name": "Sand Coffin",
        "cost": 75,
        "cd": 13.0,
        "desc": "Immobilizes target in crushing sand wrap."
      },
      "r": {
        "name": "Sand Tsunami Burial",
        "cost": 135,
        "cd": 26.0,
        "desc": "Massive sand tidal wave swallowing the arena."
      }
    }
  },
  "rocklee": {
    "id": "rocklee",
    "name": "Rock Lee",
    "krName": "록리",
    "origin": "Naruto",
    "primaryStat": "agi",
    "color": "#16a34a",
    "buffColor": "#15803d",
    "stats": {
      "str": 24,
      "agi": 28,
      "int": 10
    },
    "skills": {
      "q": {
        "name": "Leaf Whirlwind",
        "cost": 25,
        "cd": 2.5,
        "desc": "Spinning roundhouse kick."
      },
      "w": {
        "name": "Weight Release Speed",
        "cost": 40,
        "cd": 6.0,
        "desc": "Drops leg weights gaining blinding velocity."
      },
      "e": {
        "name": "Fifth Gate Open",
        "cost": 70,
        "cd": 12.0,
        "desc": "Opens Gate of Limit with berserk attacks."
      },
      "r": {
        "name": "Hidden Lotus (Ura Renge)",
        "cost": 110,
        "cd": 20.0,
        "desc": "High-altitude multi-hit drop strike."
      }
    }
  },
  "alucard": {
    "id": "alucard",
    "name": "Alucard",
    "krName": "아카드",
    "origin": "Hellsing",
    "primaryStat": "str",
    "color": "#991b1b",
    "buffColor": "#7f1d1d",
    "stats": {
      "str": 28,
      "agi": 21,
      "int": 20
    },
    "skills": {
      "q": {
        "name": "Jackal Heavy Rounds",
        "cost": 35,
        "cd": 3.0,
        "desc": "High-caliber armor-piercing bullets."
      },
      "w": {
        "name": "Mist Form",
        "cost": 45,
        "cd": 5.0,
        "desc": "Turns to red mist bypassing physical attacks."
      },
      "e": {
        "name": "Cromwell Level 1",
        "cost": 70,
        "cd": 13.0,
        "desc": "Shadow hound familiar chewing enemy armor."
      },
      "r": {
        "name": "Release Level 0",
        "cost": 140,
        "cd": 28.0,
        "desc": "River of death summoning immortal familiars."
      }
    }
  }
});

export const HERO_LIST = Object.values(HERO_ROSTER);
