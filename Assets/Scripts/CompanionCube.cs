using UnityEngine;

public class CompanionCube : MonoBehaviour
{
    Rigidbody m_Rigidbody;
    public float m_PortalDistance = 1.5f;
    public float m_MaxAngleToTeleport = 45.0f;
    bool m_AttachedObject = false;

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Portal"))
        {
            Portal l_Portal = other.GetComponent<Portal>();
            if(CanTeleport(l_Portal))
            Teleport(l_Portal);
        }
    }

    bool CanTeleport(Portal _Portal)
    {
        float l_DotValue = Vector3.Dot(_Portal.transform.forward, -m_Rigidbody.linearVelocity.normalized);
        return !m_AttachedObject && l_DotValue > Mathf.Cos(m_MaxAngleToTeleport * Mathf.Deg2Rad);
    }

    void Teleport(Portal _Portal)
    {
        Vector3 l_Direction = m_Rigidbody.linearVelocity.normalized;
        Vector3 l_WorldPosition = transform.position + l_Direction * m_PortalDistance;
        Vector3 l_LocalPosition = _Portal.m_OtherPortalTransform.InverseTransformPoint(l_WorldPosition);
        transform.position = _Portal.m_MirrorPortal.transform.TransformPoint(l_LocalPosition);

        Vector3 l_WorldDirection = transform.forward;
        Vector3 l_LocalDirection = _Portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldDirection);
        transform.forward = _Portal.m_MirrorPortal.transform.TransformDirection(l_LocalDirection);

        Vector3 l_LocalVelocity = _Portal.m_OtherPortalTransform.InverseTransformDirection(m_Rigidbody.linearVelocity);
        m_Rigidbody.linearVelocity = _Portal.m_MirrorPortal.transform.TransformDirection(l_LocalVelocity);
        float l_Scale = _Portal.m_MirrorPortal.transform.localScale.x / _Portal.transform.localScale.x;
        m_Rigidbody.transform.localScale = Vector3.one * l_Scale*m_Rigidbody.transform.localScale.x;
    }

    public void SetAttachedObject(bool AttachedObject)
    {
        m_AttachedObject = AttachedObject;
    }
}
