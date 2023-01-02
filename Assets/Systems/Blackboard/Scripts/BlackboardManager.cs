using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Blackboard<BlackboardKeyType>
{
    Dictionary<BlackboardKeyType, int>         IntValues           = new();
    Dictionary<BlackboardKeyType, float>       FloatValues         = new();
    Dictionary<BlackboardKeyType, bool>        BoolValues          = new();
    Dictionary<BlackboardKeyType, string>      StringValues        = new();
    Dictionary<BlackboardKeyType, Vector3>     Vector3Values       = new();
    Dictionary<BlackboardKeyType, GameObject>  GameObjectValues    = new();
    Dictionary<BlackboardKeyType, object>      GenericValues       = new();

    public void SetGeneric<T>(BlackboardKeyType key, T value)
    {
        GenericValues[key] = value;
    }

    public T GetGeneric<T>(BlackboardKeyType key)
    {
        object value;
        if (GenericValues.TryGetValue(key, out value))
            return (T)value;

        throw new System.ArgumentException($"Could not find value for {key} in GenericValues");
    }

    public bool TryGetGeneric<T>(BlackboardKeyType key, out T value, T defaultValue)
    {
        object localValue;
        if (GenericValues.TryGetValue(key, out localValue))
        {
            value= (T)localValue;
            return true;
        }

        value = defaultValue;
        return false;
    }

    private T Get<T>(Dictionary<BlackboardKeyType, T> keySet, BlackboardKeyType key)
    {
        T value;
        if (keySet.TryGetValue(key, out value))
            return value;

        throw new System.ArgumentException($"Could not find value for {key} in {typeof(T).Name}Values");
    }

    private bool TryGet<T>(Dictionary<BlackboardKeyType, T> keySet, BlackboardKeyType key, out T value, T defaultValue = default)
    {
        if (keySet.TryGetValue(key, out value))
            return true;

        value = default;
        return false;
    }

    public void Set(BlackboardKeyType key, int value)
    {
        IntValues[key] = value;
    }

    public int GetInt(BlackboardKeyType key)
    {
        return Get(IntValues, key);
    }

    public bool TryGet(BlackboardKeyType key, out int value, int defaultValue = 0)
    {
        return TryGet(IntValues, key, out value, defaultValue);
    }

    public void Set(BlackboardKeyType key, float value)
    {
        FloatValues[key] = value;
    }

    public float GetFloat(BlackboardKeyType key)
    {
        return Get(FloatValues, key);
    }

    public bool TryGet(BlackboardKeyType key, out float value, float defaultValue = 0)
    {
        return TryGet(FloatValues, key, out value, defaultValue);
    }

    public void Set(BlackboardKeyType key, bool value)
    {
        BoolValues[key] = value;
    }

    public bool GetBool(BlackboardKeyType key)
    {
        return Get(BoolValues, key);
    }

    public bool TryGet(BlackboardKeyType key, out bool value, bool defaultValue = false)
    {
        return TryGet(BoolValues, key, out value, defaultValue);
    }

    public void Set(BlackboardKeyType key, string value)
    {
        StringValues[key] = value;
    }

    public string GetString(BlackboardKeyType key)
    {
        return Get(StringValues, key);
    }

    public bool TryGet(BlackboardKeyType key, out string value, string defaultValue = "")
    {
        return TryGet(StringValues, key, out value, defaultValue);
    }

    public void Set(BlackboardKeyType key, Vector3 value)
    {
        Vector3Values[key] = value;
    }

    public Vector3 GetVector3(BlackboardKeyType key)
    {
        return Get(Vector3Values, key);
    }

    public bool TryGet(BlackboardKeyType key, out Vector3 value, Vector3 defaultValue)
    {
        return TryGet(Vector3Values, key, out value, defaultValue);
    }

    public void Set(BlackboardKeyType key, GameObject value)
    {
        GameObjectValues[key] = value;
    }

    public GameObject GetGameObject(BlackboardKeyType key)
    {
        return Get(GameObjectValues, key);
    }

    public bool TryGet(BlackboardKeyType key, out GameObject value, GameObject defaultValue = null)
    {
        return TryGet(GameObjectValues, key, out value, defaultValue);
    }
}


public abstract class BlackboardKeyBase
{
}

public class BlackboardManager : MonoBehaviour
{
    public static BlackboardManager Instance { get; private set; } = null;

    Dictionary<MonoBehaviour, object> IndividualBlackboards = new();
    Dictionary<int, object> SharedBlackboards = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Debug.LogError($"Trying to create second BlackboardManager on {gameObject.name}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public Blackboard<T> GetIndividualBlackboard<T>(MonoBehaviour requestor) where T : BlackboardKeyBase, new()
    {
        if (!IndividualBlackboards.ContainsKey(requestor))
            IndividualBlackboards[requestor] = new Blackboard<T>();

        return IndividualBlackboards[requestor] as Blackboard<T>;
    }

    public Blackboard<T> GetSharedBlackboard<T>(int uniqueID) where T : BlackboardKeyBase, new()
    {
        if (!SharedBlackboards.ContainsKey(uniqueID))
            SharedBlackboards[uniqueID] = new Blackboard<T>();

        return SharedBlackboards[uniqueID] as Blackboard<T>;
    }
}
