extends CharacterBody3D
class_name CreepAI3D

## ============================================================================
## CREEP AI 3D - WARCRAFT 3 AUTOMATED LANE CREW
## High-performance automated lane minion with waypoint pathfinding,
## aggro acquisition, melee attack, and gold reward.
## ============================================================================

@export var creep_name: String = "Swordsman"
@export var team_id: int = 1 # 1 = Radiant, 2 = Dire
@export var max_health: int = 450
@export var current_health: int = 450
@export var attack_damage: int = 24
@export var armor: int = 2
@export var move_speed: float = 4.8
@export var attack_range: float = 1.8
@export var gold_reward: int = 38

var target: Node3D = null
var attack_cooldown: float = 0.0
var waypoints: Array[Vector3] = []
var current_waypoint_index: int = 0

@onready var overhead_label: Label3D = $OverheadLabel if has_node("OverheadLabel") else null

func _ready() -> void:
	current_health = max_health
	add_to_group("creeps")
	add_to_group("team_" + str(team_id))
	_update_hud()

func _physics_process(delta: float) -> void:
	if current_health <= 0:
		return

	if attack_cooldown > 0.0:
		attack_cooldown = max(0.0, attack_cooldown - delta)

	# 1. Check for nearby enemies in aggro radius (8 meters)
	if target == null or not is_instance_valid(target) or target.current_health <= 0:
		target = _scan_for_enemies(8.0)

	if target != null and is_instance_valid(target) and target.current_health > 0:
		_engage_target(delta)
	else:
		_march_lane(delta)

	move_and_slide()
	_update_hud()

func _scan_for_enemies(radius: float) -> Node3D:
	var candidates: Array[Node] = get_tree().get_nodes_in_group("heroes")
	candidates.append_array(get_tree().get_nodes_in_group("creeps"))
	
	var closest: Node3D = null
	var min_d: float = radius
	
	for c in candidates:
		if c == self or not is_instance_valid(c) or c.current_health <= 0:
			continue
		if c.team_id == team_id:
			continue
		var d: float = global_position.distance_to(c.global_position)
		if d < min_d:
			min_d = d
			closest = c
			
	return closest

func _engage_target(_delta: float) -> void:
	var dist: float = global_position.distance_to(target.global_position)
	if dist > attack_range:
		var dir: Vector3 = (target.global_position - global_position).normalized()
		velocity.x = dir.x * move_speed
		velocity.z = dir.z * move_speed
		rotation.y = atan2(-dir.x, -dir.z)
	else:
		velocity.x = 0
		velocity.z = 0
		if attack_cooldown <= 0.0:
			_attack(target)

func _march_lane(_delta: float) -> void:
	if waypoints.is_empty():
		velocity = Vector3.ZERO
		return
		
	var target_wp: Vector3 = waypoints[current_waypoint_index]
	var dist: float = global_position.distance_to(target_wp)
	
	if dist < 1.5:
		current_waypoint_index = min(current_waypoint_index + 1, waypoints.size() - 1)
		target_wp = waypoints[current_waypoint_index]

	var dir: Vector3 = (target_wp - global_position).normalized()
	velocity.x = dir.x * move_speed
	velocity.z = dir.z * move_speed
	rotation.y = atan2(-dir.x, -dir.z)

func _attack(victim: Node3D) -> void:
	attack_cooldown = 1.35
	if victim.has_method("take_damage"):
		victim.take_damage(attack_damage, self)

func take_damage(raw_amount: int, dealer: Node) -> void:
	if current_health <= 0:
		return
	var reduction: float = (float(armor) * 0.06) / (1.0 + float(armor) * 0.06)
	var final_dmg: int = max(1, int(round(float(raw_amount) * (1.0 - reduction))))
	current_health = max(0, current_health - final_dmg)
	_update_hud()
	
	if current_health <= 0:
		_die(dealer)

func _die(killer: Node) -> void:
	if killer and killer.has_method("award_gold"):
		killer.award_gold(gold_reward)
	queue_free()

func _update_hud() -> void:
	if overhead_label:
		overhead_label.text = "%s [%d/%d]" % [creep_name, current_health, max_health]
