using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SkillType Type;

    // Drag�� �ϳ��� �����Ͽ� static���� ����
    public static Transform StartParent;    // �巡�� �� Skill�� �巡�� ���� ��ġ
    public static GameObject SelectSkill;   // �巡�� �� Skill�� ������Ʈ

    private ScrollRect ParentSR; // ScrollView ������ ����
    private Rect RectSR;
    private GameObject Canvas;

    bool IsMvItem = false; // true : �ٸ� ������ �ش� Skill�� �巡�� �Ҷ�

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
            // Item�� Drag �� ��� ScrollView ������ �ߴ��ϱ� ����
            ParentSR.OnEndDrag(eventData);
        }

        if (IsMvItem)
            this.transform.position = eventData.position;
        // ScrollView�� ���� ���۽�Ű�� ���ػ�
        ParentSR.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // ScrollView�� ���� ���۽�Ű�� ����
        ParentSR.OnEndDrag(eventData);

        IsMvItem = false;
        // ���� ������ �ƴ� ���ÿ� List�ʿ��� ������ �߸� �巡�� ������ ����ġ �ϱ�����
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
