using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MissionDiagnosisWindow : MonoBehaviour, IPointerClickHandler
{
    [Header("Data")]
    [SerializeField] private MissionsSO missionsSO;
    [SerializeField] public int missionId;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI problemText;
    [SerializeField] private TextMeshProUGUI notebookText;
    [SerializeField] private Transform verdictButtonsRoot;
    [SerializeField] private Button verdictButtonPrefab;
    [SerializeField] private Camera uiCamera;

    private Mission currentMission;

    private readonly HashSet<int> selectedSymptoms = new();

    private readonly Dictionary<int, Button> spawnedVerdictButtons = new();

    private string originalProblemText;

    private void Start()
    {
        LoadMission(missionId);
    }

    public void LoadMission(int newMissionId)
    {
        missionId = newMissionId;
        selectedSymptoms.Clear();
        ClearVerdictButtons();

        currentMission = missionsSO.missions.FirstOrDefault(m => m.Id == missionId);

        if (currentMission == null)
        {
            Debug.LogError("Задание не найдено!!!");
            problemText.text = "";
            notebookText.text = "";
            return;
        }

        originalProblemText = currentMission.ProblemText;

        RefreshProblemText();
        RefreshNotebook();
        RefreshVerdicts();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentMission == null || problemText == null)
            return;

        int linkIndex = TMP_TextUtilities.FindIntersectingLink(problemText, eventData.position, uiCamera);

        if (linkIndex == -1)
            return;

        TMP_LinkInfo linkInfo = problemText.textInfo.linkInfo[linkIndex];
        string linkIdString = linkInfo.GetLinkID();

        if (!int.TryParse(linkIdString, out int symptomId))
            return;

        ToggleSymptom(symptomId);
    }

    private void ToggleSymptom(int symptomId)
    {
        if (selectedSymptoms.Contains(symptomId))
            selectedSymptoms.Remove(symptomId);
        else
            selectedSymptoms.Add(symptomId);

        RefreshProblemText();
        RefreshNotebook();
        RefreshVerdicts();
    }

    private void RefreshProblemText()
    {
        if (string.IsNullOrEmpty(originalProblemText))
        {
            problemText.text = "";
            return;
        }

        string result = Regex.Replace(
            originalProblemText,
            "<link=\"(\\d+)\">(.*?)</link>",
            match =>
            {
                string idStr = match.Groups[1].Value;
                string innerContent = match.Groups[2].Value;

                if (!int.TryParse(idStr, out int symptomId))
                    return match.Value;

                bool isSelected = selectedSymptoms.Contains(symptomId);

                if (isSelected)
                {
                    return $"<link=\"{idStr}\"><mark=#FFF59D88><color=#000000>{innerContent}</color></mark></link>";
                }
                else
                {
                    return $"<link=\"{idStr}\">{innerContent}</link>";
                }
            },
            RegexOptions.Singleline
        );

        problemText.text = result;
    }

    private void RefreshNotebook()
    {
        if (currentMission == null)
        {
            notebookText.text = "";
            return;
        }

        List<string> selectedSymptomTexts = GetSelectedSymptomTextsInTextOrder();

        notebookText.text = selectedSymptomTexts.Count == 0
            ? ""
            : string.Join(", ", selectedSymptomTexts);
    }

    private List<string> GetSelectedSymptomTextsInTextOrder()
    {
        List<string> result = new();

        if (string.IsNullOrEmpty(originalProblemText))
            return result;

        MatchCollection matches = Regex.Matches(
            originalProblemText,
            "<link=\"(\\d+)\">(.*?)</link>",
            RegexOptions.Singleline
        );

        foreach (Match match in matches)
        {
            string idStr = match.Groups[1].Value;
            string innerContent = match.Groups[2].Value;

            if (!int.TryParse(idStr, out int symptomId))
                continue;

            if (!selectedSymptoms.Contains(symptomId))
                continue;

            string cleanText = StripRichText(innerContent).Trim();

            if (!string.IsNullOrEmpty(cleanText))
                result.Add(cleanText);
        }

        return result;
    }

    private void RefreshVerdicts()
    {
        if (currentMission == null)
            return;

        HashSet<int> unlockedDiseaseIds = new();

        foreach (Verdict verdict in currentMission.VerdictList)
        {
            if (verdict.SymptomsId == null || verdict.SymptomsId.Count == 0)
                continue;

            bool allSymptomsSelected = verdict.SymptomsId.All(id => selectedSymptoms.Contains(id));

            if (allSymptomsSelected)
                unlockedDiseaseIds.Add(verdict.DiseaseId);
        }

        List<int> existingIds = spawnedVerdictButtons.Keys.ToList();
        foreach (int diseaseId in existingIds)
        {
            if (!unlockedDiseaseIds.Contains(diseaseId))
            {
                if (spawnedVerdictButtons[diseaseId] != null)
                    Destroy(spawnedVerdictButtons[diseaseId].gameObject);

                spawnedVerdictButtons.Remove(diseaseId);
            }
        }

        foreach (int diseaseId in unlockedDiseaseIds)
        {
            if (spawnedVerdictButtons.ContainsKey(diseaseId))
                continue;

            Disease disease = currentMission.PossibleDiseases.FirstOrDefault(d => d.Id == diseaseId);
            if (disease == null)
            {
                Debug.LogError($"Болезнь {diseaseId} не найдена");
                continue;
            }

            Button newButton = Instantiate(verdictButtonPrefab, verdictButtonsRoot);

            TMP_Text buttonText = newButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
                buttonText.text = disease.Name;

            int capturedDiseaseId = diseaseId;
            newButton.onClick.AddListener(() => OnVerdictButtonClicked(capturedDiseaseId));

            spawnedVerdictButtons.Add(diseaseId, newButton);
        }
    }

    private void OnVerdictButtonClicked(int diseaseId)
    {
        Debug.Log($"Нажал на болезнь: {diseaseId}");

        Disease disease = currentMission.PossibleDiseases.FirstOrDefault(d => d.Id == diseaseId);
        if (disease != null)
        {
            Debug.Log($"Болезнь: {disease.Name}, IsRight={disease.IsRight}");
        }

        //Сюда запихнуть реакцию на какую-то болезнь
        var verdict = spawnedVerdictButtons[diseaseId];
        verdict.image.color = disease.IsRight ? new Color(0, 0.8773585f, 0.2685665f) : Color.darkRed;
        verdict.GetComponentInChildren<TMP_Text>().color = Color.white;

    }

    private void ClearVerdictButtons()
    {
        foreach (var pair in spawnedVerdictButtons)
        {
            if (pair.Value != null)
                Destroy(pair.Value.gameObject);
        }

        spawnedVerdictButtons.Clear();
    }

    private string StripRichText(string input)
    {
        return Regex.Replace(input, "<.*?>", string.Empty);
    }

    public void ToMainMenu() => FindAnyObjectByType<MissionHandler>().LoadMainMenuScene();
}
