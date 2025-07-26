using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ArrowManager : MonoBehaviour
{
    public static ArrowManager Instance;
    public GameObject arrow;
    public Canvas canvas;
    public GraphicRaycaster raycaster;
    public EventSystem eventSystem;
    RectTransform rt;
    ITarget iAttacker;

    Vector2 startPos = Vector2.zero;
    public bool drawable;
    public bool isDrawing;
    private bool isAttacking;
    public bool isSelecting;
    public float arrowOffset = 1.2f;

    ReferenceManager rm;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        rt = arrow.GetComponent<RectTransform>();

        rm = ReferenceManager.Instance;
    }
    void Update()
    {
        if (isSelecting)
        {
            arrow.SetActive(true);
            StartDrawing();
        }
        if (!drawable) return;
        if (Input.GetMouseButtonDown(0) && !rm.bm.isWaiting)
        {
            SetupDrawPosition();

        }
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            StartDrawing();
        }

        else if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            StopDrawing();

        }
    }

    public void SetupDrawPosition()
    {
        iAttacker = null;
        /*Debug.Log("SetupDrawPos");*/
        //Kiem tra va bat dau chon muc tieu tan cong
        PointerEventData pointerData = new(eventSystem)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new();
        raycaster.Raycast(pointerData, results);
        foreach (var result in results)
        {
            /*Debug.Log(result.gameObject.tag);*/
            if (!result.gameObject.CompareTag("Player") && !result.gameObject.CompareTag("Minion")) continue;

            ITarget target = null;
            if (result.gameObject.TryGetComponent(out CharacterDisplay character))
            {
                target = character.character;
            }
            else if (result.gameObject.TryGetComponent(out MinionDisplay mininon))
            {
                /*Debug.Log(rm.bm.playerMinionList.Contains(result.gameObject));*/
                if (!rm.bm.playerMinionList.Contains(result.gameObject)) continue;
                target = mininon.minion;
            }
            /*Debug.Log(target.CanAttack());*/
            if (target == null || !target.CanAttack()) continue;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, null, out startPos);
            rt.anchoredPosition = startPos;
            /*Debug.Log("UI Element hit: " + result.gameObject.name);*/
            iAttacker = target;
            isDrawing = true;
            isAttacking = true;
            rm.bm.isWaiting = true;
            rm.bm.ShowAttackableTarget();
            break;
        }

    }
    public void StartDrawing()
    {
        /*Debug.Log("Drawing");*/
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            Input.mousePosition,
            null,
            out Vector2 mousePos);

        Vector2 direction = mousePos - startPos;
        float distance = direction.magnitude * arrowOffset;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        RectTransform arrowRect = arrow.GetComponent<RectTransform>();

        arrowRect.sizeDelta = new Vector2(arrowRect.sizeDelta.x, distance);
        arrowRect.localEulerAngles = new Vector3(0, 0, angle - 90);
        arrow.SetActive(true);
    }
    public void StopDrawing()
    {
        isSelecting = false;
        isDrawing = false;
        arrow.SetActive(false);
        rm.bm.isWaiting = false;
        rm.bm.HideAttackableTarget();

        if (!isAttacking) return;
        isAttacking = false;
        //Kiem tra va thuc hien tan cong
        PointerEventData pointerData = new(eventSystem)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new();
        raycaster.Raycast(pointerData, results);
        foreach (var result in results)
        {
            if (!result.gameObject.CompareTag("Enemy") && !result.gameObject.CompareTag("Minion")) continue;
            ITarget target = null;
            if (result.gameObject.TryGetComponent(out CharacterDisplay character))
            {
                target = character.character;
            }
            else if (result.gameObject.TryGetComponent(out MinionDisplay minion))
            {
                if (!rm.bm.enemyMinionList.Contains(minion.gameObject)) continue;
                target = minion.minion;
            }
            if (target == null) continue;
            if (!rm.bm.CanAttackTarget(iAttacker, target)) return;
            //Do attack
            /*Debug.Log("Attacking");*/
            iAttacker.AttackTarget(target);
            return;
            /*break;*/
        }

    }

    //set up selecting a target
    public void ArrowForSelectingATarget(RectTransform rect)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, rect.position, null, out startPos);
        rt.anchoredPosition = startPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, null, out Vector2 mousePos);
        StartCoroutine(MoveArrowCoroutine(mousePos));
        //Stop drawing will be called in ExitSelectTarget (BattleManager.cs)
    }
    IEnumerator MoveArrowCoroutine(Vector2 targetPos)
    {
        float moveTime = 0.1f;
        float currentTime = 0f;
        arrow.SetActive(true);
        while (currentTime < moveTime)
        {
            Vector2 current = Vector2.Lerp(startPos, targetPos, currentTime / moveTime);
            currentTime += Time.deltaTime;
            Vector2 direction = current - startPos;
            float distance = direction.magnitude * arrowOffset;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            RectTransform arrowRect = arrow.GetComponent<RectTransform>();

            arrowRect.sizeDelta = new Vector2(arrowRect.sizeDelta.x, distance);
            arrowRect.localEulerAngles = new Vector3(0, 0, angle - 90);
            yield return null;
        }
        isSelecting = true;
    }

    ////////////////////////////
    // Enemy AI
    public void EnemyArrowForSelectingATarget(Card card, RectTransform rect, CardEffect.Target targetType)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, rect.position, null, out startPos);
        rt.anchoredPosition = startPos;
        GameObject targetObj = rm.ai.GetTargetByTargetType(card, targetType);
        MinionDisplay obj = targetObj.GetComponent<MinionDisplay>();
        Vector2 screenPoint;
        if (obj == null)
        {
            screenPoint = RectTransformUtility.WorldToScreenPoint(null, targetObj.GetComponent<RectTransform>().position);
        }
        else
            screenPoint = RectTransformUtility.WorldToScreenPoint(null, targetObj.GetComponent<RectTransform>().position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPoint, null, out Vector2 endPos);
        StartCoroutine(EnemyMoveArrowCoroutine(rect, endPos, targetObj));
        /*Debug.Log(targetObj.name);*/
        /*Debug.Log(endPos);*/
        /*isSelecting = true;*/
        //Stop drawing will be called in ExitSelectTarget (BattleManager.cs)
    }
    IEnumerator EnemyMoveArrowCoroutine(RectTransform rect, Vector2 targetPos, GameObject targetObj)
    {
        float moveTime = 0.5f;
        float currentTime = 0f;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,
            rect.position,
            null, out Vector2 startPos);
        while (currentTime < moveTime)
        {
            Vector2 current = Vector2.Lerp(startPos, targetPos, currentTime / moveTime);
            EnemyStartDrawing(current);
            currentTime += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        rm.bm.OnSelectTarget(targetObj);
    }
    public void EnemyStartDrawing(Vector2 endPos)
    {
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude * arrowOffset;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        RectTransform arrowRect = arrow.GetComponent<RectTransform>();

        arrowRect.sizeDelta = new Vector2(arrowRect.sizeDelta.x, distance);
        arrowRect.localEulerAngles = new Vector3(0, 0, angle - 90);
        arrow.SetActive(true);
    }







    ////////////////////Helper function///////////////////

}