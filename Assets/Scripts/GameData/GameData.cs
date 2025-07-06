using System;

[Serializable]
public class GameData
{
    public static GameData Data = new GameData();

    // Variabel jawaban untuk setiap kategori
    public bool definitionHexagonCorrectAnswer;  // Jawaban Definisi Depresi
    public bool symptomCorrectAnswer;            // Jawaban Gejala Depresi
    public bool causeCorrectAnswer;              // Jawaban Penyebab Depresi
    public bool treatmentCorrectAnswer;          // Jawaban Penanganan
    public bool mythCorrectAnswer;               // Jawaban Membedakan Mitos dan Fakta
    public bool supportCorrectAnswer;            // Jawaban Cara Memberi Dukungan
    public bool severityCorrectAnswer;           // Jawaban Tingkat Keparahan
    public bool recoveryCorrectAnswer;           // Jawaban Proses Pemulihan

    // Fungsi untuk mereset semua jawaban ke false
    public void ResetAllAnswers()
    {
        definitionHexagonCorrectAnswer = false;
        symptomCorrectAnswer = false;
        causeCorrectAnswer = false;
        treatmentCorrectAnswer = false;
        mythCorrectAnswer = false;
        supportCorrectAnswer = false;
        severityCorrectAnswer = false;
        recoveryCorrectAnswer = false;
    }
}