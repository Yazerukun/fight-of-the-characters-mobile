extends Node

## ============================================================================
## GAME MANAGER - FIGHT OF CHARACTERS MOBILE
## Handles Global Game State, Hero Roster Database, Offline Persistent Save,
## Audio Channels, and Match Lifecycle.
## ============================================================================

enum GameMode { PRACTICE, PLAYER_VS_AI, OFFLINE_ARENA }
enum Difficulty { EASY, NORMAL, HARD, EXPERT }

const SAVE_PATH: String = "user://foc_save_v1.json"

# Active Session State
var current_mode: GameMode = GameMode.OFFLINE_ARENA
var current_difficulty: Difficulty = Difficulty.NORMAL
var selected_hero_id: String = "ichigo"
var player_kills: int = 0
var player_deaths: int = 0
var player_gold: int = 350
var match_duration: float = 0.0

# Master Settings
var sfx_volume: float = 0.8
var music_volume: float = 0.6
var graphics_quality: String = "medium" # low, medium, high

# Hero Roster Definition Cache
var hero_database: Dictionary = {
	"ichigo": {
		"name": "Kurosaki Ichigo",
		"origin": "Bleach",
		"title": "Substitute Shinigami",
		"mesh_color": Color(0.2, 0.6, 1.0),
		"base_hp": 1350,
		"base_mp": 450,
		"base_dmg": 74,
		"base_armor": 5,
		"speed": 7.8,
		"str": 26, "agi": 22, "int": 18,
		"skills": {
			"q": {"name": "Getsuga Slash", "desc": "AoE Crescent energy slash", "cd": 4.5, "mana": 65},
			"w": {"name": "Shunpo Blink", "desc": "Instant flash step teleport", "cd": 7.0, "mana": 80},
			"e": {"name": "Hollow Mask", "desc": "+40% Dmg & Speed buff", "cd": 14.0, "mana": 95},
			"r": {"name": "Bankai Burst", "desc": "Tensa Zangetsu burst damage", "cd": 45.0, "mana": 160},
			"d": {"name": "Final Mugetsu", "desc": "Ultimate Cataclysmic Slash", "cd": 70.0, "mana": 220}
		}
	},
	"gojo": {
		"name": "Gojo Satoru",
		"origin": "Jujutsu Kaisen",
		"title": "The Honored One",
		"mesh_color": Color(0.4, 0.4, 1.0),
		"base_hp": 1450,
		"base_mp": 550,
		"base_dmg": 82,
		"base_armor": 6,
		"speed": 8.0,
		"str": 28, "agi": 26, "int": 34,
		"skills": {
			"q": {"name": "Lapse: Blue", "desc": "Gravitational pull vortex", "cd": 5.0, "mana": 70},
			"w": {"name": "Reversal: Red", "desc": "Repulsive knockback blast", "cd": 7.5, "mana": 85},
			"e": {"name": "Infinity", "desc": "100% Invulnerable barrier (4.5s)", "cd": 13.0, "mana": 100},
			"r": {"name": "Hollow Purple", "desc": "Cataclysmic erasure laser", "cd": 40.0, "mana": 180},
			"d": {"name": "Unlimited Void", "desc": "Domain Expansion stun & AoE", "cd": 65.0, "mana": 250}
		}
	},
	"saber": {
		"name": "Saber (Artoria)",
		"origin": "Fate / Stay Night",
		"title": "King of Knights",
		"mesh_color": Color(0.1, 0.35, 0.85),
		"base_hp": 1500,
		"base_mp": 400,
		"base_dmg": 76,
		"base_armor": 7,
		"speed": 7.4,
		"str": 30, "agi": 20, "int": 16,
		"skills": {
			"q": {"name": "Strike Air", "desc": "Wind hammer shockwave", "cd": 4.0, "mana": 60},
			"w": {"name": "Mana Burst", "desc": "Dash with explosive propulsion", "cd": 6.5, "mana": 75},
			"e": {"name": "Avalon Shield", "desc": "Healing golden holy barrier", "cd": 12.0, "mana": 90},
			"r": {"name": "Excalibur", "desc": "Sword of Promised Victory beam", "cd": 42.0, "mana": 170},
			"d": {"name": "Caliburn Unleashed", "desc": "Legendary radial burst", "cd": 65.0, "mana": 210}
		}
	},
	"inuyasha": {
		"name": "Inuyasha",
		"origin": "Inuyasha",
		"title": "Half-Demon Swordsman",
		"mesh_color": Color(0.85, 0.15, 0.15),
		"base_hp": 1400,
		"base_mp": 380,
		"base_dmg": 78,
		"base_armor": 5,
		"speed": 7.6,
		"str": 28, "agi": 24, "int": 14,
		"skills": {
			"q": {"name": "Iron Reaver", "desc": "Soul stealer claw strike", "cd": 4.0, "mana": 55},
			"w": {"name": "Blades of Blood", "desc": "Ranged crystalline blood missiles", "cd": 6.0, "mana": 70},
			"e": {"name": "Demon Blood", "desc": "+50% Attack speed frenzied state", "cd": 14.0, "mana": 85},
			"r": {"name": "Wind Scar", "desc": "Tessaiga ground rift slash", "cd": 38.0, "mana": 150},
			"d": {"name": "Backlash Wave", "desc": "Tornado counter vortex", "cd": 65.0, "mana": 210}
		}
	}
}

func _ready() -> void:
	load_game_data()

func get_hero_config(hero_id: String) -> Dictionary:
	if hero_database.has(hero_id):
		return hero_database[hero_id]
	return hero_database["ichigo"]

# === OFFLINE PERSISTENT STORAGE ===
func save_game_data() -> void:
	var data: Dictionary = {
		"selected_hero_id": selected_hero_id,
		"sfx_volume": sfx_volume,
		"music_volume": music_volume,
		"graphics_quality": graphics_quality,
		"player_gold": player_gold,
		"player_kills": player_kills
	}
	var file = FileAccess.open(SAVE_PATH, FileAccess.WRITE)
	if file:
		file.store_string(JSON.stringify(data))
		file.close()

func load_game_data() -> void:
	if not FileAccess.file_exists(SAVE_PATH):
		return
	var file = FileAccess.open(SAVE_PATH, FileAccess.READ)
	if not file:
		return
	var content: String = file.get_as_text()
	file.close()
	var parsed = JSON.parse_string(content)
	if parsed is Dictionary:
		if parsed.has("selected_hero_id"): selected_hero_id = parsed["selected_hero_id"]
		if parsed.has("sfx_volume"): sfx_volume = parsed["sfx_volume"]
		if parsed.has("music_volume"): music_volume = parsed["music_volume"]
		if parsed.has("graphics_quality"): graphics_quality = parsed["graphics_quality"]
		if parsed.has("player_gold"): player_gold = parsed["player_gold"]
		if parsed.has("player_kills"): player_kills = parsed["player_kills"]
