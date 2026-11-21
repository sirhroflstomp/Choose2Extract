using EFT;

namespace Choose2Extract
{
    /// <summary>
    /// Gives the transit prompt its own label so it can't be mistaken for the extract prompt.
    /// </summary>
    internal static class TransitPrompt
    {
        public static void Rename()
        {
            string name = Settings.TransitPromptName.Value;
            if (string.IsNullOrEmpty(name)) return;

            if (!TransitController.Exist<ClientTransitController>(out var transit)) return;

            var state = transit.AvailableInteractionState;
            if (state == null) return;

            foreach (var action in state.Actions)
            {
                action.Name = name;
            }
        }
    }
}
