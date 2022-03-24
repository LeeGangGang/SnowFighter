using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCtrl : MonoBehaviour
{
    public GameObject m_Player;
    public GameObject Player
    {
        get { return m_Player; }
        set { m_Player = value; }
    }

    private float m_DistView = 9.0f;

    Vector3 CamPos;

    // Start is called before the first frame update
    void Start()
    {
        m_DistView = 9.0f;
        this.transform.rotation = Quaternion.Euler(45.0f, 0.0f, 0.0f);
        CamPos = new Vector3(m_Player.transform.position.x, m_Player.transform.position.y + m_DistView, m_Player.transform.position.z - 10.0f);
        this.transform.position = CamPos;
    }

    void LateUpdate()
    {
        CamPos = new Vector3(m_Player.transform.position.x, m_Player.transform.position.y + m_DistView, m_Player.transform.position.z - 10.0f);
        this.transform.position = Vector3.Lerp(this.transform.position, CamPos, Time.deltaTime * 5.0f);
    }
}
