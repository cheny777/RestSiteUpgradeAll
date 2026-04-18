using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.RestSite;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.RestSite;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace RestSiteUpgradeAll;

[HarmonyPatch]
internal static class RestSitePatches
{
    private static readonly PropertyInfo OwnerProperty = typeof(RestSiteOption).GetProperty(
        "Owner",
        BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("RestSiteOption.Owner property not found.");

    private static readonly FieldInfo SelectionField = typeof(SmithRestSiteOption).GetField(
        "_selection",
        BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("SmithRestSiteOption._selection field not found.");

    private static readonly PropertyInfo IsFocusedProperty = typeof(NClickableControl).GetProperty(
        "IsFocused",
        BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("NClickableControl.IsFocused property not found.");

    private static readonly FieldInfo ExecutingOptionField = typeof(NRestSiteButton).GetField(
        "_executingOption",
        BindingFlags.Instance | BindingFlags.NonPublic)
        ?? throw new InvalidOperationException("NRestSiteButton._executingOption field not found.");

    [HarmonyPrefix]
    [HarmonyPatch(typeof(SmithRestSiteOption), nameof(SmithRestSiteOption.OnSelect))]
    private static bool ReplaceSmithSelection(SmithRestSiteOption __instance, ref Task<bool> __result)
    {
        __result = UpgradeEntireDeckAsync(__instance);
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(NRestSiteButton), nameof(NRestSiteButton.RefreshTextState))]
    private static bool OverrideSmithDescription(NRestSiteButton __instance)
    {
        if (__instance.Option is not SmithRestSiteOption)
        {
            return true;
        }

        var room = NRestSiteRoom.Instance;
        if (room is null)
        {
            return false;
        }

        var isFocused = (bool?)IsFocusedProperty.GetValue(__instance) ?? false;
        var isExecuting = (bool?)ExecutingOptionField.GetValue(__instance) ?? false;

        if (isFocused || isExecuting)
        {
            room.SetText("一键升级全部可升级卡牌");
        }
        else
        {
            room.FadeOutOptionDescription();
        }

        return false;
    }

    private static async Task<bool> UpgradeEntireDeckAsync(SmithRestSiteOption option)
    {
        var owner = GetOwner(option);
        var upgradableCards = owner.Deck.Cards.Where(card => card.IsUpgradable).ToArray();
        if (upgradableCards.Length == 0)
        {
            Log.Info($"Smith selected but no cards were upgradable. player={owner.NetId}");
            SelectionField.SetValue(option, Array.Empty<CardModel>());
            return false;
        }

        SelectionField.SetValue(option, upgradableCards);
        CardCmd.Upgrade(upgradableCards, CardPreviewStyle.None);
        Log.Info($"Upgraded all cards at rest site. player={owner.NetId} count={upgradableCards.Length}");
        await Hook.AfterRestSiteSmith(owner.RunState, owner);
        return true;
    }

    private static Player GetOwner(RestSiteOption option)
    {
        return (Player?)OwnerProperty.GetValue(option)
            ?? throw new InvalidOperationException("Rest site option owner was null.");
    }
}
