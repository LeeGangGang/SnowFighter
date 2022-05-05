using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillSlotCtrl : MonoBehaviour, IDropHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // 현재 슬롯에 있는 스킬 오브젝트
    public GameObject CurInObj;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (CurInObj != null)
        {
            CurInObj.GetComponent<Image>().raycastTarget = false; // Event동작을 위해raycastTarget 끈다.
            CurInObj.transform.SetParent(GameObject.Find("Canvas").transform);
            SkillDragHandler.SelectSkill = CurInObj;    // 현재 슬롯에 있던것을 드래그 했기에
            SkillDragHandler.StartParent = transform;   // 이동을 시작한 위치(위치변경 또는 되돌아오기 위해)
        }
        else
            SkillDragHandler.SelectSkill = null;

    }

    public void OnDrag(PointerEventData eventData)
    {
        // 슬롯에 있던 Skill을 드래그 했을때
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
                // Slot이 아닌 다른곳으로 던졌을 때
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
            // 이동할 Slot에 스킬이 있는 경우 서로 위치 교환을 위해
            GameObject temp = CurInObj; // 임시 저장소
            CurInObj.transform.SetParent(SkillDragHandler.StartParent);
            CurInObj.transform.localPosition = Vector3.zero;
            SkillDragHandler.SelectSkill.transform.SetParent(this.transform);
            SkillDragHandler.SelectSkill.transform.localPosition = Vector3.zero;
            CurInObj = SkillDragHandler.SelectSkill;
            SkillDragHandler.SelectSkill = temp;

            // 시작 위치가 Skill을 모아놓은 곳이라면 다시 이벤트 적용을 위해 Raycast를 킨다.
            if (SkillDragHandler.StartParent.name.Contains("SkillListContent"))
                SkillDragHandler.SelectSkill.GetComponent<Image>().raycastTarget = true;
        }
        else
        {
            // 빈곳으로 이동했을 때
            if (CurInObj == null)
            {
                // 이전에 있던 Slot의 스킬의 흔적을 지워준다.
                if (eventData.pointerDrag.GetComponent<SkillSlotCtrl>() != null)
                    eventData.pointerDrag.GetComponent<SkillSlotCtrl>().CurInObj = null;
            }

            // 이동한 스킬 적용
            if (SkillDragHandler.SelectSkill != null)
            {
                CurInObj = SkillDragHandler.SelectSkill;
                CurInObj.transform.SetParent(this.transform);
                CurInObj.transform.localPosition = Vector3.zero;
                SkillDragHandler.SelectSkill = null; // 해당 이벤트 완료 후 OnEndDrag 동작 제한을 위해
            }
        }
    }
}
