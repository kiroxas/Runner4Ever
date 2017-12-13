﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Ink.Runtime;

public class StoryTeller : MonoBehaviour 
{
	[SerializeField]
	private TextAsset inkJSONAsset;
	private Story story;

	[SerializeField]
	private Canvas canvas;

	// UI Prefabs
	[SerializeField]
	private Text textPrefab;
	[SerializeField]
	private Button buttonPrefab;

	[SerializeField]
	private GameObject textZone;
	[SerializeField]
	private GameObject buttonZone;

	void Awake () {
		StartStory();
	}

	void StartStory () {
		story = new Story (inkJSONAsset.text);
		RefreshView();
	}

	void RefreshView () {
		RemoveChildren ();

		while (story.canContinue) {
			string text = story.Continue ().Trim();
			CreateContentView(text);
		}

		if(story.currentChoices.Count > 0) {
			for (int i = 0; i < story.currentChoices.Count; i++) {
				Choice choice = story.currentChoices [i];
				Button button = CreateChoiceView (choice.text.Trim ());
				button.onClick.AddListener (delegate {
					OnClickChoiceButton (choice);
				});
			}
		} else {
			Button choice = CreateChoiceView("End of story.");
			choice.onClick.AddListener(delegate{
				GameFlow.get().LoadMainMenu();
			});
		}
	}

	void OnClickChoiceButton (Choice choice) {
		story.ChooseChoiceIndex (choice.index);
		RefreshView();
	}

	void CreateContentView (string text) {
		Text storyText = Instantiate (textPrefab) as Text;
		storyText.text = text;
		storyText.transform.SetParent (textZone.transform, false);
	}

	Button CreateChoiceView (string text) {
		Button choice = Instantiate (buttonPrefab) as Button;
		choice.transform.SetParent (buttonZone.transform, false);

		Text choiceText = choice.GetComponentInChildren<Text> ();
		choiceText.text = text;

		//HorizontalLayoutGroup layoutGroup = choice.GetComponent <HorizontalLayoutGroup> ();
		//layoutGroup.childForceExpandHeight = false;

		return choice;
	}

	void RemoveChildren () 
	{
		RemoveChildren(buttonZone);
		RemoveChildren(textZone);
	}

	void RemoveChildren (GameObject g) 
	{
		int childCount = g.transform.childCount;
		for (int i = childCount - 1; i >= 0; --i) {
			GameObject.Destroy (g.transform.GetChild (i).gameObject);
		}
	}
}
