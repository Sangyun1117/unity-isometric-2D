using UnityEngine;
using UnityEngine.Tilemaps;

public class HideColliderColor : MonoBehaviour
{
    private TilemapRenderer tilemapRenderer;
    void Awake()
    {
        tilemapRenderer = GetComponent<TilemapRenderer>();
    }
    void Start()
    {
        tilemapRenderer.enabled = false;
    }
}
