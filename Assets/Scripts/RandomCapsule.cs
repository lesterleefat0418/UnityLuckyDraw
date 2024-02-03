using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomCapsule : MonoBehaviour
{
    public Sprite[] capsuleTopSprites;
    public Sprite[] capsuleBottomSprites;
    public int randomId;
    public SpriteRenderer[] capsuleSprite;

    // Start is called before the first frame update
    void Start()
    {
        this.randomId = Random.Range(0, capsuleTopSprites.Length);

        if (capsuleSprite[0] != null && capsuleSprite[1] != null)
        {
            capsuleSprite[0].sprite = capsuleTopSprites[this.randomId];
            capsuleSprite[1].sprite = capsuleBottomSprites[this.randomId];

        }
    }

}
