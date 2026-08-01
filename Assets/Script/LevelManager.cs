using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }
    public static bool HasInstance => Instance != null;

    [Header("鳥の生成設定")]
    [Tooltip("上から順番に生成する鳥のプレハブを設定します。")]
    [SerializeField] private List<GameObject> birdOrder = new List<GameObject>();

    [Header("画面表示")]
    [SerializeField] private GameObject clearDialog;

    private readonly HashSet<Enemy> enemies = new HashSet<Enemy>();
    private int nextBirdIndex;
    private bool levelCompleted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Time.timeScale = 1f;

        if (clearDialog != null)
        {
            clearDialog.SetActive(false);
        }
    }

    private void Start()
    {
        SpawnNextBird();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    internal void RegisterEnemy(Enemy enemy)
    {
        if (enemy != null)
        {
            enemies.Add(enemy);
        }
    }

    internal void UnregisterEnemy(Enemy enemy)
    {
        if (enemy == null || !enemies.Remove(enemy) || enemies.Count != 0)
        {
            return;
        }

        CompleteLevel();
    }

    internal void RequestNextBird()
    {
        if (levelCompleted)
        {
            return;
        }

        SpawnNextBird();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void SpawnNextBird()
    {
        while (nextBirdIndex < birdOrder.Count)
        {
            GameObject birdPrefab = birdOrder[nextBirdIndex];
            nextBirdIndex++;

            if (birdPrefab == null)
            {
                Debug.LogError($"鳥の生成順の{nextBirdIndex}番目にプレハブが設定されていません。", this);
                continue;
            }

            Instantiate(birdPrefab, transform.position, Quaternion.identity);
            return;
        }

        Debug.Log("すべての鳥を発射しました。", this);
    }

    private void CompleteLevel()
    {
        levelCompleted = true;
        if (clearDialog != null)
        {
            clearDialog.SetActive(true);
        }

        Time.timeScale = 0f;
    }
}
