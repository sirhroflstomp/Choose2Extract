using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.Interactive;
using EFT.UI;
using HarmonyLib;

namespace Choose2Extract
{
    /// <summary>
    /// Holds back the extract the game would start on entering an exit zone and offers it
    /// as an interaction prompt instead, the way transits work.
    /// </summary>
    internal static class ExtractController
    {
        private static readonly MethodInfo ProceedMethod =
            AccessTools.Method(typeof(ExfiltrationPoint), "Proceed", new[] { typeof(Player), typeof(bool) });

        // Zone the player is standing in, waiting for a confirmation
        private static ExfiltrationPoint _pendingPoint;
        private static Player _pendingPlayer;
        private static bool _manuallyStarted;

        // Zone already confirmed; the game behaves normally there until the player leaves
        private static ExfiltrationPoint _confirmedPoint;

        private static bool _starting;
        private static bool _holding;

        private static AvailableInteractionState _prompt;
        private static AvailableInteractionState _combinedPrompt;
        private static GamePlayerOwner _owner;

        public static bool IsHolding => _holding;

        /// <returns>True if the game's own Proceed call should be skipped.</returns>
        public static bool TryIntercept(ExfiltrationPoint point, Player player, bool manuallyStarted)
        {
            if (_starting || player == null || !player.IsYourPlayer) return false;
            if (_confirmedPoint == point) return false;

            var status = point.Status;
            if (status != EExfiltrationStatus.RegularMode && status != EExfiltrationStatus.UncompleteRequirements)
            {
                return false;
            }

            // Missing key, switch and so on: let the game show its usual messages
            if (HasUnmetRequirements(point, player)) return false;

            SetPending(point, player, manuallyStarted);
            return true;
        }

        public static void OnPlayerExit(ExfiltrationPoint point, Player player)
        {
            if (player == null || !player.IsYourPlayer) return;

            if (_confirmedPoint == point) _confirmedPoint = null;
            if (_pendingPoint == point) ClearPending();
        }

        public static void OnInteractionsChanged(GamePlayerOwner owner)
        {
            _owner = owner;
            ShowPrompt();
        }

        public static void Tick()
        {
            // The zone is destroyed when the raid ends
            if (_prompt != null && _pendingPoint == null)
            {
                ClearPending();
            }

            if (_holding)
            {
                TimerBox.Apply(GetTimerPanel(), true);
            }
        }

        private static bool HasUnmetRequirements(ExfiltrationPoint point, Player player)
        {
            foreach (var unmet in point.UnmetRequirements(player))
            {
                return true;
            }
            return false;
        }

        private static void SetPending(ExfiltrationPoint point, Player player, bool manuallyStarted)
        {
            if (_pendingPoint == point && _prompt != null) return;

            _pendingPoint = point;
            _pendingPlayer = player;
            _manuallyStarted = manuallyStarted;

            _prompt = new AvailableInteractionState();
            _prompt.Actions.Add(new InteractionAction
            {
                Name = Settings.PromptName.Value,
                Action = Confirm
            });

            player.ForceInteractionsChanged();
            ShowPrompt();
        }

        private static void ClearPending()
        {
            var oldPrompt = _prompt;
            var oldCombined = _combinedPrompt;
            var player = _pendingPlayer;

            if (_holding)
            {
                _holding = false;
                CloseTimer();
            }

            _pendingPoint = null;
            _pendingPlayer = null;
            _prompt = null;
            _combinedPrompt = null;

            if (_owner != null && oldPrompt != null)
            {
                var current = InteractionStateAccess.Read(_owner);
                if (current != null && (current == oldPrompt || current == oldCombined))
                {
                    InteractionStateAccess.Write(_owner, null);
                }
            }

            if (player != null)
            {
                player.ForceInteractionsChanged();
            }
        }

        // Shows the prompt unless something else (a door, a loot container) is being interacted with.
        // If a transit prompt is up as well, both options go into one menu.
        private static void ShowPrompt()
        {
            if (_prompt == null || _owner == null) return;

            var current = InteractionStateAccess.Read(_owner);

            if (current == null)
            {
                _prompt.InitSelected();
                InteractionStateAccess.Write(_owner, _prompt);
                return;
            }

            if (current == _prompt || current == _combinedPrompt) return;

            if (TransitController.Exist<ClientTransitController>(out var transit) &&
                current == transit.AvailableInteractionState)
            {
                _combinedPrompt = new AvailableInteractionState { Error = current.Error };
                _combinedPrompt.Actions.AddRange(_prompt.Actions);
                _combinedPrompt.Actions.AddRange(current.Actions);
                _combinedPrompt.InitSelected();
                InteractionStateAccess.Write(_owner, _combinedPrompt);
            }
        }

        private static void Confirm()
        {
            if (_pendingPoint == null || _holding) return;

            if (!Settings.RequireHold.Value || Settings.HoldSeconds.Value <= 0f)
            {
                StartExtract();
                return;
            }

            BeginHold();
        }

        // Same 2 second hold the transit prompt uses
        private static void BeginHold()
        {
            var point = _pendingPoint;
            var player = _pendingPlayer;
            float seconds = Settings.HoldSeconds.Value;

            if (!(player.CurrentState is IdlePlayerState))
            {
                NotificationManager.DisplayMessageNotification("NeedIdle".Localized());
                return;
            }

            _holding = true;
            GetTimerPanel().Show(seconds, Settings.HoldText.Value);

            player.CurrentManagedState.Plant(true, true, seconds, (bool successful) =>
            {
                _holding = false;
                CloseTimer();

                if (!successful) return;

                player.MovementContext.ResetCanUsePropState();
                if (_pendingPoint == point)
                {
                    StartExtract();
                }
            });
        }

        private static void StartExtract()
        {
            var point = _pendingPoint;
            var player = _pendingPlayer;
            bool manuallyStarted = _manuallyStarted;

            ClearPending();
            _confirmedPoint = point;

            _starting = true;
            try
            {
                ProceedMethod.Invoke(point, new object[] { player, manuallyStarted });
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Could not start the extract: {e}");
            }
            finally
            {
                _starting = false;
            }
        }

        private static LocationTransitTimerPanel GetTimerPanel()
        {
            var gameUI = MonoBehaviourSingleton<GameUI>.Instance;
            return gameUI != null ? gameUI.LocationTransitTimerPanel : null;
        }

        private static void CloseTimer()
        {
            var panel = GetTimerPanel();
            if (panel == null) return;

            panel.Close();
            TimerBox.Apply(panel, false);
        }
    }
}
