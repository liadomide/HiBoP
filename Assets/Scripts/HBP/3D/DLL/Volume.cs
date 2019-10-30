﻿
/**
 * \file    Volume.cs
 * \author  Lance Florian
 * \date    2015
 * \brief   Define BBox, DLL.BBox and DLL.Volume classes
 */

// system
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

// unity
using UnityEngine;

namespace HBP.Module3D.DLL
{
    /// <summary>
    /// A DLL bounding box class
    /// </summary>
    public class BBox : Tools.DLL.CppDLLImportBase
    {
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 Min
        {
            get
            {
                float[] min = new float[3];
                getMin_BBox(_handle, min);
                return new Vector3(min[0], min[1], min[2]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 Max
        {
            get
            {
                float[] max = new float[3];
                getMax_BBox(_handle, max);
                return new Vector3(max[0], max[1], max[2]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 Center
        {
            get
            {
                float[] center = new float[3];
                getCenter_BBox(_handle, center);
                return new Vector3(center[0], center[1], center[2]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public float DiagonalLength
        {
            get
            {
                return (Max - Min).magnitude;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Vector3> Points
        {
            get
            {
                float[] points = new float[3 * 8];
                getPoints_BBox(_handle, points);
                List<Vector3> pointsV = new List<Vector3>(8);

                for (int ii = 0; ii < 8; ii++)
                {
                    pointsV.Add(new Vector3(points[3 * ii], points[3 * ii + 1], points[3 * ii + 2]));
                }

                return pointsV;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public List<Vector3> LinesPairPoints
        {
            get
            {
                float[] points = new float[3 * 24];
                getLinesPairPoints_BBox(_handle, points);
                List<Vector3> linesPoints = new List<Vector3>(24);

                for (int ii = 0; ii < 24; ii++)
                {
                    linesPoints.Add(new Vector3(points[3 * ii], points[3 * ii + 1], points[3 * ii + 2]));
                }

                return linesPoints;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="planeIntersec"></param>
        /// <returns></returns>
        public List<Vector3> IntersectionPointsWithPlane(Plane planeIntersec)
        {
            // init plane
            float[] plane = new float[6];
            for (int ii = 0; ii < 3; ++ii)
            {
                plane[ii] = planeIntersec.Point[ii];
                plane[ii + 3] = planeIntersec.Normal[ii];
            }

            float[] points = new float[8 * 3];
            getIntersectionsWithPlane_BBox(_handle, plane, points);
            List<Vector3> intersecPoints = new List<Vector3>(4);

            for (int ii = 0; ii < 8; ++ii)
            {
                Vector3 point = new Vector3(points[3 * ii], points[3 * ii + 1], points[3 * ii + 2]);
                if (point.x == 0 && point.y == 0 && point.z == 0)
                    continue;
                intersecPoints.Add(point);
            }

            return intersecPoints;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="planeIntersec"></param>
        /// <returns></returns>
        public List<Vector3> IntersectionLinesWithPlane(Plane planeIntersec)
        {
            float[] points = new float[8 * 3];
            getLinesIntersectionsWithPlane_BBox(_handle, planeIntersec.ConvertToArray(), points);
            List<Vector3> intersecLines = new List<Vector3>(8);

            for (int ii = 0; ii < 8; ++ii)
            {
                Vector3 point = new Vector3(points[3 * ii], points[3 * ii + 1], points[3 * ii + 2]);
                intersecLines.Add(point);
            }

            return intersecLines;
        }

        public List<Vector3> IntersectionSegmentBetweenTwoPlanes(Plane planeA, Plane planeB)
        {
            float[] result = new float[6];
            bool isOk = find_intersection_segment_BBox(_handle, planeA.ConvertToArray(), planeB.ConvertToArray(), result);
            if (!isOk)
            {
                return new List<Vector3>();
            }
            else
            {
                List<Vector3> intersectionSegment = new List<Vector3>();
                intersectionSegment.Add(new Vector3(result[0], result[1], result[2]));
                intersectionSegment.Add(new Vector3(result[3], result[4], result[5]));
                return intersectionSegment;
            }
        }
        #endregion

        #region Memory Management
        /// <summary>
        /// BBox default constructor
        /// </summary>
        public BBox()
        {
            _handle = new HandleRef(this,create_BBox());
        }
        /// <summary>
        /// BBox constructor with an already allocated dll BBox
        /// </summary>
        /// <param name="bBoxPointer"></param>
        public BBox(IntPtr bBoxPointer)
        {
            _handle = new HandleRef(this, bBoxPointer);
        }
        /// <summary>
        /// Allocate DLL memory
        /// </summary>
        protected override void create_DLL_class()
        {
            _handle = new HandleRef(this, create_BBox());
        }
        /// <summary>
        /// Clean DLL memory
        /// </summary>
        protected override void delete_DLL_class()
        {
            delete_BBox(_handle);
        }
        #endregion

        #region DLLImport

        // memory management
        [DllImport("hbp_export", EntryPoint = "create_BBox", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr create_BBox();

        [DllImport("hbp_export", EntryPoint = "delete_BBox", CallingConvention = CallingConvention.Cdecl)]
        static private extern void delete_BBox(HandleRef handleBBox);

        // retrieve data
        [DllImport("hbp_export", EntryPoint = "getMin_BBox", CallingConvention = CallingConvention.Cdecl)]
        static private extern void getMin_BBox(HandleRef handleBBox, float[] min);

        [DllImport("hbp_export", EntryPoint = "getMax_BBox", CallingConvention = CallingConvention.Cdecl)]
        static private extern void getMax_BBox(HandleRef handleBBox, float[] max);

        [DllImport("hbp_export", EntryPoint = "getPoints_BBox", CallingConvention = CallingConvention.Cdecl)]
        static private extern void getPoints_BBox(HandleRef handleBBox, float[] points);

        [DllImport("hbp_export", EntryPoint = "getLinesPairPoints_BBox", CallingConvention = CallingConvention.Cdecl)]
        static private extern void getLinesPairPoints_BBox(HandleRef handleBBox, float[] points);

        [DllImport("hbp_export", EntryPoint = "getIntersectionsWithPlane_BBox", CallingConvention = CallingConvention.Cdecl)]
        static private extern void getIntersectionsWithPlane_BBox(HandleRef handleBBox, float[] plane, float[] interPoints);

        [DllImport("hbp_export", EntryPoint = "getLinesIntersectionsWithPlane_BBox", CallingConvention = CallingConvention.Cdecl)]
        static private extern void getLinesIntersectionsWithPlane_BBox(HandleRef handleBBox, float[] plane, float[] interPoints);

        [DllImport("hbp_export", EntryPoint = "find_intersection_segment_BBox", CallingConvention = CallingConvention.Cdecl)]
        static private extern bool find_intersection_segment_BBox(HandleRef handleBBox, float[] planeA, float[] planeB, float[] interPoints);

        [DllImport("hbp_export", EntryPoint = "getCenter_BBox", CallingConvention = CallingConvention.Cdecl)]
        static private extern void getCenter_BBox(HandleRef handleBBox, float[] center);

        // memory management
        //delegate IntPtr create_BBox();
        //delegate void delete_BBox(HandleRef handleBBox);

        //// retrieve data
        //delegate void getMin_BBox(HandleRef handleBBox, float[] min);
        //delegate void getMax_BBox(HandleRef handleBBox, float[] max);
        //delegate void getPoints_BBox(HandleRef handleBBox, float[] points);
        //delegate void getLinesPairPoints_BBox(HandleRef handleBBox, float[] points);
        //delegate void getIntersectionsWithPlane_BBox(HandleRef handleBBox, float[] plane, float[] interPoints);
        //delegate void getLinesIntersectionsWithPlane_BBox(HandleRef handleBBox, float[] plane, float[] interPoints);
        //delegate void getCenter_BBox(HandleRef handleBBox, float[] center);

        #endregion
    }

    /// <summary>
    /// A DLL volume class converted from NII files
    /// </summary>
    public class Volume : Tools.DLL.CppDLLImportBase
    {
        #region Properties
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 Center
        {
            get
            {
                float[] center = new float[3];
                center_Volume(_handle, center);
                return new Vector3(center[0], center[1], center[2]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 Spacing
        {
            get
            {
                float[] spacing = new float[3];
                spacing_Volume(_handle, spacing);
                return new Vector3(spacing[0], spacing[1], spacing[2]);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public MRICalValues ExtremeValues
        {
            get
            {
                MRICalValues values = new MRICalValues();

                float[] valuesF = new float[6];
                retrieveExtremeValues_Volume(_handle, valuesF);

                values.Min = valuesF[0];
                values.Max = valuesF[1];
                values.LoadedCalMin = valuesF[2];
                values.LoadedCalMax = valuesF[3];
                values.ComputedCalMin = valuesF[4];
                values.ComputedCalMax = valuesF[5];

                return values;
            }
        }
        /// <summary>
        /// Bounding Box of this volume
        /// </summary>
        public BBox BoundingBox
        {
            get
            {
                return new BBox(boundingBox_Volume(_handle));
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cutPlane"></param>
        /// <param name="nbCuts"></param>
        /// <returns></returns>
        public float SizeOffsetCutPlane(Plane cutPlane, int nbCuts)
        {
            return sizeOffsetCutPlane_Volume(_handle, cutPlane.ConvertToArray(), nbCuts);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="plane"></param>
        /// <param name="idOrientation"></param>
        /// <param name="flip"></param>
        public void SetPlaneWithOrientation(Plane plane, Data.Enums.CutOrientation orientation, bool flip)
        {
            float[] normal = new float[3];
            definePlaneWithOrientation_Volume(_handle, normal, (int)orientation, flip);
            plane.Normal = new Vector3(normal[0], normal[1], normal[2]);
        }
        /// <summary>
        /// Returns a cube bbox around the mesh depending on the cuts used
        /// </summary>
        /// <param name="cuts"></param>
        /// <returns></returns>
        public BBox GetCubeBoundingBox(List<Cut> cuts)
        {
            float[] planes = new float[cuts.Count * 6];
            int planesCount = 0;
            for (int ii = 0; ii < cuts.Count; ++ii)
            {
                if (cuts[ii].Orientation != Data.Enums.CutOrientation.Custom)
                {
                    for (int jj = 0; jj < 3; ++jj)
                    {
                        planes[ii * 6 + jj] = cuts[ii].Point[jj];
                        planes[ii * 6 + jj + 3] = cuts[ii].Normal[jj];
                    }
                    planesCount++;
                }
            }
            return new BBox(cube_bounding_box_Volume(_handle, planes, planesCount));
        }
        #endregion

        #region Memory Management
        /// <summary>
        /// Allocate DLL memory
        /// </summary>
        protected override void create_DLL_class()
        {
            _handle = new HandleRef(this, create_Volume());
        }
        /// <summary>
        /// Clean DLL memory
        /// </summary>
        protected override void delete_DLL_class()
        {
            delete_Volume(_handle);
        }
        #endregion

        #region DLLimport

        // memory management
        [DllImport("hbp_export", EntryPoint = "create_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr create_Volume();

        [DllImport("hbp_export", EntryPoint = "delete_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern void delete_Volume(HandleRef handleVolume);

        // retrieve data
        [DllImport("hbp_export", EntryPoint = "center_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern void center_Volume(HandleRef handleVolume, float[] center);

        [DllImport("hbp_export", EntryPoint = "bBox_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern void bBox_Volume(HandleRef handleVolume, float[] minMax);

        [DllImport("hbp_export", EntryPoint = "diagonalLenght_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern float diagonalLenght_Volume(HandleRef handleVolume);

        [DllImport("hbp_export", EntryPoint = "boundingBox_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr boundingBox_Volume(HandleRef handleVolume);

        [DllImport("hbp_export", EntryPoint = "spacing_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern void spacing_Volume(HandleRef handleVolume, float[] spacing);

        [DllImport("hbp_export", EntryPoint = "definePlaneWithOrientation_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern void definePlaneWithOrientation_Volume(HandleRef handleVolume, float[] planeNormal, int idOrientation, bool flip);

        [DllImport("hbp_export", EntryPoint = "sizeOffsetCutPlane_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern float sizeOffsetCutPlane_Volume(HandleRef handleVolume, float[] planeCut, int nbCuts);

        [DllImport("hbp_export", EntryPoint = "retrieveExtremeValues_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern void retrieveExtremeValues_Volume(HandleRef handleVolume, float[] extremeValues);

        [DllImport("hbp_export", EntryPoint = "cube_bounding_box_Volume", CallingConvention = CallingConvention.Cdecl)]
        static private extern IntPtr cube_bounding_box_Volume(HandleRef handleSurface, float[] planes, int planesCount);

        

        //// memory management        
        //delegate IntPtr create_Volume();
        //delegate void delete_Volume(HandleRef handleVolume);

        //// retrieve data
        //delegate void center_Volume(HandleRef handleVolume, float[] center);
        //delegate void bBox_Volume(HandleRef handleVolume, float[] minMax);
        //delegate float diagonalLenght_Volume(HandleRef handleVolume);
        //delegate IntPtr boundingBox_Volume(HandleRef handleVolume);
        //delegate void spacing_Volume(HandleRef handleVolume, float[] spacing);
        //delegate void definePlaneWithOrientation_Volume(HandleRef handleVolume, float[] planeNormal, int idOrientation, bool flip);
        //delegate float sizeOffsetCutPlane_Volume(HandleRef handleVolume, float[] planeCut, int nbCuts);
        //delegate void retrieveExtremeValues_Volume(HandleRef handleVolume, float[] extremeValues);

        #endregion
    }
}