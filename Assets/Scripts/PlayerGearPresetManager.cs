using System.Collections.Generic;
using UnityEngine;

public class GearPresetManager : MonoBehaviour
{
    // �� ��� ������ ���� ������ ��� Ŭ����
    [System.Serializable]
    public class GearPreset
    {
        [Header("�Ӹ�")]
        public RuntimeAnimatorController headAnimator;
        public Color headColor;

        [Header("����")]
        public RuntimeAnimatorController chestAnimator;
        public Color chestColor;

        [Header("�ٸ�")]
        public RuntimeAnimatorController legsAnimator;
        public Color legsColor;

        [Header("�Ź�")]
        public RuntimeAnimatorController shoesAnimator;
        public Color shoesColor;

        [Header("�Ǻ�")]
        public Color skinColor;

        [Header("����")]
        // �� �����¿� ����� ���� GameObject (SpriteRenderer ����)�� �巡�� �� ���
        public GameObject weapon;

        [Header("���� (���� ����)")]
        // �� �����¿� ����� ���� GameObject (SpriteRenderer ����)�� �巡�� �� ���
        // �ʿ� ������ ����α�
        public GameObject shield;
        public Color shieldColor;

        [Header("�賶 (���� ����)")]
        // �� �����¿� ����� �賶 GameObject (SpriteRenderer ����)�� �巡�� �� ���
        // �ʿ� ������ ����α�
        public GameObject backpack;
        public Color backpackColor;
    }

    [Header("������ ����")]
    // �ν����Ϳ��� ������ ����
    public List<GearPreset> presets = new List<GearPreset>();

    //[Header("������ ��� UI")]
    //// ��۵��� ���⿡ �Ҵ� (���������� ToggleGroup�� ���� ����)
    //public Toggle[] presetToggles;

    [Header("��� ���� ����")]
    // �� ��� ������Ʈ �Ҵ� (Animator/SpriteRenderer ����)
    public Animator headAnimatorComponent;
    public SpriteRenderer headRenderer;

    public Animator chestAnimatorComponent;
    public SpriteRenderer chestRenderer;

    public Animator legsAnimatorComponent;
    public SpriteRenderer legsRenderer;

    public Animator shoesAnimatorComponent;
    public SpriteRenderer shoesRenderer;

    [Header("�Ǻ� ���� ����")]
    // �Ǻ� ������ ������ ���� SpriteRenderer
    public SpriteRenderer skinRenderer;

    [Header("���� ����")]
    // ��� ���� GameObject�� ���⿡ ��� (SpriteRenderer ����)
    // ������ ���� �� ��� SpriteRenderer�� ��Ȱ��ȭ�� ��, �ش� �������� ���⸸ Ȱ��ȭ
    public List<GameObject> allWeaponObjects = new List<GameObject>();

    [Header("���� ����")]
    // ��� ���� GameObject ��� (SpriteRenderer ����)
    public List<GameObject> allShieldObjects = new List<GameObject>();

    [Header("�賶 ����")]
    // ��� �賶 GameObject ��� (SpriteRenderer ����)
    public List<GameObject> allBackpackObjects = new List<GameObject>();

    void Start()
    {
        //// ��� ������ ������ ������ ���� ������ ���
        //if (presetToggles.Length != presets.Count)
        //{
        //    Debug.LogWarning("������ ��� ������ ���ǵ� ������ ������ ��ġ���� �ʽ��ϴ�.");
        //}

        //// �� ��ۿ� ������ �Ҵ� �� ������ �ش� ������ ����
        //for (int i = 0; i < presetToggles.Length; i++)
        //{
        //    int index = i; // Ŭ������ ���� ����
        //    if (presetToggles[i] != null)
        //    {
        //        presetToggles[i].onValueChanged.AddListener(isOn =>
        //        {
        //            if (isOn)
        //                ApplyPreset(index);
        //        });
        //    }
        //}
    }

    // �־��� �ε����� �������� ����
    public void ApplyPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= presets.Count)
        {
            Debug.LogError("������ �ε��� ���� �ʰ�!");
            return;
        }

        GearPreset preset = presets[presetIndex];

        // �Ӹ� ��� ������Ʈ
        if (headAnimatorComponent != null)
            headAnimatorComponent.runtimeAnimatorController = preset.headAnimator;
        if (headRenderer != null)
            headRenderer.color = preset.headColor;

        // ���� ��� ������Ʈ
        if (chestAnimatorComponent != null)
            chestAnimatorComponent.runtimeAnimatorController = preset.chestAnimator;
        if (chestRenderer != null)
            chestRenderer.color = preset.chestColor;

        // �ٸ� ��� ������Ʈ
        if (legsAnimatorComponent != null)
            legsAnimatorComponent.runtimeAnimatorController = preset.legsAnimator;
        if (legsRenderer != null)
            legsRenderer.color = preset.legsColor;

        // �Ź� ��� ������Ʈ
        if (shoesAnimatorComponent != null)
            shoesAnimatorComponent.runtimeAnimatorController = preset.shoesAnimator;
        if (shoesRenderer != null)
            shoesRenderer.color = preset.shoesColor;

        // �Ǻ� ���� ������Ʈ
        if (skinRenderer != null)
            skinRenderer.color = preset.skinColor;

        // ��� ���� SpriteRenderer ��Ȱ��ȭ
        if (allWeaponObjects != null)
        {
            foreach (GameObject weaponObj in allWeaponObjects)
            {
                if (weaponObj != null)
                {
                    SpriteRenderer sr = weaponObj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.enabled = false;
                }
            }
        }

        // �������� ���� SpriteRenderer Ȱ��ȭ
        if (preset.weapon != null)
        {
            SpriteRenderer presetWeaponSR = preset.weapon.GetComponent<SpriteRenderer>();
            if (presetWeaponSR != null)
                presetWeaponSR.enabled = true;
        }

        // ��� ���� SpriteRenderer ��Ȱ��ȭ
        if (allShieldObjects != null)
        {
            foreach (GameObject shieldObj in allShieldObjects)
            {
                if (shieldObj != null)
                {
                    SpriteRenderer sr = shieldObj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.enabled = false;
                }
            }
        }

        // �������� ���� SpriteRenderer Ȱ��ȭ (���� ����)
        if (preset.shield != null)
        {
            SpriteRenderer presetShieldSR = preset.shield.GetComponent<SpriteRenderer>();
            if (presetShieldSR != null)
            {
                presetShieldSR.color = preset.shieldColor;
                presetShieldSR.enabled = true;
            }
        }

        // ��� �賶 SpriteRenderer ��Ȱ��ȭ
        if (allBackpackObjects != null)
        {
            foreach (GameObject backpackObj in allBackpackObjects)
            {
                if (backpackObj != null)
                {
                    SpriteRenderer sr = backpackObj.GetComponent<SpriteRenderer>();
                    if (sr != null)
                        sr.enabled = false;
                }
            }
        }

        // �������� �賶 SpriteRenderer Ȱ��ȭ (���� ����)
        if (preset.backpack != null)
        {
            SpriteRenderer presetBackpackSR = preset.backpack.GetComponent<SpriteRenderer>();
            if (presetBackpackSR != null)
            {
                presetBackpackSR.color = preset.backpackColor;
                presetBackpackSR.enabled = true;
            }
        }
    }
}
