using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class GoToMainMenu : MonoBehaviour
{
    private Player player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
    }

    public void OnGoToMainMenuButtonClicked()
    {
        StartCoroutine(DeleteOnline());
        SceneManager.LoadScene(0);
    }

    private IEnumerator DeleteOnline()
    {
        OnlineSerializable online = new OnlineSerializable();
        online.IdPlayer = player.Id;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Online/DeleteOnline", "POST"))
        {
            string playerData = JsonUtility.ToJson(online);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("DeleteOnline > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("DeleteOnline > Info: " + httpClient.responseCode);
            }
        }
    }
}
