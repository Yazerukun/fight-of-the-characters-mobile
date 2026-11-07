extends Node3D
class_name DuelArenaDirector

## ============================================================================
## DUEL ARENA DIRECTOR - FOTC SIGNATURE 1v1 EVENT
## Triggers countdown, seals selected heroes inside the circular energy barrier,
## monitors duel victor, and awards duel victory bounty.
## ============================================================================

signal duel_started(hero_1: CharacterBody3D, hero_2: CharacterBody3D)
signal duel_ended(victor: CharacterBody3D, loser: CharacterBody3D)

@export var duel_interval_seconds: float = 180.0 # 3 minutes
@export var duel_duration_max: float = 60.0 # 1 minute max fight
@export var arena_center: Vector3 = Vector3(0, 0, 0)
@export var duel_radius: float = 14.0

var time_until_duel: float = 180.0
var is_duel_active: bool = false
var duel_timer: float = 0.0

var duelist_1: CharacterBody3D = null
var duelist_2: CharacterBody3D = null
var original_pos_1: Vector3 = Vector3.ZERO
var original_pos_2: Vector3 = Vector3.ZERO

@onready var barrier_mesh: CSGCylinder3D = $DuelBarrier if has_node("DuelBarrier") else null

func _ready() -> void:
	time_until_duel = duel_interval_seconds
	if barrier_mesh:
		barrier_mesh.visible = false

func _process(delta: float) -> void:
	if not is_duel_active:
		time_until_duel -= delta
		if time_until_duel <= 0.0:
			_start_duel()
	else:
		duel_timer -= delta
		_enforce_duel_boundary()
		if duel_timer <= 0.0:
			_end_duel_by_timeout()

func _start_duel() -> void:
	var team1_heroes: Array[Node] = get_tree().get_nodes_in_group("team_1")
	var team2_heroes: Array[Node] = get_tree().get_nodes_in_group("team_2")
	
	var h1: CharacterBody3D = null
	var h2: CharacterBody3D = null
	
	for n in team1_heroes:
		if n is CharacterBody3D and n.current_health > 0:
			h1 = n
			break
	for n in team2_heroes:
		if n is CharacterBody3D and n.current_health > 0:
			h2 = n
			break
			
	if h1 == null or h2 == null:
		time_until_duel = 30.0 # Retry in 30 seconds if heroes are dead
		return

	duelist_1 = h1
	duelist_2 = h2
	original_pos_1 = duelist_1.global_position
	original_pos_2 = duelist_2.global_position

	duelist_1.is_in_duel = true
	duelist_2.is_in_duel = true

	# Teleport into duel circle
	duelist_1.global_position = arena_center + Vector3(-8, 0, 0)
	duelist_2.global_position = arena_center + Vector3(8, 0, 0)

	is_duel_active = true
	duel_timer = duel_duration_max
	
	if barrier_mesh:
		barrier_mesh.visible = true

	duelist_1.hero_died.connect(_on_duelist_died)
	duelist_2.hero_died.connect(_on_duelist_died)
	
	duel_started.emit(duelist_1, duelist_2)

func _enforce_duel_boundary() -> void:
	# Keep duelists inside arena_center within duel_radius
	for duelist in [duelist_1, duelist_2]:
		if is_instance_valid(duelist) and duelist.current_health > 0:
			var d: float = duelist.global_position.distance_to(arena_center)
			if d > duel_radius:
				var pull_dir: Vector3 = (arena_center - duelist.global_position).normalized()
				duelist.global_position = arena_center - pull_dir * (duel_radius - 0.5)

func _on_duelist_died(killer: Node) -> void:
	if not is_duel_active:
		return
	var victor: CharacterBody3D = duelist_1 if duelist_2.current_health <= 0 else duelist_2
	var loser: CharacterBody3D = duelist_2 if victor == duelist_1 else duelist_1
	_finish_duel(victor, loser)

func _end_duel_by_timeout() -> void:
	if not is_duel_active:
		return
	var victor: CharacterBody3D = duelist_1 if duelist_1.current_health >= duelist_2.current_health else duelist_2
	var loser: CharacterBody3D = duelist_2 if victor == duelist_1 else duelist_1
	_finish_duel(victor, loser)

func _finish_duel(victor: CharacterBody3D, loser: CharacterBody3D) -> void:
	is_duel_active = false
	time_until_duel = duel_interval_seconds
	
	if barrier_mesh:
		barrier_mesh.visible = false

	if is_instance_valid(duelist_1):
		duelist_1.is_in_duel = false
		duelist_1.global_position = original_pos_1
		if duelist_1.hero_died.is_connected(_on_duelist_died):
			duelist_1.hero_died.disconnect(_on_duelist_died)
			
	if is_instance_valid(duelist_2):
		duelist_2.is_in_duel = false
		duelist_2.global_position = original_pos_2
		if duelist_2.hero_died.is_connected(_on_duelist_died):
			duelist_2.hero_died.disconnect(_on_duelist_died)

	duel_ended.emit(victor, loser)
