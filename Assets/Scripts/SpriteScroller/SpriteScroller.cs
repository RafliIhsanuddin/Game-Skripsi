using UnityEngine;

public class SpriteScroller : MonoBehaviour
{

    [SerializeField] Vector2 moveSpeed;

    Vector2 offset;
    Material material;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        material = GetComponent<SpriteRenderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        offset = moveSpeed * Time.deltaTime;
        material.mainTextureOffset += offset;
    }
}
