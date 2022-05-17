using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatapultCtrl : MonoBehaviour
{
    public GameObject m_SnowBall;
    public Transform m_ArmTr;
    Vector3 m_Rot = new Vector3(-110f, 0f, 0f);
    public bool IsShot = false;

    Vector3 AttPos;

    // Update is called once per frame
    void Update()
    {
        if (IsShot)
        {
            m_SnowBall.GetComponent<ParticleSystem>().Play();
            if (m_ArmTr.localRotation.x > -0.7f)
                m_ArmTr.Rotate(Vector3.right * Time.deltaTime * -500f, Space.Self);
            else
            {
                IsShot = false;
                StartCoroutine("DestroySnowBall");
            }
        }
    }

    IEnumerator DestroySnowBall()
    {
        while (m_SnowBall.transform.localPosition.y < 10f)
        {
            m_SnowBall.transform.Translate(Vector3.up * Time.deltaTime * 40f, Space.Self);
            yield return null;
        }

        m_SnowBall.transform.SetParent(null);
        m_SnowBall.transform.position = AttPos;
        m_SnowBall.GetComponent<SphereCollider>().enabled = true;
        m_SnowBall.GetComponent<CatapultSnowBallCtrl>().Drop();
        DestroyThisObj();
    }

    public void Shot(Vector3 a_Pos)
    {
        float Volume = SoundManager.Instance.GetDistVolume(this.transform.position);
        SoundManager.Instance.PlayEffSound("CatapultShot", Volume);
        
        AttPos = a_Pos;
        IsShot = true;
    }

    IEnumerator DestroyCatapult(float tm)
    {
        yield return new WaitForSeconds(tm);

        if (Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>().m_CurStatus == PlayerState.Catapult)
            Camera.main.GetComponent<CameraCtrl>().Player.GetComponent<PlayerCtrl>().m_CurStatus = PlayerState.Idle;

        Destroy(this.gameObject);
    }

    public void DestroyThisObj(float tm = 0)
    {
        StartCoroutine(this.DestroyCatapult(tm));
    }
}
