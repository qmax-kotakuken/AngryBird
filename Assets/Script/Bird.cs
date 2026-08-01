using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Bird : MonoBehaviour
{
    private const int TrajectoryPointCount = 20;

    [Header("発射設定")]
    [SerializeField, Min(0.1f)] private float maxPullDistance = 1f;
    [SerializeField, Min(0.1f)] private float flyForce = 13f;
    [SerializeField, Min(0f)] private float minimumPullDistance = 0.05f;

    [Header("飛行軌跡")]
    [SerializeField] private GameObject dotPrefab;
    [SerializeField, Min(0.01f)] private float dotTimeInterval = 0.05f;

    [Header("生成・破棄時間")]
    [SerializeField, Min(0f)] private float nextBirdDelay = 5f;
    [SerializeField, Min(0f)] private float destroyDelayAfterCollision = 5f;

    private readonly GameObject[] trajectoryDots = new GameObject[TrajectoryPointCount];
    protected Rigidbody2D Body { get; private set; }

    private Camera mainCamera;
    private Vector2 startPosition;
    private bool hasLaunched;
    private bool hasCollided;
    private Coroutine nextBirdRoutine;
    private float dotTimer;
    private int nextDotIndex;

    protected virtual void Awake()
    {
        Body = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        startPosition = transform.position;
        Body.isKinematic = true;

        CreateTrajectoryDots();
    }

    protected virtual void Update()
    {
        if (!hasLaunched)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            ActivateAbility();
        }

        UpdateFlightTrail();
    }

    protected virtual void ActivateAbility()
    {
    }

    protected virtual void OnMouseDrag()
    {
        if (hasLaunched || mainCamera == null)
        {
            return;
        }

        Vector2 pointerPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 pullOffset = Vector2.ClampMagnitude(pointerPosition - startPosition, maxPullDistance);

        // スリングショットより後方へ引いた場合だけ発射できる。
        pullOffset.x = Mathf.Min(0f, pullOffset.x);
        transform.position = startPosition + pullOffset;

        UpdateLaunchPreview();
    }

    protected virtual void OnMouseUp()
    {
        if (hasLaunched)
        {
            return;
        }

        Vector2 pullVector = startPosition - (Vector2)transform.position;
        if (pullVector.magnitude < minimumPullDistance)
        {
            ResetAim();
            return;
        }

        hasLaunched = true;
        HideAllDots();
        nextDotIndex = 0;
        dotTimer = 0f;
        Body.isKinematic = false;
        Body.AddForce(pullVector * flyForce, ForceMode2D.Impulse);
        ShowNextTrailDot();
        nextBirdRoutine = StartCoroutine(RequestNextBirdAfterDelay());
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        hasCollided = true;

        Destroy(gameObject, destroyDelayAfterCollision);
    }

    protected virtual void OnDestroy()
    {
        // このオブジェクトが破棄されるとコルーチンも停止するため、
        // 待機時間中に場外へ落ちた場合でも次の鳥を生成して進行不能を防ぐ。
        if (hasLaunched && nextBirdRoutine != null && LevelManager.HasInstance)
        {
            LevelManager.Instance.RequestNextBird();
        }

        foreach (GameObject dot in trajectoryDots)
        {
            if (dot != null)
            {
                Destroy(dot);
            }
        }
    }

    private IEnumerator RequestNextBirdAfterDelay()
    {
        yield return new WaitForSeconds(nextBirdDelay);

        nextBirdRoutine = null;
        if (LevelManager.HasInstance)
        {
            LevelManager.Instance.RequestNextBird();
        }
    }

    private void CreateTrajectoryDots()
    {
        if (dotPrefab == null)
        {
            return;
        }

        for (int i = 0; i < trajectoryDots.Length; i++)
        {
            GameObject dot = Instantiate(dotPrefab);
            dot.transform.localScale *= 1f - (0.03f * i);
            dot.SetActive(false);
            trajectoryDots[i] = dot;
        }
    }

    private void UpdateFlightTrail()
    {
        if (hasCollided || nextDotIndex >= trajectoryDots.Length)
        {
            return;
        }

        dotTimer += Time.deltaTime;
        if (dotTimer < dotTimeInterval)
        {
            return;
        }

        dotTimer -= dotTimeInterval;
        ShowNextTrailDot();
    }

    private void ShowNextTrailDot()
    {
        if (nextDotIndex >= trajectoryDots.Length)
        {
            return;
        }

        GameObject dot = trajectoryDots[nextDotIndex];
        nextDotIndex++;

        if (dot == null)
        {
            return;
        }

        dot.transform.position = transform.position;
        dot.SetActive(true);
    }

    private void UpdateLaunchPreview()
    {
        Vector2 impulse = (startPosition - (Vector2)transform.position) * flyForce;
        Vector2 initialVelocity = impulse / Body.mass;
        Vector2 gravity = Physics2D.gravity * Body.gravityScale;

        for (int i = 0; i < trajectoryDots.Length; i++)
        {
            GameObject dot = trajectoryDots[i];
            if (dot == null)
            {
                continue;
            }

            float time = dotTimeInterval * (i + 1);
            dot.transform.position = (Vector2)transform.position
                + (initialVelocity * time)
                + (0.5f * gravity * time * time);
            dot.SetActive(true);
        }
    }

    private void HideAllDots()
    {
        foreach (GameObject dot in trajectoryDots)
        {
            if (dot != null)
            {
                dot.SetActive(false);
            }
        }
    }

    private void ResetAim()
    {
        transform.position = startPosition;
        HideAllDots();
    }
}
