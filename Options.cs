using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Obeliskial_Essentials;
using Photon.Pun;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static Enums;
using static Obeliskial_Essentials.Essentials;

/*
FULL LIST OF ATO CLASSES->METHODS THAT ARE PATCHED:
AlertManager
    IsActive
AtOManager
    SaveBoughtItem
    NET_SaveBoughtItem
    CharInTown
    GetTownTier
    IsTownRerollAvailable
    SetCurrentNode
    GenerateObeliskMap
    AddItemToHero
    GetPlayerGold
    GetPlayerDust
    NodeScore
    CalculateScore
    UpgradeTownTier
    GlobalAuraCurseModificationByTraitsAndItems
BotonSkin
    OnMouseUp
CardCraftManager
    CanCraftThisCard
    SetMaxQuantity
    GetCardAvailability
    ShowElements
    ShowListCardsForCraft
    CheckForCorruptableCards
CardData
    SetDescriptionNew
    NumFormatItem
    NumFormat
    ColorTextArray
    SpriteText
    ColorFromCardDataRarity
    GetFinalAuraCharges
    SetTarget
CardItem
    OnMouseUp
    OnMouseUpController
    ShowEmoteIcon
    RemoveEmoteIcon
    HaveEmoteIcon
Character
    GetTraitAuraCurseModifiers
    ModifyEnergy
    SetAura
    GetDrawCardsTurn
    GetDrawCardsTurnForDisplayInDeck
    GetAuraCurseQuantityModification
CinematicManager
    DoCinematic
ConflictManager
    EnableButtonsForPlayerChoosing
EmoteManager
    Init
    SelectNextCharacter
    SetBlocked
EmoteTarget
    SetIcons
    SetActiveHeroOnCardEmoteButton
EventManager
    FinalResolution
    Start
    Ready
    SetEvent
Functions
    GetCardByRarity
Globals
    Awake
    CreateGameContent
    CreateAuraCurses
    CreateCardClones
    CreateCharClones
    CreateTraitClones
    GetLootData
    GetCostReroll
    GetDivinationCost
HeroSelectionManager
    Start
    StartCo
    ShowFollowStatus
HeroSelection
    SetSprite
    SetSpriteSilueta
InputController
    DoKeyBinding
IntroNewGameManager
    Start
Item
    DoItemData
LobbyManager
    InitLobby
    ShowCreate
Loot
    GetLootItems
MadnessManager
    IsActive
MainMenuManager
    Start
    SetMenuCurrentProfile
    Multiplayer
    JoinMultiplayer
MapManager
    CanTravelToThisNode
    DrawNodes
    Awake
    IncludeMapPrefab
    TravelToThisNode
    BeginMapContinue
    PlayerSelectedNode
    NET_PlayerSelectedNode
    GetNodeFromId
    GetMapNodes
    DrawArrow
MatchManager
    SendEmoteCard
    DoEmoteCard
    SetCharactersPing
    EmoteTarget
    DealNewCard
    GenerateNewCard
    GenerateNewCardCo
    FinishCombat
    ResignCombatActionExecute
NetworkManager
    LoadScene
    NET_LoadScene
PerkNode
    OnMouseUp
    OnMouseEnter
    SetIconLock
    SetLocked
    SetRequired
PerkTree
    CanModify
    SelectPerk
    Show
ProfanityFilter
    CensorString
PlayerManager
    IsCardUnlocked
    GainSupply
    GetPlayerSupplyActual
    IsHeroUnlocked
    GetProgress
    ModifyProgress
PlayerUIManager
    SetGold
    SetDust
    SetSupply
RewardsManager
    NET_CardSelected
    Start
    StartCo
SaveManager
    RestorePlayerData
SettingsManager
    IsActive
SteamManager
    DoSteam
    RestorePlayerData
    SetScore
    SetObeliskScore
Texts
    GetText
TomeManager
    SelectTomeCards
TownManager
    Start
    ShowButtons
TownUpgradeWindow
    SetButtons
Trait
    DoTrait
    TextChargesLeft
UIEnergySelector
    TurnOn
[various onmouseups to disable them when F1locked: BotHeroChar, BotonCardback, BotonEndTurn, BotonFilter, BotonGeneric, BotonMenuGameMode, BotonRollover, BotonScore, BotonSkin, BotonSupply, botTownUpgrades, BoxPlayer, CardCraftSelectorEnergy, CardCraftSelectorRarity, CardItem, CardVertical, CharacterGOItem, CharacterItem, CharacterLoot, CharPopupClose, CombatTarget, DeckInHero, DeckPile, EmoteManager, HeroSelection, InitiativePortrait, ItemCombatIcon, Node, OverCharacter, PerkChallengeItem, PerkColumnItem, PerkNode, RandomHeroSelector, Reply, Tomebutton, TomeEdge, TomeNumber, TomeRun, TownBuilding, TraitLevel]
*/

namespace Obeliskial_Options
{
  [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
  [BepInDependency("com.stiffmeds.obeliskialessentials")]
  [BepInProcess("AcrossTheObelisk.exe")]
  public class Options : BaseUnityPlugin
  {
    public const int ModDate = 20240516;
    private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
    internal static ManualLogSource Log;
    public static int iShopsWithNoPurchase = 0;
    private static bool bUpdatingSettings = false;
    public static string medsDLCCloneTwoSkin = "medsdlctwoa";
    public static string medsDLCCloneThreeSkin = "medsdlcthreea";
    public static string medsDLCCloneFourSkin = "medsdlcfoura";
    public static string medsDLCCloneTwoCardback = "medsdlctwoa";
    public static string medsDLCCloneThreeCardback = "medsdlcthreea";
    public static string medsDLCCloneFourCardback = "medsdlcfoura";
    public static float medsBLPTownTierPower = 5f;
    public static float medsBLPRollPower = 1f;
    public static float medsBLPMythicMult = 1f;
    public static float medsBLPEpicMult = 8f;
    public static float medsBLPRareMult = 128f;
    public static float medsBLPUncommonMult = 256f;
    public static List<string> medsDoNotDropList = new List<string>() { "asmody", "asmodyrare", "betty", "bettyrare", "boneclaws", "boneclawsa", "boneclawsb", "boneclawsrare", "brokenitem", "bunny", "bunnyrare", "burneditem", "champy", "champyrare", "chompy", "chompyrare", "chumpy", "chumpyrare", "combatbandages", "combatbandagesa", "combatbandagesb", "armageddon", "armageddona", "armageddonb", "armageddonrare", "ashysky", "ashyskya", "ashyskyb", "ashyskyrare", "backlash", "backlasha", "backlashb", "backlashrare", "bloodpuddle", "bomblottery", "bomblotterya", "bomblotteryb", "bomblotteryrare", "burningweapons", "burningweaponsa", "burningweaponsb", "burningweaponsrare", "chaospuddle", "chaoticwind", "chaoticwinda", "chaoticwindb", "chaoticwindrare", "coldfront", "colorfulpuddle", "colorfulpuddlea", "colorfulpuddleb", "colorfulpuddlerare", "darkpuddle", "deathgrip", "electricpuddle", "empower", "empowera", "empowerb", "empowerrare", "forestallies", "fungaloutbreak", "fungaloutbreaka", "fungaloutbreakb", "fungaloutbreakrare", "heavenlyarmaments", "heavenlyarmamentsa", "heavenlyarmamentsb", "heavenlyarmamentsrare", "heavyweaponry", "heavyweaponrya", "heavyweaponryb", "heavyweaponryrare", "hexproof", "hexproofa", "hexproofb", "hexproofrare", "holypuddle", "hypotermia", "hypotermiaa", "hypotermiab", "hypotermiarare", "icypuddle", "ironclad", "ironclada", "ironcladb", "ironcladrare", "lavabursts", "lavapuddle", "livingforest", "livingforesta", "livingforestb", "livingforestrare", "lonelyblob", "lonelybloba", "lonelyblobb", "lonelyblobrare", "meatfeast", "meatfeasta", "meatfeastb", "melancholy", "melancholya", "melancholyb", "melancholyrare", "metalpuddle", "noxiousparasites", "noxiousparasitesa", "noxiousparasitesb", "noxiousparasitesrare", "pacifism", "pacifisma", "pacifismb", "pacifismrare", "poisonfields", "poisonfieldsa", "poisonfieldsb", "poisonfieldsrare", "putrefaction", "putrefactiona", "putrefactionb", "putrefactionrare", "resurrection", "resurrectiona", "resurrectionb", "revenge", "revengea", "revengeb", "revengerare", "rosegarden", "rosegardena", "rosegardenb", "rosegardenrare", "sacredground", "sacredgrounda", "sacredgroundb", "sacredgroundrare", "snowfall", "snowfalla", "snowfallb", "snowfallrare", "spookynight", "starrynight", "starrynighta", "starrynightb", "starrynightrare", "subzero", "subzeroa", "subzerob", "subzerorare", "sugarrush", "thornproliferation", "thornproliferationa", "thornproliferationb", "thornproliferationrare", "thunderstorm", "thunderstorma", "thunderstormb", "thunderstormrare", "toxicpuddle", "trickortreat", "upwind", "upwinda", "upwindb", "upwindrare", "vigorous", "vigorousa", "vigorousb", "vigorousrare", "waterpuddle", "windsofamnesia", "windsofamnesiaa", "windsofamnesiab", "windsofamnesiarare", "cursedjewelering", "daley", "daleyrare", "bloodblobpet", "bloodblobpetrare", "chaosblobpet", "chaosblobpetrare", "darkblobpet", "darkblobpetrare", "electricblobpet", "electricblobpetrare", "holyblobpet", "holyblobpetrare", "icyblobpet", "icyblobpetrare", "lavablobpet", "lavablobpetrare", "metalblobpet", "metalblobpetrare", "toxicblobpet", "toxicblobpetrare", "waterblobpet", "waterblobpetrare", "familyjewels", "familyjewelsa", "familyjewelsb", "flamy", "flamyrare", "forestbanner", "forestbannera", "forestbannerb", "harley", "harleya", "harleyb", "harleyrare", "heavypackage", "hightchancellorstaff", "hightchancellorstaffa", "hightchancellorstaffb", "hightchancellorstaffrare", "jinglebell", "jinglebella", "jinglebellb", "liante", "lianterare", "meatbag", "meatbaga", "meatbagb", "mozzy", "mozzyrare", "oculy", "oculyrare", "orby", "orbyrare", "powerglove", "powerglovea", "powergloveb", "prophetstaff", "prophetstaffa", "prophetstaffb", "prophetstaffrare", "raggeddoll", "raggeddolla", "raggeddollb", "rangerarmor", "rangerarmora", "rangerarmorb", "reforgedcore", "reforgedcorea", "reforgedcoreb", "sharpy", "sharpyrare", "slimy", "slimyrare", "soullantern", "soullanterna", "soullanternb", "stormy", "stormyrare", "thewolfslayer", "thewolfslayera", "thewolfslayerb", "thewolfslayerrare", "tombstone", "venomflask", "venomflaska", "venomflaskb", "wolfy", "wolfyrare", "woodencrosier", "woodencrosiera", "woodencrosierb", "woodencrosierrare", "corruptedplate", "corruptedplatea", "corruptedplateb", "corruptedplaterare", "cuby", "cubyd", "cubydrare", "cubyrare", "familyjewelsrare", "fenny", "fennyrare", "gildedplate", "gildedplatea", "gildedplateb", "gildedplaterare", "scaraby", "scarabyrare", "sepulchralspectre", "sepulchralspectrerare", "whisperingstone" };
    public static List<string> medsDropOnlySoU = new List<string>() { "architectsring", "architectsringrare", "blackpyramid", "blackpyramidrare", "burialmask", "burialmaskrare", "crimsonraiment", "crimsonraimentrare", "desertjam", "desertjamrare", "durandal", "durandalrare", "energyshield", "energyshield rare", "energyshieldrare", "fistofthedamned", "fistofthedamnedrare", "holyrune", "holyrunerare", "lunaring", "lunaringrare", "necromancerrobe", "necromancerroberare", "sacredaxe", "sacredaxerare", "scarabshield", "scarabshieldrare", "shadowrune", "shadowrunerare", "solring", "solringrare", "suppressionhelmet", "suppressionhelmetrare", "tessaract", "tessaractrare", "thejuggernaut", "thejuggernautrare", "topazring", "topazringrare", "turban", "turbanrare", "undeathichor", "undeathichorrare", "unholyhammer", "unholyhammerrare" };
    public static List<string> medsKeepRequirements = new List<string>() { "_demo", "_tier1", "_tier2", "_tier3", "_tier4", "caravan", "crocomenburn", "ulmininup", "ulminindown", "ulmininportal", "ulmininsanddown" };
    public static List<string> medsObeliskNodes = new List<string>() { "sen_34", "aqua_36", "faen_39", "ulmin_40", "velka_33", "sahti_63" };
    public static List<string> medsZoneStartNodes = new List<string>() { "aqua_0", "faen_0", "ulmin_0", "velka_0", "sahti_0", "voidlow_0", "woods_0" };

    // Debug
    public static ConfigEntry<bool> medsKeyItems { get; private set; }
    public static ConfigEntry<bool> medsJuiceGold { get; private set; }
    public static ConfigEntry<bool> medsJuiceDust { get; private set; }
    public static ConfigEntry<bool> medsJuiceSupplies { get; private set; }
    public static ConfigEntry<bool> medsDeveloperMode { get; private set; }
    public static ConfigEntry<string> medsExportSettings { get; private set; }
    public static ConfigEntry<string> medsImportSettings { get; private set; }

    // Cards & Decks
    public static ConfigEntry<int> medsDiminutiveDecks { get; private set; }
    public static ConfigEntry<string> medsDenyDiminishingDecks { get; private set; }
    public static ConfigEntry<bool> medsCraftCorruptedCards { get; private set; }
    public static ConfigEntry<bool> medsInfiniteCardCraft { get; private set; }

    // Characters
    public static ConfigEntry<bool> medsDLCClones { get; private set; }
    public static ConfigEntry<string> medsDLCCloneTwo { get; private set; }
    public static ConfigEntry<string> medsDLCCloneThree { get; private set; }
    public static ConfigEntry<string> medsDLCCloneFour { get; private set; }
    public static ConfigEntry<string> medsDLCCloneTwoName { get; private set; }
    public static ConfigEntry<string> medsDLCCloneThreeName { get; private set; }
    public static ConfigEntry<string> medsDLCCloneFourName { get; private set; }
    public static ConfigEntry<bool> medsOver50s { get; private set; }
    //public static ConfigEntry<bool> medsCustomContent { get; private set; }

    // Corruption & Madness
    public static ConfigEntry<bool> medsSmallSanitySupplySelling { get; private set; }
    public static ConfigEntry<bool> medsRavingRerolls { get; private set; }
    public static ConfigEntry<bool> medsUseClaimation { get; private set; }

    // Events & Nodes
    // public static ConfigEntry<bool> medsAlwaysFail { get; private set; }
    // public static ConfigEntry<bool> medsAlwaysSucceed { get; private set; }
    public static ConfigEntry<bool> medsNoClassRequirements { get; private set; }
    public static ConfigEntry<bool> medsNoEquipmentRequirements { get; private set; }
    public static ConfigEntry<bool> medsNoKeyItemRequirements { get; private set; }
    public static ConfigEntry<bool> medsTravelAnywhere { get; private set; }
    public static ConfigEntry<bool> medsVisitAllZones { get; private set; }

    // Loot
    public static ConfigEntry<bool> medsCorruptGiovanna { get; private set; }
    public static ConfigEntry<bool> medsLootCorrupt { get; private set; }

    // Perks
    public static ConfigEntry<bool> medsPerkPoints { get; private set; }
    public static ConfigEntry<bool> medsModifyPerks { get; private set; }
    public static ConfigEntry<bool> medsNoPerkRequirements { get; private set; }

    // Shop
    public static ConfigEntry<bool> medsShopRarity { get; private set; }
    public static ConfigEntry<int> medsShopBadLuckProtection { get; private set; }
    public static ConfigEntry<bool> medsMapShopCorrupt { get; private set; }
    public static ConfigEntry<bool> medsObeliskShopCorrupt { get; private set; }
    public static ConfigEntry<bool> medsTownShopCorrupt { get; private set; }
    public static ConfigEntry<bool> medsDiscountDivination { get; private set; }
    public static ConfigEntry<bool> medsDiscountDoomroll { get; private set; }
    public static ConfigEntry<bool> medsPlentifulPetPurchases { get; private set; }
    public static ConfigEntry<bool> medsStockedShop { get; private set; }
    public static ConfigEntry<bool> medsSoloShop { get; private set; }
    public static ConfigEntry<bool> medsDropShop { get; private set; }

    // Should Be Vanilla
    public static ConfigEntry<bool> medsProfane { get; private set; }
    public static ConfigEntry<bool> medsEmotional { get; private set; }
    public static ConfigEntry<bool> medsStraya { get; private set; }
    public static ConfigEntry<string> medsStrayaServer { get; private set; }
    public static ConfigEntry<bool> medsMaxMultiplayerMembers { get; private set; }
    public static ConfigEntry<bool> medsOverlyTenergetic { get; private set; }
    public static ConfigEntry<bool> medsOverlyCardergetic { get; private set; }
    public static ConfigEntry<bool> medsBugfixEquipmentHP { get; private set; }
    public static ConfigEntry<bool> medsSkipCinematics { get; private set; }
    public static ConfigEntry<bool> medsAutoContinue { get; private set; }
    public static ConfigEntry<bool> medsMPLoadAutoCreateRoom { get; private set; }
    public static ConfigEntry<bool> medsMPLoadAutoReady { get; private set; }
    public static ConfigEntry<bool> medsSpacebarContinue { get; private set; }
    public static ConfigEntry<int> medsConflictResolution { get; private set; }
    public static ConfigEntry<bool> medsAllThePets { get; private set; }
    //public static ConfigEntry<bool> medsActivationAwareness { get; private set; }

    // Combat
    // public static ConfigEntry<int> medsBlessBehavior { get; private set; }

    // Multiplayer
    public static bool medsMPShopRarity = false;
    public static bool medsMPMapShopCorrupt = false;
    public static bool medsMPObeliskShopCorrupt = false;
    public static bool medsMPTownShopCorrupt = false;
    public static bool medsMPItemCorrupt = false;
    public static bool medsMPPerkPoints = false;
    public static bool medsMPCorruptGiovanna = false;
    public static bool medsMPKeyItems = false;
    // public static bool medsMPAlwaysSucceed = false;
    // public static bool medsMPAlwaysFail = false;
    public static bool medsMPCraftCorruptedCards = false;
    public static bool medsMPInfiniteCardCraft = false;
    public static bool medsMPStockedShop = false;
    public static bool medsMPSoloShop = false;
    public static bool medsMPDropShop = false;
    private static bool medsMPAllThePets = false;
    public static bool medsMPDeveloperMode = false;
    public static bool medsMPJuiceGold = false;
    public static bool medsMPJuiceDust = false;
    // public static bool medsMPJuiceSupplies = false;
    public static bool medsMPUseClaimation = false;
    public static bool medsMPDiscountDivination = false;
    public static bool medsMPDiscountDoomroll = false;
    public static bool medsMPRavingRerolls = false;
    public static bool medsMPSmallSanitySupplySelling = false;
    public static bool medsMPModifyPerks = false;
    public static bool medsMPPlentifulPetPurchases = false;
    public static bool medsMPNoPerkRequirements = false;
    public static bool medsMPTravelAnywhere = false;
    //public static bool medsMPNoTravelRequirements = false;
    private static bool medsMPNoClassRequirements = false;
    private static bool medsMPNoEquipmentRequirements = false;
    private static bool medsMPNoKeyItemRequirements = false;
    public static bool medsMPOverlyTenergetic = false;
    public static bool medsMPOverlyCardergetic = false;
    public static int medsMPDiminutiveDecks = 1;
    public static string medsMPDenyDiminishingDecks = "";
    public static int medsMPShopBadLuckProtection = 0;
    public static bool medsMPBugfixEquipmentHP = false;
    public static bool medsMPDLCClones = false;
    public static string medsMPDLCCloneTwo = "";
    public static string medsMPDLCCloneThree = "";
    public static string medsMPDLCCloneFour = "";
    public static bool medsMPVisitAllZones = false;
    public static int medsMPConflictResolution = 0;
    // public static int medsMPBlessBehavior = 0;

    private void Awake()
    {
      // Plugin.medsGetClaimation = this.Config.Bind<bool>("Options", "High Madness - Acquire Claims", false, "(NOT WORKING - NOT EVEN STARTED) Acquire new claims on any madness.");
      Log = Logger;
      // Debug
      medsKeyItems = Config.Bind(new ConfigDefinition("Debug", "All Key Items"), false, new ConfigDescription("Give all key items in Adventure Mode. Items are added when you load into a town; if you've already passed the town and want the key items, use Travel Anywhere to go back to town? I'll add more methods in the future :)."));
      medsJuiceGold = Config.Bind(new ConfigDefinition("Debug", "Gold ++"), false, new ConfigDescription("Many cash."));
      medsJuiceDust = Config.Bind(new ConfigDefinition("Debug", "Dust ++"), false, new ConfigDescription("Many cryttals."));
      medsJuiceSupplies = Config.Bind(new ConfigDefinition("Debug", "Supplies ++"), false, new ConfigDescription("Many supplies."));
      medsDeveloperMode = Config.Bind(new ConfigDefinition("Debug", "Developer Mode"), false, new ConfigDescription("Turns on AtO devs’ developer mode. Back up your saves before using!"));
      medsExportSettings = Config.Bind(new ConfigDefinition("Debug", "Export Settings"), "", new ConfigDescription("Export settings (for use with 'Import Settings')."));
      medsImportSettings = Config.Bind(new ConfigDefinition("Debug", "Import Settings"), "", new ConfigDescription("Paste settings here to import them."));
      //medsExportPlayerProfiles = Config.Bind(new ConfigDefinition("Debug", "Export Player Profiles"), true, new ConfigDescription("Export player profiles for use with Profile Editor."));
      //medsImportPlayerProfiles = Config.Bind(new ConfigDefinition("Debug", "Import Player Profiles"), false, new ConfigDescription("Import edited player profiles."));
      //medsCustomContent = Config.Bind(new ConfigDefinition("Debug", "Enable Custom Content"), true, new ConfigDescription("(IN TESTING) Loads custom cards/items/sprites[/auracurses]."));

      // Cards & Decks
      medsDiminutiveDecks = Config.Bind(new ConfigDefinition("Cards & Decks", "Minimum Deck Size"), 1, new ConfigDescription("Set the minimum deck size."));
      medsDenyDiminishingDecks = Config.Bind(new ConfigDefinition("Cards & Decks", "Card Removal"), "Can Remove Anything", new ConfigDescription("What cards can be removed at the church?", new AcceptableValueList<string>("Cannot Remove Cards", "Cannot Remove Curses", "Can Only Remove Curses", "Can Remove Anything")));
      medsCraftCorruptedCards = Config.Bind(new ConfigDefinition("Cards & Decks", "Craft Corrupted Cards"), false, new ConfigDescription("Allow crafting of corrupted cards."));
      medsInfiniteCardCraft = Config.Bind(new ConfigDefinition("Cards & Decks", "Craft Infinite Cards"), false, new ConfigDescription("Infinite card crafts (set available card count to 99)."));

      // Characters
      medsDLCClones = Config.Bind(new ConfigDefinition("Characters", "Enable Clones"), true, new ConfigDescription("Adds three clone characters to the DLC section of Hero Selection."));
      medsDLCCloneTwo = Config.Bind(new ConfigDefinition("Characters", "Clone 1"), "loremaster", new ConfigDescription("Which subclass should clone 1 be?"));
      medsDLCCloneThree = Config.Bind(new ConfigDefinition("Characters", "Clone 2"), "loremaster", new ConfigDescription("Which subclass should clone 2 be?"));
      medsDLCCloneFour = Config.Bind(new ConfigDefinition("Characters", "Clone 3"), "loremaster", new ConfigDescription("Which subclass should clone 3 be?"));
      medsDLCCloneTwoName = Config.Bind(new ConfigDefinition("Characters", "Clone 1 Name"), "Clone", new ConfigDescription("What should clone 1 be called?"));
      medsDLCCloneThreeName = Config.Bind(new ConfigDefinition("Characters", "Clone 2 Name"), "Copy", new ConfigDescription("What should clone 2 be called?"));
      medsDLCCloneFourName = Config.Bind(new ConfigDefinition("Characters", "Clone 3 Name"), "Counterfeit", new ConfigDescription("What should clone 3 be called?"));
      medsOver50s = Config.Bind(new ConfigDefinition("Characters", "Level Past 50"), true, new ConfigDescription("Allows characters to be raised up to rank 500."));

      // Corruption & Madness
      medsSmallSanitySupplySelling = Config.Bind(new ConfigDefinition("Corruption & Madness", "Sell Supplies"), true, new ConfigDescription("Sell supplies on high madness."));
      medsRavingRerolls = Config.Bind(new ConfigDefinition("Corruption & Madness", "Shop Rerolls"), true, new ConfigDescription("Allow multiple shop rerolls on high madness."));
      medsUseClaimation = Config.Bind(new ConfigDefinition("Corruption & Madness", "Use Claims"), true, new ConfigDescription("Use claims on any madness. Note that you cannot _get_ claims on high madness (yet...)."));

      // Events & Nodes
      // medsAlwaysFail = Config.Bind(new ConfigDefinition("Events & Nodes", "Always Fail Event Rolls"), false, new ConfigDescription("Always fail event rolls (unless Always Succeed is on), though event text might not match. Critically fails if possible."));
      // medsAlwaysSucceed = Config.Bind(new ConfigDefinition("Events & Nodes", "Always Succeed Event Rolls"), false, new ConfigDescription("Always succeed event rolls, though event text might not match. Critically succeeds if possible."));
      // medsNoTravelRequirements = Config.Bind(new ConfigDefinition("Events & Nodes", "No Travel Requirements"), false, new ConfigDescription("(NOT WORKING - shows path to node, but not actual node) Can travel to nodes that are normally invisible (e.g. western treasure node in Faeborg)."));
      medsNoClassRequirements = Config.Bind(new ConfigDefinition("Events & Nodes", "No Class Requirements"), false, new ConfigDescription("(IN TESTING) Events and replies ignore class requirements."));
      medsNoEquipmentRequirements = Config.Bind(new ConfigDefinition("Events & Nodes", "No Equipment Requirements"), false, new ConfigDescription("(IN TESTING) Events and replies ignore equipment/pet requirements."));
      medsNoKeyItemRequirements = Config.Bind(new ConfigDefinition("Events & Nodes", "No Key Item Requirements"), false, new ConfigDescription("(IN TESTING) Events and replies ignore key item / quest requirements."));
      medsTravelAnywhere = Config.Bind(new ConfigDefinition("Events & Nodes", "Travel Anywhere"), false, new ConfigDescription("Travel to any node."));
      medsVisitAllZones = Config.Bind(new ConfigDefinition("Events & Nodes", "Visit All Zones"), false, new ConfigDescription("You can choose any location to visit from the obelisk (e.g. can go to the Void early, can visit all locations before going, etc.)."));

      // Loot
      medsCorruptGiovanna = Config.Bind(new ConfigDefinition("Loot", "Corrupted Card Rewards"), false, new ConfigDescription("Card rewards are always corrupted (includes divinations)."));
      medsLootCorrupt = Config.Bind(new ConfigDefinition("Loot", "Corrupted Loot Rewards"), false, new ConfigDescription("Make item loot rewards always corrupted."));

      // Perks
      medsPerkPoints = Config.Bind(new ConfigDefinition("Perks", "Many Perk Points"), false, new ConfigDescription("(IN TESTING - visually buggy but functional) Set maximum perk points to 1000."));
      medsModifyPerks = Config.Bind(new ConfigDefinition("Perks", "Modify Perks Whenever"), false, new ConfigDescription("(IN TESTING) Change perks whenever you want."));
      medsNoPerkRequirements = Config.Bind(new ConfigDefinition("Perks", "No Perk Requirements"), false, new ConfigDescription("Can select perk without selecting its precursor perks; ignore minimum selected perk count for each row."));

      // Shop
      medsShopRarity = Config.Bind(new ConfigDefinition("Shop", "Adjusted Shop Rarity"), false, new ConfigDescription("Modify shop rarity based on current madness/corruption. This also makes the change in rarity from act 1 to 4 _slightly_ less abrupt."));
      medsShopBadLuckProtection = Config.Bind(new ConfigDefinition("Shop", "Bad Luck Protection"), 10, new ConfigDescription("Increases rarity of shops/loot based on number of shops/loot seen since an item was last acquired. If you play with discounted rerolls I recommend setting this to ~10; if you play without, I recommend ~300 :)", new AcceptableValueRange<int>(0, 10000)));
      medsMapShopCorrupt = Config.Bind(new ConfigDefinition("Shop", "Corrupted Map Shops"), true, new ConfigDescription("Allow shops on the map (e.g. werewolf shop in Senenthia) to have corrupted goods for sale."));
      medsObeliskShopCorrupt = Config.Bind(new ConfigDefinition("Shop", "Corrupted Obelisk Shops"), true, new ConfigDescription("Allow obelisk corruption shops to have corrupted goods for sale."));
      medsTownShopCorrupt = Config.Bind(new ConfigDefinition("Shop", "Corrupted Town Shops"), true, new ConfigDescription("Allow town shops to have corrupted goods for sale."));
      medsDiscountDivination = Config.Bind(new ConfigDefinition("Shop", "Discount Divination"), true, new ConfigDescription("Discounts are applied to divinations."));
      medsDiscountDoomroll = Config.Bind(new ConfigDefinition("Shop", "Discount Doomroll"), true, new ConfigDescription("Discounts are applied to shop rerolls."));
      medsPlentifulPetPurchases = Config.Bind(new ConfigDefinition("Shop", "Plentiful Pet Purchases"), true, new ConfigDescription("Buy more than one of each pet."));
      medsStockedShop = Config.Bind(new ConfigDefinition("Shop", "Post-Scarcity Shops"), true, new ConfigDescription("Does not record who purchased what in the shop."));
      medsSoloShop = Config.Bind(new ConfigDefinition("Shop", "Individual Player Shops"), true, new ConfigDescription("Does not send shop purchase records in multiplayer."));
      medsDropShop = Config.Bind(new ConfigDefinition("Shop", "Drop-Only Items Appear In Shops"), true, new ConfigDescription("Items that would normally not appear in shops, such as the Yggdrasil Root or Yogger's Cleaver, will appear."));

      // Should Be Vanilla
      medsProfane = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Allow Profanities"), true, new ConfigDescription("Allow profanities in your good Christian Piss server."));
      medsEmotional = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Emotional"), true, new ConfigDescription("Use more emotes during combat."));
      medsStraya = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Force Select Server"), false, new ConfigDescription("Force server selection to location of your choice (default: Australia)."));
      medsStrayaServer = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Force Select Server Selection"), "au", new ConfigDescription("Which server should be forced if the above option is true?", new AcceptableValueList<string>("asia", "au", "cae", "eu", "in", "jp", "ru", "rue", "za", "sa", "kr", "us", "usw")));
      medsMaxMultiplayerMembers = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Max Multiplayer Members"), true, new ConfigDescription("Change the default player count in multiplDefault to 4 players in multiplayer."));
      medsOverlyTenergetic = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Overly Tenergetic"), true, new ConfigDescription("Allow characters to have more than 10 energy."));
      medsOverlyCardergetic = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Overly Cardergetic"), true, new ConfigDescription("(IN TESTING) Allow characters to have more than 10 cards."));
      medsBugfixEquipmentHP = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Bugfix: Equipment HP"), true, new ConfigDescription("(IN TESTING - visually buggy but functional) Fixes a vanilla bug that allows infinite stacking of HP by buying the same item repeatedly."));
      medsSkipCinematics = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Skip Cinematics"), false, new ConfigDescription("Skip cinematics."));
      medsAutoContinue = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Auto Continue"), false, new ConfigDescription("(IN TESTING - visually buggy but functional) Automatically press 'Continue' in events."));
      medsMPLoadAutoCreateRoom = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Auto Create Room on MP Load"), true, new ConfigDescription("Use previous settings to automatically create lobby room when loading multiplayer game."));
      medsMPLoadAutoReady = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Auto Ready on MP Load"), true, new ConfigDescription("Automatically readies up non-host players when loading multiplayer game."));
      medsSpacebarContinue = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Spacebar to Continue"), true, new ConfigDescription("Spacebar clicks the 'Continue' button in events for you."));
      medsConflictResolution = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Conflict Resolution"), 4, new ConfigDescription("(IN TESTING) Automatically select (1) lowest card; (2) closest to 2; (3) highest card; or (4) random to determine multiplayer conflicts."));
      medsAllThePets = Config.Bind(new ConfigDefinition("Should Be Vanilla", "All The Pets"), true, new ConfigDescription("(IN TESTING) Shows blob pets and Harley in the Tome of Knowledge and shop."));
      //medsActivationAwareness = Config.Bind(new ConfigDefinition("Should Be Vanilla", "Activation Awareness"), true, new ConfigDescription("(IN TESTING) Alerts you when a card in your hand will activate an item or enchantment."));

      // Combat
      // medsBlessBehavior = Config.Bind(new ConfigDefinition("Combat", "Bless Behavior"), 0, new ConfigDescription("(IN TESTING) Bless/sharp/fortify behaviour. (0) default (applies to both damage types on a card); (1) first (applies to first damage type on card); (2) split (damage split equally between damage types)."));

      medsImportSettings.Value = "";
      medsExportSettings.Value = SettingsToString();

      medsKeyItems.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsJuiceGold.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsJuiceDust.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsJuiceSupplies.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDeveloperMode.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsExportSettings.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsImportSettings.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDiminutiveDecks.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDenyDiminishingDecks.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsCraftCorruptedCards.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsInfiniteCardCraft.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsSmallSanitySupplySelling.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsRavingRerolls.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsUseClaimation.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      // medsAlwaysFail.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      // medsAlwaysSucceed.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      // medsNoTravelRequirements.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsNoClassRequirements.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsNoEquipmentRequirements.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsNoKeyItemRequirements.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsTravelAnywhere.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsCorruptGiovanna.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsLootCorrupt.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsPerkPoints.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsModifyPerks.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsNoPerkRequirements.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsShopRarity.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsShopBadLuckProtection.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsMapShopCorrupt.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsObeliskShopCorrupt.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsTownShopCorrupt.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDiscountDivination.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDiscountDoomroll.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsPlentifulPetPurchases.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsStockedShop.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsSoloShop.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsProfane.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsEmotional.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsStraya.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsStrayaServer.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsMaxMultiplayerMembers.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsOverlyTenergetic.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsOverlyCardergetic.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsBugfixEquipmentHP.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsSkipCinematics.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsAutoContinue.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsMPLoadAutoCreateRoom.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsMPLoadAutoReady.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsSpacebarContinue.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDLCClones.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDLCCloneTwo.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDLCCloneThree.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDLCCloneFour.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDLCCloneTwoName.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDLCCloneThreeName.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDLCCloneFourName.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsOver50s.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsDropShop.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsVisitAllZones.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsConflictResolution.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      medsAllThePets.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      //medsActivationAwareness.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };
      // medsBlessBehavior.SettingChanged += (obj, args) => { if (!bUpdatingSettings) { SettingsUpdated(); }; };

      medsImportSettings.SettingChanged += (obj, args) => { StringToSettings(medsImportSettings.Value); };

      /*UniverseLib.Universe.Init(1f, ObeliskialUI.InitUI, LogHandler, new()
      {
          Disable_EventSystem_Override = false, // or null
          Force_Unlock_Mouse = true, // or null
          Unhollowed_Modules_Folder = null
      });*/
      harmony.PatchAll();
      LogInfo($"{PluginInfo.PLUGIN_GUID} {PluginInfo.PLUGIN_VERSION} has loaded! Prayge ");
      RegisterMod(PluginInfo.PLUGIN_NAME, "stiffmeds", "Options to alter gameplay.", PluginInfo.PLUGIN_VERSION, ModDate, @"https://across-the-obelisk.thunderstore.io/package/meds/Obeliskial_Options/", null, "", 90, new string[1] { "settings" }, "", true);
      //AddModVersionText(PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION, ModDate.ToString());
    }

    internal static void LogDebug(string msg)
    {
      Log.LogDebug(msg);
    }
    internal static void LogInfo(string msg)
    {
      Log.LogInfo(msg);
    }
    internal static void LogWarning(string msg)
    {
      Log.LogWarning(msg);
    }
    internal static void LogError(string msg)
    {
      Log.LogError(msg);
    }
    void LogHandler(string message, UnityEngine.LogType type)
    {
      string log = message?.ToString() ?? "";
      switch (type)
      {
        case UnityEngine.LogType.Assert:
        case UnityEngine.LogType.Log:
          LogInfo(log);
          break;
        case UnityEngine.LogType.Warning:
          LogWarning(log);
          break;
        case UnityEngine.LogType.Error:
        case UnityEngine.LogType.Exception:
          LogError(log);
          break;
      }
    }
    public static string SettingsToString(bool forMP = false)
    {
      string[] str = new string[39];
      str[0] = medsShopRarity.Value ? "1" : "0";
      str[1] = medsMapShopCorrupt.Value ? "1" : "0";
      str[2] = medsObeliskShopCorrupt.Value ? "1" : "0";
      str[3] = medsTownShopCorrupt.Value ? "1" : "0";
      str[4] = medsLootCorrupt.Value ? "1" : "0";
      str[5] = medsPerkPoints.Value ? "1" : "0";
      str[6] = medsCorruptGiovanna.Value ? "1" : "0";
      str[7] = medsKeyItems.Value ? "1" : "0";
      str[8] = "0"; // medsAlwaysSucceed.Value ? "1" : "0";
                    //str[9] = "0"; // medsAlwaysFail.Value ? "1" : "0";
      str[9] = medsOverlyCardergetic.Value ? "1" : "0";
      str[10] = medsCraftCorruptedCards.Value ? "1" : "0";
      str[11] = medsInfiniteCardCraft.Value ? "1" : "0";
      str[12] = medsStockedShop.Value ? "1" : "0";
      str[13] = medsSoloShop.Value ? "1" : "0";
      str[14] = medsDeveloperMode.Value ? "1" : "0";
      str[15] = (medsDLCClones.Value ? "1" : "0") + "&" + medsDLCCloneTwo.Value + "&" + medsDLCCloneThree.Value + "&" + medsDLCCloneFour.Value;
      // str[15] = (medsSetTeam.Value ? "1" : "0") + "&" + medsSetTeam1.Value + "&" + medsSetTeam2.Value + "&" + medsSetTeam3.Value + "&" + medsSetTeam4.Value + "&" + medsPerksFrom1.Value + "&" + medsPerksFrom2.Value + "&" + medsPerksFrom3.Value + "&" + medsPerksFrom4.Value;
      str[16] = medsUseClaimation.Value ? "1" : "0";
      str[17] = medsDiscountDivination.Value ? "1" : "0";
      str[18] = medsDiscountDoomroll.Value ? "1" : "0";
      str[19] = medsRavingRerolls.Value ? "1" : "0";
      str[20] = medsSmallSanitySupplySelling.Value ? "1" : "0";
      str[21] = medsModifyPerks.Value ? "1" : "0";
      str[22] = medsNoPerkRequirements.Value ? "1" : "0";
      str[23] = medsPlentifulPetPurchases.Value ? "1" : "0";
      str[24] = medsTravelAnywhere.Value ? "1" : "0";
      str[25] = medsAllThePets.Value ? "1" : "0";
      str[26] = medsNoClassRequirements.Value ? "1" : "0";
      str[27] = medsNoEquipmentRequirements.Value ? "1" : "0";
      str[28] = medsNoKeyItemRequirements.Value ? "1" : "0";
      str[29] = medsOverlyTenergetic.Value ? "1" : "0";
      str[30] = medsDiminutiveDecks.Value.ToString();
      str[31] = medsDenyDiminishingDecks.Value;
      str[32] = medsShopBadLuckProtection.Value.ToString();
      str[33] = medsBugfixEquipmentHP.Value ? "1" : "0";
      str[34] = medsJuiceGold.Value ? "1" : "0";
      str[35] = medsJuiceDust.Value ? "1" : "0";
      str[36] = medsDropShop.Value ? "1" : "0";
      str[37] = medsVisitAllZones.Value ? "1" : "0";
      str[38] = medsConflictResolution.Value.ToString();
      string jstr = string.Join("|", str);
      if (!forMP)
      {
        str = new string[15];
        str[0] = medsProfane.Value ? "1" : "0";
        str[0] = "%|" + str[0];
        str[1] = medsEmotional.Value ? "1" : "0";
        str[2] = medsStraya.Value ? "1" : "0";
        str[3] = medsStrayaServer.Value;
        str[4] = medsMaxMultiplayerMembers.Value ? "1" : "0";
        str[5] = medsSkipCinematics.Value ? "1" : "0";
        str[6] = medsAutoContinue.Value ? "1" : "0";
        str[7] = medsJuiceSupplies.Value ? "1" : "0";
        str[8] = medsMPLoadAutoCreateRoom.Value ? "1" : "0";
        str[9] = medsMPLoadAutoReady.Value ? "1" : "0";
        str[10] = medsSpacebarContinue.Value ? "1" : "0";
        str[11] = medsDLCCloneTwoName.Value;
        str[12] = medsDLCCloneThreeName.Value;
        str[13] = medsDLCCloneFourName.Value;
        str[14] = medsOver50s.Value ? "1" : "0";
        //str[15] = medsActivationAwareness.Value ? "1" : "0";
        jstr += string.Join("|", str);
      }
      return jstr;
    }
    public static void StringToSettings(string incomingSettings)
    {
      string[] str = incomingSettings.Split("|%|");
      if (str.Length == 2)
      {
        string[] nonMPstr = str[1].Split("|");
        if (nonMPstr.Length >= 1)
          medsProfane.Value = nonMPstr[0] == "1";
        if (nonMPstr.Length >= 2)
          medsEmotional.Value = nonMPstr[1] == "1";
        if (nonMPstr.Length >= 3)
          medsStraya.Value = nonMPstr[2] == "1";
        if (nonMPstr.Length >= 4)
          medsStrayaServer.Value = nonMPstr[3];
        if (nonMPstr.Length >= 5)
          medsMaxMultiplayerMembers.Value = nonMPstr[4] == "1";
        if (nonMPstr.Length >= 6)
          medsSkipCinematics.Value = nonMPstr[5] == "1";
        if (nonMPstr.Length >= 7)
          medsAutoContinue.Value = nonMPstr[6] == "1";
        if (nonMPstr.Length >= 8)
          medsJuiceSupplies.Value = nonMPstr[7] == "1";
        if (nonMPstr.Length >= 9)
          medsMPLoadAutoCreateRoom.Value = nonMPstr[8] == "1";
        if (nonMPstr.Length >= 10)
          medsMPLoadAutoReady.Value = nonMPstr[9] == "1";
        if (nonMPstr.Length >= 11)
          medsSpacebarContinue.Value = nonMPstr[10] == "1";
        if (nonMPstr.Length >= 12)
          medsDLCCloneTwoName.Value = nonMPstr[11];
        if (nonMPstr.Length >= 13)
          medsDLCCloneThreeName.Value = nonMPstr[12];
        if (nonMPstr.Length >= 14)
          medsDLCCloneFourName.Value = nonMPstr[13];
        if (nonMPstr.Length >= 15)
          medsOver50s.Value = nonMPstr[14] == "1";
        //if (nonMPstr.Length >= 16)
        //medsActivationAwareness.Value = nonMPstr[15] == "1";
      }
      str = str[0].Split("|");
      if (str.Length >= 1)
        medsShopRarity.Value = str[0] == "1";
      if (str.Length >= 2)
        medsMapShopCorrupt.Value = str[1] == "1";
      if (str.Length >= 3)
        medsObeliskShopCorrupt.Value = str[2] == "1";
      if (str.Length >= 4)
        medsTownShopCorrupt.Value = str[3] == "1";
      if (str.Length >= 5)
        medsLootCorrupt.Value = str[4] == "1";
      if (str.Length >= 6)
        medsPerkPoints.Value = str[5] == "1";
      if (str.Length >= 7)
        medsCorruptGiovanna.Value = str[6] == "1";
      if (str.Length >= 8)
        medsKeyItems.Value = str[7] == "1";
      //if (str.Length >= 9)
      //medsAlwaysSucceed.Value = str[8] == "1";
      //if (str.Length >= 10)
      //medsAlwaysFail.Value = str[9] == "1";
      if (str.Length >= 10)
        medsOverlyCardergetic.Value = str[9] == "1";
      if (str.Length >= 11)
        medsCraftCorruptedCards.Value = str[10] == "1";
      if (str.Length >= 12)
        medsInfiniteCardCraft.Value = str[11] == "1";
      if (str.Length >= 13)
        medsStockedShop.Value = str[12] == "1";
      if (str.Length >= 14)
        medsSoloShop.Value = str[13] == "1";
      if (str.Length >= 15)
        medsDeveloperMode.Value = str[14] == "1";
      if (str.Length >= 16)
      {
        medsDLCClones.Value = str[15].Split("&")[0] == "1";
        medsDLCCloneTwo.Value = str[15].Split("&")[1];
        medsDLCCloneThree.Value = str[15].Split("&")[2];
        medsDLCCloneFour.Value = str[15].Split("&")[3];
      }
      if (str.Length >= 17)
        medsUseClaimation.Value = str[16] == "1";
      if (str.Length >= 18)
        medsDiscountDivination.Value = str[17] == "1";
      if (str.Length >= 19)
        medsDiscountDoomroll.Value = str[18] == "1";
      if (str.Length >= 20)
        medsRavingRerolls.Value = str[19] == "1";
      if (str.Length >= 21)
        medsSmallSanitySupplySelling.Value = str[20] == "1";
      if (str.Length >= 22)
        medsModifyPerks.Value = str[21] == "1";
      if (str.Length >= 23)
        medsNoPerkRequirements.Value = str[22] == "1";
      if (str.Length >= 24)
        medsPlentifulPetPurchases.Value = str[23] == "1";
      if (str.Length >= 25)
        medsTravelAnywhere.Value = str[24] == "1";
      if (str.Length >= 26)
        medsAllThePets.Value = str[25] == "1";
      if (str.Length >= 27)
        medsNoClassRequirements.Value = str[26] == "1";
      if (str.Length >= 28)
        medsNoEquipmentRequirements.Value = str[27] == "1";
      if (str.Length >= 29)
        medsNoClassRequirements.Value = str[28] == "1";
      if (str.Length >= 30)
        medsOverlyTenergetic.Value = str[29] == "1";
      if (str.Length >= 31)
        medsDiminutiveDecks.Value = int.Parse(str[30]);
      if (str.Length >= 32)
        medsDenyDiminishingDecks.Value = str[31];
      if (str.Length >= 33)
        medsShopBadLuckProtection.Value = int.Parse(str[32]);
      if (str.Length >= 34)
        medsBugfixEquipmentHP.Value = str[33] == "1";
      if (str.Length >= 35)
        medsJuiceGold.Value = str[34] == "1";
      if (str.Length >= 36)
        medsJuiceDust.Value = str[35] == "1";
      if (str.Length >= 37)
        medsDropShop.Value = str[36] == "1";
      if (str.Length >= 38)
        medsVisitAllZones.Value = str[37] == "1";
      if (str.Length >= 39)
        medsConflictResolution.Value = int.Parse(str[38]);
      medsExportSettings.Value = SettingsToString();
      medsImportSettings.Value = "";
    }

    public static void SaveMPSettings(string _newSettings)
    {
      List<string> settingsChanged = new();
      Log.LogDebug("RECEIVING SETTINGS: " + _newSettings);
      string[] str = _newSettings.Split("|");
      if (str.Length >= 1)
        medsMPShopRarity = str[0] == "1";
      if (str.Length >= 2)
        medsMPObeliskShopCorrupt = str[1] == "1";
      if (str.Length >= 3)
        medsMPMapShopCorrupt = str[2] == "1";
      if (str.Length >= 4)
        medsMPTownShopCorrupt = str[3] == "1";
      if (str.Length >= 5)
        medsMPItemCorrupt = str[4] == "1";
      if (str.Length >= 6)
        medsMPPerkPoints = str[5] == "1";
      if (str.Length >= 7)
        medsMPCorruptGiovanna = str[6] == "1";
      if (str.Length >= 8)
        medsMPKeyItems = str[7] == "1";
      //if (str.Length >= 9)
      //medsMPAlwaysSucceed = str[8] == "1";
      //if (str.Length >= 10)
      //medsMPAlwaysFail = str[9] == "1";
      if (str.Length >= 10)
        medsMPOverlyCardergetic = str[9] == "1";
      if (str.Length >= 11)
        medsMPCraftCorruptedCards = str[10] == "1";
      if (str.Length >= 12)
        medsMPInfiniteCardCraft = str[11] == "1";
      if (str.Length >= 13)
        medsMPStockedShop = str[12] == "1";
      if (str.Length >= 14)
        medsMPSoloShop = str[13] == "1";
      if (str.Length >= 15)
        medsMPDeveloperMode = str[14] == "1";
      if (str.Length >= 16)
      {
        bool subclassChange = false;
        if (!(str[15].Split("&")[0] == "1") == medsMPDLCClones || (str[15].Split("&")[0] == "1" && (medsMPDLCCloneTwo != str[15].Split("&")[1] || medsMPDLCCloneThree != str[15].Split("&")[2] || medsMPDLCCloneFour != str[15].Split("&")[3]))) // different to current setting!
          subclassChange = true;
        medsMPDLCClones = str[15].Split("&")[0] == "1";
        medsMPDLCCloneTwo = str[15].Split("&")[1];
        medsMPDLCCloneThree = str[15].Split("&")[2];
        medsMPDLCCloneFour = str[15].Split("&")[3];
        if (subclassChange)
          SubClassReplace();
      }
      if (str.Length >= 17)
        medsMPUseClaimation = str[16] == "1";
      if (str.Length >= 18)
        medsMPDiscountDivination = str[17] == "1";
      if (str.Length >= 19)
        medsMPDiscountDoomroll = str[18] == "1";
      if (str.Length >= 20)
        medsMPRavingRerolls = str[19] == "1";
      if (str.Length >= 21)
        medsMPSmallSanitySupplySelling = str[20] == "1";
      if (str.Length >= 22)
        medsMPModifyPerks = str[21] == "1";
      if (str.Length >= 23)
        medsMPNoPerkRequirements = str[22] == "1";
      if (str.Length >= 24)
        medsMPPlentifulPetPurchases = str[23] == "1";
      if (str.Length >= 25)
        medsMPTravelAnywhere = str[24] == "1";
      if (str.Length >= 26)
        medsMPAllThePets = str[25] == "1";
      if (str.Length >= 27)
        medsMPNoClassRequirements = str[26] == "1";
      if (str.Length >= 28)
        medsMPNoEquipmentRequirements = str[27] == "1";
      if (str.Length >= 29)
        medsMPNoKeyItemRequirements = str[28] == "1";
      if (str.Length >= 30)
        medsMPOverlyTenergetic = str[29] == "1";
      if (str.Length >= 31)
        medsMPDiminutiveDecks = int.Parse(str[30]);
      if (str.Length >= 32)
        medsMPDenyDiminishingDecks = str[31];
      if (str.Length >= 33)
        medsMPShopBadLuckProtection = int.Parse(str[32]);
      if (str.Length >= 34)
        medsMPBugfixEquipmentHP = str[33] == "1";
      if (str.Length >= 35)
        medsMPJuiceGold = str[34] == "1";
      if (str.Length >= 36)
        medsMPJuiceDust = str[35] == "1";
      if (str.Length >= 37)
        medsMPDropShop = str[36] == "1";
      if (str.Length >= 38)
        medsMPVisitAllZones = str[37] == "1";
      if (str.Length >= 39)
        medsMPConflictResolution = int.Parse(str[38]);
      Log.LogDebug("RECEIVED " + str.Length + " SETTINGS!");
      if (medsNoClassRequirements.Value != medsMPNoClassRequirements)
        settingsChanged.Add("No Class Requirements");
      if (medsNoEquipmentRequirements.Value != medsMPNoEquipmentRequirements)
        settingsChanged.Add("No Equipment Requirements");
      if (medsNoKeyItemRequirements.Value != medsMPNoKeyItemRequirements)
        settingsChanged.Add("No Key Item Requirements");
      if (settingsChanged.Count > 0)
      {
        AlertManager.buttonClickDelegate = new AlertManager.OnButtonClickDelegate(UpdateSettingsForYou);
        AlertManager.Instance.AlertConfirmDouble("ERROR!\nThe host of this game has settings that are incompatible with yours and require a restart: " + String.Join(", ", settingsChanged.ToArray()) + "\nWould you like these settings to be changed?");
      }
      UpdateDropOnlyItems();
      UpdateAllThePets();
      UpdateVisitAllZones();
    }

    public static void SendSettingsMP()
    {
      int inum = 666666;
      bool b = true;
      string _values = SettingsToString(true);
      SaveMPSettings(_values);
      PhotonView medsphotonView = PhotonView.Get((Component)NetworkManager.Instance);
      // send to other players
      Log.LogInfo("SHARING SETTINGS: " + _values);
      medsphotonView.RPC("NET_LoadScene", RpcTarget.Others, (object)_values, (object)b, (object)inum);
      bUpdatingSettings = false;
    }

    public static void SettingsUpdated()
    {
      bUpdatingSettings = true;
      string newSettings = SettingsToString();
      if (
          (medsExportSettings.Value.Length > 25 && medsExportSettings.Value[25] != newSettings[25]) || // all the pets
          (medsExportSettings.Value.Length > 26 && medsExportSettings.Value[26] != newSettings[26]) || // no class requirements
          (medsExportSettings.Value.Length > 27 && medsExportSettings.Value[27] != newSettings[27]) || // no equipment requirements
          (medsExportSettings.Value.Length > 28 && medsExportSettings.Value[28] != newSettings[28])) // no key item requirements
        AlertManager.Instance.AlertConfirm("These settings will not properly take effect until you restart the game.");
      if (medsExportSettings.Value.Length > 37 && medsExportSettings.Value[37] != newSettings[37])
        UpdateVisitAllZones();
      medsExportSettings.Value = newSettings;
      if ((UnityEngine.Object)NetworkManager.Instance != (UnityEngine.Object)null && (UnityEngine.Object)GameManager.Instance != (UnityEngine.Object)null && GameManager.Instance.IsMultiplayer() && NetworkManager.Instance.IsMaster())
      {
        SendSettingsMP();
      }
      else
      {
        bUpdatingSettings = false;
        SubClassReplace();
      }
    }
    public static void UpdateSettingsForYou()
    {
      AlertManager.buttonClickDelegate -= new AlertManager.OnButtonClickDelegate(UpdateSettingsForYou);
      medsAllThePets.Value = medsMPAllThePets;
      medsNoClassRequirements.Value = medsMPNoClassRequirements;
      medsNoEquipmentRequirements.Value = medsMPNoEquipmentRequirements;
      medsNoKeyItemRequirements.Value = medsMPNoKeyItemRequirements;
    }

    public static void SubClassReplace()
    {
      Log.LogDebug("CREATECLONES START");
      // PlayerManager.Instance.SetSkin("medsdlctwo", medsSkinData.SkinId);
      if (!(IsHost() ? medsDLCClones.Value : medsMPDLCClones))
        return;
      Dictionary<string, SubClassData> medsSubClassesSource = Traverse.Create(Globals.Instance).Field("_SubClassSource").GetValue<Dictionary<string, SubClassData>>();
      string medsSCDId = "";
      string medsSCDName = "";
      string medsSCDReplaceWith = "";
      // SubClassData medsSCD = new();
      int classCount = 0;
      foreach (SubClassData _scd in medsSubClassesSource.Values)
      {
        if (_scd.Id == "medsdlctwo" || _scd.Id == "medsdlcthree" || _scd.Id == "medsdlcfour")
          continue;
        if (!_scd.MainCharacter)
          continue;
        if (_scd.HeroClassSecondary != HeroClass.None) // multiclass
        {
          LogInfo("MC: " + _scd.Id);
          classCount++;
        }
      }
      for (int chr = 0; chr <= 2; chr++)
      {
        if (chr == 0)
        {
          medsSCDId = "medsdlctwo";
          medsSCDName = medsDLCCloneTwoName.Value;
          medsSCDReplaceWith = (IsHost() ? medsDLCCloneTwo.Value : medsMPDLCCloneTwo);
          Essentials.medsCloneTwo = medsSCDReplaceWith;
        }
        else if (chr == 1)
        {
          medsSCDId = "medsdlcthree";
          medsSCDName = medsDLCCloneThreeName.Value;
          medsSCDReplaceWith = (IsHost() ? medsDLCCloneThree.Value : medsMPDLCCloneThree);
          Essentials.medsCloneThree = medsSCDReplaceWith;
        }
        else if (chr == 2)
        {
          medsSCDId = "medsdlcfour";
          medsSCDName = medsDLCCloneFourName.Value;
          medsSCDReplaceWith = (IsHost() ? medsDLCCloneFour.Value : medsMPDLCCloneFour);
          Essentials.medsCloneFour = medsSCDReplaceWith;
        }
        SubClassData medsSCD = UnityEngine.Object.Instantiate<SubClassData>(Globals.Instance.SubClass[medsSCDReplaceWith]);
        medsSCD.Id = medsSCDId;
        medsSCD.CharacterName = medsSCDName;
        medsSCD.OrderInList = classCount + chr;
        medsSCD.SubClassName = medsSCDId;
        medsSCD.MainCharacter = true;
        medsSubClassesSource[medsSCDId] = medsSCD;
        Globals.Instance.SubClass[medsSCDId] = UnityEngine.Object.Instantiate<SubClassData>(medsSCD);
      }
      Traverse.Create(Globals.Instance).Field("_SubClassSource").SetValue(medsSubClassesSource);
      Log.LogDebug("CREATECLONES END");
      // add duplicate cardbacks for medsDLC characters
      Dictionary<string, CardbackData> medsCardbackDataSource = Traverse.Create(Globals.Instance).Field("_CardbackDataSource").GetValue<Dictionary<string, CardbackData>>();
      for (int a = 97; a <= 122; a++)
      {
        if (medsCardbackDataSource.ContainsKey("medsdlctwo" + ((char)a).ToString()))
          medsCardbackDataSource.Remove("medsdlctwo" + ((char)a).ToString());
        if (medsCardbackDataSource.ContainsKey("medsdlcthree" + ((char)a).ToString()))
          medsCardbackDataSource.Remove("medsdlcthree" + ((char)a).ToString());
        if (medsCardbackDataSource.ContainsKey("medsdlcfour" + ((char)a).ToString()))
          medsCardbackDataSource.Remove("medsdlcfour" + ((char)a).ToString());
      }
      Dictionary<string, CardbackData> medsCardbacksToAdd = new();
      int b = 97;
      int c = 97;
      int d = 97;
      Log.LogDebug("duplicating cardbacks...");
      // loop through all cardbacks, duplicating those used for the current clones
      foreach (KeyValuePair<string, CardbackData> keyValuePair in medsCardbackDataSource)
      {
        // Plugin.Log.LogInfo(keyValuePair.Key + medsCardbackDataSource.Count);
        if ((UnityEngine.Object)keyValuePair.Value.CardbackSubclass != (UnityEngine.Object)null && keyValuePair.Value.CardbackSubclass.Id.ToLower() == (IsHost() ? medsDLCCloneTwo.Value : medsMPDLCCloneTwo))
        {
          CardbackData medsSingleCardback = UnityEngine.Object.Instantiate<CardbackData>(medsCardbackDataSource[keyValuePair.Key]);
          medsSingleCardback.CardbackId = "medsdlctwo" + ((char)b).ToString();
          medsSingleCardback.CardbackSubclass = Globals.Instance.SubClass["medsdlctwo"];
          medsCardbacksToAdd[medsSingleCardback.CardbackId] = medsSingleCardback;
          b++;
        }
        if ((UnityEngine.Object)keyValuePair.Value.CardbackSubclass != (UnityEngine.Object)null && keyValuePair.Value.CardbackSubclass.Id.ToLower() == (IsHost() ? medsDLCCloneThree.Value : medsMPDLCCloneThree))
        {
          CardbackData medsSingleCardback = UnityEngine.Object.Instantiate<CardbackData>(medsCardbackDataSource[keyValuePair.Key]);
          medsSingleCardback.CardbackId = "medsdlcthree" + ((char)c).ToString();
          medsSingleCardback.CardbackSubclass = Globals.Instance.SubClass["medsdlcthree"];
          medsCardbacksToAdd[medsSingleCardback.CardbackId] = medsSingleCardback;
          c++;
        }
        if ((UnityEngine.Object)keyValuePair.Value.CardbackSubclass != (UnityEngine.Object)null && keyValuePair.Value.CardbackSubclass.Id.ToLower() == (IsHost() ? medsDLCCloneFour.Value : medsMPDLCCloneFour))
        {
          CardbackData medsSingleCardback = UnityEngine.Object.Instantiate<CardbackData>(medsCardbackDataSource[keyValuePair.Key]);
          medsSingleCardback.CardbackId = "medsdlcfour" + ((char)d).ToString();
          medsSingleCardback.CardbackSubclass = Globals.Instance.SubClass["medsdlcfour"];
          medsCardbacksToAdd[medsSingleCardback.CardbackId] = medsSingleCardback;
          d++;
        }
      }
      medsDLCCloneTwoCardback = "medsdlctwo" + ((char)(b - 1)).ToString();
      medsDLCCloneThreeCardback = "medsdlcthree" + ((char)(c - 1)).ToString();
      medsDLCCloneFourCardback = "medsdlcfour" + ((char)(d - 1)).ToString();
      medsCardbacksToAdd = medsCardbackDataSource.Concat(medsCardbacksToAdd).GroupBy(p => p.Key).ToDictionary(g => g.Key, g => g.Last().Value);
      Traverse.Create(Globals.Instance).Field("_CardbackDataSource").SetValue(medsCardbacksToAdd);
      Log.LogDebug("CREATECLONECARDBACKS END");
      // add duplicate skins for medsDLC characters
      Dictionary<string, SkinData> medsSkinDataSource = Traverse.Create(Globals.Instance).Field("_SkinDataSource").GetValue<Dictionary<string, SkinData>>();
      for (int a = 97; a <= 122; a++)
      {
        if (medsSkinDataSource.ContainsKey("medsdlctwo" + ((char)a).ToString()))
          medsSkinDataSource.Remove("medsdlctwo" + ((char)a).ToString());
        if (medsSkinDataSource.ContainsKey("medsdlcthree" + ((char)a).ToString()))
          medsSkinDataSource.Remove("medsdlcthree" + ((char)a).ToString());
        if (medsSkinDataSource.ContainsKey("medsdlcfour" + ((char)a).ToString()))
          medsSkinDataSource.Remove("medsdlcfour" + ((char)a).ToString());
      }
      Dictionary<string, SkinData> medsSkinsToAdd = new();
      b = 97;
      c = 97;
      d = 97;
      // loop through all skins, duplicating those used for the current clones
      foreach (KeyValuePair<string, SkinData> keyValuePair in medsSkinDataSource)
      {
        if ((UnityEngine.Object)keyValuePair.Value.SkinSubclass != (UnityEngine.Object)null && keyValuePair.Value.SkinSubclass.Id.ToLower() == (IsHost() ? medsDLCCloneTwo.Value : medsMPDLCCloneTwo))
        {
          SkinData medsSingleSkin = UnityEngine.Object.Instantiate<SkinData>(medsSkinDataSource[keyValuePair.Key]);
          medsSingleSkin.SkinId = "medsdlctwo" + ((char)b).ToString();
          medsSingleSkin.SkinSubclass = Globals.Instance.SubClass["medsdlctwo"];
          medsSkinsToAdd[medsSingleSkin.SkinId] = medsSingleSkin;
          b++;
        }
        if ((UnityEngine.Object)keyValuePair.Value.SkinSubclass != (UnityEngine.Object)null && keyValuePair.Value.SkinSubclass.Id.ToLower() == (IsHost() ? medsDLCCloneThree.Value : medsMPDLCCloneThree))
        {
          SkinData medsSingleSkin = UnityEngine.Object.Instantiate<SkinData>(medsSkinDataSource[keyValuePair.Key]);
          medsSingleSkin.SkinId = "medsdlcthree" + ((char)c).ToString();
          medsSingleSkin.SkinSubclass = Globals.Instance.SubClass["medsdlcthree"];
          medsSkinsToAdd[medsSingleSkin.SkinId] = medsSingleSkin;
          c++;
        }
        if ((UnityEngine.Object)keyValuePair.Value.SkinSubclass != (UnityEngine.Object)null && keyValuePair.Value.SkinSubclass.Id.ToLower() == (IsHost() ? medsDLCCloneFour.Value : medsMPDLCCloneFour))
        {
          SkinData medsSingleSkin = UnityEngine.Object.Instantiate<SkinData>(medsSkinDataSource[keyValuePair.Key]);
          medsSingleSkin.SkinId = "medsdlcfour" + ((char)d).ToString();
          medsSingleSkin.SkinSubclass = Globals.Instance.SubClass["medsdlcfour"];
          medsSkinsToAdd[medsSingleSkin.SkinId] = medsSingleSkin;
          d++;
        }
      }
      medsDLCCloneTwoSkin = "medsdlctwo" + ((char)(b - 1)).ToString();
      medsDLCCloneThreeSkin = "medsdlcthree" + ((char)(c - 1)).ToString();
      medsDLCCloneFourSkin = "medsdlcfour" + ((char)(d - 1)).ToString();
      medsSkinsToAdd = medsSkinDataSource.Concat(medsSkinsToAdd).GroupBy(p => p.Key).ToDictionary(g => g.Key, g => g.Last().Value);
      Traverse.Create(Globals.Instance).Field("_SkinDataSource").SetValue(medsSkinsToAdd);
      LogDebug("CREATECLONESKINS END");
    }


    public static void SaveServerSelection()
    {
      switch (medsStrayaServer.Value)
      {
        case "asia":
          SaveManager.SaveIntoPrefsInt("networkRegion", 0);
          break;
        case "au":
          SaveManager.SaveIntoPrefsInt("networkRegion", 1);
          break;
        case "cae":
          SaveManager.SaveIntoPrefsInt("networkRegion", 2);
          break;
        case "eu":
          SaveManager.SaveIntoPrefsInt("networkRegion", 3);
          break;
        case "in":
          SaveManager.SaveIntoPrefsInt("networkRegion", 4);
          break;
        case "jp":
          SaveManager.SaveIntoPrefsInt("networkRegion", 5);
          break;
        case "ru":
          SaveManager.SaveIntoPrefsInt("networkRegion", 6);
          break;
        case "rue":
          SaveManager.SaveIntoPrefsInt("networkRegion", 7);
          break;
        case "za":
          SaveManager.SaveIntoPrefsInt("networkRegion", 8);
          break;
        case "sa":
          SaveManager.SaveIntoPrefsInt("networkRegion", 9);
          break;
        case "kr":
          SaveManager.SaveIntoPrefsInt("networkRegion", 10);
          break;
        case "us":
          SaveManager.SaveIntoPrefsInt("networkRegion", 11);
          break;
        case "usw":
          SaveManager.SaveIntoPrefsInt("networkRegion", 12);
          break;
        default:
          break;
      }
    }


    public static void UpdateAllThePets()
    {
      Dictionary<string, CardData> medsCardsSource = Traverse.Create(Globals.Instance).Field("_CardsSource").GetValue<Dictionary<string, CardData>>();
      Dictionary<string, CardData> medsCards = Traverse.Create(Globals.Instance).Field("_Cards").GetValue<Dictionary<string, CardData>>();
      Dictionary<Enums.CardType, List<string>> medsCardListByType = medsBasicCardListByType;
      Dictionary<Enums.CardClass, List<string>> medsCardListByClass = medsBasicCardListByClass;
      List<string> medsCardListNotUpgraded = medsBasicCardListNotUpgraded;
      Dictionary<Enums.CardClass, List<string>> medsCardListNotUpgradedByClass = medsBasicCardListNotUpgradedByClass;
      Dictionary<string, List<string>> medsCardListByClassType = medsBasicCardListByClassType;
      Dictionary<Enums.CardType, List<string>> medsCardItemByType = medsBasicCardItemByType;
      /*when All The Pets is enabled, need to set
          card.ShowInTome:    true
          item.QuestItem:     false
      */
      foreach (string cardID in medsAllThePetsCards)
      {
        CardData card = medsCards[cardID];
        CardData cardSource = medsCardsSource[cardID];
        if (IsHost() ? medsAllThePets.Value : medsMPAllThePets)
        {
          card.ShowInTome = true;
          cardSource.ShowInTome = true;
          card.Item.QuestItem = false;
          cardSource.Item.QuestItem = false;
          Globals.Instance.IncludeInSearch(card.CardName, card.Id);
          if (!medsCardListByClass[card.CardClass].Contains(card.Id))
            medsCardListByClass[card.CardClass].Add(card.Id);
          if (card.CardUpgraded == Enums.CardUpgraded.No)
          {
            if (!medsCardListNotUpgradedByClass[card.CardClass].Contains(card.Id))
              medsCardListNotUpgradedByClass[card.CardClass].Add(card.Id);
            if (!medsCardListNotUpgraded.Contains(card.Id))
              medsCardListNotUpgraded.Add(card.Id);
            if (card.CardClass == Enums.CardClass.Item)
            {
              if (!medsCardItemByType.ContainsKey(card.CardType))
                medsCardItemByType.Add(card.CardType, new List<string>());
              if (!medsCardItemByType[card.CardType].Contains(card.Id))
                medsCardItemByType[card.CardType].Add(card.Id);
            }
          }
          List<Enums.CardType> cardTypes = card.GetCardTypes();
          for (int index = 0; index < cardTypes.Count; ++index)
          {
            if (!medsCardListByType[cardTypes[index]].Contains(card.Id))
              medsCardListByType[cardTypes[index]].Add(card.Id);
            string key2 = Enum.GetName(typeof(Enums.CardClass), (object)card.CardClass) + "_" + Enum.GetName(typeof(Enums.CardType), (object)cardTypes[index]);
            if (!medsCardListByClassType.ContainsKey(key2))
              medsCardListByClassType[key2] = new List<string>();
            if (!medsCardListByClassType[key2].Contains(card.Id))
              medsCardListByClassType[key2].Add(card.Id);
            Globals.Instance.IncludeInSearch(Texts.Instance.GetText(Enum.GetName(typeof(Enums.CardType), (object)cardTypes[index])), card.Id);
          }
        }
        else
        {
          card.ShowInTome = false;
          cardSource.ShowInTome = false;
          card.Item.QuestItem = true;
          cardSource.Item.QuestItem = true;
        }
      }
      medsCardListByClassType["Item_Pet"].Sort();
      medsCardItemByType[CardType.Pet].Sort();
      Traverse.Create(Globals.Instance).Field("_CardListByType").SetValue(medsCardListByType);
      Traverse.Create(Globals.Instance).Field("_CardListByClass").SetValue(medsCardListByClass);
      Traverse.Create(Globals.Instance).Field("_CardListNotUpgraded").SetValue(medsCardListNotUpgraded);
      Traverse.Create(Globals.Instance).Field("_CardListNotUpgradedByClass").SetValue(medsCardListNotUpgradedByClass);
      Traverse.Create(Globals.Instance).Field("_CardListByClassType").SetValue(medsCardListByClassType);
      Traverse.Create(Globals.Instance).Field("_CardItemByType").SetValue(medsCardItemByType);
      Traverse.Create(Globals.Instance).Field("_Cards").SetValue(medsCards);
      Traverse.Create(Globals.Instance).Field("_CardsSource").SetValue(medsCardsSource);
    }
    public static void UpdateDropOnlyItems()
    {
      LogDebug("Updating drop-only items...");
      Dictionary<string, ItemData> medsItemDataSource = Traverse.Create(Globals.Instance).Field("_ItemDataSource").GetValue<Dictionary<string, ItemData>>();
      //bool bHasSOU = (GameManager.Instance != null && GameManager.Instance.IsMultiplayer() && NetworkManager.Instance != null && NetworkManager.Instance.AnyPlayersHaveSku("2511580")) || (SteamManager.Instance != null && SteamManager.Instance.PlayerHaveDLC("2511580"));
      foreach (string itemID in medsDropOnlyItems)
        if (medsItemDataSource.ContainsKey(itemID) && !medsDoNotDropList.Contains(itemID))
          medsItemDataSource[itemID].DropOnly = !(IsHost() ? medsDropShop.Value : medsMPDropShop);
      Traverse.Create(Globals.Instance).Field("_ItemDataSource").SetValue(medsItemDataSource);
      Dictionary<string, CardData> medsCardsSource = Traverse.Create(Globals.Instance).Field("_CardsSource").GetValue<Dictionary<string, CardData>>();
      Dictionary<string, CardData> medsCards = Traverse.Create(Globals.Instance).Field("_Cards").GetValue<Dictionary<string, CardData>>();
      foreach (string cardID in medsCardsSource.Keys)
      {
        if (medsCards.ContainsKey(cardID) && medsCards[cardID].CardType != CardType.Pet && medsCards[cardID].Item != null && medsDropOnlyItems.Contains(medsCards[cardID].Item.Id) && !medsDoNotDropList.Contains(medsCards[cardID].Item.Id))
        {
          medsCardsSource[cardID].Item.DropOnly = !(IsHost() ? medsDropShop.Value : medsMPDropShop);
          medsCards[cardID].Item.DropOnly = !(IsHost() ? medsDropShop.Value : medsMPDropShop);
        }
      }
      Traverse.Create(Globals.Instance).Field("_Cards").SetValue(medsCards);
      Traverse.Create(Globals.Instance).Field("_CardsSource").SetValue(medsCardsSource);
      //medsCreateCardClones();
    }

    public static void UpdateVisitAllZones()
    {
      // REALLY, DOESN'T THE AMOUNT OF COMMENTING HERE COMPARED TO EVERYWHERE ELSE TELL YOU HOW FUCKING JANK THIS IS?
      // SAD.
      Dictionary<string, EventData> medsEventDataSource = Traverse.Create(Globals.Instance).Field("_Events").GetValue<Dictionary<string, EventData>>();
      Dictionary<string, NodeData> medsNodeDataSource = Traverse.Create(Globals.Instance).Field("_NodeDataSource").GetValue<Dictionary<string, NodeData>>();
      EventReplyData newReply = medsEventDataSource["e_velka33_tier2"].Replys[0].ShallowCopy();
      newReply.IndexForAnswerTranslation = 666; // spooky.
      string replyText = Texts.Instance.GetText("events_e_velka33_tier2_rp0");
      string replyTextS = Texts.Instance.GetText("events_e_velka33_tier2_rp0_s");
      bool doWeVisitAllZones = IsHost() ? medsVisitAllZones.Value : medsMPVisitAllZones;
      //Plugin.Log.LogDebug("ARE WE VISITING ALL ZONES, MY CHILD? " + doWeVisitAllZones.ToString());
      // ohhh we need to set requirement for newreply??
      newReply.Requirement = doWeVisitAllZones ? (EventRequirementData)null : Globals.Instance.GetRequirementData("medsimpossiblerequirement");
      foreach (string nodeID in medsObeliskNodes)
      {
        if (medsNodeDataSource.ContainsKey(nodeID))
        {
          // EventData[] newEvents = new EventData[0];
          for (int a = 0; a < medsNodeDataSource[nodeID].NodeEvent.Length; a++)
          { // for every possible event on that node...
            string eventID = medsNodeDataSource[nodeID].NodeEvent[a].EventId;
            if (medsNodeDataSource[nodeID].NodeEvent[a].Requirement != (EventRequirementData)null && (medsNodeDataSource[nodeID].NodeEvent[a].Requirement.RequirementId == "medsimpossiblerequirement" || medsNodeDataSource[nodeID].NodeEvent[a].Requirement.RequirementId == "_tier2"))
            { // if the event has a requirement (either medsimpossiblerequirement, which we set as a placeholder/deliberately impossible-to-receive requirement; or _tier2, which is vanilla)
              //Plugin.Log.LogDebug("setting tier2/4 on node " + nodeID + " event " + medsNodeDataSource[nodeID].NodeEvent[a].EventId);
              // if setting is enabled, sets _tier4 so this event never shows (i.e., we don't use the vanilla "travel through big portal" event)
              // otherwise, set to _tier2 (i.e., show "travel through big portal" event if in act 3)
              medsEventDataSource[medsNodeDataSource[nodeID].NodeEvent[a].EventId].Requirement = Globals.Instance.GetRequirementData(doWeVisitAllZones ? "medsimpossiblerequirement" : "_tier2");
              medsNodeDataSource[nodeID].NodeEvent[a].Requirement = Globals.Instance.GetRequirementData(doWeVisitAllZones ? "medsimpossiblerequirement" : "_tier2");
              // Plugin.Log.LogDebug("req: " + Globals.Instance.GetRequirementData(doWeVisitAllZones ? "medsimpossiblerequirement" : "_tier2").RequirementId);
            }
            else if (medsNodeDataSource[nodeID].NodeEvent[a].Replys.Length > 2)
            { // if the event _does not_ have a requirement (vanilla act 1/2 event)
              // node
              bool existingReply = false;
              for (int b = 0; b < medsNodeDataSource[nodeID].NodeEvent[a].Replys.Length; b++)
              {
                if (medsNodeDataSource[nodeID].NodeEvent[a].Replys[b].IndexForAnswerTranslation == 666)
                { // my fake "travel through big portal" eventreply
                  existingReply = true;
                  // if setting enabled, remove requirement; otherwise set impossible to receive requirement so option does not show
                  medsNodeDataSource[nodeID].NodeEvent[a].Replys[b].Requirement = doWeVisitAllZones ? (EventRequirementData)null : Globals.Instance.GetRequirementData("medsimpossiblerequirement");
                }
                else
                {
                  // hide option if previously visited that zone; mark that zone as previously visited if option selected.
                  medsNodeDataSource[nodeID].NodeEvent[a].Replys[b].RequirementBlocked = doWeVisitAllZones ? Globals.Instance.GetRequirementData("medsvisited" + medsNodeDataSource[nodeID].NodeEvent[a].Replys[b].SsNodeTravel.NodeZone.ZoneId.ToLower()) : (EventRequirementData)null;
                  medsNodeDataSource[nodeID].NodeEvent[a].Replys[b].SsRequirementUnlock = Globals.Instance.GetRequirementData(doWeVisitAllZones ? "medsvisited" + medsNodeDataSource[nodeID].NodeEvent[a].Replys[b].SsNodeTravel.NodeZone.ZoneId.ToLower() : (nodeID == "sen_34" ? "_tier1" : "_tier2"));
                }
              }
              EventReplyData[] tempERD = new EventReplyData[0];
              if (!existingReply)
              {
                // if reply does not exist, create it
                // maybe this could be less dumb by converting it from an array into a list, and then back? is that really less dumb?
                // I just feel like this code is obtuse.
                // idk.
                tempERD = medsNodeDataSource[nodeID].NodeEvent[a].Replys;
                Array.Resize(ref tempERD, tempERD.Length + 1);
                tempERD[tempERD.Length - 1] = newReply.ShallowCopy();
                medsNodeDataSource[nodeID].NodeEvent[a].Replys = tempERD;
              }
              //event
              existingReply = false;
              for (int b = 0; b < medsEventDataSource[eventID].Replys.Length; b++)
              {
                if (medsEventDataSource[eventID].Replys[b].IndexForAnswerTranslation == 666)
                { // my fake "travel through big portal" eventreply
                  existingReply = true;
                  // if setting enabled, remove requirement; otherwise set impossible to receive requirement so option does not show
                  medsEventDataSource[eventID].Replys[b].Requirement = doWeVisitAllZones ? (EventRequirementData)null : Globals.Instance.GetRequirementData("_tier4");
                }
                else
                {
                  // hide option if previously visited that zone; mark that zone as previously visited if option selected.
                  medsEventDataSource[eventID].Replys[b].RequirementBlocked = Globals.Instance.GetRequirementData("medsvisited" + medsEventDataSource[eventID].Replys[b].SsNodeTravel.NodeZone.ZoneId.ToLower());
                  medsEventDataSource[eventID].Replys[b].SsRequirementUnlock = Globals.Instance.GetRequirementData(doWeVisitAllZones ? "medsvisited" + medsEventDataSource[eventID].Replys[b].SsNodeTravel.NodeZone.ZoneId.ToLower() : (nodeID == "sen_34" ? "_tier1" : "_tier2"));
                }
              }
              if (!existingReply)
              {
                // if reply does not exist, create it
                // maybe this could be less dumb by converting it from an array into a list, and then back? is that really less dumb?
                // I just feel like this code is obtuse.
                // idk.
                //Plugin.Log.LogDebug("node " + nodeID + " event " + eventID + ": " + medsEventDataSource[medsNodeDataSource[nodeID].NodeEvent[a].EventId].Replys.Length + " replies at beginning!");
                tempERD = medsEventDataSource[medsNodeDataSource[nodeID].NodeEvent[a].EventId].Replys;
                Array.Resize(ref tempERD, tempERD.Length + 1);
                tempERD[tempERD.Length - 1] = newReply.ShallowCopy();
                medsEventDataSource[medsNodeDataSource[nodeID].NodeEvent[a].EventId].Replys = tempERD;
                //Plugin.Log.LogDebug("node " + nodeID + " event " + eventID + ": " + medsEventDataSource[medsNodeDataSource[nodeID].NodeEvent[a].EventId].Replys.Length + " replies at end!");
              }
              // set text for translation (though translation isn't set up :D)
              medsTexts["events_" + medsNodeDataSource[nodeID].NodeEvent[a].EventId + "_rp" + newReply.IndexForAnswerTranslation] = replyText;
              medsTexts["events_" + medsNodeDataSource[nodeID].NodeEvent[a].EventId + "_rp" + newReply.IndexForAnswerTranslation + "_s"] = replyTextS;
            }
          }
        }
      }
      Traverse.Create(Globals.Instance).Field("_Events").SetValue(medsEventDataSource);
      Traverse.Create(Globals.Instance).Field("_NodeDataSource").SetValue(medsNodeDataSource);
    }

    public static void UpdateRequirements()
    {

      Dictionary<string, NodeData> medsNodeDataSource = Traverse.Create(Globals.Instance).Field("_NodeDataSource").GetValue<Dictionary<string, NodeData>>();
      // remove key item requirements
      if (medsNoKeyItemRequirements.Value)
      {
        LogDebug("removing key item requirements");
        foreach (string key in medsNodeDataSource.Keys)
        {
          if (medsNodeDataSource[key].NodeRequirement != (EventRequirementData)null && !medsKeepRequirements.Contains(medsNodeDataSource[key].NodeRequirement.RequirementId))
            medsNodeDataSource[key].NodeRequirement = (EventRequirementData)null;
          for (int a = 0; a < medsNodeDataSource[key].NodesConnectedRequirement.Length; a++)
            medsNodeDataSource[key].NodesConnectedRequirement[a].ConectionRequeriment = (EventRequirementData)null;
        }
      }
      Dictionary<string, EventData> medsEventDataSource = Traverse.Create(Globals.Instance).Field("_Events").GetValue<Dictionary<string, EventData>>();
      // remove event+reply requirements
      if (medsNoClassRequirements.Value || medsNoEquipmentRequirements.Value || medsNoKeyItemRequirements.Value)
      {
        foreach (string key in medsEventDataSource.Keys)
        {
          if (medsNoClassRequirements.Value)
            medsEventDataSource[key].RequiredClass = (SubClassData)null;
          if (medsNoKeyItemRequirements.Value && medsEventDataSource[key].Requirement != (EventRequirementData)null && !medsKeepRequirements.Contains(medsEventDataSource[key].Requirement.RequirementId))
            medsEventDataSource[key].Requirement = (EventRequirementData)null;
          foreach (EventReplyData erd in medsEventDataSource[key].Replys)
          {
            if (medsNoClassRequirements.Value && erd.RepeatForAllCharacters == false && erd.SsRoll == false)
              erd.RequiredClass = (SubClassData)null;
            if (medsNoEquipmentRequirements.Value && erd.GoldCost != 80) // gold cost != 80 is to exclude pet training events
              erd.RequirementItem = (CardData)null;
            if (medsNoKeyItemRequirements.Value)
            {
              if (erd.Requirement != (EventRequirementData)null && !medsKeepRequirements.Contains(erd.Requirement.RequirementId))
                erd.Requirement = (EventRequirementData)null;
              erd.RequirementBlocked = (EventRequirementData)null;
            }
          }
        }
      }
    }
  }
}