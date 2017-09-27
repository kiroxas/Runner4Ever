using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InputHandler))]
public class InputHandlerEditor : Editor 
{
	public static bool actionFold = false;
	public static bool actionGroundedFold = false;
	public static bool actionStoppedFold = false;

	public override void OnInspectorGUI()
	{
		InputHandler myScript = (InputHandler)target;
		
		actionFold = EditorGUILayout.Foldout(actionFold, "Actions");
		if (actionFold)
        {
        	EditorGUI.indentLevel++;
        	myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.Tap] = (InputHandler.Action)EditorGUILayout.EnumPopup("onTap", myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.Tap]);
        	myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.SwipeSameDir] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeSameDir", myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.SwipeSameDir]);
        	myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.SwipeOppDir] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeOppDir", myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.SwipeOppDir]);
        	myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.SwipeDown] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeDown", myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.SwipeDown]);
        	myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.SwipeUp] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeUp", myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.SwipeUp]);
        	myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.DoubleTap] = (InputHandler.Action)EditorGUILayout.EnumPopup("onDoubleTap", myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.DoubleTap]);
        	myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.Hold] = (InputHandler.Action)EditorGUILayout.EnumPopup("onHold", myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.Hold]);
        	myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.HoldUp] = (InputHandler.Action)EditorGUILayout.EnumPopup("onHoldDown", myScript.actions[myScript.airBorn].action[(int)InputHandler.Inputs.HoldUp]);
        	EditorGUI.indentLevel--;
        }

        actionGroundedFold = EditorGUILayout.Foldout(actionGroundedFold, "Actions grounded");
		if (actionGroundedFold)
        {
        	EditorGUI.indentLevel++;
        	myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.Tap] = (InputHandler.Action)EditorGUILayout.EnumPopup("onTap", myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.Tap]);
        	myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.SwipeSameDir] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeSameDir", myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.SwipeSameDir]);
        	myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.SwipeOppDir] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeOppDir", myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.SwipeOppDir]);
        	myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.SwipeDown] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeDown", myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.SwipeDown]);
        	myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.SwipeUp] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeUp", myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.SwipeUp]);
        	myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.DoubleTap] = (InputHandler.Action)EditorGUILayout.EnumPopup("onDoubleTap", myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.DoubleTap]);
        	myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.Hold] = (InputHandler.Action)EditorGUILayout.EnumPopup("onHold", myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.Hold]);
        	myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.HoldUp] = (InputHandler.Action)EditorGUILayout.EnumPopup("onHoldDown", myScript.actions[myScript.groundedIndex].action[(int)InputHandler.Inputs.HoldUp]);
        	EditorGUI.indentLevel--;
        }

        actionStoppedFold = EditorGUILayout.Foldout(actionStoppedFold, "Actions grounded and stopped");
		if (actionStoppedFold)
        {
        	EditorGUI.indentLevel++;
        	myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.Tap] = (InputHandler.Action)EditorGUILayout.EnumPopup("onTap", myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.Tap]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.SwipeSameDir] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeSameDir", myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.SwipeSameDir]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.SwipeOppDir] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeOppDir", myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.SwipeOppDir]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.SwipeDown] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeDown", myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.SwipeDown]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.SwipeUp] = (InputHandler.Action)EditorGUILayout.EnumPopup("onSwipeUp", myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.SwipeUp]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.DoubleTap] = (InputHandler.Action)EditorGUILayout.EnumPopup("onDoubleTap", myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.DoubleTap]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.Hold] = (InputHandler.Action)EditorGUILayout.EnumPopup("onHold", myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.Hold]);
        	myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.HoldUp] = (InputHandler.Action)EditorGUILayout.EnumPopup("onHoldDown", myScript.actions[myScript.groundedAndStopped].action[(int)InputHandler.Inputs.HoldUp]);
        	EditorGUI.indentLevel--;
        }

        myScript.touchzone = (InputHandler.TouchZone)EditorGUILayout.EnumPopup("touchzone", myScript.touchzone);

        if (GUI.changed)
        {
        	EditorUtility.SetDirty(target);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(((GameObject)myScript.gameObject).scene);
        }
    }
}