using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class FixScenePlayer 
{
    [MenuItem("Tools/Fix/Run Phase B")]
    public static void Run() 
    {
        string scenePath = "Assets/Scenes/MainScene.unity";
        var scene = EditorSceneManager.OpenScene(scenePath);

        Material matTrail = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/Fix/TrailMat.mat");
        if (matTrail != null) {
            Player3DController p3d = GameObject.FindObjectOfType<Player3DController>();
            if (p3d != null) {
                SerializedObject so = new SerializedObject(p3d);
                SerializedProperty prop = so.FindProperty("trailMaterial");
                if (prop != null) {
                    prop.objectReferenceValue = matTrail;
                    so.ApplyModifiedProperties();
                }
                EditorUtility.SetDirty(p3d);
            }
        }
        
        EditorSceneManager.SaveScene(scene, scenePath);

        // BUKTIKAN
        Debug.Log("--- FIX PHASE B REPORT ---");
        GameObject vfxPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Prefabs/ExplosionVFX.prefab");
        if (vfxPrefab != null) {
            var psrs = vfxPrefab.GetComponentsInChildren<ParticleSystemRenderer>();
            foreach(var psr in psrs) {
                Debug.Log("ExplosionVFX (" + psr.name + ") material = " + (psr.sharedMaterial ? psr.sharedMaterial.name : "None") + ", shader = " + (psr.sharedMaterial ? psr.sharedMaterial.shader.name : "None"));
            }
        }

        GameObject swordPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Obstacles/Obstacle_Sword.prefab");
        if (swordPrefab != null) {
            foreach(var r in swordPrefab.GetComponentsInChildren<Renderer>()) {
                Debug.Log("Obstacle_Sword child " + r.gameObject.name + " material = " + (r.sharedMaterial ? r.sharedMaterial.name : "None") + ", shader = " + (r.sharedMaterial ? r.sharedMaterial.shader.name : "None"));
            }
        }

        string[] rockNames = { "Obstacle_RockBig", "Obstacle_RockMed", "Obstacle_RockSmall" };
        foreach(var n in rockNames) {
            GameObject rPrefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Resources/Obstacles/" + n + ".prefab");
            if (rPrefab != null) {
                foreach(var r in rPrefab.GetComponentsInChildren<Renderer>()) {
                    Debug.Log(n + " material = " + (r.sharedMaterial ? r.sharedMaterial.name : "None") + ", shader = " + (r.sharedMaterial ? r.sharedMaterial.shader.name : "None"));
                }
            }
        }
        
        Player3DController p3dInScene = GameObject.FindObjectOfType<Player3DController>();
        if (p3dInScene != null) {
            SerializedObject so = new SerializedObject(p3dInScene);
            Material tm = (Material)so.FindProperty("trailMaterial").objectReferenceValue;
            Debug.Log("Player Trail Material = " + (tm ? tm.name : "None"));
        }
    }
}
