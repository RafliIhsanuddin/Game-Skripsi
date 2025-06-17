using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterPortraitController : MonoBehaviour
{
    [System.Serializable]
    public class Expression
    {
        public string emotion;
        public Sprite sprite;
    }

    public Image portraitImage;
    public List<Expression> expressions;
    public Sprite defaultSprite;

    public void SetExpression(string emotion)
    {
        foreach (var exp in expressions)
        {
            if (exp.emotion == emotion)
            {
                portraitImage.sprite = exp.sprite;
                return;
            }
        }

        // Jika tidak ditemukan, gunakan default
        portraitImage.sprite = defaultSprite;
    }

    public void SetPortraitVisible(bool isVisible)
    {
        if (portraitImage != null)
        {
            portraitImage.enabled = isVisible;
        }
    }
}
