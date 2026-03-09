namespace CraftKill.Classes;

using UnityEngine;

public class BlockInfo : MonoBehaviour
{
    public Block block;
}

public class BlockInfoPickup : MonoBehaviour
{
    public Block block;
    public Rigidbody rb;

    float livingTime = 0f;

    public void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.velocity = Vector3.up * 8f;
        rb.velocity += Vector3.right * Random.Range(-5f, 5f);
        rb.velocity += Vector3.forward * Random.Range(-5f, 5f);
    }

    public void Update()
    {
        livingTime += Time.deltaTime;
        transform.Rotate(0, 360 * Time.deltaTime, 0);

        if (livingTime > 120)
        {
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            CubePlacer.instance.GiveBlock(block, 1);
            Destroy(gameObject);
        }
    }

    public void FixedUpdate()
    {
        var v = rb.velocity;
        rb.velocity = new Vector3(clamp(v.x), clamp(v.y), clamp(v.z));
    }

    public float clamp(float v)
    {
        return Mathf.Clamp(v, -50, 50);
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
