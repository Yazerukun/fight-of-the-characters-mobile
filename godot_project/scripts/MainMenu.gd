extends Control

## ============================================================================
## MAIN MENU CONTROLLER - FIGHT OF CHARACTERS MOBILE
## Handles Navigation to Arena, Practice Mode, Character Select & Settings.
## ============================================================================

@onready var game_manager = get_node_or_null("/root/GameManager")

func _ready() -> void:
	pass

func _on_btn_play_pressed() -> void:
	if game_manager:
		game_manager.current_mode = game_manager.GameMode.OFFLINE_ARENA
	get_tree().change_scene_to_file("res://scenes/CharacterSelect.tscn")

func _on_btn_practice_pressed() -> void:
	if game_manager:
		game_manager.current_mode = game_manager.GameMode.PRACTICE
	get_tree().change_scene_to_file("res://scenes/CharacterSelect.tscn")

func _on_btn_heroes_pressed() -> void:
	get_tree().change_scene_to_file("res://scenes/CharacterSelect.tscn")

func _on_btn_exit_pressed() -> void:
	get_tree().quit()
