using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance { get; private set; }

    public GameObject CharacterPrefab;

    public GameObject ClearDialog;

    public int EnemyCount { get; private set; } = 0;

    private void Awake()
    {
        instance = this;
        Instantiate(CharacterPrefab,transform.position,Quaternion.identity);
    }

    public void NextCharacter()
    {
        Instantiate(CharacterPrefab,transform.position, Quaternion.identity);
    }

    public void AddEnemyCount()
    {
        EnemyCount++;
    }

    public void EnemyDie()
    { 
        EnemyCount--;
        if (EnemyCount == 0)
        {
            ClearDialog.SetActive(true);
            Time.timeScale = 0;
        }
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
    }
}
