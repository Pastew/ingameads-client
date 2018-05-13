using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InGameAds : MonoBehaviour {

    public string applicationName;

    private readonly int AD_VISIBLE_OBJECT_LIST_MAX_SIZE = 2; // How many times user will see advert to uploading stats to server
    private readonly string STATS_SERVER_URL = "http://localhost:7171/upload_stats/";
    private readonly string IMAGE_PROVIDER_SERVER_URL = "http://localhost:7070/advert/";

    private Advert advert;
    private Texture adTexture;
    private List<AdVisibleObject> adVisibleObjectList;

    private void Start()
    {
        DownloadAdImageUrl();
        adVisibleObjectList = new List<AdVisibleObject>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            DownloadAdImageUrl();
    }

    internal Texture getAdTexture()
    {
        return adTexture;
    }

    private void DownloadAdImageUrl()
    {
        StartCoroutine(DownloadAdImageCoroutine());
    }

    IEnumerator DownloadAdImageCoroutine()
    {
        WWW www = new WWW(IMAGE_PROVIDER_SERVER_URL + applicationName);
        yield return www;
        Advert advert = JsonUtility.FromJson<Advert>(www.text);
        this.advert = advert;

        if (null == advert.imageUrl)
        {
            print("Something gone wrong, can't download imageUrl for game " + applicationName);
            yield return "failed";
        }
        else if (advert.imageUrl.Contains("\"status\":404"))
        {
            print("Something gone wrong, can't download imageUrl for game " + applicationName);
            yield return "failed";
        }
        else if ("default".Equals(advert.imageUrl))
        {
            print("This game have no current advert rented, I will leave the default sprite.");
            yield return "failed";
        }
        else
        {
            print("Got imageUrl from Image Provider: " + advert.imageUrl);
            yield return new WaitForSeconds(2);
            www = new WWW(advert.imageUrl);
            yield return www;
            adTexture = www.texture;

            foreach (InGameAd ad in FindObjectsOfType<InGameAd>())
                ad.UpdateAdTexture();
        }
    }

    internal string GetAdvertId()
    {
        if (null == advert)
            return "0";
        else
            return advert.id;
    }

    internal void SubmitAdVisibleObject(AdVisibleObject adVisibleObject)
    {
        adVisibleObjectList.Add(adVisibleObject);
        if (adVisibleObjectList.Count >= AD_VISIBLE_OBJECT_LIST_MAX_SIZE)
            UploadToServerAdVisibleObjectList();
        else
            print((AD_VISIBLE_OBJECT_LIST_MAX_SIZE - adVisibleObjectList.Count) + " more to upload");
    }

    void OnApplicationQuit()
    {
        UploadToServerAdVisibleObjectList();
    }

    private void UploadToServerAdVisibleObjectList()
    {
        string json = "[";
        foreach (AdVisibleObject a in adVisibleObjectList)
        {
            a.advertId = advert.id;
            json += JsonUtility.ToJson(a) + ",";
        }
        json = json.Remove(json.Length - 1);
        json += "]";

        adVisibleObjectList.Clear();
        StartCoroutine(postRequest(STATS_SERVER_URL + applicationName, json));
    }

    IEnumerator UploadJsonToServerCoroutine(string json)
    {
        print("UploadJsonToServerCoroutine started");
        byte[] postData = System.Text.Encoding.UTF8.GetBytes(json);

        UnityWebRequest www = UnityWebRequest.Put(STATS_SERVER_URL + applicationName, postData);
        www.SetRequestHeader("Accept", "application/json");

        yield return www.SendWebRequest();

        if(www.isNetworkError)
                Debug.Log("Error While Sending: " + www.error);
        else
                Debug.Log("Received: " + www.downloadHandler.text);
    }

    IEnumerator postRequest(string url, string json)
    {
        var uwr = new UnityWebRequest(url, "POST");
        byte[] jsonToSend = new System.Text.UTF8Encoding().GetBytes(json);
        uwr.uploadHandler = (UploadHandler)new UploadHandlerRaw(jsonToSend);
        uwr.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        uwr.SetRequestHeader("Content-Type", "application/json");

        //Send the request then wait here until it returns
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            Debug.Log("Error While Sending: " + uwr.error);
        }
        else
        {
            Debug.Log("Received: " + uwr.downloadHandler.text);
        }
    }
}
