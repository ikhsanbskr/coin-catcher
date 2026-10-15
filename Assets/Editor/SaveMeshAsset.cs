using UnityEngine;
using UnityEditor;
using System.IO;

public class SaveMeshAsset
{
    [MenuItem("GameObject/Save Mesh to Asset", false, 10)]
    static void SaveSelectedMesh(MenuCommand menuCommand)
    {
        GameObject go = menuCommand.context as GameObject;
        if (go == null) return;

        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh != null)
        {
            if (!AssetDatabase.Contains(mf.sharedMesh))
            {
                string path = EditorUtility.SaveFilePanelInProject("Save Mesh", go.name + ".asset", "asset", "Save the embedded mesh");
                if (!string.IsNullOrEmpty(path))
                {
                    Mesh newMesh = Object.Instantiate(mf.sharedMesh);
                    AssetDatabase.CreateAsset(newMesh, path);
                    mf.sharedMesh = newMesh;
                    EditorUtility.SetDirty(mf);
                }
            }
        }

        MeshCollider mc = go.GetComponent<MeshCollider>();
        if (mc != null && mc.sharedMesh != null)
        {
            if (!AssetDatabase.Contains(mc.sharedMesh))
            {
                string path = EditorUtility.SaveFilePanelInProject("Save Collider Mesh", go.name + "_Collider.asset", "asset", "Save the embedded collider mesh");
                if (!string.IsNullOrEmpty(path))
                {
                    Mesh newMesh = Object.Instantiate(mc.sharedMesh);
                    AssetDatabase.CreateAsset(newMesh, path);
                    mc.sharedMesh = newMesh;
                    EditorUtility.SetDirty(mc);
                }
            }
        }
    }
}
