using UnityEngine;

public class Ghost : MonoBehaviour
{

    public float ghostDelay;
    private float ghostDelaySeconds;
    public GameObject ghost;

    public bool makeGhost = false;

    [SerializeField] PlayerDirection playerDirection;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ghostDelaySeconds = ghostDelay;

        if (playerDirection == null)
        {
            Debug.LogError("PlayerDirection script not found on the GameObject!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (makeGhost)
        {
            if (ghostDelaySeconds > 0)
            {
                ghostDelaySeconds -= Time.deltaTime;
            }
            else
            {
                // Generate a ghost
                GameObject currentGhost = Instantiate(ghost, transform.position, Quaternion.identity);
                Sprite currentSprite = GetComponent<SpriteRenderer>().sprite;

                currentGhost.GetComponent<SpriteRenderer>().sprite = currentSprite;

                // Mengatur arah ghost sesuai dengan arah player
                if (playerDirection != null)
                {
                    if (playerDirection.horizontalSpeed < 0)
                    {
                        currentGhost.transform.localScale = new Vector3(-0.2f, 0.2f, 0.2f); // Menghadap kiri
                    }
                    else if (playerDirection.horizontalSpeed > 0)
                    {
                        currentGhost.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f); // Menghadap kanan
                    }
                }

                ghostDelaySeconds = ghostDelay;
                Destroy(currentGhost, 1f);
            }
        }
    }

}
