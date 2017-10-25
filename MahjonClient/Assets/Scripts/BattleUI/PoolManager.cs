using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool
{

    private List<GameObject> pool;
    private string m_prefabName;

    public Pool(string prefabName)
    {
        m_prefabName = prefabName;
        pool = new List<GameObject>();
    }

    public T GetObject<T>()
    {
        foreach (GameObject iter in pool)
        {
            if (iter.activeSelf == false)
            {
                iter.SetActive(true);
                return iter.GetComponent<T>();
            }

        }
        string prefabPath = ResourcesManager.Instance.GetResPath(m_prefabName);
        Object prefab = Resources.Load(prefabPath);
        GameObject newPrefab = GameObject.Instantiate(prefab) as GameObject;
        newPrefab.name = m_prefabName + " Clone0" + pool.Count.ToString();
        newPrefab.SetActive(true);
        newPrefab.AddComponent(typeof(T));
        pool.Add(newPrefab);
        return newPrefab.GetComponent<T>();
    }

    public void RecycleAll()
    {
        for (int i = 0; i < pool.Count; i++)
        {
            pool[i].gameObject.SetActive(false);
        }
    }
}
