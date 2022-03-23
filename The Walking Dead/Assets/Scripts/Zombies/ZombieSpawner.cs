using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZombieSpawner : MonoBehaviour
{
    [Header("References")]
    public GameObject ZombiePrefab;
    public Path Path;

    [Header("Spawn Values")]
    public int ZombieCount;
    public float SpawnRadius;

    void Start()
    {
        for (int i = 0; i < ZombieCount; i++)
        {
            Vector2 random = Random.insideUnitCircle * Random.Range(0, SpawnRadius);
            
            Zombie zombie = Instantiate(ZombiePrefab).GetComponent<Zombie>();
            zombie.transform.position = transform.position + new Vector3(random.x, 1, random.y);
            zombie.transform.SetParent(transform.parent);

            zombie.path = Path;
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        if (Path) Gizmos.color = Color.red;
        else Gizmos.color = Color.yellow;

        Gizmos.DrawCube(transform.position + Vector3.up, new Vector3(1, 2, 1));
        Gizmos.DrawWireSphere(transform.position, SpawnRadius);
    }
}
