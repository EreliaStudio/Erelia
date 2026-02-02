using System;
using UnityEngine;

[Serializable]
public class BushConfiguration
{
    [SerializeField] private BattleAreaProfile areaProfile;

    public BattleAreaProfile AreaProfile => areaProfile;

    public void SetAreaProfile(BattleAreaProfile profile)
    {
        areaProfile = profile;
    }
}

public class BushIslandConfiguration : MonoBehaviour
{
    [SerializeField] private BushConfiguration configuration = new BushConfiguration();
    [SerializeField] private MeshCollider cachedCollider;
    [SerializeField] private MeshFilter cachedFilter;

    public BushConfiguration Configuration => configuration;
    public MeshCollider Collider => cachedCollider;
    public MeshFilter Filter => cachedFilter;

    public void SetAreaProfile(BattleAreaProfile profile)
    {
        if (configuration == null)
        {
            configuration = new BushConfiguration();
        }

        configuration.SetAreaProfile(profile);
    }

    public void EnsureComponents()
    {
        if (cachedCollider == null)
        {
            cachedCollider = GetComponent<MeshCollider>();
            if (cachedCollider == null)
            {
                cachedCollider = gameObject.AddComponent<MeshCollider>();
            }
        }

        if (cachedFilter == null)
        {
            cachedFilter = GetComponent<MeshFilter>();
            if (cachedFilter == null)
            {
                cachedFilter = gameObject.AddComponent<MeshFilter>();
            }
        }

        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            renderer = gameObject.AddComponent<MeshRenderer>();
        }
        renderer.enabled = false;
    }
}
