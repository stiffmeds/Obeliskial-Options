using HarmonyLib;
using Photon.Pun;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static Obeliskial_Essentials.Essentials;
using static Obeliskial_Options.Options;

namespace Obeliskial_Options
{
  [HarmonyPatch]
  internal class Patches
  {
    public static Vector3 medsPosIni;
    public static Vector3 medsPosIniBlocked;
    public static bool bSelectingPerk = false;
    //public static bool bRemovingCards = false;
    //public static bool bPreventCardRemoval = false;
    public static bool bFinalResolution = false;

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuManager), "Start")]
    public static void MMStartPostfix(ref MainMenuManager __instance)
    {
      UpdateDropOnlyItems();
      UpdateAllThePets();
      UpdateVisitAllZones();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Globals), "GetLootData")]
    public static void GetLootDataPostfix(ref LootData __result)
    {
      // Plugin.Log.LogInfo("GETLOOTDATA START, shops with no purchase: " + Plugin.iShopsWithNoPurchase);
      if (__result == (LootData)null)
        return;
      Log.LogDebug("GetLootData uncommon: " + __result.DefaultPercentUncommon + " rare: " + __result.DefaultPercentRare + " epic: " + __result.DefaultPercentEpic + " mythic: " + __result.DefaultPercentMythic);
      // instantiate a new version of the LootData so we're not changing the original values!
      __result = UnityEngine.Object.Instantiate<LootData>(__result);
      if (IsHost() ? medsShopRarity.Value : medsMPShopRarity)
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
          num0 += 1f;
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
        __result.DefaultPercentRare += (float)Math.Pow((float)AtOManager.Instance.GetTownTier() + 1, medsBLPTownTierPower) * num0 * num1 / 50f * medsBLPRareMult;
        __result.DefaultPercentEpic += (float)Math.Pow((float)AtOManager.Instance.GetTownTier() + 1, medsBLPTownTierPower) * num0 * num1 / 50f * medsBLPEpicMult;
        __result.DefaultPercentMythic += (float)Math.Pow((float)AtOManager.Instance.GetTownTier(), medsBLPTownTierPower) * num0 * num1 / 50f * medsBLPMythicMult;
        Log.LogDebug("ShopRarity uncommon: " + __result.DefaultPercentUncommon + " rare: " + __result.DefaultPercentRare + " epic: " + __result.DefaultPercentEpic + " mythic: " + __result.DefaultPercentMythic);
      }
      float fBadLuckProt = IsHost() ? (float)medsShopBadLuckProtection.Value : (float)medsMPShopBadLuckProtection;
      // Plugin.Log.LogInfo("fBadLuckProt over 0??? " + fBadLuckProt);
      if (fBadLuckProt > 0f)
      {
        fBadLuckProt = fBadLuckProt * (float)Math.Pow((float)AtOManager.Instance.GetTownTier() + 1, medsBLPTownTierPower) * (float)Math.Pow((float)iShopsWithNoPurchase, medsBLPRollPower) / 100000;
        Log.LogDebug("fBadLuckPro: " + fBadLuckProt);
        __result.DefaultPercentUncommon += fBadLuckProt * medsBLPUncommonMult;
        __result.DefaultPercentRare += fBadLuckProt * medsBLPRareMult;
        __result.DefaultPercentEpic += fBadLuckProt * medsBLPEpicMult;
        __result.DefaultPercentMythic += fBadLuckProt * medsBLPMythicMult * (float)AtOManager.Instance.GetTownTier();
        if (__result.DefaultPercentMythic >= 100f)
        {
          __result.DefaultPercentMythic = 100f;
          __result.DefaultPercentEpic = 0f;
          __result.DefaultPercentRare = 0f;
          __result.DefaultPercentUncommon = 0f;
        }
        else if (__result.DefaultPercentMythic + __result.DefaultPercentEpic > 100f)
        {
          __result.DefaultPercentEpic = 100f - __result.DefaultPercentMythic;
          __result.DefaultPercentRare = 0f;
          __result.DefaultPercentUncommon = 0f;
        }
        else if (__result.DefaultPercentMythic + __result.DefaultPercentEpic + __result.DefaultPercentRare > 100f)
        {
          __result.DefaultPercentRare = 100f - __result.DefaultPercentMythic - __result.DefaultPercentEpic;
          __result.DefaultPercentUncommon = 0f;
        }
        else if (__result.DefaultPercentMythic + __result.DefaultPercentEpic + __result.DefaultPercentRare + __result.DefaultPercentUncommon > 100f)
        {
          __result.DefaultPercentUncommon = 100f - __result.DefaultPercentMythic - __result.DefaultPercentEpic - __result.DefaultPercentRare;
        }
        Log.LogDebug("BadLuckProt uncommon: " + __result.DefaultPercentUncommon + " rare: " + __result.DefaultPercentRare + " epic: " + __result.DefaultPercentEpic + " mythic: " + __result.DefaultPercentMythic);
        iShopsWithNoPurchase += 1;
        Log.LogDebug("shops with no purchase increased to: " + iShopsWithNoPurchase);
      }
      //Plugin.Log.LogDebug("end of GetLootData");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Loot), "GetLootItems", new Type[] { typeof(string), typeof(string) })]
    public static void GetLootItemsPostfix(ref List<string> __result, string _itemListId, string _idAux = "")
    {
      LogDebug("_idAux: " + _idAux);
      LogDebug("_itemListId: " + _itemListId);
      LogDebug("GetlootItems Result: " + String.Join(", ", __result));
      // Plugin.Log.LogDebug($"rare commencement madnessdif: {AtOManager.Instance.GetMadnessDifficulty()}! obeliskmadness: {AtOManager.Instance.GetObeliskMadness()}! ngplus: {AtOManager.Instance.GetNgPlus()}! {__result.Count}!");
      if (__result != null)
      {
        //UnityEngine.Random.InitState((AtOManager.Instance.GetGameId() + AtOManager.Instance.currentMapNode).GetDeterministicHashCode());
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
            if (!(AtOManager.Instance.corruptionId == "exoticshop") && !(AtOManager.Instance.corruptionId == "rareshop") && !(AtOManager.Instance.corruptionId == "shop") && !(AtOManager.Instance.CharInTown()) && medsLootCorrupt.Value)
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
              bAllowCorrupt = IsHost() ? medsTownShopCorrupt.Value : medsMPTownShopCorrupt;
            }
            else if ((AtOManager.Instance.corruptionId == "exoticshop") || (AtOManager.Instance.corruptionId == "rareshop") || (AtOManager.Instance.corruptionId == "shop"))
            {
              // challenge shop
              bAllowCorrupt = IsHost() ? medsObeliskShopCorrupt.Value : medsMPObeliskShopCorrupt;
            }
            else
            {
              // node shop? I can't imagine what else this could be.
              bAllowCorrupt = IsHost() ? medsMapShopCorrupt.Value : medsMPMapShopCorrupt;
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
      if (medsProfane.Value)
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
      if (IsHost() && ChallengeSelectionManager.Instance == null ? medsCorruptGiovanna.Value : medsMPCorruptGiovanna)
        __result = _cardData?.UpgradesToRare?.Id ?? __result;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TownManager), "Start")]
    public static void StartPostfix()
    {
      // last updated... 1.0.0, maybe? need to do it again (or just move to PlayerReq method)
      if (IsHost() ? medsKeyItems.Value : medsMPKeyItems)
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EventManager), "FinalResolution")]
    public static void FinalResolutionPostfix(ref EventManager __instance)
    {
      if (medsAutoContinue.Value)
      {
        bool medsStatusReady = Traverse.Create(__instance).Field("statusReady").GetValue<bool>();
        if (!medsStatusReady)
          __instance.Ready(true);
      }
      if (medsSpacebarContinue.Value)
        bFinalResolution = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EventManager), "Start")]
    public static void EventManagerStartPostfix()
    {
      if (medsSpacebarContinue.Value)
        bFinalResolution = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(EventManager), "Ready")]
    public static void EventManagerReady()
    {
      if (medsSpacebarContinue.Value)
        bFinalResolution = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CardCraftManager), "CanCraftThisCard")]
    public static void CanCraftThisCardPostfix(ref bool __result)
    {
      if (IsHost() ? medsCraftCorruptedCards.Value : medsMPCraftCorruptedCards)
        __result = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardCraftManager), "SetMaxQuantity")]
    public static void SetMaxQuantityPrefix(ref int _maxQuantity)
    {
      if ((_maxQuantity >= 0) && (IsHost() ? medsInfiniteCardCraft.Value : medsMPInfiniteCardCraft))
        _maxQuantity = -1;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CardCraftManager), "GetCardAvailability")]
    public static void GetCardAvailabilityPostfix(ref int[] __result)
    {
      if (IsHost() ? medsInfiniteCardCraft.Value : medsMPInfiniteCardCraft)
        __result[1] = 99;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AtOManager), "SaveBoughtItem")]
    public static void SaveBoughtItemPostfix()
    {
      if (IsHost() ? medsStockedShop.Value : medsMPStockedShop)
      {
        AtOManager.Instance.boughtItems = (Dictionary<string, List<string>>)null;
        AtOManager.Instance.boughtItemInShopByWho = (Dictionary<string, int>)null;
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AtOManager), "SaveBoughtItem")]
    public static bool SaveBoughtItemPrefix()
    {

      if (IsHost() ? medsSoloShop.Value : medsMPSoloShop)
        return false;
      return true;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(AtOManager), "NET_SaveBoughtItem")]
    public static bool NET_SaveBoughtItemPrefix()
    {
      if (IsHost() ? medsSoloShop.Value : medsMPSoloShop)
        return false;
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerManager), "GainSupply")]
    public static bool GainSupplyPrefix(ref PlayerManager __instance, ref int quantity)
    {
      __instance.SupplyActual += quantity;
      PlayerUIManager.Instance.SetSupply(true);
      return false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuManager), "Multiplayer")]
    public static void MultiplayerPostfix()
    {
      if (medsStraya.Value)
        SaveServerSelection();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(MainMenuManager), "JoinMultiplayer")]
    public static void JoinMultiplayerPostfix()
    {
      if (medsStraya.Value)
        SaveServerSelection();
    }


    // Modify Perks
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PerkTree), "CanModify")]
    public static void CanModifyPostfix(ref bool __result)
    {
      if (IsHost() ? medsModifyPerks.Value : medsMPModifyPerks)
        __result = true;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(PerkTree), "SelectPerk")]
    public static void SelectPerkPrefix()
    {
      if (IsHost() ? medsModifyPerks.Value : medsMPModifyPerks)
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
    public static void OnMouseUpPrefix(ref PerkNode __instance)
    {
      if (IsHost() ? medsModifyPerks.Value : medsMPModifyPerks)
      {
        Traverse.Create(__instance).Field("nodeLocked").SetValue(false);
        __instance.iconLock.gameObject.SetActive(false);
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
    public static void OnMouseEnterPrefix(ref PerkNode __instance)
    {
      if (IsHost() ? medsModifyPerks.Value : medsMPModifyPerks)
      {
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
      if (IsHost() ? medsModifyPerks.Value : medsMPModifyPerks)
      {
        __instance.buttonReset.gameObject.SetActive(value: true);
        __instance.buttonImport.gameObject.SetActive(value: true);
        __instance.buttonExport.gameObject.SetActive(value: true);
        __instance.saveSlots.gameObject.SetActive(value: true);
        __instance.buttonConfirm.gameObject.SetActive(value: true);
        //__instance.buttonConfirm.Enable();
      }
      if (IsHost() ? medsPerkPoints.Value : medsMPPerkPoints)
        ___totalAvailablePoints = 1000;
      return;
    }

    // 20230401 ModifyPerks fix?

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PerkNode), "SetIconLock")]
    public static void SetIconLockPrefix(ref bool _state)
    {
      if (IsHost() ? medsModifyPerks.Value : medsMPModifyPerks)
        _state = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PerkNode), "SetLocked")]
    public static void SetLockedPrefix(ref bool _status)
    {
      if (IsHost() ? medsModifyPerks.Value : medsMPModifyPerks)
        _status = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(TownManager), "ShowButtons")]
    public static void ShowButtonsPrefix(out int __state)
    {
      __state = AtOManager.Instance.GetNgPlus(false);
      if (IsHost() ? medsUseClaimation.Value : medsMPUseClaimation)
        AtOManager.Instance.SetNgPlus(0);

    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TownManager), "ShowButtons")]
    public static void ShowButtonsPostfix(int __state)
    {
      if (IsHost() ? medsUseClaimation.Value : medsMPUseClaimation)
        AtOManager.Instance.SetNgPlus(__state);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Globals), "GetCostReroll")]
    public static void GetCostRerollPostfix(ref int __result)
    {
      if (IsHost() ? medsDiscountDoomroll.Value : medsMPDiscountDoomroll)
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
      if (IsHost() ? medsDiscountDivination.Value : medsMPDiscountDivination)
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
          __result = 0;
      }
    }


    [HarmonyPostfix]
    [HarmonyPatch(typeof(AtOManager), "IsTownRerollAvailable")]
    public static void IsTownRerollAvailablePostfix(ref bool __result)
    {
      if (IsHost() ? medsRavingRerolls.Value : medsMPRavingRerolls)
        __result = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SaveManager), "RestorePlayerData")]
    public static void RestorePlayerDataPostfix()
    {
      if (medsJuiceSupplies.Value)
        PlayerManager.Instance.SupplyActual = UnityEngine.Random.Range(500, 999);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(TownUpgradeWindow), "SetButtons")]
    public static void SetButtonsPostfix(ref TownUpgradeWindow __instance)
    {
      if (IsHost() ? medsSmallSanitySupplySelling.Value : medsMPSmallSanitySupplySelling)
        __instance.sellSupplyButton.gameObject.SetActive(true);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CardCraftManager), "GetCardAvailability")]
    public static void GetCardAvailabilityPostfix(ref int[] __result, string cardId)
    {
      if (IsHost() ? medsPlentifulPetPurchases.Value : medsMPPlentifulPetPurchases)
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
      if (medsMaxMultiplayerMembers.Value)
        __instance.UICreatePlayers.value = 2;
    }

    /////////////////////////////////////////// 20230401 ///////////////////////////////////////////
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapManager), "CanTravelToThisNode")]
    public static void CanTravelToThisNodePostfix(ref bool __result)
    {
      if (IsHost() ? medsTravelAnywhere.Value : medsMPTravelAnywhere)
        __result = true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PerkNode), "SetRequired")]
    public static void SetRequiredPrefix(ref bool _status)
    {
      if (IsHost() ? medsNoPerkRequirements.Value : medsMPNoPerkRequirements)
        _status = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapManager), "DrawNodes")]
    public static void DrawNodesPrefix(out List<string> __state)
    {
      __state = AtOManager.Instance.mapVisitedNodes;
      if (IsHost() ? medsTravelAnywhere.Value : medsMPTravelAnywhere)
        AtOManager.Instance.mapVisitedNodes = new List<string>();
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(MapManager), "DrawNodes")]
    public static void DrawNodesPostfix(List<string> __state)
    {
      if (IsHost() ? medsTravelAnywhere.Value : medsMPTravelAnywhere)
        AtOManager.Instance.mapVisitedNodes = __state;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AtOManager), "SetCurrentNode")]
    public static void SetCurrentNodePostfix(ref bool __result)
    {
      if (IsHost() ? medsTravelAnywhere.Value : medsMPTravelAnywhere)
        __result = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AtOManager), "GenerateObeliskMap")]
    public static void GenerateObeliskMapPrefix(ref AtOManager __instance, out List<string> __state)
    {
      __state = __instance.mapVisitedNodes;
      if (IsHost() ? medsTravelAnywhere.Value : medsMPTravelAnywhere)
        __instance.mapVisitedNodes = new List<string>();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AtOManager), "GenerateObeliskMap")]
    public static void GenerateObeliskMapPostfix(ref AtOManager __instance, List<string> __state)
    {
      if (IsHost() ? medsTravelAnywhere.Value : medsMPTravelAnywhere)
        __instance.mapVisitedNodes = __state;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(UIEnergySelector), "TurnOn")]
    public static void TurnOnPrefix(ref UIEnergySelector __instance, ref int maxToBeAssigned)
    {
      if (IsHost() ? medsOverlyTenergetic.Value : medsMPOverlyTenergetic)
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
      if (!(__instance.IsHero))
        return;
      if (__instance != null && __instance.IsHero && __state > 10 && (IsHost() ? medsOverlyTenergetic.Value : medsMPOverlyTenergetic) && (UnityEngine.Object)__instance.HeroItem != (UnityEngine.Object)null && (UnityEngine.Object)__instance.HeroItem.energyTxt != (UnityEngine.Object)null)
      {
        //Plugin.Log.LogDebug(__instance.GameName);
        //Plugin.Log.LogDebug(__instance.IsHero);
        //Plugin.Log.LogDebug(__instance.Id);
        //Plugin.Log.LogDebug(__state.ToString());
        __instance.EnergyCurrent = __state;
        //Plugin.Log.LogDebug("Why are any of us here");
        __instance.HeroItem.energyTxt.text = __instance.EnergyCurrent.ToString();
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardCraftManager), "ShowElements")]
    public static bool ShowElementsPrefix(ref CardCraftManager __instance, string cardId, string direction)
    {
      if (__instance.craftType == 1) // removing cards
      {
        CardData cardData = Globals.Instance.GetCardData(cardId, false);
        if ((UnityEngine.Object)cardData == (UnityEngine.Object)null)
          return true;
        BotonGeneric medsButtonRemove = Traverse.Create(__instance).Field("BG_Remove").GetValue<BotonGeneric>();
        bool flag = true;
        if (direction == "")
        {
          medsButtonRemove.gameObject.SetActive(false);
          __instance.transformRemoveText.gameObject.SetActive(false);
        }
        else
        {
          medsButtonRemove.gameObject.SetActive(true);
          __instance.transformRemoveText.gameObject.SetActive(true);
          if (!__instance.CanBuy("Remove"))
            flag = false;
          Hero medsHero = Traverse.Create(__instance).Field("currentHero").GetValue<Hero>();
          if (medsHero.GetTotalCardsInDeck(true) <= (IsHost() ? medsDiminutiveDecks.Value : medsMPDiminutiveDecks) && cardData.CardClass != Enums.CardClass.Injury && cardData.CardClass != Enums.CardClass.Boon)
            flag = false;
          switch (IsHost() ? medsDenyDiminishingDecks.Value : medsMPDenyDiminishingDecks)
          {
            case "Cannot Remove Cards":
              flag = false;
              break;
            case "Cannot Remove Curses":
              if (cardData.CardClass == Enums.CardClass.Injury)
                flag = false;
              break;
            case "Can Only Remove Curses":
              if (cardData.CardClass != Enums.CardClass.Injury)
                flag = false;
              break;
          }
          if (flag)
          {
            medsButtonRemove.Enable();
          }
          else
          {
            medsButtonRemove.Disable();
          }
        }
        return false;
      }
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetworkManager), "LoadScene")]
    public static void LoadScenePrefix(ref string scene, ref NetworkManager __instance)
    {
      if (scene == "HeroSelection") // going into lobby
      {
        if (IsHost())
          SendSettingsMP();
        else if (!GameManager.Instance.IsMultiplayer())
        {
          UpdateDropOnlyItems();
          UpdateAllThePets();
          UpdateVisitAllZones();
        }
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NetworkManager), "NET_LoadScene")]
    public static bool NET_LoadScenePrefix(ref string scene, ref int gameType)
    {
      if (gameType == 666666)
      {
        SaveMPSettings(scene);
        return false;
      }
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AtOManager), "AddItemToHero")]
    public static void AddItemToHeroPrefix(ref string itemToAddName, ref AtOManager __instance, ref int _heroIndex, out int __state)
    {
      __state = 0;
      CardData cardData = Globals.Instance.GetCardData(itemToAddName, false);
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
          iShopsWithNoPurchase = 0;
        }
        else if (cardData.CardType == Enums.CardType.Armor)
        {
          // max hp bugfix
          if (Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Armor, false) != null)
            __state -= Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Armor, false).Item.MaxHealth;
          // bad luck protection
          iShopsWithNoPurchase = 0;
        }
        else if (cardData.CardType == Enums.CardType.Jewelry)
        {
          // max hp bugfix
          if (Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Jewelry, false) != null)
            __state -= Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Jewelry, false).Item.MaxHealth;
          // bad luck protection
          iShopsWithNoPurchase = 0;
        }
        else if (cardData.CardType == Enums.CardType.Accesory)
        {
          // max hp bugfix
          if (Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Accesory, false) != null)
            __state -= Globals.Instance.GetCardData(medsTeamAtO[_heroIndex].Accesory, false).Item.MaxHealth;
          // bad luck protection
          iShopsWithNoPurchase = 0;
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
    public static void AddItemToHeroPostfix(ref string itemToAddName, ref AtOManager __instance, ref int _heroIndex, int __state)
    {
      if (IsHost() ? medsBugfixEquipmentHP.Value : medsMPBugfixEquipmentHP)
      {
        Hero[] medsTeamAtO = __instance.GetTeam();
        Character character = (Character)medsTeamAtO[_heroIndex];
        CardData cardD = Globals.Instance.GetCardData(itemToAddName, false);
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

    [HarmonyPostfix]
    [HarmonyPatch(typeof(CinematicManager), "DoCinematic")]
    public static void DoCinematicPostfix(ref CinematicManager __instance)
    {
      if (medsSkipCinematics.Value)
        __instance.SkipCinematic();
    }

    // NEW JUICE METHOD
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AtOManager), "GetPlayerGold")]
    public static void GetPlayerGoldPrefix(ref AtOManager __instance)
    {
      if (GameManager.Instance.IsMultiplayer() && medsMPJuiceGold)
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
      else if (!(GameManager.Instance.IsMultiplayer()) && medsJuiceGold.Value)
      {
        Traverse.Create(__instance).Field("playerGold").SetValue(UnityEngine.Random.Range(500000, 999999));
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AtOManager), "GetPlayerDust")]
    public static void GetPlayerDustPrefix(ref AtOManager __instance)
    {
      if (GameManager.Instance.IsMultiplayer() && medsMPJuiceDust)
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
      else if (!(GameManager.Instance.IsMultiplayer()) && medsJuiceDust.Value)
      {
        Traverse.Create(__instance).Field("playerDust").SetValue(UnityEngine.Random.Range(500000, 999999));
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerManager), "GetPlayerSupplyActual")]
    public static void GetPlayerSupplyActualPrefix(ref PlayerManager __instance)
    {
      if (medsJuiceSupplies.Value)
        Traverse.Create(__instance).Field("supplyActual").SetValue(UnityEngine.Random.Range(500, 999));
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerUIManager), "SetGold")]
    public static void SetGoldPrefix(ref bool animation)
    {
      if (IsHost() ? medsJuiceGold.Value : medsMPJuiceGold)
        animation = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerUIManager), "SetDust")]
    public static void SetDustPrefix(ref bool animation)
    {
      if (IsHost() ? medsJuiceDust.Value : medsMPJuiceDust)
        animation = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerUIManager), "SetSupply")]
    public static void SetSupplyPrefix(ref bool animation)
    {
      if (medsJuiceSupplies.Value)
        animation = false;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(LobbyManager), "ShowCreate")]
    public static void ShowCreatePostfix(ref LobbyManager __instance)
    {
      if (medsMPLoadAutoCreateRoom.Value && GameManager.Instance.GameStatus == Enums.GameStatus.LoadGame)
        __instance.CreateRoom();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(HeroSelectionManager), "ShowFollowStatus")]
    public static void ShowFollowStatusPostfix(ref HeroSelectionManager __instance)
    {
      if (medsMPLoadAutoReady.Value && GameManager.Instance.GameStatus == Enums.GameStatus.LoadGame && GameManager.Instance.IsMultiplayer())
      {
        Coroutine medsmanualReadyCo = Traverse.Create(__instance).Field("manualReadyCo").GetValue<Coroutine>();
        if (medsmanualReadyCo != null)
          __instance.StopCoroutine(medsmanualReadyCo);
        Traverse.Create(__instance).Field("statusReady").SetValue(true);
        NetworkManager.Instance.SetManualReady(true);
        __instance.ReadySetButton(true);
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerManager), "IsHeroUnlocked")]
    public static void IsHeroUnlockedPrefix(ref string subclass)
    {
      if (subclass == "medsdlctwo")
        subclass = (IsHost() ? medsDLCCloneTwo.Value : medsMPDLCCloneTwo);
      else if (subclass == "medsdlcthree")
        subclass = (IsHost() ? medsDLCCloneThree.Value : medsMPDLCCloneThree);
      else if (subclass == "medsdlcfour")
        subclass = (IsHost() ? medsDLCCloneFour.Value : medsMPDLCCloneFour);
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
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerManager), "GetProgress")]
    public static void GetProgressPrefix(ref string _subclassId)
    {
      if (_subclassId == "medsdlctwo")
        _subclassId = (IsHost() ? medsDLCCloneTwo.Value : medsMPDLCCloneTwo);
      else if (_subclassId == "medsdlcthree")
        _subclassId = (IsHost() ? medsDLCCloneThree.Value : medsMPDLCCloneThree);
      else if (_subclassId == "medsdlcfour")
        _subclassId = (IsHost() ? medsDLCCloneFour.Value : medsMPDLCCloneFour);
    }
    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerManager), "GetProgress")]
    public static void GetProgressPostfix(ref int __result, string _subclassId)
    {
      if (!medsOver50s.Value)
      {
        if (__result > 95500)
        {
          __result = 95500;
          PlayerManager.Instance.HeroProgress[_subclassId] = __result;
          SaveManager.SavePlayerData();
        }
      }
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Globals), "Awake")]
    public static void GlobalsAwakePostfix()
    {
      if (medsOver50s.Value)
      {
        List<int> medsPerkLevel = Globals.Instance.PerkLevel;
        for (int a = 1; a <= 950; a++)
          Globals.Instance.PerkLevel.Add(Globals.Instance.PerkLevel[Globals.Instance.PerkLevel.Count - 1] + 4000 + a * 100);
        Traverse.Create(Globals.Instance).Field("_PerkLevel").SetValue(medsPerkLevel);
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(PlayerManager), "ModifyProgress")]
    public static void ModifyProgressPrefix(ref string _subclassId)
    {
      if (_subclassId == "medsdlctwo")
        _subclassId = (IsHost() ? medsDLCCloneTwo.Value : medsMPDLCCloneTwo);
      else if (_subclassId == "medsdlcthree")
        _subclassId = (IsHost() ? medsDLCCloneThree.Value : medsMPDLCCloneThree);
      else if (_subclassId == "medsdlcfour")
        _subclassId = (IsHost() ? medsDLCCloneFour.Value : medsMPDLCCloneFour);
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ConflictManager), "EnableButtonsForPlayerChoosing")]
    public static void EnableButtonsForPlayerChoosingPostfix(ref ConflictManager __instance)
    {
      Hero[] medsHeroes = Traverse.Create(__instance).Field("heroes").GetValue<Hero[]>();
      if (!(medsHeroes[__instance.playerChoosing].Owner == NetworkManager.Instance.GetPlayerNick())) // don't press buttons if not the person that gets to choose!
        return;
      int medsMethod = IsHost() ? medsConflictResolution.Value : medsMPConflictResolution;
      Log.LogDebug("medsMethod: " + medsMethod);
      switch (medsMethod)
      {
        case 1:
          Log.LogDebug("pressing button 0");
          MapManager.Instance.ConflictSelection(0);
          break;
        case 2:
          Log.LogDebug("pressing button 1");
          MapManager.Instance.ConflictSelection(1);
          break;
        case 3:
          Log.LogDebug("pressing button 2");
          MapManager.Instance.ConflictSelection(2);
          break;
        case 4:
          int medsRandom = UnityEngine.Random.Range(0, 3);
          Log.LogDebug("pressing button " + medsRandom);
          MapManager.Instance.ConflictSelection(medsRandom);
          break;
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardItem), "OnMouseUpController")]
    public static bool OnMouseUpControllerPrefix(ref CardItem __instance)
    {
      if (!medsEmotional.Value)
        return true;
      if ((bool)(UnityEngine.Object)MatchManager.Instance && !MatchManager.Instance.IsYourTurn() && !MatchManager.Instance.IsYourTurnForAddDiscard())
      {
        Log.LogDebug("onmouseup, considered not my turn");
        if (__instance.cardfordiscard || __instance.cardforaddcard)
        {

          MatchManager.Instance.SendEmoteCard(100 + int.Parse(__instance.name.Replace("TMP_", "")));
        }
        else
        {
          MatchManager.Instance.SendEmoteCard(__instance.tablePosition);
        }
        return false;
      }
      else if (__instance.cardoutsideloot && (UnityEngine.Object)LootManager.Instance != (UnityEngine.Object)null && !LootManager.Instance.IsMyLoot)
      {

        /*return false;*/
      }
      else if (__instance.cardoutsidereward && (UnityEngine.Object)RewardsManager.Instance != (UnityEngine.Object)null)
      {
        /*if (__instance.disableT.gameObject.activeSelf) // if 'greyed out' on rewards screen
        {
            string[] splitName = __instance.name.Split("_");
            if (splitName.Length > 2) {
                PhotonView medsPhotonView = Traverse.Create(RewardsManager.Instance).Field("photonView").GetValue<PhotonView>();
                int index = int.Parse(splitName[splitName.Length - 2]) + 1000;
                Hero[] team = AtOManager.Instance.GetTeam();
                for (int heroInt = 0; heroInt < 4; ++heroInt)
                {
                    if (team[heroInt].Owner == NetworkManager.Instance.GetPlayerNick())
                    {
                        medsPhotonView.RPC("NET_CardSelected", RpcTarget.All, (object)(short)index, (object)__instance.name + "|" + team[heroInt].SubclassName);
                        return false;
                    }
                }
            }
        }*/
      }
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardItem), "OnMouseUp")]
    public static bool OnMouseUpPrefix(ref CardItem __instance)
    {
      if (!medsEmotional.Value)
        return true;
      if ((bool)(UnityEngine.Object)MatchManager.Instance && !MatchManager.Instance.IsYourTurn() && !MatchManager.Instance.IsYourTurnForAddDiscard())
      {
        Log.LogDebug("onmouseup, considered not my turn");
        if (__instance.cardfordiscard || __instance.cardforaddcard)
        {

          MatchManager.Instance.SendEmoteCard(100 + int.Parse(__instance.name.Replace("TMP_", "")));
        }
        else
        {
          MatchManager.Instance.SendEmoteCard(__instance.tablePosition);
        }
        return false;
      }
      else if (__instance.cardoutsideloot && (UnityEngine.Object)LootManager.Instance != (UnityEngine.Object)null && !LootManager.Instance.IsMyLoot)
      {

        /*return false;*/
      }
      else if (__instance.cardoutsidereward && (UnityEngine.Object)RewardsManager.Instance != (UnityEngine.Object)null)
      {
        /*if (__instance.disableT.gameObject.activeSelf) // if 'greyed out' on rewards screen
        {
            string[] splitName = __instance.name.Split("_");
            if (splitName.Length > 2) {
                PhotonView medsPhotonView = Traverse.Create(RewardsManager.Instance).Field("photonView").GetValue<PhotonView>();
                int index = int.Parse(splitName[splitName.Length - 2]) + 1000;
                Hero[] team = AtOManager.Instance.GetTeam();
                for (int heroInt = 0; heroInt < 4; ++heroInt)
                {
                    if (team[heroInt].Owner == NetworkManager.Instance.GetPlayerNick())
                    {
                        medsPhotonView.RPC("NET_CardSelected", RpcTarget.All, (object)(short)index, (object)__instance.name + "|" + team[heroInt].SubclassName);
                        return false;
                    }
                }
            }
        }*/
      }
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(RewardsManager), "NET_CardSelected")]
    public static bool NET_CardSelectedPrefix(ref RewardsManager __instance, short _index, string cardId)
    {
      /*int index = _index;
      if (index >= 1000)
      {
          index -= 1000;
          if (cardId.Split("|").Length < 2 || __instance.characterRewardArray[index] != null && __instance.characterRewardArray[index].GetComponent<CharacterReward>() != null && __instance.characterRewardArray[index].GetComponent<CharacterReward>().cardsByInternalId[cardId.Split("|")[0]] != null)
          {
              CardItem medsCI = __instance.characterRewardArray[index].GetComponent<CharacterReward>().cardsByInternalId[cardId.Split("|")[0]];
              SubClassData medsSCD = Globals.Instance.GetSubClassData(cardId.Split("|")[1]);
              if (medsSCD == null)
                  return false;
              Sprite stickerBase = medsSCD.StickerBase;
              if ((UnityEngine.Object)medsCI.emote0.sprite == (UnityEngine.Object)stickerBase)
              {
                  medsCI.emote0.sprite = (Sprite)null;
                  medsCI.emote0.gameObject.SetActive(false);
              }
              else if ((UnityEngine.Object)medsCI.emote1.sprite == (UnityEngine.Object)stickerBase)
              {
                  medsCI.emote1.sprite = (Sprite)null;
                  medsCI.emote1.gameObject.SetActive(false);
              }
              else if ((UnityEngine.Object)medsCI.emote2.sprite == (UnityEngine.Object)stickerBase)
              {
                  medsCI.emote2.sprite = (Sprite)null;
                  medsCI.emote2.gameObject.SetActive(false);
              }
              else if ((UnityEngine.Object)medsCI.emote0.sprite == (UnityEngine.Object)null)
              {
                  medsCI.emote0.sprite = stickerBase;
                  medsCI.emote0.gameObject.SetActive(true);
                  medsCI.emote0.sortingOrder = 20100 + index * 10 + 1;
              }
              else if ((UnityEngine.Object)medsCI.emote1.sprite == (UnityEngine.Object)null)
              {
                  medsCI.emote1.sprite = stickerBase;
                  medsCI.emote1.gameObject.SetActive(true);
                  medsCI.emote1.sortingOrder = 20100 + index * 10 + 2;
              }
              else if ((UnityEngine.Object)medsCI.emote2.sprite == (UnityEngine.Object)null)
              {
                  medsCI.emote2.sprite = stickerBase;
                  medsCI.emote2.gameObject.SetActive(true);
                  medsCI.emote2.sortingOrder = 20100 + index * 10 + 3;
              }
              return false;
          }
      }*/
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MatchManager), "SendEmoteCard")]
    public static bool SendEmoteCardPrefix(ref MatchManager __instance, int tablePosition)
    {
      Log.LogDebug("SendEmoteCard commenced!\nheroActive: " + __instance.GetHeroActive() + "\ntablePosition: " + tablePosition + "\nemoteHeroActive: " + __instance.emoteManager.heroActive);
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MatchManager), "DoEmoteCard")]
    public static bool DoEmoteCardPrefix(ref MatchManager __instance, byte _tablePosition, byte _heroIndex)
    {
      if (medsEmotional.Value)
      {
        Log.LogDebug("DoEmoteCard commenced!\n_tablePosition: " + _tablePosition + "\n_heroIndex: " + _heroIndex);
        int index1 = (int)_tablePosition;
        if (index1 >= 100)
        {
          index1 -= 100;
          Dictionary<string, GameObject> medsCardGos = Traverse.Create(__instance).Field("cardGos").GetValue<Dictionary<string, GameObject>>();
          if (medsCardGos == null || medsCardGos.Keys.Count <= index1 || !(medsCardGos.ContainsKey("TMP_" + index1)) || medsCardGos["TMP_" + index1].GetComponent<CardItem>() == null)
            return false;
          if (medsCardGos["TMP_" + index1].GetComponent<CardItem>().HaveEmoteIcon(_heroIndex))
          {
            Log.LogDebug("removing cardgo");
            // remove emote
            medsCardGos["TMP_" + index1].GetComponent<CardItem>().RemoveEmoteIcon(_heroIndex);
          }
          else
          {
            medsCardGos["TMP_" + index1].GetComponent<CardItem>().ShowEmoteIcon(_heroIndex);
            if (!__instance.IsYourTurn())
              return false;
            GameManager.Instance.PlayLibraryAudio("Pop6", 2.9f);
          }
        }
        else
        {
          List<CardItem> medsCardItemTable = Traverse.Create(__instance).Field("cardItemTable").GetValue<List<CardItem>>();
          if (medsCardItemTable == null)
            return true;
          Log.LogDebug("passed carditemtable");
          // player clicks on a card that has sticker
          Log.LogDebug("count: " + medsCardItemTable.Count);
          if (medsCardItemTable.Count <= index1 || !((UnityEngine.Object)medsCardItemTable[index1] != (UnityEngine.Object)null))
            return false;
          if (medsCardItemTable[index1].HaveEmoteIcon(_heroIndex))
          {
            Log.LogDebug("removing");
            // remove emote
            medsCardItemTable[index1].RemoveEmoteIcon(_heroIndex);
          }
          else // clicks on a card that does not already have sticker
          {
            /* let's put more stickers in :D
            for (int index2 = 0; index2 < medsCardItemTable.Count; ++index2)
            {
                if (index2 != (int)_tablePosition && (UnityEngine.Object)medsCardItemTable[index2] != (UnityEngine.Object)null)
                    medsCardItemTable[index2].RemoveEmoteIcon(_heroIndex);
            }*/

            Log.LogDebug("unfalse, showing " + _heroIndex);

            medsCardItemTable[index1].ShowEmoteIcon(_heroIndex);
            Log.LogDebug("ifnotmyturn");
            if (!__instance.IsYourTurn())
              return false;
            GameManager.Instance.PlayLibraryAudio("Pop6", 2.9f);
          }
        }
        return false;
      }
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardItem), "ShowEmoteIcon")]
    public static bool ShowEmoteIconPrefix(ref CardItem __instance, byte _heroIndex)
    {
      if (!medsEmotional.Value)
        return true;

      Log.LogDebug("ShowEmoteIcon commenced!\n_heroIndex: " + _heroIndex);
      __instance.ShowEmoteTransform();
      SubClassData medsSCD = Globals.Instance.GetSubClassData(vanillaSubclasses[_heroIndex]);
      Log.LogDebug("gotsubclass");
      if ((UnityEngine.Object)medsSCD == (UnityEngine.Object)null)
        return false;
      Log.LogDebug("subclass id: " + medsSCD.Id);
      Sprite stickerBase = medsSCD.StickerBase;
      Log.LogDebug("stickerBase name: " + stickerBase.name);
      if ((UnityEngine.Object)__instance.emote0.sprite == (UnityEngine.Object)stickerBase || (UnityEngine.Object)__instance.emote1.sprite == (UnityEngine.Object)stickerBase || (UnityEngine.Object)__instance.emote2.sprite == (UnityEngine.Object)stickerBase)
        return false;
      Log.LogDebug("1875");
      if (__instance.cardfordiscard || __instance.cardforaddcard)
      {
        int medsTMP_ = int.Parse(__instance.name.Replace("TMP_", ""));
        if (medsTMP_ >= 0)
        {
          __instance.emote0.sortingOrder = 20100 + medsTMP_ * 10 + 1;
          __instance.emote1.sortingOrder = 20100 + medsTMP_ * 10 + 2;
          __instance.emote2.sortingOrder = 20100 + medsTMP_ * 10 + 3;
        }
      }
      if ((UnityEngine.Object)__instance.emote0.sprite == (UnityEngine.Object)null)
      {
        Log.LogDebug("emote0");
        __instance.emote0.sprite = stickerBase;

        Log.LogDebug("emote0 sprite name: " + __instance.emote0.sprite.name);
        __instance.emote0.gameObject.SetActive(true);
      }
      else if ((UnityEngine.Object)__instance.emote1.sprite == (UnityEngine.Object)null)
      {
        Log.LogDebug("emote1");
        __instance.emote1.sprite = stickerBase;
        __instance.emote1.gameObject.SetActive(true);
      }
      else
      {
        if (!((UnityEngine.Object)__instance.emote2.sprite == (UnityEngine.Object)null))
          return false;
        Log.LogDebug("emote2");
        __instance.emote2.sprite = stickerBase;
        __instance.emote2.gameObject.SetActive(true);
      }
      Log.LogDebug("finished");
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardItem), "RemoveEmoteIcon")]
    public static bool RemoveEmoteIconPrefix(ref CardItem __instance, byte _heroIndex)
    {
      if (!medsEmotional.Value)
        return true;
      Log.LogDebug("RemoveEmoteIcon commenced!\n_heroIndex: " + _heroIndex);
      SubClassData medsSCD = Globals.Instance.GetSubClassData(vanillaSubclasses[(int)_heroIndex]);
      if (medsSCD == null)
        return false;
      Sprite stickerBase = medsSCD.StickerBase;
      if ((UnityEngine.Object)__instance.emote0.sprite == (UnityEngine.Object)stickerBase)
      {
        __instance.emote0.sprite = (Sprite)null;
        __instance.emote0.gameObject.SetActive(false);
      }
      else if ((UnityEngine.Object)__instance.emote1.sprite == (UnityEngine.Object)stickerBase)
      {
        __instance.emote1.sprite = (Sprite)null;
        __instance.emote1.gameObject.SetActive(false);
      }
      else if ((UnityEngine.Object)__instance.emote2.sprite == (UnityEngine.Object)stickerBase)
      {
        __instance.emote2.sprite = (Sprite)null;
        __instance.emote2.gameObject.SetActive(false);
      }
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MatchManager), "SetCharactersPing")]
    public static bool SetCharactersPingPrefix(ref MatchManager __instance, int _action)
    {
      if (!medsEmotional.Value)
        return true;
      if (__instance.waitingDeathScreen || __instance.WaitingForActionScreen() || __instance.emoteManager.IsBlocked() || !__instance.emoteManager.gameObject.activeSelf)
        return false;
      __instance.emoteManager.HideEmotes();
      if (__instance.emoteManager.EmoteNeedsTarget(_action))
      {
        __instance.ShowCharactersPing(_action);
      }
      else
      {
        if (__instance.emoteManager.heroActive <= -1 || __instance.emoteManager.heroActive >= vanillaSubclasses.Length || vanillaSubclasses[__instance.emoteManager.heroActive] == "")
          return false;
        __instance.EmoteTarget(vanillaSubclasses[__instance.emoteManager.heroActive], _action);
      }
      return false;
    }


    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardItem), "HaveEmoteIcon")]
    public static bool HaveEmoteIconPrefix(ref CardItem __instance, byte _heroIndex, ref bool __result)
    {
      if (!medsEmotional.Value)
        return true;
      __result = false;
      SubClassData medsSCD = Globals.Instance.GetSubClassData(vanillaSubclasses[(int)_heroIndex]);
      if (medsSCD == null)
        return false;
      Sprite stickerBase = medsSCD.StickerBase;
      if ((UnityEngine.Object)__instance.emote0.sprite == (UnityEngine.Object)stickerBase || (UnityEngine.Object)__instance.emote1.sprite == (UnityEngine.Object)stickerBase || (UnityEngine.Object)__instance.emote2.sprite == (UnityEngine.Object)stickerBase)
        __result = true;
      return false;
    }



    [HarmonyPostfix]
    [HarmonyPatch(typeof(EmoteManager), "Awake")]
    public static void AwakePostfix(ref EmoteManager __instance)
    {
      if (medsEmotional.Value)
      {
        medsPosIni = __instance.characterPortrait.transform.localPosition;
        medsPosIniBlocked = __instance.characterPortraitBlocked.transform.parent.transform.localPosition;
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EmoteManager), "Init")]
    public static void InitPrefix(ref EmoteManager __instance)
    {
      if (medsEmotional.Value && GameManager.Instance.IsMultiplayer())
      {
        List<string> myHeroIDs = new();
        foreach (Hero medsHero in MatchManager.Instance.GetTeamHero())
        {
          if (medsHero == null)
            continue;
          Log.LogDebug("subclassname: " + medsHero.SubclassName);
          if ((medsHero.Owner == NetworkManager.Instance.GetPlayerNick() || medsHero.Owner == "") && !myHeroIDs.Contains(medsHero.SubclassName))
            myHeroIDs.Add(medsHero.SubclassName);
        }
        for (var a = 0; a < vanillaSubclasses.Length; a++)
        {
          if (myHeroIDs.Contains(vanillaSubclasses[a]) && __instance.heroActive == -1)
            __instance.heroActive = a - 1;
        }
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EmoteManager), "SelectNextCharacter")]
    public static bool SelectNextCharacterPrefix(ref EmoteManager __instance)
    {
      if (medsEmotional.Value && GameManager.Instance.IsMultiplayer())
      {
        ++__instance.heroActive;
        if (__instance.heroActive >= vanillaSubclasses.Length)
          __instance.heroActive = 0;
        SubClassData medsSCD = Globals.Instance.GetSubClassData(vanillaSubclasses[__instance.heroActive]);
        if (medsSCD != null)
        {
          __instance.characterPortrait.sprite = __instance.characterPortraitBlocked.sprite = medsSCD.StickerBase;
          __instance.characterPortrait.transform.localPosition = medsPosIni + new Vector3(medsSCD.StickerOffsetX, 0f, 0f);
          __instance.characterPortraitBlocked.transform.parent.transform.localPosition = medsPosIniBlocked + new Vector3(medsSCD.StickerOffsetX, 0f, 0f);
        }
        for (int i = 0; i < __instance.emotes.Length; i++)
          __instance.emotes[i].SetBlocked(_state: false);
        return false;
      }
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EmoteTarget), "SetIcons")]
    public static bool SetIconsPrefix(ref EmoteTarget __instance, int _heroIndex, int _action)
    {

      if (medsEmotional.Value && GameManager.Instance.IsMultiplayer())
      {
        if (!(MatchManager.Instance != null))
          return false;

        SubClassData medsSCD = Globals.Instance.GetSubClassData(vanillaSubclasses[_heroIndex]);
        //Hero hero = MatchManager.Instance.GetHero(_heroIndex);
        if (medsSCD != null)
        {
          if (MatchManager.Instance.emoteManager.EmoteNeedsTarget(_action))
          {
            __instance.characterT.gameObject.SetActive(value: true);
            __instance.icon.sprite = MatchManager.Instance.emoteManager.emotesSprite[_action];
            __instance.portraitStickerBase.sprite = medsSCD.GetEmoteBase();
          }
          else
          {
            __instance.characterT.gameObject.SetActive(value: false);
            __instance.icon.sprite = medsSCD.GetEmote(_action);
            __instance.iconStickerBase.sprite = medsSCD.GetEmoteBase();
            Log.LogDebug("StickerOffsetX: " + medsSCD.StickerOffsetX);
            __instance.transform.localPosition += new Vector3(medsSCD.StickerOffsetX, 0f, 0f);
          }
          __instance.gameObject.SetActive(value: true);
          if (_action != 2 && _action != 3)
          {
            Animator medsAnimator = Traverse.Create(__instance).Field("animator").GetValue<Animator>();
            medsAnimator.SetTrigger("sticker");
            Traverse.Create(__instance).Field("animator").SetValue(medsAnimator);
          }
        }
        return false;
      }
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EmoteTarget), "SetActiveHeroOnCardEmoteButton")]
    public static bool SetActiveHeroOnCardEmoteButtonPrefix(ref EmoteTarget __instance)
    {

      if (medsEmotional.Value && GameManager.Instance.IsMultiplayer())
      {
        if ((UnityEngine.Object)MatchManager.Instance == null || MatchManager.Instance.emoteManager == null || MatchManager.Instance.emoteManager.heroActive > vanillaSubclasses.Length)
          return false;
        SubClassData medsSCD = Globals.Instance.GetSubClassData(vanillaSubclasses[MatchManager.Instance.emoteManager.heroActive]);
        if (medsSCD != (UnityEngine.Object)null && medsSCD.StickerBase != null)
          __instance.icon.sprite = medsSCD.StickerBase;
        return false;
      }
      return true;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MatchManager), "EmoteTarget")]
    public static bool EmoteTargetPrefix(ref MatchManager __instance, string _id, int _action, int _heroIndex = -1, bool _fromNet = false)
    {
      Log.LogDebug("EmoteTarget has commenced!\n_id: " + _id + "\n_action: " + _action + "\n_heroIndex: " + _heroIndex + "\n_fromNet: " + _fromNet);
      if (medsEmotional.Value && GameManager.Instance.IsMultiplayer())
      {
        int medsHeroIndex = _heroIndex;
        if (!_fromNet)
          medsHeroIndex = __instance.emoteManager.heroActive;
        if (!_fromNet && GameManager.Instance.IsMultiplayer())
        {
          PhotonView medsPhotonView = Traverse.Create(__instance).Field("photonView").GetValue<PhotonView>();
          medsPhotonView.RPC("NET_EmoteTarget", RpcTarget.Others, (object)_id, (object)(byte)_action, (object)medsHeroIndex);
        }
        Transform transform = (Transform)null;
        CharacterItem characterItem = (CharacterItem)null;
        Hero[] medsHero = Traverse.Create(__instance).Field("TeamHero").GetValue<Hero[]>();
        NPC[] medsNPC = Traverse.Create(__instance).Field("TeamNPC").GetValue<NPC[]>();
        List<int> medsMyIndex = new();
        for (int index = 0; index < medsHero.Length; ++index)
        {
          if (medsHero[index] != null && medsHero[index].Alive && (UnityEngine.Object)medsHero[index].HeroItem != (UnityEngine.Object)null && medsHero[index].Id == _id)
          {
            transform = medsHero[index].HeroItem.transform;
            characterItem = (CharacterItem)medsHero[index].HeroItem;
            break;
          }
        }
        if ((UnityEngine.Object)transform == (UnityEngine.Object)null)
        {
          for (int index = 0; index < medsNPC.Length; ++index)
          {
            if (medsNPC[index] != null && medsNPC[index].Alive && (UnityEngine.Object)medsNPC[index].NPCItem != (UnityEngine.Object)null && medsNPC[index].Id == _id)
            {
              transform = medsNPC[index].NPCItem.transform;
              characterItem = (CharacterItem)medsNPC[index].NPCItem;
              break;
            }
          }
        }
        if ((UnityEngine.Object)transform == (UnityEngine.Object)null)
        {
          for (int index = 0; index < medsHero.Length; ++index)
          {
            if (medsHero[index] != null && medsHero[index].Owner == NetworkManager.Instance.GetPlayerNick())
              medsMyIndex.Add(index);
          }
          if (medsMyIndex.Count > 0)
          {
            Log.LogDebug("medsMyIndex: " + medsMyIndex.Join());
            int index = UnityEngine.Random.Range(0, medsMyIndex.Count);
            Log.LogDebug("index: " + index);
            if (medsHero[index] != null && medsHero[index].HeroItem != null)
            {
              transform = medsHero[index].HeroItem.transform;
              characterItem = (CharacterItem)medsHero[index].HeroItem;
            }
          }
        }
        if ((UnityEngine.Object)transform != (UnityEngine.Object)null && (UnityEngine.Object)characterItem != (UnityEngine.Object)null)
        {
          GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(__instance.emoteTargetPrefab, Vector3.zero, Quaternion.identity);
          gameObject.transform.position = characterItem.emoteCharacterPing.transform.position;
          /**/
          gameObject.GetComponent<global::EmoteTarget>().SetIcons(medsHeroIndex, _action);
          GameManager.Instance.PlayLibraryAudio("Pop3", 2.9f);
        }
        if (!_fromNet)
          __instance.ResetCharactersPing();
        return false;
      }
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EmoteManager), "SetBlocked")]
    public static void SetBlockedPrefix(ref bool _state)
    {
      if (medsEmotional.Value)
        _state = false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamManager), "SetScore")]
    public static bool SetScorePrefix(int score, bool singleplayer = true)
    {
      if (score <= 0)
        return false;
      SetScoreLeaderboard(score, singleplayer, "RankingAct4");
      return false;
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamManager), "SetObeliskScore")]
    public static bool SetObeliskScorePrefix(int score, bool singleplayer = true)
    {
      if (score <= 0)
        return false;
      SetScoreLeaderboard(score, singleplayer, "Challenge");
      return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Character), "SetAura")]
    public static void SetAuraPrefix(ref Character __instance, ref AuraCurseData _acData, ref int charges)
    {
      if (_acData.Id.ToLower() == "block")
      {
        string medsSubClassName = Traverse.Create(__instance).Field("subclassName").GetValue<string>();
        if (AtOManager.Instance.CharacterHaveTrait(medsSubClassName, "queenofthorns"))
        {
          _acData = Globals.Instance.GetAuraCurseData("thorns");
          charges = Functions.FuncRoundToInt((float)charges * 0.3f);
        }
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(InputController), "DoKeyBinding")]
    public static bool DoKeyBindingPrefix(ref InputAction.CallbackContext _context)
    {
      if (Keyboard.current != null && _context.control == Keyboard.current[Key.F1])
      {
        //ObeliskialUI.ShowUI = !ObeliskialUI.ShowUI;
        return false;
      }
      else if (medsSpacebarContinue.Value && bFinalResolution && _context.control == Keyboard.current[Key.Space])
      {
        EventManager.Instance.Ready(true);
        return false;
      }
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(EventManager), "SetEvent")]
    public static void SetEventPrefix(ref EventData _eventData)
    {
      // clones copy character events?
      if (medsDLCClones.Value)
      {
        Dictionary<string, EventData> medsEventDataSource = Traverse.Create(Globals.Instance).Field("_Events").GetValue<Dictionary<string, EventData>>();
        if (_eventData != (EventData)null && medsEventDataSource.ContainsKey(_eventData.EventId))
        {
          bool erFound = false;
          EventReplyData[] tempERD = medsEventDataSource[_eventData.EventId].Replys;
          for (int a = 0; a < medsEventDataSource[_eventData.EventId].Replys.Length; a++)
          {
            EventReplyData reply = medsEventDataSource[_eventData.EventId].Replys[a];
            if (reply.RequiredClass != (SubClassData)null && !reply.RepeatForAllCharacters)
            {
              List<string> subclassAdd = new();
              if (reply.RequiredClass.Id == (IsHost() ? medsDLCCloneTwo.Value : medsMPDLCCloneTwo))
                subclassAdd.Add("medsdlctwo");
              if (reply.RequiredClass.Id == (IsHost() ? medsDLCCloneThree.Value : medsMPDLCCloneThree))
                subclassAdd.Add("medsdlcthree");
              if (reply.RequiredClass.Id == (IsHost() ? medsDLCCloneFour.Value : medsMPDLCCloneFour))
                subclassAdd.Add("medsdlcfour");
              foreach (string sub in subclassAdd)
              {
                EventReplyData eventReplyData = reply.ShallowCopy();
                eventReplyData.RequiredClass = Globals.Instance.GetSubClassData(sub);
                Array.Resize(ref tempERD, tempERD.Length + 1);
                tempERD[tempERD.Length - 1] = eventReplyData;
                erFound = true;
              }
            }
          }
          if (erFound)
          {
            medsEventDataSource[_eventData.EventId].Replys = tempERD;
            Traverse.Create(Globals.Instance).Field("_Events").SetValue(medsEventDataSource);
          }
        }
      }
    }

    public static void UpdateTownTier(bool from = true)
    {
      if (IsHost() ? medsVisitAllZones.Value : medsMPVisitAllZones)
      {
        if (AtOManager.Instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("_tier1")))
          AtOManager.Instance.RemovePlayerRequirement(Globals.Instance.GetRequirementData("_tier1"));
        if (AtOManager.Instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("_tier2")))
          AtOManager.Instance.RemovePlayerRequirement(Globals.Instance.GetRequirementData("_tier2"));
        if (AtOManager.Instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("_tier3")))
          AtOManager.Instance.RemovePlayerRequirement(Globals.Instance.GetRequirementData("_tier3"));
        if (AtOManager.Instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("medsvisitedvoidlow")))
        {
          Log.LogDebug("APPARENTLY WE HAVE VISITED VOIDLOW ?? ?? ??");
          AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("_tier3"));
          AtOManager.Instance.SetTownTier(from ? 3 : 2);
        }
        else
        {
          int a = 0;
          if (AtOManager.Instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("medsvisitedulminin")))
            a++;
          if (AtOManager.Instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("medsvisitedvelkarath")))
            a++;
          if (AtOManager.Instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("medsvisitedaquarfall")))
            a++;
          if (AtOManager.Instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("medsvisitedfaeborg")))
            a++;
          Log.LogDebug("SetTownTier a: " + a.ToString());
          if (a == 1)
          {
            AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("_tier1"));
            AtOManager.Instance.SetTownTier(from ? 1 : 0);
          }
          else if (a > 1)
          {
            AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("_tier2"));
            AtOManager.Instance.SetTownTier(from ? 2 : 1);
          }
        }
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MapManager), "TravelToThisNode")]
    public static void TravelToThisNodePrefix(Node _node)
    {
      LogDebug("currentMapNode: " + AtOManager.Instance.currentMapNode);
      NodeData _curNode = Globals.Instance.GetNodeData(AtOManager.Instance.currentMapNode);
      if (_curNode != null && medsZoneStartNodes.Contains(_curNode.NodeId))
      {
        LogDebug("travelling from: " + _curNode.NodeId);
        UpdateTownTier();
      }
      else if (_node != null && _node.nodeData != null && medsZoneStartNodes.Contains(_node.nodeData.NodeId))
      {
        LogDebug("travelling to: " + _node.nodeData.NodeId);
        if (_node.nodeData.NodeId == "voidlow_0" && !AtOManager.Instance.PlayerHasRequirement(Globals.Instance.GetRequirementData("medsvisitedvoidlow")))
          AtOManager.Instance.AddPlayerRequirement(Globals.Instance.GetRequirementData("medsvisitedvoidlow"));
        UpdateTownTier(false);
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(AtOManager), "SetTownTier")]
    public static void SetTownTierPrefix(ref AtOManager __instance, ref int _townTier)
    {
      LogDebug("SetTownTier Prefix commenced! _townTier: " + _townTier.ToString());
      //UpdateTownTier();
    }
    [HarmonyPrefix]
    [HarmonyPatch(typeof(AtOManager), "UpgradeTownTier")]
    public static void UpgradeTownTierPrefix(ref AtOManager __instance)
    {
      LogDebug("UpgradeTownTier Prefix commenced!");
      UpdateTownTier();
    }

    /*
    this _is_ doable, just beyond my current ability?
    basically you need to replace GenerateNewCard so it doesn't do this check:
      if (num < numCards)
        numCards = num;
    but that involves calling the GenerateNewCardCo IEnumerator, which... I have no fucking clue how to do with reflections?

    UPDATE: I FUCKIN' DID IT MOM
    LOOK AT THE TRANSPILER BELOW THX
    */

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Character), "GetDrawCardsTurn")]
    public static bool GetDrawCardsTurnPrefix(ref Character __instance, ref int __result)
    {
      if (IsHost() ? medsOverlyCardergetic.Value : medsMPOverlyCardergetic)
      {
        int drawCardsTurn = 5 + Traverse.Create(__instance).Field("drawModifier").GetValue<int>();
        if (drawCardsTurn < 0)
          drawCardsTurn = 0;
        __result = drawCardsTurn;
        return false;
      }
      return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Character), "GetDrawCardsTurnForDisplayInDeck")]
    public static bool GetDrawCardsTurnForDisplayInDeckPrefix(ref Character __instance, ref int __result)
    {
      if (IsHost() ? medsOverlyCardergetic.Value : medsMPOverlyCardergetic)
      {
        int drawCardsTurn = 5 + __instance.GetAuraDrawModifiers(false);
        if (drawCardsTurn < 0)
          drawCardsTurn = 0;
        __result = drawCardsTurn;
        return false;
      }
      return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Globals), "CreateGameContent")]
    public static void CreateGameContentPostfix()
    {
      LogDebug("CreateGameContentPostfix");
      Dictionary<string, EventRequirementData> medsEventRequirementDataSource = Traverse.Create(Globals.Instance).Field("_Requirements").GetValue<Dictionary<string, EventRequirementData>>();
      EventRequirementData medsReq = ScriptableObject.CreateInstance<EventRequirementData>();
      medsReq.RequirementId = "medsvisitedaquarfall";
      medsReq.RequirementName = "Visited Aquarfall";
      medsReq.Description = "Visited Aquarfall";
      medsReq.name = "medsvisitedaquarfall";
      medsEventRequirementDataSource["medsvisitedaquarfall"] = medsReq;
      medsReq = ScriptableObject.CreateInstance<EventRequirementData>();
      medsReq.RequirementId = "medsvisitedfaeborg";
      medsReq.RequirementName = "Visited Faeborg";
      medsReq.Description = "Visited Faeborg";
      medsReq.name = "medsvisitedfaeborg";
      medsEventRequirementDataSource["medsvisitedfaeborg"] = medsReq;
      medsReq = ScriptableObject.CreateInstance<EventRequirementData>();
      medsReq.RequirementId = "medsvisitedvelkarath";
      medsReq.RequirementName = "Visited Velkarath";
      medsReq.Description = "Visited Velkarath";
      medsReq.name = "medsvisitedvelkarath";
      medsEventRequirementDataSource["medsvisitedvelkarath"] = medsReq;
      medsReq = ScriptableObject.CreateInstance<EventRequirementData>();
      medsReq.RequirementId = "medsvisitedulminin";
      medsReq.RequirementName = "Visited Ulminin";
      medsReq.Description = "Visited Ulminin";
      medsReq.name = "medsvisitedulminin";
      medsEventRequirementDataSource["medsvisitedulminin"] = medsReq;
      medsReq = ScriptableObject.CreateInstance<EventRequirementData>();
      medsReq.RequirementId = "medsvisitedvoidlow";
      medsReq.RequirementName = "Visited Voidlow";
      medsReq.Description = "Visited Voidlow";
      medsReq.name = "medsvisitedvoidlow";
      medsEventRequirementDataSource["medsvisitedvoidlow"] = medsReq;
      medsReq = ScriptableObject.CreateInstance<EventRequirementData>();
      medsReq.RequirementId = "medsimpossiblerequirement";
      medsReq.RequirementName = "Always False Requirement";
      medsReq.Description = "Always False Requirement";
      medsReq.name = "medsimpossiblerequirement";
      medsEventRequirementDataSource["medsimpossiblerequirement"] = medsReq;

      // save vanilla+custom
      Traverse.Create(Globals.Instance).Field("_Requirements").SetValue(medsEventRequirementDataSource);
      SubClassReplace();
      UpdateVisitAllZones();
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(PlayerManager), "IsCardUnlocked")]
    public static void IsCardUnlockedPostfix(string _cardId, ref bool __result)
    {
      if (medsAllThePets.Value && _cardId == "tombstone" && !__result)
      {
        PlayerManager.Instance.CardUnlock(_cardId);
        __result = true;
      }
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(HeroSelectionManager), "Start")]
    public static void HSMStartPrefix(ref HeroSelectionManager __instance)
    {
      if ((UnityEngine.Object)PlayerManager.Instance != (UnityEngine.Object)null)
      {
        // reset skin if it doesn’t exist for this character
        if (!PlayerManager.Instance.SkinUsed.Keys.Contains("medsdlctwo") || String.Compare(PlayerManager.Instance.SkinUsed["medsdlctwo"], medsDLCCloneTwoSkin) > 0)
          PlayerManager.Instance.SkinUsed["medsdlctwo"] = "medsdlctwoa";
        if (!PlayerManager.Instance.SkinUsed.Keys.Contains("medsdlcthree") || String.Compare(PlayerManager.Instance.SkinUsed["medsdlcthree"], medsDLCCloneThreeSkin) > 0)
          PlayerManager.Instance.SkinUsed["medsdlcthree"] = "medsdlcthreea";
        if (!PlayerManager.Instance.SkinUsed.Keys.Contains("medsdlcfour") || String.Compare(PlayerManager.Instance.SkinUsed["medsdlcfour"], medsDLCCloneFourSkin) > 0)
          PlayerManager.Instance.SkinUsed["medsdlcfour"] = "medsdlcfoura";
        // reset cardback if it doesn’t exist for this character
        if (!PlayerManager.Instance.CardbackUsed.Keys.Contains("medsdlctwo") || String.Compare(PlayerManager.Instance.CardbackUsed["medsdlctwo"], medsDLCCloneTwoCardback) > 0)
          PlayerManager.Instance.CardbackUsed["medsdlctwo"] = "medsdlctwoa";
        if (!PlayerManager.Instance.CardbackUsed.Keys.Contains("medsdlcthree") || String.Compare(PlayerManager.Instance.CardbackUsed["medsdlcthree"], medsDLCCloneThreeCardback) > 0)
          PlayerManager.Instance.CardbackUsed["medsdlcthree"] = "medsdlcthreea";
        if (!PlayerManager.Instance.CardbackUsed.Keys.Contains("medsdlcfour") || String.Compare(PlayerManager.Instance.CardbackUsed["medsdlcfour"], medsDLCCloneFourCardback) > 0)
          PlayerManager.Instance.CardbackUsed["medsdlcfour"] = "medsdlcfoura";
      }
      iShopsWithNoPurchase = 0;
      return;
    }

    /* shouldn't be necessary with scrollbar
    [HarmonyPostfix]
    [HarmonyPatch(typeof(SubClassData), "IsMultiClass")]
    public static void IsMultiClassPostfix(ref bool __result, ref SubClassData __instance)
    {
      LogInfo("YEEEP");
      if (__instance.Id == "medsdlctwo" || __instance.Id == "medsdlcthree" || __instance.Id == "medsdlcfour")
      {
        __result = true;
        LogInfo("SET TO TRUE?? " + __instance.OrderInList);
      }
    }
    */

    [HarmonyPostfix]
    [HarmonyPatch(typeof(GameManager), "GetDeveloperMode")]
    public static void GetDeveloperModePostfix(ref bool __result)
    {
      if (IsHost() ? medsDeveloperMode.Value : medsMPDeveloperMode)
        __result = true;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(AtOManager), "GetItemList")]
    public static void GetItemListPostfix(List<string> __result, string itemListId)
    {
      LogDebug("itemListId: " + itemListId);
      LogDebug("GetItemList Result: " + String.Join(", ", __result));
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(SteamManager), "PlayerHaveDLC")]
    public static void PlayerHaveDLCPrefix(ref bool __result, string _sku, ref SteamManager __instance)
    {
      // AAAAAAAH I HATE REFLECTIONSSSSSSSSSSSSS
      __result = SteamApps.IsSubscribedToApp((AppId)uint.Parse(_sku)) && (bool)__instance.GetType().GetMethod("LauncherEnabledDLC", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).Invoke(__instance, new object[] { uint.Parse(_sku) });
      LogInfo("PlayerHaveDLC " + _sku + ": " + __result.ToString());
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SteamManager), "LauncherEnabledDLC")]
    public static bool LauncherEnabledDLCPrefix(ref bool __result)
    {
      if (GameManager.Instance != null && GameManager.Instance.DisabledDLCs != null)
      {
        LogInfo("LauncherEnabledDLC: DisabledDLCs should be fine.");
        return true; // run original method
      }
      else
      { // GameManager hasn't started yet, so just assume it's enabled
        LogInfo("LauncherEnabledDLC: DisabledDLCs not present!");
        __result = true;
        return false; // do not run original method
      }
    }

    /* #TODO: activationawareness
    [HarmonyPrefix]
    [HarmonyPatch(typeof(CardItem), "DrawEnergyBorder")]
    public static bool DrawBorderPrefix(ref CardItem __instance, ref string color)
    {
        if (color != "green" || !medsActivationAwareness.Value)
            return true;
        CardData crd = Traverse.Create(__instance).Field("cardData").GetValue<CardData>();
        Hero hero = Traverse.Create(__instance).Field("theHero").GetValue<Hero>();
        if (crd != null && hero != null)
        {
            LogDebug("DrawEnergyBorder: " + crd.Id);
            int itemSlots = Traverse.Create(hero).Field("itemSlots").GetValue<int>();
            for (int index = 0; index < itemSlots; ++index)
            {
                string id = "";
                if (index == 0 && hero.Weapon != "")
                    id = hero.Weapon;
                else if (index == 1 && hero.Armor != "")
                    id = hero.Armor;
                else if (index == 2 && hero.Jewelry != "")
                    id = hero.Jewelry;
                else if (index == 3 && hero.Accesory != "")
                    id = hero.Accesory;
                else if (index == 4 && hero.Corruption != "")
                    id = hero.Corruption;
                else if (index == 5 && hero.Pet != "")
                    id = hero.Pet;
                else if (index == 6 && hero.Enchantment != "")
                    id = hero.Enchantment;
                else if (index == 7 && hero.Enchantment2 != "")
                    id = hero.Enchantment2;
                else if (index == 8 && hero.Enchantment3 != "")
                    id = hero.Enchantment3;
                if (id != "")
                {
                    CardData cardData = Globals.Instance.GetCardData(id, false);
                    if ((UnityEngine.Object)cardData != (UnityEngine.Object)null)
                    {
                        ItemData itemData = (ItemData)null;
                        if ((UnityEngine.Object)cardData.Item != (UnityEngine.Object)null)
                            itemData = cardData.Item;
                        else if ((UnityEngine.Object)cardData.ItemEnchantment != (UnityEngine.Object)null)
                            itemData = cardData.ItemEnchantment;
                        if ((UnityEngine.Object)itemData != (UnityEngine.Object)null)
                        {
                            bool canDo = false;
                            if (itemData.Activation == Enums.EventActivation.CastCard ||
                                itemData.Activation == Enums.EventActivation.PreFinishCast ||
                                itemData.Activation == Enums.EventActivation.FinishCast ||
                                itemData.Activation == Enums.EventActivation.FinishFinishCast)
                            {
                                if ((itemData.CastedCardType != Enums.CardType.None && cardData.GetCardTypes().Contains(itemData.CastedCardType)) || itemData.CastedCardType == Enums.CardType.None)
                                    canDo = true;
                            }
                            if (itemData.Id == "manaloop" || itemData.Id == "manalooprare")
                                canDo = false; // don't display mana loop, because it activates on everything!
                            if (itemData.TimesPerCombat > 0 && !MatchManager.Instance.CanExecuteItemInThisCombat(hero.Id, itemData.Id, itemData.TimesPerCombat))
                                canDo = false; // cannot be activated again this combat
                            if (itemData.TimesPerTurn > 0 && !MatchManager.Instance.CanExecuteItemInThisTurn(hero.Id, itemData.Id, itemData.TimesPerTurn))
                                canDo = false; // cannot be activated again this turn
                            if (canDo)
                            {
                                SpriteRenderer cardBorderSR = Traverse.Create(__instance).Field("cardBorderSR").GetValue<SpriteRenderer>();
                                cardBorderSR.color = new UnityEngine.Color(1f, 1f, 0f, 0.05f);
                                return false; // do not run original method
                            }
                        }
                    }
                }
            }
        }
        return true;
    }*/


    /*[HarmonyPrefix]
    [HarmonyPatch(typeof(MatchManager), "DealNewCard")]
    public static System.Collections.IEnumerator DealNewCardPrefix(Enums.CardFrom fromPlace, string comingFromCardId)
    {
        Plugin.Log.LogDebug("DEALNEWCARDPREFIX");

        yield return (object)null;
    }*/


    /* maybe unnecessary???
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MatchManager), "GenerateNewCard")]
    public static bool GenerateNewCardPrefix(ref MatchManager __instance,
      int numCards,
      string theCard,
      bool createCard,
      Enums.CardPlace where,
      CardData cardDataForModification = null,
      CardData copyDataFromThisCard = null,
      int heroIndex = -1,
      bool isHero = true,
      int indexForBatch = 0)
    {
        Plugin.Log.LogDebug("HELLO? AM I EVEN GENERATING NEW CARDS???");
        return false;
        if (__instance.MatchIsOver)
            return false;
        if (Plugin.IsHost() ? Plugin.medsOverlyCardergetic.Value : Plugin.medsMPOverlyCardergetic)
        {
            Plugin.Log.LogDebug("HeLLO, I am preparing for the IEnumerator...");
            if (where == Enums.CardPlace.Hand && numCards <= 0)
                return false;
            Traverse.Create(__instance).Field("gameBusy").SetValue(true);
            MethodInfo myE = __instance.GetType().GetMethod("GenerateNewCardCo", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            __instance.StartCoroutine((System.Collections.IEnumerator)myE.Invoke(__instance, new object[] { numCards, theCard, createCard, where, cardDataForModification, copyDataFromThisCard, heroIndex, isHero, indexForBatch }));
            Plugin.Log.LogDebug("at least the abs(%) of that working is positive :) :)");
            return false;
        }
        return true;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(MatchManager), "GenerateNewCardCo")]
    public static void GenerateNewCardCoPrefix(ref int numCards)
    {
        Plugin.Log.LogInfo("HELLO IT IS I, THE IENUMERATOR! " + numCards.ToString());
    }
    */












  }
  /*
  THIS TRANSPILER WORKS!
  WOO
  (but cards end up all the way on the right, so probably need to look at CardItem.PositionCardInTable + MatchManager.RepositionCards ??)


  [HarmonyPatch]
  public static class DealNewCard_Transpiler
  {
      static MethodBase TargetMethod()
      {
          var mainClass = typeof(MatchManager).GetNestedTypes(AccessTools.all).FirstOrDefault(t => t.FullName.Contains("d__512"));
          return mainClass.GetMethods(AccessTools.all).FirstOrDefault(m => m.Name.Contains("MoveNext"));
      }
      /*[HarmonyTranspiler]
      [HarmonyPatch(typeof(MatchManager), "DealNewCard")]
      static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
      {
          Plugin.Log.LogDebug("THE CALL IS COMING FROM WITHIN THE TRANSPILER");
          var codes = new List<CodeInstruction>(instructions);
          for (int i = 0; i < codes.Count - 1; i++)
          {
              string s = "OPCODE: ";
              if (codes[i].opcode == System.Reflection.Emit.OpCodes.Call &&
                  codes[i].operand != null &&
                  codes[i].operand.ToString().Contains("CountHeroHand", StringComparison.OrdinalIgnoreCase) &&
                  codes[i + 1].opcode == System.Reflection.Emit.OpCodes.Ldc_I4_S &&
                  codes[i + 1].operand != null &&
                  codes[i + 1].operand.ToString() == "10")
              {
                  codes[i + 1].operand = (sbyte)100;
                  //Plugin.Log.LogDebug("FOUNDOPER")
                  //codes[i + 1].operand = 
              }
              s += codes[i].opcode.ToString() + " ";
              if (codes[i].operand != null)
                  s += codes[i].operand.ToString() + " (" + codes[i].operand.GetType().ToString() + ")";
              Plugin.Log.LogDebug(s);
          }
          return codes.AsEnumerable();
      }
  }*/
}
