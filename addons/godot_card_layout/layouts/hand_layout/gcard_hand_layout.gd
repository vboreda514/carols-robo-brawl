@tool
@icon("res://addons/godot_card_layout/resources/icons/hand_layout_icon.svg")
class_name GCardHandLayout
extends Control

signal card_hoverd(card: Control, index: int)
signal cards_unhovered()
signal card_dragging_started(card: Control, index: int)
signal card_dragging_finished(card: Control, index: int)

@export_group("card size")
@export var card_width: float = 64.0
@export var card_height: float = 96.0
@export var width_scale_factor: float = 4
@export var height_scale_factor: float = 1.5
@export var x_offset_adjust: float = .15
@export var y_offset_adjust: float = .1

@export_group("idle layout")
@export var dynamic_radius := true: set = _set_dynamic_radius
@export var dynamic_radius_factor: float = 100.0: set = _set_dynamic_radius_factor
@export var radius := 1000.0: set = _set_radius
@export var circle_percentage: float = 0.05: set = _set_circle_percentage

@export_group("hover")
@export var enable_hover := true: set = _set_enable_hover
@export var hovered_index := -1: set = _set_hovered_index
@export var hover_padding := 40.0: set = _set_hover_padding
@export var hovered_scale := Vector2(1.1, 1.1): set = _set_hovered_scale
@export var hover_relative_position := Vector2(0, -20): set = _set_hover_relative_position

@export_group("dragging")
@export var enable_dragging := false: set = _set_enable_dragging
@export var dragging_scale := Vector2(1.1, 1.1)

@export_group("animation")
@export var animation_time := 0.1: set = _set_animation_time
@export var animation_ease := Tween.EASE_IN
@export var animation_trans := Tween.TRANS_QUAD

@export_group("sounds")
@export var hover_sound: AudioStreamPlayer2D

var gcard_hand_layout_service := GCardHandLayoutService.new()
var _reset_position_tween: Tween
var _mouse_in: bool = false
var _dragging_card: Control
var _dragging_index: int = -100
var _dragging_mouse_position: Vector2

func _ready():
	_dragging_index = -100
	child_order_changed.connect(_on_child_order_changed)
	_setup_cards()
	if get_child_count() > 0:
		_reset_positions_if_in_tree(false, false)

func _process(delta):
	if Engine.is_editor_hint() or (!enable_hover and !_dragging_card):
		return
	if _dragging_card:
		_dragging_card.global_position = _get_global_mouse_position_for_interaction() - _dragging_mouse_position
	elif _mouse_in:
		var new_hover_index := _find_card_index_with_point(_get_global_mouse_position_for_interaction())
		if hovered_index != new_hover_index:
			hovered_index = new_hover_index
	elif hovered_index != -1:
		hovered_index = -1

func _enter_tree():
	if is_node_ready() and get_child_count() > 0:
		_reset_positions_if_in_tree(false, false)

func _validate_property(property):
	if property.name in ["radius"] and dynamic_radius:
		property.usage = PROPERTY_USAGE_NO_EDITOR
	if property.name in ["dynamic_radius_factor"] and not dynamic_radius:
		property.usage = PROPERTY_USAGE_NO_EDITOR
	if property.name in ["hovered_index", "hover_padding", "hovered_scale", "hover_relative_position"] and not enable_hover:
		property.usage = PROPERTY_USAGE_NO_EDITOR
	if property.name in ["dragging_scale"] and not enable_dragging:
		property.usage = PROPERTY_USAGE_NO_EDITOR
	if property.name in ["animation_ease", "animation_trans"] and animation_time <= 0.0:
		property.usage = PROPERTY_USAGE_NO_EDITOR

func _reset_positions_if_in_tree(recalculate_curve: bool = false, animated: bool = true):
	if is_inside_tree():
		_reset_positions(recalculate_curve, animated)

func _reset_positions(recalculate_curve: bool = false, animated: bool = true):
	var number_of_cards := get_child_count()
	if _dragging_card and _dragging_card.get_parent() == self:
		number_of_cards -= 1

	# Ensure container resizes to fit this layout
	custom_minimum_size = Vector2(
		card_width * width_scale_factor,
		card_height * height_scale_factor
	)

	gcard_hand_layout_service.number_of_cards = number_of_cards
	gcard_hand_layout_service.dynamic_radius = dynamic_radius
	gcard_hand_layout_service.dynamic_radius_factor = dynamic_radius_factor
	gcard_hand_layout_service.radius = radius
	gcard_hand_layout_service.circle_percentage = circle_percentage
	gcard_hand_layout_service.hover_padding = hover_padding
	gcard_hand_layout_service.hovered_index = hovered_index
	gcard_hand_layout_service.hover_relative_position = hover_relative_position

	var layout_infos := gcard_hand_layout_service.get_card_layouts()
	
	# Compute bounding box of layout to center it
	var min_x := INF
	var max_x := -INF
	var min_y := INF
	var max_y := -INF
	for info in layout_infos:
		min_x = min(min_x, info.position.x)
		max_x = max(max_x, info.position.x)
		min_y = min(min_y, info.position.y)
		max_y = max(max_y, info.position.y)

	# Calculate base offsets to center content
	var layout_width := max_x - min_x
	var layout_height := max_y - min_y
	var offset_x := (size.x - layout_width) * x_offset_adjust - min_x 
	var offset_y := (size.y - layout_height) * y_offset_adjust - min_y
	
	var position_index := 0
	var should_animate := animation_time > 0.0 and animated and number_of_cards > 0 and is_inside_tree()

	if _reset_position_tween and _reset_position_tween.is_running():
		_reset_position_tween.stop()
	if should_animate:
		_reset_position_tween = create_tween()

	for i in get_child_count():
		var card: Control = get_children()[i]
		var target_scale = Vector2.ONE
		var target_position: Vector2
		var target_rotation := 0.0

		if i == _dragging_index:
			target_scale = dragging_scale
		else:
			var layout_info: GCardLayoutInfo = layout_infos[position_index]
			
			# target_position = layout_info.position
			target_position = layout_info.position + Vector2(offset_x, offset_y)
			
			target_rotation = layout_info.rotation
			if i == hovered_index:
				target_rotation = 0
				target_scale = hovered_scale

		if not should_animate:
			if i != _dragging_index:
				card.position = target_position
			card.rotation = target_rotation
			card.scale = target_scale
		else:
			if i != _dragging_index:
				_reset_position_tween.parallel().tween_property(card, "position", target_position, animation_time).set_ease(animation_ease).set_trans(animation_trans)
			_reset_position_tween.parallel().tween_property(card, "rotation", target_rotation, animation_time).set_ease(animation_ease).set_trans(animation_trans)
			_reset_position_tween.parallel().tween_property(card, "scale", target_scale, animation_time).set_ease(animation_ease).set_trans(animation_trans)

		if i != _dragging_index:
			position_index += 1

	if should_animate:
		_reset_position_tween.play()

func _setup_cards():
	for child in get_children():
		var card = child as Control
		card.custom_minimum_size = Vector2(card_width, card_height)
		if card.mouse_entered.is_connected(_on_child_mouse_entered):
			continue
		card.mouse_entered.connect(_on_child_mouse_entered)
		card.mouse_exited.connect(_on_child_mouse_exited)
		card.gui_input.connect(_on_child_gui_input.bind(card))

func _find_card_index_with_point(global_point: Vector2) -> int:
	for i in range(get_child_count() - 1, -1, -1):
		var card = get_children()[i] as Control
		if card.get_global_rect().has_point(global_point):
			return i
	return -1

func _set_dynamic_radius(val: bool):
	if dynamic_radius == val:
		return
	dynamic_radius = val
	notify_property_list_changed()
	_reset_positions_if_in_tree()

func _set_dynamic_radius_factor(val: float):
	dynamic_radius_factor = val
	_reset_positions_if_in_tree()

func _set_radius(val: float):
	radius = val
	_reset_positions_if_in_tree()

func _set_circle_percentage(val: float):
	circle_percentage = val
	_reset_positions_if_in_tree()

func _set_enable_hover(val: bool):
	enable_hover = val
	notify_property_list_changed()

func _set_hovered_index(val: int):
	hovered_index = val
	if hovered_index >= get_child_count():
		return
	_reset_positions_if_in_tree()
	var card: Control = null
	if hovered_index >= 0:
		if hover_sound:
			hover_sound.play()
		card = get_children()[hovered_index]
	if hovered_index >= 0:
		card_hoverd.emit(card, hovered_index)
	else:
		cards_unhovered.emit()

func _set_hover_padding(val: float):
	hover_padding = val
	_reset_positions_if_in_tree()

func _set_hovered_scale(val: Vector2):
	hovered_scale = val
	_reset_positions_if_in_tree()

func _set_hover_relative_position(val: Vector2):
	hover_relative_position = val
	_reset_positions_if_in_tree()

func _set_enable_dragging(val: bool):
	enable_dragging = val
	notify_property_list_changed()

func _set_animation_time(val: float):
	animation_time = val
	notify_property_list_changed()

func _get_global_mouse_position_for_interaction():
	return get_global_mouse_position()

func _on_child_mouse_entered():
	if _dragging_card:
		return
	_mouse_in = true

func _on_child_mouse_exited():
	if _dragging_card:
		return
	_mouse_in = false

func _on_child_order_changed():
	_setup_cards()
	_reset_positions_if_in_tree()

func _on_child_gui_input(event: InputEvent, card: Control):
	if not enable_dragging:
		return
	if event is InputEventMouseButton:
		var mouse_button_event = event as InputEventMouseButton
		_dragging_index = get_children().find(card) if card.get_parent() == self else -1
		if mouse_button_event.pressed and mouse_button_event.button_index == MOUSE_BUTTON_LEFT:
			_dragging_card = card
			_dragging_mouse_position = card.get_local_mouse_position()
			_dragging_card.z_index = 1
			hovered_index = -1
			card_dragging_started.emit(_dragging_card, _dragging_index)
			_reset_positions_if_in_tree()
		elif not mouse_button_event.pressed and mouse_button_event.button_index == MOUSE_BUTTON_LEFT:
			_dragging_card = null
			card.z_index = 0
			card_dragging_finished.emit(card, _dragging_index)
			_dragging_index = -100
			_reset_positions_if_in_tree()
