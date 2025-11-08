using UnityEngine;
using UnityEngine.Rendering;

public class RefractionCube : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public LineRenderer m_LineRenderer;
    public float m_MaxDistance = 50.0f;
    public LayerMask m_LayerMask;
    bool m_IsReflectingLaser = false;


    private void Start()
    {
        m_LineRenderer.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (m_IsReflectingLaser)
        {
            UpdateLaser();
            m_IsReflectingLaser = false;
        }  
        else
            m_LineRenderer.gameObject.SetActive(false);
        
    }
    public void Reflect()
    {
        if (m_IsReflectingLaser)
            return;
        m_IsReflectingLaser = true;
        UpdateLaser();
    }
    public void UpdateLaser()
    {
        m_LineRenderer.gameObject.SetActive(true);
        float l_Distance = m_MaxDistance;
        Ray l_Ray = new Ray(m_LineRenderer.transform.position, m_LineRenderer.transform.forward);
        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxDistance, m_LayerMask.value, QueryTriggerInteraction.Ignore))
        {
            l_Distance = l_RaycastHit.distance;
            if (l_RaycastHit.collider.CompareTag("RefractionCube"))
            {
                l_RaycastHit.collider.GetComponent<RefractionCube>().Reflect();
            }
        }
        Vector3 l_Position = new Vector3(0.0f, 0.0f, l_Distance);
        m_LineRenderer.SetPosition(1, l_Position);
    }
}
