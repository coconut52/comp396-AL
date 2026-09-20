using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MyPhysics : MonoBehaviour
{
    [SerializeField] private GameObject _canonBallPrefab;
    [SerializeField] private ForceMode _forceMode;
    [SerializeField] private Vector3 _force;
    private Rigidbody _canonBody;
    
    void Start()
    {
        _canonBody = Instantiate(_canonBallPrefab, transform).GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            _canonBody.AddForce(_force, _forceMode);
        }
    }
}