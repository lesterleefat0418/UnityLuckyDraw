using UnityEngine;

public class AddforceToCollider : MonoBehaviour
{
    public GameObject colliderObj = null;
    public ForeceDirection foreceDirection = ForeceDirection.Up;
    public float min_force = 60f;
    public float max_force = 100f;

    public enum ForeceDirection { 
        Up,
        Down,
        Left,
        Right
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }


    Vector2 ForceDirection
    {
        get
        {
            switch (foreceDirection)
            {
                case ForeceDirection.Up:
                    return Vector2.up;
                case ForeceDirection.Down:
                    return Vector2.down;
                case ForeceDirection.Left:
                    return Vector2.left;
                case ForeceDirection.Right:
                    return Vector2.right;
                default:
                    return Vector2.up;
            }
        }
    }

    void OnCollisionEnter2D (Collision2D col)
    {
        if (col.gameObject != null && GameController.Instance.allowDraw)
        {
            Debug.Log("Collision!");
            colliderObj = col.gameObject;
            colliderObj.GetComponent<ColliderAutoForce>().AddForce(ForceDirection, min_force, max_force);
        }
    }
}
