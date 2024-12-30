using Codice.Client.BaseCommands;
using UnityEditor;
using UnityEngine;
[CustomEditor(typeof(FootballFormation))]
public class FootballFormation_Inspector : Editor
{

    private FootballFormation formation;

    private int remainingPlaces = 10; 
    private void OnEnable()
    {
        formation = (FootballFormation)target;
    }

    public override void OnInspectorGUI()
    {

        // Display the remaining points
        EditorGUILayout.LabelField("Remaining Points: " + remainingPlaces);


        // Display sliders for defense, midfield, and forward
        formation.defenseCount = EditorGUILayout.IntSlider("Defense", formation.defenseCount, 0, remainingPlaces + formation.defenseCount);
        formation.midfieldCount = EditorGUILayout.IntSlider("Midfield", formation.midfieldCount, 0, remainingPlaces + formation.midfieldCount);
        formation.forwardCount = EditorGUILayout.IntSlider("Forward", formation.forwardCount, 0, remainingPlaces + formation.forwardCount);

        // Calculate the total allocated points
        int totalAllocated = formation.defenseCount + formation.midfieldCount + formation.forwardCount;

        // Ensure the total allocated points do not exceed the remaining points
        if (totalAllocated > 10)
        {
            int excess = totalAllocated - 10;
            if (formation.defenseCount > 0)
            {
                formation.defenseCount -= excess;
            }
            else if (formation.midfieldCount > 0)
            {
                formation.midfieldCount -= excess;
            }
            else if (formation.forwardCount > 0)
            {
                formation.forwardCount -= excess;
            }
        }

        // Update the remaining points
       remainingPlaces = 10 - (formation.defenseCount + formation.midfieldCount + formation.forwardCount);
        
        // Apply changes to the serialized object
        if (GUI.changed)
        {
            EditorUtility.SetDirty(formation);
        }
    }
}

