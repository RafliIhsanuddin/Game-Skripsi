using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

public class ShowRelic : MonoBehaviour
{
    public GameObject relic1;
    public GameObject relic2;

    [YarnCommand("Show1")]
    public void ShowDoubleJumpRelic()
    {
        relic1.SetActive(true);
    }

    [YarnCommand("Hide1")]
    public void HideDoubleJumpRelic()
    {
        relic1.SetActive(false);
    }

    [YarnCommand("Show2")]
    public void ShowWallJumpRelic()
    {
        relic2.SetActive(true);
    }

    [YarnCommand("Hide2")]
    public void HideWallJumpRelic()
    {
        relic2.SetActive(false);
    }
}
