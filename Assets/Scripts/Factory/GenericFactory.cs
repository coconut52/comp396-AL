using System;
using UnityEngine;

public class GenericFactory : MonoBehaviour
{
    [SerializeField] private Transform projectileParent;

    public T CreateProjectile<T>(Vector3 position) where T : Projectile
    {
        GameObject go = new GameObject(typeof(T).Name);
        if (projectileParent != null)
        {
            go.transform.SetParent(projectileParent);
        }
        go.transform.position = position;
        T projectile = go.AddComponent<T>();
        return projectile;
    }

    public T CreateProjectileFromPrefab<T>(T prefab, Vector3 position) where T : Projectile
    {
        T instance = Instantiate(prefab, position, Quaternion.identity);
        if (projectileParent != null)
        {
            instance.transform.SetParent(projectileParent);
        }
        return instance;
    }
}