using UnityEngine;

public class ExpressionRegistrarDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            var registrars = FindObjectsOfType<ExpressionCommandRegistrar>(true);
            Debug.Log($"Jumlah ExpressionCommandRegistrar aktif di scene: {registrars.Length}");

            foreach (var registrar in registrars)
            {
                Debug.Log($"Terpasang di GameObject: {registrar.gameObject.name}", registrar.gameObject);
            }
        }
    }
}
