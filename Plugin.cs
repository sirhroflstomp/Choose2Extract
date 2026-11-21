using System;
using BepInEx;
using BepInEx.Logging;
using SPT.Reflection.Patching;

namespace Choose2Extract
{
    [BepInPlugin("com.sirhroflstomp.choose2extract", "sirhroflstomp-Choose2Extract", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log { get; private set; }

        private void Awake()
        {
            Log = Logger;
            Settings.Bind(Config);

            // ProceedPatch goes last: if anything before it fails we never hold back an
            // extract that has no prompt to confirm it.
            var patches = new ModulePatch[]
            {
                new InteractionsPatch(),
                new ExitPatch(),
                new TimerPanelPatch(),
                new ProceedPatch()
            };

            try
            {
                foreach (var patch in patches)
                {
                    patch.Enable();
                }
            }
            catch (Exception e)
            {
                Log.LogError($"Could not apply patches, extracting works as normal: {e}");
            }
        }

        private void Update()
        {
            ExtractController.Tick();
        }
    }
}
