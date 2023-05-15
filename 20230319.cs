﻿//using BepInEx.Configuration;
//using BepInEx.Logging;
//using BepInEx;
//using ConfigurationManager;
using HarmonyLib;
using System;
using System.Collections.Generic;
//using System.Text;
//using System.Drawing;
//using System.Threading.Tasks;
//using System.Reflection;
using UnityEngine;
//using UnityEngine.Networking.Types;
//using UnityEngine.InputSystem;
using Steamworks.Data;
using Steamworks;
using Photon.Pun;
using UnityEngine.TextCore;
using static Enums;
using System.Collections;
using System.Reflection;
using System.Linq;
using UnityEngine.InputSystem;
using System.Runtime.ConstrainedExecution;
//using TMPro;

namespace Obeliskial_Options
{
    internal class SupportingActs
    {

        public static void OnGameLobbyJoinRequested(Lobby _lobby, SteamId _friendId)
        {
            Debug.Log((object)nameof(OnGameLobbyJoinRequested));
            Debug.Log((object)_lobby.Id);
            Debug.Log((object)_friendId);
            SteamMatchmaking.JoinLobbyAsync(_lobby.Id);
        }

        public static void OnNewLaunchParameters()
        {
            Debug.Log((object)nameof(OnNewLaunchParameters));
            Debug.Log((object)("[Steam] launchParam -> " + SteamApps.GetLaunchParam("+connect_lobby")));
        }

        public static void OnChatMessage(Friend _friendId, string _string0, string _string1)
        {
            Debug.Log((object)nameof(OnChatMessage));
            Debug.Log((object)_friendId);
            Debug.Log((object)_string0);
            Debug.Log((object)_string1);
        }

        public static void OnGameRichPresenceJoinRequested(Friend _friendId, string _action)
        {
            Debug.Log((object)nameof(OnGameRichPresenceJoinRequested));
            Debug.Log((object)_friendId);
            Debug.Log((object)_action);
        }

        public static void OnLobbyCreated(Result result, Lobby _lobby)
        {
            SteamManager.Instance.lobby = _lobby;
            SteamManager.Instance.lobby.SetPublic();
            SteamManager.Instance.lobby.SetJoinable(true);
            SteamManager.Instance.lobby.SetData("RoomName", NetworkManager.Instance.GetRoomName());
            Debug.Log((object)("[Lobby] OnLobbyCreated " + SteamManager.Instance.lobby.Id.ToString()));
            SteamFriends.OpenGameInviteOverlay(SteamManager.Instance.lobby.Id);
        }

        public static void OnLobbyMemberJoined(Lobby _lobby, Friend _friendId)
        {
        }

        public static void OnLobbyEntered(Lobby _lobby)
        {
            Debug.Log((object)"[Lobby] OnLobbyEntered");
            if (_lobby.IsOwnedBy(SteamManager.Instance.steamId))
                return;
            string data = _lobby.GetData("RoomName");
            Debug.Log((object)("Steam wants to join room -> " + data));
            NetworkManager.Instance.WantToJoinRoomName = data;
            SteamManager.Instance.steamLoaded = true;
        }

        public static void OnLobbyInvite(Friend _friendId, Lobby _lobby)
        {
            Debug.Log((object)"[Lobby] OnLobbyInvite");
            Debug.Log((object)_friendId);
        }
    }
    [HarmonyPatch]
    internal class Patch20230319
    {
        public static Vector3 medsPosIni;
        public static Vector3 medsPosIniBlocked;
        public static bool bSelectingPerk = false;
        public static bool bRemovingCards = false;
        public static bool bPreventCardRemoval = false;
        public static bool bFinalResolution = false;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "Start")]
        public static void GMStartPostfix(ref GameManager __instance)
        {
            __instance.gameVersion = __instance.gameVersion + " (OO v" + Plugin.ModVersion + ")";
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), "Start")]
        public static void MMStartPostfix(ref MainMenuManager __instance)
        {
            __instance.version.text = __instance.version.text.Replace("(", "    (").Replace(")", ")     ") + Plugin.ModDate;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), "GetLootData")]
        public static void GetLootDataPostfix(ref LootData __result)
        {
            // Plugin.Log.LogInfo("GetLootData: " + __result);
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPShopRarity : Plugin.medsShopRarity.Value)
            {
                float num0 = 0f;
                if (MadnessManager.Instance.IsMadnessTraitActive("impedingdoom"))
                    num0 += 0.25f;
                if (MadnessManager.Instance.IsMadnessTraitActive("decadence"))
                    num0 += 0.5f;
                if (MadnessManager.Instance.IsMadnessTraitActive("restrictedpower"))
                    num0 += 1f;
                if (MadnessManager.Instance.IsMadnessTraitActive("resistantmonsters"))
                    num0 += 0.75f;
                if (MadnessManager.Instance.IsMadnessTraitActive("poverty"))
                    num0 += 0.5f;
                if (MadnessManager.Instance.IsMadnessTraitActive("overchargedmonsters"))
                    num0 += 1.5f;
                if (MadnessManager.Instance.IsMadnessTraitActive("randomcombats"))
                    num0 += 0.75f;
                if (MadnessManager.Instance.IsMadnessTraitActive("despair"))
                    num0 += 1.25f;
                num0 += (float)AtOManager.Instance.GetNgPlus(false);
                float num1 = 1f;
                if (AtOManager.Instance.corruptionId == "shop")
                    num1 += 2f * ((float)AtOManager.Instance.GetTownTier() + 1);
                if (AtOManager.Instance.corruptionId == "exoticshop")
                    num1 += 5f * ((float)AtOManager.Instance.GetTownTier() + 1);
                __result.DefaultPercentMythic += (((float)AtOManager.Instance.GetTownTier() * (float)AtOManager.Instance.GetTownTier() * (float)AtOManager.Instance.GetTownTier() * (float)AtOManager.Instance.GetTownTier() + 1f) * num0 * num1 / 50f);
                __result.DefaultPercentRare += (((float)AtOManager.Instance.GetTownTier() * (float)AtOManager.Instance.GetTownTier() * (float)AtOManager.Instance.GetTownTier() * (float)AtOManager.Instance.GetTownTier() + 1f) * num0 * num1 / 50f);
                __result.DefaultPercentEpic += (((float)AtOManager.Instance.GetTownTier() * (float)AtOManager.Instance.GetTownTier() * (float)AtOManager.Instance.GetTownTier() * (float)AtOManager.Instance.GetTownTier() + 1f) * num0 * num1 / 50f);
            }
            float fBadLuckProt = GameManager.Instance.IsMultiplayer() ? (float)Plugin.medsMPShopBadLuckProtection : (float)Plugin.medsShopBadLuckProtection.Value;
            if (fBadLuckProt > 0f)
            {
                fBadLuckProt = fBadLuckProt * ((float)AtOManager.Instance.GetTownTier() + 1) * (float)Plugin.iShopsWithNoPurchase / 100000;
                __result.DefaultPercentMythic += fBadLuckProt;
                __result.DefaultPercentEpic += fBadLuckProt;
                __result.DefaultPercentRare += fBadLuckProt;
                __result.DefaultPercentUncommon += fBadLuckProt;
                Plugin.iShopsWithNoPurchase += 1;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Loot), "GetLootItems", new Type[] { typeof(string), typeof(string) })]
        public static void GetLootItemsPostfix(ref List<string> __result)
        {
            // Plugin.Log.LogDebug($"rare commencement madnessdif: {AtOManager.Instance.GetMadnessDifficulty()}! obeliskmadness: {AtOManager.Instance.GetObeliskMadness()}! ngplus: {AtOManager.Instance.GetNgPlus()}! {__result.Count}!");
            if (__result != null)
            {
                for (int index3 = 0; index3 < __result.Count; ++index3)
                {
                    int num6 = UnityEngine.Random.Range(0, 100);
                    // Plugin.Log.LogDebug($"num6: {num6}!");
                    CardData cardData = Globals.Instance.GetCardData(__result[index3], false);
                    if (!((UnityEngine.Object)cardData == (UnityEngine.Object)null))
                    {
                        int num5 = 0;
                        num5 += Functions.FuncRoundToInt((float)AtOManager.Instance.GetTownTier());
                        num5 *= 2;
                        float num0 = 0f;
                        if (MadnessManager.Instance.IsMadnessTraitActive("impedingdoom"))
                            num0 += 0.25f;
                        if (MadnessManager.Instance.IsMadnessTraitActive("decadence"))
                            num0 += 0.5f;
                        if (MadnessManager.Instance.IsMadnessTraitActive("restrictedpower"))
                            num0 += 1f;
                        if (MadnessManager.Instance.IsMadnessTraitActive("resistantmonsters"))
                            num0 += 0.75f;
                        if (MadnessManager.Instance.IsMadnessTraitActive("poverty"))
                            num0 += 0.5f;
                        if (MadnessManager.Instance.IsMadnessTraitActive("overchargedmonsters"))
                            num0 += 1.5f;
                        if (MadnessManager.Instance.IsMadnessTraitActive("randomcombats"))
                            num0 += 0.75f;
                        if (MadnessManager.Instance.IsMadnessTraitActive("despair"))
                            num0 += 1.25f;
                        num0 += (float)AtOManager.Instance.GetNgPlus(false);
                        num5 += Functions.FuncRoundToInt((float)num0);
                        if (!AtOManager.Instance.CharInTown())
                            num5 += 40;
                        if (AtOManager.Instance.corruptionId == "shop")
                            num5 -= 10;
                        if (AtOManager.Instance.corruptionId == "exoticshop")
                            num5 += 10;
                        if (!(AtOManager.Instance.corruptionId == "exoticshop") && !(AtOManager.Instance.corruptionId == "rareshop") && !(AtOManager.Instance.corruptionId == "shop") && !(AtOManager.Instance.CharInTown()) && Plugin.medsLootCorrupt.Value)
                            num5 += 100;
                        bool flag = false;
                        if ((cardData.CardRarity == Enums.CardRarity.Mythic || cardData.CardRarity == Enums.CardRarity.Epic) && num6 < 3 + num5)
                            flag = true;
                        else if (cardData.CardRarity == Enums.CardRarity.Rare && num6 < 7 + num5)
                            flag = true;
                        else if (cardData.CardRarity == Enums.CardRarity.Uncommon && num6 < 11 + num5)
                            flag = true;
                        else if (cardData.CardRarity == Enums.CardRarity.Common && num6 < 15 + num5)
                            flag = true;
                        bool bAllowCorrupt = true;
                        if (AtOManager.Instance.CharInTown())
                        {
                            // town shop
                            bAllowCorrupt = GameManager.Instance.IsMultiplayer() ? Plugin.medsMPTownShopCorrupt : Plugin.medsTownShopCorrupt.Value;
                        }
                        else if ((AtOManager.Instance.corruptionId == "exoticshop") || (AtOManager.Instance.corruptionId == "rareshop") || (AtOManager.Instance.corruptionId == "shop"))
                        {
                            // challenge shop
                            bAllowCorrupt = GameManager.Instance.IsMultiplayer() ? Plugin.medsMPObeliskShopCorrupt : Plugin.medsObeliskShopCorrupt.Value;
                        }
                        else
                        {
                            // node shop? I can't imagine what else this could be.
                            bAllowCorrupt = GameManager.Instance.IsMultiplayer() ? Plugin.medsMPMapShopCorrupt : Plugin.medsMapShopCorrupt.Value;
                        }
                        if (bAllowCorrupt && flag && (UnityEngine.Object)cardData.UpgradesToRare != (UnityEngine.Object)null)
                            __result[index3] = cardData.UpgradesToRare.Id;
                        // Plugin.Log.LogDebug($"num6: {num6}! num5: {num5}! {flag}!");
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ProfanityFilter.ProfanityFilter), "CensorString", new Type[] { typeof(string), typeof(char), typeof(bool) })]
        public static bool CensorStringPrefix(ref string __result, string sentence)
        {
            if (Plugin.medsProfane.Value)
            {
                __result = sentence;
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Functions), "GetCardByRarity")]
        public static void GetCardByRarityPostfix(ref string __result, CardData _cardData)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPCorruptGiovanna : Plugin.medsCorruptGiovanna.Value)
                __result = _cardData?.UpgradesToRare?.Id ?? __result;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TownManager), "Start")]
        public static void StartPostfix()
        {
            // last updated... 1.0.0, maybe? need to do it again (or just move to PlayerReq method)
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPKeyItems : Plugin.medsKeyItems.Value)
            {
                // Plugin.Log.LogInfo($"giving key items!");
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("altarcorrupted"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("ancientsong"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("apprentice"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("arenachampion"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("asmodyquest1"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("asmodyquest2"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("asmodyquest3"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("assassinquest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("bakerson"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("bakersonsaved"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("bakersonscorched"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("barakexit"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("belphyorhorn"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("belphyorquest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("belphyorquestdone"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("belphyorscroll"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("belphyorsummoned"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("bigfish"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("boatcenter"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("boatdown"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("boatfaenlor"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("boatfail"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("boatrime"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("boatup"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("caravan"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("caravannopay"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("caravanpay"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("childofthestorm"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("cranecode"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("crocomenburn"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("crocomenhelp"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("crocomensteal"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("crossroadnorth"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("crossroadsouth"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("eeriecgestiout"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("elemlava"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("elemrock"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("elemstone"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("elvenarmory"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("elvenmansion"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("farminfested"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("forestrail"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("freeboat"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("goblinhelp"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("goblinnorth"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("goblinquest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("goldensheep"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("goldensheepquest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("goldenwool"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("goldtrophy"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("grainsack"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("hammer"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("happyowlrunes"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("harpyegg"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("hugeruby"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("impaltar"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("jeweledkey"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("keyaquarfall"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("keynorth"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("keyobelisk"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("keyvelkarath"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("largeemerald"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("lavacascade"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("lizardmenhelp"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("lorequest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("magicsapphyre"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("magictorch"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("meetraul"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("merchantcard"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("minstrelquest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("moonstone"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("mosquitoegg"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("naganotes"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("obsidianingots"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("oldjournal"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("oldnotes"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("oldrope"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("pigcaptured"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("piratecoin"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("priestquest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("prophetquest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("pyromancerquest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("rabbitmeat"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("rime"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("rimeoftheancienti"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("rimeoftheancientii"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("samaritan"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("sentinelquest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("sewersexit"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("sheeplost"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("sheeplostnode"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("slimebait"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("slimefriend"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("smalllog"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("spiderpassage"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("stargazervisited"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("testsubject"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("thiefhealed"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("treasurehunt"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("treasurehuntii"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("treasuremap"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("treasurespot"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("tsnemogem"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("tsnemotip1"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("tsnemotip2"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("voidnorth"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("voidnorthpass"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("voidsouth"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("voidsouthpass"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("wardenquest"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("waterwatchtower"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("wolfstory"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("ylmerseed"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("youngharpy"));
                AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("zonahielo"));
                if (NetworkManager.Instance.IsMaster() && GameManager.Instance.IsMultiplayer())
                {
                    AtOManager.Instance.AddPlayerRequirementOthers("altarcorrupted");
                    AtOManager.Instance.AddPlayerRequirementOthers("ancientsong");
                    AtOManager.Instance.AddPlayerRequirementOthers("apprentice");
                    AtOManager.Instance.AddPlayerRequirementOthers("arenachampion");
                    AtOManager.Instance.AddPlayerRequirementOthers("asmodyquest1");
                    AtOManager.Instance.AddPlayerRequirementOthers("asmodyquest2");
                    AtOManager.Instance.AddPlayerRequirementOthers("asmodyquest3");
                    AtOManager.Instance.AddPlayerRequirementOthers("assassinquest");
                    AtOManager.Instance.AddPlayerRequirementOthers("bakerson");
                    AtOManager.Instance.AddPlayerRequirementOthers("bakersonsaved");
                    AtOManager.Instance.AddPlayerRequirementOthers("bakersonscorched");
                    AtOManager.Instance.AddPlayerRequirementOthers("barakexit");
                    AtOManager.Instance.AddPlayerRequirementOthers("belphyorhorn");
                    AtOManager.Instance.AddPlayerRequirementOthers("belphyorquest");
                    AtOManager.Instance.AddPlayerRequirementOthers("belphyorquestdone");
                    AtOManager.Instance.AddPlayerRequirementOthers("belphyorscroll");
                    AtOManager.Instance.AddPlayerRequirementOthers("belphyorsummoned");
                    AtOManager.Instance.AddPlayerRequirementOthers("bigfish");
                    AtOManager.Instance.AddPlayerRequirementOthers("boatcenter");
                    AtOManager.Instance.AddPlayerRequirementOthers("boatdown");
                    AtOManager.Instance.AddPlayerRequirementOthers("boatfaenlor");
                    AtOManager.Instance.AddPlayerRequirementOthers("boatfail");
                    AtOManager.Instance.AddPlayerRequirementOthers("boatrime");
                    AtOManager.Instance.AddPlayerRequirementOthers("boatup");
                    AtOManager.Instance.AddPlayerRequirementOthers("caravan");
                    AtOManager.Instance.AddPlayerRequirementOthers("caravannopay");
                    AtOManager.Instance.AddPlayerRequirementOthers("caravanpay");
                    AtOManager.Instance.AddPlayerRequirementOthers("childofthestorm");
                    AtOManager.Instance.AddPlayerRequirementOthers("cranecode");
                    AtOManager.Instance.AddPlayerRequirementOthers("crocomenburn");
                    AtOManager.Instance.AddPlayerRequirementOthers("crocomenhelp");
                    AtOManager.Instance.AddPlayerRequirementOthers("crocomensteal");
                    AtOManager.Instance.AddPlayerRequirementOthers("crossroadnorth");
                    AtOManager.Instance.AddPlayerRequirementOthers("crossroadsouth");
                    AtOManager.Instance.AddPlayerRequirementOthers("eeriecgestiout");
                    AtOManager.Instance.AddPlayerRequirementOthers("elemlava");
                    AtOManager.Instance.AddPlayerRequirementOthers("elemrock");
                    AtOManager.Instance.AddPlayerRequirementOthers("elemstone");
                    AtOManager.Instance.AddPlayerRequirementOthers("elvenarmory");
                    AtOManager.Instance.AddPlayerRequirementOthers("elvenmansion");
                    AtOManager.Instance.AddPlayerRequirementOthers("farminfested");
                    AtOManager.Instance.AddPlayerRequirementOthers("forestrail");
                    AtOManager.Instance.AddPlayerRequirementOthers("freeboat");
                    AtOManager.Instance.AddPlayerRequirementOthers("goblinhelp");
                    AtOManager.Instance.AddPlayerRequirementOthers("goblinnorth");
                    AtOManager.Instance.AddPlayerRequirementOthers("goblinquest");
                    AtOManager.Instance.AddPlayerRequirementOthers("goldensheep");
                    AtOManager.Instance.AddPlayerRequirementOthers("goldensheepquest");
                    AtOManager.Instance.AddPlayerRequirementOthers("goldenwool");
                    AtOManager.Instance.AddPlayerRequirementOthers("goldtrophy");
                    AtOManager.Instance.AddPlayerRequirementOthers("grainsack");
                    AtOManager.Instance.AddPlayerRequirementOthers("hammer");
                    AtOManager.Instance.AddPlayerRequirementOthers("happyowlrunes");
                    AtOManager.Instance.AddPlayerRequirementOthers("harpyegg");
                    AtOManager.Instance.AddPlayerRequirementOthers("hugeruby");
                    AtOManager.Instance.AddPlayerRequirementOthers("impaltar");
                    AtOManager.Instance.AddPlayerRequirementOthers("jeweledkey");
                    AtOManager.Instance.AddPlayerRequirementOthers("keyaquarfall");
                    AtOManager.Instance.AddPlayerRequirementOthers("keynorth");
                    AtOManager.Instance.AddPlayerRequirementOthers("keyobelisk");
                    AtOManager.Instance.AddPlayerRequirementOthers("keyvelkarath");
                    AtOManager.Instance.AddPlayerRequirementOthers("largeemerald");
                    AtOManager.Instance.AddPlayerRequirementOthers("lavacascade");
                    AtOManager.Instance.AddPlayerRequirementOthers("lizardmenhelp");
                    AtOManager.Instance.AddPlayerRequirementOthers("lorequest");
                    AtOManager.Instance.AddPlayerRequirementOthers("magicsapphyre");
                    AtOManager.Instance.AddPlayerRequirementOthers("magictorch");
                    AtOManager.Instance.AddPlayerRequirementOthers("meetraul");
                    AtOManager.Instance.AddPlayerRequirementOthers("merchantcard");
                    AtOManager.Instance.AddPlayerRequirementOthers("minstrelquest");
                    AtOManager.Instance.AddPlayerRequirementOthers("moonstone");
                    AtOManager.Instance.AddPlayerRequirementOthers("mosquitoegg");
                    AtOManager.Instance.AddPlayerRequirementOthers("naganotes");
                    AtOManager.Instance.AddPlayerRequirementOthers("obsidianingots");
                    AtOManager.Instance.AddPlayerRequirementOthers("oldjournal");
                    AtOManager.Instance.AddPlayerRequirementOthers("oldnotes");
                    AtOManager.Instance.AddPlayerRequirementOthers("oldrope");
                    AtOManager.Instance.AddPlayerRequirementOthers("pigcaptured");
                    AtOManager.Instance.AddPlayerRequirementOthers("piratecoin");
                    AtOManager.Instance.AddPlayerRequirementOthers("priestquest");
                    AtOManager.Instance.AddPlayerRequirementOthers("prophetquest");
                    AtOManager.Instance.AddPlayerRequirementOthers("pyromancerquest");
                    AtOManager.Instance.AddPlayerRequirementOthers("rabbitmeat");
                    AtOManager.Instance.AddPlayerRequirementOthers("rime");
                    AtOManager.Instance.AddPlayerRequirementOthers("rimeoftheancienti");
                    AtOManager.Instance.AddPlayerRequirementOthers("rimeoftheancientii");
                    AtOManager.Instance.AddPlayerRequirementOthers("samaritan");
                    AtOManager.Instance.AddPlayerRequirementOthers("sentinelquest");
                    AtOManager.Instance.AddPlayerRequirementOthers("sewersexit");
                    AtOManager.Instance.AddPlayerRequirementOthers("sheeplost");
                    AtOManager.Instance.AddPlayerRequirementOthers("sheeplostnode");
                    AtOManager.Instance.AddPlayerRequirementOthers("slimebait");
                    AtOManager.Instance.AddPlayerRequirementOthers("slimefriend");
                    AtOManager.Instance.AddPlayerRequirementOthers("smalllog");
                    AtOManager.Instance.AddPlayerRequirementOthers("spiderpassage");
                    AtOManager.Instance.AddPlayerRequirementOthers("stargazervisited");
                    AtOManager.Instance.AddPlayerRequirementOthers("testsubject");
                    AtOManager.Instance.AddPlayerRequirementOthers("thiefhealed");
                    AtOManager.Instance.AddPlayerRequirementOthers("treasurehunt");
                    AtOManager.Instance.AddPlayerRequirementOthers("treasurehuntii");
                    AtOManager.Instance.AddPlayerRequirementOthers("treasuremap");
                    AtOManager.Instance.AddPlayerRequirementOthers("treasurespot");
                    AtOManager.Instance.AddPlayerRequirementOthers("tsnemogem");
                    AtOManager.Instance.AddPlayerRequirementOthers("tsnemotip1");
                    AtOManager.Instance.AddPlayerRequirementOthers("tsnemotip2");
                    AtOManager.Instance.AddPlayerRequirementOthers("voidnorth");
                    AtOManager.Instance.AddPlayerRequirementOthers("voidnorthpass");
                    AtOManager.Instance.AddPlayerRequirementOthers("voidsouth");
                    AtOManager.Instance.AddPlayerRequirementOthers("voidsouthpass");
                    AtOManager.Instance.AddPlayerRequirementOthers("wardenquest");
                    AtOManager.Instance.AddPlayerRequirementOthers("waterwatchtower");
                    AtOManager.Instance.AddPlayerRequirementOthers("wolfstory");
                    AtOManager.Instance.AddPlayerRequirementOthers("ylmerseed");
                    AtOManager.Instance.AddPlayerRequirementOthers("youngharpy");
                    AtOManager.Instance.AddPlayerRequirementOthers("zonahielo");
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EventManager), "FinalResolution")]
        public static void FinalResolutionPrefix(ref bool ___groupWinner, ref bool[] ___charWinner, ref bool ___criticalSuccess, ref bool ___criticalFail, EventReplyData ___replySelected, ref EventManager __instance)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPAlwaysSucceed : Plugin.medsAlwaysSucceed.Value)
            {
                ___groupWinner = true;
                for (int index = 0; index < 4; ++index)
                    ___charWinner[index] = true;

                if (!((UnityEngine.Object)___replySelected.SscAddCard1 == (UnityEngine.Object)null) || !((UnityEngine.Object)___replySelected.SscAddCard2 == (UnityEngine.Object)null) || !((UnityEngine.Object)___replySelected.SscAddCard3 == (UnityEngine.Object)null))
                    ___criticalSuccess = true;
                ___criticalFail = false;
            }
            else if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPAlwaysFail : Plugin.medsAlwaysFail.Value)
            {
                ___groupWinner = false;
                for (int index = 0; index < 4; ++index)
                    ___charWinner[index] = false;
                if (!((UnityEngine.Object)___replySelected.SscAddCard1 == (UnityEngine.Object)null) || !((UnityEngine.Object)___replySelected.SscAddCard2 == (UnityEngine.Object)null) || !((UnityEngine.Object)___replySelected.SscAddCard3 == (UnityEngine.Object)null))
                    ___criticalFail = true;
                ___criticalSuccess = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EventManager), "FinalResolution")]
        public static void FinalResolutionPostfix(ref EventManager __instance)
        {
            if (Plugin.medsAutoContinue.Value)
            {
                bool medsStatusReady = Traverse.Create(__instance).Field("statusReady").GetValue<bool>();
                if (!medsStatusReady)
                    __instance.Ready(true);
            }
            if (Plugin.medsSpacebarContinue.Value)
                bFinalResolution = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EventManager), "Start")]
        public static void EventManagerStartPostfix()
        {
            if (Plugin.medsSpacebarContinue.Value)
                bFinalResolution = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EventManager), "Ready")]
        public static void EventManagerReady()
        {
            if (Plugin.medsSpacebarContinue.Value)
                bFinalResolution = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(InputController), "DoKeyBinding")]
        public static bool DoKeyBindingPrefix(ref InputAction.CallbackContext _context)
        {

            if (Plugin.medsSpacebarContinue.Value && bFinalResolution && _context.control == Keyboard.current[Key.Space])
            {
                EventManager.Instance.Ready(true);
                return false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardCraftManager), "CanCraftThisCard")]
        public static void CanCraftThisCardPostfix(ref bool __result)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPCraftCorruptedCards : Plugin.medsCraftCorruptedCards.Value)
                __result = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardCraftManager), "SetMaxQuantity")]
        public static void SetMaxQuantityPrefix(ref int _maxQuantity)
        {
            if ((_maxQuantity >= 0) && (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPInfiniteCardCraft : Plugin.medsInfiniteCardCraft.Value))
                _maxQuantity = -1;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardCraftManager), "GetCardAvailability")]
        public static void GetCardAvailabilityPostfix(ref int[] __result)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPInfiniteCardCraft : Plugin.medsInfiniteCardCraft.Value)
                __result[1] = 99;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "SaveBoughtItem")]
        public static void SaveBoughtItemPostfix()
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPStockedShop : Plugin.medsStockedShop.Value)
            {
                AtOManager.Instance.boughtItems = (Dictionary<string, List<string>>)null;
                AtOManager.Instance.boughtItemInShopByWho = (Dictionary<string, int>)null;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "SaveBoughtItem")]
        public static bool SaveBoughtItemPrefix()
        {

            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPSoloShop : Plugin.medsSoloShop.Value)
                return false;
            return true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "NET_SaveBoughtItem")]
        public static bool NET_SaveBoughtItemPrefix()
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPSoloShop : Plugin.medsSoloShop.Value)
                return false;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), "SetMenuCurrentProfile")]
        public static void SetMenuCurrentProfilePostfix()
        {
            MainMenuManager.Instance.profileMenuText.text += $" (Obeliskial)";
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(HeroSelectionManager), "IsHeroSelected")]
        public static void IsHeroSelectedPostfix(ref bool __result)
        {
            if (Plugin.medsLegalCloning.Value)
                __result = false;
        }

        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(HeroSelectionManager), "SetSeed")]
        public static void SetSeedPrefix(ref string _seed)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                if (_seed = "RAT")

            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroSelection), "OnMouseOver")]
        public static bool OnMouseOverPrefix(ref HeroSelection __instance)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                __instance.blocked = false;
                __instance.SetMultiplayerBlocked(false);
            }
            return true;
        }*/

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SteamManager), "DoSteam")]
        public static bool DoSteamPrefix(ref SteamManager __instance)
        {
            uint releaseAppId = 1385380;
            try
            {
                SteamClient.Init(releaseAppId);
            }
            catch (System.Exception)
            {
                __instance.steamConnected = false;
            }
            if (!__instance.steamConnected)
                return false;
            if (SteamApps.IsSubscribedToApp((AppId)releaseAppId))
                GameManager.Instance.SetDemo(false);
            __instance.steamName = SteamClient.Name;
            __instance.steamId = SteamClient.SteamId;
            if (Plugin.medsDeveloperMode.Value)
                GameManager.Instance.SetDeveloperMode(true);
            __instance.GetDLCInformation();

            SteamFriends.OnGameRichPresenceJoinRequested += new Action<Friend, string>(SupportingActs.OnGameRichPresenceJoinRequested);
            SteamMatchmaking.OnLobbyCreated += new Action<Result, Lobby>(SupportingActs.OnLobbyCreated);
            SteamMatchmaking.OnLobbyMemberJoined += new Action<Lobby, Friend>(SupportingActs.OnLobbyMemberJoined);
            SteamFriends.OnGameLobbyJoinRequested += new Action<Lobby, SteamId>(SupportingActs.OnGameLobbyJoinRequested);
            SteamMatchmaking.OnLobbyEntered += new Action<Lobby>(SupportingActs.OnLobbyEntered);
            SteamMatchmaking.OnLobbyInvite += new Action<Friend, Lobby>(SupportingActs.OnLobbyInvite);
            SteamFriends.OnChatMessage += new Action<Friend, string, string>(SupportingActs.OnChatMessage);
            SteamApps.OnNewLaunchParameters += new Action(SupportingActs.OnNewLaunchParameters);
            SteamApps.GetLaunchParam("+connect_lobby");
            int num = -1;
            string s = "";
            string[] commandLineArgs = Environment.GetCommandLineArgs();
            for (int index = 0; index < commandLineArgs.Length; ++index)
            {
                if (index == num)
                    s = commandLineArgs[index];
                else if (commandLineArgs[index] == "+connect_lobby")
                    num = index + index;
            }
            if (s != "")
            {
                SteamId lobbyId = (SteamId)ulong.Parse(s);
                try
                {
                    SteamMatchmaking.JoinLobbyAsync(lobbyId);
                }
                catch
                {
                    __instance.steamLoaded = false;
                }
            }
            else
                __instance.steamLoaded = true;
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerManager), "GainSupply")]
        public static bool GainSupplyPrefix(ref PlayerManager __instance, ref int quantity)
        {
            __instance.SupplyActual += quantity;
            PlayerUIManager.Instance.SetSupply(true);
            return false;
        }

         /* 
         * [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroSelectionManager), "NET_AssignHeroToBox")]
        public static bool NET_AssignHeroToBoxPrefix(ref string _hero, int _boxId, int _perkRank, string _skinId, string _cardbackId, HeroSelectionManager __instance)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                _hero = _hero.ToLower();
                if (this.SubclassByName.ContainsKey(_hero))
                    _hero = this.SubclassByName[_hero];
                _hero = _hero.ToLower();
                GameObject key = this.boxGO[_boxId];
                if (this.heroSelectionDictionary[_hero].selected)
                    this.heroSelectionDictionary[_hero].Reset();
                if (!this.boxHero.ContainsKey(key) || (UnityEngine.Object)this.boxHero[key] == (UnityEngine.Object)null)
                {
                    this.heroSelectionDictionary[_hero].AssignHeroToBox(this.boxGO[_boxId]);
                    this.heroSelectionDictionary[_hero].SetRankBox(_perkRank);
                    this.heroSelectionDictionary[_hero].SetSkin(_skinId);
                    this.AddToPlayerHeroSkin(_hero, _skinId);
                    this.AddToPlayerHeroCardback(_hero, _cardbackId);
                }
                else
                {
                    if (!(this.boxHero[key].nameTM.text != _hero))
                        return;
                    this.boxHero[key].GoBackToOri();
                    this.heroSelectionDictionary[_hero].AssignHeroToBox(this.boxGO[_boxId]);
                    this.heroSelectionDictionary[_hero].SetRankBox(_perkRank);
                    this.heroSelectionDictionary[_hero].SetSkin(_skinId);
                    this.AddToPlayerHeroSkin(_hero, _skinId);
                    this.AddToPlayerHeroCardback(_hero, _cardbackId);
                }
                return false;
            }
            return true;
        }

        /*
         * 
         * [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroSelectionManager), "BeginAdventure")]
        public static bool BeginAdventurePrefix(ref HeroSelectionManager __instance)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                this.botonBegin.gameObject.SetActive(false);
                if (GameManager.Instance.IsMultiplayer() && (!GameManager.Instance.IsMultiplayer() || !NetworkManager.Instance.IsMaster()))
                    return;
                if (GameManager.Instance.GameStatus == Enums.GameStatus.LoadGame)
                {
                    AtOManager.Instance.DoLoadGameFromMP();
                }
                else
                {
                    string[] strArray = new string[4];
                    for (int index = 0; index < HeroSelectionManager.Instance.boxHero.Count; ++index)
                        strArray[index] = this.boxHero[this.boxGO[index]].GetSubclassName();
                    if (!GameManager.Instance.IsMultiplayer() && !GameManager.Instance.IsWeeklyChallenge())
                    {
                        PlayerManager.Instance.LastUsedTeam = new string[4];
                        for (int index = 0; index < 4; ++index)
                            PlayerManager.Instance.LastUsedTeam[index] = strArray[index].ToLower();
                        SaveManager.SavePlayerData();
                    }
                    if (!GameManager.Instance.IsObeliskChallenge())
                    {
                        AtOManager.Instance.SetPlayerPerks(this.playerHeroPerksDict, strArray);
                        AtOManager.Instance.SetNgPlus(this.ngValue);
                        AtOManager.Instance.SetMadnessCorruptors(this.ngCorruptors);
                    }
                    else if (!GameManager.Instance.IsWeeklyChallenge())
                        AtOManager.Instance.SetObeliskMadness(this.obeliskMadnessValue);
                    AtOManager.Instance.SetTeamFromArray(strArray);
                    AtOManager.Instance.BeginAdventure();
                }
                return false
            }
            return true;
        }

        /*

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroSelection), "PickHero")]
        public static void PickHeroPostfix(ref HeroSelection __instance)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                __instance.nameOver.gameObject.SetActive(true);
                __instance.rankOver.gameObject.SetActive(true);
                __instance.rankTM.gameObject.SetActive(true);
                ////__instance.sprite.GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroSelection), "MoveToBox")]
        public static void MoveToBoxPostfix(ref HeroSelection __instance)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                __instance.nameOver.gameObject.SetActive(true);
                __instance.rankOver.gameObject.SetActive(true);
                __instance.rankTM.gameObject.SetActive(true);
                ////__instance.sprite.GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroSelection), "Start")]
        public static void StartPostfix(ref HeroSelection __instance)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                __instance.nameOver.gameObject.SetActive(true);
                __instance.rankOver.gameObject.SetActive(true);
                __instance.rankTM.gameObject.SetActive(true);
                ////__instance.sprite.GetComponent<SpriteRenderer>().enabled = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroSelectionManager), "NET_AssignHeroToBox")]
        public static bool NET_AssignHeroToBoxPrefix(ref string _hero, int _boxId, int _perkRank, string _skinId, string _cardbackId, ref HeroSelectionManager __instance)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                _hero = _hero.ToLower();
                HeroSelectionManager.Instance.su
                if (__instance.SubclassByName.ContainsKey(_hero))
                    _hero = __instance.SubclassByName[_hero];
                _hero = _hero.ToLower();
                __instance.heroSelectionDictionary[_hero].selected = false;
                GameObject key = __instance.boxGO[_boxId];
                if (!__instance.boxHero.ContainsKey(key) || (UnityEngine.Object)this.boxHero[key] == (UnityEngine.Object)null)
                {
                }

                }
            return true;
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroSelection), "OnMouseDown")]
        public static bool OnMouseDownPrefix(ref HeroSelection __instance)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                __instance.selected = false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HeroSelection), "OnMouseDrag")]
        public static bool OnMouseDragPrefix(ref HeroSelection __instance)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                __instance.selected = false;
            }
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroSelection), "PickHero")]
        public static void PickHeroPostfix(ref HeroSelection __instance)
        {
            if (Plugin.medsLegalCloning.Value)
            {
                __instance.selected = false;
            }
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), "DoCredits")]
        public static void DoCreditsPostfix()
        {
            string str1 = "<size=-1.5><color=#FFF>";
            string str2 = "</color></size>";
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append("Adam Gándara Espart");
            stringBuilder.Append("<br>");
            stringBuilder.Append(str1);
            stringBuilder.Append(Texts.Instance.GetText("gameDesigner"));
            stringBuilder.Append("<br>");
            stringBuilder.Append(Texts.Instance.GetText("gameDeveloper"));
            stringBuilder.Append(str2);
            MainMenuManager.Instance.creditsAdam.text = stringBuilder.ToString();
        }*/

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EmoteManager), "Awake")]
        public static void AwakePostfix(ref EmoteManager __instance)
        {
            if (Plugin.medsEmotional.Value)
            {
                medsPosIni = __instance.characterPortrait.transform.localPosition;
                medsPosIniBlocked = __instance.characterPortraitBlocked.transform.parent.transform.localPosition;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmoteManager), "SelectNextCharacter")]
        public static bool SelectNextCharacterPrefix(ref EmoteManager __instance)
        {
            if (Plugin.medsEmotional.Value)
            {
                Hero[] teamHero = MatchManager.Instance.GetTeamHero();
                if (teamHero != null)
                {
                    int num = 0;
                    bool flag = false;
                    while (!flag && num < 100)
                    {
                        num++;
                        __instance.heroActive++;
                        if (__instance.heroActive > 3)
                            __instance.heroActive = 0;
                        if (teamHero[__instance.heroActive] != null || !GameManager.Instance.IsMultiplayer())
                            flag = true;
                    }
                    if (teamHero[__instance.heroActive] != null && teamHero[__instance.heroActive].HeroData != null)
                    {
                        SpriteRenderer spriteRenderer = __instance.characterPortrait;
                        Sprite sprite = (__instance.characterPortraitBlocked.sprite = teamHero[__instance.heroActive].HeroData.HeroSubClass.StickerBase);
                        spriteRenderer.sprite = sprite;
                        __instance.characterPortrait.transform.localPosition = medsPosIni + new Vector3(teamHero[__instance.heroActive].HeroData.HeroSubClass.StickerOffsetX, 0f, 0f);
                        __instance.characterPortraitBlocked.transform.parent.transform.localPosition = medsPosIniBlocked + new Vector3(teamHero[__instance.heroActive].HeroData.HeroSubClass.StickerOffsetX, 0f, 0f);
                    }
                }
                if (teamHero[__instance.heroActive] == null || !(teamHero[__instance.heroActive].HeroData != null))
                {
                    return false;
                }
                if (teamHero[__instance.heroActive].Alive)
                {
                    for (int i = 0; i < __instance.emotes.Length; i++)
                    {
                        __instance.emotes[i].SetBlocked(_state: false);
                    }
                    return false;
                }
                for (int j = 0; j < __instance.emotes.Length; j++)
                {
                    if (!__instance.EmoteNeedsTarget(j))
                        __instance.emotes[j].SetBlocked(_state: true);
                }
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EmoteManager), "SetBlocked")]
        public static void SetBlockedPrefix(ref bool _state)
        {
            if (Plugin.medsEmotional.Value)
                _state = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), "Multiplayer")]
        public static void MultiplayerPostfix()
        {
            if (Plugin.medsStraya.Value)
                SaveManager.SaveIntoPrefsInt("networkRegion", 1);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MainMenuManager), "JoinMultiplayer")]
        public static void JoinMultiplayerPostfix()
        {
            if (Plugin.medsStraya.Value)
            {
                switch (Plugin.medsStrayaServer.Value)
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
        }


        // Modify Perks
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PerkTree), "CanModify")]
        public static void CanModifyPostfix(ref bool __result)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPModifyPerks : Plugin.medsModifyPerks.Value)
                __result = true;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerkTree), "SelectPerk")]
        public static void SelectPerkPrefix()
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPModifyPerks : Plugin.medsModifyPerks.Value)
                bSelectingPerk = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PerkTree), "SelectPerk")]
        public static void SelectPerkPostfix()
        {
            bSelectingPerk = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "CharInTown")]
        public static void CharInTownPostfix(ref bool __result)
        {
            if (bSelectingPerk)
                __result = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GetTownTier")]
        public static void GetTownTierPostfix(ref int __result)
        {
            if (bSelectingPerk)
                __result = 0;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SettingsManager), "IsActive")]
        public static void SettingsManagerIsActivePostfix(ref bool __result)
        {
            if (bSelectingPerk)
                __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AlertManager), "IsActive")]
        public static void AlertManagerIsActivePostfix(ref bool __result)
        {
            if (bSelectingPerk)
                __result = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MadnessManager), "IsActive")]
        public static void MadnessManagerIsActivePostfix(ref bool __result)
        {
            if (bSelectingPerk)
                __result = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerkNode), "OnMouseUp")]
        public static void OnMouseUpPrefix(ref PerkNode __instance, ref bool ___nodeLocked)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPModifyPerks : Plugin.medsModifyPerks.Value)
            {
                ___nodeLocked = false;
                bSelectingPerk = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PerkNode), "OnMouseUp")]
        public static void OnMouseUpPostfix()
        {
            bSelectingPerk = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerkNode), "OnMouseEnter")]
        public static void OnMouseEnterPrefix(ref PerkNode __instance, ref bool ___nodeLocked)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPModifyPerks : Plugin.medsModifyPerks.Value)
            {
                ___nodeLocked = false;
                bSelectingPerk = true;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PerkNode), "OnMouseEnter")]
        public static void OnMouseEnterPostfix()
        {
            bSelectingPerk = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PerkTree), "Show")]
        public static void ShowPostfix(ref PerkTree __instance, ref int ___totalAvailablePoints)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPModifyPerks : Plugin.medsModifyPerks.Value)
            {
                if ((bool)HeroSelectionManager.Instance && !GameManager.Instance.IsLoadingGame())
                {
                    __instance.buttonReset.gameObject.SetActive(value: true);
                    __instance.buttonImport.gameObject.SetActive(value: true);
                    __instance.buttonExport.gameObject.SetActive(value: true);
                    __instance.saveSlots.gameObject.SetActive(value: true);
                    __instance.buttonConfirm.gameObject.SetActive(value: true);
                    //__instance.buttonConfirm.Enable();
                }
            }
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPPerkPoints : Plugin.medsPerkPoints.Value)
                ___totalAvailablePoints = 1000;
            return;
        }

        // 20230401 ModifyPerks fix?

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerkNode), "SetIconLock")]
        public static void SetIconLockPrefix(ref bool _state)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPModifyPerks : Plugin.medsModifyPerks.Value)
                _state = false;
        }


        /*
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerkTree), "GetPointsAvailable")]
        public static bool GetPointsAvailablePrefix(ref int ___availablePoints)
        {
            if (Plugin.medsPerkPoints.Value)
                ___availablePoints = 500;
            return true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Functions), "GetCardByRarity")]
        public static void GetCardByRarityPostfix(ref string __result, CardData _cardData)
        {
            if (Plugin.medsCorruptGiovanna.Value)
                __result = _cardData?.UpgradesToRare?.Id ?? __result;
        }
        */


        [HarmonyPrefix]
        [HarmonyPatch(typeof(TownManager), "ShowButtons")]
        public static void ShowButtonsPrefix(out int __state)
        {
            __state = AtOManager.Instance.GetNgPlus(false);
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPUseClaimation : Plugin.medsUseClaimation.Value)
                AtOManager.Instance.SetNgPlus(0);

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TownManager), "ShowButtons")]
        public static void ShowButtonsPostfix(int __state)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPUseClaimation : Plugin.medsUseClaimation.Value)
                AtOManager.Instance.SetNgPlus(__state);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), "GetCostReroll")]
        public static void GetCostRerollPostfix(ref int __result)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDiscountDoomroll : Plugin.medsDiscountDoomroll.Value)
            {
                int num1;
                switch (AtOManager.Instance.GetTownTier())
                {
                    case 0:
                        num1 = 150;
                        break;
                    case 1:
                        num1 = 200;
                        break;
                    case 2:
                        num1 = 250;
                        break;
                    default:
                        num1 = 300;
                        break;
                }
                int num2 = 4;
                if (GameManager.Instance.IsMultiplayer())
                {
                    num2 = 0;
                    Hero[] team = AtOManager.Instance.GetTeam();
                    for (int index = 0; index < 4; ++index)
                    {
                        if (team[index].Owner == NetworkManager.Instance.GetPlayerNick())
                            ++num2;
                    }
                }
                int costReroll = num1 * num2;
                float num3 = 1f;
                if (PlayerManager.Instance.PlayerHaveSupply("townUpgrade_5_4"))
                    num3 -= 0.5f;
                float num4 = 1f;
                for (int index = 0; index < 4; ++index)
                    num4 -= (AtOManager.Instance.GetHero(index).GetItemDiscountModification() / 100f);
                __result = Functions.FuncRoundToInt((float)costReroll * num3 * num4);
                if (__result < 0)
                    __result = 0;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), "GetDivinationCost")]
        public static void GetDivinationCostPostfix(ref Globals __instance, ref int __result, ref string divinationTier)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDiscountDivination : Plugin.medsDiscountDivination.Value)
            {
                int divinationCost = 0;
                bool medsOk = false;
                switch (divinationTier)
                {
                    case "0":
                        divinationCost = 400;
                        medsOk = true;
                        break;
                    case "1":
                        divinationCost = 800;
                        medsOk = true;
                        break;
                    case "2":
                        divinationCost = 1600;
                        medsOk = true;
                        break;
                    case "3":
                        divinationCost = 3200;
                        medsOk = true;
                        break;
                    case "4":
                        divinationCost = 5000;
                        medsOk = true;
                        break;
                }
                float num1 = 1f;
                if (PlayerManager.Instance.PlayerHaveSupply("townUpgrade_4_5"))
                    num1 -= 0.4f;
                else if (PlayerManager.Instance.PlayerHaveSupply("townUpgrade_4_3"))
                    num1 -= 0.25f;
                else if (PlayerManager.Instance.PlayerHaveSupply("townUpgrade_4_1"))
                    num1 -= 0.1f;
                float num2 = 1f;
                for (int index = 0; index < 4; ++index)
                    num2 -= (AtOManager.Instance.GetHero(index).GetItemDiscountModification() / 100f);
                __result = Functions.FuncRoundToInt((float)divinationCost * num1 * num2);
                if ((__result < 0) && medsOk)
                    __result = 1;
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "IsTownRerollAvailable")]
        public static void IsTownRerollAvailablePostfix(ref bool __result)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPRavingRerolls : Plugin.medsRavingRerolls.Value)
                __result = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SaveManager), "RestorePlayerData")]
        public static void RestorePlayerDataPostfix()
        {
            if (Plugin.medsJuiceSupplies.Value)
                PlayerManager.Instance.SupplyActual = UnityEngine.Random.Range(500, 999);
        }

        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "SetPlayerDust")]
        public static void SetPlayerDustPrefix(ref int _playerDust)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPJuiceDust : Plugin.medsJuiceDust.Value)
                _playerDust = UnityEngine.Random.Range(500000, 999999);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "SetPlayerGold")]
        public static void SetPlayerGoldPrefix(ref int _playerGold)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPJuiceGold : Plugin.medsJuiceGold.Value)
                _playerGold = UnityEngine.Random.Range(500000, 999999);
        }*/

        [HarmonyPostfix]
        [HarmonyPatch(typeof(TownUpgradeWindow), "SetButtons")]
        public static void SetButtonsPostfix(ref TownUpgradeWindow __instance)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPSmallSanitySupplySelling : Plugin.medsSmallSanitySupplySelling.Value)
                __instance.sellSupplyButton.gameObject.SetActive(true);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardCraftManager), "GetCardAvailability")]
        public static void GetCardAvailabilityPostfix(ref int[] __result, string cardId)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPPlentifulPetPurchases : Plugin.medsPlentifulPetPurchases.Value)
            {
                CardData cardData1 = Globals.Instance.GetCardData(cardId, false);
                if (cardData1.CardUpgraded != Enums.CardUpgraded.No && cardData1.UpgradedFrom != "")
                    cardData1 = Globals.Instance.GetCardData(cardData1.UpgradedFrom.ToLower());
                if (cardData1.CardClass == Enums.CardClass.Item && cardData1.CardType == Enums.CardType.Pet)
                    __result[0] = 0;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LobbyManager), "InitLobby")]
        public static void InitLobbyPostfix(ref LobbyManager __instance)
        {
            if (Plugin.medsMaxMultiplayerMembers.Value)
                __instance.UICreatePlayers.value = 2;
        }

        /////////////////////////////////////////// 20230401 ///////////////////////////////////////////
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapManager), "CanTravelToThisNode")]
        public static void CanTravelToThisNodePostfix(ref bool __result)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPTravelAnywhere : Plugin.medsTravelAnywhere.Value)
                __result = true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerkNode), "SetRequired")]
        public static void SetRequiredPrefix(ref bool _status)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPNoPerkRequirements : Plugin.medsNoPerkRequirements.Value)
                _status = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerkNode), "SetLocked")]
        public static void SetLockedPrefix(ref bool _status)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPNoPerkRequirements : Plugin.medsNoPerkRequirements.Value)
                _status = false;
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(NodeData), "VisibleIfNotRequirement")]
        public static void VisibleIfNotRequirementPostfix(ref bool __result)
        {
            if (Plugin.medsNoTravelRequirements.Value)
            {
                __result = true;
            }
        }*/

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MapManager), "DrawNodes")]
        public static void DrawNodesPrefix(out List<string> __state)
        {
            __state = AtOManager.Instance.mapVisitedNodes;
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPTravelAnywhere : Plugin.medsTravelAnywhere.Value)
                AtOManager.Instance.mapVisitedNodes = new List<string>();
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(MapManager), "DrawNodes")]
        public static void DrawNodesPostfix(List<string> __state)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPTravelAnywhere : Plugin.medsTravelAnywhere.Value)
                AtOManager.Instance.mapVisitedNodes = __state;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "SetCurrentNode")]
        public static void SetCurrentNodePostfix(ref bool __result)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPTravelAnywhere : Plugin.medsTravelAnywhere.Value)
                __result = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GenerateObeliskMap")]
        public static void GenerateObeliskMapPrefix(ref AtOManager __instance, out List<string> __state)
        {
            __state = __instance.mapVisitedNodes;
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPTravelAnywhere : Plugin.medsTravelAnywhere.Value)
                __instance.mapVisitedNodes = new List<string>();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "GenerateObeliskMap")]
        public static void GenerateObeliskMapPostfix(ref AtOManager __instance, List<string> __state)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPTravelAnywhere : Plugin.medsTravelAnywhere.Value)
                __instance.mapVisitedNodes = __state;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIEnergySelector), "TurnOn")]
        public static void TurnOnPrefix(ref UIEnergySelector __instance, ref int maxToBeAssigned)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPOverlyTenergetic : Plugin.medsOverlyTenergetic.Value)
            {
                if (maxToBeAssigned == 0)
                    maxToBeAssigned = 100;
                Traverse.Create(__instance).Field("maxEnergy").SetValue(100);
                Traverse.Create(__instance).Field("maxEnergyToBeAssigned").SetValue(100);
                // int myvalue = int.Parse(Traverse.Create(__instance).Field("maxEnergy").GetValue() as string);
                // Plugin.Log.LogInfo("MYVAL");
                // Plugin.Log.LogInfo(myvalue);
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(Character), "ModifyEnergy")]
        public static void ModifyEnergyPrefix(ref Character __instance, ref int _energy, out int __state)
        {
            __state = __instance.EnergyCurrent + _energy;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Character), "ModifyEnergy")]
        public static void ModifyEnergyPostfix(ref Character __instance, int __state)
        {
            if (__state > 10 && (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPOverlyTenergetic : Plugin.medsOverlyTenergetic.Value))
                __instance.EnergyCurrent = __state;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CardCraftManager), "ShowElements")]
        public static void ShowElementsPrefix(ref CardCraftManager __instance, string cardId)
        {
            if (__instance.craftType == 1) // removing cards
            {
                if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDiminutiveDecks : Plugin.medsDiminutiveDecks.Value)
                    bRemovingCards = true;
                CardData cardData = Globals.Instance.GetCardData(cardId, false);
                switch (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDenyDiminishingDecks : Plugin.medsDenyDiminishingDecks.Value)
                {
                    case "Cannot Remove Cards":
                        bPreventCardRemoval = true;
                        break;
                    case "Cannot Remove Curses":
                        if (cardData.CardClass == Enums.CardClass.Injury)
                            bPreventCardRemoval = true;
                        break;
                    case "Can Only Remove Curses":
                        if (cardData.CardClass != Enums.CardClass.Injury)
                            bPreventCardRemoval = true;
                        break;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CardCraftManager), "ShowElements")]
        public static void ShowElementsPostfix(ref CardCraftManager __instance)
        {
            bRemovingCards = false;
            bPreventCardRemoval = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Hero), "GetTotalCardsInDeck")]
        public static void GetTotalCardsInDeckPostfix(ref int __result)
        {
            if ((bRemovingCards) && (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDiminutiveDecks : Plugin.medsDiminutiveDecks.Value))
                __result = 50;
            if (bPreventCardRemoval)
                __result = 10;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "PlayerHasRequirement")]
        public static void PlayerHasRequirementPostfix(ref bool __result)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPNoPlayerRequirements : Plugin.medsNoPlayerRequirements.Value)
                __result = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "PlayerHasRequirementClass")]
        public static void PlayerHasRequirementClassPostfix(ref bool __result)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPNoPlayerClassRequirements : Plugin.medsNoPlayerClassRequirements.Value)
                __result = true;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "PlayerHasRequirementItem")]
        public static void PlayerHasRequirementItemPostfix(ref int __result)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPNoPlayerItemRequirements : Plugin.medsNoPlayerItemRequirements.Value)
                __result = 0;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetworkManager), "LoadScene")]
        public static void LoadScenePrefix(ref string scene, ref NetworkManager __instance)
        {
            if (scene == "HeroSelection" && GameManager.Instance.IsMultiplayer() && NetworkManager.Instance.IsMaster()) //multiplayer host, going into lobby
            {
                Plugin.SendSettingsMP();
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NetworkManager), "NET_LoadScene")]
        public static bool NET_LoadScenePrefix(ref string scene, ref int gameType)
        {
            if (gameType == 666666)
            {
                Plugin.SaveMPSettings(scene);
                return false;
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "AddItemToHero")]
        public static void AddItemToHeroPrefix(ref string _cardName, ref AtOManager __instance, ref int _heroIndex, out int __state)
        {
            __state = 0;
            CardData cardData = Globals.Instance.GetCardData(_cardName, false);
            if ((UnityEngine.Object)cardData != (UnityEngine.Object)null)
            {
                Hero[] medsTeamAtO = __instance.GetTeam();
                Character character = (Character)medsTeamAtO[_heroIndex];
                __state = character.GetMaxHP();
                if (cardData.CardType == Enums.CardType.Weapon)
                {
                    // max hp bugfix
                    if (Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Weapon, false) != null)
                        __state -= Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Weapon, false).Item.MaxHealth;
                    // bad luck protection
                    Plugin.iShopsWithNoPurchase = 0;
                }
                else if (cardData.CardType == Enums.CardType.Armor)
                {
                    // max hp bugfix
                    if (Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Armor, false) != null)
                        __state -= Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Armor, false).Item.MaxHealth;
                    // bad luck protection
                    Plugin.iShopsWithNoPurchase = 0;
                }
                else if (cardData.CardType == Enums.CardType.Jewelry)
                {
                    // max hp bugfix
                    if (Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Jewelry, false) != null)
                        __state -= Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Jewelry, false).Item.MaxHealth;
                    // bad luck protection
                    Plugin.iShopsWithNoPurchase = 0;
                }
                else if (cardData.CardType == Enums.CardType.Accesory)
                {
                    // max hp bugfix
                    if (Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Accesory, false) != null)
                        __state -= Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Accesory, false).Item.MaxHealth;
                    // bad luck protection
                    Plugin.iShopsWithNoPurchase = 0;
                }
                else if (cardData.CardType == Enums.CardType.Pet)
                {
                    // max hp bugfix
                    if (Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Pet, false) != null)
                        __state -= Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Pet, false).Item.MaxHealth;
                    // don't count pets towards bad luck protection
                }
            }

        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "AddItemToHero")]
        public static void AddItemToHeroPostfix(ref string _cardName, ref AtOManager __instance, ref int _heroIndex, int __state)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPBugfixEquipmentHP : Plugin.medsBugfixEquipmentHP.Value)
            {
                Hero[] medsTeamAtO = __instance.GetTeam();
                Character character = (Character)medsTeamAtO[_heroIndex];
                CardData cardD = Globals.Instance.GetCardData(_cardName, false);
                int medsMaxHP = __state;
                switch (cardD.CardType)
                {
                    case Enums.CardType.Weapon:
                        medsMaxHP += Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Weapon, false).Item.MaxHealth;
                        break;
                    case Enums.CardType.Armor:
                        medsMaxHP += Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Armor, false).Item.MaxHealth;
                        break;
                    case Enums.CardType.Jewelry:
                        medsMaxHP += Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Jewelry, false).Item.MaxHealth;
                        break;
                    case Enums.CardType.Accesory:
                        medsMaxHP += Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Accesory, false).Item.MaxHealth;
                        break;
                    case Enums.CardType.Pet:
                        medsMaxHP += Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Pet, false).Item.MaxHealth;
                        break;
                }
                if (medsMaxHP != character.GetMaxHP())
                    character.ModifyMaxHP(medsMaxHP - character.GetMaxHP());
            }
        }

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(PerkNodeData), "PerkRequired")]
        public static void PerkRequired(ref bool __result)
        {
            if (Plugin.medsNoPerkRequirements.Value)
            {
                __result = true;
            }
        }*/

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CinematicManager), "DoCinematic")]
        public static void DoCinematicPostfix(ref CinematicManager __instance)
        {
            if (Plugin.medsSkipCinematics.Value)
            {
                __instance.SkipCinematic();
            }
        }

        // NEW JUICE METHOD
        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "GetPlayerGold")]
        public static void GetPlayerGoldPrefix(ref AtOManager __instance)
        {
            if (GameManager.Instance.IsMultiplayer() && Plugin.medsMPJuiceGold)
            {
                string medsplayerNick = NetworkManager.Instance.GetPlayerNick();
                Dictionary<string, int> medsmpPlayersGold = __instance.GetMpPlayersGold();
                foreach (var playerKey in medsmpPlayersGold.Keys.ToList())
                {
                    medsmpPlayersGold[playerKey] = UnityEngine.Random.Range(500000, 999999);
                    if (playerKey == medsplayerNick)
                        Traverse.Create(__instance).Field("playerGold").SetValue(medsmpPlayersGold[playerKey]);
                }
            }
            else if (!(GameManager.Instance.IsMultiplayer()) && Plugin.medsJuiceGold.Value)
            {
                Traverse.Create(__instance).Field("playerGold").SetValue(UnityEngine.Random.Range(500000, 999999));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "GetPlayerDust")]
        public static void GetPlayerDustPrefix(ref AtOManager __instance)
        {
            if (GameManager.Instance.IsMultiplayer() && Plugin.medsMPJuiceDust)
            {
                string medsplayerNick = NetworkManager.Instance.GetPlayerNick();
                Dictionary<string, int> medsmpPlayersDust = __instance.GetMpPlayersDust();
                foreach (var playerKey in medsmpPlayersDust.Keys.ToList())
                {
                    medsmpPlayersDust[playerKey] = UnityEngine.Random.Range(500000, 999999);
                    if (playerKey == medsplayerNick)
                        Traverse.Create(__instance).Field("playerDust").SetValue(medsmpPlayersDust[playerKey]);
                }
            }
            else if (!(GameManager.Instance.IsMultiplayer()) && Plugin.medsJuiceDust.Value)
            {
                Traverse.Create(__instance).Field("playerDust").SetValue(UnityEngine.Random.Range(500000, 999999));
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerManager), "GetPlayerSupplyActual")]
        public static void GetPlayerSupplyActualPrefix(ref PlayerManager __instance)
        {
            if (Plugin.medsJuiceSupplies.Value)
                Traverse.Create(__instance).Field("supplyActual").SetValue(UnityEngine.Random.Range(500, 999));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerUIManager), "SetGold")]
        public static void SetGoldPrefix(ref bool animation)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPJuiceGold : Plugin.medsJuiceGold.Value)
                animation = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerUIManager), "SetDust")]
        public static void SetDustPrefix(ref bool animation)
        {
            if (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPJuiceDust : Plugin.medsJuiceDust.Value)
                animation = false;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerUIManager), "SetSupply")]
        public static void SetSupplyPrefix(ref bool animation)
        {
            if (Plugin.medsJuiceSupplies.Value)
                animation = false;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(LobbyManager), "ShowCreate")]
        public static void ShowCreatePostfix(ref LobbyManager __instance)
        {
            if (Plugin.medsMPLoadAutoCreateRoom.Value && GameManager.Instance.GameStatus == Enums.GameStatus.LoadGame)
                __instance.CreateRoom();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HeroSelectionManager), "ShowFollowStatus")]
        public static void ShowFollowStatusPostfix(ref HeroSelectionManager __instance)
        {
            if (Plugin.medsMPLoadAutoReady.Value && GameManager.Instance.GameStatus == Enums.GameStatus.LoadGame && GameManager.Instance.IsMultiplayer())
            {
                Coroutine medsmanualReadyCo = Traverse.Create(__instance).Field("manualReadyCo").GetValue<Coroutine>();
                if (medsmanualReadyCo != null)
                    __instance.StopCoroutine(medsmanualReadyCo);
                Traverse.Create(__instance).Field("statusReady").SetValue(true);
                NetworkManager.Instance.SetManualReady(true);
                __instance.ReadySetButton(true);
            }
        }

        /*
         * 
         * [HarmonyPrefix]
        [HarmonyPatch(typeof(AtOManager), "SetTeamFromArray")]
        public static void SetTeamFromArrayPrefix(ref string[] _team)
        {
            if (Plugin.medsMPSetTeam)
            {
                Plugin.Log.LogInfo("WAS: " + string.Join(", ", _team));
                if (Array.Exists(Plugin.medsSubclassList, element => element == Plugin.medsMPSetTeam1))
                {
                    _team[0] = Plugin.medsMPSetTeam1;
                }
                if (Array.Exists(Plugin.medsSubclassList, element => element == Plugin.medsMPSetTeam2))
                {
                    _team[1] = Plugin.medsMPSetTeam2;
                }
                if (Array.Exists(Plugin.medsSubclassList, element => element == Plugin.medsMPSetTeam3))
                {
                    _team[2] = Plugin.medsMPSetTeam3;
                }
                if (Array.Exists(Plugin.medsSubclassList, element => element == Plugin.medsMPSetTeam4))
                {
                    _team[3] = Plugin.medsMPSetTeam4;
                }
                Plugin.Log.LogInfo("NOW: " + string.Join(", ", _team));
                //if ((GameManager.Instance.IsMultiplayer() && NetworkManager.Instance.IsMaster()) || !GameManager.Instance.IsMultiplayer()) // multiplayer host or not multiplayer
                    //Plugin.medsSetTeam.Value = false;
                //Plugin.medsMPSetTeam = false;
            }
        }
        [HarmonyPostfix]
        [HarmonyPatch(typeof(AtOManager), "SetTeamFromArray")]
        public static void SetTeamFromArrayPostfix(ref string[] _team)
        {
            Plugin.Log.LogInfo("AND: " + string.Join(", ", _team));
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PerkTree), "Show")]
        public static void PerkTreeShowPrefix(ref string _subClassId, int currentHeroIndex)
        {
            if (Plugin.medsMPSetTeam)
            {
                if (currentHeroIndex == -1)
                {
                    // how do we know which hero it's ACTUALLY referring to?
                    // ... do we care?
                    // coming from CharPopup.DoPerks or BotHeroChar.OnMouseUp. don't care if it's coming from BotHeroChar, right? that's from HeroSelection only, so idgaf.
                }
                else
                {
                    switch (currentHeroIndex)
                    {
                        case 0:
                            _subClassId = Plugin.medsMPPerksFrom1;
                            break;
                        case 1:
                            _subClassId = Plugin.medsMPPerksFrom2;
                            break;
                        case 2:
                            _subClassId = Plugin.medsMPPerksFrom3;
                            break;
                        case 3:
                            _subClassId = Plugin.medsMPPerksFrom4;
                            break;
                    }
                }
            }
        }

        // 1st healer replace with
        // 1st healer name
        // 1st scout name
        // 1st scout replace with
        // 1st warrior reaplce with

        //        [HarmonyPostfix]
        //        [HarmonyPatch(typeof(AtOManager), "ClearGame")]
        //[HarmonyPostfix]
        //[HarmonyPatch(typeof(GameManager), "GenerateHeroes")]
        //public static void GenerateHeroesPostfix()
        //{

        //}

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(GameManager), "CreateHero")]
        public static void CreateHeroPostfix(string subClass, ref Hero __result)
        
            __result.GameName = 
        }*/

        /*
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), "CreateGameContent")]
        public static void CreateGameContentPostfix()
        {
            Plugin.Log.LogInfo("ping");
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Globals), "CreateCharClones")]
        public static void CreateCharClonesPrefix()
        {
            Plugin.Log.LogInfo("pong");
        }*/

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Globals), "CreateCharClones")]
        public static void CreateCharClonesPostfix()
        {
            Plugin.Log.LogInfo("CREATECLONES START");
            if (!(GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCClones : Plugin.medsDLCClones.Value))
                return;
            string medsSCDId = "";
            string medsSCDName = "";
            string medsSCDReplaceWith = "";
            // SubClassData medsSCD = new();
            for (int chr = 1; chr <= 3; chr++)
            {
                if (chr == 1)
                {
                    medsSCDId = "medsdlctwo";
                    medsSCDName = "Clone";
                    medsSCDReplaceWith = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneTwo : Plugin.medsDLCCloneTwo.Value);
                }
                else if (chr == 2)
                {
                    medsSCDId = "medsdlcthree";
                    medsSCDName = "Copy";
                    medsSCDReplaceWith = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneThree : Plugin.medsDLCCloneThree.Value);
                }
                else if (chr == 3)
                {
                    medsSCDId = "medsdlcfour";
                    medsSCDName = "Counterfeit";
                    medsSCDReplaceWith = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneFour : Plugin.medsDLCCloneFour.Value);
                }
                SubClassData medsSCD = UnityEngine.Object.Instantiate<SubClassData>(Globals.Instance.SubClass[medsSCDReplaceWith]);
                medsSCD.Id = medsSCDId;
                medsSCD.CharacterName = medsSCDName;
                medsSCD.OrderInList = chr;
                medsSCD.SubClassName = medsSCDId;
                medsSCD.ExpansionCharacter = true;
                if (Globals.Instance.SubClass.ContainsKey(medsSCDId))
                    Globals.Instance.SubClass[medsSCDId] = medsSCD;
                else
                    Globals.Instance.SubClass.Add(medsSCDId, medsSCD);
                Plugin.Log.LogInfo(medsSCDId + " ADDED!");
            }
            Plugin.Log.LogInfo("CREATECLONES END");
        }
        /*[HarmonyPrefix]
        [HarmonyPatch(typeof(Globals), "GetSubClassData")]
        public static void GetSubClassDataPrefix()
        {
            Plugin.Log.LogInfo("pang");
        }*/
        [HarmonyPrefix]
        [HarmonyPatch(typeof(PlayerManager), "IsHeroUnlocked")]
        public static void IsHeroUnlockedPrefix(ref string subclass)
        {
            Plugin.Log.LogInfo("start subclass: " + subclass);
            if (subclass == "medsdlctwo")
                subclass = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneTwo : Plugin.medsDLCCloneTwo.Value);
            else if (subclass == "medsdlcthree")
                subclass = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneThree : Plugin.medsDLCCloneThree.Value);
            else if (subclass == "medsdlcfour")
                subclass = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneFour : Plugin.medsDLCCloneFour.Value);
            /*Plugin.Log.LogInfo("HSM ISU");
            foreach (KeyValuePair<string, HeroSelection> heroSelection in HeroSelectionManager.Instance.heroSelectionDictionary)
            {
                Plugin.Log.LogInfo(heroSelection.Key);
            }*/
            Plugin.Log.LogInfo("final subclass: " + subclass);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Globals), "GetSkinsBySubclass")]
        public static void GetSkinsBySubclassPrefix(ref string id)
        {
            if (id == "medsdlctwo")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneTwo : Plugin.medsDLCCloneTwo.Value);
            else if (id == "medsdlcthree")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneThree : Plugin.medsDLCCloneThree.Value);
            else if (id == "medsdlcfour")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneFour : Plugin.medsDLCCloneFour.Value);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Globals), "GetSkinBaseIdBySubclass")]
        public static void GetSkinBaseIdBySubclassPrefix(ref string id)
        {
            if (id == "medsdlctwo")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneTwo : Plugin.medsDLCCloneTwo.Value);
            else if (id == "medsdlcthree")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneThree : Plugin.medsDLCCloneThree.Value);
            else if (id == "medsdlcfour")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneFour : Plugin.medsDLCCloneFour.Value);
        }
        [HarmonyPrefix]
        [HarmonyPatch(typeof(Globals), "GetCardbackBaseIdBySubclass")]
        public static void GetCardbackBaseIdBySubclassPrefix(ref string id)
        {
            if (id == "medsdlctwo")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneTwo : Plugin.medsDLCCloneTwo.Value);
            else if (id == "medsdlcthree")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneThree : Plugin.medsDLCCloneThree.Value);
            else if (id == "medsdlcfour")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneFour : Plugin.medsDLCCloneFour.Value);
        }
        
        [HarmonyPrefix]
        [HarmonyPatch(typeof(BotonSkin), "OnMouseUp")]
        public static bool BotonSkinOnMouseUpPrefix(ref BotonSkin __instance)
        {
            // This isn't strictly necessary, given skins are cloned, but I suspect I'll use it for custom characters/skins later?
            // Basically, when clicking the button to change skins, this code uses the subclass id attached to the character popup rather than the subclass id attached to the skin.
            string medsSubClassId = HeroSelectionManager.Instance.charPopup.GetActive();
            if (!(medsSubClassId == "medsdlctwo" || medsSubClassId == "medsdlcthree" || medsSubClassId == "medsdlcfour"))
                return true;
            bool medsLocked = Traverse.Create(__instance).Field("locked").GetValue<bool>();
            if (AlertManager.Instance.IsActive() || SettingsManager.Instance.IsActive() || medsLocked)
                return false;
            SkinData medsSkinData = Traverse.Create(__instance).Field("skinData").GetValue<SkinData>();
            PlayerManager.Instance.SetSkin(medsSubClassId, medsSkinData.SkinId);
            HeroSelectionManager.Instance.SetSkinIntoSubclassData(medsSubClassId, medsSkinData.SkinId);
            HeroSelectionManager.Instance.charPopup.DoSkins();
            return false;
            /*Plugin.Log.LogInfo("0" + __instance.transform.parent.name);
            Plugin.Log.LogInfo("1" + __instance.transform.parent.parent.name);
            Plugin.Log.LogInfo("2" + __instance.transform.parent.parent.parent.name);*/
            // CharPopup medsCharPopup = HeroSelectionManager.Instance.charPopup;

            // Plugin.Log.LogInfo("3" + __instance.transform.root.gameObject.GetComponentByName("CharPopup"));
            //CharPopup
            /*
            if (id == "medsdlctwo")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneTwo : Plugin.medsDLCCloneTwo.Value);
            else if (id == "medsdlcthree")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneThree : Plugin.medsDLCCloneThree.Value);
            else if (id == "medsdlcfour")
                id = (GameManager.Instance.IsMultiplayer() ? Plugin.medsMPDLCCloneFour : Plugin.medsDLCCloneFour.Value);
            */
        }
        public static string SubClass2Name()
        {
            string medsName = "";

            return medsName;
        }
        public static void SubClassReplace()
        {

            // the below subclassreplace was 
            /*foreach (KeyValuePair<string, SubClassData> keyValuePair in Globals.Instance.SubClass)
            {
                if ((UnityEngine.Object)keyValuePair.Value != (UnityEngine.Object)null && keyValuePair.Value.MainCharacter)
                {
                    SubClassData medsSCD = keyValuePair.Value as SubClassData;
                    if (keyValuePair.Key == Plugin.)
                    Globals.Instance.SubClass.Remove("")
                }
            }*/
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerManager), "GetProgress")]
        public static void GetProgressPostfix(string _subclassId, ref int __result)
        {
            if (_subclassId == "medsdlctwo" || _subclassId == "medsdlcthree" || _subclassId == "medsdlcfour")
                __result = 50;
        }

        
    }
}
