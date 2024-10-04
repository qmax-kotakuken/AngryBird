using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bird : MonoBehaviour
{
    private Vector2 startPosition;
    private bool isFly = false;
    private GameObject[] dotObjects = new GameObject[20];

    private float effectVelocity = 20;

    public float maxPullDistance = 1;
    public float flyForce = 10;

    public GameObject dotPrefab;
    public float dotTimeInterval = 0.05f;

    public ParticleSystem collsionEffect;

    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Rigidbody2D>().isKinematic = true;
        startPosition = transform.position;

        for (int i = 0; i < dotObjects.Length; i++)
        {
            dotObjects[i]=Instantiate(dotPrefab);
            dotObjects[i].transform.localScale = dotObjects[i].transform.localScale * (1 - 0.03f * i);
            dotObjects[i].transform.parent = transform;
            dotObjects[i].SetActive(false);
        }
    }

    private void OnMouseDrag()
    {
        if (isFly) { return; }
        Vector2 Pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (Vector2.Distance(startPosition, Pos) > maxPullDistance) 
        {
            Pos = (Pos - startPosition).normalized * maxPullDistance + startPosition;
        }

        if (Pos.x > startPosition.x) 
        {
            Pos.x = startPosition.x;
        }

        transform.position = Pos;

        UpdateDot();
    }

    private void OnMouseUp()
    {
        if (isFly) {return; }
        
        var Force = (startPosition - (Vector2)transform.position) * flyForce;

        var Rigidbody2D = GetComponent<Rigidbody2D>();
        Rigidbody2D.isKinematic = false;
        Rigidbody2D.AddForce(Force, ForceMode2D.Impulse);

        Invoke("NextCharacter", 5);

        for (int i = 0; i < dotObjects.Length; i++)
        {
            dotObjects[i].SetActive(false);
        }

        isFly = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.relativeVelocity.sqrMagnitude > effectVelocity)
        {
            PlayOneShot();
        }
        Destroy(gameObject, 5);
    }

    private void NextCharacter()
    {
        LevelManager.instance.NextCharacter();
    }

    private void UpdateDot()
    {
        var force = (startPosition - (Vector2)transform.position) * flyForce;
        var currentTime =dotTimeInterval;
        for (int i = 0; i < dotObjects.Length; i++)
        {
            dotObjects[i].SetActive(true);
            var pos =new Vector2();
            pos.x = (transform.position.x + force.x * currentTime);
            pos.y = (transform.position.y + force.y * currentTime) - (Physics2D.gravity.magnitude * currentTime * currentTime) / 2;

            dotObjects[i].transform.position = pos;
            currentTime += dotTimeInterval;
        }
    }

    
    public void PlayOneShot()
    {
        if (collsionEffect != null && !collsionEffect.isPlaying)
        {
            collsionEffect.Play();
        }
    }
}
