using Unity.Netcode;
using UnityEngine;

public class Stats : NetworkBehaviour
{
    public int maxHealth = 100;
    public int maxMana = 100;
    public int maxStamina = 100;

    public int curHealth;

    void Start(){
        curHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        curHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Remaining: {curHealth}");

        if (curHealth <= 0)
        {
            Debug.Log("We need to handle the death here");
            // Destroy(gameObject); // or trigger networked despawn
        }
    }
}
