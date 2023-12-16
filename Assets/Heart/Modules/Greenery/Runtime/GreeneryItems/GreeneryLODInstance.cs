using System;
using System.Collections.Generic;
using Pancake.Apex;
using UnityEngine;

// ReSharper disable InconsistentNaming
namespace Pancake.Greenery
{
    [CreateAssetMenu(fileName = "GreeneryLODInstance", menuName = "Pancake/Greenery/Greenery LOD Instance", order = 0)]
    public class GreeneryLODInstance : GreeneryItem
    {
        [Serializable]
        public class InstanceLOD
        {
            public Mesh instancedMesh;
            public Material instanceMaterial;
            public bool castShadows;
            [Range(0.0f, 1.0f)] public float LODFactor = 0.2f;
        }

        public ComputeShader lodCullingCS;
        public float maxDrawDistance = 100.0f;
        public Vector2 sizeRange = new(1, 1);
        public float pivotOffset;
        public bool alignToSurface = true;
        public bool useSurfaceNormals;
        [MinMaxSlider(0, 360)] public Vector2Int xRotation;
        [MinMaxSlider(0, 360)] public Vector2Int yRotation;
        [MinMaxSlider(0, 360)] public Vector2Int zRotation;

        public List<InstanceLOD> instanceLODs;

        [HideInInspector] public int previewIndex;

        private void OnValidate()
        {
            if (instanceLODs.Count >= 2)
            {
                for (int i = 1; i < instanceLODs.Count; i++)
                {
                    instanceLODs[i].LODFactor = Mathf.Max(instanceLODs[i].LODFactor, instanceLODs[i - 1].LODFactor);
                }
            }
        }
    }
}