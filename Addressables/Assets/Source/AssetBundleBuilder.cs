using UnityEngine;
using UnityEditor;
using System.IO;

public class AssetBundleBuilder
{
    [MenuItem("Assets/Build AssetBundle From Selection - Single")]
    static void BuildAssetBundleFromSelection()
    {
        string path = EditorUtility.SaveFilePanel("Save Asset Bundle", "", "MyAssetBundle", "unity3d");
        if (string.IsNullOrEmpty(path)) return;

        BuildPipeline.BuildAssetBundles(Path.GetDirectoryName(path), BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows); // Or other target

        Debug.Log("AssetBundle built to: " + path);
    }
}