using UnityEngine;
using System.Collections.Generic;

public class Vectors : MonoBehaviour
{
    [SerializeField] private Transform _player, _enemy;
    [SerializeField] private Vector3 _v1, _v2, _v3;
    [SerializeField] private float _k; // _ prefix
    [SerializeField] private Dictionary<int, Vector3> _matrix = new Dictionary<int, Vector3>();
    public float k; // camel Case
    public static float s_k = 0f; // s_ initial -> Every start, increase by 0.1f
    public const float K = 3.4f; // CAPITAL CASE


    private void Start()
    {
        _v1 = _player.position;
        _v2 = _enemy.position;
        _v3 = new Vector3(2, 4, -8);

        _k = 1.5f;
        k = 1.5f;
        s_k += 0.1f;

        print($"Initial Values => v1 = {_v1}, v2 = {_v2}, v3 = {_v3}, _k = {_k}, k = {k}, s_k = {s_k}");

        _matrix.Add(0, _player.position);
        _matrix.Add(1, _enemy.position);

        float dot = Vector3.Dot(_v1, _v1 - _v2);
        print($"Dot Value from Player and Enemy = {dot}");

        var v1_times_v2 = new Vector3(_v1.x * _v2.x, _v1.y * _v2.y, _v1.z * _v2.z);
        var v1_times_k = _v1 * k;
        var v1_plus_v2 = _v1 + _v2;
        var v1_minus_v2 = _v1 - _v2;

        print($"v1_times_v2 = {v1_times_v2}, v1_times_k = {v1_times_k}, v1_plus_v2 = {v1_plus_v2}, v1_minus_v2 ={v1_minus_v2}");
        // Normalize, Magnitude, SqrMagnitude, CrossProduct, Dot, Distance
    }

    private void OnDisable()
    {
        s_k = 0f; // Reseting Static Variables
    }
}
