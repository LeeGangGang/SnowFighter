using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DamageTxtCtrl : MonoBehaviour
{
    [HideInInspector] public Text m_RefText = null;
    [HideInInspector] public float m_DamageVal = 0.0f;

    Animator m_RefAnimator = null;  //지금은 이게 " RootModel " 이기도 하다.

    RectTransform CanvasRect;
    Vector2 screenPos = Vector2.zero;
    Vector2 WdScPos = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        m_RefText = this.gameObject.GetComponentInChildren<Text>();
        if (m_RefText != null)
            m_RefText.text = "-" + m_DamageVal.ToString();

        m_RefAnimator = GetComponentInChildren<Animator>();
        if (m_RefAnimator != null)
        {
            AnimatorStateInfo animaterStateInfo = m_RefAnimator.GetCurrentAnimatorStateInfo(0);
            float a_LifeTime = animaterStateInfo.length;
            Destroy(gameObject, a_LifeTime);
        }
    }
}
