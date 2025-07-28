using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    ReferenceManager rm;
    [SerializeField] private GameObject popupPre;
    [SerializeField] private int poolSize = 5;
    Queue<GameObject> pool = new();


    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        rm = ReferenceManager.Instance;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(popupPre, rm.canvas.transform);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
    //Popup
    public GameObject GetPopup()
    {
        if (pool.Count > 0)
        {
            return pool.Dequeue();
        }
        else
        {
            GameObject obj = Instantiate(popupPre, rm.canvas.transform);
            obj.transform.position = Vector3.zero;
            obj.SetActive(false);
            return obj;
        }
    }
    public void ReturnPopup(GameObject popup)
    {
        popup.SetActive(false);
        pool.Enqueue(popup);
    }

    /*public void Show(GameObject popup, int value, Transform target)
    {
        StartCoroutine(ShowCoroutine(popup, value, target));
    }

    private IEnumerator ShowCoroutine(GameObject popup, int value, Transform target)
    {
        yield return null;
        Popup popupS = popup.GetComponent<Popup>();
        popupS.AddValue(value, target);
    }*/
}