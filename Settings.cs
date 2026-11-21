using BepInEx.Configuration;

namespace Choose2Extract
{
    internal static class Settings
    {
        public static ConfigEntry<string> PromptName { get; private set; }
        public static ConfigEntry<string> TransitPromptName { get; private set; }
        public static ConfigEntry<bool> RequireHold { get; private set; }
        public static ConfigEntry<float> HoldSeconds { get; private set; }
        public static ConfigEntry<string> HoldText { get; private set; }
        public static ConfigEntry<string> HoldBoxColor { get; private set; }

        public static void Bind(ConfigFile config)
        {
            PromptName = config.Bind("General", "Prompt name", "Extract",
                "Text shown on the extract prompt.");

            TransitPromptName = config.Bind("General", "Transit prompt name", "Transit",
                "Text shown on the transit prompt, so it can't be mistaken for the extract prompt. Leave empty to keep the game's own text.");

            RequireHold = config.Bind("General", "Require hold to confirm", true,
                "Hold the interact key to confirm, like a transit. Turn this off to extract as soon as you select the prompt.");

            HoldSeconds = config.Bind("General", "Hold seconds", 2f,
                "How long the interact key has to be held. 0 skips the hold.");

            HoldText = config.Bind("General", "Countdown text", "Confirmation {0:F1}",
                "Text of the countdown while holding. {0:F1} is the time left.");

            HoldBoxColor = config.Bind("General", "Hold box color", "#7CB704DC",
                "Color of the countdown box as #RRGGBB or #RRGGBBAA. The default matches the game's extraction timer.");
        }
    }
}
