using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro; // TextMeshPro에 필요

namespace SmallScaleInc.CharacterCreatorModern
{
    public class AnimatorGearSwitcher : MonoBehaviour
    {
        [Header("장비용 애니메이터 컨트롤러")]
        public List<RuntimeAnimatorController> animatorControllers;

        [Header("UI 버튼")]
        public Button nextButton;
        public Button previousButton;
        public Button randomButton; // 랜덤 버튼 추가됨

        [Header("색상 토글")]
        public Toggle[] colorToggles = new Toggle[5];
        public Color[] toggleColors = new Color[5];
        // 새로운 스킨 컬러 플래그: true이면 위 리스트에서 랜덤 토글이 선택됨
        public bool isSkinColor;

        [Header("무기 설정")]
        public bool isWeapon; // 이 장비 부위가 무기인 경우 true로 설정
        public Toggle[] weaponToggles; // 무기용 토글 리스트

        [Header("가방 설정")]
        public bool isBag; // 이 장비 부위가 배낭인 경우 true로 설정
        public Toggle[] bagToggles; // 배낭용 토글 리스트

        [Header("방패 설정")]
        public bool isShield; // 이 장비 부위가 방패인 경우 true로 설정
        public Toggle[] shieldToggles; // 방패용 토글 리스트

        [Header("전역 색상 견본 패널 (모든 장비 부위가 공유)")]
        public GameObject colorSwatchPanel; // 모든 장비 부위가 공유하는 패널
        public Button[] colorSwatchButtons; // 지정된 색상을 가진 견본 버튼들
        public Button closeColorPickerButton; // 작업 없이 패널을 닫는 버튼

        [Header("UI 색상 정보")]
        public TextMeshProUGUI colorInfoText; // 견본 정보를 표시할 TextMeshPro 객체

        [Header("무기 색상 선택기")]
        // 무기 색상 선택기를 사용할 때 모두 업데이트되어야 하는 무기 SpriteRenderer 리스트
        public List<SpriteRenderer> weaponSpriteRenderers;

        public AnimationController animationController;

        private Animator animator;
        private SpriteRenderer spriteRenderer;
        private int currentAnimatorIndex = 0;

        // 색상 변경을 위한 활성 대상을 저장하는 정적 변수들
        public static SpriteRenderer currentTarget; // 장비용 (단일 대상)
        public static List<SpriteRenderer> currentWeaponTargets; // 무기용 (다중 대상)
        public static bool globalSwatchSetup = false;
        public static GameObject globalColorSwatchPanel;

        // 스크립트가 부착된 신체 부위는? 시작 시 장비와 색상을 랜덤화하는 데 사용됨
        public bool isHead;
        public bool isChest;
        public bool isLegs;
        public bool isShoes;
        // 이전 "isSkin" 필드는 유지됨 (다른 곳에서 사용되는 경우) 하지만 이제 스킨 색상 랜덤화에는 isSkinColor를 사용

        void Start()
        {
            animator = GetComponent<Animator>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            // 애니메이터 버튼 설정
            if (nextButton != null)
                nextButton.onClick.AddListener(NextAnimator);
            if (previousButton != null)
                previousButton.onClick.AddListener(PreviousAnimator);
            if (randomButton != null)
                randomButton.onClick.AddListener(RandomGear);

            // 첫 번째 애니메이터 컨트롤러 초기화
            if (animatorControllers.Count > 0)
                animator.runtimeAnimatorController = animatorControllers[currentAnimatorIndex];

            // 색상 토글 설정
            for (int i = 0; i < colorToggles.Length; i++)
            {
                int index = i; // 클로저를 위한 지역 복사본
                if (colorToggles[index] != null)
                {
                    colorToggles[index].onValueChanged.AddListener(isOn =>
                    {
                        if (isOn)
                            ChangeColor(index);
                    });
                }
            }
            // 토글의 색상 설정
            for (int i = 0; i < colorToggles.Length; i++)
            {
                if (colorToggles[i] != null && i < toggleColors.Length && colorToggles[i].targetGraphic != null)
                {
                    colorToggles[i].targetGraphic.color = toggleColors[i];
                }
            }

            // 전역 색상 견본 패널과 버튼들을 한 번만 설정
            if (!globalSwatchSetup && colorSwatchPanel != null && colorSwatchButtons != null)
            {
                globalColorSwatchPanel = colorSwatchPanel;
                SetupColorSwatchButtons();
                SetupCloseButton();
                globalSwatchSetup = true;
                // 선택적으로, 기본적으로 패널을 숨김
                globalColorSwatchPanel.SetActive(false);
            }

            // 이 장비 부위가 시작 시 자동으로 랜덤화되어야 하는 경우, 모든 가능한 유형 포함
            if (isHead || isChest || isLegs || isShoes || isWeapon || isBag || isShield || isSkinColor)
            {
                RandomGear();
            }
        }

        void NextAnimator()
        {
            if (animatorControllers.Count == 0) return;
            currentAnimatorIndex = (currentAnimatorIndex + 1) % animatorControllers.Count;
            animator.runtimeAnimatorController = animatorControllers[currentAnimatorIndex];
        }

        void PreviousAnimator()
        {
            if (animatorControllers.Count == 0) return;
            currentAnimatorIndex--;
            if (currentAnimatorIndex < 0)
                currentAnimatorIndex = animatorControllers.Count - 1;
            animator.runtimeAnimatorController = animatorControllers[currentAnimatorIndex];
        }

        void ChangeColor(int toggleIndex)
        {
            if (spriteRenderer != null && toggleColors.Length > toggleIndex)
            {
                spriteRenderer.color = toggleColors[toggleIndex];
            }
        }

        // 랜덤 애니메이터/색상 또는 해당되는 경우 랜덤 토글(무기, 가방, 방패, 스킨 색상)을 선택
        public void RandomGear()
        {
            bool randomizationApplied = false;

            // 무기 랜덤화
            if (isWeapon && weaponToggles != null && weaponToggles.Length > 0)
            {
                foreach (Toggle toggle in weaponToggles)
                {
                    if (toggle != null)
                        toggle.isOn = false;
                }
                int randomWeaponIndex = Random.Range(0, weaponToggles.Length);
                if (weaponToggles[randomWeaponIndex] != null)
                    weaponToggles[randomWeaponIndex].isOn = true;
                randomizationApplied = true;
            }

            // 가방 랜덤화
            if (isBag && bagToggles != null && bagToggles.Length > 0)
            {
                foreach (Toggle toggle in bagToggles)
                {
                    if (toggle != null)
                        toggle.isOn = false;
                }
                int randomBagIndex = Random.Range(0, bagToggles.Length);
                if (bagToggles[randomBagIndex] != null)
                    bagToggles[randomBagIndex].isOn = true;
                randomizationApplied = true;
            }

            // 방패 랜덤화
            if (isShield && shieldToggles != null && shieldToggles.Length > 0)
            {
                foreach (Toggle toggle in shieldToggles)
                {
                    if (toggle != null)
                        toggle.isOn = false;
                }
                int randomShieldIndex = Random.Range(0, shieldToggles.Length);
                if (shieldToggles[randomShieldIndex] != null)
                    shieldToggles[randomShieldIndex].isOn = true;
                randomizationApplied = true;
            }

            // isSkinColor가 true인 경우 colorToggles를 사용한 스킨 색상 랜덤화
            if (isSkinColor && colorToggles != null && colorToggles.Length > 0)
            {
                foreach (Toggle toggle in colorToggles)
                {
                    if (toggle != null)
                        toggle.isOn = false;
                }
                int randomSkinIndex = Random.Range(0, colorToggles.Length);
                if (colorToggles[randomSkinIndex] != null)
                    colorToggles[randomSkinIndex].isOn = true;
                randomizationApplied = true;
            }

            // 토글 유형들이 활성화되지 않은 경우, 기본 의류/장비 랜덤화 수행
            if (!randomizationApplied)
            {
                if (animatorControllers.Count > 0)
                {
                    currentAnimatorIndex = Random.Range(0, animatorControllers.Count);
                    animator.runtimeAnimatorController = animatorControllers[currentAnimatorIndex];
                }
                Color randomColor = Random.ColorHSV(0f, 1f, 0f, 1f, 0f, 1f, 1f, 1f);
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = randomColor;
                }
            }
        }

        // 견본 버튼들이 현재 활성 대상들을 업데이트하도록 설정
        void SetupColorSwatchButtons()
        {
            foreach (Button swatchButton in colorSwatchButtons)
            {
                if (swatchButton != null && swatchButton.targetGraphic != null)
                {
                    Color swatchColor = swatchButton.targetGraphic.color;

                    // 클릭 리스너 추가: 클릭 시 적절한 대상들을 업데이트
                    swatchButton.onClick.AddListener(() =>
                    {
                        // 무기 대상들이 활성화된 경우, 모두 업데이트
                        if (currentWeaponTargets != null && currentWeaponTargets.Count > 0)
                        {
                            foreach (SpriteRenderer sr in currentWeaponTargets)
                            {
                                if (sr != null)
                                    sr.color = swatchColor;
                            }
                            currentWeaponTargets = null;
                        }
                        // 그렇지 않고 단일 장비 대상이 활성화된 경우, 업데이트
                        else if (currentTarget != null)
                        {
                            currentTarget.color = swatchColor;
                            currentTarget = null;
                        }
                        // 선택 후 견본 패널 숨김
                        globalColorSwatchPanel.SetActive(false);
                    });

                    // 견본 정보 표시를 위한 포인터 진입 및 이탈 이벤트 설정
                    EventTrigger trigger = swatchButton.gameObject.GetComponent<EventTrigger>();
                    if (trigger == null)
                    {
                        trigger = swatchButton.gameObject.AddComponent<EventTrigger>();
                    }

                    // 포인터 진입 이벤트: 버튼 이름과 16진수 코드 표시
                    EventTrigger.Entry entryEnter = new EventTrigger.Entry();
                    entryEnter.eventID = EventTriggerType.PointerEnter;
                    entryEnter.callback.AddListener((data) =>
                    {
                        if (colorInfoText != null)
                        {
                            string hexCode = "#" + ColorUtility.ToHtmlStringRGB(swatchColor);
                            colorInfoText.text = swatchButton.gameObject.name + ": " + hexCode;
                        }
                    });
                    trigger.triggers.Add(entryEnter);

                    // 포인터 이탈 이벤트: 텍스트 지움
                    EventTrigger.Entry entryExit = new EventTrigger.Entry();
                    entryExit.eventID = EventTriggerType.PointerExit;
                    entryExit.callback.AddListener((data) =>
                    {
                        if (colorInfoText != null)
                        {
                            colorInfoText.text = "";
                        }
                    });
                    trigger.triggers.Add(entryExit);
                }
            }
        }

        // 견본 패널을 숨기는 닫기 버튼 설정
        void SetupCloseButton()
        {
            if (closeColorPickerButton != null)
            {
                closeColorPickerButton.onClick.AddListener(CloseColorPicker);
            }
        }

        // 장비 부위의 자체 "색상 선택기 열기" 버튼에서 이 메서드를 호출
        // 해당 장비 부위를 현재 대상으로 표시하고 견본 패널을 표시
        public void OpenColorPicker()
        {
            currentTarget = spriteRenderer;
            currentWeaponTargets = null; // 무기 대상 지움
            if (globalColorSwatchPanel != null)
            {
                globalColorSwatchPanel.SetActive(true);
            }
        }

        // 무기 색상 선택기 버튼에서 이 메서드를 호출
        // 무기 SpriteRenderers 리스트를 정적 무기 대상에 할당하고 견본 패널을 열음
        public void OpenWeaponColorPicker()
        {
            currentWeaponTargets = weaponSpriteRenderers;
            currentTarget = null; // 장비 대상이 지워짐을 보장
            if (globalColorSwatchPanel != null)
            {
                globalColorSwatchPanel.SetActive(true);
            }
        }

        // 색상을 적용하지 않고 색상 견본 패널을 닫음
        public void CloseColorPicker()
        {
            if (globalColorSwatchPanel != null)
            {
                globalColorSwatchPanel.SetActive(false);
            }
            // 두 대상 모두 지움
            currentTarget = null;
            if (currentWeaponTargets != null)
                currentWeaponTargets = null;
        }
    }
}