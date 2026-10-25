using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(Rigidbody))]
public class BoidAgent : MonoBehaviour
{
    [Header("Movement")] 
    [SerializeField] private float maxSpeed = 7f;
    [SerializeField] private float accelerationForce = 18f;
    [SerializeField] private float rotationSharpness = 10f;

    [Header("Behavior Distances")] 
    [SerializeField] private float separationDistance = 1.6f;
    [SerializeField] private float alignmentDistance = 3.5f;
    [SerializeField] private float cohesionDistance = 4.5f;

    [Header("Behavior Weights")] 
    [SerializeField] private float separationWeight = 1.6f;
    [SerializeField] private float alignmentWeight = 1f;
    [SerializeField] private float cohesionWeight = 1.2f;
    [SerializeField] private float boundsWeight = 2.5f;
    [SerializeField] private float obstacleWeight = 3f;

    [Header("Neighbor Query")] 
    [SerializeField] private LayerMask neighborMask = ~0;
    [SerializeField, Min(8)] private int maxNeighborColliderCount = 128;

    [Header("Obstacle Avoidance")] 
    [SerializeField] private bool avoidObstacle = true;
    [SerializeField] private LayerMask obstacleMask = ~0;
    [SerializeField, Min(0.1f)] private float obstacleProbeRadius = 0.45f;
    [SerializeField, Min(0.1f)] private float obstacleLookAhead = 3.5f;
    [SerializeField, Min(0.1f)] private float floorClearance = 0.8f;

    [Header("Vertical Constraint")] 
    [SerializeField] private bool shouldConstraintHeight;
    [SerializeField] private float constraintHeight = 4f;

    private Rigidbody rb;
    private readonly List<BoidAgent> neighbors = new List<BoidAgent>(32);
    private Collider[] neighborHits; // Raycast from BoidAgent to Neighbors, having Collider Array, can allow a NonAlloc method to be used.

    private Vector3 boundCenter;
    private float boundRadius = 25f;
    private bool useBounds = true; // If False, Boid will navigate forever

    private float separationDistanceSqr;
    private float alignmentDistanceSqr;
    private float cohesionDistanceSqr;
    private float neighborScanRadius;

    public Vector3 Velocity => rb ? rb.linearVelocity : Vector3.zero;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        neighborHits = new Collider[maxNeighborColliderCount];

        separationDistanceSqr = separationDistance * separationDistance;
        alignmentDistanceSqr = alignmentDistance* alignmentDistance;
        cohesionDistanceSqr = cohesionDistance * cohesionDistance;
        neighborScanRadius = Mathf.Max(separationDistance, Mathf.Max(alignmentDistance, cohesionDistance));
    }

    private void Start()
    {
        if (rb.linearVelocity.sqrMagnitude < 0.01f)
        {
            rb.linearVelocity = Random.onUnitSphere * maxSpeed;
        }

        if (!shouldConstraintHeight) { return; }

        Vector3 position = transform.position;
        position.y = constraintHeight;
        transform.position = position;
    }

    public void ConfigureBounds(Vector3 center, float radius, bool shouldUseBounds = true)
    {
        boundCenter = center;
        boundRadius = radius;
        useBounds = shouldUseBounds;
    }

    public void ConfigureSpeed(float boidBaseSpeed)
    {
        maxSpeed = boidBaseSpeed;
    }

    public void ConfigureVerticalConstraint(float boidHeightConstraint, bool shouldConstraintBoidHeight)
    {
        shouldConstraintHeight = shouldConstraintBoidHeight;
        constraintHeight = boidHeightConstraint;
    }

    private void FixedUpdate()
    {
        FindNeighbors();

        Vector3 steering =
            ComputeSeparation() * separationWeight +
            ComputeAlignment() * alignmentWeight +
            ComputeCohesion() * cohesionWeight +
            ComputeBounds() * boundsWeight +
            ComputeObstacleAvoidance() * obstacleWeight;

        if (steering.sqrMagnitude < 0.0001f)
        {
            steering = transform.forward;
        }

        Vector3 acceleration = steering.normalized * accelerationForce;
        Vector3 nextVelocity = rb.linearVelocity + acceleration * Time.fixedDeltaTime;
        if (shouldConstraintHeight)
        {
            nextVelocity.y = 0f;
        }

        rb.linearVelocity = Vector3.ClampMagnitude(nextVelocity, maxSpeed);

        if (!(rb.linearVelocity.sqrMagnitude > 0.01f)) { return; }

        Vector3 lookDirection = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        Quaternion targetRotation = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        transform.rotation =
            Quaternion.Slerp(transform.rotation, targetRotation, rotationSharpness * Time.fixedDeltaTime);
    }

    private void FindNeighbors()
    {
        neighbors.Clear();
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, neighborScanRadius, neighborHits, neighborMask,
            QueryTriggerInteraction.Collide);

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = neighborHits[i];
            if (!hit) { continue; }

            BoidAgent other = hit.attachedRigidbody
                ? hit.attachedRigidbody.GetComponent<BoidAgent>()
                : hit.GetComponent<BoidAgent>();

            if (!other || other == this) { continue; }

            neighbors.Add(other);
        }
    }

    private Vector3 ComputeSeparation()
    {
        Vector3 force = Vector3.zero;
        int count = 0;
        Vector3 position = transform.position;

        for (int i = 0; i < neighbors.Count; i++)
        {
            Vector3 toOther = position - neighbors[i].transform.position;
            float sqrDistance = toOther.sqrMagnitude;
            if (sqrDistance > separationDistanceSqr || sqrDistance < 0.0001f) { continue; }

            force += toOther / sqrDistance;
            count++;
        }

        return count > 0 ? force : Vector3.zero;
    }

    private Vector3 ComputeAlignment()
    {
        Vector3 averageVel = Vector3.zero;
        int count = 0;
        Vector3 position = transform.position;

        for (int i = 0; i < neighbors.Count; i++)
        {
            Vector3 offset = neighbors[i].transform.position - position;
            if (offset.sqrMagnitude > alignmentDistanceSqr) { continue; }

            averageVel += neighbors[i].Velocity;
            count++;
        }

        return count > 0 ? (averageVel / count).normalized : transform.forward;
    }

    private Vector3 ComputeCohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;
        Vector3 position = transform.position;

        for (int i = 0; i < neighbors.Count; i++)
        {
            Vector3 otherPosition = neighbors[i].transform.position;
            if ((otherPosition - position).sqrMagnitude > cohesionDistanceSqr) { continue; }

            center += otherPosition;
            count++;
        }

        return count > 0 ? (center / count - position).normalized : Vector3.zero;
    }

    private Vector3 ComputeBounds()
    {
        if (!useBounds) { return Vector3.zero; }

        Vector3 offset = transform.position - boundCenter;
        float distance = offset.magnitude;
        float innedRadius = boundRadius * 0.85f;
        if (distance < innedRadius) { return Vector3.zero; }

        float strength = Mathf.InverseLerp(innedRadius, boundRadius, distance);
        return (boundCenter - transform.position).normalized * strength;
    }

    private Vector3 ComputeObstacleAvoidance()
    {
        if (!avoidObstacle) { return Vector3.zero; }

        Vector3 vel = rb.linearVelocity;
        if (vel.sqrMagnitude < 0.0001f) { return Vector3.zero;}

        Vector3 direction = vel.normalized;
        Vector3 position = transform.position;
        Vector3 avoidance = Vector3.zero;

        if (Physics.SphereCast(position, obstacleProbeRadius, direction, out RaycastHit hit, obstacleLookAhead,
                obstacleMask, QueryTriggerInteraction.Ignore))
        {
            Vector3 awayFromHit = Vector3.Reflect(direction, hit.normal).normalized;
            avoidance += awayFromHit;
        }

        if (Physics.Raycast(position, Vector3.down, out RaycastHit floorHit, floorClearance, obstacleMask,
                QueryTriggerInteraction.Ignore))
        {
            float floorStrength = Mathf.InverseLerp(floorClearance, 0f, floorHit.distance);
            avoidance += Vector3.up * floorStrength;
        }

        return avoidance.normalized;
    }
}