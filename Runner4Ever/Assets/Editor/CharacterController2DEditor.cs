using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterController2D))]
public class CharacterController2DEditor : Editor 
{
	public static bool actionFold = false;
	public static bool actionGroundedFold = false;
	public static bool actionStoppedFold = false;
	public static bool collisionsFold = false;
	public static bool infosFold = false;

	static LayerMask LayerMaskField( string label, LayerMask layerMask) {
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
       
        myScript.speedBonusOnJump = EditorGUILayout.FloatField("speedBonusOnJump", myScript.speedBonusOnJump);
        myScript.runSpeed = EditorGUILayout.FloatField("runSpeed", myScript.runSpeed);
        myScript.accelerationSmooth = EditorGUILayout.FloatField("accelerationSmooth", myScript.accelerationSmooth);
        myScript.dashSpeedMul = EditorGUILayout.FloatField("dashSpeedMul", myScript.dashSpeedMul);
        myScript.timeBetweenJumps = EditorGUILayout.FloatField("timeBetweenJumps", myScript.timeBetweenJumps);
        myScript.jumpMagnitude = EditorGUILayout.FloatField("jumpMagnitude", myScript.jumpMagnitude);
        myScript.highJumpMagnitude = EditorGUILayout.FloatField("highJumpMagnitude", myScript.highJumpMagnitude);
        myScript.jumpStrategy = (CharacterController2D.JumpStrat)EditorGUILayout.EnumPopup("jumpStrategy", myScript.jumpStrategy);
        myScript.jumpRes = (CharacterController2D.JumpRestrictions)EditorGUILayout.EnumPopup("JumpRestrictions", myScript.jumpRes);
        myScript.runDir = (CharacterController2D.RunDirectionOnGround)EditorGUILayout.EnumPopup("Direction when hitting ground", myScript.runDir);
        myScript.dashTime = EditorGUILayout.FloatField("dashTime", myScript.dashTime);

        infosFold = EditorGUILayout.Foldout(infosFold, "Infos");
		if (infosFold)
        {
        	EditorGUI.indentLevel++;
        	EditorGUILayout.LabelField("CollidingRight ", myScript.collidingRight() ? "true" : "false");
        	EditorGUILayout.LabelField("CollidingLeft ", myScript.collidingLeft() ? "true" : "false");
        	EditorGUILayout.LabelField("Grounded ", myScript.grounded() ? "true" : "false");
        	EditorGUILayout.LabelField("isGrabingEdge ", myScript.grabingEdge() ? "true" : "false");
        	EditorGUILayout.LabelField("isWallSticking ", myScript.wallSticking() ? "true" : "false");
        	EditorGUILayout.LabelField("runSpeed ", myScript.runspeed().ToString());
        	EditorGUILayout.LabelField("jumpWallDir ", myScript.jumpWallStack.Peek().ToString());
            EditorGUILayout.LabelField("movstate ", myScript.movstate.ToString());
        	int jumps = myScript.getCurrentJumpCount();
        	EditorGUILayout.LabelField("jumps", jumps.ToString());

        	EditorGUI.indentLevel--;
        }
	}
}
