using UnityEditor;
using UnityEngine;
using Utility;

[CustomEditor(typeof(Utility.TestRunner))]
public class TestRunner_Inspector : Editor
{
    public override void OnInspectorGUI()
    {
        Utility.TestRunner myTarget = (Utility.TestRunner)target;

        DrawDefaultInspector();

        if (GUILayout.Button("Execute Test"))
        {
            myTarget.ExecuteAndPrintTest();
        }
    }

}
