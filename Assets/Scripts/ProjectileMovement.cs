using UnityEngine;

public class ProjectileMovement : MonoBehaviour
{

    public float projectileSpeed = 100f;

    void Start(){
        Destroy(gameObject, 10f);
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<Rigidbody>().linearVelocity = transform.forward * projectileSpeed;
    }
}
