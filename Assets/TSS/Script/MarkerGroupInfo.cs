using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCvSharp;
using OpenCvSharp.Aruco;

[CreateAssetMenu(fileName = "MarkerGroupInfo", menuName = "MarkerGroupInfo")]
[Serializable]
public class MarkerGroupInfo : ScriptableObject
{
    public string label;
    public MarkerInfo[] setMarkers;

    //private Dictionary<int, (MarkerInfo markerInfo, Point2f ptCenter)> sortedDic = new Dictionary<int, (MarkerInfo, Point2f ptCenter)>();
    private List<(MarkerInfo markerInfo, Point2f ptCenter)> sortedList = new List<(MarkerInfo, Point2f ptCenter)>();

    /// <summary>
    /// Returns the position and angle of the marker group.
    /// </summary>
    /// <param name="ptCenters">Array of marker center coordinates that in the same coordinate system as the set coordinates.</param>
    /// <param name="ids"></param>
    /// <returns></returns>
    public MarkerGroupPose GetGroupPose(Point2f[] ptCenters, int[] ids)
    {
        sortedList.Clear();
        for(int inputId = 0; inputId < ids.Length; inputId++)//inputId is an index of ptCenters & ids.
        {
            int inputMarkerId = ids[inputId];
            
            for(int infoId = 0; infoId < setMarkers.Length; infoId++)//infoId is an index of setMarkers.
            {
                if(inputMarkerId == setMarkers[infoId].id)
                {
                    bool addFlag = true;
                    for(int listId = 0; listId < sortedList.Count; listId++)
                    {
                        if(sortedList[listId].markerInfo.id == setMarkers[infoId].id)
                        {
                            addFlag = false;
                            break;
                        }
                    }
                    if (addFlag)
                    {
                        sortedList.Add((setMarkers[infoId], ptCenters[inputId]));
                    }
                    
                }
            }
        }

        if(sortedList.Count < setMarkers.Length)
        {
            return null;
        }

        Point2f subVectorSum = new Point2f(0, 0);

        for (int i = 0; i < sortedList.Count - 1; i++)
        {
            for(int j = i + 1; j < sortedList.Count; j++)
            {
                Point2f setVector = sortedList[j].markerInfo.relatedPosition - sortedList[i].markerInfo.relatedPosition;

                Point2f inputVector = sortedList[j].ptCenter - sortedList[i].ptCenter;

                float setRad = Mathf.Atan2(setVector.Y, setVector.X);
                float inputRad = Mathf.Atan2(inputVector.Y, inputVector.X);

                float subRad = Mathf.Repeat(inputRad - setRad, Mathf.PI * 2);

                subVectorSum.X += Mathf.Sin(subRad);
                subVectorSum.Y += Mathf.Cos(subRad);
            }
        }
        float resultRad = Mathf.Atan2(subVectorSum.Y, subVectorSum.X) - 1.5708f;

        float sumX = 0;
        float sumY = 0;
        for(int i = 0; i < sortedList.Count; i++)
        {
            Point2f setVector = sortedList[i].markerInfo.relatedPosition;
            float setMagnitude = Mathf.Sqrt(Mathf.Pow(setVector.X, 2) + Mathf.Pow(setVector.Y, 2));
            float dstRad = Mathf.Atan2(setVector.Y, setVector.X) + resultRad;
            Point2f rotatedSetVector = new Point2f(Mathf.Sin(dstRad) * setMagnitude, Mathf.Cos(dstRad) * setMagnitude);
            Point2f subVector = sortedList[i].ptCenter - rotatedSetVector;
            sumX += subVector.X;
            sumY += subVector.Y;
        }
        Point2f resultPosition = new Point2f(sumX / sortedList.Count, sumY / sortedList.Count);

        return new MarkerGroupPose(resultPosition, resultRad);
    }

    
}
