using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public SkillData.SkillType Type;

    // Drag�� �ϳ��� �����Ͽ� static���� ����
    public static Transform StartParent;    // �巡�� �� Skill�� �巡�� ���� ��ġ
    public static GameObject SelectSkill;   // �巡�� �� Skill�� ������Ʈ

    private ScrollRect ParentSR; // ScrollView ������ ����
    private Rect RectSR;
    private GameObject Canvas;

    private Text InfoTxt;
    private Text ExplainTxt;

    bool IsMvItem = false; // true : �ٸ� ������ �ش� Skill�� �巡�� �Ҷ�

    public void OnBeginDrag(PointerEventData eventData)
    {
        SetRectSR();
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
        // ScrollView�� ���� ���۽�Ű�� ���ؼ�
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
        InfoTxt = GameObject.Find("InfoTxt").GetComponent<Text>();
        ExplainTxt = GameObject.Find("ExpainTxt").GetComponent<Text>();
    }

    public void SetRectSR()
    {
        Vector2 Size = ParentSR.GetComponent<RectTransform>().sizeDelta;
        Vector2 LTPos = new Vector2(ParentSR.transform.position.x - Size.x / 2f, ParentSR.transform.position.y - Size.y / 2f);
        RectSR = new Rect(LTPos, Size);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InfoTxt.text = SkillData.InfoTxt(Type);
        ExplainTxt.text = SkillData.ExplainTxt(Type);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InfoTxt.text = string.Empty;
        ExplainTxt.text = string.Empty;
    }
}
