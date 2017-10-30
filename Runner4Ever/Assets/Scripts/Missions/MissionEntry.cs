using UnityEngine;
using UnityEngine.UI;

public class MissionEntry : MonoBehaviour
{
    public Text descText;
    public Text progressText;
	public Image background;

	public Color notCompletedColor;
	public Color completedColor;

    public void FillWithMission(MissionBase m, MissionUI owner)
    {
        descText.text = m.GetMissionDesc();

        if (m.isComplete)
        {
            progressText.gameObject.SetActive(false);

			//background.color = completedColor;

			//progressText.color = Color.white;
			//descText.color = Color.white;

			//claimButton.onClick.AddListener(delegate { owner.Claim(m); } );
        }
        else
        {
            //progressText.gameObject.SetActive(true);

			//background.color = notCompletedColor;

			//progressText.color = Color.black;
			//descText.color = completedColor;

			progressText.text = ((int)m.progress) + " / " + ((int)m.max);
        }
    }
}
