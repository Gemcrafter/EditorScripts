// Copyright 2020 The Tilt Brush Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class MatrialThumbnailGenerator : MonoBehaviour
{
    private static readonly string materialDirectory = "T:/BrushEditing/BrushEditKT05/BrushEditTK05/Assets/Resources/Brushes"; // The directory where your materials are stored
    private static readonly string outputDirectory = "T:/MaterialPreviews"; // The directory to save the thumbnails.  This does not add them directly to unity, but rather an external output folder
    private static string[] materialFiles;
    private static int currentIndex = 0;

    [MenuItem("Open Brush/Tools/Create Material Thumbnails")]
    public static void CreateMaterialThumbnails()
    {
        // Check if output directory exists, if not create it
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // Get all material files in the material directory and its subdirectories
        materialFiles = Directory.GetFiles(materialDirectory, "*.mat", SearchOption.AllDirectories);

        // Start the generation process
        EditorApplication.update += GenerateNextThumbnail;
    }

    private static void GenerateNextThumbnail()
    {
        if (currentIndex >= materialFiles.Length)
        {
            // All thumbnails have been generated, stop the process
            EditorApplication.update -= GenerateNextThumbnail;
            return;
        }

        // The asset path should be relative to the Assets directory for LoadAssetAtPath
        string file = materialFiles[currentIndex];

        //Debug Code
        /*
        Debug.Log("File: " + file);
        Debug.Log("File length: " + file.Length);
        Debug.Log("Application.dataPath: " + Application.dataPath);
        Debug.Log("Application.dataPath length: " + Application.dataPath.Length);
        */

        // Replace backslashes with forward slashes
        file = file.Replace("\\", "/");

        if (file.Length > Application.dataPath.Length)
        {
            string relativePath = "Assets" + file.Substring(Application.dataPath.Length);

            // Load the material from the file
            Material material = AssetDatabase.LoadAssetAtPath<Material>(relativePath);
            if (material != null)
            {
                Texture2D texture = AssetPreview.GetAssetPreview(material);

                if (texture == null)
                {
                    if (AssetPreview.IsLoadingAssetPreview(material.GetInstanceID()))
                    {
                        // The asset preview is being loaded, do not move on to the next material
                        return;
                    }
                }
                else
                {
                    // If the texture isn't null, resize it and save it
                    Texture2D resizedTexture = Resize(texture, 128, 128);
                    byte[] pngData = resizedTexture.EncodeToPNG();

                    // Construct the output filename and save the png data to a file
                    string outputFilename = outputDirectory + "/Mx" + material.name + ".png";
                    File.WriteAllBytes(outputFilename, pngData);
                }
            }

            // Move on to the next material
            currentIndex++;
        }
        else
        {
            Debug.LogError("Cannot create relative path. File path is shorter than Application.dataPath.");
        }
    }


    public static Texture2D Resize(Texture2D source, int newWidth, int newHeight)
    {
        source.filterMode = FilterMode.Bilinear;
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        rt.filterMode = FilterMode.Point;
        RenderTexture.active = rt;
        Graphics.Blit(source, rt);
        Texture2D nTex = new Texture2D(newWidth, newHeight);
        nTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        nTex.Apply();
        RenderTexture.active = null;
        return nTex;
    }
}
