using UnityEngine;
using UnityEngine.Events;

public class PortalButton : MonoBehaviour
{
    public UnityEvent m_Event;
    public CompanionSpawner m_Spawner;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube"))
            m_Event.Invoke();

        if (other.CompareTag("Player"))
            m_Spawner.Spawn();
    }
}
