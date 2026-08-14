#pragma once

#include <cstdint>

namespace dotnet_loader {

// Object pointers are passed as void* to keep this header dependency-free.
// String return values use stable buffers (thread-local / static) valid until
// the next bridge call; the managed side copies them immediately.
// Location/Vector values are transferred as float arrays:
//   location: [x, y, z, pitch, yaw]
//   vector:   [x, y, z]

struct BridgeTable {
    // ---- player ----
    const char *(*player_get_name)(void *);
    void (*player_send_message)(void *, const char *);
    void (*player_send_error_message)(void *, const char *);
    void (*player_send_popup)(void *, const char *);
    void (*player_send_tip)(void *, const char *);
    void (*player_send_toast)(void *, const char *, const char *);
    void (*player_send_title)(void *, const char *, const char *);
    void (*player_reset_title)(void *);
    void (*player_kick)(void *, const char *);
    bool (*player_perform_command)(void *, const char *);
    bool (*player_is_op)(void *);
    void (*player_set_op)(void *, bool);
    const char *(*player_get_xuid)(void *);
    const char *(*player_get_address)(void *);
    bool (*player_is_sneaking)(void *);
    void (*player_set_sneaking)(void *, bool);
    bool (*player_is_sprinting)(void *);
    void (*player_set_sprinting)(void *, bool);
    int (*player_get_ping)(void *);
    const char *(*player_get_locale)(void *);
    const char *(*player_get_device_os)(void *);
    const char *(*player_get_device_id)(void *);
    const char *(*player_get_game_version)(void *);
    int (*player_get_game_mode)(void *);
    void (*player_set_game_mode)(void *, int);
    bool (*player_get_allow_flight)(void *);
    void (*player_set_allow_flight)(void *, bool);
    bool (*player_is_flying)(void *);
    void (*player_set_flying)(void *, bool);
    int (*player_get_exp_level)(void *);
    void (*player_set_exp_level)(void *, int);
    void (*player_give_exp)(void *, int);
    void (*player_give_exp_levels)(void *, int);
    float (*player_get_exp_progress)(void *);
    void (*player_set_exp_progress)(void *, float);
    int (*player_get_total_exp)(void *);
    void (*player_transfer)(void *, const char *, int);
    void (*player_play_sound)(void *, const float *, const char *, float, float);
    void (*player_stop_sound)(void *, const char *);
    void (*player_stop_all_sounds)(void *);
    void (*player_spawn_particle)(void *, const char *, const float *, const char *);
    float (*player_get_fly_speed)(void *);
    void (*player_set_fly_speed)(void *, float);
    float (*player_get_walk_speed)(void *);
    void (*player_set_walk_speed)(void *, float);
    void (*player_update_commands)(void *);
    void (*player_close_form)(void *);
    void (*player_send_packet)(void *, int, const void *, int);
    const char *(*player_get_skin_id)(void *);
    const char *(*player_get_skin_cape_id)(void *);
    void *(*player_get_item_in_hand)(void *);

    // ---- server ----
    const char *(*server_get_name)(void *);
    const char *(*server_get_version)(void *);
    const char *(*server_get_minecraft_version)(void *);
    int (*server_get_protocol_version)(void *);
    int (*server_get_max_players)(void *);
    void (*server_broadcast_message)(void *, const char *);
    int (*server_get_online_players)(void *, void **, int);
    void *(*server_get_player)(void *, const char *);
    void *(*server_get_console_sender)(void *);
    bool (*server_dispatch_command)(void *, void *, const char *);

    // ---- events: common ----
    void *(*event_get_player)(void *);
    void *(*event_get_actor)(void *);
    bool (*event_is_cancelled)(void *);
    void (*event_set_cancelled)(void *, bool);

    // ---- events: chat/command ----
    const char *(*chat_get_message)(void *);
    void (*chat_set_message)(void *, const char *);
    const char *(*chat_get_format)(void *);
    void (*chat_set_format)(void *, const char *);
    int (*chat_get_recipient_count)(void *);
    const char *(*command_get_command)(void *);
    void (*command_set_command)(void *, const char *);
    const char *(*server_cmd_get_command)(void *);
    void (*server_cmd_set_command)(void *, const char *);
    const char *(*server_cmd_get_sender_name)(void *);
    void *(*server_cmd_get_sender)(void *);

    // ---- events: movement / teleport ----
    void (*move_get_from)(void *, float *);
    void (*move_get_to)(void *, float *);
    void (*move_set_from)(void *, const float *);
    void (*move_set_to)(void *, const float *);
    void (*actor_tp_get_from)(void *, float *);
    void (*actor_tp_get_to)(void *, float *);
    void (*actor_tp_set_from)(void *, const float *);
    void (*actor_tp_set_to)(void *, const float *);

    // ---- events: interact ----
    int (*interact_get_action)(void *);
    int (*interact_get_clicked_position)(void *, float *);
    bool (*interact_has_item)(void *);
    void *(*interact_get_item)(void *);
    bool (*interact_has_block)(void *);
    void *(*interact_get_block)(void *);
    int (*interact_get_block_face)(void *);
    void *(*interact_actor_get_actor)(void *);

    // ---- events: actor ----
    float (*actor_damage_get_damage)(void *);
    void (*actor_damage_set_damage)(void *, float);
    void *(*event_get_damage_source)(void *);
    void (*actor_explode_get_location)(void *, float *);
    int (*actor_explode_get_block_count)(void *);
    void *(*actor_explode_get_block)(void *, int);
    void *(*actor_knockback_get_source)(void *);
    void (*actor_knockback_get_vector)(void *, float *);
    void (*actor_knockback_set_vector)(void *, const float *);

    // ---- events: player ----
    const char *(*death_get_message)(void *);
    void (*death_set_message)(void *, const char *);
    void *(*bed_get_bed)(void *);
    const char *(*dim_change_get_from)(void *);
    const char *(*dim_change_get_to)(void *);
    void *(*drop_get_item)(void *);
    const char *(*emote_get_id)(void *);
    bool (*emote_is_muted)(void *);
    void (*emote_set_muted)(void *, bool);
    int (*gm_change_get_new_mode)(void *);
    void *(*consume_get_item)(void *);
    int (*consume_get_hand)(void *);
    int (*held_get_previous_slot)(void *);
    int (*held_get_new_slot)(void *);
    const char *(*join_get_message)(void *);
    void (*join_set_message)(void *, const char *);
    const char *(*quit_get_message)(void *);
    void (*quit_set_message)(void *, const char *);
    const char *(*kick_get_reason)(void *);
    void (*kick_set_reason)(void *, const char *);
    const char *(*login_get_kick_message)(void *);
    void (*login_set_kick_message)(void *, const char *);
    void *(*pickup_get_item)(void *);
    const char *(*skin_change_get_new_skin_id)(void *);
    const char *(*skin_change_get_new_skin_cape_id)(void *);
    const char *(*skin_change_get_message)(void *);
    void (*skin_change_set_message)(void *, const char *);

    // ---- events: block ----
    void *(*cook_get_source)(void *);
    void *(*cook_get_result)(void *);
    int (*block_explode_get_block_count)(void *);
    void *(*block_explode_get_block)(void *, int);
    void *(*grow_get_new_state)(void *);
    void *(*from_to_get_to_block)(void *);
    int (*piston_get_direction)(void *);
    void *(*place_get_placed_state)(void *);
    void *(*place_get_against)(void *);

    // ---- events: chunk ----
    int (*chunk_get_x)(void *);
    int (*chunk_get_z)(void *);
    const char *(*chunk_get_dimension_name)(void *);

    // ---- events: server ----
    const char *(*broadcast_get_message)(void *);
    void (*broadcast_set_message)(void *, const char *);
    int (*broadcast_get_recipient_count)(void *);
    int (*packet_get_id)(void *);
    const char *(*packet_get_payload)(void *, int *);
    void (*packet_set_payload)(void *, const void *, int);
    void *(*packet_get_player)(void *);
    const char *(*packet_get_address)(void *);
    int (*packet_get_sub_client_id)(void *);
    const char *(*plugin_event_get_plugin_name)(void *);
    const char *(*script_get_message_id)(void *);
    const char *(*script_get_message)(void *);
    const char *(*script_get_sender_name)(void *);
    const char *(*ping_get_address)(void *);
    const char *(*ping_get_server_guid)(void *);
    void (*ping_set_server_guid)(void *, const char *);
    int (*ping_get_local_port)(void *);
    void (*ping_set_local_port)(void *, int);
    int (*ping_get_local_port_v6)(void *);
    void (*ping_set_local_port_v6)(void *, int);
    const char *(*ping_get_motd)(void *);
    void (*ping_set_motd)(void *, const char *);
    int (*ping_get_network_protocol_version)(void *);
    const char *(*ping_get_minecraft_version_network)(void *);
    void (*ping_set_minecraft_version_network)(void *, const char *);
    int (*ping_get_num_players)(void *);
    void (*ping_set_num_players)(void *, int);
    int (*ping_get_max_players)(void *);
    void (*ping_set_max_players)(void *, int);
    const char *(*ping_get_level_name)(void *);
    void (*ping_set_level_name)(void *, const char *);
    int (*ping_get_game_mode)(void *);
    void (*ping_set_game_mode)(void *, int);
    int (*server_load_get_type)(void *);
    bool (*thunder_change_get_to)(void *);
    bool (*weather_change_get_to)(void *);

    // ---- objects: actor / mob ----
    const char *(*actor_get_type)(void *);
    uint64_t (*actor_get_runtime_id)(void *);
    void (*actor_get_location)(void *, float *);
    void (*actor_get_velocity)(void *, float *);
    bool (*actor_is_on_ground)(void *);
    bool (*actor_is_in_water)(void *);
    bool (*actor_is_in_lava)(void *);
    bool (*actor_is_dead)(void *);
    bool (*actor_is_valid)(void *);
    const char *(*actor_get_dimension_name)(void *);
    const char *(*actor_get_name_tag)(void *);
    const char *(*actor_get_score_tag)(void *);
    int64_t (*actor_get_id)(void *);
    void (*actor_set_rotation)(void *, float, float);
    bool (*actor_teleport_location)(void *, const float *);
    bool (*actor_teleport_actor)(void *, void *);
    void (*actor_remove)(void *);
    void (*actor_send_message)(void *, const char *);
    const char *(*actor_get_name)(void *);
    int (*actor_get_scoreboard_tag_count)(void *);
    const char *(*actor_get_scoreboard_tag)(void *, int);
    bool (*actor_add_scoreboard_tag)(void *, const char *);
    bool (*actor_remove_scoreboard_tag)(void *, const char *);
    bool (*actor_is_name_tag_visible)(void *);
    void (*actor_set_name_tag_visible)(void *, bool);
    bool (*actor_is_name_tag_always_visible)(void *);
    void (*actor_set_name_tag_always_visible)(void *, bool);
    void (*actor_set_name_tag)(void *, const char *);
    void (*actor_set_score_tag)(void *, const char *);
    int (*mob_get_health)(void *);
    void (*mob_set_health)(void *, int);
    int (*mob_get_max_health)(void *);
    void (*mob_set_max_health)(void *, int);
    bool (*mob_is_gliding)(void *);
    void *(*actor_as_mob)(void *);
    void *(*actor_get_dimension)(void *);
    const char *(*dimension_get_name)(void *);
    void *(*dimension_get_block_at)(void *, int, int, int);
    void *(*actor_spawn_actor)(void *, const float *, const char *);

    // ---- objects: item / block / damage source ----
    const char *(*item_get_type)(void *);
    int (*item_get_amount)(void *);
    int (*item_get_data)(void *);
    int (*item_get_max_stack_size)(void *);
    const char *(*item_get_translation_key)(void *);
    const char *(*item_actor_get_type)(void *);
    int (*item_actor_get_amount)(void *);
    const char *(*item_actor_get_translation_key)(void *);
    bool (*item_has_display_name)(void *);
    const char *(*item_get_display_name)(void *);
    bool (*item_has_lore)(void *);
    int (*item_get_lore_count)(void *);
    const char *(*item_get_lore_line)(void *, int);
    bool (*item_has_damage)(void *);
    int (*item_get_damage)(void *);
    bool (*item_is_unbreakable)(void *);
    bool (*item_has_enchants)(void *);
    int (*item_get_enchant_count)(void *);
    const char *(*item_get_enchant_name)(void *, int);
    int (*item_get_enchant_level)(void *, int);
    const char *(*block_get_type)(void *);
    int (*block_get_x)(void *);
    int (*block_get_y)(void *);
    int (*block_get_z)(void *);
    void (*block_set_type)(void *, const char *);
    void (*block_set_type_physics)(void *, const char *, bool);
    void (*block_get_location)(void *, float *);
    const char *(*block_get_dimension_name)(void *);
    void *(*block_get_relative)(void *, int, int, int);
    void *(*block_capture_state)(void *);
    void (*block_delete)(void *);
    const char *(*block_state_get_type)(void *);
    int (*block_state_get_x)(void *);
    int (*block_state_get_y)(void *);
    int (*block_state_get_z)(void *);
    void (*block_state_set_type)(void *, const char *);
    void (*block_state_get_location)(void *, float *);
    bool (*block_state_update)(void *);
    bool (*block_state_update_force)(void *, bool);
    bool (*block_state_update_force_physics)(void *, bool, bool);
    void (*block_state_delete)(void *);
    const char *(*damage_source_get_type)(void *);
    void *(*damage_source_get_actor)(void *);
    void *(*damage_source_get_damaging_actor)(void *);
    bool (*damage_source_is_indirect)(void *);

    // ---- objects: sender ----
    const char *(*sender_get_name)(void *);
    void (*sender_send_message)(void *, const char *);
    void (*sender_send_error_message)(void *, const char *);
    bool (*sender_has_permission)(void *, const char *);
    void *(*sender_as_player)(void *);

    // ---- objects: form ----
    void *(*form_create)(int);
    void (*form_set_title)(void *, const char *);
    void (*form_set_content)(void *, const char *);
    void (*form_set_button1)(void *, const char *);
    void (*form_set_button2)(void *, const char *);
    void (*form_add_button)(void *, const char *, const char *);
    void (*form_add_control)(void *, int, const char *, const char *, const char *);
    void (*form_set_submit_button)(void *, const char *);
    void (*form_set_icon)(void *, const char *);
    void (*form_set_callbacks)(void *, uint64_t);
    void (*form_send)(void *, void *);
    void (*form_destroy)(void *);
    void (*form_dispatch_result)(void *, int, uint64_t, int, const char *);

    // ---- objects: boss bar ----
    // flags is a bitmask: bit0 = DarkenSky, bit1 = CreateFog
    void *(*server_create_boss_bar)(void *, const char *, int, int, int);
    const char *(*boss_bar_get_title)(void *);
    void (*boss_bar_set_title)(void *, const char *);
    int (*boss_bar_get_color)(void *);
    void (*boss_bar_set_color)(void *, int);
    int (*boss_bar_get_style)(void *);
    void (*boss_bar_set_style)(void *, int);
    bool (*boss_bar_has_flag)(void *, int);
    void (*boss_bar_add_flag)(void *, int);
    void (*boss_bar_remove_flag)(void *, int);
    float (*boss_bar_get_progress)(void *);
    void (*boss_bar_set_progress)(void *, float);
    bool (*boss_bar_is_visible)(void *);
    void (*boss_bar_set_visible)(void *, bool);
    void (*boss_bar_add_player)(void *, void *);
    void (*boss_bar_remove_player)(void *, void *);
    void (*boss_bar_remove_all)(void *);
    int (*boss_bar_get_player_count)(void *);
    void *(*boss_bar_get_player)(void *, int);
    void (*boss_bar_destroy)(void *);

    // ---- objects: level ----
    void *(*server_get_level)(void *);
    const char *(*level_get_name)(void *);
    int (*level_get_time)(void *);
    void (*level_set_time)(void *, int);
    int64_t (*level_get_seed)(void *);
    int (*level_get_actors)(void *, void **, int);
    int (*level_get_dimensions)(void *, void **, int);
    void *(*level_get_dimension_by_name)(void *, const char *);

    // ---- objects: dimension ----
    int (*dimension_get_type)(void *);
    void *(*dimension_get_level)(void *);
    int (*dimension_get_highest_block_y_at)(void *, int, int);
    void *(*dimension_get_highest_block_at)(void *, int, int);
    int (*dimension_get_loaded_chunks)(void *, void **, int);
    int (*dimension_get_actors)(void *, void **, int);
    void *(*dimension_spawn_actor)(void *, const float *, const char *);
    void *(*dimension_drop_item)(void *, const float *, void *);

    // ---- objects: chunk / item stack ----
    int (*chunk_obj_get_x)(void *);
    int (*chunk_obj_get_z)(void *);
    void *(*chunk_obj_get_dimension)(void *);
    void (*chunk_obj_delete)(void *);
    void *(*item_stack_create)(const char *, int, int);
    void (*item_stack_delete)(void *);

    // ---- objects: map ----
    void *(*server_get_map)(void *, int64_t);
    void *(*server_create_map)(void *, void *);
    int64_t (*map_get_id)(void *);
    bool (*map_is_virtual)(void *);
    int (*map_get_scale)(void *);
    void (*map_set_scale)(void *, int);
    int (*map_get_center_x)(void *);
    int (*map_get_center_z)(void *);
    void (*map_set_center_x)(void *, int);
    void (*map_set_center_z)(void *, int);
    void *(*map_get_dimension)(void *);
    void (*map_set_dimension)(void *, void *);
    bool (*map_is_unlimited_tracking)(void *);
    void (*map_set_unlimited_tracking)(void *, bool);
    bool (*map_is_locked)(void *);
    void (*map_set_locked)(void *, bool);
    void (*player_send_map)(void *, void *);
    // renderer_id is a managed-side id; contextual: 0/1. Returns a holder ptr
    // owned by managed (destroy with map_renderer_destroy).
    void *(*map_renderer_create)(int, uint64_t);
    void (*map_renderer_destroy)(void *);
    void (*map_add_renderer)(void *, void *);
    bool (*map_remove_renderer)(void *, void *);
    int (*map_get_renderer_count)(void *);
    // Returns 1 and writes the renderer id when the renderer is dotnet-provided, else 0.
    int (*map_get_renderer)(void *, int, uint64_t *);
    // canvas: cursor record is 5 bytes [x, y, direction, type, visible].
    void *(*canvas_get_map_view)(void *);
    int (*canvas_get_cursor_count)(void *);
    void (*canvas_get_cursor)(void *, int, int8_t *);
    const char *(*canvas_get_cursor_caption)(void *, int);
    void (*canvas_set_cursors)(void *, const int8_t *, int, const char *const *);
    void (*canvas_set_pixel_color)(void *, int, int, int, int, int, int);
    int (*canvas_get_pixel_color)(void *, int, int);
    int (*canvas_get_base_pixel_color)(void *, int, int);
    void (*canvas_set_pixel)(void *, int, int, uint32_t);
    uint32_t (*canvas_get_pixel)(void *, int, int);
    uint32_t (*canvas_get_base_pixel)(void *, int, int);
    // item map meta
    bool (*item_has_map_view)(void *);
    void *(*item_get_map_view)(void *);
    bool (*item_set_map_view)(void *, void *);

    // ---- objects: inventory ----
    void *(*player_get_inventory)(void *);
    void *(*player_get_ender_chest)(void *);
    int (*inventory_get_size)(void *);
    int (*inventory_get_max_stack_size)(void *);
    void *(*inventory_get_item)(void *, int);
    void (*inventory_set_item)(void *, int, void *);
    bool (*inventory_add_item)(void *, void *);
    bool (*inventory_remove_item)(void *, void *);
    bool (*inventory_contains)(void *, void *);
    bool (*inventory_is_empty)(void *);
    int (*inventory_first_empty)(void *);
    void (*inventory_clear)(void *);
    int (*inventory_first)(void *, const char *);
    void *(*inventory_get_item_in_main_hand)(void *);
    void (*inventory_set_item_in_main_hand)(void *, void *);
    void *(*inventory_get_item_in_off_hand)(void *);
    void (*inventory_set_item_in_off_hand)(void *, void *);
    void *(*inventory_get_helmet)(void *);
    void (*inventory_set_helmet)(void *, void *);
    void *(*inventory_get_chestplate)(void *);
    void (*inventory_set_chestplate)(void *, void *);
    void *(*inventory_get_leggings)(void *);
    void (*inventory_set_leggings)(void *, void *);
    void *(*inventory_get_boots)(void *);
    void (*inventory_set_boots)(void *, void *);
    int (*inventory_get_held_item_slot)(void *);
    void (*inventory_set_held_item_slot)(void *, int);

    // ---- plugin registration ----
    void (*plugin_register_event)(void *, const char *, int, bool, void *);
    // Filled by installEventBridge: render(map-view) callback into managed code.
    // (canvas, map, player, renderer_id).
    void (*map_render_callback)(void *, void *, void *, uint64_t);

    // ---- scheduler ----
    void *(*server_get_scheduler)(void *);
    // mode: 0=runTask 1=runTaskLater 2=runTaskTimer 3=runTaskAsync
    //       4=runTaskLaterAsync 5=runTaskTimerAsync
    // managed_id correlates the fire callback; returns the native TaskId (0 on failure).
    uint32_t (*scheduler_run_task)(void *, void *, int, uint64_t, uint64_t, uint64_t);
    void (*scheduler_cancel_task)(void *, uint32_t);
    void (*scheduler_cancel_tasks)(void *, void *);
    bool (*scheduler_is_running)(void *, uint32_t);
    bool (*scheduler_is_queued)(void *, uint32_t);
    int (*scheduler_get_pending_tasks)(void *, void **, int);
    uint32_t (*task_get_id)(void *);
    bool (*task_is_sync)(void *);
    bool (*task_is_cancelled)(void *);
    // Filled by installEventBridge: fires into managed code by managed task id.
    void (*scheduler_task_callback)(uint64_t);
};

}  // namespace dotnet_loader

namespace dotnet_loader {

const BridgeTable &getBridgeTable();
BridgeTable &mutableBridgeTable();

}  // namespace dotnet_loader