using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterState))]
public class CharacterStateEditor : Editor 
{
	public static bool infosFold = false;

	static LayerMask LayerMaskField( string label, LayerMask layerMask) 
    {
     List<string> layers = new List<string>();
     List<int> layerNumbers = new List<int>();
 
     for (int i = 0; i < 32; i++) {
         string layerName = LayerMask.LayerToName(i);
         if (layerName != "") {
             layers.Add(layerName);
             layerNumbers.Add(i);
         }
     }
     int maskWithoutEmpty = 0;
     for (int i = 0; i < layerNumbers.Count; i++) {
         if (((1 << layerNumbers[i]) & layerMask.value) > 0)
             maskWithoutEmpty |= (1 << i);
     }
     maskWithoutEmpty = EditorGUILayout.MaskField( label, maskWithoutEmpty, layers.ToArray());
     int mask = 0;
     for (int i = 0; i < layerNumbers.Count; i++) {
         if ((maskWithoutEmpty & (1 << i)) > 0)
             mask |= (1 << layerNumbers[i]);
     }
     layerMask.value = mask;
     return layerMask;
    }

	public override void OnInspectorGUI()
	{
		CharacterState myScript = (CharacterState)target;

        myScript.wallStickingPercent = EditorGUILayout.FloatField("wallStickingPercent", myScript.wallStickingPercent);
        myScript.xGroundedOffset = EditorGUILayout.FloatField("xGroundedOffset", myScript.xGroundedOffset);
        myScript.yRightColDetDelta = EditorGUILayout.FloatField("yRightColDetDelta", myScript.yRightColDetDelta);
        myScript.groundedCastDistance = EditorGUILayout.FloatField("groundedCastDistance", myScript.groundedCastDistance);
        myScript.rcCastDistance = EditorGUILayout.FloatField("rcCastDistance", myScript.rcCastDistance);
        myScript.groundedRayCasts = EditorGUILayout.IntField("groundedRayCasts", myScript.groundedRayCasts);
        myScript.rightCollisionRayCasts = EditorGUILayout.IntField("rightCollisionRayCasts", myScript.rightCollisionRayCasts);
        myScript.PlatformMask = LayerMaskField("layers", myScript.PlatformMask);

        infosFold = EditorGUILayout.Foldout(infosFold, "Infos");
        if(infosFold)
        {
            EditorGUILayout.Toggle("grounded", myScript.innerState.isGrounded);
            EditorGUILayout.Toggle("right", myScript.innerState.isCollidingRight);
            EditorGUILayout.Toggle("left", myScript.innerState.isCollidingLeft);
            EditorGUILayout.Toggle("above", myScript.innerState.isCollidingAbove);
            EditorGUILayout.Toggle("wallsticking", myScript.innerState.isWallSticking);
        }
       
       /* if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(((GameObject)myScript.gameObject).scene);
        }*/
	}
}
