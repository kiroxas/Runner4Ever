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
		

		actionFold = EditorGUILayout.Foldout(actionFold, "Actions");
		if (actionFold)
        {
        	EditorGUI.indentLevel++;
        	myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.Tap] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onTap", myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.Tap]);
        	myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.SwipeSameDir] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeSameDir", myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.SwipeSameDir]);
        	myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.SwipeOppDir] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeOppDir", myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.SwipeOppDir]);
        	myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.SwipeDown] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeDown", myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.SwipeDown]);
        	myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.SwipeUp] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeUp", myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.SwipeUp]);
        	myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.DoubleTap] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onDoubleTap", myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.DoubleTap]);
        	myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.Hold] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onHold", myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.Hold]);
        	myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.HoldUp] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onHoldDown", myScript.actions[myScript.airBorn].action[(int)CharacterController2D.Inputs.HoldUp]);
        	EditorGUI.indentLevel--;
        }

        actionGroundedFold = EditorGUILayout.Foldout(actionGroundedFold, "Actions grounded");
		if (actionGroundedFold)
        {
        	EditorGUI.indentLevel++;
        	myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.Tap] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onTap", myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.Tap]);
        	myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.SwipeSameDir] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeSameDir", myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.SwipeSameDir]);
        	myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.SwipeOppDir] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeOppDir", myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.SwipeOppDir]);
        	myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.SwipeDown] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeDown", myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.SwipeDown]);
        	myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.SwipeUp] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeUp", myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.SwipeUp]);
        	myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.DoubleTap] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onDoubleTap", myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.DoubleTap]);
        	myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.Hold] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onHold", myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.Hold]);
        	myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.HoldUp] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onHoldDown", myScript.actions[myScript.groundedIndex].action[(int)CharacterController2D.Inputs.HoldUp]);
        	EditorGUI.indentLevel--;
        }

        actionStoppedFold = EditorGUILayout.Foldout(actionStoppedFold, "Actions grounded and stopped");
		if (actionStoppedFold)
        {
        	EditorGUI.indentLevel++;
        	myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.Tap] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onTap", myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.Tap]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.SwipeSameDir] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeSameDir", myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.SwipeSameDir]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.SwipeOppDir] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeOppDir", myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.SwipeOppDir]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.SwipeDown] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeDown", myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.SwipeDown]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.SwipeUp] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onSwipeUp", myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.SwipeUp]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.DoubleTap] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onDoubleTap", myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.DoubleTap]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.Hold] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onHold", myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.Hold]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.HoldUp] = (CharacterController2D.Action)EditorGUILayout.EnumPopup("onHoldDown", myScript.actions[myScript.groundedAndStopped].action[(int)CharacterController2D.Inputs.HoldUp]);
        	EditorGUI.indentLevel--;
        }

        myScript.animator = (Animator)EditorGUILayout.ObjectField("Animator", myScript.animator, typeof(Animator), true);
       // myScript.state = (CharacterState)EditorGUILayout.ObjectField("CharacterState", myScript.state, typeof(CharacterState), true);
       
        myScript.runSpeed = EditorGUILayout.FloatField("runSpeed", myScript.runSpeed);
        myScript.accelerationSmooth = EditorGUILayout.FloatField("accelerationSmooth", myScript.accelerationSmooth);
        myScript.timeBetweenJumps = EditorGUILayout.FloatField("timeBetweenJumps", myScript.timeBetweenJumps);
        myScript.jumpMagnitude = EditorGUILayout.FloatField("jumpMagnitude", myScript.jumpMagnitude);
        myScript.highJumpMagnitude = EditorGUILayout.FloatField("highJumpMagnitude", myScript.highJumpMagnitude);
        myScript.jumpStrategy = (CharacterController2D.JumpStrat)EditorGUILayout.EnumPopup("jumpStrategy", myScript.jumpStrategy);
        myScript.jumpRes = (CharacterController2D.JumpRestrictions)EditorGUILayout.EnumPopup("JumpRestrictions", myScript.jumpRes);
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
       
       if (GUI.changed)
        {
        	EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(((GameObject)myScript.gameObject).scene);
        }
       
	}
}
