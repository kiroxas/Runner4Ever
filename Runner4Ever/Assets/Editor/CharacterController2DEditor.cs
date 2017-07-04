using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CharacterController2D))]
public class CharacterController2DEditor : Editor 
{
	public static bool actionFold = false;
	public static bool actionGroundedFold = false;
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
		

		actionFold = EditorGUILayout.Foldout(actionFold, "Actions");
		if (actionFold)
        {
        	EditorGUI.indentLevel++;
        	myScript.onTap = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onTap", myScript.onTap);
        	myScript.onSwipeLeft = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeLeft", myScript.onSwipeLeft);
        	myScript.onSwipeRight = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeRight", myScript.onSwipeRight);
        	myScript.onSwipeDown = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeDown", myScript.onSwipeDown);
        	myScript.onSwipeUp = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeUp", myScript.onSwipeUp);
        	myScript.onDoubleTap = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onDoubleTap", myScript.onDoubleTap);
        	myScript.onHoldDown = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onHoldDown", myScript.onHoldDown);
        	myScript.onHoldUp = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onHoldUp", myScript.onHoldUp);
        	myScript.onRightCollision = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onRightCollision", myScript.onRightCollision);
        	EditorGUI.indentLevel--;
        }

        actionGroundedFold = EditorGUILayout.Foldout(actionGroundedFold, "Actions grounded");
		if (actionGroundedFold)
        {
        	EditorGUI.indentLevel++;
        	myScript.onTapGrounded = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onTapGrounded", myScript.onTapGrounded);
        	myScript.onSwipeLeftGrounded = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeLeftGrounded", myScript.onSwipeLeftGrounded);
        	myScript.onSwipeRightGrounded = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeRightGrounded", myScript.onSwipeRightGrounded);
        	myScript.onSwipeDownGrounded = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeDownGrounded", myScript.onSwipeDownGrounded);
        	myScript.onSwipeUpGrounded = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeUpGrounded", myScript.onSwipeUpGrounded);
        	myScript.onDoubleTapGrounded = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onDoubleTapGrounded", myScript.onDoubleTapGrounded);
        	myScript.onHoldDownGrounded = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onHoldDownGrounded", myScript.onHoldDownGrounded);
        	myScript.onHoldUpGrounded = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onHoldUpGrounded", myScript.onHoldUpGrounded);
        	myScript.onRightCollisionGrounded = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onRightCollisionGrounded", myScript.onRightCollisionGrounded);
        	EditorGUI.indentLevel--;
        }

        myScript.animator = (Animator)EditorGUILayout.ObjectField("Animator", myScript.animator, typeof(Animator), true);
       // myScript.state = (CharacterState)EditorGUILayout.ObjectField("CharacterState", myScript.state, typeof(CharacterState), true);
       
        myScript.runSpeed = EditorGUILayout.FloatField("runSpeed", myScript.runSpeed);
        myScript.jumpMagnitude = EditorGUILayout.FloatField("jumpMagnitude", myScript.jumpMagnitude);
        myScript.jumpRes = (CharacterController2D.JumpRestrictions)EditorGUILayout.EnumPopup("JumpRestrictions", myScript.jumpRes);
        myScript.jumpWall = (CharacterController2D.JumpDirectionOnWallOrEdge)EditorGUILayout.EnumPopup("JumpDirectionOnWallOrEdge", myScript.jumpWall);
        myScript.runDir = (CharacterController2D.RunDirectionOnGround)EditorGUILayout.EnumPopup("Direction when hitting ground", myScript.runDir);

        infosFold = EditorGUILayout.Foldout(infosFold, "Infos");
		if (infosFold)
        {
        	EditorGUI.indentLevel++;
        	EditorGUILayout.LabelField("CollidingRight ", myScript.collidingRight() ? "true" : "false");
        	EditorGUILayout.LabelField("Grounded ", myScript.grounded() ? "true" : "false");
        	EditorGUILayout.LabelField("isGrabingEdge ", myScript.grabingEdge() ? "true" : "false");
        	EditorGUILayout.LabelField("isWallSticking ", myScript.wallSticking() ? "true" : "false");
        	EditorGUILayout.LabelField("runSpeed ", myScript.runspeed().ToString());
        	EditorGUI.indentLevel--;
        }
       
       
	}
}
