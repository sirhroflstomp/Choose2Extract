using System.Reflection;
using EFT;
using EFT.Interactive;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace Choose2Extract
{
    // The game calls Proceed when the player is in an exit zone and may extract
    internal class ProceedPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ExfiltrationPoint), "Proceed", new[] { typeof(Player), typeof(bool) });
        }

        [PatchPrefix]
        private static bool Prefix(ExfiltrationPoint __instance, Player player, bool __1)
        {
            return !ExtractController.TryIntercept(__instance, player, __1);
        }
    }

    internal class ExitPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ExfiltrationPoint), "OnPlayerExit", new[] { typeof(Player) });
        }

        [PatchPrefix]
        private static void Prefix(ExfiltrationPoint __instance, Player obj)
        {
            ExtractController.OnPlayerExit(__instance, obj);
        }
    }

    // The game rebuilds the interaction prompt here whenever the player's surroundings change
    internal class InteractionsPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GamePlayerOwner), "InteractionsChangedHandler");
        }

        // The prompt is drawn as soon as the game sets it, so the transit label has to be
        // changed before that happens
        [PatchPrefix]
        private static void Prefix()
        {
            TransitPrompt.Rename();
        }

        [PatchPostfix]
        private static void Postfix(GamePlayerOwner __instance)
        {
            ExtractController.OnInteractionsChanged(__instance);
        }
    }

    internal class TimerPanelPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LocationTransitTimerPanel), "Show", new[] { typeof(float), typeof(string) });
        }

        [PatchPostfix]
        private static void Postfix(LocationTransitTimerPanel __instance)
        {
            TimerBox.Apply(__instance, ExtractController.IsHolding);
        }
    }
}
