using System.Collections;
using UnityEditor.Build.Content;
using UnityEngine;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    Vector3 m_StartPosition;
    Quaternion m_StartRotation;
    float m_Yaw;
    float m_Pitch;
    public float m_YawSpeed;
    public float m_PitchSpeed;
    public float m_MinPitch;
    public float m_MaxPitch;
    public Transform m_PitchController;
    public bool m_UseInvertedYaw;
    public bool m_UseInvertedPitch;
    public CharacterController m_CharacterController;
    float m_VerticalSpeed=0.0f;

    bool m_AngleLocked=false;
    public float m_Speed;
    public float m_JumpSpeed;
    public float m_SpeedMultiplier;
    public Camera m_Camera;
    public RaycastHit l_RaycastHit;

    [Header("Shoot")]
    public float m_ShootMaxDistance = 50.0f;
    public LayerMask m_ShootLayerMask;

    [Header("Input")]
    public KeyCode m_LeftKeyCode=KeyCode.A;
    public KeyCode m_RightKeyCode=KeyCode.D;
    public KeyCode m_UpKeyCode=KeyCode.W;
    public KeyCode m_DownKeyCode=KeyCode.S;
    public KeyCode m_JumpKeyCode=KeyCode.Space;
    public KeyCode m_RunKeyCode=KeyCode.LeftShift;
    public KeyCode m_GrabKeyCode = KeyCode.E;
    public int m_BlueshootMouseButton=0;
    public int m_OrangeshootMouseButton = 1;

    [Header("Debug Input")]
    public KeyCode m_DebugLockAngleKeyCode=KeyCode.I;
    
    [Header("Animation")]
    public Animation m_Animation;
    public AnimationClip m_IdleAnimationClip;
    public AnimationClip m_ShootAnimationClip;

    [Header("Portal")]
    public float m_PortalDistance = 1.5f;
    public float m_MaxAngleToTeleport = 75.0f;
    Vector3 m_MovementDirection;

    [Header("Portals")]
    public Portal m_BluePortal;
    public Portal m_OrangePortal;

    [Header("Object")]
    public ForceMode m_ForceMode;
    public float m_ThrowForce = 10.0f;
    public Transform m_GripTransform;
    Rigidbody m_AttachedObjectRigidbody;
    bool m_AttachingObject;
    Vector3 m_StartAttachingObjectPosition;
    float m_AttachingCurrentTime;
    public float m_AttachingTime = 1.5f;
    public float m_AttachingObjectRotationDistancelerp = 2.0f;
    bool m_AttachedObject;
    public LayerMask m_ValidAttachObjectLayerMask;

    void Start()
    {
        /*PlayerController l_Player = GameController.GetGameController().GetPlayer();
        if(l_Player!=null)
        {
            l_Player.m_CharacterController.enabled = false;
            l_Player.transform.position = transform.position;
            l_Player.transform.rotation = transform.rotation;
            l_Player.m_CharacterController.enabled = true;
            GameObject.Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
        GameController.GetGameController().SetPlayer(this);
        */
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
    }
    void Update()
    {
        float l_MouseX=Input.GetAxis("Mouse X");
        float l_MouseY=Input.GetAxis("Mouse Y");

        if(Input.GetKeyDown(m_DebugLockAngleKeyCode))
            m_AngleLocked=!m_AngleLocked;

        if(!m_AngleLocked)
        {
            m_Yaw=m_Yaw+l_MouseX*m_YawSpeed*Time.deltaTime*(m_UseInvertedYaw ? -1.0f : 1.0f);
            m_Pitch=m_Pitch+l_MouseY*m_PitchSpeed*Time.deltaTime*(m_UseInvertedPitch ? -1.0f : 1.0f);
            m_Pitch=Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);
            transform.rotation=Quaternion.Euler(0.0f, m_Yaw, 0.0f);
            m_PitchController.localRotation=Quaternion.Euler(m_Pitch, 0.0f, 0.0f);
        }
        
        Vector3 l_Movement=Vector3.zero;
        float l_YawPiRadians=m_Yaw*Mathf.Deg2Rad;
        float l_Yaw90PiRadians=(m_Yaw+90.0f)*Mathf.Deg2Rad;
        Vector3 l_ForwardDirection=new Vector3(Mathf.Sin(l_YawPiRadians), 0.0f, Mathf.Cos(l_YawPiRadians));
        Vector3 l_RightDirection=new Vector3(Mathf.Sin(l_Yaw90PiRadians), 0.0f, Mathf.Cos(l_Yaw90PiRadians));

        if(Input.GetKey(m_RightKeyCode))
            l_Movement=l_RightDirection;
		else if(Input.GetKey(m_LeftKeyCode))
            l_Movement=-l_RightDirection;

        if(Input.GetKey(m_UpKeyCode))
            l_Movement+=l_ForwardDirection;
		else if(Input.GetKey(m_DownKeyCode))
            l_Movement-=l_ForwardDirection;

        float l_SpeedMultiplier=1.0f;

        if(Input.GetKey(m_RunKeyCode))
            l_SpeedMultiplier=m_SpeedMultiplier;

        l_Movement.Normalize();
        m_MovementDirection = l_Movement;
        l_Movement*=m_Speed*l_SpeedMultiplier*Time.deltaTime;
        
        m_VerticalSpeed=m_VerticalSpeed+Physics.gravity.y*Time.deltaTime;
        l_Movement.y=m_VerticalSpeed*Time.deltaTime;
        
		CollisionFlags l_CollisionFlags=m_CharacterController.Move(l_Movement);
        if(m_VerticalSpeed<0.0f && (l_CollisionFlags & CollisionFlags.Below)!=0) //si estoy cayendo y colisiono con el suelo
        {
            m_VerticalSpeed=0.0f;
            if(Input.GetKeyDown(m_JumpKeyCode))
                m_VerticalSpeed=m_JumpSpeed;
        }
        else if(m_VerticalSpeed>0.0f && (l_CollisionFlags & CollisionFlags.Above)!=0) //si estoy subiendo y colision con un techo
            m_VerticalSpeed=0.0f;

        if (CanShoot())
        {
            if (Input.GetMouseButtonUp(m_BlueshootMouseButton))
                Shoot(m_BluePortal);
            else if (Input.GetMouseButtonUp(m_OrangeshootMouseButton))
                Shoot(m_OrangePortal);
        }

        if (CanAttachObject())
            AttachObject();

        if(m_AttachedObjectRigidbody != null)
        {
            UpdateAttachedObject();
        }

    }
    bool CanAttachObject() /* hay que hacer este codigo */
    {
        return true;
    }

    bool CanShoot() /* hay que hacer este codigo */
    {
        return m_AttachedObjectRigidbody == null;
    }

    void Shoot(Portal _Portal)
    {
       SetShootAnimation();
       Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
       if(Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDistance, _Portal.m_ValidPortalLayerMask.value, QueryTriggerInteraction.Ignore))
       {
            if (l_RaycastHit.collider.CompareTag("DrawableWall"))
            {
                if(_Portal.IsValidPosition(l_RaycastHit.point, l_RaycastHit.normal))
                {
                    _Portal.gameObject.SetActive(true);
                }
                else
                    _Portal.gameObject.SetActive(false);
            }
       }
            
    }

    void SetShootAnimation()
    {
        m_Animation.CrossFade(m_ShootAnimationClip.name, 0.1f);
        m_Animation.CrossFadeQueued(m_IdleAnimationClip.name, 0.0f);
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
       float l_DotValue = Vector3.Dot(_Portal.transform.forward, -m_MovementDirection);
       return l_DotValue>Mathf.Cos(m_MaxAngleToTeleport*Mathf.Deg2Rad);
    }

    void Teleport(Portal _Portal)
    {
        Vector3 l_NextPosition = transform.position + m_MovementDirection * m_PortalDistance;
        Vector3 l_Localposition = _Portal.m_OtherPortalTransform.InverseTransformPoint(l_NextPosition);
        Vector3 l_WorldPosition = _Portal.m_MirrorPortal.transform.TransformPoint(l_Localposition);

        Vector3 l_WorldForward = transform.forward;
        Vector3 l_LocalForward = _Portal.m_OtherPortalTransform.InverseTransformDirection(l_WorldPosition);
        l_WorldForward = _Portal.m_MirrorPortal.transform.TransformDirection(l_LocalForward);

        m_CharacterController.enabled = false;
        transform.position = l_WorldPosition;
        transform.rotation = Quaternion.LookRotation(l_WorldForward);
        m_Yaw = transform.rotation.eulerAngles.y;
        m_CharacterController.enabled = true;

    }

    public void Restart()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
    void AttachObject()
    {
        if(Input.GetKeyDown(m_GrabKeyCode))
        {
            Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, m_ShootMaxDistance, m_ValidAttachObjectLayerMask.value))
            {

                if (l_RaycastHit.collider.CompareTag("Cube") || l_RaycastHit.collider.CompareTag("Turret"))
                    AttachObject(l_RaycastHit.rigidbody);
            }
        }
    }

    void AttachObject(Rigidbody _Rigidbody)
    {
        m_AttachingObject = true;
        m_AttachedObjectRigidbody = _Rigidbody;
        m_AttachedObjectRigidbody.GetComponent<CompanionCube>().SetAttachedObject(true);
        m_StartAttachingObjectPosition = _Rigidbody.transform.position;
        m_AttachingCurrentTime = 0.0f;
        m_AttachedObject = false;
    }

    void UpdateAttachedObject()
    {
        if(m_AttachingObject)
        {
            m_AttachingCurrentTime+= Time.deltaTime;
            float l_Pct = Mathf.Min(1.0f, m_AttachingCurrentTime / m_AttachingTime);
            Vector3 l_Position = Vector3.Lerp(m_StartAttachingObjectPosition, m_GripTransform.position, l_Pct);
            float l_Distance = Vector3.Distance(l_Position, m_GripTransform.position);
            float l_RotationPct = 1.0f - Mathf.Min(1.0f, l_Distance / m_AttachingObjectRotationDistancelerp);
            Quaternion l_Rotation = Quaternion.Lerp(transform.rotation, m_GripTransform.rotation, l_RotationPct);
            m_AttachedObjectRigidbody.MovePosition(l_Position);
            m_AttachedObjectRigidbody.MoveRotation(l_Rotation);
            if(l_Pct == 1.0f)
            {
                m_AttachingObject = false;
                m_AttachedObject = true;
                m_AttachedObjectRigidbody.transform.SetParent(m_GripTransform);
                m_AttachedObjectRigidbody.transform.localPosition = Vector3.zero;
                m_AttachedObjectRigidbody.transform.localRotation = Quaternion.identity;
                m_AttachedObjectRigidbody.isKinematic = true;
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            ThrowObject(m_ThrowForce);
        }
        else if (Input.GetMouseButtonDown(1) || Input.GetKeyUp(m_GrabKeyCode))
        {
            ThrowObject(0.0f);
        }
    }
    void ThrowObject(float Force)
    {
        m_AttachedObjectRigidbody.isKinematic = false;
        m_AttachedObjectRigidbody.AddForce(m_PitchController.forward * Force, m_ForceMode);
        m_AttachedObjectRigidbody.transform.SetParent(null);
        m_AttachingObject = false;
        m_AttachedObject = false;
        m_AttachedObjectRigidbody.GetComponent<CompanionCube>().SetAttachedObject(false);
        m_AttachedObjectRigidbody = null;
    }
}
