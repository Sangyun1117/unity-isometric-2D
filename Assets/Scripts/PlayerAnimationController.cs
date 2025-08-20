using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [Header("Sprite Renderers")]
    public SpriteRenderer renderer;


    [Header("Sprite Arrays (Set in Inspector)")]
    [SerializeField]public Sprite[] sprites;

    // ���� �������� ������ ���� (�ɼ�, ���� ����ȭ��)
    private int currentFrame = -1;

    /// <summary>
    /// Animation Event���� ȣ��
    /// frameIndex: �ش� ������ ��ȣ
    /// </summary>
    public void SetFrame(int frameIndex)
    {
        // ���� �������̸� ���� ����
        if (frameIndex == currentFrame) return;
        currentFrame = frameIndex;

        if (renderer != null && sprites != null && frameIndex < sprites.Length)
            renderer.sprite = sprites[frameIndex];

    }
}
