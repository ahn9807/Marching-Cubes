﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TextureArrayWizard : ScriptableWizard
{
    public Texture2D[] textures;

    [MenuItem("Asset/Create/Texture Array")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<TextureArrayWizard>("Create Texture Array", "Create");
    }

    private void OnWizardCreate()
    {
        if (textures.Length == 0)
        {
            return;
        }
        string path = EditorUtility.SaveFilePanelInProject(
            "Save Texture Array", "Texture Array", "asset", "Save Texture Array"
        );
        if (path.Length == 0)
        {
            return;
        }

        Texture2D t = textures[0];
        Texture2DArray textureArray = new Texture2DArray(t.width, t.height, textures.Length, t.format, false);
        textureArray.anisoLevel = t.anisoLevel;
        textureArray.filterMode = t.filterMode;
        textureArray.wrapMode = t.wrapMode;

        for (int i = 0; i < textures.Length; i++)
        {
            Texture2D scaledTexture = new Texture2D(t.width, t.height, TextureFormat.ARGB32, false);
            Graphics.ConvertTexture(textures[i], scaledTexture);
            Graphics.ConvertTexture(scaledTexture, 0, textureArray, i);
            //for (int m = 0; m < t.mipmapCount; m++)
            //{
            //    Graphics.CopyTexture(scaledTexture, 0, m, textureArray, i, m);
            //}
        }

        AssetDatabase.CreateAsset(textureArray, path);
    }
}