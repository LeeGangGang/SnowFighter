using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlotCtrl : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // ���� ���Կ� �ִ� ��ų ������Ʈ
    public GameObject CurInObj;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CurInObj != null)
        {
            CurInObj.GetComponent<Image>().raycastTarget = false; // Event������ ����raycastTarget ����.
            CurInObj.transform.SetParent(GameObject.Find("Canvas").transform);
            SkillDragHandler.SelectSkill = CurInObj;    // ���� ���Կ� �ִ����� �巡�� �߱⿡
            SkillDragHandler.StartParent = transform;   // �̵��� ������ ��ġ(��ġ���� �Ǵ� �ǵ��ƿ��� ����)
        }
        else
            SkillDragHandler.SelectSkill = null;

    }

    public void OnDrag(PointerEventData eventData)
    {
        // ���Կ� �ִ� Skill�� �巡�� ������
        if (CurInObj != null)
            CurInObj.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (SkillDragHandler.SelectSkill != null)
            CurInObj = SkillDragHandler.SelectSkill;
        
        if (eventData.pointerEnter == null || eventData.pointerEnter.name.Contains("Slot") == false)
        {
            if (CurInObj != null)
            {
                // Slot�� �ƴ� �ٸ������� ������ ��
                CurInObj.transform.SetParent(GameObject.Find("SkillListContent").transform);
                CurInObj.transform.localPosition = Vector3.zero;
                CurInObj.GetComponent<Image>().raycastTarget = true;
                CurInObj = null;
            }
        }
    }


    public void OnDrop(PointerEventData eventData)
    {
        if (CurInObj != null && this.transform.name != SkillDragHandler.StartParent.name)
        {
            // �̵��� Slot�� ��ų�� �ִ� ��� ���� ��ġ ��ȯ�� ����
            GameObject temp = CurInObj; // �ӽ� �����
            CurInObj.transform.SetParent(SkillDragHandler.StartParent);
            CurInObj.transform.localPosition = Vector3.zero;
            SkillDragHandler.SelectSkill.transform.SetParent(this.transform);
            SkillDragHandler.SelectSkill.transform.localPosition = Vector3.zero;
            CurInObj = SkillDragHandler.SelectSkill;
            SkillDragHandler.SelectSkill = temp;

            // ���� ��ġ�� Skill�� ��Ƴ��� ���̶�� �ٽ� �̺�Ʈ ������ ���� Raycast�� Ų��.
            if (SkillDragHandler.StartParent.name.Contains("SkillListContent"))
                SkillDragHandler.SelectSkill.GetComponent<Image>().raycastTarget = true;
        }
        else
        {
            // ������� �̵����� ��
            if (CurInObj == null)
            {
                // ������ �ִ� Slot�� ��ų�� ������ �����ش�.
                if (eventData.pointerDrag.GetComponent<SkillSlotCtrl>() != null)
                    eventData.pointerDrag.GetComponent<SkillSlotCtrl>().CurInObj = null;
            }

            // �̵��� ��ų ����
            if (SkillDragHandler.SelectSkill != null)
            {
                CurInObj = SkillDragHandler.SelectSkill;
                CurInObj.transform.SetParent(this.transform);
                CurInObj.transform.localPosition = Vector3.zero;
                SkillDragHandler.SelectSkill = null; // �ش� �̺�Ʈ �Ϸ� �� OnEndDrag ���� ������ ����
            }
        }
    }
}
