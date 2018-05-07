using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InGameAds : MonoBehaviour {

    public string applicationName;

    private readonly int AD_VISIBLE_OBJECT_LIST_MAX_SIZE = 5; // How many times user will see advert to uploading stats to server
    private readonly string STATS_SERVER_URL = "http://localhost:7171/advert/";
    private readonly string IMAGE_PROVIDER_SERVER_URL = "http://localhost:7070/advert/";

    private string imageUrl;
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
        imageUrl = www.text;
        if (www.text.Contains("\"status\":404"))
        {
            print("Something gone wrong, can't download imageUrl for game " + applicationName);
            yield return "failed";
        }

        if (null == imageUrl)
        {
            print("Something gone wrong, can't download imageUrl for game " + applicationName);
            yield return "failed";
        }

        if ("default" == imageUrl)
        {
            print("This game have no current advert rented, I will leave the default sprite.");
            yield return "failed";
        }

        print("Got imageUrl from Image Provider: " + imageUrl);
        yield return new WaitForSeconds(2);
        www = new WWW(imageUrl);
        yield return www;
        adTexture = www.texture;

        /*
        SpriteRenderer sr = gameObject.GetComponent<SpriteRenderer>();
        sr.sprite = Sprite.Create(www.texture, new Rect(0, 0, www.texture.width, www.texture.height), new Vector2(0, 0));
        */
        foreach (InGameAd ad in FindObjectsOfType<InGameAd>())
            ad.UpdateAdTexture();
    }

    internal void SubmitAdVisibleObject(AdVisibleObject adVisibleObject)
    {
        adVisibleObjectList.Add(adVisibleObject);
        if (adVisibleObjectList.Count > AD_VISIBLE_OBJECT_LIST_MAX_SIZE)
            UploadToServerAdVisibleObjectList();
    }

    void OnApplicationQuit()
    {
        UploadToServerAdVisibleObjectList();
    }

    private void UploadToServerAdVisibleObjectList()
    {
        string json = JsonUtility.ToJson(adVisibleObjectList);
        adVisibleObjectList.Clear();
        StartCoroutine(UploadJsonToServerCoroutine(json));
    }

    IEnumerator UploadJsonToServerCoroutine(string json)
    {
        UnityWebRequest www = UnityWebRequest.Post(STATS_SERVER_URL, json);
        www.SetRequestHeader("Accept", "application/json");
        yield return www.SendWebRequest();
    }
}
