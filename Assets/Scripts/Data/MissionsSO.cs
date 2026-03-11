using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionsSO", menuName = "Scriptable Objects/MissionsSO")]
public class MissionsSO : ScriptableObject
{
    public List<Mission> missions;
}

[System.Serializable]
public class Mission
{
    public int Id;
    [TextArea]
    public string ProblemText;
    public List<Disease> PossibleDiseases;
    public List<Verdict> VerdictList;
}

[System.Serializable]
public class Verdict
{
    public int Id;
    public List<int> SymptomsId;
    public int DiseaseId;
}

[System.Serializable]
public class Disease
{
    public int Id;
    public string Name;
    public bool IsRight = false;
}