using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameAds : MonoBehaviour {

    private string ingameadsImageProvider = "http://localhost:7070/advert/";
    public string applicationName;

    private string imageUrl;
    private Texture adTexture;


    private void Start()
    {
        DownloadAdImageUrl();
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
        WWW www = new WWW(ingameadsImageProvider + applicationName);
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

}
