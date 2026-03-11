using UnityEngine;
using UnityEngine.SceneManagement;

public class MissionHandler : SingletonMonobehaviour<MissionHandler>
{
    private static int s_currentMissionIndex = 0;

    public void LoadMission(int missionIndex)
    {
        s_currentMissionIndex = missionIndex;
        SceneManager.LoadScene("Appointment");
    }

    protected override void SingleOnSceneChanged(Scene s1, Scene s2)
    {
        base.SingleOnSceneChanged(s1, s2);
        if (s2.name == "MainMenu")
        {
            s_currentMissionIndex = -1;
        }
        else if (s2.name == "Appointment")
        {
            MissionDiagnosisWindow missionWindow = FindAnyObjectByType<MissionDiagnosisWindow>();
            missionWindow.missionId = s_currentMissionIndex;
        }
    }
}
