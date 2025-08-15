using UnityEngine;
using UnityEngine.UI;             // UI 요소용
using UnityEngine.SceneManagement; // 씬 리로드용
using System.Collections;
using TMPro;
using System.Collections.Generic;

namespace SmallScaleInc.CharacterCreatorModern
{
    public class PlayerController : MonoBehaviour
    {
        public AnimationController animationController;
        private CircleCollider2D circleCollider;
        public float speed = 1.0f; // 플레이어의 이동 속도
        private Rigidbody2D rb;
        public bool isMounted = false;
        private Vector2 movementDirection;
        private bool isOnStairs = false; // 계단에 있을 때 플레이어가 다른 각도로 이동
        public bool isCrouching = false; // 앉아있을 때 플레이어가 더 느리게 이동
        private SpriteRenderer spriteRenderer;
        private float lastAngle;  // 마지막으로 계산된 각도 저장
        private bool isRunning = false;
        private Color originalColor;

        // 상단에 다른 변수들과 함께 이 필드를 추가
        private AudioSource gunfireAudioSource;



        // 궁수 전용 설정
        public bool isActive; // 캐릭터가 활성 상태인지
        public bool isRanged; // 캐릭터가 궁수나 마법사인지
        public bool isStealth; // true일 경우 웅크릴 때 플레이어를 투명하게 만듦
        public bool isShapeShifter;
        public bool isSummoner;
        public GameObject projectilePrefab;
        public GameObject AoEPrefab;
        public GameObject Special1Prefab;
        public GameObject HookPrefab;
        public GameObject ShapeShiftPrefab;
        public float projectileSpeed = 10.0f;
        public float shootDelay = 0.5f;

        // 근접 전투 전용 설정
        public bool isMelee;
        public GameObject meleePrefab;

        // 데미지 및 발사 속도 설정
        [Header("데미지 설정")]
        public float bulletDamage = 1f;
        public float bulletsPerSecond = 3f;
        private float nextFireTime = 0f;

        [Header("선 렌더러 / 탄환 궤적")]
        public GameObject bulletLinePrefab;
        public float lineDisplayTime = 0.05f;

        [Header("발사 원점 오프셋")]
        public float muzzleForwardOffset = 0.5f;
        public float muzzleUpOffset = 0.2f;

        // -------------------- 체력 및 UI --------------------
        public int maxHealth = 100;
        public int currentHealth;
        public bool isDead = false;
        public Slider healthSlider;     // 인스펙터에서 UI 슬라이더를 할당하세요
        public GameObject gameOver;     // 인스펙터에서 GAMEOVER UI GameObject를 할당하세요

        // --- 점수 / 킬 카운트 ---
        public int killCount = 0;
        public TextMeshProUGUI killCountText; // 인스펙터에서 점수 UI 요소와 함께 할당하세요


        void Start()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animationController = GetComponent<AnimationController>();
            circleCollider = GetComponent<CircleCollider2D>();
            originalColor = spriteRenderer.color;
            // 체력 초기화
            currentHealth = maxHealth;
            if (healthSlider != null)
            {
                healthSlider.maxValue = maxHealth;
                healthSlider.value = currentHealth;
            }
            // 총성 AudioSource 설정
            gunfireAudioSource = GetComponent<AudioSource>();
            if (killCountText != null)
            {
                originalScale = killCountText.transform.localScale; // 원래 크기 저장
            }
        }

        void Update()
        {
            // 기존 이동 및 입력 코드...
            if(isDead)
            {
                return;
            }
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); //마우스 스크린 좌표를 월드좌표로 변환환
            Vector2 directionToMouse = (mousePosition - (Vector2)transform.position).normalized; //플레이어 위치에서 마우스위치까지의 방향벡터 정규화
            isMounted = animationController.isMounted; //애니메이션 컨트롤러에서 탑승 상태 확인
            float angle = Mathf.Atan2(directionToMouse.y, directionToMouse.x) * Mathf.Rad2Deg; //마우스 방향에 따른 각도 계산
            lastAngle = SnapAngleToEightDirections(angle); //8방향 각도로 스냅

            movementDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad)); //마지막 각도에 따른 이동 방향 벡터 계산

            // 이동, 데미지, 사격, 탐승처리 핸들러러
            HandleMovement();
            HandleDamage();
            HandleShooting(); // 사운드 재생용
            HandleRiding();

            bool isMoving = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) ||
                             Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D);//wasd 중 하나라도 눌려있으면 이동중

            if (isMoving && !isRunning) 
            {
                isRunning = true;
            }
            else if (!isMoving && isRunning)
            {
                isRunning = false;
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                if (isShapeShifter && isActive) //변신 캐릭터이고 활성화상태면면
                {
                    StartCoroutine(ShapeShiftDelayed());
                }
                HandleCrouching();
            }

            if (isActive)
            {
                // 누락된 프리팹들(projectile, AoE 등) 확인, 필요한 프리팹들이 하나라도 없으면 종료료
                if (projectilePrefab == null || AoEPrefab == null ||
                    Special1Prefab == null || HookPrefab == null)
                {
                    return;
                } 

                if (isRanged) //원거리 캐릭터터
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        Invoke(nameof(DelayedShoot), shootDelay);
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        StartCoroutine(DeploySpecial1Delayed());
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha3))
                    {
                        StartCoroutine(DeployAoEDelayed());
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        if (isSummoner)
                        {
                            StartCoroutine(DeployHookDelayed());
                        }
                        else
                        {
                            StartCoroutine(Quickshot());
                        }
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha6))
                    {
                        StartCoroutine(CircleShot());
                    }
                }

                if (isMelee) //근접 캐릭터
                {
                    if (Input.GetKeyDown(KeyCode.Alpha1))
                    {
                        StartCoroutine(DeployAoEDelayed());
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha5))
                    {
                        StartCoroutine(DeployHookDelayed());
                    }
                    if (Input.GetKeyDown(KeyCode.Alpha6))
                    {
                        Invoke(nameof(DelayedShoot), shootDelay);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.LeftControl) && isRunning) //캐릭터가 뛰고 있고 컨트롤키를 누르면
                {
                    if (isShapeShifter && isActive)
                    {
                        StartCoroutine(ShapeShiftDelayed());
                    }
                }
            }
        }

        void FixedUpdate()
        {
            if (movementDirection != Vector2.zero) //이동방향이 0이 아니면 이동동
            {
                rb.MovePosition(rb.position + movementDirection * speed * Time.fixedDeltaTime);
            }
        }

        private void HandleShooting()
        {
            if (Input.GetMouseButtonDown(1)) // 마우스 우클릭 시
            {
                PlayGunfireSound(); // 겹쳐서 재생 가능한 총성 사운드 재생
            }
        }

        private void PlayGunfireSound()
        {
            // // 사운드용 새 GameObject 생성하고 AudioSource 추가
            // GameObject gunfireSoundObject = new GameObject("GunfireSound");
            // AudioSource newGunfireSource = gunfireSoundObject.AddComponent<AudioSource>();

            // // AudioSource 속성 설정
            // newGunfireSource.clip = gunfireAudioSource.clip; // 기존 총성 사운드 클립 사용
            // newGunfireSource.outputAudioMixerGroup = gunfireAudioSource.outputAudioMixerGroup; // 믹서 설정 유지
            // newGunfireSource.volume = gunfireAudioSource.volume;
            // newGunfireSource.spatialBlend = 0; // 2D 사운드의 경우 0으로 설정 (3D 사운드 사용 시 적절히 조정)

            // // 사운드 재생
            // newGunfireSource.Play();

            // // 클립 재생 완료 후 사운드 객체 제거
            // Destroy(gunfireSoundObject, newGunfireSource.clip.length);
        }

        /// <summary>
        /// 단발 사격 시 총성 사운드가 자연스럽게 끝날 수 있도록 합니다.
        /// 버튼을 누르고 있으면 반복 재생됩니다.
        /// </summary>
        private IEnumerator StopGunfireSoundAfterDelay()
        {
            yield return new WaitForSeconds(0.25f); // 자연스러운 페이드아웃을 위한 짧은 지연
            if (!Input.GetMouseButton(1)) // 버튼이 여전히 눌려있는지 확인
            {
                gunfireAudioSource.Stop();
            }
        }

        /// <summary>
        /// 플레이어의 체력을 감소시키고, UI 슬라이더를 업데이트하며, 사망을 확인합니다.
        /// </summary>
        public void TakeDamage(int damageAmount)
        {
            if (isDead) return; // 이미 죽어있으면 데미지 무시

            currentHealth -= damageAmount;
            // Debug.Log($"Player took {damageAmount} damage. Current Health: {currentHealth}");

            if (healthSlider != null)
            {
                healthSlider.value = currentHealth;
            }

            if (currentHealth <= 0)
            {
                Die();
            }
            else
            {
                animationController.TriggerTakeDamageAnimation();
            }
        }

        /// <summary>
        /// 플레이어의 체력이 0이 될 때 호출됩니다. 죽음 애니메이션을 재생하고,
        /// 이동을 비활성화하며, GAMEOVER 화면을 표시하고, 지연 후 씬을 다시 시작합니다.
        /// </summary>
        private void Die()
        {
            // Debug.Log("Player died!");
            isDead = true;

            if (circleCollider != null)
            {
                circleCollider.enabled = false;
            }

            animationController.TriggerDie();

            // 플레이어 몸체가 밀려나지 않도록 방지
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Static; // 몸체를 완전히 움직이지 않게 만듦
            }

            // GAMEOVER UI를 3초간 표시
            if (gameOver != null)
            {
                gameOver.SetActive(true);
            }
            StartCoroutine(RestartSceneAfterDelay(3f));
        }


        /// <summary>
        /// 지정된 지연 시간을 기다린 후 현재 씬을 다시 시작합니다.
        /// </summary>
        /// <param name="delay">다시 시작하기 전 지연 시간(초)</param>
        private IEnumerator RestartSceneAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }


     // ---------------------------------------------------------------
        // 새로 추가: 마우스 우클릭 시 마우스 아래 객체에 데미지 주기
        // ---------------------------------------------------------------
        private Coroutine pulseCoroutine; 
        private Vector3 originalScale; // 게임 시작 시 원래 크기 저장


public void IncrementKillCount()
{
    killCount++;
    if (killCountText != null)
    {
        killCountText.text = killCount.ToString();

        // 새로운 펄스 효과 시작 전 진행 중인 효과 중지
        if (pulseCoroutine != null)
        {
            StopCoroutine(pulseCoroutine);
        }

        // 새 펄스 효과 시작
        pulseCoroutine = StartCoroutine(PulseTextEffect(killCountText));
    }
}

private IEnumerator PulseTextEffect(TextMeshProUGUI text)
{
    float duration = 0.2f; // 전체 펄스 지속 시간
    float maxScaleFactor = 1.5f; // 얼마나 크게 만들지
    float time = 0f;

    Vector3 maxScale = originalScale * maxScaleFactor; // 목표 크기 계산

    // 텍스트 확대
    while (time < duration / 2)
    {
        text.transform.localScale = Vector3.Lerp(text.transform.localScale, maxScale, time / (duration / 2));
        time += Time.deltaTime;
        yield return null;
    }
    text.transform.localScale = maxScale;
    time = 0f;

    // 원래 크기로 축소
    while (time < duration / 2)
    {
        text.transform.localScale = Vector3.Lerp(text.transform.localScale, originalScale, time / (duration / 2));
        time += Time.deltaTime;
        yield return null;
    }

    text.transform.localScale = originalScale; // 최종 리셋 보장
    pulseCoroutine = null;
}
private void HandleDamage()
{
    if (Input.GetMouseButton(1))
    {
        float timeBetweenShots = 1f / bulletsPerSecond; // 발사 속도 제어
        if (Time.time >= nextFireTime)
        {
            speed = 0.5f;
            nextFireTime = Time.time + timeBetweenShots;

            Vector2 playerPos = transform.position;
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mousePos - playerPos).normalized;
            Vector2 muzzleOrigin = playerPos;
            float maxDistance = 10f;
            if (isMelee)
            {
                maxDistance = 1f; 
            }

            // 계속 히트하는 동안 레이캐스트 지속
            Vector2 rayOrigin = muzzleOrigin;
            bool shouldContinue = true;
            List<Vector2> hitPoints = new List<Vector2> { muzzleOrigin }; // 트레이서용 히트 포인트 저장

            if(isMelee == false)
            {
                // 전체 탄환 경로에 대한 트레이서 선 표시
                StartCoroutine(ShowShotLine(hitPoints));
            }
        }
    }
    else
    {
        speed = 1.0f;
        nextFireTime = 0f;
    }
}

// -------------------------------------------------------------
// 코루틴: 선 프리팹을 인스턴스화하고 탄환 경로를 표시
// -------------------------------------------------------------
private IEnumerator ShowShotLine(List<Vector2> hitPoints)
{
    GameObject lineObj = Instantiate(bulletLinePrefab, Vector3.zero, Quaternion.identity);
    LineRenderer lr = lineObj.GetComponent<LineRenderer>();

    lr.positionCount = hitPoints.Count;
    for (int i = 0; i < hitPoints.Count; i++)
    {
        lr.SetPosition(i, hitPoints[i]);
    }

    yield return new WaitForSeconds(lineDisplayTime);
    Destroy(lineObj);
}

float SnapAngleToEightDirections(float angle)
{
    angle = (angle + 360) % 360; // 각도를 [0..360)으로 정규화

    if (isOnStairs)
    {
        // -- 특별한 "계단" 각도가 있다면, 마찬가지로 조정.
        //    (아래는 어떻게 할 수 있는지 예시입니다.)
        if (angle < 30 || angle >= 330)
            return 0;
        else if (angle >= 30 && angle < 75)
            return 60;
        else if (angle >= 75 && angle < 105)
            return 90;
        else if (angle >= 105 && angle < 150)
            return 120;
        else if (angle >= 150 && angle < 210)
            return 180;
        else if (angle >= 210 && angle < 255)
            return 240;
        else if (angle >= 255 && angle < 285)
            return 270;
        else if (angle >= 285 && angle < 330)
            return 300;
    }
    else
    {
        // -- 일반적인 아이소메트릭 8방향
        //    대각선이 30°, 150°, 210°, 330°에 오도록 조정.
        if (angle < 15 || angle >= 345)
            return 0;    // 동쪽
        else if (angle >= 15 && angle < 75)
            return 30;   // 북동
        else if (angle >= 75 && angle < 105)
            return 90;   // 북쪽
        else if (angle >= 105 && angle < 165)
            return 150;  // 북서
        else if (angle >= 165 && angle < 195)
            return 180;  // 서쪽
        else if (angle >= 195 && angle < 255)
            return 210;  // 남서
        else if (angle >= 255 && angle < 285)
            return 270;  // 남쪽
        else if (angle >= 285 && angle < 345)
            return 330;  // 남동
    }

    return 0;
}


        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.tag == "Stairs")
            {
                isOnStairs = true;
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.tag == "Stairs")
            {
                isOnStairs = false;
            }
        }

        float GetPerpendicularAngle(float angle, bool isLeft)
        {
            // 기본 수직 각도 계산 (90도 오프셋)
            float perpendicularAngle = isLeft ? angle - 90 : angle + 90;
            perpendicularAngle = (perpendicularAngle + 360) % 360; // 각도 정규화

            // SnapAngleToEightDirections 함수를 사용하여 가장 가까운 유효한 각도로 스냅
            return SnapAngleToEightDirections(perpendicularAngle);
        }

        void HandleMovement()
        {
            if (Input.GetKey(KeyCode.W))
            {
                return;
            }
            else if (!isCrouching || !isMounted) // 웅크리지 않았거나 탑승하지 않았을 때만 스트레이핑 허용
            {
                if (Input.GetKey(KeyCode.S))
                {
                    movementDirection = -movementDirection; // 뒤로 이동
                }

                else if (Input.GetKey(KeyCode.A))
                {
                    float leftAngle = GetPerpendicularAngle(Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg, true);
                    movementDirection = new Vector2(Mathf.Cos(leftAngle * Mathf.Deg2Rad), Mathf.Sin(leftAngle * Mathf.Deg2Rad));

                }
                else if (Input.GetKey(KeyCode.D))
                {

                    float rightAngle = GetPerpendicularAngle(Mathf.Atan2(movementDirection.y, movementDirection.x) * Mathf.Rad2Deg, false);
                    movementDirection = new Vector2(Mathf.Cos(rightAngle * Mathf.Deg2Rad), Mathf.Sin(rightAngle * Mathf.Deg2Rad));
                }
                else
                {
                    movementDirection = Vector2.zero; // 이동 입력 없음
                }
            }
            else
            {
                movementDirection = Vector2.zero; // 이동 입력 없음
            }
        }

        void HandleCrouching()
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                isCrouching = !isCrouching; // 웅크리기 토글
                // speed = isCrouching ? 1.0f : 2.0f; // 웅크린 상태에 따라 속도 조정 (필요시)

                if (isCrouching && isStealth)
                {
                    // 색상을 어두운 회색으로 설정하고 투명도를 50%로 감소
                    spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                }

            }
        }

        void HandleRiding()
        {
                speed = isMounted ? 2.0f : 1.0f; // 탑승 상태에 따라 속도 조정
        }

        // 원거리 캐릭터 전용 메서드:

        public void SetArcherStatus(bool status)
        {
            isRanged = status;
        }

        public void SetMeleeStatus(bool status)
        {
            isMelee = status;
        }

        public void SetActiveStatus(bool status)
        {
            isActive = status;
        }

        void DelayedShoot()
        {
            Vector2 fireDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
            ShootProjectile(fireDirection);
        }

        void ShootProjectile(Vector2 direction)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            GameObject projectileInstance = Instantiate(projectilePrefab, transform.position, Quaternion.Euler(0, 0, angle));
            Rigidbody2D rbProjectile = projectileInstance.GetComponent<Rigidbody2D>();
            if (rbProjectile != null)
            {
                rbProjectile.linearVelocity = direction * projectileSpeed;
            }
            // 인스턴스화된 프리팹을 1.5초 후에 제거
            Destroy(projectileInstance, 1.5f);
        }

        IEnumerator Quickshot()
        {
            // 퀵샷 시퀀스 시작 전 초기 짧은 지연
            yield return new WaitForSeconds(0.1f);

            // 바라보는 방향으로 5개의 투사체 발사
            for (int i = 0; i < 5; i++)
            {
                Vector2 fireDirection = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                ShootProjectile(fireDirection);

                // 다음 투사체 발사 전 0.18초 대기
                yield return new WaitForSeconds(0.18f);
            }
        }

        IEnumerator CircleShot()
        {
            float initialDelay = 0.1f;
            float timeBetweenShots = 0.9f / 8;  // 전체 시간을 샷 수로 나눔

            yield return new WaitForSeconds(initialDelay);

            // lastAngle을 시작 각도로 사용하고 8방향으로 투사체 생성
            for (int i = 0; i < 8; i++)
            {
                float angle = lastAngle + i * 45;  // 각 방향마다 45도씩 증가
                angle = Mathf.Deg2Rad * angle;  // 방향 계산을 위해 라디안으로 변환
                Vector2 fireDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                ShootProjectile(fireDirection);

                yield return new WaitForSeconds(timeBetweenShots);
            }
        }

        IEnumerator DeployAoEDelayed()
        {
            if (AoEPrefab != null)
            {
                GameObject aoeInstance; // 나중에 제거할 수 있도록 외부에 선언

                if (isSummoner)
                {
                    // 마우스 위치를 가져와서 월드 좌표로 변환
                    Vector3 mouseScreenPosition = Input.mousePosition;
                    mouseScreenPosition.z = Camera.main.nearClipPlane; // 카메라의 near clip plane으로 설정
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

                    yield return new WaitForSeconds(0.3f); // 인스턴스화 전 대기 (필요에 따라 시간 조정)
                    // AoE 프리팹을 마우스의 월드 위치에 인스턴스화
                    aoeInstance = Instantiate(AoEPrefab, new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0), Quaternion.identity);

                    Destroy(aoeInstance, 8.7f);
                }
                else
                {
                    if(isMelee)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                    else if(isShapeShifter)
                    {
                        yield return new WaitForSeconds(0.2f);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.3f);
                    }
                    // AoE 프리팹을 플레이어 위치에 인스턴스화
                    aoeInstance = Instantiate(AoEPrefab, transform.position, Quaternion.identity);
                    Destroy(aoeInstance, 0.9f);
                }

                // AoE 인스턴스를 0.9초 후에 제거
                
            }
        }


        IEnumerator ShapeShiftDelayed()
        {
            if (ShapeShiftPrefab != null)
            {

                yield return new WaitForSeconds(0.001f);
                
                // ShapeShift 프리팹을 플레이어 위치에 인스턴스화
                GameObject shapeShiftInstance = Instantiate(ShapeShiftPrefab, transform.position, Quaternion.identity);

                
                // 인스턴스화된 프리팹을 0.5초 후에 제거
                Destroy(shapeShiftInstance, 0.9f);
            }
        }
        IEnumerator DeploySpecial1Delayed()
        {
            if (Special1Prefab != null)
            {
                GameObject Special1PrefabInstance; // 나중에 제거할 수 있도록 외부에 선언

                if (isSummoner)
                {
                    // 마우스 위치를 가져와서 월드 좌표로 변환
                    Vector3 mouseScreenPosition = Input.mousePosition;
                    mouseScreenPosition.z = Camera.main.nearClipPlane; // 카메라의 near clip plane으로 설정
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

                    yield return new WaitForSeconds(0.6f); // 인스턴스화 전 대기 (필요에 따라 시간 조정)
                    // Special1 프리팹을 마우스의 월드 위치에 인스턴스화
                    Special1PrefabInstance = Instantiate(Special1Prefab, new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0), Quaternion.identity);
                }
                else
                {
                    if(isMelee)
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                    else
                    {
                        yield return new WaitForSeconds(0.6f);
                    }
                    // Special1 프리팹을 플레이어 위치에 인스턴스화
                    Special1PrefabInstance = Instantiate(Special1Prefab, transform.position, Quaternion.identity);
                }

                // Special1 인스턴스를 1.0초 후에 제거
                Destroy(Special1PrefabInstance, 1.0f);
            }
        }

        IEnumerator DeployHookDelayed()
        {
            GameObject hookInstance;
            if (isSummoner)
                {
                    // 마우스 위치를 가져와서 월드 좌표로 변환
                    Vector3 mouseScreenPosition = Input.mousePosition;
                    mouseScreenPosition.z = Camera.main.nearClipPlane; // 카메라의 near clip plane으로 설정
                    Vector3 mouseWorldPosition = Camera.main.ScreenToWorldPoint(mouseScreenPosition);

                    yield return new WaitForSeconds(0.6f); // 인스턴스화 전 대기 (필요에 따라 시간 조정)
                    // Hook 프리팹을 마우스의 월드 위치에 인스턴스화
                    hookInstance = Instantiate(HookPrefab, new Vector3(mouseWorldPosition.x, mouseWorldPosition.y, 0), Quaternion.identity);

                    Destroy(hookInstance, 5.2f);
                }
                else
                {
                    if (HookPrefab != null)
                    {
                        Vector2 direction = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
                        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                        hookInstance = Instantiate(HookPrefab, transform.position, Quaternion.Euler(0, 0, angle));
                        // 인스턴스화된 프리팹을 1.0초 후에 제거
                        Destroy(hookInstance, 1.0f);
                    }
                    yield return null; // 메서드가 IEnumerator를 올바르게 구현하도록 보장
                }
        }

        public void FlashGreen()
        {
            StartCoroutine(FlashEffect());
        }

        private IEnumerator FlashEffect()
        {
            spriteRenderer.color = Color.green; // 녹색으로 변경
            yield return new WaitForSeconds(0.7f); // 0.2초 대기
            spriteRenderer.color = originalColor; // 원래 색상으로 복원
        }


        // 근접 공격 메서드
        // void MeleeAttack()
        // {
        //     if (meleePrefab != null)
        //     {
        //         StartCoroutine(DelayedMeleeAttack());
        //     }
        // }

        // IEnumerator DelayedMeleeAttack()
        // {
        //     // 근접 공격 시작 전 0.5초 대기
        //     yield return new WaitForSeconds(0.5f);

        //     Vector2 direction = new Vector2(Mathf.Cos(lastAngle * Mathf.Deg2Rad), Mathf.Sin(lastAngle * Mathf.Deg2Rad));
        //     // 근접 공격용 회전 각도 계산
        //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        //     // 근접 공격 프리팹을 플레이어 위치에 인스턴스화
        //     GameObject meleeInstance = Instantiate(meleePrefab, transform.position, Quaternion.Euler(0, 0, angle));

        //     // 인스턴스화된 근접 공격 프리팹을 플레이어의 자식으로 설정
        //     meleeInstance.transform.SetParent(transform);

        //     // 선택사항: 짧은 지속시간 후 근접 공격 프리팹 제거
        //     Destroy(meleeInstance, 0.1f); // 필요에 따라 지속시간 조정
        // }

    }
}