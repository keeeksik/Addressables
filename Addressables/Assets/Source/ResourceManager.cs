using System;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro; // **днаюбкемн**

public class ResourceManager : MonoBehaviour
{
    [Serializable]
    public class ResourceData
    {
        public string Key;
        public string Url;
        public ResourceType Type;
    }

    public enum ResourceType
    {
        Model,
        Image,
        Audio,
        Text
    }

    public List<ResourceData> resourcesToLoad = new List<ResourceData>();

    [Header("UI Elements")]
    public Transform buttonContainer;
    public GameObject buttonPrefab;
    public Image loadedImage;
    public AudioSource audioSource;
    public TMP_Text loadedText; // **хглемемн**
    public Transform modelContainer;

    private Dictionary<string, object> loadedResources = new Dictionary<string, object>();


    void Start()
    {
        CreateLoadButtons();
    }

    private void CreateLoadButtons()
    {
        foreach (var resourceData in resourcesToLoad)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, buttonContainer);
            Button button = buttonGO.GetComponent<Button>();
            TMP_Text buttonText = buttonGO.GetComponentInChildren<TMP_Text>(); // **хглемемн**

            if (button != null && buttonText != null)
            {
                buttonText.text = $"Load {resourceData.Key}";
                string key = resourceData.Key; // capture key to avoid the closure problem
                button.onClick.AddListener(() => StartCoroutine(LoadResource(key)));
            }
            else
            {
                Debug.LogError("Button prefab missing Button or Text component.");
            }
        }
    }

    public IEnumerator LoadResource(string key)
    {
        ResourceData resourceData = resourcesToLoad.Find(r => r.Key == key);

        if (resourceData == null)
        {
            Debug.LogError($"Resource with key '{key}' not found in the configuration.");
            yield break;
        }

        if (loadedResources.ContainsKey(key))
        {
            Debug.LogWarning($"Resource with key '{key}' already loaded. Skipping load. Attempting to show the existing loaded resource.");
            DisplayLoadedResource(key);
            yield break;
        }

        UnityWebRequest www = null;

        if (resourceData.Type == ResourceType.Image)
        {
            www = UnityWebRequestTexture.GetTexture(resourceData.Url);
        }
        else if (resourceData.Type == ResourceType.Model)
        {
            Debug.LogError("Loading Models from URL is not supported directly. Please use Asset Bundles");
            yield break;
        }
        else if (resourceData.Type == ResourceType.Audio)
        {
            www = UnityWebRequestMultimedia.GetAudioClip(resourceData.Url, AudioType.OGGVORBIS);
        }
        else if (resourceData.Type == ResourceType.Text)
        {
            www = UnityWebRequest.Get(resourceData.Url);
        }

        if (www == null)
        {
            Debug.LogError($"Failed to create UnityWebRequest for resource '{key}'. Check the ResourceType.");
            yield break;
        }

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Error loading resource '{key}': {www.error}");
            yield break;
        }

        object loadedObject = null;

        switch (resourceData.Type)
        {
            case ResourceType.Image:
                loadedObject = DownloadHandlerTexture.GetContent(www);
                break;
            case ResourceType.Model:
                break;
            case ResourceType.Audio:
                loadedObject = ((DownloadHandlerAudioClip)www.downloadHandler).audioClip;
                break;
            case ResourceType.Text:
                loadedObject = www.downloadHandler.text;
                break;
        }

        if (loadedObject != null)
        {
            loadedResources.Add(key, loadedObject);
            DisplayLoadedResource(key);
            Debug.Log($"Resource '{key}' loaded successfully.");
        }
        else
        {
            Debug.LogError($"Failed to load or process resource '{key}'.");
        }
    }


    private void DisplayLoadedResource(string key)
    {
        ResourceData resourceData = resourcesToLoad.Find(r => r.Key == key);

        if (resourceData == null)
        {
            Debug.LogError($"Resource with key '{key}' not found.");
            return;
        }

        if (!loadedResources.ContainsKey(key))
        {
            Debug.LogError($"Resource with key '{key}' not loaded.");
            return;
        }

        switch (resourceData.Type)
        {
            case ResourceType.Image:
                if (loadedResources[key] is Texture2D texture)
                {
                    loadedImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                }
                break;
            case ResourceType.Model:
                break;
            case ResourceType.Audio:
                if (loadedResources[key] is AudioClip audioClip)
                {
                    audioSource.clip = audioClip;
                    audioSource.Play();
                }
                break;
            case ResourceType.Text:
                if (loadedResources[key] is string text)
                {
                    loadedText.text = text;
                }
                break;
        }
    }

    public void UnloadResource(string key)
    {
        if (loadedResources.ContainsKey(key))
        {
            ResourceData resourceData = resourcesToLoad.Find(r => r.Key == key);

            if (resourceData == null)
            {
                Debug.LogError($"Resource data for '{key}' not found. This is an internal error.");
                return;
            }

            switch (resourceData.Type)
            {
                case ResourceType.Image:
                    loadedImage.sprite = null;
                    break;
                case ResourceType.Model:
                    foreach (Transform child in modelContainer)
                    {
                        Destroy(child.gameObject);
                    }
                    break;
                case ResourceType.Audio:
                    audioSource.clip = null;
                    audioSource.Stop();
                    break;
                case ResourceType.Text:
                    loadedText.text = ""; // Changed to access TMP_Text's text property
                    break;
            }

            if (loadedResources[key] is Texture2D texture)
            {
                Destroy(texture);
            }
            else if (loadedResources[key] is AudioClip audioClip)
            {
                Destroy(audioClip);
            }

            loadedResources.Remove(key);
            Debug.Log($"Resource '{key}' unloaded.");
        }
        else
        {
            Debug.LogWarning($"Resource '{key}' not loaded.");
        }
    }
}