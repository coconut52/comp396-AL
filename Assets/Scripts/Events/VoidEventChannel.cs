using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "VoidEventChannel", menuName = "Events/VoidEventChannel")]
public class VoidEventChannel : ScriptableObject
{
    public event UnityAction OnEventRaised; // This event is subscribed by observers or listeners

    public void RaiseEvent() // This is called by anyone that needs to dispatch this event
    {
        OnEventRaised?.Invoke();
    }
}


public class PlayerEventChannel : GenericEventChannel<Player>
{ }

public class Player
{
    public string name;
    public int level;
    public float currHealth;
    public float maxHealth;
}
