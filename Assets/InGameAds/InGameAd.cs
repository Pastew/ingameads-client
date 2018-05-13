using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameAd : MonoBehaviour
{
    private InGameAds inGameAds;
    private SpriteRenderer spriteRenderer;

    private float timeSeen = 0; // seconds
    AdVisibleObject adVisibleObject = null;

    private void Start()
    {
        UpdateAdTexture();
    }

    private void Update()
    {
        if (null != adVisibleObject)
            timeSeen += Time.deltaTime;
    }

    public void UpdateAdTexture()
    {
        inGameAds = FindObjectOfType<InGameAds>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Texture2D tex = (Texture2D)inGameAds.getAdTexture();

        if (null == tex)
            print("InGameAds has no texture downloaded");
        else {
            int x = (int)spriteRenderer.sprite.rect.size.x/2;
            int y = (int)spriteRenderer.sprite.rect.size.y/2;
            Texture2D scaledTexture = ScaleTexture(tex, x, y);
            spriteRenderer.sprite = Sprite.Create(scaledTexture, new Rect(0, 0, scaledTexture.width, scaledTexture.height), new Vector2(0, 0));
        }
    }

    // Found this method here: https://answers.unity.com/questions/1203440/resize-a-sprite-on-a-sprite-renderer.html
    private Texture2D ScaleTexture(Texture2D source, int targetWidth, int targetHeight)
    {
        Texture2D result = new Texture2D(targetWidth, targetHeight, source.format, true);
        Color[] rpixels = result.GetPixels(0);
        float incX = (1.0f / (float)targetWidth);
        float incY = (1.0f / (float)targetHeight);
        for (int px = 0; px < rpixels.Length; px++)
        {
            rpixels[px] = source.GetPixelBilinear(incX * ((float)px % targetWidth), incY * ((float)Mathf.Floor(px / targetWidth)));
        }
        result.SetPixels(rpixels, 0);
        result.Apply();
        return result;

    }

    void OnBecameVisible()
    {
        adVisibleObject = new AdVisibleObject
        {
            gameObjectName = gameObject.name,
            visibleStartTimestamp = GetEpochTimestamp()
        };

        timeSeen = 0;
    }

    void OnBecameInvisible()
    {
        adVisibleObject.visibleEndTimestamp = GetEpochTimestamp();
        inGameAds.SubmitAdVisibleObject(adVisibleObject);
    }

    private int GetEpochTimestamp()
    {
        return (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
    }
}