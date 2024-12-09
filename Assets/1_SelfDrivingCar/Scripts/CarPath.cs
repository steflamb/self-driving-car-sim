﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AI;

public class PathNode
{
    public Vector3 pos;
    public Quaternion rotation;
    public string activity;
}

public class CarPath
{
    public List<PathNode> nodes;
    public List<PathNode> centerNodes;
    public NavMeshPath navMeshPath;
    public int iActiveSpan = 0;
    public string pathType = "none";


    public CarPath()
    {
        nodes = new List<PathNode>();
        centerNodes = new List<PathNode>();
        navMeshPath = new NavMeshPath();
        ResetActiveSpan();
    }

    public CarPath(string pathType) : this()
    {
        this.pathType = pathType;
        nodes = new List<PathNode>();
        centerNodes = new List<PathNode>();
        navMeshPath = new NavMeshPath();
        ResetActiveSpan();
    }

    public void ResetActiveSpan(bool sign = true)
    {
        if (sign)
            iActiveSpan = 0;
        else
            iActiveSpan = nodes.Count - 2;
    }

    public void GetClosestSpan(Vector3 carPos)
    {
        float minDistance = float.MaxValue;
        int minDistanceIndex = -1;
        for (int i = 0; i < nodes.Count; i++)
        {
            float dist = Vector3.Distance(nodes[i].pos, carPos);
            if (dist < minDistance)
            {
                minDistance = dist;
                minDistanceIndex = i;
            }
        }

        iActiveSpan = minDistanceIndex;
    }

    public List<PathNode> GetPathNodes()
    {
        //return this.nodes;
        return this.centerNodes;
    }

    public PathNode GetActiveNode()
    {
        if (iActiveSpan < nodes.Count)
            return nodes[iActiveSpan];

        return null;
    }

    public void SmoothPath(float factor = 0.5f)
    {
        LineSeg3d.SegResult segRes = new LineSeg3d.SegResult();

        for (int iN = 1; iN < nodes.Count - 2; iN++)
        {
            PathNode p = nodes[iN - 1];
            PathNode c = nodes[iN];
            PathNode n = nodes[iN + 1];

            LineSeg3d seg = new LineSeg3d(ref p.pos, ref n.pos);
            Vector3 closestP = seg.ClosestPointOnSegmentTo(ref c.pos, ref segRes);
            Vector3 dIntersect = closestP - c.pos;
            c.pos += dIntersect.normalized * factor;
        }
    }

    public double getDistance(Vector3 currentPosition, Vector3 target)
    {
        double distance = 0.0;
        if (NavMesh.CalculatePath(currentPosition, target, NavMesh.AllAreas, this.navMeshPath))
        {
            if (this.navMeshPath.corners.Length > 5)
            {
                //ignore the first corners -> more stable
                distance += (this.navMeshPath.corners[2] - currentPosition).magnitude;
                for (int i = 3; i < this.navMeshPath.corners.Length; i++)
                {
                    Vector3 start = this.navMeshPath.corners[i - 1];
                    Vector3 end = this.navMeshPath.corners[i];
                    distance += (end - start).magnitude;
                }
            }
        }
        return distance;
    }

    // public (bool, bool) GetCrossTrackErrExtended(Transform tm, ref float err, ref float angl, ref Vector3 carPosition, ref float carOrientation, ref Vector3 lookAheadPoint)
    // {
    //     Vector3 pos = tm.position;

    //     if (iActiveSpan > nodes.Count - 2)
    //     {
    //         return (false, false);
    //     }

    //     PathNode a = nodes[iActiveSpan];
    //     PathNode b = nodes[iActiveSpan + 1];

    //     // 2D path
    //     pos.y = a.pos.y;

    //     LineSeg3d pathSeg = new LineSeg3d(ref a.pos, ref b.pos);
    //     LineSeg3d.SegResult segRes = new LineSeg3d.SegResult();
    //     Vector3 closePt = pathSeg.ClosestPointOnSegmentTo(ref pos, ref segRes);
    //     Vector3 errVec = pathSeg.ClosestVectorTo(ref pos);

    //     pathSeg.Draw(Color.green);
    //     Debug.DrawLine(a.pos, closePt, Color.blue);
    //     Debug.DrawRay(closePt, errVec, Color.white);

    //     float sign = 1.0f;
    //     Vector3 cp = Vector3.Cross(pathSeg.m_dir.normalized, errVec.normalized);

    //     if (cp.y > 0.0f)
    //         sign = -1f;

    //     err = errVec.magnitude * sign;
    //     angl = CalculateSignedAngle(tm, b);

    //     if (segRes == LineSeg3d.SegResult.GreaterThanEnd || angle(tm, b) > 90)
    //     {
    //         if (iActiveSpan < nodes.Count - 2)
    //             iActiveSpan++;
    //         else
    //             return (true, false);
    //     }
    //     else if (segRes == LineSeg3d.SegResult.LessThanOrigin)
    //     {
    //         if (iActiveSpan > 0)
    //             iActiveSpan--;
    //         else
    //             return (false, true);
    //     }

    //     // Set car position and orientation
    //     carPosition = pos;
    //     carOrientation = tm.eulerAngles.y * Mathf.Deg2Rad;

    //     // Calculate the look-ahead point
    //     float distanceCovered = 0f;
    //     for (int i = iActiveSpan; i < nodes.Count - 1; i++)
    //     {
    //         Vector3 start = nodes[i].pos;
    //         Vector3 end = nodes[i + 1].pos;
    //         float segmentLength = Vector3.Distance(start, end);

    //         if (distanceCovered + segmentLength >= lookAheadDistance)
    //         {
    //             float remainingDistance = lookAheadDistance - distanceCovered;
    //             Vector3 direction = (end - start).normalized;
    //             lookAheadPoint = start + direction * remainingDistance;
    //             break;
    //         }

    //         distanceCovered += segmentLength;
    //     }

    //     return (false, false);
    // }

    // private float CalculateSignedAngle(Transform tm, PathNode b)
    // {
    //     Vector3 toTarget = (b.pos - tm.position).normalized;
    //     return Vector3.SignedAngle(tm.forward, toTarget, Vector3.up) * Mathf.Deg2Rad;
    // }

    // private float angle(Transform tm, PathNode b)
    // {
    //     return Vector3.Angle(tm.forward, (b.pos - tm.position).normalized) * Mathf.Deg2Rad;
    // }

    // Brian Fix (does not work either on GeneratedTrack scene when attempting to reset the car to a random waypoint)
    public (bool, bool) GetCrossTrackErr(Transform tm, ref float err, ref float angl)
    {
        Vector3 pos = tm.position;

        if(iActiveSpan > nodes.Count - 2)
        {
            return (false, false);
        }

        PathNode a = nodes[iActiveSpan];
        PathNode b = nodes[iActiveSpan + 1];

        //2d path.
        pos.y = a.pos.y;

        LineSeg3d pathSeg = new LineSeg3d(ref a.pos, ref b.pos);
        LineSeg3d.SegResult segRes = new LineSeg3d.SegResult();
        Vector3 closePt = pathSeg.ClosestPointOnSegmentTo(ref pos, ref segRes);
        Vector3 errVec = pathSeg.ClosestVectorTo(ref pos);

        pathSeg.Draw(Color.green);
        Debug.DrawLine(a.pos, closePt, Color.blue);
        Debug.DrawRay(closePt, errVec, Color.white);

        float sign = 1.0f;

        Vector3 cp = Vector3.Cross(pathSeg.m_dir.normalized, errVec.normalized);

        if (cp.y > 0.0f)
            sign = -1f;

        err = errVec.magnitude * sign;
        angl = CalculateSignedAngle(tm, b);

        if (segRes == LineSeg3d.SegResult.GreaterThanEnd || angle(tm, b) > 90)
        {
            if (iActiveSpan < nodes.Count - 2)
                iActiveSpan++;
            else
                return (true, false);
        }
        else if (segRes == LineSeg3d.SegResult.LessThanOrigin)
        {
            if (iActiveSpan > 0)
                iActiveSpan--;
            else
                return (false, true);
        }

        return (false, false);
    }

    // Original
    public (bool, bool) GetCrossTrackErr(Vector3 pos, ref float err)
    {

        if (iActiveSpan > nodes.Count - 4)
        {
            return (false, false);
        }

        PathNode a = nodes[iActiveSpan];
        PathNode b = nodes[iActiveSpan + 3];

        //2d path.
        pos.y = a.pos.y;

        LineSeg3d pathSeg = new LineSeg3d(ref a.pos, ref b.pos);
        LineSeg3d.SegResult segRes = new LineSeg3d.SegResult();
        Vector3 closePt = pathSeg.ClosestPointOnSegmentTo(ref pos, ref segRes);
        Vector3 errVec = pathSeg.ClosestVectorTo(ref pos);

        pathSeg.Draw(Color.green);
        Debug.DrawLine(a.pos, closePt, Color.blue);
        Debug.DrawRay(closePt, errVec, Color.white);

        float sign = 1.0f;

        Vector3 cp = Vector3.Cross(pathSeg.m_dir.normalized, errVec.normalized);

        if (cp.y > 0.0f)
            sign = -1f;

        err = errVec.magnitude * sign;
        // angl = angle(tm, b);

        if (segRes == LineSeg3d.SegResult.GreaterThanEnd)
        {
            if (iActiveSpan < nodes.Count - 2)
                iActiveSpan++;
            else
                return (true, false);
        }
        else if (segRes == LineSeg3d.SegResult.LessThanOrigin)
        {
            if (iActiveSpan > 0)
                iActiveSpan--;
            else
                return (false, true);
        }

        return (false, false);
    }

    // Computes the angle at which the car looks at the next waypoint
    private float angle(Transform transform, PathNode waypoint)
    {
        Vector3 heading = waypoint.pos - transform.position;
        heading.y = 0;

        return Quaternion.Angle(transform.rotation, Quaternion.LookRotation(heading));
    }

    private float CalculateSignedAngle(Transform transform, PathNode waypoint)
    {
        // Calculate the heading vector
        Vector3 heading = waypoint.pos - transform.position;
        heading.y = 0; // Ignore the y component for 2D calculations

        // Calculate the forward direction of the transform
        Vector3 forward = transform.forward;
        forward.y = 0; // Ignore the y component for 2D calculations

        // Calculate the angle between the forward direction and the heading
        float angle = Vector3.SignedAngle(forward, heading, Vector3.up);

        return angle;
    }



    public (float xmin, float xmax, float ymin, float ymax, float zmin, float zmax) GetPathBounds()
    {
        (float xmin, float xmax, float ymin, float ymax, float zmin, float zmax) bounds;
        bounds.xmin = bounds.ymin = bounds.zmin = float.MaxValue;
        bounds.xmax = bounds.ymax = bounds.zmax = float.MinValue;

        foreach (PathNode node in centerNodes)
        {

            Vector3 pos = node.pos;
            float x = pos.x;
            float y = pos.y;
            float z = pos.z;

            if (x < bounds.xmin)
                bounds.xmin = x;
            if (x > bounds.xmax)
                bounds.xmax = x;

            if (y < bounds.ymin)
                bounds.ymin = y;
            if (y > bounds.ymax)
                bounds.ymax = y;

            if (z < bounds.zmin)
                bounds.zmin = z;
            if (z > bounds.zmax)
                bounds.zmax = z;

        }
        return bounds;
    }
}