using UnityEngine;
using System.Collections;

public class TestBehaviour : MonoBehaviour
{
	public static Run InputPopUp(string aTitle, string aStartValue,string aOkString, string aCancelString, System.Action<string> aOnUpdateValue)
	{
		if (aOnUpdateValue == null)
			return null;
		Rect r = new Rect(0,0,300,200);
		r.center = new Vector2(Screen.width, Screen.height)*0.5f;
		string val = aStartValue;
		if (string.IsNullOrEmpty(aOkString))
			aOkString = "Ok";
		
		var result = Run.CreateGUIWindow(r,aTitle,(win)=>
		{
			val = GUILayout.TextField(val);
			GUILayout.BeginHorizontal();
			if(GUILayout.Button(aOkString))
			{
				aOnUpdateValue(val); 
				win.Close();
			}
			if (!string.IsNullOrEmpty(aCancelString))
			{
				if(GUILayout.Button(aCancelString))
				{
					aOnUpdateValue(aStartValue); 
					win.Close();
				}
			}
			GUILayout.EndHorizontal();
		});
		return result.inst;
	}
	public static Run IntInputPopUp(string aTitle, int aStartValue, string aOkString, string aCancelString, System.Action<int> aOnUpdateValue)
	{
		return InputPopUp(aTitle, aStartValue.ToString(),aOkString,aCancelString, (str)=>{
			int val = 0;
			if (!int.TryParse(str,out val))
				val = aStartValue;
			aOnUpdateValue(val);
		});
	}
	public static Run FloatInputPopUp(string aTitle, float aStartValue, string aOkString, string aCancelString, System.Action<float> aOnUpdateValue)
	{
		return InputPopUp(aTitle, aStartValue.ToString(),aOkString,aCancelString, (str)=>{
			float val = 0;
			if (!float.TryParse(str,out val))
				val = aStartValue;
			aOnUpdateValue(val);
		});
	}
	
	private static IEnumerator Wizard()
	{
		string message = "";
		yield return InputPopUp("Input scheduled message","", "ok", "cancel",(str)=>message = str).WaitFor;
		if (message == "")
			yield break;
		
		float delay = 0f;
		yield return FloatInputPopUp("input delay in seconds", 2, "ok", "use defaul", (val)=>delay = val).WaitFor;
		float duration = 0f;
		yield return FloatInputPopUp("input message duration", 5, "ok", "use default", (val)=>duration = val).WaitFor;
		
		Run.CTempWindow window = null;
		
		yield return Run.After(delay,()=>{
			Rect r = new Rect(0,0,300,200);
			r.center = new Vector2(Screen.width, Screen.height)*0.5f;
			
			window = Run.CreateGUIWindow(r,"Scheduled Message",(win)=>{
				GUILayout.Label(message);
			});
			Run.After(duration,()=> window.Close());
		}).WaitFor;
		
		yield return window.inst.WaitFor;
	}
	
    void Start()
    {
		Run.OnGUI(0,()=> {
			GUILayout.Label("There are " + CoroutineHelper.Instance.ScheduledOnGUIItems + " scheduled OnGUI items");
			if (GUILayout.Button("open window"))
			{
				Run runningWizard = null;
				Run.CreateGUIWindow(new Rect(10,10,200,200),"Sample Window",(win)=>{
					GUILayout.Label("Some content");
					GUILayout.Label("WindowID: " + win.winID);
					if (GUILayout.Button("open popup"))
					{
						float counter = 0;
						Run closeAction = null;
						Run closeIfParentClosed = null;
						Run linkGameObject = null;
						Transform windowObject = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
						float distance = 10;
						var popUpWin = Run.CreateGUIWindow(new Rect(100,100,200,300),"Popup Window",(popup)=>{
							if (closeAction != null)
							{
								GUILayout.Label("This is a popup window which will close in a few sec.");
								GUILayout.Label("closing in " + counter.ToString("0.0"));
								
								if (GUILayout.Button("don't close after countdown"))
								{
									closeAction.Abort();
									closeAction = null;
									popup.pos.height = 150;
								}
							}
							
							if (GUILayout.Button("close now"))
								popup.Close();
							if (linkGameObject == null)
							{
								if (GUILayout.Button("link"))
									linkGameObject = Run.EachFrame(()=>{
										Vector3 P = new Vector3(popup.pos.x, Screen.height - popup.pos.y, distance);
										windowObject.position = Camera.main.ScreenToWorldPoint(P);
									});
							}
							else
							{
								if (GUILayout.Button("break link"))
								{
									linkGameObject.Abort();
									linkGameObject = null;
								}
								distance = GUILayout.HorizontalSlider(distance, 5,15);
							}
							if (GUILayout.Button("Lerp"))
							{
								Run.Lerp(1,(t)=>{windowObject.localEulerAngles = new Vector3(0,t*90,0);});
							}
							GUILayout.Label("This popup belongs to \n" + win.title);
							
							GUI.DragWindow();
						});
						
						popUpWin.inst.ExecuteWhenDone(()=>{
							if (linkGameObject != null)
								linkGameObject.Abort();
							Destroy(windowObject.gameObject);
						});
						
						closeIfParentClosed = Run.Every(0.5f,0.5f,()=>{
							if (win.inst.isDone)
							{
								popUpWin.Close();
								closeIfParentClosed.Abort();
							}
						});
						
						closeAction = Run.After(10,()=>popUpWin.Close());
						
						Run.Lerp(10,(t)=>counter = (1-t)*10);
					}
					if (GUILayout.Button("edit title"))
					{
						InputPopUp("edit window title", win.title, "ok", "cancel",(str)=>win.title = str);
					}
					GUI.enabled = runningWizard == null;
					if (GUILayout.Button("start wizard"))
					{
						runningWizard = Run.Coroutine(Wizard());
						runningWizard.ExecuteWhenDone(()=>{runningWizard = null;});
					}
					GUI.enabled = true;
					if (GUILayout.Button("close this window"))
					{
						win.Close();
					}
					GUI.DragWindow(new Rect(0,0,10000,20));
				});
			}
		});
    }
}
