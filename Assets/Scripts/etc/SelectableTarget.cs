using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectableTarget : MonoBehaviour, IPointerClickHandler
{
    bool isTargetable = false;
    public Outline targetableHighlight;

    public void SetTargetSelectable(bool v)
    {
        isTargetable = v;
    }
    public void EnableHighlight()
    {
        targetableHighlight.effectColor = Color.red;
        targetableHighlight.enabled = true;
    }
    public void DisableHighlight()
    {
        targetableHighlight.enabled = false;
    }
    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        if (!isTargetable || BattleManager.Instance.turn != 0) return;
        /*Debug.Log(gameObject.name);*/
        BattleManager.Instance.OnSelectTarget(gameObject);
    }
    public void ReadyToAttackHighlight()
    {
        targetableHighlight.effectColor = Color.green;
        targetableHighlight.enabled = true;
    }
}
