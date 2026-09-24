using UnityEngine;

public class EventTimer : MonoBehaviour
{
    [SerializeField] private VoidEventChannel _channelPos;
    [SerializeField] private Transform[] _harvestLocations;
    private float timer = 0;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > 5f)
        {
            _channelPos.RaiseEvent();
            timer = 0;
        }
    }
}
