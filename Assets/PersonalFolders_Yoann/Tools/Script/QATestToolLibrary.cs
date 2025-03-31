using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using Unity.EditorCoroutines.Editor;

public static class SpreadsheetUtils
{
    public static List<List<string>> SheetData { get; private set; } = new List<List<string>>();
    
    /// <summary>
    /// ...
    /// </summary>
    public static void SetSheetData(List<List<string>> newData)
    {
        SheetData = newData;
    }
}

public static class QATool
{
    #region ObtainDataFromSheets
    
    /// <summary>
    /// ...
    /// </summary>
    public static void FetchSheetData(string id, string sheetName, string apiKey, System.Action onComplete = null)
    {
        EditorCoroutineUtility.StartCoroutineOwnerless(ObtainSheetData(id, sheetName, apiKey, onComplete));
    }
    
    /// <summary>
    /// ...
    /// </summary>
    public static IEnumerator ObtainSheetData(string id, string sheetName, string apiKey, System.Action onComplete = null)
    {
        UnityWebRequest www = UnityWebRequest.Get("https://sheets.googleapis.com/v4/spreadsheets/" + id + "/values/" + sheetName + "?key=" + apiKey);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("ERROR: " + www.error);
        }
        else
        {
            // Récupération du JSON
            string json = www.downloadHandler.text;
            var o = JSON.Parse(json);

            // Vérifier si "values" existe
            if (o.HasKey("values"))
            {
                var valuesArray = o["values"].AsArray;
                int rowCount = valuesArray.Count;
                int columnCount = valuesArray[0].AsArray.Count;
                
                // Initialiser les données avec la bonne taille
                List<List<string>> newData = new List<List<string>>();

                
                for (int i = 0; i < rowCount; i++)
                {
                    List<string> row = new List<string>();
                    var rowData  = valuesArray[i].AsArray;
                    for (int j = 0; j < columnCount; j++)
                    {
                        row.Add(rowData.Count > j ? rowData[j] : "");
                    }
                    newData.Add(row);
                }
                
                SpreadsheetUtils.SetSheetData(newData); // Stocke les nouvelles données
                Debug.Log("Données récupérées avec succès !");
            }
            else
            {
                Debug.LogWarning("Aucune donnée trouvée dans le Google Sheet.");
            }
        }
        
        onComplete?.Invoke();
    }
    
    #endregion
    
    #region Bug Report

    public static string category;
    public static string severity;
    public static string reproductibility;
    public static string summary;
    public static string description;
    public static string reproSteps;

    public static string submitStatus;

    [SerializeField] private static string BASE_URL =
        "https://docs.google.com/forms/u/0/d/1cwljRmn3eJH_t3v6x9gdwMvEQy5IJlUMOsjhY2UfRTQ/formResponse";

    public static IEnumerator Send(string category, string severity, string reproductibility, string summary, string description,
        string reproSteps)
    {

        submitStatus = "Getting Input";
        WWWForm form = new WWWForm();
        form.AddField("entry.660508839", category);
        form.AddField("entry.871233109", severity);
        form.AddField("entry.545369044", reproductibility);
        form.AddField("entry.1111863239", summary);
        form.AddField("entry.1063225769", description);
        form.AddField("entry.1222736087", reproSteps);
        
        using (UnityWebRequest www = UnityWebRequest.Post(BASE_URL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                submitStatus = "Error: " + www.error;
            }
            else
            {
                submitStatus = "Bug Report Submitted Successfully!";
            }
        }
    }

    public static void SubmitBugReport()
    {
        if (summary == null || reproSteps == null || submitStatus == null)
        {
            submitStatus = "At least one field is empty !";
        }
        else
        {
            submitStatus = "Submitting Bug Report";
            EditorCoroutineUtility.StartCoroutineOwnerless(Send(category, severity, reproductibility, summary, description, reproSteps));
        }
    }

    #endregion
}