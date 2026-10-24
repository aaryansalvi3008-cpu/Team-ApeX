namespace EscapeTheLab.Interaction
{
    /// <summary>
    /// Interface implemented by any object the player can look at and interact with using 'E'.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Text prompt displayed on the player's HUD when looking at this object.
        /// Example: "[E] Pick up Keycard" or "[E] Open Door"
        /// </summary>
        string GetPromptText();

        /// <summary>
        /// Executes the interaction logic when the player presses 'E'.
        /// </summary>
        void Interact(PlayerInteraction player);

        /// <summary>
        /// Whether the object is currently interactive.
        /// </summary>
        bool CanInteract();
    }
}
