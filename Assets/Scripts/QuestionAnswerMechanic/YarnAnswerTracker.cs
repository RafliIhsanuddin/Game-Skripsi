using UnityEngine;
using Yarn.Unity;

public class YarnAnswerTracker : MonoBehaviour
{
    [YarnCommand("SetDefinitionTrue")]
    public void SetDefinitionTrue()
    {
        GameData.Data.definitionHexagonCorrectAnswer = true;
        Debug.Log("Definition Answer set to TRUE");
    }

    [YarnCommand("SetDefinitionFalse")]
    public void SetDefinitionFalse()
    {
        GameData.Data.definitionHexagonCorrectAnswer = false;
        Debug.Log("Definition Answer set to FALSE");
    }

    [YarnCommand("SetSymptomTrue")]
    public void SetSymptomTrue()
    {
        GameData.Data.symptomCorrectAnswer = true;
        Debug.Log("Symptom Answer set to TRUE");
    }

    [YarnCommand("SetSymptomFalse")]
    public void SetSymptomFalse()
    {
        GameData.Data.symptomCorrectAnswer = false;
        Debug.Log("Symptom Answer set to FALSE");
    }

    [YarnCommand("SetCauseTrue")]
    public void SetCauseTrue()
    {
        GameData.Data.causeCorrectAnswer = true;
        Debug.Log("Cause Answer set to TRUE");
    }

    [YarnCommand("SetCauseFalse")]
    public void SetCauseFalse()
    {
        GameData.Data.causeCorrectAnswer = false;
        Debug.Log("Cause Answer set to FALSE");
    }

    [YarnCommand("SetTreatmentTrue")]
    public void SetTreatmentTrue()
    {
        GameData.Data.treatmentCorrectAnswer = true;
        Debug.Log("Treatment Answer set to TRUE");
    }

    [YarnCommand("SetTreatmentFalse")]
    public void SetTreatmentFalse()
    {
        GameData.Data.treatmentCorrectAnswer = false;
        Debug.Log("Treatment Answer set to FALSE");
    }

    [YarnCommand("SetMythTrue")]
    public void SetMythTrue()
    {
        GameData.Data.mythCorrectAnswer = true;
        Debug.Log("Myth Answer set to TRUE");
    }

    [YarnCommand("SetMythFalse")]
    public void SetMythFalse()
    {
        GameData.Data.mythCorrectAnswer = false;
        Debug.Log("Myth Answer set to FALSE");
    }

    [YarnCommand("SetSupportTrue")]
    public void SetSupportTrue()
    {
        GameData.Data.supportCorrectAnswer = true;
        Debug.Log("Support Answer set to TRUE");
    }

    [YarnCommand("SetSupportFalse")]
    public void SetSupportFalse()
    {
        GameData.Data.supportCorrectAnswer = false;
        Debug.Log("Support Answer set to FALSE");
    }

    [YarnCommand("SetSeverityTrue")]
    public void SetSeverityTrue()
    {
        GameData.Data.severityCorrectAnswer = true;
        Debug.Log("Severity Answer set to TRUE");
    }

    [YarnCommand("SetSeverityFalse")]
    public void SetSeverityFalse()
    {
        GameData.Data.severityCorrectAnswer = false;
        Debug.Log("Severity Answer set to FALSE");
    }

    [YarnCommand("SetRecoveryTrue")]
    public void SetRecoveryTrue()
    {
        GameData.Data.recoveryCorrectAnswer = true;
        Debug.Log("Recovery Answer set to TRUE");
    }

    [YarnCommand("SetRecoveryFalse")]
    public void SetRecoveryFalse()
    {
        GameData.Data.recoveryCorrectAnswer = false;
        Debug.Log("Recovery Answer set to FALSE");
    }
}