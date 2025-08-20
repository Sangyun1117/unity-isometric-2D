using System.Collections.Generic;
using UnityEngine;

public class GearPresetManager : MonoBehaviour
{
    // 각 장비 조각에 대한 설정을 담는 클래스
    [System.Serializable]
    public class GearPreset
    {
        [Header("머리")]
        public RuntimeAnimatorController headAnimator;
        public Color headColor;

        [Header("가슴")]
        public RuntimeAnimatorController chestAnimator;
        public Color chestColor;

        [Header("다리")]
        public RuntimeAnimatorController legsAnimator;
        public Color legsColor;

        [Header("신발")]
        public RuntimeAnimatorController shoesAnimator;
        public Color shoesColor;

        [Header("피부")]
        public Color skinColor;

        [Header("무기")]
        // 이 프리셋에 사용할 무기 GameObject (SpriteRenderer 포함)를 드래그 앤 드롭
        public GameObject weapon;

        [Header("방패 (선택 사항)")]
        // 이 프리셋에 사용할 방패 GameObject (SpriteRenderer 포함)를 드래그 앤 드롭
        // 필요 없으면 비워두기
        public GameObject shield;
        public Color shieldColor;

        [Header("배낭 (선택 사항)")]
        // 이 프리셋에 사용할 배낭 GameObject (SpriteRenderer 포함)를 드래그 앤 드롭
        // 필요 없으면 비워두기
        public GameObject backpack;
        public Color backpackColor;
    }

    [Header("프리셋 설정")]
    // 인스펙터에서 프리셋 정의
    public List<GearPreset> presets = new List<GearPreset>();

    //[Header("프리셋 토글 UI")]
    //// 토글들을 여기에 할당 (선택적으로 ToggleGroup에 연결 가능)
    //public Toggle[] presetToggles;

    [Header("장비 슬롯 참조")]
    // 각 장비 오브젝트 할당 (Animator/SpriteRenderer 포함)
    public Animator headAnimatorComponent;
    public SpriteRenderer headRenderer;

    public Animator chestAnimatorComponent;
    public SpriteRenderer chestRenderer;

    public Animator legsAnimatorComponent;
    public SpriteRenderer legsRenderer;

    public Animator shoesAnimatorComponent;
    public SpriteRenderer shoesRenderer;

    [Header("피부 슬롯 참조")]
    // 피부 색상을 적용할 단일 SpriteRenderer
    public SpriteRenderer skinRenderer;

    [Header("무기 참조")]
    // 모든 무기 GameObject를 여기에 등록 (SpriteRenderer 포함)
    // 프리셋 적용 시 모든 SpriteRenderer를 비활성화한 후, 해당 프리셋의 무기만 활성화
    public List<GameObject> allWeaponObjects = new List<GameObject>();

    [Header("방패 참조")]
    // 모든 방패 GameObject 등록 (SpriteRenderer 포함)
    public List<GameObject> allShieldObjects = new List<GameObject>();

    [Header("배낭 참조")]
    // 모든 배낭 GameObject 등록 (SpriteRenderer 포함)
    public List<GameObject> allBackpackObjects = new List<GameObject>();

    void Start()
    {
        //// 토글 개수와 프리셋 개수가 맞지 않으면 경고
        //if (presetToggles.Length != presets.Count)
        //{
        //    Debug.LogWarning("프리셋 토글 개수가 정의된 프리셋 개수와 일치하지 않습니다.");
        //}

        //// 각 토글에 리스너 할당 → 켜지면 해당 프리셋 적용
        //for (int i = 0; i < presetToggles.Length; i++)
        //{
        //    int index = i; // 클로저용 로컬 복사
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

    // 주어진 인덱스의 프리셋을 적용
    public void ApplyPreset(int presetIndex)
    {
        if (presetIndex < 0 || presetIndex >= presets.Count)
        {
            Debug.LogError("프리셋 인덱스 범위 초과!");
            return;
        }

        GearPreset preset = presets[presetIndex];

        // 머리 장비 업데이트
        if (headAnimatorComponent != null)
            headAnimatorComponent.runtimeAnimatorController = preset.headAnimator;
        if (headRenderer != null)
            headRenderer.color = preset.headColor;

        // 가슴 장비 업데이트
        if (chestAnimatorComponent != null)
            chestAnimatorComponent.runtimeAnimatorController = preset.chestAnimator;
        if (chestRenderer != null)
            chestRenderer.color = preset.chestColor;

        // 다리 장비 업데이트
        if (legsAnimatorComponent != null)
            legsAnimatorComponent.runtimeAnimatorController = preset.legsAnimator;
        if (legsRenderer != null)
            legsRenderer.color = preset.legsColor;

        // 신발 장비 업데이트
        if (shoesAnimatorComponent != null)
            shoesAnimatorComponent.runtimeAnimatorController = preset.shoesAnimator;
        if (shoesRenderer != null)
            shoesRenderer.color = preset.shoesColor;

        // 피부 색상 업데이트
        if (skinRenderer != null)
            skinRenderer.color = preset.skinColor;

        // 모든 무기 SpriteRenderer 비활성화
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

        // 프리셋의 무기 SpriteRenderer 활성화
        if (preset.weapon != null)
        {
            SpriteRenderer presetWeaponSR = preset.weapon.GetComponent<SpriteRenderer>();
            if (presetWeaponSR != null)
                presetWeaponSR.enabled = true;
        }

        // 모든 방패 SpriteRenderer 비활성화
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

        // 프리셋의 방패 SpriteRenderer 활성화 (선택 사항)
        if (preset.shield != null)
        {
            SpriteRenderer presetShieldSR = preset.shield.GetComponent<SpriteRenderer>();
            if (presetShieldSR != null)
            {
                presetShieldSR.color = preset.shieldColor;
                presetShieldSR.enabled = true;
            }
        }

        // 모든 배낭 SpriteRenderer 비활성화
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

        // 프리셋의 배낭 SpriteRenderer 활성화 (선택 사항)
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
