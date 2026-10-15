using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class FixPhase8 
{
    static int GetState() => EditorPrefs.GetInt("FixPhase8_State", -1);
    static void SetState(int s) => EditorPrefs.SetInt("FixPhase8_State", s);

    static FixPhase8()
    {
        if (GetState() >= 0 && GetState() < 6) {
            EditorApplication.update += OnUpdate;
        }
    }

    [MenuItem("Tools/Fix/Run Phase 8")]
    public static void RunManual() 
    {
        SetState(0);
        EditorApplication.update -= OnUpdate;
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate() 
    {
        if (GetState() == 0) 
        {
            Debug.Log("FixPhase8: State 0 - Fixing Assets");
            // 1. Fix Rock FBX Normals (to make them faceted / flat shaded)
            string[] fbxGuids = AssetDatabase.FindAssets("t:Model", new[] { "Assets/ThirdParty/UltimateNaturePack" });
            foreach (var guid in fbxGuids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ModelImporter importer = AssetImporter.GetAtPath(path) as ModelImporter;
                if (importer != null && importer.importNormals != ModelImporterNormals.Calculate) {
                    importer.importNormals = ModelImporterNormals.Calculate;
                    importer.normalCalculationMode = ModelImporterNormalCalculationMode.Unweighted_Legacy;
                    importer.normalSmoothingSource = ModelImporterNormalSmoothingSource.None;
                    importer.normalSmoothingAngle = 0f;
                    importer.SaveAndReimport();
                } else if (importer != null && importer.normalSmoothingAngle > 0f) {
                    importer.normalSmoothingAngle = 0f;
                    importer.normalSmoothingSource = ModelImporterNormalSmoothingSource.None;
                    importer.SaveAndReimport();
                }
            }

            // 2. Fix Sword Prefab Materials
            string swordPath = "Assets/Resources/Obstacles/Obstacle_Sword.prefab";
            using (var editingScope = new PrefabUtility.EditPrefabContentsScope(swordPath)) {
                var swordPrefab = editingScope.prefabContentsRoot;
                
                Material matBlade = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Fix/SwordBlade.mat");
                Material matHilt = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Fix/SwordHilt.mat");
                Material matGuard = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Fix/SwordGuard.mat");
                Material matDirt = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Fix/Dirt.mat");

                foreach(var r in swordPrefab.GetComponentsInChildren<MeshRenderer>(true)) {
                    string mname = "";
                    var mf = r.GetComponent<MeshFilter>();
                    if (mf && mf.sharedMesh) mname = mf.sharedMesh.name.ToLower();
                    
                    Material[] mats = new Material[r.sharedMaterials.Length];
                    for(int i=0; i<mats.Length; i++) {
                        if (mname.Contains("cube") || mname.Contains("blade")) mats[i] = matBlade;
                        else if (mname.Contains("cylinder") || mname.Contains("hilt")) mats[i] = matHilt;
                        else if (mname.Contains("sphere") || mname.Contains("guard")) mats[i] = matGuard;
                        else mats[i] = matDirt;
                    }
                    r.sharedMaterials = mats;
                }
            }
            AssetDatabase.SaveAssets();
            SetState(1);
        }
        else if (GetState() == 1) 
        {
            Debug.Log("FixPhase8: State 1 - Stopping Play Mode");
            if (EditorApplication.isPlaying) {
                EditorApplication.isPlaying = false;
            }
            SetState(2);
        }
        else if (GetState() == 2) 
        {
            if (!EditorApplication.isPlaying) {
                Debug.Log("FixPhase8: State 2 - Play Mode Stopped");
                SetState(3);
            }
        }
        else if (GetState() == 3) 
        {
            Debug.Log("FixPhase8: State 3 - Clearing Clones");
            string scenePath = "Assets/Scenes/MainScene.unity";
            var scene = EditorSceneManager.GetSceneByPath(scenePath);
            if (!scene.isLoaded) scene = EditorSceneManager.OpenScene(scenePath);

            bool changed = false;
            foreach (var root in scene.GetRootGameObjects()) {
                var clones = root.GetComponentsInChildren<Transform>(true);
                foreach (var child in clones) {
                    if (child != null && child.name.StartsWith("Obstacle_") && child.name.Contains("(Clone)")) {
                        GameObject.DestroyImmediate(child.gameObject);
                        changed = true;
                    }
                }
            }
            if (changed) EditorSceneManager.SaveScene(scene, scenePath);
            
            SetState(4);
        }
        else if (GetState() == 4) 
        {
            Debug.Log("FixPhase8: State 4 - Starting Play Mode");
            EditorApplication.isPlaying = true;
            SetState(5);
        }
        else if (GetState() == 5)
        {
            if (EditorApplication.isPlaying) {
                Debug.Log("FixPhase8: State 5 - DONE");
                EditorApplication.update -= OnUpdate;
                SetState(6);
            }
        }
    }
}
