using System;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class ReadGoogleSheet : MonoBehaviour
{
    public float timeBeforeNewRequest = 2f; // La durée du timer
    private float t; // Le temps actuel du Timer

    public string id = "1iK0tbb5mA7bE84rOALNWjRPUHevWIe_9UdO3_IYTQSo";
    public string sheetName = "Test_Case";
    public string apiKey = "AIzaSyDx_lUzjEyCufDkxhLlN-LfXyNG0k_jIdo";

    public TextMeshProUGUI text;

    private void Start()
    {
        StartCoroutine(ObtainSheetData());
    }

    private void Update()
    {
        t += Time.deltaTime;
        
        if(t >= timeBeforeNewRequest)
        {
            t = 0f;
            StartCoroutine(ObtainSheetData());
        }
    }

    IEnumerator ObtainSheetData()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://sheets.googleapis.com/v4/spreadsheets/" + id + "/values/" + sheetName + "?key=" + apiKey);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.ConnectionError || www.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("ERROR: " + www.error);
        }
        else
        {
            string updateText = "";

            // Récupération du JSON
            string json = www.downloadHandler.text;
            var o = JSON.Parse(json);

            // Vérifier si "values" existe
            if (o.HasKey("values"))
            {
                var valuesArray = o["values"].AsArray;

                // On commence à partir de la deuxième ligne (index 1), car la première contient les headers
                for (int i = 1; i < valuesArray.Count; i++)
                {
                    var row = valuesArray[i].AsArray;
                
                    // Vérifier que la ligne a assez de colonnes avant d'accéder aux index
                    string caseName = row.Count > 0 ? row[0] : "N/A";
                    string status = row.Count > 1 ? row[1] : "N/A";
                    string comment = row.Count > 3 ? row[3] : "N/A";

                    updateText += caseName + " | " + status + " | " + comment + "\n";
                }
            }
            else
            {
                updateText = "Aucune donnée trouvée.";
            }

            text.text = updateText;
        }
    }
}
