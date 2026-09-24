using UnityEngine;

public class FarmerFactory : MonoBehaviour, IThemeFactory
{
    [SerializeField] private GameObject _farmerPrefab;

    public IEntity CreateEntity()
    {
        Instantiate(_farmerPrefab, gameObject.transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity);
        return null;
    }

    public IStateMachine CreateStateMachine()
    {
        Debug.Log("FarmerFactory");
        return null;
    }

    public ITask CreateTask()
    {
        Debug.Log("FarmerFactory");
        return null;
    }
}