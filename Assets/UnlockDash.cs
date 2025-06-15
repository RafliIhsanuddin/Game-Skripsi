using UnityEngine;
using Yarn.Unity;

public class DashUnlocker : MonoBehaviour
{
    public Movement kael;

    [YarnCommand("UnlockDash")]
    public void UnlockKaelDash()
    {
        if (kael != null)
        {
            kael.dashEnabled = true;
        }
        else
        {
            Debug.LogWarning("Kael reference not set!");
        }
    }
}
