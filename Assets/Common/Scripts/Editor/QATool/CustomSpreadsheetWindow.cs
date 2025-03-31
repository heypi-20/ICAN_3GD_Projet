using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Debug = FMOD.Debug;

public class CustomSpreadsheetWindow : EditorWindow
{
    private Vector2 scrollPos; // Stocke la position du scroll dans la fenêtre de l'éditeur
    private const float cellWidth = 100f; // Largeur d'une cellule du tableau
    private const float cellHeight = 25f; // Hauteur d'une cellule du tableau

    // Identifiants pour récupérer les données du Google Sheet
    public string id = "1iK0tbb5mA7bE84rOALNWjRPUHevWIe_9UdO3_IYTQSo"; // INTERDICTION DE TOUCHER
    public string sheetName = "Test_Case";
    public string[] sheetDropdown = {"Test_Case", "Bug_Report"};
    public string apiKey = "AIzaSyDx_lUzjEyCufDkxhLlN-LfXyNG0k_jIdo"; // INTERDICTION DE TOUCHER

    private bool canModifyKey = false; // Contrôle si l'utilisateur peut modifier l'API Key
    private bool isLoading = false; // Empêche le rechargement multiple pendant une requête

    [MenuItem("Tools/QA/Spreadsheet Viewer")]
    public static void ShowWindow()
    {
        var window = GetWindow<CustomSpreadsheetWindow>(); // Ouvre ou crée une instance de la fenêtre
        window.titleContent = new GUIContent("Spreadsheet Viewer"); // Définit le titre de la fenêtre
        window.minSize = new Vector2(400, 300); // Définit la taille minimale de la fenêtre
        window.position = new Rect(Screen.width / 2, Screen.height / 2, 500, 400); // Centre la fenêtre à l'écran
        window.Show(); // Affiche la fenêtre
    }

    private void OnEnable()
    {
        QATool.FetchSheetData(id, sheetName, apiKey); // Charge les données dès que la fenêtre s'ouvre
    }

    private void OnGUI()
    {
        GUILayout.Label("Custom Spreadsheet", EditorStyles.boldLabel); // Affiche un titre en gras
        EditorGUILayout.HelpBox("INTERDICTION DE TOUCHER A L'ID ET A L'API KEY", MessageType.Warning);

        // Champs de texte pour saisir l'ID et le nom de la feuille Google Sheets
        GUI.enabled = false;
        id = EditorGUILayout.TextField("Spreadsheet ID", id);
        GUI.enabled = true;
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Sheet Name");
        int sheetNameIndex = Mathf.Max(0, System.Array.IndexOf(sheetDropdown, sheetName));
        sheetNameIndex = EditorGUILayout.Popup(sheetNameIndex, sheetDropdown);
        sheetName = sheetDropdown[sheetNameIndex];
        EditorGUILayout.EndHorizontal();

        string keyButtonText; // Texte temporaire du bouton pour modifier l'API Key

        EditorGUILayout.BeginHorizontal();
        if (canModifyKey) // Afficher le champ sous forme de Texte
        {
            apiKey = EditorGUILayout.TextField("API Key", apiKey);
            keyButtonText = "Valider la clé API"; // Modification du texte temportaire
        }
        else // Sinon, affiche le champ sous forme de mot de passe
        {
            GUI.enabled = canModifyKey;
            apiKey = EditorGUILayout.PasswordField("API Key", apiKey);
            GUI.enabled = true;
            keyButtonText = "Modifier la clé API"; // Modification du texte temportaire
        }

        // Bouton pour activer/désactiver l'édition de l'API Key
        if (GUILayout.Button(keyButtonText, GUILayout.Width(200))) canModifyKey = !canModifyKey;
        EditorGUILayout.EndHorizontal();

        // Bouton pour charger les données du Google Sheet - Evite trop de Request
        if (GUILayout.Button("Rafraichir les données du Google Sheet"))
        {
            // Passer le callback pour arrêter le chargement après la fin de la requête
            QATool.FetchSheetData(id, sheetName, apiKey);
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos); // Ajoute une barre de défilement

        // Vérifie si des données ont été chargées
        if (SpreadsheetUtils.SheetData != null && SpreadsheetUtils.SheetData.Count > 0)
        {
            // Calcule la taille du contenu en fonction des données récupérées
            float contentHeight = SpreadsheetUtils.SheetData.Count * cellHeight;
            float contentWidth = SpreadsheetUtils.SheetData[0].Count * cellWidth;

            EditorGUILayout.BeginVertical(GUILayout.Width(contentWidth));
            GUILayout.Space(contentHeight);

            // Déterminer le nombre de colonnes
            int columnCount = SpreadsheetUtils.SheetData[0].Count;
            int rowCount = SpreadsheetUtils.SheetData.Count;
            List<float> columnWidths = new List<float>(new float[columnCount]);
            List<float> rowHeights = new List<float>(new float[rowCount]);

            // Étape 1 : Trouver la largeur maximale de chaque colonne
            for (int j = 0; j < columnCount; j++)
            {
                float maxWidth = cellWidth; // Valeur par défaut

                for (int i = 0; i < rowCount; i++)
                {
                    string cellText = SpreadsheetUtils.SheetData[i][j];
                    float textWidth = EditorStyles.textField.CalcSize(new GUIContent(cellText)).x + 10;
                    maxWidth = Mathf.Max(maxWidth, textWidth);
                }

                columnWidths[j] = maxWidth; // Stocker la largeur maximale de cette colonne
            }

            // Étape 2 : Trouver la hauteur maximale de chaque ligne
            for (int i = 0; i < rowCount; i++)
            {
                float maxHeight = cellHeight; // Valeur par défaut

                for (int j = 0; j < columnCount; j++)
                {
                    string cellText = SpreadsheetUtils.SheetData[i][j];
                    float textHeight = EditorStyles.textField.CalcSize(new GUIContent(cellText)).y + 10;
                    maxHeight = Mathf.Max(maxHeight, textHeight);
                }

                rowHeights[i] = maxHeight; // Stocker la hauteur maximale de cette ligne
            }

            // Étape 3 : Dessiner les cellules avec la bonne largeur par colonne et hauteur par ligne
            float yOffset = 0f;
            for (int i = 0; i < rowCount; i++)
            {
                float rowHeight = rowHeights[i]; // Récupérer la hauteur de la ligne
                float xOffset = 0f;

                for (int j = 0; j < columnCount; j++)
                {
                    float columnWidth = columnWidths[j]; // Récupérer la largeur de la colonne

                    Rect cellRect = new Rect(xOffset, yOffset, columnWidth, rowHeight);
                    GUI.Box(cellRect, GUIContent.none, EditorStyles.helpBox);

                    GUI.enabled = false;
                    SpreadsheetUtils.SheetData[i][j] = EditorGUI.TextArea(cellRect, SpreadsheetUtils.SheetData[i][j],
                        EditorStyles.textField);
                    GUI.enabled = true;

                    xOffset += columnWidth; // Déplacer la position X pour la colonne suivante
                }

                yOffset += rowHeight; // Déplacer la position Y pour la ligne suivante
            }

            EditorGUILayout.EndVertical();
        }
        else
        {
            GUILayout.Label("Aucune donnée chargée.");
        }

        EditorGUILayout.EndScrollView();

        Repaint();
    }
}