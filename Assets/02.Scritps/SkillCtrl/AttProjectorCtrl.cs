using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttProjectorCtrl : MonoBehaviour, IPunObservable
{
    // PhotonView ������Ʈ�� �Ҵ��� ����
    [HideInInspector] public PhotonView pv = null;
    private Transform tr;
    private Vector3 m_CurPos = Vector3.zero;

    public GameObject m_SnowBallPrefab;

    // Start is called before the first frame update
    void Awake()
    {
        tr = GetComponent<Transform>();
        pv = GetComponent<PhotonView>();

        m_CurPos = tr.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!pv.IsMine)
            NetworkTransform_Update();
    }

    private void NetworkTransform_Update()
    {
        // �߰����(��ġ, ȸ����) ���� ����
        if (10f < (tr.position - m_CurPos).magnitude)
            tr.position = m_CurPos;
        else
            tr.position = Vector3.Lerp(tr.position, m_CurPos, Time.deltaTime * 10f);
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(tr.position);
        }
        else
        {
            m_CurPos = (Vector3)stream.ReceiveNext();
        }
    }
}