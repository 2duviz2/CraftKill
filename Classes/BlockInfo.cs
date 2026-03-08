namespace CraftKill.Classes;

using UnityEngine;

public class BlockInfo : MonoBehaviour
{
    public Block block;
}

public class BlockInfoPickup : MonoBehaviour
{
    public Block block;

    public void Update()
    {
        transform.Rotate(0, 360 * Time.deltaTime, 0);
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            CubePlacer.instance.GiveBlock(block, 1);
            Destroy(gameObject);
        }
    }
}

public class CubeParticle : MonoBehaviour
{
    public float time = 2f;

    public void Update()
    {
        time -= Time.deltaTime;
        if (time <= 0) Destroy(gameObject);
    }
}
