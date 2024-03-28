using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MouseInteractivity)), CanEditMultipleObjects]
public class MouseInteractivityEditor : Editor
{
    protected virtual void OnSceneGUI()
    {
        MouseInteractivity mouseInteractivity = (MouseInteractivity)target;

        EditorGUI.BeginChangeCheck();

        //Position Handle
        Vector3 newTargetPosition = Handles.PositionHandle(mouseInteractivity.targetPosition, Quaternion.identity);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(mouseInteractivity, "Change Look At Target Position");
            mouseInteractivity.targetPosition = newTargetPosition;
            mouseInteractivity.Update();
        }

        //Rotation Handle
        Quaternion rot = Handles.RotationHandle(mouseInteractivity.rot, mouseInteractivity.targetPosition);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Rotated RotateAt Point");
            mouseInteractivity.rot = rot;
            mouseInteractivity.Update();
        }
    }
}
