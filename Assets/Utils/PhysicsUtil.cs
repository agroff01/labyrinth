using UnityEngine;
using UnityEngine.Rendering.Universal;


public static class PhysicsUtil
{

    public static GameObject GetRootPhysicsObject(this Collider collider)
    {
        if (collider == null)
        {
            return null;
        }

        return collider.attachedRigidbody != null ? collider.attachedRigidbody.gameObject : collider.gameObject;
    }

    public static bool TryGetRigidbody(this Collider collider, out Rigidbody rigidbody)
    {
        rigidbody = null;
        if (collider != null) rigidbody = collider.attachedRigidbody;
        return rigidbody != null;
    }
    
    public static Vector3 GetRandomPointInside(this BoxCollider c)
    {
        // Get the world-space bounds of the collider
        var boxExtents = c.size * .5f;

        // Generate random local coordinates within the collider's extents
        float randomX = Random.Range(-boxExtents.x, boxExtents.x);
        float randomY = Random.Range(-boxExtents.y, boxExtents.y);
        float randomZ = Random.Range(-boxExtents.z, boxExtents.z);

        // Convert the local random point to a world-space point relative to the collider's center
        Vector3 randomLocalPoint = new Vector3(randomX, randomY, randomZ);
        Vector3 worldPoint = c.transform.TransformPoint(c.center + randomLocalPoint);

        return worldPoint;
    }
}

