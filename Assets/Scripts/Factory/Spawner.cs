using UnityEngine;

public class Spawner : MonoBehaviour
{
    private GenericFactory factory;
    private FarmerFactory farmerFactory;
    private GuardFactory guardFactory = new GuardFactory();

    private void Start()
    {
        factory = GetComponent<GenericFactory>();
        if (factory == null)
        {
            factory = gameObject.AddComponent<GenericFactory>();
        }

        farmerFactory = GetComponent<FarmerFactory>();
        if (farmerFactory == null)
        {
            farmerFactory = gameObject.AddComponent<FarmerFactory>();
        }
        Bomb bomb = factory.CreateProjectile<Bomb>(transform.position);
        Missile missile = factory.CreateProjectile<Missile>(transform.position);
        Spell spell = factory.CreateProjectile<Spell>(transform.position);

        SpawnUnits(farmerFactory, 5);

        SpawnUnit(guardFactory);
    }

    private void SpawnUnit(IThemeFactory themeFactory)
    {
        IEntity entity = themeFactory.CreateEntity();
        IStateMachine stateMachine = themeFactory.CreateStateMachine();
        ITask task = themeFactory.CreateTask();
    }

    private void SpawnUnits(IThemeFactory themeFactory, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnUnit(themeFactory);
        }
    }
}

public interface IEntity {}
public interface IStateMachine {}
public interface ITask {}

public interface IThemeFactory
{
    IEntity CreateEntity();
    IStateMachine CreateStateMachine();
    ITask CreateTask();
}

public class GuardFactory : IThemeFactory
{
    public IEntity CreateEntity()
    {
        Debug.Log("GuardFactory");
        return null;
    }

    public IStateMachine CreateStateMachine()
    {
        Debug.Log("GuardFactory");
        return null;
    }

    public ITask CreateTask()
    {
        Debug.Log("GuardFactory");
        return null;
    }
}