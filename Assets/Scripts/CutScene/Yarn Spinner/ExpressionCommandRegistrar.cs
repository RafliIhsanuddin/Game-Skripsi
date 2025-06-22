using UnityEngine;
using Yarn.Unity;
using Yarn.Markup;
using System.Collections.Generic;

public class ExpressionCommandRegistrar : MonoBehaviour
{
    public DialogueRunner dialogueRunner;
    public List<NamedPortraitController> portraitControllers;

    [System.Serializable]
    public class NamedPortraitController
    {
        public string characterName;
        public CharacterPortraitController controller;
    }

    void Awake()
    {
        dialogueRunner.onDialogueComplete.AddListener(() =>
        {
            HideAllPortraits();
        });

        // Command untuk mengganti ekspresi
        dialogueRunner.AddCommandHandler<string, string>("setportrait", (characterName, emotion) =>
        {
            foreach (var item in portraitControllers)
            {
                if (item.characterName == characterName)
                {
                    item.controller.SetExpression(emotion);
                    return;
                }
            }
            Debug.LogWarning($"No portrait controller found for {characterName}");
        });

        // Command speaker
        dialogueRunner.AddCommandHandler<string>("speaker", (speakerName) =>
        {
            HideAllPortraits();

            // Jika speaker adalah Narator, jangan tampilkan portrait
            if (speakerName.Equals("Puffkin", System.StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Selain itu, tampilkan portrait seperti biasa
            foreach (var item in portraitControllers)
            {
                bool isActive = string.Equals(item.characterName, speakerName, System.StringComparison.OrdinalIgnoreCase);
                item.controller.SetPortraitVisible(isActive);
            }
        });
    }

    private void HideAllPortraits()
    {
        foreach (var item in portraitControllers)
        {
            item.controller.SetPortraitVisible(false);
        }
    }
}
