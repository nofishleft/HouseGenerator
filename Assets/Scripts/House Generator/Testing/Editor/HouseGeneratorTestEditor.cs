using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HouseGeneratorTest))]
public class HouseGeneratorTestEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var tester = target as HouseGeneratorTest;

        if (GUILayout.Button("Generate")) tester.Test();
    }
}
