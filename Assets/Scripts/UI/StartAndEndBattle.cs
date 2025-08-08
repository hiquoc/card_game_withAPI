using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Sequence = DG.Tweening.Sequence;

public class StartAndEndBattle : MonoBehaviour
{
    public IEnumerator PlayStartBattle()
    {
        ReferenceManager rm=ReferenceManager.Instance;
        rm.sm.Play("start");
        RectTransform blurPanel = rm.blurPanel;
        blurPanel.gameObject.SetActive(true);
        Transform startImage = blurPanel.transform.GetChild(0);
        startImage.localScale = Vector3.zero;
        startImage.gameObject.SetActive(true);
        blurPanel.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);
        yield return startImage.DOScale(Vector3.one, 0.5f).WaitForCompletion();
        yield return new WaitForSeconds(2f);
        yield return startImage.DOScale(Vector3.zero, 0.2f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        startImage.gameObject.SetActive(false);
        blurPanel.GetComponent<Image>().raycastTarget=false;
        blurPanel.gameObject.SetActive(false);
        rm.sm.PlayLoop("battleMusic");
    }
    public void PlayEndBattle(bool isWin)
    {
        StartCoroutine(PlayeEndBattleCoroutine(isWin));
    }
    public IEnumerator PlayeEndBattleCoroutine(bool isWin)
    {
        ReferenceManager.Instance.sm.audioSource.Stop();
        RectTransform endGamePanel = ReferenceManager.Instance.endGamePanel;
        endGamePanel.localScale = Vector3.zero;
        endGamePanel.gameObject.SetActive(true);
        if (SceneLoader.Instance.selectedStage != 3 && isWin)
        {
            Transform nextStageBtn = endGamePanel.transform.Find("NextStageBtn");
            if(nextStageBtn != null)
                nextStageBtn.gameObject.SetActive(true);
        }
        if (BattleManager.Instance.player.GetHealth() <= 0 && BattleManager.Instance.enemy.GetHealth() <= 0)
        {
            ReferenceManager.Instance.sm.Play("lose");
            endGamePanel.gameObject.transform.Find("drawImg").gameObject.SetActive(true);
        }
        else
        {
            if (isWin)
            {
                UpdateStage();
                ReferenceManager.Instance.sm.Play("win");
                endGamePanel.gameObject.transform.Find("winImg").gameObject.SetActive(true);
            }
                
            else
            {
                ReferenceManager.Instance.sm.Play("lose");
                endGamePanel.gameObject.transform.Find("loseImg").gameObject.SetActive(true);
            }               
        }
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        seq.Append(endGamePanel.DOScale(Vector3.one, 0.5f));

        yield return seq.WaitForCompletion();
    }
    public void PlayDraw()
    {        
        StartCoroutine(PlayDrawBattleCoroutine());
    }
    public IEnumerator PlayDrawBattleCoroutine()
    {
        ReferenceManager.Instance.sm.Play("lose");
        RectTransform endGamePanel = ReferenceManager.Instance.endGamePanel;
        endGamePanel.localScale = Vector3.zero;
        endGamePanel.gameObject.SetActive(true);
        endGamePanel.gameObject.transform.Find("drawImg").gameObject.SetActive(true);

        DG.Tweening.Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        seq.Append(endGamePanel.DOScale(Vector3.one, 0.5f));

        yield return seq.WaitForCompletion();
    }
    void UpdateStage()
    {
        SceneLoader sceneLoader = SceneLoader.Instance;
        /*Debug.Log(sceneLoader.selectedStage);
        Debug.Log(sceneLoader.finishedStage);*/
        if (sceneLoader.selectedStage > sceneLoader.finishedStage)
        {
            sceneLoader.finishedStage = sceneLoader.selectedStage;
            StartCoroutine(UpdateStageCoroutine());
        }
    }

    public void ShowRewardPanel()
    {
        Transform rewardPanel = ReferenceManager.Instance.endGamePanel.transform.Find("RewardPanel");
        GameObject rewardCardObj = rewardPanel.Find("RewardCard").gameObject;
        StartCoroutine(LoadRewardCard(CardManager.Instance.stageRewardId[SceneLoader.Instance.selectedStage-1], rewardCardObj));
        rewardPanel.localScale = Vector3.zero;
        rewardPanel.gameObject.SetActive(true);
        rewardPanel.DOScale(1f, 1f);
    }


    IEnumerator UpdateStageCoroutine()
    {
        string updateStageUrl = DataFetcher.address + "users/game/" + SceneLoader.Instance.finishedStage;
        /*Debug.Log(updateStageUrl);*/
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes("{}");
        using UnityWebRequest request=UnityWebRequest.Put(updateStageUrl,bodyRaw);
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error updating stage: " + request.error);
            CardManager cm=CardManager.Instance;
            if (request.responseCode == 401)
            {
                cm.ShowErrorPanel("Expired session\nPlease re-login");
                Debug.LogWarning("Access forbidden (401). Possibly due to an invalid or expired token.");
                SceneLoader.Instance.BackToMenuAndLogout();
            }
            else if (!cm.errorPanel.activeInHierarchy)
            {
                cm.ShowErrorPanel();
            }
            yield break;
        }
    }
    IEnumerator LoadRewardCard(int cardId,GameObject cardObj) {       
        
        string getCardUrl = $"{DataFetcher.address}cards/{cardId}";
        using UnityWebRequest request = UnityWebRequest.Get(getCardUrl);
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error giving reward: " + request.error);
            if (request.responseCode == 401)
            {
                CardManager.Instance.ShowErrorPanel("Expired session\nPlease re-login");
                Debug.LogWarning("Access forbidden (401). Possibly due to an invalid or expired token.");
                SceneLoader.Instance.BackToMenuAndLogout();
            }
            else if (!CardManager.Instance.errorPanel.activeInHierarchy)
            {
                CardManager.Instance.ShowErrorPanel();
            }
            yield break;
        }
        SingleCardResult response = JsonConvert.DeserializeObject<SingleCardResult>(request.downloadHandler.text);
        if (response?.data == null)
        {
            Debug.LogWarning("Dữ liệu data null hoặc parse lỗi ở get card data.");
            yield break;
        }
        GameObject manaObj = cardObj.transform.Find("ManaText").gameObject;
        manaObj.GetComponent<TMP_Text>().text=response.data.mana.ToString();
        if (response.data.type == "MINION")
        {
            GameObject AttackObj = cardObj.transform.Find("AttackImg").gameObject;
            AttackObj.GetComponentInChildren<TMP_Text>().text = response.data.attack.ToString();
            AttackObj.SetActive(true);

            GameObject HealthObj = cardObj.transform.Find("HealthImg").gameObject;
            HealthObj.GetComponentInChildren<TMP_Text>().text = response.data.health.ToString();
            HealthObj.SetActive(true);
        }

        using UnityWebRequest requestImg = UnityWebRequestTexture.GetTexture(response.data.mainImg);
        yield return requestImg.SendWebRequest();

        if (requestImg.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + requestImg.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(requestImg);
            Texture2D cropped = ReferenceManager.Instance.cm.CropTransparent(texture);

            Sprite sprite = Sprite.Create(cropped, new Rect(0, 0, cropped.width, cropped.height), new Vector2(0.5f, 0.5f), 1f);
            cardObj.GetComponent<Image>().sprite = sprite;
        }

    }

    [Serializable]
    public class SingleCardResult
    {
        public int code;
        public string message;
        public SingleCardData data;
    }
    [Serializable]
    public class SingleCardData
    {
        public int cardId;
        public string name;
        public string type;
        public string rarity;
        public int mana;
        public int attack;
        public int health;
        public string description;
        public string? animationId;
        public string imageUrl;
        public string mainImg;
        public string createdAt;
        public string updatedAt;
        public object effectBindings;
    }
}
