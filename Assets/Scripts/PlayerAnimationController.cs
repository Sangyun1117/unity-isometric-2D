using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Sprite Renderers")]
    public SpriteRenderer renderer;


    [Header("Sprite Arrays (Set in Inspector)")]
    [SerializeField]public Sprite[] sprites;

    // 현재 파츠별로 프레임 저장 (옵션, 성능 최적화용)
    private int currentFrame = -1;

    /// <summary>
    /// Animation Event에서 호출
    /// frameIndex: 해당 프레임 번호
    /// </summary>
    public void SetFrame(int frameIndex)
    {
        // 같은 프레임이면 갱신 안함
        if (frameIndex == currentFrame) return;
        currentFrame = frameIndex;

        if (renderer != null && sprites != null && frameIndex < sprites.Length)
            renderer.sprite = sprites[frameIndex];

    }
}
