using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    private GameObject m_Player;
    public GameObject Player
    {
        get { return m_Player; }
        set { m_Player = value; }
    }

    public float m_DistViewY = 11.0f;
    public float m_DistViewZ = 7.5f;
    public float m_Angle = 55.0f;

    Vector3 CamPos;

    // Start is called before the first frame update
    void Start()
    {
        m_DistViewY = 10.0f;
        CamPos = new Vector3(m_Player.transform.position.x, m_Player.transform.position.y + m_DistViewY, m_Player.transform.position.z - m_DistViewZ);
        this.transform.rotation = Quaternion.Euler(m_Angle, 0.0f, 0.0f);
        this.transform.position = CamPos;
    }

    void LateUpdate()
    {
        CamPos = new Vector3(m_Player.transform.position.x, m_Player.transform.position.y + m_DistViewY, m_Player.transform.position.z - m_DistViewZ);
        this.transform.rotation = Quaternion.Euler(m_Angle, 0.0f, 0.0f);
        this.transform.position = Vector3.Lerp(this.transform.position, CamPos, Time.deltaTime * 5.0f);
    }
}
