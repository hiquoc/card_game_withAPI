using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class LoginHandler : MonoBehaviour
{
    public TMP_InputField  username, password;
    public GameObject resultPanel, loginPanel, loginBtn, logoutBtn,signupBtn, userBtn;
    public TMP_Text resultTxt;
    string loginUrl = DataFetcher.address + "auth/login";


    public void LoginBtnClicked()
    {
        string cleanUsername = username.text.Replace("\u200B", "").Trim();
        string cleanPassword = password.text.Replace("\u200B", "").Trim();

        StartCoroutine(LoginCoroutine(cleanUsername, cleanPassword));
    }
    public IEnumerator LoginCoroutine(string username, string password)
    {
        string jsonBody = JsonUtility.ToJson(new LoginRequest(username, password));
        /*Debug.Log("JSON Body: " + jsonBody);*/
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);

        UnityWebRequest request = new(loginUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            /*Debug.Log("Login success: " + request.downloadHandler.text);*/

            LoginResult response = JsonUtility.FromJson<LoginResult>(request.downloadHandler.text);
            SceneLoader.Instance.token = response.data;
            SceneLoader.Instance.username = username;
            Debug.Log("Received Token: " + response.data);

            resultPanel.SetActive(true);
            resultTxt.text = "Login successfully!";
            yield return new WaitForSeconds(2f);
            loginPanel.SetActive(false);
            loginBtn.SetActive(false);
            signupBtn.SetActive(false);
            logoutBtn.SetActive(true);
            userBtn.SetActive(true);
            TMP_Text userTxt = userBtn.GetComponentInChildren<TMP_Text>();
            userTxt.text = username;

        }
        else
        {
            Debug.Log($"Sending login with: {username}, {password}");

            Debug.LogError("Login failed: " + request.error);
            Debug.LogError("Response code: " + request.responseCode);
            Debug.LogError("Response text: " + request.downloadHandler.text);
            Debug.Log(request.result);
            Debug.Log("Login URL: " + loginUrl);
            Debug.LogError("Login failed: " + request.error);
            resultPanel.SetActive(true);
            resultTxt.text = "Incorrect username or password!";
            yield return new WaitForSeconds(2f);
            resultPanel.SetActive(false);
        }
    }


    [System.Serializable]
    public class LoginRequest
    {
        public string username;
        public string password;

        public LoginRequest(string u, string p)
        {
            username = u;
            password = p;
        }
    }
    [System.Serializable]
    public class LoginResult
    {
        public int code;
        public string message;
        public string data;
    }

    

}