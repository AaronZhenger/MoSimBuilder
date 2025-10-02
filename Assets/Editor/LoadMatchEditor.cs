using UnityEditor;
using UnityEngine;
using System.Linq;

[CustomEditor(typeof(LoadMatch))]
public class LoadMatchEditor : Editor
{
    private LoadMatch loadMatch;

    private void OnEnable()
    {
        loadMatch = (LoadMatch)target;
    }

    public override void OnInspectorGUI()
    {
        // Call the base method to draw all other serialized fields (like fieldPrefab, spawnPoint, etc.)
        DrawDefaultInspector();

        // ----------------------------------------------------------------
        // ROBOT DROPDOWN LOGIC
        // ----------------------------------------------------------------
        
        GUILayout.Space(10);
        EditorGUILayout.LabelField("Robot Selector", EditorStyles.boldLabel);

        // 1. Ensure the list of available robots is up-to-date
        // We call this here to ensure the dropdown list is current every time the inspector is drawn.
        loadMatch.CheckRobots(); 

        if (loadMatch.availableRobots.Count == 0)
        {
            EditorGUILayout.HelpBox("No robot prefabs found in Resources/Robots folder.", MessageType.Warning);
        }
        else
        {
            // 2. Get the names for the dropdown options
            string[] robotNames = loadMatch.availableRobots.Select(robot => robot.name).ToArray();
            
            // 3. Get the serialized property for 'selectedRobotIndex'

            // 4. Draw the dropdown (Popup)
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Selected Robot", loadMatch.selectedRobotIndex, robotNames);
            
            // 5. Apply changes if the user selected a new robot
            if (EditorGUI.EndChangeCheck())
            {
                loadMatch.selectedRobotIndex = newIndex;
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}