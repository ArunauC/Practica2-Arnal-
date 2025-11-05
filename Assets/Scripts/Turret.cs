using UnityEngine;

public class Turret : MonoBehaviour
{
    public LineRenderer m_LineRenderer;
    public float m_MaxDistance = 50.0f;
    public LayerMask m_LayerMask;
    public float m_MaxAlifeAngle = 15.0f;

    void Update()
    {
        float l_DotAngle = Vector3.Dot(transform.up, Vector3.up);
        if (l_DotAngle<Mathf.Cos(m_MaxAlifeAngle*Mathf.Deg2Rad))
        {
            m_LineRenderer.gameObject.SetActive(false);
        }
        else
        {
            m_LineRenderer.gameObject.SetActive(true);
            float l_Distance = m_MaxDistance;
            Ray l_Ray = new Ray(m_LineRenderer.transform.position, m_LineRenderer.transform.forward);
            if(Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_MaxDistance, m_LayerMask.value, QueryTriggerInteraction.Ignore))
                l_Distance = l_RaycastHit.distance;
                Vector3 l_Position = new Vector3(0.0f, 0.0f, l_Distance);
            m_LineRenderer.SetPosition(1, l_Position);
        }

    }
}
