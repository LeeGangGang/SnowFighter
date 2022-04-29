using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimEvent : MonoBehaviour
{
    PlayerCtrl m_PlayerCtrl;

    // Start is called before the first frame update
    void Start()
    {
        m_PlayerCtrl = transform.parent.GetComponent<PlayerCtrl>(); 
    }

    void Event_RunAnim_L()
    {
        int Idx = Random.Range(1, 3);
        float Volume = SoundManager.Instance.GetDistVolume(m_PlayerCtrl.tr.position);
        SoundManager.Instance.PlayEffSound(string.Format($"RunL_{Idx}"), Volume);
    }
    void Event_RunAnim_R()
    {
        int Idx = Random.Range(1, 3);
        float Volume = SoundManager.Instance.GetDistVolume(m_PlayerCtrl.tr.position);
        SoundManager.Instance.PlayEffSound(string.Format($"RunR_{Idx}"), Volume);
    }

    void Event_HitAnim()
    {
        float Volume = SoundManager.Instance.GetDistVolume(m_PlayerCtrl.tr.position);
        SoundManager.Instance.PlayEffSound("Hit", Volume);
    }

    void Event_StartAnim()
    {
        if (m_PlayerCtrl.pv.IsMine == false)
            return;

        m_PlayerCtrl.m_CurStatus = PlayerState.HoldAction;
    }

    void Event_EndAnim()
    {
        if (m_PlayerCtrl.pv.IsMine == false)
            return;

        m_PlayerCtrl.m_CurStatus = PlayerState.Idle;
        m_PlayerCtrl.MySetAnim(AnimState.Idle);
    }
}
