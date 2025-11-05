using UnityEngine;
using UnityEngine.Events;

public class PortalButton : MonoBehaviour
{
    public UnityEvent m_Event;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cube"))
            m_Event.Invoke();
    } 
}
