using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class MouseInteractivity : MonoBehaviour
{
    public Vector3 targetPosition { 
        get { return m_TargetPosition; }
        set { m_TargetPosition = value; } 
    }
    
    [SerializeField]
    private Vector3 m_TargetPosition = new Vector3(1f, 0f, 2f);

    public Quaternion rot = Quaternion.identity;

    public virtual void Update()
    {
        m_TargetPosition = transform.position;

        transform.LookAt(m_TargetPosition);
        transform.rotation = rot;
    }
}


