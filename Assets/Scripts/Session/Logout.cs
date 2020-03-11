using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Logout : MonoBehaviour
{
    public Player player;

    public delegate void LogoutAction(string message);
    public static event LogoutAction OnLogout;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<Player>();
    }


    public void OnLogoutButtonClicked()
    {
        TryLogout();
        StartCoroutine(DeleteOnline());
    }

    private void TryLogout()
    {
        UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "api/Account/Logout", "POST");
        httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);
        httpClient.SendWebRequest();
        while (!httpClient.isDone)
        {
            Task.Delay(1);
        }

        if (httpClient.isNetworkError || httpClient.isHttpError)
        {
            throw new Exception("Login > TryLogout: " + httpClient.error);
        }
        else
        {
            //if (OnLogout != null)
            //{
            //    OnLogout();
            //}
            OnLogout?.Invoke("" + httpClient.responseCode);
            player.Token = string.Empty;
            player.Id = string.Empty;
            player.Email = string.Empty;
            player.Name = string.Empty;
            player.BirthDay = DateTime.MinValue;
        }
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
