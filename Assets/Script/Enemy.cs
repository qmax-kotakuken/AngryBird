using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float dieVelocity = 12.5f;

    // Start is called before the first frame update
    void Start()
    {
        LevelManager.instance.AddEnemyCount();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.relativeVelocity.sqrMagnitude > dieVelocity)
        {
            Destroy(gameObject);
            LevelManager.instance.EnemyDie();
        }
    }
}
