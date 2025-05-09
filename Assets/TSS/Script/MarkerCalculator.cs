using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;


public class MarkerCalculator
{

    public static Point2f GetMarkerCenter(Point2f[] corners)
    {
        Point2f ave = new Point2f(0, 0);
        for(int i = 0; i < corners.Length; i++)
        {
            ave += corners[i];
        }
        ave.X = ave.X / corners.Length;
        ave.Y = ave.Y / corners.Length;

        return ave;
    }

    public static float GetMarkerLength(Point2f[] corners)
    {
        Point2f cornerVec = (corners[0] - corners[3]);
        return Mathf.Sqrt(Mathf.Pow(cornerVec.X, 2) + Mathf.Pow(cornerVec.Y, 2));
    }

    public static float GetMarkerRotation(Point2f[] corners)
    {
        //Simple version
        float x = corners[0].X - corners[3].X;
        float y = corners[0].Y - corners[3].Y;
        return Mathf.Atan2(y, x);
    }
}

public class MarkerGroupManager
{
    private Dictionary<int, Point2f[]> markerDic = new Dictionary<int, Point2f[]>();
    private Dictionary<int, Pose> poseDic = new Dictionary<int, Pose>();

    private void WriteMarkerDic(Point2f[][] corners, int[] ids)
    {
        markerDic.Clear();
        for (int elemId = 0; elemId < ids.Length; elemId++)
        {
            if (!markerDic.ContainsKey(ids[elemId]))
            {
                markerDic.Add(ids[elemId], corners[elemId]);
            }
        }
    }

    public void SortMarkersForGroups(Point2f[][] corners, int[] ids)
    {
        for (int elemId = 0; elemId < ids.Length; elemId++)
        {

        }
    }
    /*
    public Dictionary<int, MarkerGroup.Pose> GetPoses(Point2f[][] corners, int[] ids)
    {
        poseDic.Clear();
        for(int groupId = 0; groupId < markerGroups.Length; groupId++)
        {
            poseDic.Add(markerGroups[groupId].)
        }
    }
    */
}

[Serializable]
public class MarkerInfo
{
    public int id;

    /// <summary>
    /// Enter the marker length in meters.
    /// </summary>
    public float markerLength = 0.02f;

    /// <summary>
    /// Enter the relative positions of each markers in meters from the direction in which the marker pattern can be seen.
    /// </summary>
    public Point2f relatedPosition = new Point2f();

    public MarkerInfo(int id, float markerLength, Point2f relatedPosition)
    {
        this.id = id;
        this.markerLength = markerLength;
        this.relatedPosition = relatedPosition;
    }

}

[Serializable]
public class MarkerGroupPose
{
    public Point2f position;
    public float rotation;

    public MarkerGroupPose(Point2f position, float rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}