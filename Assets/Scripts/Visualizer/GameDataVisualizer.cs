using System;
using UnityEngine;

public class GameDataVisualizer : MonoBehaviour
{
    [Header("Definition Hexagon")]
    [SerializeField] private GameObject definitionFalseActive;
    [SerializeField] private GameObject definitionTrueActive;

    [Header("Symptom")]
    [SerializeField] private GameObject symptomFalseActive;
    [SerializeField] private GameObject symptomTrueActive;

    [Header("Cause")]
    [SerializeField] private GameObject causeFalseActive;
    [SerializeField] private GameObject causeTrueActive;

    [Header("Treatment")]
    [SerializeField] private GameObject treatmentFalseActive;
    [SerializeField] private GameObject treatmentTrueActive;

    [Header("Myth")]
    [SerializeField] private GameObject mythFalseActive;
    [SerializeField] private GameObject mythTrueActive;

    [Header("Support")]
    [SerializeField] private GameObject supportFalseActive;
    [SerializeField] private GameObject supportTrueActive;

    [Header("Severity")]
    [SerializeField] private GameObject severityFalseActive;
    [SerializeField] private GameObject severityTrueActive;

    [Header("Recovery")]
    [SerializeField] private GameObject recoveryFalseActive;
    [SerializeField] private GameObject recoveryTrueActive;

    private void Start()
    {
        UpdateAllVisuals();
    }

    private void Update()
    {
        UpdateAllVisuals();
    }

    /// <summary>
    /// Update visual states for all answer categories based on GameData booleans.
    /// </summary>
    private void UpdateAllVisuals()
    {
        SetActiveState(GameData.Data.definitionHexagonCorrectAnswer, definitionFalseActive, definitionTrueActive);
        SetActiveState(GameData.Data.symptomCorrectAnswer, symptomFalseActive, symptomTrueActive);
        SetActiveState(GameData.Data.causeCorrectAnswer, causeFalseActive, causeTrueActive);
        SetActiveState(GameData.Data.treatmentCorrectAnswer, treatmentFalseActive, treatmentTrueActive);
        SetActiveState(GameData.Data.mythCorrectAnswer, mythFalseActive, mythTrueActive);
        SetActiveState(GameData.Data.supportCorrectAnswer, supportFalseActive, supportTrueActive);
        SetActiveState(GameData.Data.severityCorrectAnswer, severityFalseActive, severityTrueActive);
        SetActiveState(GameData.Data.recoveryCorrectAnswer, recoveryFalseActive, recoveryTrueActive);
    }

    /// <summary>
    /// Sets active state for True/False GameObjects based on boolean value.
    /// </summary>
    private void SetActiveState(bool condition, GameObject falseActive, GameObject trueActive)
    {
        if (falseActive != null)
            falseActive.SetActive(!condition);

        if (trueActive != null)
            trueActive.SetActive(condition);
    }
}