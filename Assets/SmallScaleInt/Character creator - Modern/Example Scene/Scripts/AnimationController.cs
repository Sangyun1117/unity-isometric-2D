using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace SmallScaleInc.CharacterCreatorModern
{
    public class AnimationController : MonoBehaviour
    {
        public Animator animator;
        public Animator muzzleAnimator;
        public SpriteRenderer muzzleFlashRenderer;
        private SpriteRenderer spriteRenderer;

        public string currentDirection = "isEast"; // 기본 방향
        public bool isCurrentlyRunning; // 디버깅 목적용
        public bool isCrouching = false;

        public bool isMounted = false;
        public Toggle isNotMountedToggle; // 이것이 ON이면 isMounted가 false여야 함을 의미

        public bool isDying = false;
        public PlayerController playerController;
        // 프리팹 리스트들
        [SerializeField] private List<GameObject> bloodPrefabs = new List<GameObject>();
        [SerializeField] private List<GameObject> radiatedPrefabs = new List<GameObject>();

        public bool isRadiated = false; // 방사능 효과 사용 여부 결정


        public float rollTime = 0.5f; // 기본 애니메이션으로 다시 전환하기 전 구르기를 수행하는 시간

        [Header("기본 색상 설정")]
        public Color defaultColor = Color.white;

        void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            GameObject toggleObj = GameObject.Find("IsNotMountedToggle");
            if (toggleObj != null)
            {
                isNotMountedToggle = toggleObj.GetComponent<Toggle>();
            }

            // 안전성 검사
            if (isNotMountedToggle == null)
            {
                Debug.LogWarning("isNotMountedToggle을 찾을 수 없습니다. GameObject의 이름이 'IsNotMountedToggle'이고 Toggle 컴포넌트가 있는지 확인하세요.");
            }
            // animator.SetBool("isEast", true); // 기본 방향을 동쪽으로 설정
            // animator.SetBool("isRunning", false);
            // animator.SetBool("isAttacking", false);
            // animator.SetBool("isWalking", false);
        }

        public void ResetAnimator()
        {
            if (animator != null && animator.runtimeAnimatorController != null)
            {
                animator.Rebind();     // 모든 애니메이터 매개변수와 상태를 리셋
                animator.Update(0f);   // 리셋을 즉시 적용
                ResetMountIdleParameters();
                isMounted = false;
                isCrouching = false;
                animator.SetBool("isEast", true); // 기본 방향을 동쪽으로 설정
            }
        }


        void Update()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            if (isDying)
            {
                return;
            }
            // 작동함
            // if(isAttacking == false)
            // {
            //     HandleMovement();
            // }            
            if (isNotMountedToggle != null)
            {
                // 토글 값이 변경되었는지 확인
                if (!isNotMountedToggle.isOn)
                {
                    if (!isMounted)
                    {
                        ResetAnimator();
                        isMounted = true;
                        TriggerMountIdleAnimation();
                    }
                }
                else
                {
                    if (isMounted)
                    {
                        isMounted = false;
                        ResetMountIdleParameters();
                    }
                }
            }
            HandleAttackAttack();
            HandleMovement();
            // 기타 입력 액션들:
            if (isMounted)
            {
                return;
            }
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (isMounted == false)
                {

                    if (isCrouching == false)
                    {
                        TriggerCrouchIdleAnimation();
                        isCrouching = true;
                    }
                    else
                    {
                        isCrouching = false;
                        // 지연 후 또는 애니메이션 끝에서 웅크리기 유휴 매개변수들을 리셋
                        ResetCrouchIdleParameters();
                    }
                }
            }
            else if (Input.GetKey(KeyCode.Alpha1))
            {
                TriggerTakeDamageAnimation();
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                TriggerSpecialAbility2Animation();
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                TriggerCastSpellAnimation();
            }
            else if (Input.GetKey(KeyCode.Alpha4))
            {
                TriggerKickAnimation();
            }
            else if (Input.GetKey(KeyCode.Alpha5))
            {
                TriggerPummelAnimation();
            }
            else if (Input.GetKey(KeyCode.Alpha6))
            {
                TriggerAttackSpinAnimation();
            }
            else if (Input.GetKey(KeyCode.Alpha7))
            {
                TriggerDie();
            }
            else if (Input.GetKey(KeyCode.LeftShift) && isCurrentlyRunning)
            {
                TriggerFlipAnimation();
            }
            else if (Input.GetKey(KeyCode.LeftControl) && isCurrentlyRunning)
            {
                TriggerRollAnimation();
            }
            else if (Input.GetKey(KeyCode.LeftAlt) && isCurrentlyRunning)
            {
                TriggerSlideAnimation();
            }

        }

        /// <summary>
        /// SpriteRenderer의 색상을 기본 색상으로 설정합니다.
        /// 어디서든 호출할 수 있는 공개 메서드입니다.
        /// </summary>
        public void SetDefaultColor()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = defaultColor;
            }

        }



        public void UpdateDirection(string newDirection)
        {
            // 모든 가능한 방향 이름들을 반복
            string[] directions = { "isWest", "isEast", "isSouth", "isSouthWest", "isNorthEast", "isSouthEast", "isNorth", "isNorthWest" };

            foreach (string direction in directions)
            {
                // 새로운 방향을 제외하고 모든 방향을 false로 설정
                animator.SetBool(direction, direction == newDirection);
            }

            if (currentDirection != newDirection)
            {
                isAttacking = false;
                ResetAttackAttackParameters();
            }
            // 현재 방향 업데이트
            currentDirection = newDirection;
            // 새로운 방향에서 애니메이션을 다시 시작하기 위해 매개변수들을 리셋
        }

        public bool isRunning;
        public bool isRunningBackwards;
        public bool isStrafingLeft;
        public bool isStrafingRight;
        public bool isAttacking = false;


        void HandleMovement()
        {

            // 마우스 위치를 기반으로 방향 계산
            Vector3 mouseScreenPosition = Input.mousePosition;
            mouseScreenPosition.z = Camera.main.transform.position.z - transform.position.z;
            Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
            Vector3 directionToMouse = mouseWorldPosition - transform.position;
            directionToMouse.Normalize(); // 방향 벡터를 정규화

            // 가장 가까운 기본 또는 중간 기본 방향 결정
            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
            if (angle < 0) angle += 360;

            string newDirection = DetermineDirectionFromAngle(angle);

            if (newDirection != currentDirection)
            {
                UpdateDirection(newDirection);
            }
            string movementDirection = newDirection.Substring(2); // 방향 이름에서 "is" 제거

            // 이동 입력 상태들 캡처
            isRunning = Input.GetKey(KeyCode.W);
            isRunningBackwards = Input.GetKey(KeyCode.S);
            isStrafingLeft = Input.GetKey(KeyCode.A);
            isStrafingRight = Input.GetKey(KeyCode.D);

            // 일반적인 이동 불린 설정
            isCurrentlyRunning = isRunning || isRunningBackwards || isStrafingLeft || isStrafingRight;

            // 모든 방향 이동 매개변수들 리셋
            ResetAllMovementBools();


            // 이동 조건들로 애니메이터 업데이트
            animator.SetBool("isRunning", isRunning);
            animator.SetBool("isRunningBackwards", isRunningBackwards);
            animator.SetBool("isStrafingLeft", isStrafingLeft);
            animator.SetBool("isStrafingRight", isStrafingRight);
            if (isMounted)
            { animator.SetBool("isRideRunning", isRunning); }
            if (isCrouching)
            { animator.SetBool("isCrouchRunning", isRunning); }

            // 특정 이동 애니메이션들 설정
            if (isMounted)
            {
                SetMovementAnimation(isRunning, "RideRun", movementDirection);
            }
            else if (isCrouching)
            {
                SetMovementAnimation(isRunning, "CrouchRun", movementDirection);
            }
            else
            {
                SetMovementAnimation(isRunning, "Move", movementDirection);
                SetMovementAnimation(isRunningBackwards, "RunBackwards", movementDirection);
                SetMovementAnimation(isStrafingLeft, "StrafeLeft", movementDirection);
                SetMovementAnimation(isStrafingRight, "StrafeRight", movementDirection);
                SetMovementAnimation(isRunningBackwards, "Move", movementDirection);
                SetMovementAnimation(isStrafingLeft, "Move", movementDirection);
                SetMovementAnimation(isStrafingRight, "Move", movementDirection);
            }
        }

        void SetMovementAnimation(bool isActive, string baseKey, string direction)
        {
            if (isActive)
            {
                string animationKey = $"{baseKey}{direction}";
                animator.SetBool(animationKey, true);
            }
        }

        void ResetAllMovementBools()
        {
            string[] directions = new string[] { "North", "South", "East", "West", "NorthEast", "NorthWest", "SouthEast", "SouthWest" };
            foreach (string baseKey in new string[] { "Move", "RunBackwards", "StrafeLeft", "StrafeRight" })
            {
                foreach (string direction in directions)
                {
                    animator.SetBool($"{baseKey}{direction}", false);
                }
            }
            animator.SetBool("RideRunNorth", false);
            animator.SetBool("RideRunSouth", false);
            animator.SetBool("RideRunEast", false);
            animator.SetBool("RideRunWest", false);
            animator.SetBool("RideRunNorthEast", false);
            animator.SetBool("RideRunNorthWest", false);
            animator.SetBool("RideRunSouthEast", false);
            animator.SetBool("RideRunSouthWest", false);

            animator.SetBool("CrouchRunNorth", false);
            animator.SetBool("CrouchRunSouth", false);
            animator.SetBool("CrouchRunEast", false);
            animator.SetBool("CrouchRunWest", false);
            animator.SetBool("CrouchRunNorthEast", false);
            animator.SetBool("CrouchRunNorthWest", false);
            animator.SetBool("CrouchRunSouthEast", false);
            animator.SetBool("CrouchRunSouthWest", false);
        }


        string DetermineDirectionFromAngle(float angle)
        {
            // 각도를 [0..360)으로 정규화
            angle = (angle + 360) % 360;

            if (angle < 15f || angle >= 345f)
                return "isEast";        // ~0°에 해당
            else if (angle >= 15f && angle < 75f)
                return "isNorthEast";   // ~30°에 해당
            else if (angle >= 75f && angle < 105f)
                return "isNorth";       // 90°에 해당
            else if (angle >= 105f && angle < 165f)
                return "isNorthWest";   // 150°에 해당
            else if (angle >= 165f && angle < 195f)
                return "isWest";        // 180°에 해당
            else if (angle >= 195f && angle < 255f)
                return "isSouthWest";   // 210°에 해당
            else if (angle >= 255f && angle < 285f)
                return "isSouth";       // 270°에 해당
            else if (angle >= 285f && angle < 345f)
                return "isSouthEast";   // 330°에 해당

            // 폴백 (위의 조건들이 0..360을 다 커버하면 여기까지 거의 도달하지 않음)
            return "isEast";
        }



        void SetDirectionBools(bool isWest, bool isEast, bool isSouth, bool isSouthWest, bool isNorthEast, bool isSouthEast, bool isNorth, bool isNorthWest)
        {
            animator.SetBool("isWest", isWest);
            animator.SetBool("isEast", isEast);
            animator.SetBool("isSouth", isSouth);
            animator.SetBool("isSouthWest", isSouthWest);
            animator.SetBool("isNorthEast", isNorthEast);
            animator.SetBool("isSouthEast", isSouthEast);
            animator.SetBool("isNorth", isNorth);
            animator.SetBool("isNorthWest", isNorthWest);
        }


        // 기본 공격들:

        void HandleAttackAttack()
        {
            if (isMounted)
            {
                return;
            }

            if (Input.GetMouseButton(1))
            {
                if (playerController.isMelee)
                {
                    TriggerAttackSpinAnimation();
                    return;
                }
                // if (isCrouching)
                // {
                //     return;
                // }
                isAttacking = true;

                if (playerController.isMelee == false)
                {
                    // 머즐 플래시 스프라이트를 높은 레이어로 이동하여 보이게 만듦
                    muzzleFlashRenderer.sortingOrder = 150;
                }
                // 발사를 시작할 때마다 새로운 머즐 애니메이션을 원한다면,
                // 애니메이터 상태를 리셋하거나 불린들을 토글할 수도 있음.

                // 마우스로부터 방향 파악
                Vector3 mouseScreenPosition = Input.mousePosition;
                mouseScreenPosition.z = Camera.main.transform.position.z - transform.position.z;
                Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);
                Vector3 directionToMouse = mouseWorldPosition - transform.position;
                directionToMouse.Normalize();

                float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg;
                if (angle < 0) angle += 360;
                string newDirection = DetermineDirectionFromAngle(angle);

                // if (newDirection != currentDirection)
                // {
                //     ResetAllGunFireBools();
                //     UpdateDirection(newDirection);
                // }

                // 달리는 중인지 확인
                bool isRunning =
                    Input.GetKey(KeyCode.W) ||
                    Input.GetKey(KeyCode.S) ||
                    Input.GetKey(KeyCode.A) ||
                    Input.GetKey(KeyCode.D);

                // 메인 애니메이터에서 달리기-공격 vs 정지-공격
                if (isRunning)
                {
                    animator.SetBool("isAttackRunning", false);
                    animator.SetBool("isAttackAttacking", false);

                    // // 머즐 애니메이터 로직
                    // if(playerController.isMelee == false)
                    // {
                    //     ResetAllGunFireBools();
                    //     string muzzleState = "Gunfire" + newDirection.Substring(2);
                    //     muzzleAnimator.SetBool(muzzleState, true);
                    // }
                }
                else
                {
                    animator.SetBool("isAttackRunning", false);
                    animator.SetBool("isAttackAttacking", true);

                    // if(playerController.isMelee == false)
                    // {
                    //     // 머즐 애니메이터 로직
                    //     ResetAllGunFireBools();
                    //     string muzzleState = "Gunfire" + newDirection.Substring(2);
                    //     muzzleAnimator.SetBool(muzzleState, true);
                    // }
                }

                // 메인 애니메이터용 AttackAttack 매개변수들도 수행
                TriggerAttack(isRunning, newDirection.Substring(2));
            }
            else if (Input.GetMouseButtonUp(1))
            {
                // 마우스가 떼어짐 => 공격 중지
                isAttacking = false;

                // 메인 애니메이터의 불린들 리셋
                ResetAttackAttackParameters();
                RestoreDirectionAfterAttack();

                if (playerController.isMelee == false)
                {
                    // 머즐 플래시 스프라이트를 정렬 순서 0으로 이동하여 효과적으로 숨김
                    muzzleFlashRenderer.sortingOrder = -5;

                    // 머즐 애니메이터 불린들 리셋
                    // ResetAllGunFireBools();
                }
            }
        }



        // 이것은 당신의 일반적인 메서드이지만, 방향을 매개변수로 추가하고
        // 랜덤 공격 부분을 단순화했습니다. (원하는 대로 조정할 수 있습니다.)
        void TriggerAttack(bool isRunning, string direction)
        {
            if (isCurrentlyRunning)
            { return; }
            // 예: "AttackAttackEast" / "AttackAttackSouth" 등
            string attackParam = "AttackAttack" + direction;
            animator.SetBool(attackParam, true);

            // isRunning? "isAttackRunning" 설정, 아니면 "isAttackAttacking"
            animator.SetBool("isAttackRunning", isRunning);
            animator.SetBool("isAttackAttacking", !isRunning);
        }

        // -------------------------------------------------------------------
        // 머즐 애니메이터 메서드들
        // -------------------------------------------------------------------
        void ResetAllGunFireBools()
        {
            // 머즐 애니메이터에서 모든 8방향 끄기
            // 예: GunFireNorth, GunFireSouth, GunFireEast, ...
            string[] muzzleDirs = {
                "GunfireNorth","GunfireSouth","GunfireEast","GunfireWest",
                "GunfireNorthEast","GunfireNorthWest","GunfireSouthEast","GunfireSouthWest"
            };

            foreach (var dir in muzzleDirs)
            {
                muzzleAnimator.SetBool(dir, false);
            }
        }



        void ResetAttackAttackParameters()
        {
            string[] directions = {
                "North","South","East","West",
                "NorthEast","NorthWest","SouthEast","SouthWest"
            };
            foreach (string dir in directions)
            {
                animator.SetBool("AttackAttack" + dir, false);
                animator.SetBool("Attack2" + dir, false);
                animator.SetBool("AttackRun" + dir, false);
            }
        }

        void RestoreDirectionAfterAttack()
        {
            animator.SetBool("isAttackAttacking", false);
            animator.SetBool("isAttackRunning", false);
            animator.SetBool("isRunning", false);
        }

        // 데미지 받기:

        public void TriggerTakeDamageAnimation()
        {
            if (playerController != null && !playerController.isActive)
            {
                return;
            }
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            // // 데미지 받기 애니메이션을 시작하기 위해 'isTakeDamage'를 true로 설정
            // animator.SetBool("isTakeDamage", true);

            // // 현재 방향을 결정하고 적절한 데미지 받기 애니메이션 트리거
            // if (animator.GetBool("isNorth")) animator.SetBool("TakeDamageNorth", true);
            // else if (animator.GetBool("isSouth")) animator.SetBool("TakeDamageSouth", true);
            // else if (animator.GetBool("isEast")) animator.SetBool("TakeDamageEast", true);
            // else if (animator.GetBool("isWest")) animator.SetBool("TakeDamageWest", true);
            // else if (animator.GetBool("isNorthEast")) animator.SetBool("TakeDamageNorthEast", true);
            // else if (animator.GetBool("isNorthWest")) animator.SetBool("TakeDamageNorthWest", true);
            // else if (animator.GetBool("isSouthEast")) animator.SetBool("TakeDamageSouthEast", true);
            // else if (animator.GetBool("isSouthWest")) animator.SetBool("TakeDamageSouthWest", true);

            // 캐릭터 위치에 적절한 효과 생성
            SpawnEffect();

            // // 선택사항으로, 지연 후 또는 애니메이션 끝에서 데미지 받기 매개변수들을 리셋
            // StartCoroutine(ResetTakeDamageParameters());
        }

        private void SpawnEffect()
        {
            // isRadiated 플래그를 기반으로 사용할 프리팹 리스트 결정
            List<GameObject> prefabsToUse = isRadiated ? radiatedPrefabs : bloodPrefabs;

            if (prefabsToUse == null || prefabsToUse.Count == 0)
            {
                Debug.LogWarning("선택된 리스트에 사용 가능한 프리팹이 없습니다!");
                return;
            }

            // 선택된 리스트에서 랜덤 프리팹 선택
            GameObject selectedPrefab = prefabsToUse[Random.Range(0, prefabsToUse.Count)];

            if (selectedPrefab == null)
            {
                Debug.LogWarning("선택된 프리팹이 null입니다!");
                return;
            }

            // 선택된 프리팹을 캐릭터의 위치와 방향에 인스턴스화
            GameObject effectInstance = Instantiate(selectedPrefab, transform.position, Quaternion.identity);

            // 0.5초 후 Order in Layer를 수정하기 위한 코루틴 시작
            StartCoroutine(UpdateSpriteOrder(effectInstance));
        }

        private IEnumerator UpdateSpriteOrder(GameObject effectInstance)
        {
            if (effectInstance == null)
            {
                yield break;
            }

            // 0.5초 대기
            yield return new WaitForSeconds(0.5f);

            // 효과 인스턴스의 SpriteRenderer 컴포넌트 가져오기
            SpriteRenderer spriteRenderer = effectInstance.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                // Order in Layer를 40으로 설정
                spriteRenderer.sortingOrder = 40;
                // Debug.Log($"Updated sorting order to 40 for {effectInstance.name}");
            }
            else
            {
                Debug.LogWarning("효과 인스턴스에서 SpriteRenderer를 찾을 수 없습니다!");
            }
        }



        IEnumerator ResetTakeDamageParameters()
        {
            // 리셋하기 전에 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(0.5f); // 애니메이션 길이에 따라 대기 시간 조정

            // 모든 데미지 받기 매개변수들을 false로 리셋
            animator.SetBool("isTakeDamage", false);
            animator.SetBool("TakeDamageNorth", false);
            animator.SetBool("TakeDamageSouth", false);
            animator.SetBool("TakeDamageEast", false);
            animator.SetBool("TakeDamageWest", false);
            animator.SetBool("TakeDamageNorthEast", false);
            animator.SetBool("TakeDamageNorthWest", false);
            animator.SetBool("TakeDamageSouthEast", false);
            animator.SetBool("TakeDamageSouthWest", false);

            // 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }

        // 웅크리기:
        public void TriggerCrouchIdleAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 웅크리기 유휴 애니메이션을 시작하기 위해 'isCrouchIdling'을 true로 설정
            animator.SetBool("isCrouchIdling", true);

            // 현재 방향을 결정하고 적절한 웅크리기 유휴 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("CrouchIdleNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("CrouchIdleSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("CrouchIdleEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("CrouchIdleWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("CrouchIdleNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("CrouchIdleNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("CrouchIdleSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("CrouchIdleSouthWest", true);

        }

        public void ResetCrouchIdleParameters()
        {
            // 모든 웅크리기 유휴 매개변수들을 false로 리셋
            animator.SetBool("isCrouchIdling", false);
            animator.SetBool("CrouchIdleNorth", false);
            animator.SetBool("CrouchIdleSouth", false);
            animator.SetBool("CrouchIdleEast", false);
            animator.SetBool("CrouchIdleWest", false);
            animator.SetBool("CrouchIdleNorthEast", false);
            animator.SetBool("CrouchIdleNorthWest", false);
            animator.SetBool("CrouchIdleSouthEast", false);
            animator.SetBool("CrouchIdleSouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }

        // 탑승:
        public void TriggerMountIdleAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 탑승 유휴 애니메이션을 시작하기 위해 'isRideIdling'을 true로 설정
            animator.SetBool("isRideIdling", true);

            // 현재 방향을 결정하고 적절한 탑승 유휴 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("RideIdleNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("RideIdleSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("RideIdleEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("RideIdleWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("RideIdleNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("RideIdleNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("RideIdleSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("RideIdleSouthWest", true);
        }

        public void ResetMountIdleParameters()
        {
            // 모든 탑승 유휴 매개변수들을 false로 리셋
            animator.SetBool("isRideIdling", false);
            animator.SetBool("RideIdleNorth", false);
            animator.SetBool("RideIdleSouth", false);
            animator.SetBool("RideIdleEast", false);
            animator.SetBool("RideIdleWest", false);
            animator.SetBool("RideIdleNorthEast", false);
            animator.SetBool("RideIdleNorthWest", false);
            animator.SetBool("RideIdleSouthEast", false);
            animator.SetBool("RideIdleSouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }


        // 죽음
        public void TriggerDie()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            isDying = true;
            // 현재 방향을 확인하고 적절한 죽음 애니메이션 트리거
            if (currentDirection.Equals("isNorth")) TriggerDeathAnimation("dieNorth");
            else if (currentDirection.Equals("isSouth")) TriggerDeathAnimation("dieSouth");
            else if (currentDirection.Equals("isEast")) TriggerDeathAnimation("dieEast");
            else if (currentDirection.Equals("isWest")) TriggerDeathAnimation("dieWest");
            else if (currentDirection.Equals("isNorthEast")) TriggerDeathAnimation("dieNorthEast");
            else if (currentDirection.Equals("isNorthWest")) TriggerDeathAnimation("dieNorthWest");
            else if (currentDirection.Equals("isSouthEast")) TriggerDeathAnimation("dieSouthEast");
            else if (currentDirection.Equals("isSouthWest")) TriggerDeathAnimation("dieSouthWest");
        }

        private void TriggerDeathAnimation(string deathDirectionTrigger)
        {
            // 특정 죽음 방향 트리거
            animator.SetTrigger(deathDirectionTrigger);

            if (playerController.isDead == false)
                StartCoroutine(ResetDieParameters());
        }


        IEnumerator ResetDieParameters()
        {
            yield return new WaitForSeconds(2);
            animator.ResetTrigger("dieNorth");
            animator.ResetTrigger("dieSouth");
            animator.ResetTrigger("dieEast");
            animator.ResetTrigger("dieWest");
            animator.ResetTrigger("dieNorthEast");
            animator.ResetTrigger("dieNorthWest");
            animator.ResetTrigger("dieSouthEast");
            animator.ResetTrigger("dieSouthWest");

            animator.SetBool("isDie", false);

            // Animator를 기본 상태로 강제 복귀
            animator.Play("IdleEast", 0);
            // 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
            isDying = false;
        }

        // 특수 능력 1:
        public void TriggerSpecialAbility1Animation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 특수 능력 애니메이션을 시작하기 위해 'isSpecialAbility1'을 true로 설정
            animator.SetBool("isSpecialAbility1", true);

            // 현재 방향을 결정하고 적절한 특수 능력 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("SpecialAbility1North", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("SpecialAbility1South", true);
            else if (animator.GetBool("isEast")) animator.SetBool("SpecialAbility1East", true);
            else if (animator.GetBool("isWest")) animator.SetBool("SpecialAbility1West", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("SpecialAbility1NorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("SpecialAbility1NorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("SpecialAbility1SouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("SpecialAbility1SouthWest", true);

            // 지연 후 또는 애니메이션 끝에서 특수 능력 매개변수들을 리셋
            StartCoroutine(ResetSpecialAbility1Parameters());
        }

        IEnumerator ResetSpecialAbility1Parameters()
        {
            // 리셋하기 전에 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(0.5f); // 애니메이션 길이에 따라 대기 시간 조정

            // 모든 특수 능력 매개변수들을 false로 리셋
            animator.SetBool("isSpecialAbility1", false);
            animator.SetBool("SpecialAbility1North", false);
            animator.SetBool("SpecialAbility1South", false);
            animator.SetBool("SpecialAbility1East", false);
            animator.SetBool("SpecialAbility1West", false);
            animator.SetBool("SpecialAbility1NorthEast", false);
            animator.SetBool("SpecialAbility1NorthWest", false);
            animator.SetBool("SpecialAbility1SouthEast", false);
            animator.SetBool("SpecialAbility1SouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }

        // 특수 능력 2:
        public void TriggerSpecialAbility2Animation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 특수 능력 애니메이션을 시작하기 위해 'isSpecialAbility2'를 true로 설정
            animator.SetBool("isSpecialAbility2", true);

            // 현재 방향을 결정하고 적절한 특수 능력 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("SpecialAbility2North", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("SpecialAbility2South", true);
            else if (animator.GetBool("isEast")) animator.SetBool("SpecialAbility2East", true);
            else if (animator.GetBool("isWest")) animator.SetBool("SpecialAbility2West", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("SpecialAbility2NorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("SpecialAbility2NorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("SpecialAbility2SouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("SpecialAbility2SouthWest", true);

            // 지연 후 또는 애니메이션 끝에서 특수 능력 매개변수들을 리셋
            StartCoroutine(ResetSpecialAbility2Parameters());
        }

        IEnumerator ResetSpecialAbility2Parameters()
        {
            // 리셋하기 전에 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(0.5f); // 애니메이션 길이에 따라 대기 시간 조정

            // 모든 특수 능력 매개변수들을 false로 리셋
            animator.SetBool("isSpecialAbility2", false);
            animator.SetBool("SpecialAbility2North", false);
            animator.SetBool("SpecialAbility2South", false);
            animator.SetBool("SpecialAbility2East", false);
            animator.SetBool("SpecialAbility2West", false);
            animator.SetBool("SpecialAbility2NorthEast", false);
            animator.SetBool("SpecialAbility2NorthWest", false);
            animator.SetBool("SpecialAbility2SouthEast", false);
            animator.SetBool("SpecialAbility2SouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }


        // 마법 시전
        public void TriggerCastSpellAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 마법 시전 애니메이션을 시작하기 위해 'isCastingSpell'을 true로 설정
            animator.SetBool("isCastingSpell", true);

            // 현재 방향을 결정하고 적절한 마법 시전 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("CastSpellNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("CastSpellSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("CastSpellEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("CastSpellWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("CastSpellNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("CastSpellNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("CastSpellSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("CastSpellSouthWest", true);

            // 지연 후 또는 애니메이션 끝에서 마법 시전 매개변수들을 리셋
            StartCoroutine(ResetCastSpellParameters());
        }

        IEnumerator ResetCastSpellParameters()
        {
            // 리셋하기 전에 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(0.5f); // 애니메이션 길이에 따라 대기 시간 조정

            // 모든 마법 시전 매개변수들을 false로 리셋
            animator.SetBool("isCastingSpell", false);
            animator.SetBool("CastSpellNorth", false);
            animator.SetBool("CastSpellSouth", false);
            animator.SetBool("CastSpellEast", false);
            animator.SetBool("CastSpellWest", false);
            animator.SetBool("CastSpellNorthEast", false);
            animator.SetBool("CastSpellNorthWest", false);
            animator.SetBool("CastSpellSouthEast", false);
            animator.SetBool("CastSpellSouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }

        // 발차기:
        public void TriggerKickAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 발차기 애니메이션을 시작하기 위해 'isKicking'을 true로 설정
            animator.SetBool("isKicking", true);

            // 현재 방향을 결정하고 적절한 발차기 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("KickNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("KickSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("KickEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("KickWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("KickNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("KickNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("KickSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("KickSouthWest", true);

            // 지연 후 또는 애니메이션 끝에서 발차기 매개변수들을 리셋
            StartCoroutine(ResetKickParameters());
        }

        IEnumerator ResetKickParameters()
        {
            // 리셋하기 전에 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(0.5f); // 애니메이션 길이에 따라 대기 시간 조정

            // 모든 발차기 매개변수들을 false로 리셋
            animator.SetBool("isKicking", false);
            animator.SetBool("KickNorth", false);
            animator.SetBool("KickSouth", false);
            animator.SetBool("KickEast", false);
            animator.SetBool("KickWest", false);
            animator.SetBool("KickNorthEast", false);
            animator.SetBool("KickNorthWest", false);
            animator.SetBool("KickSouthEast", false);
            animator.SetBool("KickSouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }

        // 공중제비 애니메이션:
        public void TriggerFlipAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 공중제비 애니메이션을 시작하기 위해 'isFlipping'을 true로 설정
            animator.SetBool("isFlipping", true);

            // 현재 방향을 결정하고 적절한 공중제비 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("FrontFlipNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("FrontFlipSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("FrontFlipEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("FrontFlipWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("FrontFlipNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("FrontFlipNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("FrontFlipSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("FrontFlipSouthWest", true);

            // 지연 후 또는 애니메이션 끝에서 공중제비 매개변수들을 리셋
            StartCoroutine(ResetFlipParameters());
        }

        IEnumerator ResetFlipParameters()
        {
            // 리셋하기 전에 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(0.5f); // 애니메이션 길이에 따라 대기 시간 조정

            // 모든 공중제비 매개변수들을 false로 리셋
            animator.SetBool("isFlipping", false);
            animator.SetBool("FrontFlipNorth", false);
            animator.SetBool("FrontFlipSouth", false);
            animator.SetBool("FrontFlipEast", false);
            animator.SetBool("FrontFlipWest", false);
            animator.SetBool("FrontFlipNorthEast", false);
            animator.SetBool("FrontFlipNorthWest", false);
            animator.SetBool("FrontFlipSouthEast", false);
            animator.SetBool("FrontFlipSouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }


        // 구르기

        public void TriggerRollAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 구르기 애니메이션을 시작하기 위해 'isRolling'을 true로 설정
            animator.SetBool("isRolling", true);

            // 현재 방향을 결정하고 적절한 구르기 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("RollingNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("RollingSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("RollingEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("RollingWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("RollingNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("RollingNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("RollingSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("RollingSouthWest", true);

            // 지연 후 또는 애니메이션 끝에서 구르기 매개변수들을 리셋
            StartCoroutine(ResetRollParameters());
        }

        IEnumerator ResetRollParameters()
        {
            // 리셋하기 전에 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(rollTime); // 애니메이션 길이에 따라 대기 시간 조정

            // 모든 구르기 매개변수들을 false로 리셋
            animator.SetBool("isRolling", false);
            animator.SetBool("RollingNorth", false);
            animator.SetBool("RollingSouth", false);
            animator.SetBool("RollingEast", false);
            animator.SetBool("RollingWest", false);
            animator.SetBool("RollingNorthEast", false);
            animator.SetBool("RollingNorthWest", false);
            animator.SetBool("RollingSouthEast", false);
            animator.SetBool("RollingSouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }

        // 슬라이딩
        public void TriggerSlideAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 슬라이딩 애니메이션을 시작하기 위해 'isSliding'을 true로 설정
            animator.SetBool("isSliding", true);

            // 현재 방향을 결정하고 적절한 슬라이딩 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("SlidingNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("SlidingSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("SlidingEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("SlidingWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("SlidingNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("SlidingNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("SlidingSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("SlidingSouthWest", true);

            // 지연 후 또는 애니메이션 끝에서 슬라이딩 매개변수들을 리셋
            StartCoroutine(ResetSlideParameters());
        }

        IEnumerator ResetSlideParameters()
        {
            // 리셋하기 전에 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(0.7f); // 애니메이션 길이에 따라 대기 시간 조정

            // 모든 슬라이딩 매개변수들을 false로 리셋
            animator.SetBool("isSliding", false);
            animator.SetBool("SlidingNorth", false);
            animator.SetBool("SlidingSouth", false);
            animator.SetBool("SlidingEast", false);
            animator.SetBool("SlidingWest", false);
            animator.SetBool("SlidingNorthEast", false);
            animator.SetBool("SlidingNorthWest", false);
            animator.SetBool("SlidingSouthEast", false);
            animator.SetBool("SlidingSouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }

        // 연타 공격
        public void TriggerPummelAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 연타 공격 애니메이션을 시작하기 위해 'isPummeling'을 true로 설정
            animator.SetBool("isPummeling", true);

            // 현재 방향을 결정하고 적절한 연타 공격 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("PummelNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("PummelSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("PummelEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("PummelWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("PummelNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("PummelNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("PummelSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("PummelSouthWest", true);

            // 지연 후 또는 애니메이션 끝에서 연타 공격 매개변수들을 리셋
            StartCoroutine(ResetPummelParameters());
        }

        IEnumerator ResetPummelParameters()
        {
            // 리셋하기 전에 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(0.5f); // 애니메이션 길이에 따라 대기 시간 조정

            // 모든 연타 공격 매개변수들을 false로 리셋
            animator.SetBool("isPummeling", false);
            animator.SetBool("PummelNorth", false);
            animator.SetBool("PummelSouth", false);
            animator.SetBool("PummelEast", false);
            animator.SetBool("PummelWest", false);
            animator.SetBool("PummelNorthEast", false);
            animator.SetBool("PummelNorthWest", false);
            animator.SetBool("PummelSouthEast", false);
            animator.SetBool("PummelSouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }

        // 회전 공격
        public void TriggerAttackSpinAnimation()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            // 회전 공격 애니메이션을 시작하기 위해 'isAttackSpinning'을 true로 설정
            animator.SetBool("isAttackSpinning", true);

            // 현재 방향을 결정하고 적절한 회전 공격 애니메이션 트리거
            if (animator.GetBool("isNorth")) animator.SetBool("AttackSpinNorth", true);
            else if (animator.GetBool("isSouth")) animator.SetBool("AttackSpinSouth", true);
            else if (animator.GetBool("isEast")) animator.SetBool("AttackSpinEast", true);
            else if (animator.GetBool("isWest")) animator.SetBool("AttackSpinWest", true);
            else if (animator.GetBool("isNorthEast")) animator.SetBool("AttackSpinNorthEast", true);
            else if (animator.GetBool("isNorthWest")) animator.SetBool("AttackSpinNorthWest", true);
            else if (animator.GetBool("isSouthEast")) animator.SetBool("AttackSpinSouthEast", true);
            else if (animator.GetBool("isSouthWest")) animator.SetBool("AttackSpinSouthWest", true);

            // 지연 후 또는 애니메이션 끝에서 회전 공격 매개변수들을 리셋
            StartCoroutine(ResetAttackSpinParameters());
        }

        IEnumerator ResetAttackSpinParameters()
        {
            // 리셋하기 전에 애니메이션 길이만큼 대기
            yield return new WaitForSeconds(0.5f); // 애니메이션 길이에 따라 대기 시간 조정

            // 모든 회전 공격 매개변수들을 false로 리셋
            animator.SetBool("isAttackSpinning", false);
            animator.SetBool("AttackSpinNorth", false);
            animator.SetBool("AttackSpinSouth", false);
            animator.SetBool("AttackSpinEast", false);
            animator.SetBool("AttackSpinWest", false);
            animator.SetBool("AttackSpinNorthEast", false);
            animator.SetBool("AttackSpinNorthWest", false);
            animator.SetBool("AttackSpinSouthEast", false);
            animator.SetBool("AttackSpinSouthWest", false);

            // 선택사항으로, 캐릭터가 올바른 유휴 상태로 돌아가도록 방향 복원
            RestoreDirectionAfterAttack();
        }


    }
}