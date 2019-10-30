﻿

/**
 * \file    TexturesGenerator.cs
 * \author  Lance Florian
 * \date    2015
 * \brief   Define UITextureGenerator and DLLTextureGenerator classes
 */

// system
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Tools.CSharp;

// unity
using UnityEngine;

namespace HBP.Module3D
{
    namespace DLL
    {
        /// <summary>
        /// Generate UV for brain surfaces
        /// </summary>
        public class MRIBrainGenerator : Tools.DLL.CppDLLImportBase, ICloneable
        {
            #region Properties
            public Vector2[] IEEGUV { get; private set; } = new Vector2[0];
            public Vector2[] AlphaUV { get; private set; } = new Vector2[0];
            public Vector2[] UVNull { get; private set; } = new Vector2[0];

            /// <summary>
            /// Maximum density
            /// </summary>
            public float MaximumDensity
            {
                get
                {
                    return getMaximumDensity_BrainSurfaceTextureGenerator(_handle);
                }
            }
            /// <summary>
            /// Minimum influence
            /// </summary>
            public float MinimumInfluence
            {
                get
                {
                    return getMinInf_BrainSurfaceTextureGenerator(_handle);
                }
            }
            /// <summary>
            /// Maximum influence
            /// </summary>
            public float MaximumInfluence
            {
                get
                {
                    return getMaxInf_BrainSurfaceTextureGenerator(_handle);
                }
            }

            private GCHandle m_UVAmplitudesHandle;
            private GCHandle m_UVAlphaHandle;
            #endregion

            #region Public Methods
            /// <summary>
            /// 
            /// </summary>
            /// <param name="surface"></param>
            /// <param name="volume"></param>
            public void Reset(DLL.Surface surface, Volume volume)
            {
                reset_BrainSurfaceTextureGenerator(_handle, surface.getHandle(), volume.getHandle());
                ApplicationState.DLLDebugManager.check_error();
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="rawPlotList"></param>
            public void InitializeOctree(RawSiteList rawPlotList)
            {
                initOctree_BrainSurfaceTextureGenerator(_handle, rawPlotList.getHandle());
                ApplicationState.DLLDebugManager.check_error();
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="maxDistance"></param>
            /// <param name="multiCPU"></param>
            /// <returns></returns>
            public bool ComputeDistances(float maxDistance, bool multiCPU)
            {
                bool noError = false;
                noError = computeDistances_BrainSurfaceTextureGenerator( _handle, maxDistance, multiCPU ? 1 : 0) == 1;
                ApplicationState.DLLDebugManager.check_error();

                if (!noError)
                    Debug.LogError("computeDistances_BrainSurfaceTextureGenerator failed ! (check DLL console debug output)");

                return noError;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="sharedMaxDensity"></param>
            /// <param name="sharedMinInf"></param>
            /// <param name="sharedMaxInf"></param>
            public void SynchronizeWithOthersGenerators(float sharedMaxDensity, float sharedMinInf, float sharedMaxInf)
            {
                synchronizeWithOthersGenerators_BrainSurfaceTextureGenerator(_handle, sharedMaxDensity, sharedMinInf, sharedMaxInf);
            }
            /// <summary>
            /// Compute the influence
            /// </summary>
            /// <param name="IEEGColumn"></param>
            /// <param name="multiCPU"></param>
            /// <param name="addValues"></param>
            /// <param name="ratioDistances"></param>
            /// <returns></returns>
            public bool ComputeInfluences(Column3DDynamic IEEGColumn, bool multiCPU, bool addValues = false, int ratioDistances = 0)
            {
                bool noError = false;
                noError = computeInfluences_BrainSurfaceTextureGenerator(_handle, IEEGColumn.ActivityValues, IEEGColumn.Timeline.Length, IEEGColumn.Sites.Count, IEEGColumn.DynamicParameters.InfluenceDistance, multiCPU ? 1 : 0, addValues ? 1 : 0, ratioDistances,
                    IEEGColumn.DynamicParameters.Middle, IEEGColumn.DynamicParameters.SpanMin, IEEGColumn.DynamicParameters.SpanMax) == 1;
                ApplicationState.DLLDebugManager.check_error();

                if(!noError)
                    Debug.LogError("computeInfluences_BrainSurfaceTextureGenerator failed ! (check DLL console debug output)");

                return noError;
            }
            /// <summary>
            /// 
            /// </summary>
            public void AdjustInfluencesToColormap(Column3DDynamic column3DIEEG)
            {
                ajustInfluencesToColormap_BrainSurfaceTextureGenerator( _handle, column3DIEEG.DynamicParameters.Middle, column3DIEEG.DynamicParameters.SpanMin, column3DIEEG.DynamicParameters.SpanMax);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="surface"></param>
            /// <param name="volume"></param>
            /// <param name="calMin"></param>
            /// <param name="calMax"></param>
            public void ComputeUVMainWithVolume(DLL.Surface surface, DLL.Volume volume, float calMin, float calMax)
            {
                compute_UVMain_with_volume_BrainSurfaceTextureGenerator(_handle, volume.getHandle(), surface.getHandle(), calMin, calMax);// calMin, calMax);
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="surface"></param>
            /// <param name="IEEGColumn"></param>
            /// <returns></returns>
            public bool ComputeSurfaceUVIEEG(DLL.Surface surface, Column3DDynamic IEEGColumn)
            {                
                bool noError = false;
                noError = computeSurfaceTextCoordAmplitudes_BrainSurfaceTextureGenerator( _handle, surface.getHandle(), IEEGColumn.Timeline.CurrentIndex, IEEGColumn.DynamicParameters.AlphaMin, IEEGColumn.DynamicParameters.AlphaMax) == 1;

                int m_nbVertices = surface.NumberOfVertices;
                if (m_nbVertices == 0) // mesh is empty
                    return true;

                // amplitudes
                if (IEEGUV.Length != m_nbVertices)
                {
                    IEEGUV = new Vector2[m_nbVertices];
                    if (m_UVAmplitudesHandle.IsAllocated) m_UVAmplitudesHandle.Free();
                    m_UVAmplitudesHandle = GCHandle.Alloc(IEEGUV, GCHandleType.Pinned); 
                }
                updateUVAmplitudes_BrainSurfaceTextureGenerator(_handle, m_UVAmplitudesHandle.AddrOfPinnedObject());

                // alpha
                if (AlphaUV.Length != m_nbVertices)
                {
                    AlphaUV = new Vector2[m_nbVertices];
                    if (m_UVAlphaHandle.IsAllocated) m_UVAlphaHandle.Free();
                    m_UVAlphaHandle = GCHandle.Alloc(AlphaUV, GCHandleType.Pinned);
                }
                updateUVAlpha_BrainSurfaceTextureGenerator(_handle, m_UVAlphaHandle.AddrOfPinnedObject());

                ApplicationState.DLLDebugManager.check_error();
                if (!noError)
                    Debug.LogError("computeSurfaceTextCoordAmplitudes_BrainSurfaceTextureGenerator failed ! (check DLL console debug output)");

                return noError;
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="surface"></param>
            public void ComputeUVNull(DLL.Surface surface)
            {
                UVNull = new Vector2[surface.NumberOfVertices];
                UVNull.Fill(new Vector2(0.01f, 1f));
            }
            #endregion

            #region Memory Management
            /// <summary>
            /// Allocate DLL memory
            /// </summary>
            protected override void create_DLL_class()
            {
                _handle = new HandleRef(this,create_BrainSurfaceTextureGenerator());
            }
            /// <summary>
            /// Clean DLL memory
            /// </summary>
            protected override void delete_DLL_class()
            {
               delete_BrainSurfaceTextureGenerator(_handle);
            }
            /// <summary>
            /// BrainTextureGenerator default constructor
            /// </summary>
            public MRIBrainGenerator() : base() { }
            /// <summary>
            /// BrainTextureGenerator copy constructor
            /// </summary>
            /// <param name="other"></param>
            public MRIBrainGenerator(MRIBrainGenerator other) : base(clone_BrainSurfaceTextureGenerator(other.getHandle())) { }
            /// <summary>
            /// Clone the BrainTextureGenerator
            /// </summary>
            /// <returns></returns>
            public object Clone()
            {
                return new MRIBrainGenerator(this);
            }
            #endregion

            #region DLLImport

            [DllImport("hbp_export", EntryPoint = "create_BrainSurfaceTextureGeneratorsContainer", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr create_BrainSurfaceTextureGeneratorsContainer();
            [DllImport("hbp_export", EntryPoint = "add_BrainSurfaceTextureGeneratorsContainer", CallingConvention = CallingConvention.Cdecl)]
            static private extern void add_BrainSurfaceTextureGeneratorsContainer(IntPtr container, HandleRef handleBrainSurfaceTextureGenerator);
            [DllImport("hbp_export", EntryPoint = "display_BrainSurfaceTextureGeneratorsContainer", CallingConvention = CallingConvention.Cdecl)]
            static private extern void display_BrainSurfaceTextureGeneratorsContainer(IntPtr container);

            // memory management
            [DllImport("hbp_export", EntryPoint = "create_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr create_BrainSurfaceTextureGenerator();
            [DllImport("hbp_export", EntryPoint = "clone_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern IntPtr clone_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator);
            [DllImport("hbp_export", EntryPoint = "delete_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern void delete_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator);

            // actions
            [DllImport("hbp_export", EntryPoint = "reset_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern void reset_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, HandleRef handleSurface, HandleRef handleVolume);
            [DllImport("hbp_export", EntryPoint = "initOctree_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern void initOctree_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, HandleRef handleRawPlotList);
            [DllImport("hbp_export", EntryPoint = "computeDistances_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern int computeDistances_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, float maxDistance, int multiCPU);
            [DllImport("hbp_export", EntryPoint = "computeInfluences_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern int computeInfluences_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, float[] timelineAmplitudes, int timelineLength, int sitesNumber, float maxDistance, int multiCPU,
                int addValues, int ratioDistances, float middle, float spanMin, float spanMax);
            [DllImport("hbp_export", EntryPoint = "ajustInfluencesToColormap_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern void ajustInfluencesToColormap_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, float middle, float min, float max);
            [DllImport("hbp_export", EntryPoint = "synchronizeWithOthersGenerators_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern void synchronizeWithOthersGenerators_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, float sharedMaxDensity, float sharedMinInf, float sharedMaxInf);

            [DllImport("hbp_export", EntryPoint = "compute_UVMain_with_volume_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern void compute_UVMain_with_volume_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, HandleRef handleVolume, HandleRef handleSurface, float calMin, float calMax);

            // retrieve data                            
            [DllImport("hbp_export", EntryPoint = "computeSurfaceTextCoordAmplitudes_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern int computeSurfaceTextCoordAmplitudes_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, HandleRef handleSurface, int idTimeline, float alphaMin, float alphaMax);
            [DllImport("hbp_export", EntryPoint = "getUVAmplitudes_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern void getUVAmplitudes_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, float[] textureCoordsArray);
            [DllImport("hbp_export", EntryPoint = "updateUVAmplitudes_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern void updateUVAmplitudes_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, IntPtr uv);
            [DllImport("hbp_export", EntryPoint = "getUVAlpha_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern void getUVAlpha_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, float[] textureCoordsArray);
            [DllImport("hbp_export", EntryPoint = "updateUVAlpha_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern void updateUVAlpha_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator, IntPtr uv);
            [DllImport("hbp_export", EntryPoint = "getMaximumDensity_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern float getMaximumDensity_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator);
            [DllImport("hbp_export", EntryPoint = "getMinInf_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern float getMinInf_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator);
            [DllImport("hbp_export", EntryPoint = "getMaxInf_BrainSurfaceTextureGenerator", CallingConvention = CallingConvention.Cdecl)]
            static private extern float getMaxInf_BrainSurfaceTextureGenerator(HandleRef handleBrainSurfaceTextureGenerator);

            #endregion
        }
        
    }
}