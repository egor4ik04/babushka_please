using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionPrefabData : MonoBehaviour
{
    public int MissionID;
    public Button Button;
    public TextMeshProUGUI TextGUI;

    public void StartMission()
    {
        if (MissionID < 0) return;
        MissionHandler handler = FindAnyObjectByType<MissionHandler>();
        handler.LoadMission(MissionID);
    }
    public void SetMission(int id)
    {
        MissionID = id;
        SetMission();
    }
    public void SetMission()
    {
        SetButtonAction(StartMission);
    }
    public void SetButtonAction(UnityEngine.Events.UnityAction action, bool removeAllOthers = true)
    {
        ValidateComponent(ref Button);
        if (removeAllOthers)
            Button.onClick.RemoveAllListeners();
        Button.onClick.AddListener(action);
    }
    public void SetText(string text)
    {
        ValidateComponent(ref TextGUI);
        TextGUI.text = text;
    }
    private void ValidateComponent<T>(ref T data) where T : Component
    {
        bool isValid = true;
        if (data == null) isValid = false;
        else
        {
            try
            {
                data.name.ToString();   
            }
            catch (MissingReferenceException)
            {
                isValid = false;
            }
        }

        if (isValid)
            return;

        if (TryGetComponent(out T component))
        {
            data = component;
        }
        else
        {
            data = GetComponentInChildren<T>(true);
            if (data == null)
                Debug.LogError($"MissionPrefabData: Missing component of type {typeof(T).Name} in {gameObject.name}");
        }
    }
}
