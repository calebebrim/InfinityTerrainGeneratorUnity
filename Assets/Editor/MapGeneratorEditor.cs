using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (MapGenerator))]
public class MapGeneratorEditor : Editor {
    public static MapGenerator mapGen;
    public override void OnInspectorGUI () {
        mapGen = (MapGenerator) target;

        if (DrawDefaultInspector ()) {
            if (mapGen.autoUpdate) {
                mapGen.DrawnMapInEditor ();
            }
        }

        if (GUILayout.Button ("Generate")) {
            mapGen.DrawnMapInEditor ();
        }
    }

    public static string[] OnWillSaveAssets (string[] paths) {
        // Get the name of the scene to save.
        Debug.Log ("Saving");
        var mapGen = MapGeneratorEditor.mapGen;

        if (mapGen.autoUpdate) {
            mapGen.DrawnMapInEditor ();
        }

        return paths;
    }
}

// [CustomEditor (typeof (MapGenerator))]
// public class OnSave : UnityEditor.AssetModificationProcessor, Editor {
//     public static string[] OnWillSaveAssets (string[] paths) {
//         // Get the name of the scene to save.
//         Debug.Log ("Saving");
//         var mapGen = MapGeneratorEditor.mapGen;

//         if (mapGen.autoUpdate) {
//             mapGen.DrawnMapInEditor ();
//         }

//         return paths;
//     }
// }