using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoidSpawner : MonoBehaviour
{
    [SerializeField] private GameObject boidPrefab;

    [SerializeField] private int boidCount = 32;
    [SerializeField] private Transform spawnArea;
    [SerializeField] private Vector3 spawnOffset;
    [SerializeField] private float boidSize = 1f;

    [SerializeField] private float boidBaseSpeed = 7f;
    [SerializeField] private float boidSpeedVariation = 1.5f;
    [SerializeField] private float flockBoundRadius = 30f;

    [SerializeField] private float boidHeightConstraint = 4f;
    [SerializeField] private bool shouldConstraintHeight;

    private readonly List<BoidAgent> boids = new List<BoidAgent>();

    private void Start()
    {
        SpawnFlock();
    }

    [ContextMenu("Spawn Flock")]
    private void SpawnFlock()
    {
        ClearFlock();
        EnsureFlockRoot();

        for (int i = 0; i < boidCount; i++)
        {
            BoidAgent boid = CreateBoid();
            boids.Add(boid);
        }
    }

    [ContextMenu("Clear Flock")]
    private void ClearFlock()
    {
        for (int i = boids.Count - 1; i >= 0; i--)
        {
            BoidAgent boid = boids[i];
            if (boid)
            {
                DestroyImmediate(boid.gameObject);
            }
        }
        boids.Clear();
    }

    private void EnsureFlockRoot()
    {
        if (spawnArea) { return; }

        GameObject root = new GameObject("Flock Spawn Area");
        spawnArea = root.transform;
    }

    private BoidAgent CreateBoid()
    {
        Vector3 spawnPosition = GetRandomSpawnOffset();
        GameObject boidObject = Instantiate(boidPrefab, spawnPosition, Quaternion.identity, spawnArea);
        boidObject.transform.localScale = Vector3.one * boidSize;

        CapsuleCollider collider = boidObject.GetOrAddComponent<CapsuleCollider>();
        collider.isTrigger = false;

        Rigidbody body = boidObject.GetOrAddComponent<Rigidbody>();
        body.useGravity = false;
        body.constraints = RigidbodyConstraints.FreezeRotation;
        body.interpolation = RigidbodyInterpolation.Interpolate;
        body.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

        BoidAgent boid = boidObject.GetOrAddComponent<BoidAgent>();
        boid.ConfigureBounds(spawnArea.position, flockBoundRadius, true);
        boid.ConfigureSpeed(boidBaseSpeed + Random.Range(-boidSpeedVariation, boidSpeedVariation));
        boid.ConfigureVerticalConstraint(boidHeightConstraint, shouldConstraintHeight);
        return boid;
    }

    private Vector3 GetRandomSpawnOffset()
    {
        return spawnArea.position + new Vector3(
            Random.Range(-spawnOffset.x, spawnOffset.x),
            shouldConstraintHeight ? 0f : Random.Range(-spawnOffset.y, spawnOffset.y),
            Random.Range(-spawnOffset.z, spawnOffset.z));
    }
}
