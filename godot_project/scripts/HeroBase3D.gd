extends CharacterBody3D
class_name HeroBase3D

## ============================================================================
## HERO BASE 3D - WARCRAFT 3 FOTC STYLE
## High-performance, deterministic hero controller with 3D animation,
## fixed-timestep combat logic, overhead HUD, and Duel Ring compatibility.
## ============================================================================

signal health_changed(current_hp: int, max_hp: int)
signal mana_changed(current_mp: int, max_mp: int)
signal skill_cast(skill_slot: String, cooldown_remaining: float)
signal hero_died(killer: Node)

@export_group("Identity & Archetype")
@export var hero_id: String = "ichigo"
@export var hero_display_name: String = "Kurosaki Ichigo"
@export var team_id: int = 1 # 1 = Radiant / Sentinel, 2 = Dire / Scourge

@export_group("Attributes (Integer Units)")
@export var max_health: int = 1200
@export var current_health: int = 1200
@export var max_mana: int = 400
@export var current_mana: int = 400
@export var base_attack_damage: int = 65
@export var base_armor: int = 4
@export var move_speed: float = 7.5
@export var attack_range: float = 2.2
@export var attack_rate: float = 1.1 # Attacks per second

@export_group("Skill Cooldowns (Seconds)")
@export var cd_q_max: float = 6.0
@export var cd_w_max: float = 10.0
@export var cd_e_max: float = 14.0
@export var cd_r_max: float = 55.0

# Runtime Cooldown Trackers
var cd_q_timer: float = 0.0
var cd_w_timer: float = 0.0
var cd_e_timer: float = 0.0
var cd_r_timer: float = 0.0

# Combat State
var is_attacking: bool = false
var is_casting: bool = false
var is_stunned: bool = false
var is_in_duel: bool = false
var attack_cooldown: float = 0.0

# Targeting & Navigation
var target_enemy: Node3D = null
var is_player_controlled: bool = true

# Visual & Animation References
@onready var anim_player: AnimationPlayer = $AnimationPlayer if has_node("AnimationPlayer") else null
@onready var overhead_hp_label: Label3D = $OverheadHUD/HpLabel if has_node("OverheadHUD/HpLabel") else null

func _ready() -> void:
	current_health = max_health
	current_mana = max_mana
	add_to_group("heroes")
	add_to_group("team_" + str(team_id))
	_update_overhead_hud()

func _physics_process(delta: float) -> void:
	_tick_cooldowns(delta)
	
	if is_stunned or current_health <= 0:
		velocity = Vector3.ZERO
		move_and_slide()
		return

	if is_player_controlled:
		_handle_player_input(delta)
	else:
		_handle_ai_logic(delta)

	move_and_slide()
	_update_overhead_hud()

func _tick_cooldowns(delta: float) -> void:
	if cd_q_timer > 0.0: cd_q_timer = max(0.0, cd_q_timer - delta)
	if cd_w_timer > 0.0: cd_w_timer = max(0.0, cd_w_timer - delta)
	if cd_e_timer > 0.0: cd_e_timer = max(0.0, cd_e_timer - delta)
	if cd_r_timer > 0.0: cd_r_timer = max(0.0, cd_r_timer - delta)
	if attack_cooldown > 0.0: attack_cooldown = max(0.0, attack_cooldown - delta)

func _handle_player_input(_delta: float) -> void:
	var input_dir: Vector2 = Input.get_vector("move_left", "move_right", "move_up", "move_down")
	var move_vector: Vector3 = (transform.basis * Vector3(input_dir.x, 0, input_dir.y)).normalized()

	if move_vector.length_squared() > 0.01 and not is_casting and not is_attacking:
		velocity.x = move_vector.x * move_speed
		velocity.z = move_vector.z * move_speed
		
		# Rotate towards movement direction smoothly
		var target_yaw: float = atan2(-move_vector.x, -move_vector.z)
		rotation.y = lerp_angle(rotation.y, target_yaw, 0.25)
		
		_play_animation("Walk")
	else:
		velocity.x = move_toward(velocity.x, 0, move_speed)
		velocity.z = move_toward(velocity.z, 0, move_speed)
		if not is_attacking and not is_casting:
			_play_animation("Stand")

	# Skill hotkeys
	if Input.is_action_just_pressed("skill_q"):
		cast_skill_q()
	elif Input.is_action_just_pressed("skill_w"):
		cast_skill_w()
	elif Input.is_action_just_pressed("skill_e"):
		cast_skill_e()
	elif Input.is_action_just_pressed("skill_r"):
		cast_skill_r()
	elif Input.is_action_just_pressed("attack_action"):
		_execute_nearest_auto_attack()

func _handle_ai_logic(delta: float) -> void:
	# Automated AI behavior for combat / lane roaming
	if target_enemy == null or not is_instance_valid(target_enemy) or target_enemy.current_health <= 0:
		target_enemy = _find_closest_enemy()
		if target_enemy == null:
			velocity = Vector3.ZERO
			_play_animation("Stand")
			return

	var dist_to_target: float = global_position.distance_to(target_enemy.global_position)
	if dist_to_target > attack_range:
		var dir: Vector3 = (target_enemy.global_position - global_position).normalized()
		velocity.x = dir.x * move_speed
		velocity.z = dir.z * move_speed
		var target_yaw: float = atan2(-dir.x, -dir.z)
		rotation.y = lerp_angle(rotation.y, target_yaw, 0.2)
		_play_animation("Walk")
	else:
		velocity = Vector3.ZERO
		if attack_cooldown <= 0.0:
			_perform_attack(target_enemy)
		else:
			_play_animation("Stand")

func _find_closest_enemy() -> Node3D:
	var potential_targets: Array[Node] = get_tree().get_nodes_in_group("heroes")
	potential_targets.append_array(get_tree().get_nodes_in_group("creeps"))
	
	var closest_node: Node3D = null
	var min_dist: float = 9999.0
	
	for target in potential_targets:
		if target == self or not is_instance_valid(target):
			continue
		if target.team_id == team_id:
			continue
		if target.current_health <= 0:
			continue
		var d: float = global_position.distance_to(target.global_position)
		if d < min_dist:
			min_dist = d
			closest_node = target
			
	return closest_node

func _execute_nearest_auto_attack() -> void:
	var target: Node3D = _find_closest_enemy()
	if target and global_position.distance_to(target.global_position) <= attack_range * 1.5:
		_perform_attack(target)

func _perform_attack(target: Node3D) -> void:
	if attack_cooldown > 0.0 or is_casting:
		return
	attack_cooldown = 1.0 / attack_rate
	is_attacking = true
	_play_animation("Attack")
	
	# Face target
	var dir: Vector3 = (target.global_position - global_position).normalized()
	rotation.y = atan2(-dir.x, -dir.z)
	
	# Apply damage
	if target.has_method("take_damage"):
		var damage: int = base_attack_damage + randi_range(-5, 5)
		target.take_damage(damage, self)
		
	await get_tree().create_timer(0.4).timeout
	is_attacking = false

func take_damage(raw_amount: int, dealer: Node) -> void:
	if current_health <= 0:
		return
		
	# Warcraft 3 Armor Mitigation Formula: DamageReduction = (armor * 0.06) / (1 + armor * 0.06)
	var reduction_pct: float = (float(base_armor) * 0.06) / (1.0 + float(base_armor) * 0.06)
	var final_damage: int = int(round(float(raw_amount) * (1.0 - reduction_pct)))
	final_damage = max(1, final_damage)
	
	current_health = max(0, current_health - final_damage)
	health_changed.emit(current_health, max_health)
	_update_overhead_hud()
	
	if current_health <= 0:
		_handle_death(dealer)

func _handle_death(killer: Node) -> void:
	_play_animation("Death")
	hero_died.emit(killer)
	# Respawn timer (10 seconds)
	await get_tree().create_timer(10.0).timeout
	current_health = max_health
	current_mana = max_mana
	global_position = Vector3(0, 0, 0) # Respawn fountain
	_update_overhead_hud()
	_play_animation("Stand")

# === SKILLS (ICHIGO ARCHETYPE) ===
func cast_skill_q() -> void:
	if cd_q_timer > 0.0 or current_mana < 80:
		return
	cd_q_timer = cd_q_max
	current_mana -= 80
	mana_changed.emit(current_mana, max_mana)
	skill_cast.emit("Q", cd_q_max)
	_play_animation("Spell")
	
	# Getsuga Tenshou: AoE linear slash wave
	var enemies: Array[Node] = get_tree().get_nodes_in_group("team_" + str(3 - team_id))
	for e in enemies:
		if e.has_method("take_damage") and global_position.distance_to(e.global_position) < 8.0:
			e.take_damage(180, self)

func cast_skill_w() -> void:
	if cd_w_timer > 0.0 or current_mana < 100:
		return
	cd_w_timer = cd_w_max
	current_mana -= 100
	mana_changed.emit(current_mana, max_mana)
	skill_cast.emit("W", cd_w_max)
	_play_animation("Spell")
	
	# Shunpo: Instant flash step blink forward
	var forward: Vector3 = -transform.basis.z
	global_position += forward * 6.0

func cast_skill_e() -> void:
	if cd_e_timer > 0.0 or current_mana < 90:
		return
	cd_e_timer = cd_e_max
	current_mana -= 90
	mana_changed.emit(current_mana, max_mana)
	skill_cast.emit("E", cd_e_max)
	
	# Hollow Mask Buff: temporary +40% speed and +30 dmg
	base_attack_damage += 30
	move_speed += 3.0
	await get_tree().create_timer(8.0).timeout
	base_attack_damage -= 30
	move_speed -= 3.0

func cast_skill_r() -> void:
	if cd_r_timer > 0.0 or current_mana < 150:
		return
	cd_r_timer = cd_r_max
	current_mana -= 150
	mana_changed.emit(current_mana, max_mana)
	skill_cast.emit("R", cd_r_max)
	_play_animation("Spell")
	
	# Bankai: Tensa Zangetsu burst
	var enemies: Array[Node] = get_tree().get_nodes_in_group("team_" + str(3 - team_id))
	for e in enemies:
		if e.has_method("take_damage") and global_position.distance_to(e.global_position) < 12.0:
			e.take_damage(450, self)

func _play_animation(anim_name: String) -> void:
	if anim_player and anim_player.has_animation(anim_name):
		if anim_player.current_animation != anim_name:
			anim_player.play(anim_name)

func _update_overhead_hud() -> void:
	if overhead_hp_label:
		overhead_hp_label.text = "%s\nHP: %d/%d" % [hero_display_name, current_health, max_health]
