using System.Runtime.InteropServices;
using System.Text;

namespace Endstone.Loader;

/// <summary>
/// Native function table provided by the C++ loader (see bridge.h).
/// Field order MUST match the C++ BridgeTable declaration. Returned char*
/// values are copied into managed strings immediately (C++ uses a
/// thread-local buffer valid until the next call).
/// </summary>
internal static unsafe class Bridge
{
#pragma warning disable CS0649  // table fields are populated by the native side
    internal struct Table
    {
        // ---- player ----
        public delegate* unmanaged[Cdecl]<void*, byte*> PlayerGetName;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> PlayerSendMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> PlayerSendErrorMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> PlayerSendPopup;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> PlayerSendTip;
        public delegate* unmanaged[Cdecl]<void*, byte*, byte*, void> PlayerSendToast;
        public delegate* unmanaged[Cdecl]<void*, byte*, byte*, void> PlayerSendTitle;
        public delegate* unmanaged[Cdecl]<void*, void> PlayerResetTitle;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> PlayerKick;
        public delegate* unmanaged[Cdecl]<void*, byte*, bool> PlayerPerformCommand;
        public delegate* unmanaged[Cdecl]<void*, bool> PlayerIsOp;
        public delegate* unmanaged[Cdecl]<void*, bool, void> PlayerSetOp;
        public delegate* unmanaged[Cdecl]<void*, byte*> PlayerGetXuid;
        public delegate* unmanaged[Cdecl]<void*, byte*> PlayerGetAddress;
        public delegate* unmanaged[Cdecl]<void*, bool> PlayerIsSneaking;
        public delegate* unmanaged[Cdecl]<void*, bool, void> PlayerSetSneaking;
        public delegate* unmanaged[Cdecl]<void*, bool> PlayerIsSprinting;
        public delegate* unmanaged[Cdecl]<void*, bool, void> PlayerSetSprinting;
        public delegate* unmanaged[Cdecl]<void*, int> PlayerGetPing;
        public delegate* unmanaged[Cdecl]<void*, byte*> PlayerGetLocale;
        public delegate* unmanaged[Cdecl]<void*, byte*> PlayerGetDeviceOS;
        public delegate* unmanaged[Cdecl]<void*, byte*> PlayerGetDeviceId;
        public delegate* unmanaged[Cdecl]<void*, byte*> PlayerGetGameVersion;
        public delegate* unmanaged[Cdecl]<void*, int> PlayerGetGameMode;
        public delegate* unmanaged[Cdecl]<void*, int, void> PlayerSetGameMode;
        public delegate* unmanaged[Cdecl]<void*, bool> PlayerGetAllowFlight;
        public delegate* unmanaged[Cdecl]<void*, bool, void> PlayerSetAllowFlight;
        public delegate* unmanaged[Cdecl]<void*, bool> PlayerIsFlying;
        public delegate* unmanaged[Cdecl]<void*, bool, void> PlayerSetFlying;
        public delegate* unmanaged[Cdecl]<void*, int> PlayerGetExpLevel;
        public delegate* unmanaged[Cdecl]<void*, int, void> PlayerSetExpLevel;
        public delegate* unmanaged[Cdecl]<void*, int, void> PlayerGiveExp;
        public delegate* unmanaged[Cdecl]<void*, int, void> PlayerGiveExpLevels;
        public delegate* unmanaged[Cdecl]<void*, float> PlayerGetExpProgress;
        public delegate* unmanaged[Cdecl]<void*, float, void> PlayerSetExpProgress;
        public delegate* unmanaged[Cdecl]<void*, int> PlayerGetTotalExp;
        public delegate* unmanaged[Cdecl]<void*, byte*, int, void> PlayerTransfer;
        public delegate* unmanaged[Cdecl]<void*, float*, byte*, float, float, void> PlayerPlaySound;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> PlayerStopSound;
        public delegate* unmanaged[Cdecl]<void*, void> PlayerStopAllSounds;
        public delegate* unmanaged[Cdecl]<void*, byte*, float*, byte*, void> PlayerSpawnParticle;
        public delegate* unmanaged[Cdecl]<void*, float> PlayerGetFlySpeed;
        public delegate* unmanaged[Cdecl]<void*, float, void> PlayerSetFlySpeed;
        public delegate* unmanaged[Cdecl]<void*, float> PlayerGetWalkSpeed;
        public delegate* unmanaged[Cdecl]<void*, float, void> PlayerSetWalkSpeed;
        public delegate* unmanaged[Cdecl]<void*, void> PlayerUpdateCommands;
        public delegate* unmanaged[Cdecl]<void*, void> PlayerCloseForm;
        public delegate* unmanaged[Cdecl]<void*, int, void*, int, void> PlayerSendPacket;
        public delegate* unmanaged[Cdecl]<void*, byte*> PlayerGetSkinId;
        public delegate* unmanaged[Cdecl]<void*, byte*> PlayerGetSkinCapeId;
        public delegate* unmanaged[Cdecl]<void*, void*> PlayerGetItemInHand;

        // ---- server ----
        public delegate* unmanaged[Cdecl]<void*, byte*> ServerGetName;
        public delegate* unmanaged[Cdecl]<void*, byte*> ServerGetVersion;
        public delegate* unmanaged[Cdecl]<void*, byte*> ServerGetMinecraftVersion;
        public delegate* unmanaged[Cdecl]<void*, int> ServerGetProtocolVersion;
        public delegate* unmanaged[Cdecl]<void*, int> ServerGetMaxPlayers;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> ServerBroadcastMessage;
        public delegate* unmanaged[Cdecl]<void*, void**, int, int> ServerGetOnlinePlayers;
        public delegate* unmanaged[Cdecl]<void*, byte*, void*> ServerGetPlayer;
        public delegate* unmanaged[Cdecl]<void*, void*> ServerGetConsoleSender;
        public delegate* unmanaged[Cdecl]<void*, void*, byte*, bool> ServerDispatchCommand;

        // ---- events: common ----
        public delegate* unmanaged[Cdecl]<void*, int, void*> EventGetPlayer;
        public delegate* unmanaged[Cdecl]<void*, int, void*> EventGetActor;
        public delegate* unmanaged[Cdecl]<void*, int, bool> EventIsCancelled;
        public delegate* unmanaged[Cdecl]<void*, int, bool, void> EventSetCancelled;

        // ---- events: chat/command ----
        public delegate* unmanaged[Cdecl]<void*, byte*> ChatGetMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> ChatSetMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*> ChatGetFormat;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> ChatSetFormat;
        public delegate* unmanaged[Cdecl]<void*, int> ChatGetRecipientCount;
        public delegate* unmanaged[Cdecl]<void*, byte*> CommandGetCommand;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> CommandSetCommand;
        public delegate* unmanaged[Cdecl]<void*, byte*> ServerCmdGetCommand;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> ServerCmdSetCommand;
        public delegate* unmanaged[Cdecl]<void*, byte*> ServerCmdGetSenderName;
        public delegate* unmanaged[Cdecl]<void*, void*> ServerCmdGetSender;

        // ---- events: movement / teleport ----
        public delegate* unmanaged[Cdecl]<void*, float*, void> MoveGetFrom;
        public delegate* unmanaged[Cdecl]<void*, float*, void> MoveGetTo;
        public delegate* unmanaged[Cdecl]<void*, float*, void> MoveSetFrom;
        public delegate* unmanaged[Cdecl]<void*, float*, void> MoveSetTo;
        public delegate* unmanaged[Cdecl]<void*, float*, void> ActorTpGetFrom;
        public delegate* unmanaged[Cdecl]<void*, float*, void> ActorTpGetTo;
        public delegate* unmanaged[Cdecl]<void*, float*, void> ActorTpSetFrom;
        public delegate* unmanaged[Cdecl]<void*, float*, void> ActorTpSetTo;

        // ---- events: interact ----
        public delegate* unmanaged[Cdecl]<void*, int> InteractGetAction;
        public delegate* unmanaged[Cdecl]<void*, float*, int> InteractGetClickedPosition;
        public delegate* unmanaged[Cdecl]<void*, bool> InteractHasItem;
        public delegate* unmanaged[Cdecl]<void*, void*> InteractGetItem;
        public delegate* unmanaged[Cdecl]<void*, bool> InteractHasBlock;
        public delegate* unmanaged[Cdecl]<void*, void*> InteractGetBlock;
        public delegate* unmanaged[Cdecl]<void*, int> InteractGetBlockFace;
        public delegate* unmanaged[Cdecl]<void*, void*> InteractActorGetActor;

        // ---- events: actor ----
        public delegate* unmanaged[Cdecl]<void*, float> ActorDamageGetDamage;
        public delegate* unmanaged[Cdecl]<void*, float, void> ActorDamageSetDamage;
        public delegate* unmanaged[Cdecl]<void*, int, void*> EventGetDamageSource;
        public delegate* unmanaged[Cdecl]<void*, float*, void> ActorExplodeGetLocation;
        public delegate* unmanaged[Cdecl]<void*, int> ActorExplodeGetBlockCount;
        public delegate* unmanaged[Cdecl]<void*, int, void*> ActorExplodeGetBlock;
        public delegate* unmanaged[Cdecl]<void*, void*> ActorKnockbackGetSource;
        public delegate* unmanaged[Cdecl]<void*, float*, void> ActorKnockbackGetVector;
        public delegate* unmanaged[Cdecl]<void*, float*, void> ActorKnockbackSetVector;

        // ---- events: player ----
        public delegate* unmanaged[Cdecl]<void*, byte*> DeathGetMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> DeathSetMessage;
        public delegate* unmanaged[Cdecl]<void*, int, void*> BedGetBed;
        public delegate* unmanaged[Cdecl]<void*, byte*> DimChangeGetFrom;
        public delegate* unmanaged[Cdecl]<void*, byte*> DimChangeGetTo;
        public delegate* unmanaged[Cdecl]<void*, void*> DropGetItem;
        public delegate* unmanaged[Cdecl]<void*, byte*> EmoteGetId;
        public delegate* unmanaged[Cdecl]<void*, bool> EmoteIsMuted;
        public delegate* unmanaged[Cdecl]<void*, bool, void> EmoteSetMuted;
        public delegate* unmanaged[Cdecl]<void*, int> GmChangeGetNewMode;
        public delegate* unmanaged[Cdecl]<void*, void*> ConsumeGetItem;
        public delegate* unmanaged[Cdecl]<void*, int> ConsumeGetHand;
        public delegate* unmanaged[Cdecl]<void*, int> HeldGetPreviousSlot;
        public delegate* unmanaged[Cdecl]<void*, int> HeldGetNewSlot;
        public delegate* unmanaged[Cdecl]<void*, byte*> JoinGetMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> JoinSetMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*> QuitGetMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> QuitSetMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*> KickGetReason;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> KickSetReason;
        public delegate* unmanaged[Cdecl]<void*, byte*> LoginGetKickMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> LoginSetKickMessage;
        public delegate* unmanaged[Cdecl]<void*, void*> PickupGetItem;
        public delegate* unmanaged[Cdecl]<void*, byte*> SkinChangeGetNewSkinId;
        public delegate* unmanaged[Cdecl]<void*, byte*> SkinChangeGetNewSkinCapeId;
        public delegate* unmanaged[Cdecl]<void*, byte*> SkinChangeGetMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> SkinChangeSetMessage;

        // ---- events: block ----
        public delegate* unmanaged[Cdecl]<void*, void*> CookGetSource;
        public delegate* unmanaged[Cdecl]<void*, void*> CookGetResult;
        public delegate* unmanaged[Cdecl]<void*, int> BlockExplodeGetBlockCount;
        public delegate* unmanaged[Cdecl]<void*, int, void*> BlockExplodeGetBlock;
        public delegate* unmanaged[Cdecl]<void*, int, void*> GrowGetNewState;
        public delegate* unmanaged[Cdecl]<void*, void*> FromToGetToBlock;
        public delegate* unmanaged[Cdecl]<void*, int> PistonGetDirection;
        public delegate* unmanaged[Cdecl]<void*, void*> PlaceGetPlacedState;
        public delegate* unmanaged[Cdecl]<void*, void*> PlaceGetAgainst;

        // ---- events: chunk ----
        public delegate* unmanaged[Cdecl]<void*, int, int> ChunkGetX;
        public delegate* unmanaged[Cdecl]<void*, int, int> ChunkGetZ;
        public delegate* unmanaged[Cdecl]<void*, int, byte*> ChunkGetDimensionName;

        // ---- events: server ----
        public delegate* unmanaged[Cdecl]<void*, byte*> BroadcastGetMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> BroadcastSetMessage;
        public delegate* unmanaged[Cdecl]<void*, int> BroadcastGetRecipientCount;
        public delegate* unmanaged[Cdecl]<void*, int, int> PacketGetId;
        public delegate* unmanaged[Cdecl]<void*, int, int*, byte*> PacketGetPayload;
        public delegate* unmanaged[Cdecl]<void*, int, void*, int, void> PacketSetPayload;
        public delegate* unmanaged[Cdecl]<void*, int, void*> PacketGetPlayer;
        public delegate* unmanaged[Cdecl]<void*, int, byte*> PacketGetAddress;
        public delegate* unmanaged[Cdecl]<void*, int, int> PacketGetSubClientId;
        public delegate* unmanaged[Cdecl]<void*, int, byte*> PluginEventGetPluginName;
        public delegate* unmanaged[Cdecl]<void*, byte*> ScriptGetMessageId;
        public delegate* unmanaged[Cdecl]<void*, byte*> ScriptGetMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*> ScriptGetSenderName;
        public delegate* unmanaged[Cdecl]<void*, byte*> PingGetAddress;
        public delegate* unmanaged[Cdecl]<void*, byte*> PingGetServerGuid;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> PingSetServerGuid;
        public delegate* unmanaged[Cdecl]<void*, int> PingGetLocalPort;
        public delegate* unmanaged[Cdecl]<void*, int, void> PingSetLocalPort;
        public delegate* unmanaged[Cdecl]<void*, int> PingGetLocalPortV6;
        public delegate* unmanaged[Cdecl]<void*, int, void> PingSetLocalPortV6;
        public delegate* unmanaged[Cdecl]<void*, byte*> PingGetMotd;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> PingSetMotd;
        public delegate* unmanaged[Cdecl]<void*, int> PingGetNetworkProtocolVersion;
        public delegate* unmanaged[Cdecl]<void*, byte*> PingGetMinecraftVersionNetwork;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> PingSetMinecraftVersionNetwork;
        public delegate* unmanaged[Cdecl]<void*, int> PingGetNumPlayers;
        public delegate* unmanaged[Cdecl]<void*, int, void> PingSetNumPlayers;
        public delegate* unmanaged[Cdecl]<void*, int> PingGetMaxPlayers;
        public delegate* unmanaged[Cdecl]<void*, int, void> PingSetMaxPlayers;
        public delegate* unmanaged[Cdecl]<void*, byte*> PingGetLevelName;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> PingSetLevelName;
        public delegate* unmanaged[Cdecl]<void*, int> PingGetGameMode;
        public delegate* unmanaged[Cdecl]<void*, int, void> PingSetGameMode;
        public delegate* unmanaged[Cdecl]<void*, int> ServerLoadGetType;
        public delegate* unmanaged[Cdecl]<void*, bool> ThunderChangeGetTo;
        public delegate* unmanaged[Cdecl]<void*, bool> WeatherChangeGetTo;

        // ---- objects: actor / mob ----
        public delegate* unmanaged[Cdecl]<void*, byte*> ActorGetType;
        public delegate* unmanaged[Cdecl]<void*, ulong> ActorGetRuntimeId;
        public delegate* unmanaged[Cdecl]<void*, float*, void> ActorGetLocation;
        public delegate* unmanaged[Cdecl]<void*, float*, void> ActorGetVelocity;
        public delegate* unmanaged[Cdecl]<void*, bool> ActorIsOnGround;
        public delegate* unmanaged[Cdecl]<void*, bool> ActorIsInWater;
        public delegate* unmanaged[Cdecl]<void*, bool> ActorIsInLava;
        public delegate* unmanaged[Cdecl]<void*, bool> ActorIsDead;
        public delegate* unmanaged[Cdecl]<void*, bool> ActorIsValid;
        public delegate* unmanaged[Cdecl]<void*, byte*> ActorGetDimensionName;
        public delegate* unmanaged[Cdecl]<void*, byte*> ActorGetNameTag;
        public delegate* unmanaged[Cdecl]<void*, byte*> ActorGetScoreTag;
        public delegate* unmanaged[Cdecl]<void*, long> ActorGetId;
        public delegate* unmanaged[Cdecl]<void*, float, float, void> ActorSetRotation;
        public delegate* unmanaged[Cdecl]<void*, float*, bool> ActorTeleportLocation;
        public delegate* unmanaged[Cdecl]<void*, void*, bool> ActorTeleportActor;
        public delegate* unmanaged[Cdecl]<void*, void> ActorRemove;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> ActorSendMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*> ActorGetName;
        public delegate* unmanaged[Cdecl]<void*, int> ActorGetScoreboardTagCount;
        public delegate* unmanaged[Cdecl]<void*, int, byte*> ActorGetScoreboardTag;
        public delegate* unmanaged[Cdecl]<void*, byte*, bool> ActorAddScoreboardTag;
        public delegate* unmanaged[Cdecl]<void*, byte*, bool> ActorRemoveScoreboardTag;
        public delegate* unmanaged[Cdecl]<void*, bool> ActorIsNameTagVisible;
        public delegate* unmanaged[Cdecl]<void*, bool, void> ActorSetNameTagVisible;
        public delegate* unmanaged[Cdecl]<void*, bool> ActorIsNameTagAlwaysVisible;
        public delegate* unmanaged[Cdecl]<void*, bool, void> ActorSetNameTagAlwaysVisible;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> ActorSetNameTag;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> ActorSetScoreTag;
        public delegate* unmanaged[Cdecl]<void*, int> MobGetHealth;
        public delegate* unmanaged[Cdecl]<void*, int, void> MobSetHealth;
        public delegate* unmanaged[Cdecl]<void*, int> MobGetMaxHealth;
        public delegate* unmanaged[Cdecl]<void*, int, void> MobSetMaxHealth;
        public delegate* unmanaged[Cdecl]<void*, bool> MobIsGliding;
        public delegate* unmanaged[Cdecl]<void*, void*> ActorAsMob;
        public delegate* unmanaged[Cdecl]<void*, void*> ActorGetDimension;
        public delegate* unmanaged[Cdecl]<void*, byte*> DimensionGetName;
        public delegate* unmanaged[Cdecl]<void*, int, int, int, void*> DimensionGetBlockAt;
        public delegate* unmanaged[Cdecl]<void*, float*, byte*, void*> ActorSpawnActor;

        // ---- objects: item / block / damage source ----
        public delegate* unmanaged[Cdecl]<void*, byte*> ItemGetType;
        public delegate* unmanaged[Cdecl]<void*, int> ItemGetAmount;
        public delegate* unmanaged[Cdecl]<void*, int> ItemGetData;
        public delegate* unmanaged[Cdecl]<void*, int> ItemGetMaxStackSize;
        public delegate* unmanaged[Cdecl]<void*, byte*> ItemGetTranslationKey;
        public delegate* unmanaged[Cdecl]<void*, byte*> ItemActorGetType;
        public delegate* unmanaged[Cdecl]<void*, int> ItemActorGetAmount;
        public delegate* unmanaged[Cdecl]<void*, byte*> ItemActorGetTranslationKey;
        public delegate* unmanaged[Cdecl]<void*, bool> ItemHasDisplayName;
        public delegate* unmanaged[Cdecl]<void*, byte*> ItemGetDisplayName;
        public delegate* unmanaged[Cdecl]<void*, bool> ItemHasLore;
        public delegate* unmanaged[Cdecl]<void*, int> ItemGetLoreCount;
        public delegate* unmanaged[Cdecl]<void*, int, byte*> ItemGetLoreLine;
        public delegate* unmanaged[Cdecl]<void*, bool> ItemHasDamage;
        public delegate* unmanaged[Cdecl]<void*, int> ItemGetDamage;
        public delegate* unmanaged[Cdecl]<void*, bool> ItemIsUnbreakable;
        public delegate* unmanaged[Cdecl]<void*, bool> ItemHasEnchants;
        public delegate* unmanaged[Cdecl]<void*, int> ItemGetEnchantCount;
        public delegate* unmanaged[Cdecl]<void*, int, byte*> ItemGetEnchantName;
        public delegate* unmanaged[Cdecl]<void*, int, int> ItemGetEnchantLevel;
        public delegate* unmanaged[Cdecl]<void*, byte*, bool> ItemHasEnchant;
        public delegate* unmanaged[Cdecl]<void*, byte*, int> ItemGetEnchantLevelById;
        public delegate* unmanaged[Cdecl]<void*, byte*, int, bool, bool> ItemAddEnchant;
        public delegate* unmanaged[Cdecl]<void*, byte*, bool> ItemRemoveEnchant;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> ItemRemoveEnchants;
        public delegate* unmanaged[Cdecl]<void*, byte*, bool> ItemHasConflictingEnchant;
        public delegate* unmanaged[Cdecl]<void*, byte*> BlockGetType;
        public delegate* unmanaged[Cdecl]<void*, int> BlockGetX;
        public delegate* unmanaged[Cdecl]<void*, int> BlockGetY;
        public delegate* unmanaged[Cdecl]<void*, int> BlockGetZ;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> BlockSetType;
        public delegate* unmanaged[Cdecl]<void*, byte*, bool, void> BlockSetTypePhysics;
        public delegate* unmanaged[Cdecl]<void*, float*, void> BlockGetLocation;
        public delegate* unmanaged[Cdecl]<void*, byte*> BlockGetDimensionName;
        public delegate* unmanaged[Cdecl]<void*, int, int, int, void*> BlockGetRelative;
        public delegate* unmanaged[Cdecl]<void*, void*> BlockCaptureState;
        public delegate* unmanaged[Cdecl]<void*, void> BlockDelete;
        public delegate* unmanaged[Cdecl]<void*, byte*> BlockStateGetType;
        public delegate* unmanaged[Cdecl]<void*, int> BlockStateGetX;
        public delegate* unmanaged[Cdecl]<void*, int> BlockStateGetY;
        public delegate* unmanaged[Cdecl]<void*, int> BlockStateGetZ;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> BlockStateSetType;
        public delegate* unmanaged[Cdecl]<void*, float*, void> BlockStateGetLocation;
        public delegate* unmanaged[Cdecl]<void*, bool> BlockStateUpdate;
        public delegate* unmanaged[Cdecl]<void*, bool, bool> BlockStateUpdateForce;
        public delegate* unmanaged[Cdecl]<void*, bool, bool, bool> BlockStateUpdateForcePhysics;
        public delegate* unmanaged[Cdecl]<void*, void> BlockStateDelete;
        public delegate* unmanaged[Cdecl]<void*, byte*> DamageSourceGetType;
        public delegate* unmanaged[Cdecl]<void*, void*> DamageSourceGetActor;
        public delegate* unmanaged[Cdecl]<void*, void*> DamageSourceGetDamagingActor;
        public delegate* unmanaged[Cdecl]<void*, bool> DamageSourceIsIndirect;

        // ---- objects: enchantment ----
        public delegate* unmanaged[Cdecl]<byte*, void*> EnchantGetById;
        public delegate* unmanaged[Cdecl]<void*, byte*> EnchantGetId;
        public delegate* unmanaged[Cdecl]<void*, int> EnchantGetMaxLevel;
        public delegate* unmanaged[Cdecl]<void*, int> EnchantGetStartLevel;
        public delegate* unmanaged[Cdecl]<void*, void*, bool> EnchantConflictsWith;
        public delegate* unmanaged[Cdecl]<void*, void*, bool> EnchantCanEnchantItem;

        // ---- objects: sender ----
        public delegate* unmanaged[Cdecl]<void*, byte*> SenderGetName;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> SenderSendMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> SenderSendErrorMessage;
        public delegate* unmanaged[Cdecl]<void*, byte*, bool> SenderHasPermission;
        public delegate* unmanaged[Cdecl]<void*, void*> SenderAsPlayer;

        // ---- objects: form ----
        public delegate* unmanaged[Cdecl]<int, void*> FormCreate;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> FormSetTitle;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> FormSetContent;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> FormSetButton1;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> FormSetButton2;
        public delegate* unmanaged[Cdecl]<void*, byte*, byte*, void> FormAddButton;
        public delegate* unmanaged[Cdecl]<void*, int, byte*, byte*, byte*, void> FormAddControl;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> FormSetSubmitButton;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> FormSetIcon;
        public delegate* unmanaged[Cdecl]<void*, ulong, void> FormSetCallbacks;
        public delegate* unmanaged[Cdecl]<void*, void*, void> FormSend;
        public delegate* unmanaged[Cdecl]<void*, void> FormDestroy;
        public delegate* unmanaged[Cdecl]<void*, int, ulong, int, byte*, void> FormDispatchResult;

        // ---- objects: boss bar ----
        public delegate* unmanaged[Cdecl]<void*, byte*, int, int, int, void*> ServerCreateBossBar;
        public delegate* unmanaged[Cdecl]<void*, byte*> BossBarGetTitle;
        public delegate* unmanaged[Cdecl]<void*, byte*, void> BossBarSetTitle;
        public delegate* unmanaged[Cdecl]<void*, int> BossBarGetColor;
        public delegate* unmanaged[Cdecl]<void*, int, void> BossBarSetColor;
        public delegate* unmanaged[Cdecl]<void*, int> BossBarGetStyle;
        public delegate* unmanaged[Cdecl]<void*, int, void> BossBarSetStyle;
        public delegate* unmanaged[Cdecl]<void*, int, bool> BossBarHasFlag;
        public delegate* unmanaged[Cdecl]<void*, int, void> BossBarAddFlag;
        public delegate* unmanaged[Cdecl]<void*, int, void> BossBarRemoveFlag;
        public delegate* unmanaged[Cdecl]<void*, float> BossBarGetProgress;
        public delegate* unmanaged[Cdecl]<void*, float, void> BossBarSetProgress;
        public delegate* unmanaged[Cdecl]<void*, bool> BossBarIsVisible;
        public delegate* unmanaged[Cdecl]<void*, bool, void> BossBarSetVisible;
        public delegate* unmanaged[Cdecl]<void*, void*, void> BossBarAddPlayer;
        public delegate* unmanaged[Cdecl]<void*, void*, void> BossBarRemovePlayer;
        public delegate* unmanaged[Cdecl]<void*, void> BossBarRemoveAll;
        public delegate* unmanaged[Cdecl]<void*, int> BossBarGetPlayerCount;
        public delegate* unmanaged[Cdecl]<void*, int, void*> BossBarGetPlayer;
        public delegate* unmanaged[Cdecl]<void*, void> BossBarDestroy;

        // ---- objects: level ----
        public delegate* unmanaged[Cdecl]<void*, void*> ServerGetLevel;
        public delegate* unmanaged[Cdecl]<void*, byte*> LevelGetName;
        public delegate* unmanaged[Cdecl]<void*, int> LevelGetTime;
        public delegate* unmanaged[Cdecl]<void*, int, void> LevelSetTime;
        public delegate* unmanaged[Cdecl]<void*, long> LevelGetSeed;
        public delegate* unmanaged[Cdecl]<void*, void**, int, int> LevelGetActors;
        public delegate* unmanaged[Cdecl]<void*, void**, int, int> LevelGetDimensions;
        public delegate* unmanaged[Cdecl]<void*, byte*, void*> LevelGetDimensionByName;

        // ---- objects: dimension ----
        public delegate* unmanaged[Cdecl]<void*, int> DimensionGetType;
        public delegate* unmanaged[Cdecl]<void*, void*> DimensionGetLevel;
        public delegate* unmanaged[Cdecl]<void*, int, int, int> DimensionGetHighestBlockYAt;
        public delegate* unmanaged[Cdecl]<void*, int, int, void*> DimensionGetHighestBlockAt;
        public delegate* unmanaged[Cdecl]<void*, void**, int, int> DimensionGetLoadedChunks;
        public delegate* unmanaged[Cdecl]<void*, void**, int, int> DimensionGetActors;
        public delegate* unmanaged[Cdecl]<void*, float*, byte*, void*> DimensionSpawnActor;
        public delegate* unmanaged[Cdecl]<void*, float*, void*, void*> DimensionDropItem;

        // ---- objects: chunk / item stack ----
        public delegate* unmanaged[Cdecl]<void*, int> ChunkObjGetX;
        public delegate* unmanaged[Cdecl]<void*, int> ChunkObjGetZ;
        public delegate* unmanaged[Cdecl]<void*, void*> ChunkObjGetDimension;
        public delegate* unmanaged[Cdecl]<void*, void> ChunkObjDelete;
        public delegate* unmanaged[Cdecl]<byte*, int, int, void*> ItemStackCreate;
        public delegate* unmanaged[Cdecl]<void*, void> ItemStackDelete;

        // ---- objects: map ----
        public delegate* unmanaged[Cdecl]<void*, long, void*> ServerGetMap;
        public delegate* unmanaged[Cdecl]<void*, void*, void*> ServerCreateMap;
        public delegate* unmanaged[Cdecl]<void*, long> MapGetId;
        public delegate* unmanaged[Cdecl]<void*, bool> MapIsVirtual;
        public delegate* unmanaged[Cdecl]<void*, int> MapGetScale;
        public delegate* unmanaged[Cdecl]<void*, int, void> MapSetScale;
        public delegate* unmanaged[Cdecl]<void*, int> MapGetCenterX;
        public delegate* unmanaged[Cdecl]<void*, int> MapGetCenterZ;
        public delegate* unmanaged[Cdecl]<void*, int, void> MapSetCenterX;
        public delegate* unmanaged[Cdecl]<void*, int, void> MapSetCenterZ;
        public delegate* unmanaged[Cdecl]<void*, void*> MapGetDimension;
        public delegate* unmanaged[Cdecl]<void*, void*, void> MapSetDimension;
        public delegate* unmanaged[Cdecl]<void*, bool> MapIsUnlimitedTracking;
        public delegate* unmanaged[Cdecl]<void*, bool, void> MapSetUnlimitedTracking;
        public delegate* unmanaged[Cdecl]<void*, bool> MapIsLocked;
        public delegate* unmanaged[Cdecl]<void*, bool, void> MapSetLocked;
        public delegate* unmanaged[Cdecl]<void*, void*, void> PlayerSendMap;
        public delegate* unmanaged[Cdecl]<int, ulong, void*> MapRendererCreate;
        public delegate* unmanaged[Cdecl]<void*, void> MapRendererDestroy;
        public delegate* unmanaged[Cdecl]<void*, void*, void> MapAddRenderer;
        public delegate* unmanaged[Cdecl]<void*, void*, bool> MapRemoveRenderer;
        public delegate* unmanaged[Cdecl]<void*, int> MapGetRendererCount;
        public delegate* unmanaged[Cdecl]<void*, int, ulong*, int> MapGetRenderer;
        public delegate* unmanaged[Cdecl]<void*, void*> CanvasGetMapView;
        public delegate* unmanaged[Cdecl]<void*, int> CanvasGetCursorCount;
        public delegate* unmanaged[Cdecl]<void*, int, sbyte*, void> CanvasGetCursor;
        public delegate* unmanaged[Cdecl]<void*, int, byte*> CanvasGetCursorCaption;
        public delegate* unmanaged[Cdecl]<void*, sbyte*, int, byte**, void> CanvasSetCursors;
        public delegate* unmanaged[Cdecl]<void*, int, int, int, int, int, int, void> CanvasSetPixelColor;
        public delegate* unmanaged[Cdecl]<void*, int, int, int> CanvasGetPixelColor;
        public delegate* unmanaged[Cdecl]<void*, int, int, int> CanvasGetBasePixelColor;
        public delegate* unmanaged[Cdecl]<void*, int, int, uint, void> CanvasSetPixel;
        public delegate* unmanaged[Cdecl]<void*, int, int, uint> CanvasGetPixel;
        public delegate* unmanaged[Cdecl]<void*, int, int, uint> CanvasGetBasePixel;
        public delegate* unmanaged[Cdecl]<void*, bool> ItemHasMapView;
        public delegate* unmanaged[Cdecl]<void*, void*> ItemGetMapView;
        public delegate* unmanaged[Cdecl]<void*, void*, bool> ItemSetMapView;

        // ---- objects: inventory ----
        public delegate* unmanaged[Cdecl]<void*, void*> PlayerGetInventory;
        public delegate* unmanaged[Cdecl]<void*, void*> PlayerGetEnderChest;
        public delegate* unmanaged[Cdecl]<void*, int> InventoryGetSize;
        public delegate* unmanaged[Cdecl]<void*, int> InventoryGetMaxStackSize;
        public delegate* unmanaged[Cdecl]<void*, int, void*> InventoryGetItem;
        public delegate* unmanaged[Cdecl]<void*, int, void*, void> InventorySetItem;
        public delegate* unmanaged[Cdecl]<void*, void*, bool> InventoryAddItem;
        public delegate* unmanaged[Cdecl]<void*, void*, bool> InventoryRemoveItem;
        public delegate* unmanaged[Cdecl]<void*, void*, bool> InventoryContains;
        public delegate* unmanaged[Cdecl]<void*, bool> InventoryIsEmpty;
        public delegate* unmanaged[Cdecl]<void*, int> InventoryFirstEmpty;
        public delegate* unmanaged[Cdecl]<void*, void> InventoryClear;
        public delegate* unmanaged[Cdecl]<void*, byte*, int> InventoryFirst;
        public delegate* unmanaged[Cdecl]<void*, void*> InventoryGetItemInMainHand;
        public delegate* unmanaged[Cdecl]<void*, void*, void> InventorySetItemInMainHand;
        public delegate* unmanaged[Cdecl]<void*, void*> InventoryGetItemInOffHand;
        public delegate* unmanaged[Cdecl]<void*, void*, void> InventorySetItemInOffHand;
        public delegate* unmanaged[Cdecl]<void*, void*> InventoryGetHelmet;
        public delegate* unmanaged[Cdecl]<void*, void*, void> InventorySetHelmet;
        public delegate* unmanaged[Cdecl]<void*, void*> InventoryGetChestplate;
        public delegate* unmanaged[Cdecl]<void*, void*, void> InventorySetChestplate;
        public delegate* unmanaged[Cdecl]<void*, void*> InventoryGetLeggings;
        public delegate* unmanaged[Cdecl]<void*, void*, void> InventorySetLeggings;
        public delegate* unmanaged[Cdecl]<void*, void*> InventoryGetBoots;
        public delegate* unmanaged[Cdecl]<void*, void*, void> InventorySetBoots;
        public delegate* unmanaged[Cdecl]<void*, int> InventoryGetHeldItemSlot;
        public delegate* unmanaged[Cdecl]<void*, int, void> InventorySetHeldItemSlot;

        // ---- plugin registration ----
        public delegate* unmanaged[Cdecl]<void*, byte*, int, bool, void*, void> PluginRegisterEvent;
        public delegate* unmanaged[Cdecl]<void*, void*, void*, ulong, void> MapRenderCallback;

        // ---- scheduler ----
        public delegate* unmanaged[Cdecl]<void*, void*> ServerGetScheduler;
        public delegate* unmanaged[Cdecl]<void*, void*, int, ulong, ulong, ulong, uint> SchedulerRunTask;
        public delegate* unmanaged[Cdecl]<void*, uint, void> SchedulerCancelTask;
        public delegate* unmanaged[Cdecl]<void*, void*, void> SchedulerCancelTasks;
        public delegate* unmanaged[Cdecl]<void*, uint, bool> SchedulerIsRunning;
        public delegate* unmanaged[Cdecl]<void*, uint, bool> SchedulerIsQueued;
        public delegate* unmanaged[Cdecl]<void*, void**, int, int> SchedulerGetPendingTasks;
        public delegate* unmanaged[Cdecl]<void*, uint> TaskGetId;
        public delegate* unmanaged[Cdecl]<void*, bool> TaskIsSync;
        public delegate* unmanaged[Cdecl]<void*, bool> TaskIsCancelled;
        public delegate* unmanaged[Cdecl]<ulong, void> SchedulerTaskCallback;

        // ---- service manager ----
        // A provider holder is a heap-allocated std::shared_ptr<endstone::Service>
        // owned by the managed side (release with ServiceProviderRelease).
        public delegate* unmanaged[Cdecl]<void*, void*> ServerGetServiceManager;
        public delegate* unmanaged[Cdecl]<void*> ServiceProviderCreate;
        public delegate* unmanaged[Cdecl]<void*, void*> ServiceProviderGetPtr;
        public delegate* unmanaged[Cdecl]<void*, void> ServiceProviderRelease;
        public delegate* unmanaged[Cdecl]<void*, byte*, void*, void*, int, void> ServiceManagerRegister;
        public delegate* unmanaged[Cdecl]<void*, void*, void> ServiceManagerUnregisterAll;
        public delegate* unmanaged[Cdecl]<void*, byte*, void*, void> ServiceManagerUnregister;
        public delegate* unmanaged[Cdecl]<void*, void*, void> ServiceManagerUnregisterProvider;
        public delegate* unmanaged[Cdecl]<void*, byte*, void*> ServiceManagerGet;
    }
#pragma warning restore CS0649

    private static Table* T;

    internal static void Initialize(IntPtr tablePtr)
    {
        T = (Table*)tablePtr;
    }

    internal static bool Ready => T != null;

    internal static string Str(void* p) => Marshal.PtrToStringUTF8((IntPtr)p) ?? string.Empty;

    // ---- string marshalling helpers ----
    internal static byte[] ToUtf8(string s)
    {
        var bytes = Encoding.UTF8.GetBytes(s);
        var buf = new byte[bytes.Length + 1];
        bytes.CopyTo(buf, 0);
        return buf;
    }

    internal static void Call1(delegate* unmanaged[Cdecl]<void*, byte*, void> fn, void* obj, string s)
    {
        var buf = ToUtf8(s);
        fixed (byte* p = buf)
        {
            fn(obj, p);
        }
    }

    internal static void Call2(delegate* unmanaged[Cdecl]<void*, byte*, byte*, void> fn, void* obj, string s1, string s2)
    {
        var b1 = ToUtf8(s1);
        var b2 = ToUtf8(s2);
        fixed (byte* p1 = b1)
        fixed (byte* p2 = b2)
        {
            fn(obj, p1, p2);
        }
    }

    internal static bool CallBoolStr(delegate* unmanaged[Cdecl]<void*, byte*, bool> fn, void* obj, string s)
    {
        var buf = ToUtf8(s);
        fixed (byte* p = buf)
        {
            return fn(obj, p);
        }
    }

    internal static int CallIntStr(delegate* unmanaged[Cdecl]<void*, byte*, int> fn, void* obj, string s)
    {
        var buf = ToUtf8(s);
        fixed (byte* p = buf)
        {
            return fn(obj, p);
        }
    }

    internal static void CallVoidStr(delegate* unmanaged[Cdecl]<void*, byte*, void> fn, void* obj, string s)
    {
        var buf = ToUtf8(s);
        fixed (byte* p = buf)
        {
            fn(obj, p);
        }
    }

    internal static bool CallBoolStrInt(delegate* unmanaged[Cdecl]<void*, byte*, int, bool, bool> fn, void* obj,
                                        string s, int i, bool force)
    {
        var buf = ToUtf8(s);
        fixed (byte* p = buf)
        {
            return fn(obj, p, i, force);
        }
    }

    // ---- event-kind marshalling helpers ----
    // Multi-type event accessors receive the concrete event kind (resolved once
    // per event instance by EventFactory) instead of relying on native RTTI —
    // typeinfo is not shared across DSOs on Linux (see bridge.cpp).

    internal static void* CallKindPtr(delegate* unmanaged[Cdecl]<void*, int, void*> fn, void* obj, EventKind kind)
        => fn(obj, (int)kind);

    internal static bool CallKindBool(delegate* unmanaged[Cdecl]<void*, int, bool> fn, void* obj, EventKind kind)
        => fn(obj, (int)kind);

    internal static void CallKind2(delegate* unmanaged[Cdecl]<void*, int, bool, void> fn, void* obj, EventKind kind,
                                   bool v)
        => fn(obj, (int)kind, v);

    internal static int CallKindInt(delegate* unmanaged[Cdecl]<void*, int, int> fn, void* obj, EventKind kind)
        => fn(obj, (int)kind);

    internal static byte* CallKindStr(delegate* unmanaged[Cdecl]<void*, int, byte*> fn, void* obj, EventKind kind)
        => fn(obj, (int)kind);

    internal static byte* CallKindStr2(delegate* unmanaged[Cdecl]<void*, int, int*, byte*> fn, void* obj,
                                       EventKind kind, int* len)
        => fn(obj, (int)kind, len);

    internal static void CallKind3(delegate* unmanaged[Cdecl]<void*, int, void*, int, void> fn, void* obj,
                                   EventKind kind, void* data, int len)
        => fn(obj, (int)kind, data, len);

    internal static void CallHostPort(delegate* unmanaged[Cdecl]<void*, byte*, int, void> fn, void* obj, string host,
                                      int port)
    {
        var buf = ToUtf8(host);
        fixed (byte* p = buf)
        {
            fn(obj, p, port);
        }
    }

    internal static void CallSound(delegate* unmanaged[Cdecl]<void*, float*, byte*, float, float, void> fn, void* obj,
                                   Location location, string sound, float volume, float pitch)
    {
        var values = stackalloc float[5] { location.X, location.Y, location.Z, location.Pitch, location.Yaw };
        var buf = ToUtf8(sound);
        fixed (byte* p = buf)
        {
            fn(obj, values, p, volume, pitch);
        }
    }

    internal static void CallParticle(delegate* unmanaged[Cdecl]<void*, byte*, float*, byte*, void> fn, void* obj,
                                      string name, Location location, string? molang)
    {
        var values = stackalloc float[5] { location.X, location.Y, location.Z, location.Pitch, location.Yaw };
        var nameBuf = ToUtf8(name);
        var molangBuf = molang == null ? null : ToUtf8(molang);
        fixed (byte* pn = nameBuf)
        fixed (byte* pm = molangBuf)
        {
            fn(obj, pn, values, pm);
        }
    }

    internal static void CallRegisterEvent(void* gcHandle, string eventName, int priority, bool ignoreCancelled,
                                           void* cbHandle)
    {
        var buf = ToUtf8(eventName);
        fixed (byte* p = buf)
        {
            T->PluginRegisterEvent(gcHandle, p, priority, ignoreCancelled, cbHandle);
        }
    }

    internal static Table* Raw => T;
}
