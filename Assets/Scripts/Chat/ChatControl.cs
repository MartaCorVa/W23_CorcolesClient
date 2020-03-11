using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChatControl : MonoBehaviour
{
    private Player player;

    public GameObject chatColumn;
    public InputField messageContent;
    public Text messageText;
    public Text nickName;
    public Button sendButton;

    public GameObject bannedRegion;
    public GameObject titleScreen;

    void Start()
    {
        player = FindObjectOfType<Player>();
        ChargeNickName();
        StartCoroutine(InsertOnline());
        StartCoroutine(ExecuteRefresh());
        Debug.Log(player.State);
    }

    void Update()
    {
        if (("banned").Equals(player.State))
        {
            titleScreen.SetActive(false);
            bannedRegion.SetActive(true);
            messageContent.enabled = false;
            sendButton.enabled = false;
        }
    }

    private void ChargeNickName()
    {
        nickName.text = player.Name;
    }

    private IEnumerator InsertOnline()
    {
        OnlineSerializable online = new OnlineSerializable();
        online.IdPlayer = player.Id;

        using(UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Online/InsertOnline", "POST"))
        {
            string playerData = JsonUtility.ToJson(online);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if(httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("InsertOnline > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("InsertOnline > Info: " + httpClient.responseCode);
            }
        }

    }

    private IEnumerator InsertMessage()
    {
        MessageSerializable messageSerializable = new MessageSerializable();
        messageSerializable.IdPlayer = player.Id;
        messageSerializable.Content = messageContent.text;

        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Message/InsertNewMessage", "POST"))
        {
            string playerData = JsonUtility.ToJson(messageSerializable);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(playerData);
            httpClient.uploadHandler = new UploadHandlerRaw(bodyRaw);
            httpClient.downloadHandler = new DownloadHandlerBuffer();

            httpClient.SetRequestHeader("Content-type", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("InsertMessage > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("InsertMessage > Info: " + httpClient.responseCode);
            }
        }
        messageContent.text = "";
    }

    public void OnClickSendMessage()
    {
        if (("admin@fmail.com").Equals(player.Email) && messageContent.text.Contains("ban"))
        {
            StartCoroutine(BanPlayer());
        } 
        else
        {
            StartCoroutine(InsertMessage());
        }
    }

    private IEnumerator BanPlayer()
    {
        string userBanned = messageContent.text.Substring(4);
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Player/BanPlayer/" + userBanned, "GET"))
        {
            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("BanPlayer > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("BanPlayer > Info: " + httpClient.responseCode);
            }
        }
    }

    private IEnumerator ExecuteRefresh()
    {
        while (true)
        {
            DeleteChilds();
            yield return GetMessages();
            yield return new WaitForSeconds(5f);
            yield return Helper.GetPlayerInfo();
        }
    }

    private IEnumerator GetMessages()
    {
        using (UnityWebRequest httpClient = new UnityWebRequest(player.HttpServerAddress + "/api/Message/GetMessages", "GET"))
        {
            httpClient.SetRequestHeader("Accept", "application/json");
            httpClient.SetRequestHeader("Authorization", "bearer " + player.Token);

            httpClient.downloadHandler = new DownloadHandlerBuffer();

            yield return httpClient.SendWebRequest();

            if (httpClient.isNetworkError || httpClient.isHttpError)
            {
                throw new System.Exception("GetMessages > Error: " + httpClient.responseCode + ", Info: " + httpClient.error);
            }
            else
            {
                Debug.Log("GetMessages > Info: " + httpClient.responseCode);

                string jsonResponse = httpClient.downloadHandler.text;
                string response = "{\"listOfMessages\":" + jsonResponse + "}";
                ListMessageSerializable list = JsonUtility.FromJson<ListMessageSerializable>(response);

                foreach (MessageSerializable message in list.listOfMessages)
                {
                    var newMessage = Instantiate(messageText, Vector3.zero, Quaternion.identity) as Text;
                    newMessage.transform.GetComponent<Text>().text = message.IdPlayer.Substring(0, 3) + " > " + message.Content;
                    newMessage.transform.SetParent(chatColumn.transform);
                }

            }
        }
    }

    private void DeleteChilds()
    {
        int childs = chatColumn.transform.childCount;
        GameObject child;

        for (int i = 0; i < childs; i++)
        {
            child = chatColumn.transform.GetChild(i).gameObject;
            Destroy(child);
        }

    }

}
