#include "bridge.h"

#include <algorithm>
#include <array>
#include <cstring>
#include <format>
#include <iterator>
#include <memory>
#include <optional>
#include <string>
#include <unordered_map>
#include <variant>

#include <endstone/endstone.hpp>

namespace dotnet_loader {

namespace {

// Stable buffer for string returns. Thread-local so it is safe when the
// server invokes events from different threads.
thread_local std::string g_str_buffer;
thread_local std::string g_payload_buffer;
thread_local std::optional<endstone::ItemStack> g_item_slot;
thread_local std::array<std::optional<endstone::ItemStack>, 64> g_item_slots;
thread_local size_t g_item_slot_next = 0;

// Transient snapshot of an optional ItemStack; the managed side treats it as
// read-only and must not outlive the next call on the same thread.
void *itemSnapshot(std::optional<endstone::ItemStack> slot)
{
    if (!slot) {
        return nullptr;
    }
    auto &dst = g_item_slots[g_item_slot_next++ % g_item_slots.size()];
    dst = std::move(slot);
    return &*dst;
}

const char *strOut(std::string s)
{
    g_str_buffer = std::move(s);
    return g_str_buffer.c_str();
}

endstone::Player *asPlayer(void *p) { return static_cast<endstone::Player *>(p); }
endstone::Server *asServer(void *p) { return static_cast<endstone::Server *>(p); }
endstone::Event *asEvent(void *e) { return static_cast<endstone::Event *>(e); }
endstone::ItemStack *asItem(void *i) { return static_cast<endstone::ItemStack *>(i); }
std::unique_ptr<endstone::ItemMeta> itemMeta(void *i) { return asItem(i)->getItemMeta(); }
endstone::Block *asBlock(void *b) { return static_cast<endstone::Block *>(b); }

// optional<Message> -> const char* (only the string alternative is exposed;
// returns nullptr when absent or when the message is a Translatable).
const char *optionalMessage(const std::optional<endstone::Message> &msg)
{
    if (!msg.has_value()) {
        return nullptr;
    }
    if (const auto *s = std::get_if<std::string>(&msg.value())) {
        return strOut(*s);
    }
    return nullptr;
}

// Build a Location from a float[5] buffer, borrowing the dimension from ref.
endstone::Location locationFrom(const float *v, const endstone::Location &ref)
{
    return endstone::Location(ref.getDimension(), v[0], v[1], v[2], v[3], v[4]);
}

// ---- player ----

const char *playerGetName(void *p) { return strOut(asPlayer(p)->getName()); }
void playerSendMessage(void *p, const char *msg) { asPlayer(p)->sendMessage(std::string(msg)); }
void playerSendErrorMessage(void *p, const char *msg) { asPlayer(p)->sendErrorMessage(std::string(msg)); }
void playerSendPopup(void *p, const char *msg) { asPlayer(p)->sendPopup(msg); }
void playerSendTip(void *p, const char *msg) { asPlayer(p)->sendTip(msg); }
void playerSendToast(void *p, const char *title, const char *content) { asPlayer(p)->sendToast(title, content); }
void playerSendTitle(void *p, const char *title, const char *subtitle) { asPlayer(p)->sendTitle(title, subtitle); }
void playerResetTitle(void *p) { asPlayer(p)->resetTitle(); }
void playerKick(void *p, const char *reason) { asPlayer(p)->kick(reason); }
bool playerPerformCommand(void *p, const char *cmd) { return asPlayer(p)->performCommand(cmd); }
bool playerIsOp(void *p) { return asPlayer(p)->isOp(); }
void playerSetOp(void *p, bool v) { asPlayer(p)->setOp(v); }
const char *playerGetXuid(void *p) { return strOut(asPlayer(p)->getXuid()); }
const char *playerGetAddress(void *p)
{
    const auto addr = asPlayer(p)->getAddress();
    return strOut(std::format("{}:{}", addr.getHostname(), addr.getPort()));
}
bool playerIsSneaking(void *p) { return asPlayer(p)->isSneaking(); }
void playerSetSneaking(void *p, bool v) { asPlayer(p)->setSneaking(v); }
bool playerIsSprinting(void *p) { return asPlayer(p)->isSprinting(); }
void playerSetSprinting(void *p, bool v) { asPlayer(p)->setSprinting(v); }
int playerGetPing(void *p) { return static_cast<int>(asPlayer(p)->getPing().count()); }
const char *playerGetLocale(void *p) { return strOut(asPlayer(p)->getLocale()); }
const char *playerGetDeviceOS(void *p) { return strOut(asPlayer(p)->getDeviceOS()); }
const char *playerGetDeviceId(void *p) { return strOut(asPlayer(p)->getDeviceId()); }
const char *playerGetGameVersion(void *p) { return strOut(asPlayer(p)->getGameVersion()); }
int playerGetGameMode(void *p) { return static_cast<int>(asPlayer(p)->getGameMode()); }
void playerSetGameMode(void *p, int m) { asPlayer(p)->setGameMode(static_cast<endstone::GameMode>(m)); }
bool playerGetAllowFlight(void *p) { return asPlayer(p)->getAllowFlight(); }
void playerSetAllowFlight(void *p, bool v) { asPlayer(p)->setAllowFlight(v); }
bool playerIsFlying(void *p) { return asPlayer(p)->isFlying(); }
void playerSetFlying(void *p, bool v) { asPlayer(p)->setFlying(v); }
int playerGetExpLevel(void *p) { return asPlayer(p)->getExpLevel(); }
void playerSetExpLevel(void *p, int v) { asPlayer(p)->setExpLevel(v); }
void playerGiveExp(void *p, int v) { asPlayer(p)->giveExp(v); }
void playerGiveExpLevels(void *p, int v) { asPlayer(p)->giveExpLevels(v); }
float playerGetExpProgress(void *p) { return asPlayer(p)->getExpProgress(); }
void playerSetExpProgress(void *p, float v) { asPlayer(p)->setExpProgress(v); }
int playerGetTotalExp(void *p) { return asPlayer(p)->getTotalExp(); }

void playerTransfer(void *p, const char *host, int port)
{
    asPlayer(p)->transfer(host, port);
}
void playerPlaySound(void *p, const float *loc, const char *sound, float volume, float pitch)
{
    auto *player = asPlayer(p);
    const endstone::Location location(player->getDimension(), loc[0], loc[1], loc[2], loc[3], loc[4]);
    player->playSound(location, sound, volume, pitch);
}
void playerStopSound(void *p, const char *sound) { asPlayer(p)->stopSound(sound); }
void playerStopAllSounds(void *p) { asPlayer(p)->stopAllSounds(); }
void playerSpawnParticle(void *p, const char *name, const float *loc, const char *molang)
{
    auto *player = asPlayer(p);
    const endstone::Location location(player->getDimension(), loc[0], loc[1], loc[2], loc[3], loc[4]);
    if (molang && *molang) {
        player->spawnParticle(name, location, std::string(molang));
    }
    else {
        player->spawnParticle(name, location);
    }
}
float playerGetFlySpeed(void *p) { return asPlayer(p)->getFlySpeed(); }
void playerSetFlySpeed(void *p, float v) { asPlayer(p)->setFlySpeed(v); }
float playerGetWalkSpeed(void *p) { return asPlayer(p)->getWalkSpeed(); }
void playerSetWalkSpeed(void *p, float v) { asPlayer(p)->setWalkSpeed(v); }
void playerUpdateCommands(void *p) { asPlayer(p)->updateCommands(); }
void playerCloseForm(void *p) { asPlayer(p)->closeForm(); }
void playerSendPacket(void *p, int packet_id, const void *payload, int len)
{
    asPlayer(p)->sendPacket(packet_id, std::string_view(static_cast<const char *>(payload), static_cast<size_t>(len)));
}
const char *playerGetSkinId(void *p) { return strOut(asPlayer(p)->getSkin().getId()); }
const char *playerGetSkinCapeId(void *p)
{
    const auto cape = asPlayer(p)->getSkin().getCapeId();
    return cape ? strOut(cape.value()) : nullptr;
}
void *playerGetItemInHand(void *p)
{
    g_item_slot = asPlayer(p)->getInventory().getItemInMainHand();
    return g_item_slot.has_value() ? &*g_item_slot : nullptr;
}

// ---- server ----

const char *serverGetName(void *p) { return strOut(asServer(p)->getName()); }
const char *serverGetVersion(void *p) { return strOut(asServer(p)->getVersion()); }
const char *serverGetMinecraftVersion(void *p) { return strOut(asServer(p)->getMinecraftVersion()); }
int serverGetProtocolVersion(void *p) { return asServer(p)->getProtocolVersion(); }
int serverGetMaxPlayers(void *p) { return asServer(p)->getMaxPlayers(); }
void serverBroadcastMessage(void *p, const char *msg) { asServer(p)->broadcastMessage(std::string(msg)); }
int serverGetOnlinePlayers(void *p, void **out, int cap)
{
    const auto players = asServer(p)->getOnlinePlayers();
    const auto n = static_cast<int>(players.size());
    if (out && cap > 0) {
        const auto copy = std::min(n, cap);
        for (int i = 0; i < copy; ++i) {
            out[i] = players[static_cast<size_t>(i)];
        }
    }
    return n;
}
void *serverGetConsoleSender(void *p) { return &asServer(p)->getCommandSender(); }
void *serverGetPlayer(void *p, const char *name)
{
    return asServer(p)->getPlayer(name ? name : "");
}
bool serverDispatchCommand(void *p, void *sender, const char *cmd)
{
    return asServer(p)->dispatchCommand(*static_cast<endstone::CommandSender *>(sender), cmd);
}

// ---- events ----
//
// The managed side always knows the exact event type (the server dispatches
// handlers by event name, 1:1 with the concrete class), so multi-type
// accessors classify the event by kind (the managed side maps the name to an
// EventKind exactly once per event instance) and static_cast from the
// original object pointer. dynamic_cast is deliberately avoided: typeinfo
// objects are not shared across DSO boundaries on Linux (both the server and
// plugins are built with hidden visibility), which makes cross-module RTTI
// checks fail. All event classes are single-inheritance chains rooted at
// Event at offset 0, so casting the object pointer back to the concrete type
// is always identity.

void *eventGetPlayer(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::PlayerBedEnterEvent:
    case EventKind::PlayerCommandEvent:
    case EventKind::PlayerDimensionChangeEvent:
    case EventKind::PlayerEmoteEvent:
    case EventKind::PlayerGameModeChangeEvent:
    case EventKind::PlayerInteractActorEvent:
    case EventKind::PlayerInteractEvent:
    case EventKind::PlayerJoinEvent:
    case EventKind::PlayerJumpEvent:
    case EventKind::PlayerKickEvent:
    case EventKind::PlayerLoginEvent:
    case EventKind::PlayerMoveEvent:
    case EventKind::PlayerPickupItemEvent:
    case EventKind::PlayerPortalEvent:
    case EventKind::PlayerQuitEvent:
    case EventKind::PlayerRespawnEvent:
    case EventKind::PlayerSkinChangeEvent:
    case EventKind::PlayerTeleportEvent:
    case EventKind::PlayerChatEvent:
    case EventKind::PlayerDropItemEvent:
    case EventKind::PlayerItemHeldEvent:
    case EventKind::PlayerItemConsumeEvent:
        return &static_cast<endstone::PlayerEvent *>(e)->getPlayer();
    case EventKind::PlayerDeathEvent:
        return &static_cast<endstone::PlayerDeathEvent *>(e)->getPlayer();
    case EventKind::ActorDeathEvent:
        return static_cast<endstone::ActorDeathEvent *>(e)->getActor().asPlayer();
    case EventKind::BlockBreakEvent:
        return &static_cast<endstone::BlockBreakEvent *>(e)->getPlayer();
    case EventKind::BlockPlaceEvent:
        return &static_cast<endstone::BlockPlaceEvent *>(e)->getPlayer();
    case EventKind::PacketReceiveEvent:
        return static_cast<endstone::PacketReceiveEvent *>(e)->getPlayer();
    case EventKind::PacketSendEvent:
        return static_cast<endstone::PacketSendEvent *>(e)->getPlayer();
    default:
        return nullptr;
    }
}

void *eventGetActor(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::ActorExplodeEvent:
    case EventKind::ActorRemoveEvent:
    case EventKind::ActorSpawnEvent:
    case EventKind::ActorTeleportEvent:
        return &static_cast<endstone::ActorEvent<endstone::Actor> *>(e)->getActor();
    case EventKind::ActorDamageEvent:
    case EventKind::ActorDeathEvent:
    case EventKind::PlayerDeathEvent:
    case EventKind::ActorKnockbackEvent:
        return &static_cast<endstone::ActorEvent<endstone::Mob> *>(e)->getActor();
    case EventKind::PlayerInteractActorEvent:
        return &static_cast<endstone::PlayerInteractActorEvent *>(e)->getActor();
    default:
        return nullptr;
    }
}

bool eventIsCancelled(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
#define ENDSTONE_CASE_IS_CANCELLED(T) \
    case EventKind::T: return static_cast<endstone::T *>(e)->isCancelled();
    ENDSTONE_CASE_IS_CANCELLED(ActorDamageEvent)
    ENDSTONE_CASE_IS_CANCELLED(ActorExplodeEvent)
    ENDSTONE_CASE_IS_CANCELLED(ActorKnockbackEvent)
    ENDSTONE_CASE_IS_CANCELLED(ActorSpawnEvent)
    ENDSTONE_CASE_IS_CANCELLED(ActorTeleportEvent)
    ENDSTONE_CASE_IS_CANCELLED(BlockBreakEvent)
    ENDSTONE_CASE_IS_CANCELLED(BlockCookEvent)
    ENDSTONE_CASE_IS_CANCELLED(BlockExplodeEvent)
    ENDSTONE_CASE_IS_CANCELLED(BlockFromToEvent)
    ENDSTONE_CASE_IS_CANCELLED(BlockGrowEvent)
    ENDSTONE_CASE_IS_CANCELLED(BlockPistonExtendEvent)
    ENDSTONE_CASE_IS_CANCELLED(BlockPistonRetractEvent)
    ENDSTONE_CASE_IS_CANCELLED(BlockPlaceEvent)
    ENDSTONE_CASE_IS_CANCELLED(LeavesDecayEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerBedEnterEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerChatEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerCommandEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerDropItemEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerEmoteEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerGameModeChangeEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerInteractActorEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerInteractEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerItemConsumeEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerItemHeldEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerJumpEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerKickEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerLoginEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerMoveEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerPickupItemEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerPortalEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerSkinChangeEvent)
    ENDSTONE_CASE_IS_CANCELLED(PlayerTeleportEvent)
    ENDSTONE_CASE_IS_CANCELLED(BroadcastMessageEvent)
    ENDSTONE_CASE_IS_CANCELLED(PacketReceiveEvent)
    ENDSTONE_CASE_IS_CANCELLED(PacketSendEvent)
    ENDSTONE_CASE_IS_CANCELLED(ScriptMessageEvent)
    ENDSTONE_CASE_IS_CANCELLED(ServerCommandEvent)
    ENDSTONE_CASE_IS_CANCELLED(ServerListPingEvent)
    ENDSTONE_CASE_IS_CANCELLED(ThunderChangeEvent)
    ENDSTONE_CASE_IS_CANCELLED(WeatherChangeEvent)
#undef ENDSTONE_CASE_IS_CANCELLED
    default:
        return false;
    }
}

void eventSetCancelled(void *e, int kind, bool v)
{
    switch (static_cast<EventKind>(kind)) {
#define ENDSTONE_CASE_SET_CANCELLED(T) \
    case EventKind::T: static_cast<endstone::T *>(e)->setCancelled(v); return;
    ENDSTONE_CASE_SET_CANCELLED(ActorDamageEvent)
    ENDSTONE_CASE_SET_CANCELLED(ActorExplodeEvent)
    ENDSTONE_CASE_SET_CANCELLED(ActorKnockbackEvent)
    ENDSTONE_CASE_SET_CANCELLED(ActorSpawnEvent)
    ENDSTONE_CASE_SET_CANCELLED(ActorTeleportEvent)
    ENDSTONE_CASE_SET_CANCELLED(BlockBreakEvent)
    ENDSTONE_CASE_SET_CANCELLED(BlockCookEvent)
    ENDSTONE_CASE_SET_CANCELLED(BlockExplodeEvent)
    ENDSTONE_CASE_SET_CANCELLED(BlockFromToEvent)
    ENDSTONE_CASE_SET_CANCELLED(BlockGrowEvent)
    ENDSTONE_CASE_SET_CANCELLED(BlockPistonExtendEvent)
    ENDSTONE_CASE_SET_CANCELLED(BlockPistonRetractEvent)
    ENDSTONE_CASE_SET_CANCELLED(BlockPlaceEvent)
    ENDSTONE_CASE_SET_CANCELLED(LeavesDecayEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerBedEnterEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerChatEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerCommandEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerDropItemEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerEmoteEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerGameModeChangeEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerInteractActorEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerInteractEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerItemConsumeEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerItemHeldEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerJumpEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerKickEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerLoginEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerMoveEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerPickupItemEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerPortalEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerSkinChangeEvent)
    ENDSTONE_CASE_SET_CANCELLED(PlayerTeleportEvent)
    ENDSTONE_CASE_SET_CANCELLED(BroadcastMessageEvent)
    ENDSTONE_CASE_SET_CANCELLED(PacketReceiveEvent)
    ENDSTONE_CASE_SET_CANCELLED(PacketSendEvent)
    ENDSTONE_CASE_SET_CANCELLED(ScriptMessageEvent)
    ENDSTONE_CASE_SET_CANCELLED(ServerCommandEvent)
    ENDSTONE_CASE_SET_CANCELLED(ServerListPingEvent)
    ENDSTONE_CASE_SET_CANCELLED(ThunderChangeEvent)
    ENDSTONE_CASE_SET_CANCELLED(WeatherChangeEvent)
#undef ENDSTONE_CASE_SET_CANCELLED
    default:
        return;
    }
}

const char *chatGetMessage(void *e)
{
    return strOut(static_cast<endstone::PlayerChatEvent *>(e)->getMessage());
}
void chatSetMessage(void *e, const char *msg) { static_cast<endstone::PlayerChatEvent *>(e)->setMessage(msg); }
const char *chatGetFormat(void *e)
{
    return strOut(static_cast<endstone::PlayerChatEvent *>(e)->getFormat());
}
void chatSetFormat(void *e, const char *fmt) { static_cast<endstone::PlayerChatEvent *>(e)->setFormat(fmt); }
int chatGetRecipientCount(void *e)
{
    return static_cast<int>(static_cast<endstone::PlayerChatEvent *>(e)->getRecipients().size());
}
const char *commandGetCommand(void *e)
{
    return strOut(static_cast<endstone::PlayerCommandEvent *>(e)->getCommand());
}
void commandSetCommand(void *e, const char *cmd)
{
    static_cast<endstone::PlayerCommandEvent *>(e)->setCommand(cmd);
}

const char *serverCmdGetCommand(void *e)
{
    return strOut(static_cast<endstone::ServerCommandEvent *>(e)->getCommand());
}
void serverCmdSetCommand(void *e, const char *cmd)
{
    static_cast<endstone::ServerCommandEvent *>(e)->setCommand(cmd);
}
const char *serverCmdGetSenderName(void *e)
{
    return strOut(static_cast<endstone::ServerCommandEvent *>(e)->getSender().getName());
}
void *serverCmdGetSender(void *e)
{
    return &static_cast<endstone::ServerCommandEvent *>(e)->getSender();
}

void moveGetFrom(void *e, float *out)
{
    const auto &loc = static_cast<endstone::PlayerMoveEvent *>(e)->getFrom();
    out[0] = loc.getX();
    out[1] = loc.getY();
    out[2] = loc.getZ();
    out[3] = loc.getPitch();
    out[4] = loc.getYaw();
}
void moveGetTo(void *e, float *out)
{
    const auto &loc = static_cast<endstone::PlayerMoveEvent *>(e)->getTo();
    out[0] = loc.getX();
    out[1] = loc.getY();
    out[2] = loc.getZ();
    out[3] = loc.getPitch();
    out[4] = loc.getYaw();
}
void moveSetFrom(void *e, const float *v)
{
    auto *ev = static_cast<endstone::PlayerMoveEvent *>(e);
    ev->setFrom(locationFrom(v, ev->getFrom()));
}
void moveSetTo(void *e, const float *v)
{
    auto *ev = static_cast<endstone::PlayerMoveEvent *>(e);
    ev->setTo(locationFrom(v, ev->getTo()));
}

void actorTpGetFrom(void *e, float *out)
{
    const auto &loc = static_cast<endstone::ActorTeleportEvent *>(e)->getFrom();
    out[0] = loc.getX();
    out[1] = loc.getY();
    out[2] = loc.getZ();
    out[3] = loc.getPitch();
    out[4] = loc.getYaw();
}
void actorTpGetTo(void *e, float *out)
{
    const auto &loc = static_cast<endstone::ActorTeleportEvent *>(e)->getTo();
    out[0] = loc.getX();
    out[1] = loc.getY();
    out[2] = loc.getZ();
    out[3] = loc.getPitch();
    out[4] = loc.getYaw();
}
void actorTpSetFrom(void *e, const float *v)
{
    auto *ev = static_cast<endstone::ActorTeleportEvent *>(e);
    ev->setFrom(locationFrom(v, ev->getFrom()));
}
void actorTpSetTo(void *e, const float *v)
{
    auto *ev = static_cast<endstone::ActorTeleportEvent *>(e);
    ev->setTo(locationFrom(v, ev->getTo()));
}

int interactGetAction(void *e)
{
    return static_cast<int>(static_cast<endstone::PlayerInteractEvent *>(e)->getAction());
}
int interactGetClickedPosition(void *e, float *out)
{
    const auto &pos = static_cast<endstone::PlayerInteractEvent *>(e)->getClickedPosition();
    if (!pos.has_value()) {
        return 0;
    }
    out[0] = pos->getX();
    out[1] = pos->getY();
    out[2] = pos->getZ();
    return 1;
}
bool interactHasItem(void *e) { return static_cast<endstone::PlayerInteractEvent *>(e)->hasItem(); }
void *interactGetItem(void *e)
{
    auto &item = static_cast<endstone::PlayerInteractEvent *>(e)->getItem();
    return item.has_value() ? const_cast<endstone::ItemStack *>(&item.value()) : nullptr;
}
bool interactHasBlock(void *e) { return static_cast<endstone::PlayerInteractEvent *>(e)->hasBlock(); }
void *interactGetBlock(void *e)
{
    return static_cast<endstone::PlayerInteractEvent *>(e)->getBlock();
}
int interactGetBlockFace(void *e)
{
    return static_cast<int>(static_cast<endstone::PlayerInteractEvent *>(e)->getBlockFace());
}
void *interactActorGetActor(void *e)
{
    return &static_cast<endstone::PlayerInteractActorEvent *>(e)->getActor();
}

float actorDamageGetDamage(void *e) { return static_cast<endstone::ActorDamageEvent *>(e)->getDamage(); }
void actorDamageSetDamage(void *e, float v) { static_cast<endstone::ActorDamageEvent *>(e)->setDamage(v); }
void *eventGetDamageSource(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::ActorDamageEvent:
        return &static_cast<endstone::ActorDamageEvent *>(e)->getDamageSource();
    case EventKind::ActorDeathEvent:
    case EventKind::PlayerDeathEvent:
        return &static_cast<endstone::ActorDeathEvent *>(e)->getDamageSource();
    default:
        return nullptr;
    }
}
void actorExplodeGetLocation(void *e, float *out)
{
    const auto &loc = static_cast<endstone::ActorExplodeEvent *>(e)->getLocation();
    out[0] = loc.getX();
    out[1] = loc.getY();
    out[2] = loc.getZ();
    out[3] = loc.getPitch();
    out[4] = loc.getYaw();
}
int actorExplodeGetBlockCount(void *e)
{
    return static_cast<int>(static_cast<endstone::ActorExplodeEvent *>(e)->getBlockList().size());
}
void *actorExplodeGetBlock(void *e, int idx)
{
    auto &list = static_cast<endstone::ActorExplodeEvent *>(e)->getBlockList();
    return (idx >= 0 && static_cast<size_t>(idx) < list.size()) ? list[static_cast<size_t>(idx)].get() : nullptr;
}
void *actorKnockbackGetSource(void *e)
{
    return static_cast<endstone::ActorKnockbackEvent *>(e)->getSource();
}
void actorKnockbackGetVector(void *e, float *out)
{
    const auto &v = static_cast<endstone::ActorKnockbackEvent *>(e)->getKnockback();
    out[0] = v.getX();
    out[1] = v.getY();
    out[2] = v.getZ();
}
void actorKnockbackSetVector(void *e, const float *v)
{
    static_cast<endstone::ActorKnockbackEvent *>(e)->setKnockback({v[0], v[1], v[2]});
}

const char *deathGetMessage(void *e)
{
    return optionalMessage(static_cast<endstone::PlayerDeathEvent *>(e)->getDeathMessage());
}
void deathSetMessage(void *e, const char *msg)
{
    static_cast<endstone::PlayerDeathEvent *>(e)->setDeathMessage(std::string(msg));
}
void *bedGetBed(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::PlayerBedEnterEvent:
        return &static_cast<endstone::PlayerBedEnterEvent *>(e)->getBed();
    case EventKind::PlayerBedLeaveEvent:
        return &static_cast<endstone::PlayerBedLeaveEvent *>(e)->getBed();
    default:
        return nullptr;
    }
}
const char *dimChangeGetFrom(void *e)
{
    return strOut(static_cast<endstone::PlayerDimensionChangeEvent *>(e)->getFrom().getName());
}
const char *dimChangeGetTo(void *e)
{
    return strOut(static_cast<endstone::PlayerDimensionChangeEvent *>(e)->getTo().getName());
}
void *dropGetItem(void *e) { return const_cast<endstone::ItemStack *>(&static_cast<endstone::PlayerDropItemEvent *>(e)->getItem()); }
const char *emoteGetId(void *e) { return strOut(static_cast<endstone::PlayerEmoteEvent *>(e)->getEmoteId()); }
bool emoteIsMuted(void *e) { return static_cast<endstone::PlayerEmoteEvent *>(e)->isMuted(); }
void emoteSetMuted(void *e, bool v) { static_cast<endstone::PlayerEmoteEvent *>(e)->setMuted(v); }
int gmChangeGetNewMode(void *e) { return static_cast<int>(static_cast<endstone::PlayerGameModeChangeEvent *>(e)->getNewGameMode()); }
void *consumeGetItem(void *e) { return const_cast<endstone::ItemStack *>(&static_cast<endstone::PlayerItemConsumeEvent *>(e)->getItem()); }
int consumeGetHand(void *e) { return static_cast<int>(static_cast<endstone::PlayerItemConsumeEvent *>(e)->getHand()); }
int heldGetPreviousSlot(void *e) { return static_cast<endstone::PlayerItemHeldEvent *>(e)->getPreviousSlot(); }
int heldGetNewSlot(void *e) { return static_cast<endstone::PlayerItemHeldEvent *>(e)->getNewSlot(); }
const char *joinGetMessage(void *e)
{
    return optionalMessage(static_cast<endstone::PlayerJoinEvent *>(e)->getJoinMessage());
}
void joinSetMessage(void *e, const char *msg)
{
    static_cast<endstone::PlayerJoinEvent *>(e)->setJoinMessage(std::string(msg));
}
const char *quitGetMessage(void *e)
{
    return optionalMessage(static_cast<endstone::PlayerQuitEvent *>(e)->getQuitMessage());
}
void quitSetMessage(void *e, const char *msg)
{
    static_cast<endstone::PlayerQuitEvent *>(e)->setQuitMessage(std::string(msg));
}
const char *kickGetReason(void *e) { return strOut(static_cast<endstone::PlayerKickEvent *>(e)->getReason()); }
void kickSetReason(void *e, const char *r) { static_cast<endstone::PlayerKickEvent *>(e)->setReason(r); }
const char *loginGetKickMessage(void *e)
{
    return strOut(static_cast<endstone::PlayerLoginEvent *>(e)->getKickMessage());
}
void loginSetKickMessage(void *e, const char *m) { static_cast<endstone::PlayerLoginEvent *>(e)->setKickMessage(m); }
void *pickupGetItem(void *e) { return &static_cast<endstone::PlayerPickupItemEvent *>(e)->getItem(); }
const char *skinChangeGetNewSkinId(void *e)
{
    return strOut(static_cast<endstone::PlayerSkinChangeEvent *>(e)->getNewSkin().getId());
}
const char *skinChangeGetNewSkinCapeId(void *e)
{
    const auto cape = static_cast<endstone::PlayerSkinChangeEvent *>(e)->getNewSkin().getCapeId();
    return cape ? strOut(cape.value()) : nullptr;
}
const char *skinChangeGetMessage(void *e)
{
    return optionalMessage(static_cast<endstone::PlayerSkinChangeEvent *>(e)->getSkinChangeMessage());
}
void skinChangeSetMessage(void *e, const char *msg)
{
    static_cast<endstone::PlayerSkinChangeEvent *>(e)->setSkinChangeMessage(std::string(msg));
}

void *cookGetSource(void *e) { return const_cast<endstone::ItemStack *>(&static_cast<endstone::BlockCookEvent *>(e)->getSource()); }
void *cookGetResult(void *e) { return const_cast<endstone::ItemStack *>(&static_cast<endstone::BlockCookEvent *>(e)->getResult()); }
int blockExplodeGetBlockCount(void *e)
{
    return static_cast<int>(static_cast<endstone::BlockExplodeEvent *>(e)->getBlockList().size());
}
void *blockExplodeGetBlock(void *e, int idx)
{
    auto &list = static_cast<endstone::BlockExplodeEvent *>(e)->getBlockList();
    return (idx >= 0 && static_cast<size_t>(idx) < list.size()) ? list[static_cast<size_t>(idx)].get() : nullptr;
}
void *growGetNewState(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::BlockGrowEvent:
        return &static_cast<endstone::BlockGrowEvent *>(e)->getNewState();
    case EventKind::BlockFormEvent:
        return &static_cast<endstone::BlockFormEvent *>(e)->getNewState();
    default:
        return nullptr;
    }
}
void *fromToGetToBlock(void *e) { return &static_cast<endstone::BlockFromToEvent *>(e)->getToBlock(); }
int pistonGetDirection(void *e) { return static_cast<int>(static_cast<endstone::BlockPistonEvent *>(e)->getDirection()); }
void *placeGetPlacedState(void *e)
{
    return &static_cast<endstone::BlockPlaceEvent *>(e)->getBlockPlacedState();
}
void *placeGetAgainst(void *e) { return &static_cast<endstone::BlockPlaceEvent *>(e)->getBlockAgainst(); }

int chunkGetX(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::ChunkLoadEvent:
        return static_cast<endstone::ChunkLoadEvent *>(e)->getChunk().getX();
    case EventKind::ChunkUnloadEvent:
        return static_cast<endstone::ChunkUnloadEvent *>(e)->getChunk().getX();
    default:
        return 0;
    }
}
int chunkGetZ(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::ChunkLoadEvent:
        return static_cast<endstone::ChunkLoadEvent *>(e)->getChunk().getZ();
    case EventKind::ChunkUnloadEvent:
        return static_cast<endstone::ChunkUnloadEvent *>(e)->getChunk().getZ();
    default:
        return 0;
    }
}
const char *chunkGetDimensionName(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::ChunkLoadEvent:
        return strOut(static_cast<endstone::ChunkLoadEvent *>(e)->getChunk().getDimension().getName());
    case EventKind::ChunkUnloadEvent:
        return strOut(static_cast<endstone::ChunkUnloadEvent *>(e)->getChunk().getDimension().getName());
    default:
        return nullptr;
    }
}

const char *broadcastGetMessage(void *e)
{
    return optionalMessage(static_cast<endstone::BroadcastMessageEvent *>(e)->getMessage());
}
void broadcastSetMessage(void *e, const char *msg)
{
    static_cast<endstone::BroadcastMessageEvent *>(e)->setMessage(std::string(msg));
}
int broadcastGetRecipientCount(void *e)
{
    return static_cast<int>(static_cast<endstone::BroadcastMessageEvent *>(e)->getRecipients().size());
}
int packetGetId(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::PacketReceiveEvent:
        return static_cast<endstone::PacketReceiveEvent *>(e)->getPacketId();
    case EventKind::PacketSendEvent:
        return static_cast<endstone::PacketSendEvent *>(e)->getPacketId();
    default:
        return 0;
    }
}
const char *packetGetPayload(void *e, int kind, int *len)
{
    std::string_view payload;
    switch (static_cast<EventKind>(kind)) {
    case EventKind::PacketReceiveEvent:
        payload = static_cast<endstone::PacketReceiveEvent *>(e)->getPayload();
        break;
    case EventKind::PacketSendEvent:
        payload = static_cast<endstone::PacketSendEvent *>(e)->getPayload();
        break;
    default:
        break;
    }
    g_payload_buffer.assign(payload.data(), payload.size());
    if (len) {
        *len = static_cast<int>(payload.size());
    }
    return g_payload_buffer.c_str();
}
void packetSetPayload(void *e, int kind, const void *data, int len)
{
    const auto payload = std::string_view(static_cast<const char *>(data), static_cast<size_t>(len));
    switch (static_cast<EventKind>(kind)) {
    case EventKind::PacketReceiveEvent:
        static_cast<endstone::PacketReceiveEvent *>(e)->setPayload(payload);
        break;
    case EventKind::PacketSendEvent:
        static_cast<endstone::PacketSendEvent *>(e)->setPayload(payload);
        break;
    default:
        break;
    }
}
void *packetGetPlayer(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::PacketReceiveEvent:
        return static_cast<endstone::PacketReceiveEvent *>(e)->getPlayer();
    case EventKind::PacketSendEvent:
        return static_cast<endstone::PacketSendEvent *>(e)->getPlayer();
    default:
        return nullptr;
    }
}
const char *packetGetAddress(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::PacketReceiveEvent: {
        const auto addr = static_cast<endstone::PacketReceiveEvent *>(e)->getAddress();
        return strOut(std::format("{}:{}", addr.getHostname(), addr.getPort()));
    }
    case EventKind::PacketSendEvent: {
        const auto addr = static_cast<endstone::PacketSendEvent *>(e)->getAddress();
        return strOut(std::format("{}:{}", addr.getHostname(), addr.getPort()));
    }
    default:
        return nullptr;
    }
}
int packetGetSubClientId(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::PacketReceiveEvent:
        return static_cast<endstone::PacketReceiveEvent *>(e)->getSubClientId();
    case EventKind::PacketSendEvent:
        return static_cast<endstone::PacketSendEvent *>(e)->getSubClientId();
    default:
        return 0;
    }
}
const char *pluginEventGetPluginName(void *e, int kind)
{
    switch (static_cast<EventKind>(kind)) {
    case EventKind::PluginEnableEvent:
        return strOut(static_cast<endstone::PluginEnableEvent *>(e)->getPlugin().getName());
    case EventKind::PluginDisableEvent:
        return strOut(static_cast<endstone::PluginDisableEvent *>(e)->getPlugin().getName());
    default:
        return nullptr;
    }
}
const char *scriptGetMessageId(void *e)
{
    return strOut(static_cast<endstone::ScriptMessageEvent *>(e)->getMessageId());
}
const char *scriptGetMessage(void *e)
{
    return strOut(static_cast<endstone::ScriptMessageEvent *>(e)->getMessage());
}
const char *scriptGetSenderName(void *e)
{
    return strOut(static_cast<endstone::ScriptMessageEvent *>(e)->getSender().getName());
}
const char *pingGetAddress(void *e)
{
    const auto addr = static_cast<endstone::ServerListPingEvent *>(e)->getAddress();
    return strOut(std::format("{}:{}", addr.getHostname(), addr.getPort()));
}
const char *pingGetServerGuid(void *e)
{
    return strOut(static_cast<endstone::ServerListPingEvent *>(e)->getServerGuid());
}
void pingSetServerGuid(void *e, const char *v) { static_cast<endstone::ServerListPingEvent *>(e)->setServerGuid(v); }
int pingGetLocalPort(void *e) { return static_cast<endstone::ServerListPingEvent *>(e)->getLocalPort(); }
void pingSetLocalPort(void *e, int v) { static_cast<endstone::ServerListPingEvent *>(e)->setLocalPort(v); }
int pingGetLocalPortV6(void *e) { return static_cast<endstone::ServerListPingEvent *>(e)->getLocalPortV6(); }
void pingSetLocalPortV6(void *e, int v) { static_cast<endstone::ServerListPingEvent *>(e)->setLocalPortV6(v); }
const char *pingGetMotd(void *e) { return strOut(static_cast<endstone::ServerListPingEvent *>(e)->getMotd()); }
void pingSetMotd(void *e, const char *v) { static_cast<endstone::ServerListPingEvent *>(e)->setMotd(v); }
int pingGetNetworkProtocolVersion(void *e)
{
    return static_cast<endstone::ServerListPingEvent *>(e)->getNetworkProtocolVersion();
}
const char *pingGetMinecraftVersionNetwork(void *e)
{
    return strOut(static_cast<endstone::ServerListPingEvent *>(e)->getMinecraftVersionNetwork());
}
void pingSetMinecraftVersionNetwork(void *e, const char *v)
{
    static_cast<endstone::ServerListPingEvent *>(e)->setMinecraftVersionNetwork(v);
}
int pingGetNumPlayers(void *e) { return static_cast<endstone::ServerListPingEvent *>(e)->getNumPlayers(); }
void pingSetNumPlayers(void *e, int v) { static_cast<endstone::ServerListPingEvent *>(e)->setNumPlayers(v); }
int pingGetMaxPlayers(void *e) { return static_cast<endstone::ServerListPingEvent *>(e)->getMaxPlayers(); }
void pingSetMaxPlayers(void *e, int v) { static_cast<endstone::ServerListPingEvent *>(e)->setMaxPlayers(v); }
const char *pingGetLevelName(void *e) { return strOut(static_cast<endstone::ServerListPingEvent *>(e)->getLevelName()); }
void pingSetLevelName(void *e, const char *v) { static_cast<endstone::ServerListPingEvent *>(e)->setLevelName(v); }
int pingGetGameMode(void *e) { return static_cast<int>(static_cast<endstone::ServerListPingEvent *>(e)->getGameMode()); }
void pingSetGameMode(void *e, int v)
{
    static_cast<endstone::ServerListPingEvent *>(e)->setGameMode(static_cast<endstone::GameMode>(v));
}
int serverLoadGetType(void *e) { return static_cast<int>(static_cast<endstone::ServerLoadEvent *>(e)->getType()); }
bool thunderChangeGetTo(void *e) { return static_cast<endstone::ThunderChangeEvent *>(e)->toThunderState(); }
bool weatherChangeGetTo(void *e) { return static_cast<endstone::WeatherChangeEvent *>(e)->toWeatherState(); }

// ---- objects ----

const char *actorGetType(void *a) { return strOut(static_cast<endstone::Actor *>(a)->getType()); }
uint64_t actorGetRuntimeId(void *a) { return static_cast<endstone::Actor *>(a)->getRuntimeId(); }
void actorGetLocation(void *a, float *out)
{
    const auto &loc = static_cast<endstone::Actor *>(a)->getLocation();
    out[0] = loc.getX();
    out[1] = loc.getY();
    out[2] = loc.getZ();
    out[3] = loc.getPitch();
    out[4] = loc.getYaw();
}
void actorGetVelocity(void *a, float *out)
{
    const auto &v = static_cast<endstone::Actor *>(a)->getVelocity();
    out[0] = v.getX();
    out[1] = v.getY();
    out[2] = v.getZ();
}
bool actorIsOnGround(void *a) { return static_cast<endstone::Actor *>(a)->isOnGround(); }
bool actorIsInWater(void *a) { return static_cast<endstone::Actor *>(a)->isInWater(); }
bool actorIsInLava(void *a) { return static_cast<endstone::Actor *>(a)->isInLava(); }
bool actorIsDead(void *a) { return static_cast<endstone::Actor *>(a)->isDead(); }
bool actorIsValid(void *a) { return static_cast<endstone::Actor *>(a)->isValid(); }
const char *actorGetDimensionName(void *a) { return strOut(static_cast<endstone::Actor *>(a)->getDimension().getName()); }
const char *actorGetNameTag(void *a) { return strOut(static_cast<endstone::Actor *>(a)->getNameTag()); }
const char *actorGetScoreTag(void *a) { return strOut(static_cast<endstone::Actor *>(a)->getScoreTag()); }
int64_t actorGetId(void *a) { return static_cast<endstone::Actor *>(a)->getId(); }
void actorSetRotation(void *a, float yaw, float pitch) { static_cast<endstone::Actor *>(a)->setRotation(yaw, pitch); }
bool actorTeleportLocation(void *a, const float *v)
{
    const auto &ref = static_cast<endstone::Actor *>(a)->getLocation();
    return static_cast<endstone::Actor *>(a)->teleport(locationFrom(v, ref));
}
bool actorTeleportActor(void *a, void *target)
{
    if (!target) {
        return false;
    }
    return static_cast<endstone::Actor *>(a)->teleport(*static_cast<endstone::Actor *>(target));
}
void actorRemove(void *a) { static_cast<endstone::Actor *>(a)->remove(); }
void actorSendMessage(void *a, const char *msg)
{
    static_cast<endstone::Actor *>(a)->sendMessage(std::string(msg ? msg : ""));
}
const char *actorGetName(void *a) { return strOut(static_cast<endstone::Actor *>(a)->getName()); }
int actorGetScoreboardTagCount(void *a)
{
    return static_cast<int>(static_cast<endstone::Actor *>(a)->getScoreboardTags().size());
}
const char *actorGetScoreboardTag(void *a, int index)
{
    const auto &tags = static_cast<endstone::Actor *>(a)->getScoreboardTags();
    if (index >= 0 && index < static_cast<int>(tags.size())) {
        return strOut(tags[index]);
    }
    return nullptr;
}
bool actorAddScoreboardTag(void *a, const char *tag)
{
    return static_cast<endstone::Actor *>(a)->addScoreboardTag(tag ? tag : "");
}
bool actorRemoveScoreboardTag(void *a, const char *tag)
{
    return static_cast<endstone::Actor *>(a)->removeScoreboardTag(tag ? tag : "");
}
bool actorIsNameTagVisible(void *a) { return static_cast<endstone::Actor *>(a)->isNameTagVisible(); }
void actorSetNameTagVisible(void *a, bool v) { static_cast<endstone::Actor *>(a)->setNameTagVisible(v); }
bool actorIsNameTagAlwaysVisible(void *a) { return static_cast<endstone::Actor *>(a)->isNameTagAlwaysVisible(); }
void actorSetNameTagAlwaysVisible(void *a, bool v) { static_cast<endstone::Actor *>(a)->setNameTagAlwaysVisible(v); }
void actorSetNameTag(void *a, const char *v) { static_cast<endstone::Actor *>(a)->setNameTag(v ? v : ""); }
void actorSetScoreTag(void *a, const char *v) { static_cast<endstone::Actor *>(a)->setScoreTag(v ? v : ""); }
int mobGetHealth(void *m) { return static_cast<endstone::Mob *>(m)->getHealth(); }
void mobSetHealth(void *m, int v) { static_cast<endstone::Mob *>(m)->setHealth(v); }
int mobGetMaxHealth(void *m) { return static_cast<endstone::Mob *>(m)->getMaxHealth(); }
void mobSetMaxHealth(void *m, int v) { static_cast<endstone::Mob *>(m)->setMaxHealth(v); }
bool mobIsGliding(void *m) { return static_cast<endstone::Mob *>(m)->isGliding(); }
void *actorAsMob(void *a) { return static_cast<endstone::Actor *>(a)->asMob(); }
void *actorGetDimension(void *a) { return &static_cast<endstone::Actor *>(a)->getDimension(); }
const char *dimensionGetName(void *d) { return strOut(static_cast<endstone::Dimension *>(d)->getName()); }
void *dimensionGetBlockAt(void *d, int x, int y, int z)
{
    auto block = static_cast<endstone::Dimension *>(d)->getBlockAt(x, y, z);
    return block ? block.release() : nullptr;
}
void *actorSpawnActor(void *a, const float *loc, const char *type)
{
    if (!type) {
        return nullptr;
    }
    const auto &ref = static_cast<endstone::Actor *>(a)->getLocation();
    return static_cast<endstone::Actor *>(a)->getDimension().spawnActor(locationFrom(loc, ref), type);
}

// ---- level ----

void *serverGetLevel(void *s) { return asServer(s)->getLevel(); }
const char *levelGetName(void *l) { return strOut(static_cast<endstone::Level *>(l)->getName()); }
int levelGetTime(void *l) { return static_cast<endstone::Level *>(l)->getTime(); }
void levelSetTime(void *l, int time) { static_cast<endstone::Level *>(l)->setTime(time); }
int64_t levelGetSeed(void *l) { return static_cast<endstone::Level *>(l)->getSeed(); }
int levelGetActors(void *l, void **out, int capacity)
{
    const auto actors = static_cast<endstone::Level *>(l)->getActors();
    const int count = std::min(capacity, static_cast<int>(actors.size()));
    std::copy_n(actors.begin(), count, out);
    return count;
}
int levelGetDimensions(void *l, void **out, int capacity)
{
    const auto dimensions = static_cast<endstone::Level *>(l)->getDimensions();
    const int count = std::min(capacity, static_cast<int>(dimensions.size()));
    std::copy_n(dimensions.begin(), count, out);
    return count;
}
void *levelGetDimensionByName(void *l, const char *name)
{
    if (!name) {
        return nullptr;
    }
    return static_cast<endstone::Level *>(l)->getDimension(name);
}

// ---- dimension ----

int dimensionGetType(void *d) { return static_cast<int>(static_cast<endstone::Dimension *>(d)->getType()); }
void *dimensionGetLevel(void *d) { return &static_cast<endstone::Dimension *>(d)->getLevel(); }
int dimensionGetHighestBlockYAt(void *d, int x, int z)
{
    return static_cast<endstone::Dimension *>(d)->getHighestBlockYAt(x, z);
}
void *dimensionGetHighestBlockAt(void *d, int x, int z)
{
    auto block = static_cast<endstone::Dimension *>(d)->getHighestBlockAt(x, z);
    return block ? block.release() : nullptr;
}
int dimensionGetLoadedChunks(void *d, void **out, int capacity)
{
    auto chunks = static_cast<endstone::Dimension *>(d)->getLoadedChunks();
    const int count = std::min(capacity, static_cast<int>(chunks.size()));
    for (int i = 0; i < count; i++) {
        out[i] = chunks[i].release();
    }
    return count;
}
int dimensionGetActors(void *d, void **out, int capacity)
{
    const auto actors = static_cast<endstone::Dimension *>(d)->getActors();
    const int count = std::min(capacity, static_cast<int>(actors.size()));
    std::copy_n(actors.begin(), count, out);
    return count;
}
void *dimensionSpawnActor(void *d, const float *loc, const char *type)
{
    if (!loc || !type) {
        return nullptr;
    }
    auto &dimension = *static_cast<endstone::Dimension *>(d);
    return dimension.spawnActor(endstone::Location(dimension, loc[0], loc[1], loc[2], loc[3], loc[4]), type);
}
void *dimensionDropItem(void *d, const float *loc, void *stack)
{
    if (!loc || !stack) {
        return nullptr;
    }
    auto &dimension = *static_cast<endstone::Dimension *>(d);
    return &dimension.dropItem(endstone::Location(dimension, loc[0], loc[1], loc[2], loc[3], loc[4]),
                               *static_cast<endstone::ItemStack *>(stack));
}

// ---- chunk / item stack ----

int chunkObjGetX(void *c) { return static_cast<endstone::Chunk *>(c)->getX(); }
int chunkObjGetZ(void *c) { return static_cast<endstone::Chunk *>(c)->getZ(); }
void *chunkObjGetDimension(void *c) { return &static_cast<endstone::Chunk *>(c)->getDimension(); }
void chunkObjDelete(void *c) { delete static_cast<endstone::Chunk *>(c); }
void *itemStackCreate(const char *type, int amount, int data)
{
    if (!type) {
        return nullptr;
    }
    return new endstone::ItemStack(endstone::ItemTypeId{std::string(type)}, amount, data);
}
void itemStackDelete(void *i) { delete static_cast<endstone::ItemStack *>(i); }

// ---- map ----

struct MapRendererHolder {
    std::shared_ptr<endstone::MapRenderer> renderer;
};

class BridgeMapRenderer final : public endstone::MapRenderer {
public:
    BridgeMapRenderer(const bool contextual, uint64_t renderer_id)
        : MapRenderer(contextual), renderer_id_(renderer_id)
    {
    }

    void render(endstone::MapView &map, endstone::MapCanvas &canvas, endstone::Player &player) override
    {
        auto fn = mutableBridgeTable().map_render_callback;
        if (fn) {
            fn(&canvas, &map, &player, renderer_id_);
        }
    }

private:
    uint64_t renderer_id_;
};

MapRendererHolder *asMapRenderer(void *h) { return static_cast<MapRendererHolder *>(h); }

// Native renderer pointer -> managed renderer id (only for bridge renderers).
std::unordered_map<const endstone::MapRenderer *, uint64_t> &rendererIds()
{
    static std::unordered_map<const endstone::MapRenderer *, uint64_t> ids;
    return ids;
}

void *serverGetMap(void *s, int64_t id) { return asServer(s)->getMap(id); }
void *serverCreateMap(void *s, void *dimension)
{
    if (!dimension) {
        return nullptr;
    }
    return &asServer(s)->createMap(*static_cast<endstone::Dimension *>(dimension));
}
int64_t mapGetId(void *m) { return static_cast<endstone::MapView *>(m)->getId(); }
bool mapIsVirtual(void *m) { return static_cast<endstone::MapView *>(m)->isVirtual(); }
int mapGetScale(void *m) { return static_cast<int>(static_cast<endstone::MapView *>(m)->getScale()); }
void mapSetScale(void *m, int scale)
{
    static_cast<endstone::MapView *>(m)->setScale(static_cast<endstone::MapView::Scale>(scale));
}
int mapGetCenterX(void *m) { return static_cast<endstone::MapView *>(m)->getCenterX(); }
int mapGetCenterZ(void *m) { return static_cast<endstone::MapView *>(m)->getCenterZ(); }
void mapSetCenterX(void *m, int x) { static_cast<endstone::MapView *>(m)->setCenterX(x); }
void mapSetCenterZ(void *m, int z) { static_cast<endstone::MapView *>(m)->setCenterZ(z); }
void *mapGetDimension(void *m) { return static_cast<endstone::MapView *>(m)->getDimension(); }
void mapSetDimension(void *m, void *dimension)
{
    if (dimension) {
        static_cast<endstone::MapView *>(m)->setDimension(*static_cast<endstone::Dimension *>(dimension));
    }
}
bool mapIsUnlimitedTracking(void *m) { return static_cast<endstone::MapView *>(m)->isUnlimitedTracking(); }
void mapSetUnlimitedTracking(void *m, bool v) { static_cast<endstone::MapView *>(m)->setUnlimitedTracking(v); }
bool mapIsLocked(void *m) { return static_cast<endstone::MapView *>(m)->isLocked(); }
void mapSetLocked(void *m, bool v) { static_cast<endstone::MapView *>(m)->setLocked(v); }
void playerSendMap(void *p, void *m) { asPlayer(p)->sendMap(*static_cast<endstone::MapView *>(m)); }

void *mapRendererCreate(int contextual, uint64_t renderer_id)
{
    auto *holder = new MapRendererHolder{std::make_shared<BridgeMapRenderer>(contextual != 0, renderer_id)};
    rendererIds()[holder->renderer.get()] = renderer_id;
    return holder;
}
void mapRendererDestroy(void *h)
{
    auto *holder = asMapRenderer(h);
    if (!holder) {
        return;
    }
    rendererIds().erase(holder->renderer.get());
    delete holder;
}
void mapAddRenderer(void *m, void *h) { static_cast<endstone::MapView *>(m)->addRenderer(asMapRenderer(h)->renderer); }
bool mapRemoveRenderer(void *m, void *h)
{
    return static_cast<endstone::MapView *>(m)->removeRenderer(asMapRenderer(h)->renderer);
}
int mapGetRendererCount(void *m) { return static_cast<int>(static_cast<endstone::MapView *>(m)->getRenderers().size()); }
int mapGetRenderer(void *m, int index, uint64_t *out_id)
{
    const auto renderers = static_cast<endstone::MapView *>(m)->getRenderers();
    if (index < 0 || index >= static_cast<int>(renderers.size())) {
        return 0;
    }
    const auto it = rendererIds().find(renderers[static_cast<size_t>(index)].get());
    if (it == rendererIds().end()) {
        return 0;
    }
    *out_id = it->second;
    return 1;
}

void *canvasGetMapView(void *c) { return &static_cast<endstone::MapCanvas *>(c)->getMapView(); }
int canvasGetCursorCount(void *c) { return static_cast<int>(static_cast<endstone::MapCanvas *>(c)->getCursors().size()); }
void canvasGetCursor(void *c, int index, int8_t *out)
{
    const auto cursors = static_cast<endstone::MapCanvas *>(c)->getCursors();
    if (index < 0 || index >= static_cast<int>(cursors.size())) {
        return;
    }
    const auto &cursor = cursors[static_cast<size_t>(index)];
    out[0] = cursor.getX();
    out[1] = cursor.getY();
    out[2] = cursor.getDirection();
    out[3] = static_cast<int8_t>(cursor.getType());
    out[4] = cursor.isVisible() ? 1 : 0;
}
const char *canvasGetCursorCaption(void *c, int index)
{
    const auto cursors = static_cast<endstone::MapCanvas *>(c)->getCursors();
    if (index < 0 || index >= static_cast<int>(cursors.size())) {
        return nullptr;
    }
    return strOut(cursors[static_cast<size_t>(index)].getCaption());
}
void canvasSetCursors(void *c, const int8_t *records, int count, const char *const *captions)
{
    std::vector<endstone::MapCursor> cursors;
    cursors.reserve(count);
    for (int i = 0; i < count; i++) {
        const auto *r = records + static_cast<size_t>(i) * 5;
        cursors.emplace_back(r[0], r[1], r[2], static_cast<endstone::MapCursor::Type>(r[3]), r[4] != 0,
                             captions && captions[i] ? captions[i] : "");
    }
    static_cast<endstone::MapCanvas *>(c)->setCursors(cursors);
}
void canvasSetPixelColor(void *c, int x, int y, int r, int g, int b, int a)
{
    static_cast<endstone::MapCanvas *>(c)->setPixelColor(x, y, endstone::Color::fromRGBA(r, g, b, a));
}
int canvasGetPixelColor(void *c, int x, int y) { return static_cast<endstone::MapCanvas *>(c)->getPixelColor(x, y).asRGBA(); }
int canvasGetBasePixelColor(void *c, int x, int y)
{
    return static_cast<endstone::MapCanvas *>(c)->getBasePixelColor(x, y).asRGBA();
}
void canvasSetPixel(void *c, int x, int y, uint32_t color)
{
    static_cast<endstone::MapCanvas *>(c)->setPixel(x, y, color);
}
uint32_t canvasGetPixel(void *c, int x, int y) { return static_cast<endstone::MapCanvas *>(c)->getPixel(x, y); }
uint32_t canvasGetBasePixel(void *c, int x, int y) { return static_cast<endstone::MapCanvas *>(c)->getBasePixel(x, y); }

bool itemHasMapView(void *i)
{
    const auto meta = itemMeta(i);
    const auto *map_meta = meta ? meta->as<endstone::MapMeta>() : nullptr;
    return map_meta && map_meta->hasMapView();
}
void *itemGetMapView(void *i)
{
    const auto meta = itemMeta(i);
    const auto *map_meta = meta ? meta->as<endstone::MapMeta>() : nullptr;
    return map_meta && map_meta->hasMapView() ? map_meta->getMapView() : nullptr;
}
bool itemSetMapView(void *i, void *map)
{
    if (!map) {
        return false;
    }
    auto meta = itemMeta(i);
    auto *map_meta = meta ? meta->as<endstone::MapMeta>() : nullptr;
    if (!map_meta) {
        return false;
    }
    map_meta->setMapView(static_cast<endstone::MapView *>(map));
    return asItem(i)->setItemMeta(meta.get());
}

void *playerGetInventory(void *p) { return &asPlayer(p)->getInventory(); }
void *playerGetEnderChest(void *p) { return &asPlayer(p)->getEnderChest(); }

endstone::Inventory *asInventory(void *i) { return static_cast<endstone::Inventory *>(i); }
endstone::PlayerInventory *asPlayerInventory(void *i) { return static_cast<endstone::PlayerInventory *>(i); }

int inventoryGetSize(void *i) { return asInventory(i)->getSize(); }
int inventoryGetMaxStackSize(void *i) { return asInventory(i)->getMaxStackSize(); }
void *inventoryGetItem(void *i, int index) { return itemSnapshot(asInventory(i)->getItem(index)); }
void inventorySetItem(void *i, int index, void *stack)
{
    if (stack) {
        asInventory(i)->setItem(index, *asItem(stack));
    }
    else {
        asInventory(i)->setItem(index, std::nullopt);
    }
}
bool inventoryAddItem(void *i, void *stack)
{
    const auto leftover = asInventory(i)->addItem({*asItem(stack)});
    return leftover.empty();
}
bool inventoryRemoveItem(void *i, void *stack)
{
    const auto leftover = asInventory(i)->removeItem({*asItem(stack)});
    return leftover.empty();
}
bool inventoryContains(void *i, void *stack) { return asInventory(i)->contains(*asItem(stack)); }
bool inventoryIsEmpty(void *i) { return asInventory(i)->isEmpty(); }
int inventoryFirstEmpty(void *i) { return asInventory(i)->firstEmpty(); }
void inventoryClear(void *i) { asInventory(i)->clear(); }
int inventoryFirst(void *i, const char *type) { return asInventory(i)->first(std::string(type ? type : "")); }

void *inventoryGetItemInMainHand(void *i) { return itemSnapshot(asPlayerInventory(i)->getItemInMainHand()); }
void inventorySetItemInMainHand(void *i, void *stack)
{
    if (stack) {
        asPlayerInventory(i)->setItemInMainHand(*asItem(stack));
    }
    else {
        asPlayerInventory(i)->setItemInMainHand(std::nullopt);
    }
}
void *inventoryGetItemInOffHand(void *i) { return itemSnapshot(asPlayerInventory(i)->getItemInOffHand()); }
void inventorySetItemInOffHand(void *i, void *stack)
{
    if (stack) {
        asPlayerInventory(i)->setItemInOffHand(*asItem(stack));
    }
    else {
        asPlayerInventory(i)->setItemInOffHand(std::nullopt);
    }
}
void *inventoryGetHelmet(void *i) { return itemSnapshot(asPlayerInventory(i)->getHelmet()); }
void inventorySetHelmet(void *i, void *stack)
{
    if (stack) {
        asPlayerInventory(i)->setHelmet(*asItem(stack));
    }
    else {
        asPlayerInventory(i)->setHelmet(std::nullopt);
    }
}
void *inventoryGetChestplate(void *i) { return itemSnapshot(asPlayerInventory(i)->getChestplate()); }
void inventorySetChestplate(void *i, void *stack)
{
    if (stack) {
        asPlayerInventory(i)->setChestplate(*asItem(stack));
    }
    else {
        asPlayerInventory(i)->setChestplate(std::nullopt);
    }
}
void *inventoryGetLeggings(void *i) { return itemSnapshot(asPlayerInventory(i)->getLeggings()); }
void inventorySetLeggings(void *i, void *stack)
{
    if (stack) {
        asPlayerInventory(i)->setLeggings(*asItem(stack));
    }
    else {
        asPlayerInventory(i)->setLeggings(std::nullopt);
    }
}
void *inventoryGetBoots(void *i) { return itemSnapshot(asPlayerInventory(i)->getBoots()); }
void inventorySetBoots(void *i, void *stack)
{
    if (stack) {
        asPlayerInventory(i)->setBoots(*asItem(stack));
    }
    else {
        asPlayerInventory(i)->setBoots(std::nullopt);
    }
}
int inventoryGetHeldItemSlot(void *i) { return asPlayerInventory(i)->getHeldItemSlot(); }
void inventorySetHeldItemSlot(void *i, int slot) { asPlayerInventory(i)->setHeldItemSlot(slot); }

void *serverGetScheduler(void *s) { return &asServer(s)->getScheduler(); }

uint32_t schedulerRunTask(void *scheduler, void *plugin, int mode, uint64_t delay, uint64_t period, uint64_t managed_id)
{
    auto &s = *static_cast<endstone::Scheduler *>(scheduler);
    auto &p = *static_cast<endstone::Plugin *>(plugin);
    std::function<void()> fn = [managed_id] {
        const auto cb = mutableBridgeTable().scheduler_task_callback;
        if (cb) cb(managed_id);
    };
    std::shared_ptr<endstone::Task> task;
    switch (mode) {
        case 0: task = s.runTask(p, fn); break;
        case 1: task = s.runTaskLater(p, fn, delay); break;
        case 2: task = s.runTaskTimer(p, fn, delay, period); break;
        case 3: task = s.runTaskAsync(p, fn); break;
        case 4: task = s.runTaskLaterAsync(p, fn, delay); break;
        case 5: task = s.runTaskTimerAsync(p, fn, delay, period); break;
        default: return 0;
    }
    return task ? task->getTaskId() : 0;
}
void schedulerCancelTask(void *scheduler, uint32_t task_id)
{
    static_cast<endstone::Scheduler *>(scheduler)->cancelTask(task_id);
}
void schedulerCancelTasks(void *scheduler, void *plugin)
{
    static_cast<endstone::Scheduler *>(scheduler)->cancelTasks(*static_cast<endstone::Plugin *>(plugin));
}
bool schedulerIsRunning(void *scheduler, uint32_t task_id)
{
    return static_cast<endstone::Scheduler *>(scheduler)->isRunning(task_id);
}
bool schedulerIsQueued(void *scheduler, uint32_t task_id)
{
    return static_cast<endstone::Scheduler *>(scheduler)->isQueued(task_id);
}
int schedulerGetPendingTasks(void *scheduler, void **out, int capacity)
{
    const auto tasks = static_cast<endstone::Scheduler *>(scheduler)->getPendingTasks();
    const int n = std::min(capacity, static_cast<int>(tasks.size()));
    for (int i = 0; i < n; ++i) out[i] = tasks[i];
    return static_cast<int>(tasks.size());
}
uint32_t taskGetId(void *t) { return static_cast<endstone::Task *>(t)->getTaskId(); }
bool taskIsSync(void *t) { return static_cast<endstone::Task *>(t)->isSync(); }
bool taskIsCancelled(void *t) { return static_cast<endstone::Task *>(t)->isCancelled(); }

// ---- service manager ----

endstone::ServiceManager *asServiceManager(void *s) { return static_cast<endstone::ServiceManager *>(s); }

// Holder = heap-allocated std::shared_ptr<endstone::Service>; the managed side
// owns the allocation and must release it with serviceProviderRelease once the
// wrapper is no longer needed.
using ServiceHolder = std::shared_ptr<endstone::Service>;

ServiceHolder *asServiceHolder(void *h) { return static_cast<ServiceHolder *>(h); }

void *serverGetServiceManager(void *s) { return &asServer(s)->getServiceManager(); }

void *serviceProviderCreate()
{
    return new ServiceHolder(std::make_shared<endstone::Service>());
}
void *serviceProviderGetPtr(void *holder)
{
    auto *h = asServiceHolder(holder);
    return h && *h ? h->get() : nullptr;
}
void serviceProviderRelease(void *holder) { delete asServiceHolder(holder); }

void serviceManagerRegister(void *sm, const char *name, void *provider_holder, void *plugin, int priority)
{
    auto *holder = asServiceHolder(provider_holder);
    if (!holder || !*holder || !plugin) {
        return;
    }
    asServiceManager(sm)->registerService(name ? name : "", *holder, *static_cast<endstone::Plugin *>(plugin),
                                          static_cast<endstone::ServicePriority>(priority));
}
void serviceManagerUnregisterAll(void *sm, void *plugin)
{
    if (!plugin) {
        return;
    }
    asServiceManager(sm)->unregisterAll(*static_cast<endstone::Plugin *>(plugin));
}
void serviceManagerUnregister(void *sm, const char *name, void *provider_ptr)
{
    if (!provider_ptr) {
        return;
    }
    asServiceManager(sm)->unregister(name ? name : "", *static_cast<endstone::Service *>(provider_ptr));
}
void serviceManagerUnregisterProvider(void *sm, void *provider_ptr)
{
    if (!provider_ptr) {
        return;
    }
    asServiceManager(sm)->unregister(*static_cast<endstone::Service *>(provider_ptr));
}
void *serviceManagerGet(void *sm, const char *name)
{
    auto provider = asServiceManager(sm)->get(name ? name : "");
    if (!provider) {
        return nullptr;
    }
    return new ServiceHolder(std::move(provider));
}

const char *itemGetType(void *i)
{
    const auto &type = asItem(i)->getType();
    return strOut(static_cast<std::string>(type.getId()));
}
int itemGetAmount(void *i) { return asItem(i)->getAmount(); }
int itemGetData(void *i) { return asItem(i)->getData(); }
int itemGetMaxStackSize(void *i) { return asItem(i)->getMaxStackSize(); }
const char *itemGetTranslationKey(void *i) { return strOut(asItem(i)->getTranslationKey()); }
const char *itemActorGetType(void *i)
{
    const auto &type = static_cast<endstone::Item *>(i)->getItemStack().getType();
    return strOut(static_cast<std::string>(type.getId()));
}
int itemActorGetAmount(void *i) { return static_cast<endstone::Item *>(i)->getItemStack().getAmount(); }
const char *itemActorGetTranslationKey(void *i)
{
    return strOut(static_cast<endstone::Item *>(i)->getItemStack().getTranslationKey());
}

std::unique_ptr<endstone::ItemMeta> itemMeta(void *i);

bool itemHasDisplayName(void *i)
{
    const auto meta = itemMeta(i);
    return meta && meta->hasDisplayName();
}
const char *itemGetDisplayName(void *i)
{
    const auto meta = itemMeta(i);
    return (meta && meta->hasDisplayName()) ? strOut(meta->getDisplayName()) : nullptr;
}
bool itemHasLore(void *i)
{
    const auto meta = itemMeta(i);
    return meta && meta->hasLore();
}
int itemGetLoreCount(void *i)
{
    const auto meta = itemMeta(i);
    return (meta && meta->hasLore()) ? static_cast<int>(meta->getLore().size()) : 0;
}
const char *itemGetLoreLine(void *i, int index)
{
    const auto meta = itemMeta(i);
    if (meta && meta->hasLore()) {
        const auto &lore = meta->getLore();
        if (index >= 0 && index < static_cast<int>(lore.size())) {
            return strOut(lore[index]);
        }
    }
    return nullptr;
}
bool itemHasDamage(void *i)
{
    const auto meta = itemMeta(i);
    return meta && meta->hasDamage();
}
int itemGetDamage(void *i)
{
    const auto meta = itemMeta(i);
    return meta ? meta->getDamage() : 0;
}
bool itemIsUnbreakable(void *i)
{
    const auto meta = itemMeta(i);
    return meta && meta->isUnbreakable();
}
bool itemHasEnchants(void *i)
{
    const auto meta = itemMeta(i);
    return meta && meta->hasEnchants();
}
int itemGetEnchantCount(void *i)
{
    const auto meta = itemMeta(i);
    return meta ? static_cast<int>(meta->getEnchants().size()) : 0;
}
const char *itemGetEnchantName(void *i, int index)
{
    const auto meta = itemMeta(i);
    if (meta) {
        const auto &enchants = meta->getEnchants();
        if (index >= 0 && index < static_cast<int>(enchants.size())) {
            return strOut(static_cast<std::string>(std::next(enchants.begin(), index)->first->getId()));
        }
    }
    return nullptr;
}
int itemGetEnchantLevel(void *i, int index)
{
    const auto meta = itemMeta(i);
    if (meta) {
        const auto &enchants = meta->getEnchants();
        if (index >= 0 && index < static_cast<int>(enchants.size())) {
            return std::next(enchants.begin(), index)->second;
        }
    }
    return 0;
}

// Enchantment ids are "namespace:key" strings; a bare key implies the minecraft namespace.
endstone::EnchantmentId enchantId(const char *text) { return endstone::EnchantmentId{text ? text : ""}; }

bool itemHasEnchant(void *i, const char *id)
{
    const auto meta = itemMeta(i);
    return meta && meta->hasEnchant(enchantId(id));
}
int itemGetEnchantLevelById(void *i, const char *id)
{
    const auto meta = itemMeta(i);
    return meta ? meta->getEnchantLevel(enchantId(id)) : 0;
}
bool itemAddEnchant(void *i, const char *id, int level, bool force)
{
    const auto meta = itemMeta(i);
    if (!meta) {
        return false;
    }
    const bool changed = meta->addEnchant(enchantId(id), level, force);
    return changed && asItem(i)->setItemMeta(meta.get());
}
bool itemRemoveEnchant(void *i, const char *id)
{
    const auto meta = itemMeta(i);
    if (!meta) {
        return false;
    }
    const bool changed = meta->removeEnchant(enchantId(id));
    return changed && asItem(i)->setItemMeta(meta.get());
}
void itemRemoveEnchants(void *i)
{
    const auto meta = itemMeta(i);
    if (meta && meta->hasEnchants()) {
        meta->removeEnchants();
        asItem(i)->setItemMeta(meta.get());
    }
}
bool itemHasConflictingEnchant(void *i, const char *id)
{
    const auto meta = itemMeta(i);
    return meta && meta->hasConflictingEnchant(enchantId(id));
}

const endstone::Enchantment *asEnchantment(const void *e) { return static_cast<const endstone::Enchantment *>(e); }

const void *enchantGetById(const char *id)
{
    try {
        return endstone::Enchantment::get(enchantId(id));
    }
    catch (const std::exception &) {
        return nullptr;
    }
}
const char *enchantGetId(const void *e) { return strOut(static_cast<std::string>(asEnchantment(e)->getId())); }
int enchantGetMaxLevel(const void *e) { return asEnchantment(e)->getMaxLevel(); }
int enchantGetStartLevel(const void *e) { return asEnchantment(e)->getStartLevel(); }
bool enchantConflictsWith(const void *e, const void *other)
{
    return other && asEnchantment(e)->conflictsWith(*asEnchantment(other));
}
bool enchantCanEnchantItem(const void *e, void *item)
{
    return item && asEnchantment(e)->canEnchantItem(*asItem(item));
}
const char *blockGetType(void *b) { return strOut(asBlock(b)->getType()); }
int blockGetX(void *b) { return asBlock(b)->getX(); }
int blockGetY(void *b) { return asBlock(b)->getY(); }
int blockGetZ(void *b) { return asBlock(b)->getZ(); }
void blockSetType(void *b, const char *type) { asBlock(b)->setType(std::string(type ? type : "")); }
void blockSetTypePhysics(void *b, const char *type, bool apply_physics)
{
    asBlock(b)->setType(std::string(type ? type : ""), apply_physics);
}
void blockGetLocation(void *b, float *out)
{
    const auto loc = asBlock(b)->getLocation();
    out[0] = loc.getX();
    out[1] = loc.getY();
    out[2] = loc.getZ();
    out[3] = loc.getPitch();
    out[4] = loc.getYaw();
}
const char *blockGetDimensionName(void *b) { return strOut(asBlock(b)->getDimension().getName()); }
void *blockGetRelative(void *b, int dx, int dy, int dz)
{
    auto rel = asBlock(b)->getRelative(dx, dy, dz);
    return rel ? rel.release() : nullptr;
}
void *blockCaptureState(void *b)
{
    auto state = asBlock(b)->captureState();
    return state ? state.release() : nullptr;
}
void blockDelete(void *b) { delete static_cast<endstone::Block *>(b); }
const char *blockStateGetType(void *b)
{
    return strOut(static_cast<endstone::BlockState *>(b)->getType());
}
int blockStateGetX(void *b) { return static_cast<endstone::BlockState *>(b)->getX(); }
int blockStateGetY(void *b) { return static_cast<endstone::BlockState *>(b)->getY(); }
int blockStateGetZ(void *b) { return static_cast<endstone::BlockState *>(b)->getZ(); }
void blockStateSetType(void *b, const char *type)
{
    static_cast<endstone::BlockState *>(b)->setType(std::string(type ? type : ""));
}
void blockStateGetLocation(void *b, float *out)
{
    const auto loc = static_cast<endstone::BlockState *>(b)->getLocation();
    out[0] = loc.getX();
    out[1] = loc.getY();
    out[2] = loc.getZ();
    out[3] = loc.getPitch();
    out[4] = loc.getYaw();
}
bool blockStateUpdate(void *b) { return static_cast<endstone::BlockState *>(b)->update(); }
bool blockStateUpdateForce(void *b, bool force) { return static_cast<endstone::BlockState *>(b)->update(force); }
bool blockStateUpdateForcePhysics(void *b, bool force, bool apply_physics)
{
    return static_cast<endstone::BlockState *>(b)->update(force, apply_physics);
}
void blockStateDelete(void *b) { delete static_cast<endstone::BlockState *>(b); }

// ---- form ----
struct FormHolder {
    std::variant<endstone::MessageForm, endstone::ActionForm, endstone::ModalForm> form;
};

FormHolder *asForm(void *f) { return static_cast<FormHolder *>(f); }

void formDispatch(void *player, int result_kind, uint64_t form_id, int button, const std::string &payload)
{
    auto fn = mutableBridgeTable().form_dispatch_result;
    if (fn) {
        fn(player, result_kind, form_id, button, payload.c_str());
    }
}

void *formCreate(int kind)
{
    auto *h = new FormHolder();
    switch (kind) {
    case 0:
        h->form = endstone::MessageForm();
        break;
    case 1:
        h->form = endstone::ActionForm();
        break;
    default:
        h->form = endstone::ModalForm();
        break;
    }
    return h;
}

void formSetTitle(void *f, const char *title)
{
    std::visit([&](auto &form) { form.setTitle(std::string(title ? title : "")); }, asForm(f)->form);
}

void formSetContent(void *f, const char *content)
{
    auto &h = *asForm(f);
    std::visit(
        [&](auto &form) {
            using F = std::decay_t<decltype(form)>;
            if constexpr (std::is_same_v<F, endstone::MessageForm> || std::is_same_v<F, endstone::ActionForm>) {
                form.setContent(std::string(content ? content : ""));
            }
        },
        h.form);
}

void formSetButton1(void *f, const char *text)
{
    std::visit(
        [&](auto &form) {
            using F = std::decay_t<decltype(form)>;
            if constexpr (std::is_same_v<F, endstone::MessageForm>) {
                form.setButton1(std::string(text ? text : ""));
            }
        },
        asForm(f)->form);
}

void formSetButton2(void *f, const char *text)
{
    std::visit(
        [&](auto &form) {
            using F = std::decay_t<decltype(form)>;
            if constexpr (std::is_same_v<F, endstone::MessageForm>) {
                form.setButton2(std::string(text ? text : ""));
            }
        },
        asForm(f)->form);
}

void formAddButton(void *f, const char *text, const char *icon)
{
    std::visit(
        [&](auto &form) {
            using F = std::decay_t<decltype(form)>;
            if constexpr (std::is_same_v<F, endstone::ActionForm>) {
                if (icon) {
                    form.addButton(std::string(text ? text : ""), std::string(icon));
                }
                else {
                    form.addButton(std::string(text ? text : ""));
                }
            }
        },
        asForm(f)->form);
}

void formAddControl(void *f, int kind, const char *text, const char *options, const char *fmt)
{
    auto &h = *asForm(f);
    auto split = [](const char *s, char sep) {
        std::vector<std::string> out;
        if (!s) {
            return out;
        }
        std::string cur;
        for (const char *p = s; *p; ++p) {
            if (*p == sep) {
                out.push_back(cur);
                cur.clear();
            }
            else {
                cur.push_back(*p);
            }
        }
        out.push_back(cur);
        return out;
    };
    const std::vector<std::string> opts = split(options, ';');
    const std::vector<std::string> args = split(fmt, ';');
    std::visit(
        [&](auto &form) {
            using F = std::decay_t<decltype(form)>;
            if constexpr (std::is_same_v<F, endstone::ModalForm>) {
                const std::string t = text ? text : "";
                switch (kind) {
                case 0:
                    form.addControl(endstone::Label(t));
                    break;
                case 1:
                    form.addControl(endstone::Header(t));
                    break;
                case 2:
                    form.addControl(endstone::Divider());
                    break;
                case 3: {  // dropdown
                    std::optional<int> def;
                    if (args.size() > 0 && !args[0].empty()) {
                        def = std::stoi(args[0]);
                    }
                    form.addControl(endstone::Dropdown(t, opts, def));
                    break;
                }
                case 4: {  // slider: fmt = default;min;max;step
                    float min = 0, max = 100, step = 1, def = 0;
                    bool has_def = false;
                    if (args.size() > 0 && !args[0].empty()) {
                        def = std::stof(args[0]);
                        has_def = true;
                    }
                    if (args.size() > 1 && !args[1].empty()) {
                        min = std::stof(args[1]);
                    }
                    if (args.size() > 2 && !args[2].empty()) {
                        max = std::stof(args[2]);
                    }
                    if (args.size() > 3 && !args[3].empty()) {
                        step = std::stof(args[3]);
                    }
                    form.addControl(endstone::Slider(t, min, max, step, has_def ? std::optional<float>(def) : std::nullopt));
                    break;
                }
                case 5: {  // step slider: options;fmt=default
                    std::optional<int> def;
                    if (args.size() > 0 && !args[0].empty()) {
                        def = std::stoi(args[0]);
                    }
                    form.addControl(endstone::StepSlider(t, opts, def));
                    break;
                }
                case 6: {  // text input: fmt = placeholder;default
                    std::string placeholder = args.size() > 0 ? args[0] : "";
                    std::optional<std::string> def;
                    if (args.size() > 1 && !args[1].empty()) {
                        def = args[1];
                    }
                    form.addControl(endstone::TextInput(t, placeholder, def));
                    break;
                }
                case 7: {  // toggle: fmt = default(0/1)
                    bool def = args.size() > 0 && args[0] == "1";
                    form.addControl(endstone::Toggle(t, def));
                    break;
                }
                }
            }
        },
        h.form);
}

void formSetSubmitButton(void *f, const char *text)
{
    std::visit(
        [&](auto &form) {
            using F = std::decay_t<decltype(form)>;
            if constexpr (std::is_same_v<F, endstone::ModalForm>) {
                form.setSubmitButton(std::optional<endstone::Message>(std::string(text ? text : "")));
            }
        },
        asForm(f)->form);
}

void formSetIcon(void *f, const char *icon)
{
    std::visit(
        [&](auto &form) {
            using F = std::decay_t<decltype(form)>;
            if constexpr (std::is_same_v<F, endstone::ModalForm>) {
                form.setIcon(icon ? std::optional<std::string>(icon) : std::nullopt);
            }
        },
        asForm(f)->form);
}

void formSetCallbacks(void *f, uint64_t form_id)
{
    auto &h = *asForm(f);
    std::visit(
        [form_id](auto &form) {
            using F = std::decay_t<decltype(form)>;
            form.setOnClose([form_id](endstone::Player *p) { formDispatch(p, 1, form_id, -1, ""); });
            if constexpr (std::is_same_v<F, endstone::ModalForm>) {
                form.setOnSubmit([form_id](endstone::Player *p, std::string result) {
                    formDispatch(p, 0, form_id, -1, result);
                });
            }
            else {
                form.setOnSubmit([form_id](endstone::Player *p, int button) {
                    formDispatch(p, 0, form_id, button, "");
                });
            }
        },
        h.form);
}

void formSend(void *player, void *f)
{
    auto *h = asForm(f);
    asPlayer(player)->sendForm(std::move(h->form));
    delete h;
}

void formDestroy(void *f) { delete asForm(f); }

// ---- boss bar ----
endstone::BossBar *asBossBar(void *b) { return static_cast<endstone::BossBar *>(b); }

endstone::BarFlag barFlagFromInt(int flag)
{
    return flag & 2 ? endstone::BarFlag::CreateFog : endstone::BarFlag::DarkenSky;
}

std::vector<endstone::BarFlag> barFlagsFromMask(int flags)
{
    std::vector<endstone::BarFlag> out;
    if (flags & 1) {
        out.push_back(endstone::BarFlag::DarkenSky);
    }
    if (flags & 2) {
        out.push_back(endstone::BarFlag::CreateFog);
    }
    return out;
}

void *serverCreateBossBar(void *s, const char *title, int color, int style, int flags)
{
    auto bar = asServer(s)->createBossBar(std::string(title ? title : ""),
                                          static_cast<endstone::BarColor>(color),
                                          static_cast<endstone::BarStyle>(style), barFlagsFromMask(flags));
    return bar ? bar.release() : nullptr;
}

const char *bossBarGetTitle(void *b) { return strOut(asBossBar(b)->getTitle()); }
void bossBarSetTitle(void *b, const char *v) { asBossBar(b)->setTitle(v ? v : ""); }
int bossBarGetColor(void *b) { return static_cast<int>(asBossBar(b)->getColor()); }
void bossBarSetColor(void *b, int v) { asBossBar(b)->setColor(static_cast<endstone::BarColor>(v)); }
int bossBarGetStyle(void *b) { return static_cast<int>(asBossBar(b)->getStyle()); }
void bossBarSetStyle(void *b, int v) { asBossBar(b)->setStyle(static_cast<endstone::BarStyle>(v)); }
bool bossBarHasFlag(void *b, int flag) { return asBossBar(b)->hasFlag(barFlagFromInt(flag)); }
void bossBarAddFlag(void *b, int flag) { asBossBar(b)->addFlag(barFlagFromInt(flag)); }
void bossBarRemoveFlag(void *b, int flag) { asBossBar(b)->removeFlag(barFlagFromInt(flag)); }
float bossBarGetProgress(void *b) { return asBossBar(b)->getProgress(); }
void bossBarSetProgress(void *b, float v) { asBossBar(b)->setProgress(v); }
bool bossBarIsVisible(void *b) { return asBossBar(b)->isVisible(); }
void bossBarSetVisible(void *b, bool v) { asBossBar(b)->setVisible(v); }
void bossBarAddPlayer(void *b, void *p) { asBossBar(b)->addPlayer(*asPlayer(p)); }
void bossBarRemovePlayer(void *b, void *p) { asBossBar(b)->removePlayer(*asPlayer(p)); }
void bossBarRemoveAll(void *b) { asBossBar(b)->removeAll(); }
int bossBarGetPlayerCount(void *b)
{
    return static_cast<int>(asBossBar(b)->getPlayers().size());
}
void *bossBarGetPlayer(void *b, int index)
{
    const auto &players = asBossBar(b)->getPlayers();
    if (index >= 0 && index < static_cast<int>(players.size())) {
        return players[static_cast<size_t>(index)];
    }
    return nullptr;
}
void bossBarDestroy(void *b)
{
    auto *bar = asBossBar(b);
    bar->removeAll();
    delete bar;
}

const char *damageSourceGetType(void *d)
{
    return strOut(std::string(static_cast<endstone::DamageSource *>(d)->getType()));
}
void *damageSourceGetActor(void *d) { return static_cast<endstone::DamageSource *>(d)->getActor(); }
void *damageSourceGetDamagingActor(void *d) { return static_cast<endstone::DamageSource *>(d)->getDamagingActor(); }
bool damageSourceIsIndirect(void *d) { return static_cast<endstone::DamageSource *>(d)->isIndirect(); }

const char *senderGetName(void *s)
{
    return strOut(static_cast<endstone::CommandSender *>(s)->getName());
}
void senderSendMessage(void *s, const char *msg)
{
    static_cast<endstone::CommandSender *>(s)->sendMessage(std::string(msg ? msg : ""));
}
void senderSendErrorMessage(void *s, const char *msg)
{
    static_cast<endstone::CommandSender *>(s)->sendErrorMessage(std::string(msg ? msg : ""));
}
bool senderHasPermission(void *s, const char *perm)
{
    return static_cast<endstone::CommandSender *>(s)->hasPermission(perm ? perm : "");
}
void *senderAsPlayer(void *s)
{
    return static_cast<endstone::CommandSender *>(s)->asPlayer();
}

}  // namespace

const BridgeTable &getBridgeTable()
{
    static BridgeTable table{
        .player_get_name = &playerGetName,
        .player_send_message = &playerSendMessage,
        .player_send_error_message = &playerSendErrorMessage,
        .player_send_popup = &playerSendPopup,
        .player_send_tip = &playerSendTip,
        .player_send_toast = &playerSendToast,
        .player_send_title = &playerSendTitle,
        .player_reset_title = &playerResetTitle,
        .player_kick = &playerKick,
        .player_perform_command = &playerPerformCommand,
        .player_is_op = &playerIsOp,
        .player_set_op = &playerSetOp,
        .player_get_xuid = &playerGetXuid,
        .player_get_address = &playerGetAddress,
        .player_is_sneaking = &playerIsSneaking,
        .player_set_sneaking = &playerSetSneaking,
        .player_is_sprinting = &playerIsSprinting,
        .player_set_sprinting = &playerSetSprinting,
        .player_get_ping = &playerGetPing,
        .player_get_locale = &playerGetLocale,
        .player_get_device_os = &playerGetDeviceOS,
        .player_get_device_id = &playerGetDeviceId,
        .player_get_game_version = &playerGetGameVersion,
        .player_get_game_mode = &playerGetGameMode,
        .player_set_game_mode = &playerSetGameMode,
        .player_get_allow_flight = &playerGetAllowFlight,
        .player_set_allow_flight = &playerSetAllowFlight,
        .player_is_flying = &playerIsFlying,
        .player_set_flying = &playerSetFlying,
        .player_get_exp_level = &playerGetExpLevel,
        .player_set_exp_level = &playerSetExpLevel,
        .player_give_exp = &playerGiveExp,
        .player_give_exp_levels = &playerGiveExpLevels,
        .player_get_exp_progress = &playerGetExpProgress,
        .player_set_exp_progress = &playerSetExpProgress,
        .player_get_total_exp = &playerGetTotalExp,
        .player_transfer = &playerTransfer,
        .player_play_sound = &playerPlaySound,
        .player_stop_sound = &playerStopSound,
        .player_stop_all_sounds = &playerStopAllSounds,
        .player_spawn_particle = &playerSpawnParticle,
        .player_get_fly_speed = &playerGetFlySpeed,
        .player_set_fly_speed = &playerSetFlySpeed,
        .player_get_walk_speed = &playerGetWalkSpeed,
        .player_set_walk_speed = &playerSetWalkSpeed,
        .player_update_commands = &playerUpdateCommands,
        .player_close_form = &playerCloseForm,
        .player_send_packet = &playerSendPacket,
        .player_get_skin_id = &playerGetSkinId,
        .player_get_skin_cape_id = &playerGetSkinCapeId,
        .player_get_item_in_hand = &playerGetItemInHand,
        .server_get_name = &serverGetName,
        .server_get_version = &serverGetVersion,
        .server_get_minecraft_version = &serverGetMinecraftVersion,
        .server_get_protocol_version = &serverGetProtocolVersion,
        .server_get_max_players = &serverGetMaxPlayers,
        .server_broadcast_message = &serverBroadcastMessage,
        .server_get_online_players = &serverGetOnlinePlayers,
        .server_get_player = &serverGetPlayer,
        .server_get_console_sender = &serverGetConsoleSender,
        .server_dispatch_command = &serverDispatchCommand,
        .event_get_player = &eventGetPlayer,
        .event_get_actor = &eventGetActor,
        .event_is_cancelled = &eventIsCancelled,
        .event_set_cancelled = &eventSetCancelled,
        .chat_get_message = &chatGetMessage,
        .chat_set_message = &chatSetMessage,
        .chat_get_format = &chatGetFormat,
        .chat_set_format = &chatSetFormat,
        .chat_get_recipient_count = &chatGetRecipientCount,
        .command_get_command = &commandGetCommand,
        .command_set_command = &commandSetCommand,
        .server_cmd_get_command = &serverCmdGetCommand,
        .server_cmd_set_command = &serverCmdSetCommand,
        .server_cmd_get_sender_name = &serverCmdGetSenderName,
        .server_cmd_get_sender = &serverCmdGetSender,
        .move_get_from = &moveGetFrom,
        .move_get_to = &moveGetTo,
        .move_set_from = &moveSetFrom,
        .move_set_to = &moveSetTo,
        .actor_tp_get_from = &actorTpGetFrom,
        .actor_tp_get_to = &actorTpGetTo,
        .actor_tp_set_from = &actorTpSetFrom,
        .actor_tp_set_to = &actorTpSetTo,
        .interact_get_action = &interactGetAction,
        .interact_get_clicked_position = &interactGetClickedPosition,
        .interact_has_item = &interactHasItem,
        .interact_get_item = &interactGetItem,
        .interact_has_block = &interactHasBlock,
        .interact_get_block = &interactGetBlock,
        .interact_get_block_face = &interactGetBlockFace,
        .interact_actor_get_actor = &interactActorGetActor,
        .actor_damage_get_damage = &actorDamageGetDamage,
        .actor_damage_set_damage = &actorDamageSetDamage,
        .event_get_damage_source = &eventGetDamageSource,
        .actor_explode_get_location = &actorExplodeGetLocation,
        .actor_explode_get_block_count = &actorExplodeGetBlockCount,
        .actor_explode_get_block = &actorExplodeGetBlock,
        .actor_knockback_get_source = &actorKnockbackGetSource,
        .actor_knockback_get_vector = &actorKnockbackGetVector,
        .actor_knockback_set_vector = &actorKnockbackSetVector,
        .death_get_message = &deathGetMessage,
        .death_set_message = &deathSetMessage,
        .bed_get_bed = &bedGetBed,
        .dim_change_get_from = &dimChangeGetFrom,
        .dim_change_get_to = &dimChangeGetTo,
        .drop_get_item = &dropGetItem,
        .emote_get_id = &emoteGetId,
        .emote_is_muted = &emoteIsMuted,
        .emote_set_muted = &emoteSetMuted,
        .gm_change_get_new_mode = &gmChangeGetNewMode,
        .consume_get_item = &consumeGetItem,
        .consume_get_hand = &consumeGetHand,
        .held_get_previous_slot = &heldGetPreviousSlot,
        .held_get_new_slot = &heldGetNewSlot,
        .join_get_message = &joinGetMessage,
        .join_set_message = &joinSetMessage,
        .quit_get_message = &quitGetMessage,
        .quit_set_message = &quitSetMessage,
        .kick_get_reason = &kickGetReason,
        .kick_set_reason = &kickSetReason,
        .login_get_kick_message = &loginGetKickMessage,
        .login_set_kick_message = &loginSetKickMessage,
        .pickup_get_item = &pickupGetItem,
        .skin_change_get_new_skin_id = &skinChangeGetNewSkinId,
        .skin_change_get_new_skin_cape_id = &skinChangeGetNewSkinCapeId,
        .skin_change_get_message = &skinChangeGetMessage,
        .skin_change_set_message = &skinChangeSetMessage,
        .cook_get_source = &cookGetSource,
        .cook_get_result = &cookGetResult,
        .block_explode_get_block_count = &blockExplodeGetBlockCount,
        .block_explode_get_block = &blockExplodeGetBlock,
        .grow_get_new_state = &growGetNewState,
        .from_to_get_to_block = &fromToGetToBlock,
        .piston_get_direction = &pistonGetDirection,
        .place_get_placed_state = &placeGetPlacedState,
        .place_get_against = &placeGetAgainst,
        .chunk_get_x = &chunkGetX,
        .chunk_get_z = &chunkGetZ,
        .chunk_get_dimension_name = &chunkGetDimensionName,
        .broadcast_get_message = &broadcastGetMessage,
        .broadcast_set_message = &broadcastSetMessage,
        .broadcast_get_recipient_count = &broadcastGetRecipientCount,
        .packet_get_id = &packetGetId,
        .packet_get_payload = &packetGetPayload,
        .packet_set_payload = &packetSetPayload,
        .packet_get_player = &packetGetPlayer,
        .packet_get_address = &packetGetAddress,
        .packet_get_sub_client_id = &packetGetSubClientId,
        .plugin_event_get_plugin_name = &pluginEventGetPluginName,
        .script_get_message_id = &scriptGetMessageId,
        .script_get_message = &scriptGetMessage,
        .script_get_sender_name = &scriptGetSenderName,
        .ping_get_address = &pingGetAddress,
        .ping_get_server_guid = &pingGetServerGuid,
        .ping_set_server_guid = &pingSetServerGuid,
        .ping_get_local_port = &pingGetLocalPort,
        .ping_set_local_port = &pingSetLocalPort,
        .ping_get_local_port_v6 = &pingGetLocalPortV6,
        .ping_set_local_port_v6 = &pingSetLocalPortV6,
        .ping_get_motd = &pingGetMotd,
        .ping_set_motd = &pingSetMotd,
        .ping_get_network_protocol_version = &pingGetNetworkProtocolVersion,
        .ping_get_minecraft_version_network = &pingGetMinecraftVersionNetwork,
        .ping_set_minecraft_version_network = &pingSetMinecraftVersionNetwork,
        .ping_get_num_players = &pingGetNumPlayers,
        .ping_set_num_players = &pingSetNumPlayers,
        .ping_get_max_players = &pingGetMaxPlayers,
        .ping_set_max_players = &pingSetMaxPlayers,
        .ping_get_level_name = &pingGetLevelName,
        .ping_set_level_name = &pingSetLevelName,
        .ping_get_game_mode = &pingGetGameMode,
        .ping_set_game_mode = &pingSetGameMode,
        .server_load_get_type = &serverLoadGetType,
        .thunder_change_get_to = &thunderChangeGetTo,
        .weather_change_get_to = &weatherChangeGetTo,
        .actor_get_type = &actorGetType,
        .actor_get_runtime_id = &actorGetRuntimeId,
        .actor_get_location = &actorGetLocation,
        .actor_get_velocity = &actorGetVelocity,
        .actor_is_on_ground = &actorIsOnGround,
        .actor_is_in_water = &actorIsInWater,
        .actor_is_in_lava = &actorIsInLava,
        .actor_is_dead = &actorIsDead,
        .actor_is_valid = &actorIsValid,
        .actor_get_dimension_name = &actorGetDimensionName,
        .actor_get_name_tag = &actorGetNameTag,
        .actor_get_score_tag = &actorGetScoreTag,
        .actor_get_id = &actorGetId,
        .actor_set_rotation = &actorSetRotation,
        .actor_teleport_location = &actorTeleportLocation,
        .actor_teleport_actor = &actorTeleportActor,
        .actor_remove = &actorRemove,
        .actor_send_message = &actorSendMessage,
        .actor_get_name = &actorGetName,
        .actor_get_scoreboard_tag_count = &actorGetScoreboardTagCount,
        .actor_get_scoreboard_tag = &actorGetScoreboardTag,
        .actor_add_scoreboard_tag = &actorAddScoreboardTag,
        .actor_remove_scoreboard_tag = &actorRemoveScoreboardTag,
        .actor_is_name_tag_visible = &actorIsNameTagVisible,
        .actor_set_name_tag_visible = &actorSetNameTagVisible,
        .actor_is_name_tag_always_visible = &actorIsNameTagAlwaysVisible,
        .actor_set_name_tag_always_visible = &actorSetNameTagAlwaysVisible,
        .actor_set_name_tag = &actorSetNameTag,
        .actor_set_score_tag = &actorSetScoreTag,
        .mob_get_health = &mobGetHealth,
        .mob_set_health = &mobSetHealth,
        .mob_get_max_health = &mobGetMaxHealth,
        .mob_set_max_health = &mobSetMaxHealth,
        .mob_is_gliding = &mobIsGliding,
        .actor_as_mob = &actorAsMob,
        .actor_get_dimension = &actorGetDimension,
        .dimension_get_name = &dimensionGetName,
        .dimension_get_block_at = &dimensionGetBlockAt,
        .actor_spawn_actor = &actorSpawnActor,
        .item_get_type = &itemGetType,
        .item_get_amount = &itemGetAmount,
        .item_get_data = &itemGetData,
        .item_get_max_stack_size = &itemGetMaxStackSize,
        .item_get_translation_key = &itemGetTranslationKey,
        .item_actor_get_type = &itemActorGetType,
        .item_actor_get_amount = &itemActorGetAmount,
        .item_actor_get_translation_key = &itemActorGetTranslationKey,
        .item_has_display_name = &itemHasDisplayName,
        .item_get_display_name = &itemGetDisplayName,
        .item_has_lore = &itemHasLore,
        .item_get_lore_count = &itemGetLoreCount,
        .item_get_lore_line = &itemGetLoreLine,
        .item_has_damage = &itemHasDamage,
        .item_get_damage = &itemGetDamage,
        .item_is_unbreakable = &itemIsUnbreakable,
        .item_has_enchants = &itemHasEnchants,
        .item_get_enchant_count = &itemGetEnchantCount,
        .item_get_enchant_name = &itemGetEnchantName,
        .item_get_enchant_level = &itemGetEnchantLevel,
        .item_has_enchant = &itemHasEnchant,
        .item_get_enchant_level_by_id = &itemGetEnchantLevelById,
        .item_add_enchant = &itemAddEnchant,
        .item_remove_enchant = &itemRemoveEnchant,
        .item_remove_enchants = &itemRemoveEnchants,
        .item_has_conflicting_enchant = &itemHasConflictingEnchant,
        .block_get_type = &blockGetType,
        .block_get_x = &blockGetX,
        .block_get_y = &blockGetY,
        .block_get_z = &blockGetZ,
        .block_set_type = &blockSetType,
        .block_set_type_physics = &blockSetTypePhysics,
        .block_get_location = &blockGetLocation,
        .block_get_dimension_name = &blockGetDimensionName,
        .block_get_relative = &blockGetRelative,
        .block_capture_state = &blockCaptureState,
        .block_delete = &blockDelete,
        .block_state_get_type = &blockStateGetType,
        .block_state_get_x = &blockStateGetX,
        .block_state_get_y = &blockStateGetY,
        .block_state_get_z = &blockStateGetZ,
        .block_state_set_type = &blockStateSetType,
        .block_state_get_location = &blockStateGetLocation,
        .block_state_update = &blockStateUpdate,
        .block_state_update_force = &blockStateUpdateForce,
        .block_state_update_force_physics = &blockStateUpdateForcePhysics,
        .block_state_delete = &blockStateDelete,
        .damage_source_get_type = &damageSourceGetType,
        .damage_source_get_actor = &damageSourceGetActor,
        .damage_source_get_damaging_actor = &damageSourceGetDamagingActor,
        .damage_source_is_indirect = &damageSourceIsIndirect,
        .enchant_get_by_id = &enchantGetById,
        .enchant_get_id = &enchantGetId,
        .enchant_get_max_level = &enchantGetMaxLevel,
        .enchant_get_start_level = &enchantGetStartLevel,
        .enchant_conflicts_with = &enchantConflictsWith,
        .enchant_can_enchant_item = &enchantCanEnchantItem,
        .sender_get_name = &senderGetName,
        .sender_send_message = &senderSendMessage,
        .sender_send_error_message = &senderSendErrorMessage,
        .sender_has_permission = &senderHasPermission,
        .sender_as_player = &senderAsPlayer,
        .form_create = &formCreate,
        .form_set_title = &formSetTitle,
        .form_set_content = &formSetContent,
        .form_set_button1 = &formSetButton1,
        .form_set_button2 = &formSetButton2,
        .form_add_button = &formAddButton,
        .form_add_control = &formAddControl,
        .form_set_submit_button = &formSetSubmitButton,
        .form_set_icon = &formSetIcon,
        .form_set_callbacks = &formSetCallbacks,
        .form_send = &formSend,
        .form_destroy = &formDestroy,
        .server_create_boss_bar = &serverCreateBossBar,
        .boss_bar_get_title = &bossBarGetTitle,
        .boss_bar_set_title = &bossBarSetTitle,
        .boss_bar_get_color = &bossBarGetColor,
        .boss_bar_set_color = &bossBarSetColor,
        .boss_bar_get_style = &bossBarGetStyle,
        .boss_bar_set_style = &bossBarSetStyle,
        .boss_bar_has_flag = &bossBarHasFlag,
        .boss_bar_add_flag = &bossBarAddFlag,
        .boss_bar_remove_flag = &bossBarRemoveFlag,
        .boss_bar_get_progress = &bossBarGetProgress,
        .boss_bar_set_progress = &bossBarSetProgress,
        .boss_bar_is_visible = &bossBarIsVisible,
        .boss_bar_set_visible = &bossBarSetVisible,
        .boss_bar_add_player = &bossBarAddPlayer,
        .boss_bar_remove_player = &bossBarRemovePlayer,
        .boss_bar_remove_all = &bossBarRemoveAll,
        .boss_bar_get_player_count = &bossBarGetPlayerCount,
        .boss_bar_get_player = &bossBarGetPlayer,
        .boss_bar_destroy = &bossBarDestroy,
        .server_get_level = &serverGetLevel,
        .level_get_name = &levelGetName,
        .level_get_time = &levelGetTime,
        .level_set_time = &levelSetTime,
        .level_get_seed = &levelGetSeed,
        .level_get_actors = &levelGetActors,
        .level_get_dimensions = &levelGetDimensions,
        .level_get_dimension_by_name = &levelGetDimensionByName,
        .dimension_get_type = &dimensionGetType,
        .dimension_get_level = &dimensionGetLevel,
        .dimension_get_highest_block_y_at = &dimensionGetHighestBlockYAt,
        .dimension_get_highest_block_at = &dimensionGetHighestBlockAt,
        .dimension_get_loaded_chunks = &dimensionGetLoadedChunks,
        .dimension_get_actors = &dimensionGetActors,
        .dimension_spawn_actor = &dimensionSpawnActor,
        .dimension_drop_item = &dimensionDropItem,
        .chunk_obj_get_x = &chunkObjGetX,
        .chunk_obj_get_z = &chunkObjGetZ,
        .chunk_obj_get_dimension = &chunkObjGetDimension,
        .chunk_obj_delete = &chunkObjDelete,
        .item_stack_create = &itemStackCreate,
        .item_stack_delete = &itemStackDelete,
        .server_get_map = &serverGetMap,
        .server_create_map = &serverCreateMap,
        .map_get_id = &mapGetId,
        .map_is_virtual = &mapIsVirtual,
        .map_get_scale = &mapGetScale,
        .map_set_scale = &mapSetScale,
        .map_get_center_x = &mapGetCenterX,
        .map_get_center_z = &mapGetCenterZ,
        .map_set_center_x = &mapSetCenterX,
        .map_set_center_z = &mapSetCenterZ,
        .map_get_dimension = &mapGetDimension,
        .map_set_dimension = &mapSetDimension,
        .map_is_unlimited_tracking = &mapIsUnlimitedTracking,
        .map_set_unlimited_tracking = &mapSetUnlimitedTracking,
        .map_is_locked = &mapIsLocked,
        .map_set_locked = &mapSetLocked,
        .player_send_map = &playerSendMap,
        .map_renderer_create = &mapRendererCreate,
        .map_renderer_destroy = &mapRendererDestroy,
        .map_add_renderer = &mapAddRenderer,
        .map_remove_renderer = &mapRemoveRenderer,
        .map_get_renderer_count = &mapGetRendererCount,
        .map_get_renderer = &mapGetRenderer,
        .canvas_get_map_view = &canvasGetMapView,
        .canvas_get_cursor_count = &canvasGetCursorCount,
        .canvas_get_cursor = &canvasGetCursor,
        .canvas_get_cursor_caption = &canvasGetCursorCaption,
        .canvas_set_cursors = &canvasSetCursors,
        .canvas_set_pixel_color = &canvasSetPixelColor,
        .canvas_get_pixel_color = &canvasGetPixelColor,
        .canvas_get_base_pixel_color = &canvasGetBasePixelColor,
        .canvas_set_pixel = &canvasSetPixel,
        .canvas_get_pixel = &canvasGetPixel,
        .canvas_get_base_pixel = &canvasGetBasePixel,
        .item_has_map_view = &itemHasMapView,
        .item_get_map_view = &itemGetMapView,
        .item_set_map_view = &itemSetMapView,
        .player_get_inventory = &playerGetInventory,
        .player_get_ender_chest = &playerGetEnderChest,
        .inventory_get_size = &inventoryGetSize,
        .inventory_get_max_stack_size = &inventoryGetMaxStackSize,
        .inventory_get_item = &inventoryGetItem,
        .inventory_set_item = &inventorySetItem,
        .inventory_add_item = &inventoryAddItem,
        .inventory_remove_item = &inventoryRemoveItem,
        .inventory_contains = &inventoryContains,
        .inventory_is_empty = &inventoryIsEmpty,
        .inventory_first_empty = &inventoryFirstEmpty,
        .inventory_clear = &inventoryClear,
        .inventory_first = &inventoryFirst,
        .inventory_get_item_in_main_hand = &inventoryGetItemInMainHand,
        .inventory_set_item_in_main_hand = &inventorySetItemInMainHand,
        .inventory_get_item_in_off_hand = &inventoryGetItemInOffHand,
        .inventory_set_item_in_off_hand = &inventorySetItemInOffHand,
        .inventory_get_helmet = &inventoryGetHelmet,
        .inventory_set_helmet = &inventorySetHelmet,
        .inventory_get_chestplate = &inventoryGetChestplate,
        .inventory_set_chestplate = &inventorySetChestplate,
        .inventory_get_leggings = &inventoryGetLeggings,
        .inventory_set_leggings = &inventorySetLeggings,
        .inventory_get_boots = &inventoryGetBoots,
        .inventory_set_boots = &inventorySetBoots,
        .inventory_get_held_item_slot = &inventoryGetHeldItemSlot,
        .inventory_set_held_item_slot = &inventorySetHeldItemSlot,
        .plugin_register_event = nullptr,  // filled by DotNetPluginLoader
        .map_render_callback = nullptr,  // filled by DotNetPluginLoader
        .server_get_scheduler = &serverGetScheduler,
        .scheduler_run_task = &schedulerRunTask,
        .scheduler_cancel_task = &schedulerCancelTask,
        .scheduler_cancel_tasks = &schedulerCancelTasks,
        .scheduler_is_running = &schedulerIsRunning,
        .scheduler_is_queued = &schedulerIsQueued,
        .scheduler_get_pending_tasks = &schedulerGetPendingTasks,
        .task_get_id = &taskGetId,
        .task_is_sync = &taskIsSync,
        .task_is_cancelled = &taskIsCancelled,
        .scheduler_task_callback = nullptr,  // filled by DotNetPluginLoader
        .server_get_service_manager = &serverGetServiceManager,
        .service_provider_create = &serviceProviderCreate,
        .service_provider_get_ptr = &serviceProviderGetPtr,
        .service_provider_release = &serviceProviderRelease,
        .service_manager_register = &serviceManagerRegister,
        .service_manager_unregister_all = &serviceManagerUnregisterAll,
        .service_manager_unregister = &serviceManagerUnregister,
        .service_manager_unregister_provider = &serviceManagerUnregisterProvider,
        .service_manager_get = &serviceManagerGet,
    };
    return table;
}

BridgeTable &mutableBridgeTable() { return const_cast<BridgeTable &>(getBridgeTable()); }

}  // namespace dotnet_loader