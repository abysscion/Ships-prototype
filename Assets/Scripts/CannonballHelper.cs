using UnityEngine;

/// <summary>
/// Realizes cannonball collision logic and particles spawning
/// </summary>
public class CannonballHelper : MonoBehaviour
{
    public GameObject explosionParticle;
    public GameObject rippleParticle;
    public GameObject splashParticle;

    private bool _onceCollided;

    private void OnCollisionEnter(Collision collision)
    {
        if (_onceCollided)
            return;

        var collisionGO = collision.gameObject;
        var contactPoint = collision.contacts[0].point;

        if (collisionGO.CompareTag("Enemy") || collisionGO.CompareTag("Player"))
            SpawnParticle(explosionParticle, contactPoint);
        else if (collisionGO.CompareTag("Sea"))
        {
            SpawnParticle(splashParticle, contactPoint);
            SpawnParticle(rippleParticle, contactPoint);
        }

        _onceCollided = true;
        Destroy(this.gameObject);
    }

    private void SpawnParticle(GameObject particle, Vector3 spawnPoint)
    {
        if (spawnPoint.y < 0)
            spawnPoint.y = 0;

        var particleInstance = Instantiate(particle, spawnPoint, Quaternion.identity);
        var particleSystem = particle.GetComponent<ParticleSystem>();

        Destroy(particleInstance.gameObject, particleSystem.main.duration);
    }
}
