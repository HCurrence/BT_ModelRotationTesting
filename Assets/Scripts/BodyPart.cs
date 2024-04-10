using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BodyPart : MonoBehaviour
{
    public GameObject JointA, JointB;
    public float rotationSpeed = 1;

    private Quaternion rotation;

    private Vector3 jointAPos, jointBPos, prevAPos, testGizmo;
    private float targetXAngle, targetYAngle, targetZAngle;

    private Vector3 forward;
    private Vector3 backwards;
    private Vector3 upwards;
    private Vector3 downwards;
    private Vector3 right;
    private Vector3 left;

    private void Start()
    {
        rotation = transform.rotation;

        prevAPos = JointA.transform.position;
        jointAPos = JointA.transform.position;
        jointBPos = JointB.transform.position;

        // ***** POSITION ***** //
        //Full model will start in a T-pose. Body points will either be vertically (y) 
        //or horizontally(x or z) between joints.
        if(!arePointsOnTheSameLine(jointAPos, jointBPos, transform.position) || !isPointInTheMiddle(jointAPos, jointBPos, transform.position))
        {
            transform.position = (jointAPos + JointB.transform.position) / 2.0f;
        }

        // ***** SCALE ***** //

        // Scale this object to fill the space between JointA and JointB
        // TODO: Scale when line is not straight?
        Vector3 line = jointBPos - jointAPos;
        Vector3 newLocalScale = transform.localScale;
        float offset = 2;

        if (areTwoLinesParallel(line, Vector3.right))
        {
            newLocalScale.x = line.magnitude - offset;
        }

        if (areTwoLinesParallel(line, Vector3.up))
        {
            newLocalScale.y = line.magnitude - offset;
        }

        if (areTwoLinesParallel(line, Vector3.forward))
        {
            newLocalScale.z = line.magnitude - offset;
        }

        transform.localScale = newLocalScale;

        // ***** ROTATION ***** //

        // Rotates so that the object faces perpendicular to the line AB
        float initYRotation = 0, initZRotation = 0, initXRotation = 0;
        forward = transform.position + (transform.forward * 5.0f);
        backwards = transform.position - (transform.forward * 5.0f);
        upwards = transform.position + (transform.up * 5.0f);
        downwards = transform.position - (transform.up * 5.0f);
        right = transform.position + (transform.right * 5.0f);
        left = transform.position + (transform.right * 5.0f);

        float startingForwardAngle = Vector3.Angle((forward - transform.position).normalized, (jointAPos - transform.position).normalized);
        float startingLeftAngle = Vector3.Angle((right - transform.position).normalized, (jointAPos - transform.position).normalized);
        float startingDownAngle = Vector3.Angle((downwards - transform.position).normalized, (jointAPos - transform.position).normalized);

        // Calculating the target X rotation based on the rotation of this object at the start.
        if ((Mathf.Approximately(startingDownAngle % 90, 0) && Mathf.Approximately(startingForwardAngle % 90, 0))
            || (Mathf.Approximately(startingDownAngle % 90, 0) || Mathf.Approximately(startingForwardAngle % 90, 0)))
        {
            initXRotation = 0;
            targetXAngle = startingForwardAngle;
        }
        else if (!Mathf.Approximately(startingDownAngle % 90, 0))
        {
            initXRotation = calculateAxisRotation(left, downwards, 90);
            targetXAngle = 90.0f;
        }

        // Calculating the target Y rotation based on the rotation of this object at the start.
        if ((Mathf.Approximately(startingForwardAngle % 90, 0) && Mathf.Approximately(startingLeftAngle % 90, 0))
            || (Mathf.Approximately(startingForwardAngle % 90, 0) || Mathf.Approximately(startingLeftAngle % 90, 0)))
        {
            initYRotation = 0;
            targetXAngle = startingLeftAngle;
        }
        else if(!Mathf.Approximately(startingForwardAngle, 90.0f))
        {
            initYRotation = calculateAxisRotation(upwards, forward, 90, true);
            targetYAngle = 90.0f;
        }

        // Calculating the target Z rotation based on the rotation of this object at the start.
        if ((Mathf.Approximately(startingDownAngle % 90, 0) && Mathf.Approximately(startingLeftAngle % 90, 0))
            || (Mathf.Approximately(startingDownAngle % 90, 0) || Mathf.Approximately(startingLeftAngle % 90, 0)))
        {
            initZRotation = 0;
            targetZAngle = startingLeftAngle;
        }
        else if (!Mathf.Approximately(startingDownAngle, 90))
        {
            initZRotation = calculateAxisRotation(forward, left, 90);
            targetZAngle = 90.0f;
        }

        //Debug.Log("Down: " + startingDownAngle);
        //Debug.Log("Forward: " + startingForwardAngle);
        //Debug.Log("Left: " + startingLeftAngle);
        Debug.Log("Target Angles: " + targetXAngle + ", " + targetYAngle + ", " + targetZAngle);

       // Debug.Log("Init Target Rotations: " + initXRotation + ", " + initYRotation + ", " + initZRotation);

        Quaternion xRot = Quaternion.AngleAxis(initXRotation, transform.right);
        Quaternion yRot = Quaternion.AngleAxis(initYRotation, transform.up);
        Quaternion zRot = Quaternion.AngleAxis(initZRotation, transform.forward);
        //Debug.Log("Init Quaternion Rotations: " + xRot.eulerAngles + " " + yRot.eulerAngles + " " + zRot.eulerAngles);

        Quaternion finalInitRot = (xRot * yRot * zRot);
        //Debug.Log("Final Init Rotation: " + finalInitRot.eulerAngles);
        transform.rotation *= finalInitRot;
    }

    private void FixedUpdate()
    {
        // Update Local Axis Directions
        forward = transform.position + (transform.forward * 2.0f); // +Z
        backwards = transform.position - (transform.forward * 2.0f); // -Z
        upwards = transform.position + (transform.up * 2.0f); // +Y
        downwards = transform.position - (transform.up * 2.0f); // -Y
        right = transform.position + (transform.right * 2.0f); // +X
        left = transform.position + (transform.right * 2.0f); // -X

        // Update Joint Positions
        if (!jointAPos.Equals(JointA.transform.position))
        {
            prevAPos = jointAPos;
        }
        jointAPos = JointA.transform.position;
        jointBPos = JointB.transform.position;

        // ***** POSITION ***** //
        // - Ensures the object is between the two joints
        transform.position = (jointAPos + jointBPos) / 2.0f;

        // ***** ROTATION ***** //
        // - Ensures the object is perpendicular to line AB.
        float targetXRotation = calculateAxisRotation(left, downwards, targetXAngle);
        float targetYRotation = calculateAxisRotation(upwards, forward, targetYAngle);
        float targetZRotation = calculateAxisRotation(forward, left, targetZAngle);
        Debug.Log("Target Rotations: " + targetXRotation + ", " + targetYRotation + ", " + targetZRotation);

        Quaternion xRot = Quaternion.AngleAxis(targetXRotation, transform.right);
        Quaternion yRot = Quaternion.AngleAxis(targetYRotation, transform.up);
        Quaternion zRot = Quaternion.AngleAxis(targetZRotation, transform.forward);
        Debug.Log("Quaternion Rotations: " + xRot.eulerAngles + " " + yRot.eulerAngles + " " + zRot.eulerAngles);

        Quaternion finalRot = xRot * yRot * zRot;
        Debug.Log("Final Rotation: " + finalRot.eulerAngles);
        transform.rotation *= yRot;
    }

    private float calculateAxisRotation(Vector3 axis, Vector3 forward, float targetAngle, bool debug = false)
    {
        float axisRotation = 0.0f;
        string debugStr = "";
        
        float angleAXF = Vector3.SignedAngle((forward - transform.position),
                                         (jointAPos - transform.position),
                                         axis);

        if(debug)
        {
            debugStr += "Target Angle: " + targetAngle + " = ";
            debugStr += "AngleAXF: " + angleAXF + " - ";
        }
        

        if (!Mathf.Approximately(angleAXF, targetAngle))
        {
            if (debug)
            {
                // Determines if rotating left (-) or right (+)
                bool turnRight = Vector3.SignedAngle(jointAPos - jointBPos, prevAPos - jointBPos, axis) < 0.0f;
                debugStr += $" (Turning Right?: {turnRight}) ";
            }

            // Calculates how far the object needs to rotate to get to the
            // starting angle.

            if (angleAXF > 0)
            {
                axisRotation = (targetAngle - (180.0f - angleAXF));
            }
            else
            {
                axisRotation = (targetAngle - (180.0f + angleAXF));
            }
        }

        if(debug)
        {
            debugStr += "Rot: " + axisRotation;
            Debug.Log(debugStr);
        }

        return axisRotation;
    }

    bool isPointInTheMiddle(Vector3 start, Vector3 end, Vector3 middle)
    {
        return Vector3.Dot((end - start).normalized, (middle - end).normalized) < 0f && Vector3.Dot((start - end).normalized, (middle - start).normalized) < 0f;
    }

    bool arePointsOnTheSameLine(Vector3 A, Vector3 B, Vector3 C)
    {
        return Mathf.Approximately(
            (B-A).magnitude,
            (B-C).magnitude
        );
    }

    bool areTwoLinesParallel(Vector3 lineA, Vector3 lineB)
    {
        return Vector3.Dot(lineA.normalized, lineB.normalized) == 1
            || Vector3.Dot(lineA.normalized, lineB.normalized) == -1;
    }

    bool areTwoLinesPerpendicular(Vector3 lineA, Vector3 lineB)
    {
        return Vector3.Dot(lineA.normalized, lineB.normalized) == 0;
    }
    bool areTwoLinesSameDirection(Vector3 lineA, Vector3 lineB)
    {
        return Vector3.Dot(lineA.normalized, lineB.normalized) == 1;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(jointAPos, jointBPos);

        Handles.color = Color.green;
        Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(transform.up), 1.0f, EventType.Repaint);
        Handles.color = Color.red;
        Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(transform.right), 1.0f, EventType.Repaint);
        Handles.color = Color.blue;
        Handles.ArrowHandleCap(0, transform.position, Quaternion.LookRotation(transform.forward), 1.0f, EventType.Repaint);
    }

}
