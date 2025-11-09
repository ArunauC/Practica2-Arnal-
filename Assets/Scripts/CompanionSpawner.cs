using UnityEngine;

public class CompanionSpawner : MonoBehaviour
{
    public GameObject m_CompanionCubePrefab;
    public Transform m_SpawnerTransform;

    private void OnTriggerEnter(Collider other)
    {
        //if (other.CompareTag("Player"))
            //Spawn();
    }
    public void Spawn()
    {
        GameObject l_GameObject = GameObject.Instantiate(m_CompanionCubePrefab);
        l_GameObject.transform.position = m_SpawnerTransform.position;
        l_GameObject.transform.rotation = m_SpawnerTransform.rotation;
    }
}
