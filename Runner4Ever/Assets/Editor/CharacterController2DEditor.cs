using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterController2D))]
public class CharacterController2DEditor : Editor 
{
	public static bool jumpFold = false;
    public static bool secondJumpFold = false;
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
		CharacterController2D myScript = (CharacterController2D)target;

        myScript.animator = (Animator)EditorGUILayout.ObjectField("Animator", myScript.animator, typeof(Animator), true);

        jumpFold = EditorGUILayout.Foldout(jumpFold, "First Jump");
        if (jumpFold)
        {
            EditorGUI.indentLevel++;
            myScript.jumpDefinition.jumpShape = EditorGUILayout.CurveField("Shape", myScript.jumpDefinition.jumpShape);
            myScript.jumpDefinition.xDistance = EditorGUILayout.FloatField("total x distance", myScript.jumpDefinition.xDistance);
            myScript.jumpDefinition.yDistance = EditorGUILayout.FloatField("y distance for 1 unit", myScript.jumpDefinition.yDistance);
            EditorGUI.indentLevel--;
        }

        secondJumpFold = EditorGUILayout.Foldout(secondJumpFold, "Double Jump");
        if (secondJumpFold)
        {
            EditorGUI.indentLevel++;
            myScript.doubleJumpDefinition.jumpShape = EditorGUILayout.CurveField("Shape", myScript.doubleJumpDefinition.jumpShape);
            myScript.doubleJumpDefinition.xDistance = EditorGUILayout.FloatField("total x distance", myScript.doubleJumpDefinition.xDistance);
            myScript.doubleJumpDefinition.yDistance = EditorGUILayout.FloatField("y distance for 1 unit", myScript.doubleJumpDefinition.yDistance);
            EditorGUI.indentLevel--;
        }
       
        myScript.xSpeedBySecond = EditorGUILayout.FloatField("runSpeed", myScript.xSpeedBySecond);
        myScript.gravityFactor = EditorGUILayout.FloatField("gravityFactor", myScript.gravityFactor);
        myScript.accelerationSmooth = EditorGUILayout.FloatField("accelerationSmooth", myScript.accelerationSmooth);
        myScript.gravitySmooth = EditorGUILayout.FloatField("gravitySmooth", myScript.gravitySmooth);
        myScript.dashSpeedMul = EditorGUILayout.FloatField("dashSpeedMul", myScript.dashSpeedMul);
        myScript.timeBetweenJumps = EditorGUILayout.FloatField("timeBetweenJumps", myScript.timeBetweenJumps);
        myScript.jumpStrategy = (CharacterController2D.JumpStrat)EditorGUILayout.EnumPopup("jumpStrategy", myScript.jumpStrategy);
        myScript.jumpRes = (CharacterController2D.JumpRestrictions)EditorGUILayout.EnumPopup("JumpRestrictions", myScript.jumpRes);
        myScript.momenutmHangWalljumpTime = EditorGUILayout.FloatField("momenutmHangWalljumpTime", myScript.momenutmHangWalljumpTime);
        myScript.firstJumpInAirEnabled = EditorGUILayout.Toggle("firstJumpInAirEnabled", myScript.firstJumpInAirEnabled);
        myScript.runDir = (CharacterController2D.RunDirectionOnGround)EditorGUILayout.EnumPopup("Direction when hitting ground", myScript.runDir);
        myScript.dashTime = EditorGUILayout.FloatField("dashTime", myScript.dashTime);
        myScript.nullifyGravityOnDash = EditorGUILayout.Toggle("nullifyGravityOnDash", myScript.nullifyGravityOnDash);
       
        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(((GameObject)myScript.gameObject).scene);
        }
	}
}
