using UnityEditor;
using UnityEngine;
using System.IO;

[InitializeOnLoad]
public static class FixPhase5 
{
    [MenuItem("Tools/Fix/Run Phase A")]
    public static void Run() 
    {
        // 1. Create Radial Texture
        if (!AssetDatabase.IsValidFolder("Assets/Sprites")) AssetDatabase.CreateFolder("Assets", "Sprites");
        if (!AssetDatabase.IsValidFolder("Assets/Sprites/Fx")) AssetDatabase.CreateFolder("Assets/Sprites", "Fx");
        
        string texPath = "Assets/Sprites/Fx/RadialSoft.png";
        Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
        for(int y=0; y<128; y++) {
            for(int x=0; x<128; x++) {
                float dist = Vector2.Distance(new Vector2(x, y), new Vector2(64, 64)) / 64f;
                float alpha = Mathf.Clamp01(1f - dist);
                tex.SetPixel(x, y, new Color(1, 1, 1, alpha));
            }
        }
        tex.Apply();
        File.WriteAllBytes(texPath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(texPath, ImportAssetOptions.ForceUpdate);
        TextureImporter importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null) {
            importer.textureType = TextureImporterType.Default;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
        Texture2D radialTex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);

        // 2. Create Materials
        if (!AssetDatabase.IsValidFolder("Assets/Materials/Fix")) 
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets", "Materials");
            AssetDatabase.CreateFolder("Assets/Materials", "Fix");
        }

        Shader litShader = Shader.Find("Universal Render Pipeline/Lit");
        if (litShader == null) litShader = Shader.Find("Standard");

        Shader unlitParticleShader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (unlitParticleShader == null) unlitParticleShader = Shader.Find("Particles/Standard Unlit");

        Material GetMat(string name, Color color, float smooth = 0.5f, float metal = 0f) {
            string path = "Assets/Materials/Fix/" + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(litShader); AssetDatabase.CreateAsset(m, path); }
            m.shader = litShader;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            else m.color = color;
            if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", smooth);
            if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metal);
            EditorUtility.SetDirty(m);
            return m;
        }

        Material GetParticleMat(string name, Color color, Texture2D t, bool additive) {
            string path = "Assets/Materials/Fix/" + name + ".mat";
            Material m = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (m == null) { m = new Material(unlitParticleShader); AssetDatabase.CreateAsset(m, path); }
            m.shader = unlitParticleShader;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
            if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", t);
            
            // Surface Type Transparent
            m.SetFloat("_Surface", 1);
            if (additive) {
                m.SetFloat("_Blend", 2); // Additive
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.One);
            } else {
                m.SetFloat("_Blend", 0); // Alpha
                m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            }
            m.SetInt("_ZWrite", 0);
            m.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            EditorUtility.SetDirty(m);
            return m;
        }

        ColorUtility.TryParseHtmlString("#D7D7DA", out Color swordBladeCol);
        ColorUtility.TryParseHtmlString("#C18047", out Color swordHiltCol);
        ColorUtility.TryParseHtmlString("#FBC437", out Color swordGuardCol);
        ColorUtility.TryParseHtmlString("#8A8D91", out Color rockCol);

        Material matBlade = GetMat("SwordBlade", swordBladeCol, 0.6f, 0.4f);
        Material matHilt = GetMat("SwordHilt", swordHiltCol);
        Material matGuard = GetMat("SwordGuard", swordGuardCol);
        Material matRock = GetMat("Rock", rockCol, 0.1f, 0f);

        Material matExplosionFire = GetParticleMat("ExplosionFire", Color.white, radialTex, true);
        Material matExplosionSmoke = GetParticleMat("ExplosionSmoke", new Color(0.5f, 0.5f, 0.5f, 0.5f), radialTex, false);
        Material matTrail = GetParticleMat("TrailMat", new Color(1f, 1f, 1f, 0.5f), radialTex, false);

        // 3. Assign to Prefabs
        GameObject vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ExplosionVFX.prefab");
        if (vfxPrefab != null)
        {
            ParticleSystem ps = vfxPrefab.GetComponent<ParticleSystem>();
            if (ps != null) {
                var main = ps.main;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.7f);
                main.startSize = new ParticleSystem.MinMaxCurve(0.15f, 0.5f);
                
                var em = ps.emission;
                em.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, new ParticleSystem.MinMaxCurve(20, 30)) });

                ParticleSystemRenderer psr = vfxPrefab.GetComponent<ParticleSystemRenderer>();
                if (psr != null) {
                    psr.renderMode = ParticleSystemRenderMode.Billboard;
                    psr.sharedMaterial = matExplosionFire;
                }
            }
            // If there's a child smoke
            foreach(Transform child in vfxPrefab.transform) {
                ParticleSystemRenderer childPsr = child.GetComponent<ParticleSystemRenderer>();
                if (childPsr != null) {
                    childPsr.sharedMaterial = matExplosionSmoke;
                }
            }
            PrefabUtility.SavePrefabAsset(vfxPrefab);
        }

        GameObject swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Obstacles/Obstacle_Sword.prefab");
        if (swordPrefab != null)
        {
            MeshFilter[] mfs = swordPrefab.GetComponentsInChildren<MeshFilter>();
            foreach(var mf in mfs) {
                if (mf.sharedMesh != null) {
                    Renderer r = mf.GetComponent<Renderer>();
                    if (r != null) {
                        if (mf.sharedMesh.name.Contains("Cube")) r.sharedMaterial = matBlade;
                        else if (mf.sharedMesh.name.Contains("Cylinder")) r.sharedMaterial = matHilt;
                        else if (mf.sharedMesh.name.Contains("Sphere")) r.sharedMaterial = matGuard;
                    }
                }
            }
            PrefabUtility.SavePrefabAsset(swordPrefab);
        }

        string[] rockNames = { "Obstacle_RockBig", "Obstacle_RockMed", "Obstacle_RockSmall" };
        foreach(var n in rockNames) {
            GameObject rPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Obstacles/" + n + ".prefab");
            if (rPrefab != null) {
                Renderer[] rr = rPrefab.GetComponentsInChildren<Renderer>();
                foreach(var r in rr) r.sharedMaterial = matRock;
                PrefabUtility.SavePrefabAsset(rPrefab);
            }
        }

        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/Player.prefab");
        if (playerPrefab != null)
        {
            Player3DController p3d = playerPrefab.GetComponent<Player3DController>();
            if (p3d != null) {
                SerializedObject so = new SerializedObject(p3d);
                SerializedProperty prop = so.FindProperty("trailMaterial");
                if (prop != null) {
                    prop.objectReferenceValue = matTrail;
                    so.ApplyModifiedProperties();
                }
            }
            PrefabUtility.SavePrefabAsset(playerPrefab);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // 4. Report
        Debug.Log("--- FIX PHASE A REPORT ---");
        if (vfxPrefab != null) {
            var psr = vfxPrefab.GetComponent<ParticleSystemRenderer>();
            Debug.Log("ExplosionVFX material = " + (psr.sharedMaterial ? psr.sharedMaterial.name : "None") + ", shader = " + (psr.sharedMaterial ? psr.sharedMaterial.shader.name : "None"));
        }
        if (swordPrefab != null) {
            foreach(var r in swordPrefab.GetComponentsInChildren<Renderer>()) {
                Debug.Log("Obstacle_Sword child " + r.gameObject.name + " material = " + (r.sharedMaterial ? r.sharedMaterial.name : "None") + ", shader = " + (r.sharedMaterial ? r.sharedMaterial.shader.name : "None"));
            }
        }
        if (playerPrefab != null) {
            Player3DController p3d = playerPrefab.GetComponent<Player3DController>();
            SerializedObject so = new SerializedObject(p3d);
            SerializedProperty prop = so.FindProperty("trailMaterial");
            Material tm = prop != null ? (Material)prop.objectReferenceValue : null;
            Debug.Log("Player Trail Material = " + (tm ? tm.name : "None"));
        }
    }
}
