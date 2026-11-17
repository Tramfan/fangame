using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerProjectilePool : MonoBehaviour
{
    private sealed class PooledInstance : MonoBehaviour
    {
        public PlayerProjectilePool OwnerPool;
        public GameObject PrefabKey;
        public bool IsInPool;
    }

    private static PlayerProjectilePool instance;

    private readonly Dictionary<GameObject, Stack<GameObject>> pools = new();

    public static PlayerProjectilePool Instance => instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        instance = null;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    public static PlayerProjectilePool GetOrCreate()
    {
        if (instance != null)
        {
            return instance;
        }

        GameObject host = new("PlayerProjectilePool");
        return host.AddComponent<PlayerProjectilePool>();
    }

    public void Prewarm(GameObject prefab, int targetCount)
    {
        if (prefab == null || targetCount <= 0)
        {
            return;
        }

        Stack<GameObject> stack = GetOrCreateStack(prefab);

        while (stack.Count < targetCount)
        {
            GameObject created = CreateInstance(prefab);
            created.SetActive(false);

            PooledInstance tag = created.GetComponent<PooledInstance>();
            tag.IsInPool = true;

            stack.Push(created);
        }
    }

    public GameObject Spawn(
        GameObject prefab,
        Vector3 position,
        Quaternion rotation
    )
    {
        if (prefab == null)
        {
            return null;
        }

        Stack<GameObject> stack = GetOrCreateStack(prefab);
        GameObject item = null;

        while (stack.Count > 0 && item == null)
        {
            item = stack.Pop();
        }

        if (item == null)
        {
            item = CreateInstance(prefab);
        }

        PooledInstance tag = item.GetComponent<PooledInstance>();
        tag.IsInPool = false;

        Transform itemTransform = item.transform;
        itemTransform.SetPositionAndRotation(position, rotation);

        item.SetActive(true);

        if (item.TryGetComponent(out PlayerBullet bullet))
        {
            bullet.SyncSpawnPosition(position);
        }

        return item;
    }

    public static void Despawn(GameObject projectileObject)
    {
        if (projectileObject == null)
        {
            return;
        }

        if (projectileObject.TryGetComponent(out PooledInstance tag) &&
            tag.OwnerPool != null)
        {
            tag.OwnerPool.ReturnInternal(projectileObject, tag);
            return;
        }

        Destroy(projectileObject);
    }

    private void ReturnInternal(
        GameObject projectileObject,
        PooledInstance tag
    )
    {
        if (tag.IsInPool)
        {
            return;
        }

        tag.IsInPool = true;
        projectileObject.SetActive(false);

        Stack<GameObject> stack = GetOrCreateStack(tag.PrefabKey);
        stack.Push(projectileObject);
    }

    private Stack<GameObject> GetOrCreateStack(GameObject prefab)
    {
        if (!pools.TryGetValue(prefab, out Stack<GameObject> stack))
        {
            stack = new Stack<GameObject>(64);
            pools.Add(prefab, stack);
        }

        return stack;
    }

    private GameObject CreateInstance(GameObject prefab)
    {
        GameObject created = Instantiate(prefab, transform);

        PooledInstance tag = created.AddComponent<PooledInstance>();
        tag.OwnerPool = this;
        tag.PrefabKey = prefab;
        tag.IsInPool = false;

        return created;
    }
}