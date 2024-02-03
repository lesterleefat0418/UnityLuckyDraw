using UnityEngine;
using DG.Tweening;

public class ColliderAutoForce : MonoBehaviour
{
    public float min_force = 60f;
    public float max_force = 100f;
    public float stopThreshold = 0.1f;
    private Rigidbody2D rg = null;
    public int addForceTime = 0;
    public int addForceStep = 1;
    public Ease easeType = Ease.Linear;
    private AudioSource effect = null;
    public bool playSoundEffect = true;
    //public float magnitude;
    // Start is called before the first frame update
    void Start()
    {
        this.rg = GetComponent<Rigidbody2D>();
        this.effect = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown("s"))
        {
            AddForce(Vector2.up, min_force, max_force);
        }*/

        /*this.magnitude = rg.velocity.magnitude;
        if (this.magnitude < stopThreshold)
        {
            // Object has stopped moving
            Debug.Log("Object has stopped moving.");
            rg.bodyType = RigidbodyType2D.Static;
        }*/
    }

    public void AddForce(Vector2 force, float minForce, float maxForce)
    {
        if(rg != null && addForceTime < 10) { 
            Debug.Log("Add Force!");
            //rg.bodyType = RigidbodyType2D.Dynamic;
            addForceTime += addForceStep;
            rg.AddForce(force * addForceTime * 10f * Random.Range(minForce, maxForce), ForceMode2D.Impulse);
        }
        else
        {
            rg.AddForce(force * 100f * Random.Range(min_force, max_force), ForceMode2D.Impulse);
        }

        if(playSoundEffect && this.effect != null && !this.effect.isPlaying) 
            this.effect.Play();
    }

    public void AddForceWithDoTween(Vector2 force, float minForce, float maxForce)
    {
        if (rg != null)
        {
            float startForce = Random.Range(minForce, maxForce);
            float endForce = Random.Range(minForce, maxForce);

            DOTween.To(() => startForce, x => startForce = x, endForce, 1f)
                .SetEase(this.easeType) // Apply ease to the animation
                .OnUpdate(() =>
                {
                    rg.AddForce(force * 100f * startForce, ForceMode2D.Impulse);
                })
                .OnComplete(() =>
                {
                    Debug.Log("Force animation completed!");
                });
        }
    }
}

