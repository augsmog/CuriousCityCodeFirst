using UnityEngine;
using CuriousCity.Characters;  // For FirstPersonController

namespace CuriousCity.Core
{
    public interface IInteractable
    {
        void Interact(FirstPersonController player);
        string GetInteractionPrompt();
        string GetInteractionType();
        bool CanInteract();
    }
}