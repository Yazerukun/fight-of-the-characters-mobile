extends Control

## ============================================================================
## CHARACTER SELECTION CONTROLLER - FIGHT OF CHARACTERS MOBILE
## 3D Hero Preview Pedestal, Attribute Radar, Skills Inspector & Confirmation.
## ============================================================================

@onready var hero_name_label: Label = $UI/Panel/HeroName
@onready var hero_title_label: Label = $UI/Panel/HeroTitle
@onready var stats_label: Label = $UI/Panel/StatsLabel
@onready var skills_label: Label = $UI/Panel/SkillsLabel
@onready var preview_mesh: CSGCylinder3D = $World3D/HeroPedestal/PreviewHeroMesh
@onready var game_manager = get_node_or_null("/root/GameManager")

var current_hero: String = "gojo"

func _ready() -> void:
	select_hero("gojo")

func _process(delta: float) -> void:
	if preview_mesh:
		preview_mesh.rotation.y += delta * 0.8 # Smooth idle 3D rotation

func select_hero(hero_id: String) -> void:
	current_hero = hero_id
	if game_manager:
		game_manager.selected_hero_id = hero_id

	var cfg = _get_hero_data(hero_id)
	if hero_name_label: hero_name_label.text = cfg["name"]
	if hero_title_label: hero_title_label.text = cfg["title"] + " (" + cfg["origin"] + ")"

	if stats_label:
		stats_label.text = "HP: %d  |  MP: %d\nDMG: %d  |  ARMOR: %d  |  SPD: %.1f\nSTR: %d  |  AGI: %d  |  INT: %d" % [
			cfg["base_hp"], cfg["base_mp"], cfg["base_dmg"], cfg["base_armor"], cfg["speed"],
			cfg["str"], cfg["agi"], cfg["int"]
		]

	if skills_label:
		var sk = cfg["skills"]
		skills_label.text = "[Q] %s: %s (CD: %.1fs)\n[W] %s: %s (CD: %.1fs)\n[E] %s: %s (CD: %.1fs)\n[R] %s: %s (CD: %.1fs)\n[D] %s: %s (CD: %.1fs)" % [
			sk["q"]["name"], sk["q"]["desc"], sk["q"]["cd"],
			sk["w"]["name"], sk["w"]["desc"], sk["w"]["cd"],
			sk["e"]["name"], sk["e"]["desc"], sk["e"]["cd"],
			sk["r"]["name"], sk["r"]["desc"], sk["r"]["cd"],
			sk["d"]["name"], sk["d"]["desc"], sk["d"]["cd"]
		]

	if preview_mesh and cfg.has("mesh_color"):
		var mat = StandardMaterial3D.new()
		mat.albedo_color = cfg["mesh_color"]
		preview_mesh.material = mat

func _on_btn_ichigo_pressed() -> void: select_hero("ichigo")
func _on_btn_gojo_pressed() -> void: select_hero("gojo")
func _on_btn_saber_pressed() -> void: select_hero("saber")
func _on_btn_inuyasha_pressed() -> void: select_hero("inuyasha")

func _on_btn_confirm_pressed() -> void:
	if game_manager:
		game_manager.selected_hero_id = current_hero
		game_manager.save_game_data()
	get_tree().change_scene_to_file("res://scenes/MainArena.tscn")

func _on_btn_back_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/MainMenu.tscn")

func _get_hero_data(hero_id: String) -> Dictionary:
	if game_manager and game_manager.has_method("get_hero_config"):
		return game_manager.get_hero_config(hero_id)
	return {
		"name": "Gojo Satoru", "origin": "Jujutsu Kaisen", "title": "The Honored One",
		"base_hp": 1450, "base_mp": 550, "base_dmg": 82, "base_armor": 6, "speed": 8.0,
		"str": 28, "agi": 26, "int": 34, "mesh_color": Color(0.4, 0.4, 1.0),
		"skills": {
			"q": {"name": "Lapse: Blue", "desc": "Suction pull", "cd": 5.0},
			"w": {"name": "Reversal: Red", "desc": "Repulsive blast", "cd": 7.5},
			"e": {"name": "Infinity", "desc": "Invulnerable barrier", "cd": 13.0},
			"r": {"name": "Hollow Purple", "desc": "Cataclysmic laser", "cd": 40.0},
			"d": {"name": "Unlimited Void", "desc": "Domain Expansion", "cd": 65.0}
		}
	}
