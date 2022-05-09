using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SkillType Type;

    // Drag는 하나만 가능하여 static으로 관리
    public static Transform StartParent;    // 드래그 한 Skill의 드래그 시점 위치
    public static GameObject SelectSkill;   // 드래그 한 Skill의 오브젝트

    private ScrollRect ParentSR; // ScrollView 동작을 위해
    private Rect RectSR;
    private GameObject Canvas;

    bool IsMvItem = false; // true : 다른 곳으로 해당 Skill을 드래그 할때

    public void OnBeginDrag(PointerEventData eventData)
    {
        ParentSR.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!RectSR.Contains(eventData.position))
        {
            if (IsMvItem == false)
            {
                IsMvItem = true;
                StartParent = transform.parent;
                SelectSkill = this.gameObject;
                this.GetComponent<Image>().raycastTarget = false;
                this.transform.SetParent(Canvas.transform);
            }
            // Item을 Drag 할 경우 ScrollView 동작은 중단하기 위해
            ParentSR.OnEndDrag(eventData);
        }

        if (IsMvItem)
            this.transform.position = eventData.position;
        // ScrollView도 같이 동작시키기 위해사
        ParentSR.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // ScrollView도 같이 동작시키기 위해
        ParentSR.OnEndDrag(eventData);

        IsMvItem = false;
        // 장착 슬롯이 아닌 선택용 List쪽에서 밖으로 잘못 드래그 했을때 원위치 하기위해
        if (eventData.pointerEnter == null || eventData.pointerEnter.name.Contains("Slot_") == false)
        {
            this.transform.SetParent(GameObject.Find("SkillListContent").transform);
            //this.transform.localPosition = Vector3.zero;
            this.GetComponent<Image>().raycastTarget = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Canvas = GameObject.Find("Canvas");
        ParentSR = GameObject.Find("SkillSvObj").GetComponent<ScrollRect>();
        Vector2 Size = ParentSR.GetComponent<RectTransform>().sizeDelta;
        Vector2 LTPos = new Vector2(990f - Size.x / 2f, ParentSR.transform.position.y - Size.y / 2f);
        RectSR = new Rect(LTPos, Size);
    }
}
