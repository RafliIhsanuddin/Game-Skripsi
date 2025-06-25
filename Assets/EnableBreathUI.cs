using UnityEngine;
using Yarn.Unity;

public class EnableBreathUI : MonoBehaviour
{
    public GameObject breathUI;

    [YarnCommand("EnableBreathUI")]
    public void EnablingBreathUI()
    {
        breathUI.SetActive(true);
    }

}
