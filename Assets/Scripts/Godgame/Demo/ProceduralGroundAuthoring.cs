using UnityEngine;
using UnityResources = UnityEngine.Resources;

namespace Godgame.Demo
{
    /// <summary>
    /// Generates a simple quad mesh at edit/runtime so we have a ground plane that can receive navmesh data.
    /// Avoids having to depend on external terrain meshes while we stand up the demo scene.
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public sealed class ProceduralGroundAuthoring : MonoBehaviour
    {
        [Min(1f)] public float sizeX = 120f;
        [Min(1f)] public float sizeZ = 120f;
        public float height = 0f;
        [Tooltip("Number of segments per axis (higher = smoother normals for large grounds).")]
        [Min(1)] public int subdivisions = 1;
        [Tooltip("Optional override. If empty the script loads the default Bakings ground material from Resources.")]
        public Material materialOverride;
        [SerializeField] string resourceMaterialPath = "Materials/BakingsGround";

        static Material s_SharedMaterial;

        Mesh _mesh;

        void OnEnable()
        {
            RebuildMesh();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            RebuildMesh();
        }
#endif

        void OnDisable()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (_mesh != null)
                {
                    DestroyImmediate(_mesh);
                }
            }
            else if (_mesh != null)
            {
                Destroy(_mesh);
            }
#endif
        }

        void RebuildMesh()
        {
            var filter = GetComponent<MeshFilter>();
            if (_mesh == null)
            {
                _mesh = new Mesh
                {
                    name = "ProceduralGroundMesh"
                };
                _mesh.MarkDynamic();
            }
            filter.sharedMesh = _mesh;
            ApplyMaterial();

            var vertsX = subdivisions + 1;
            var vertsZ = subdivisions + 1;
            var vertexCount = vertsX * vertsZ;
            var vertices = new Vector3[vertexCount];
            var normals = new Vector3[vertexCount];
            var uvs = new Vector2[vertexCount];

            var originX = -sizeX * 0.5f;
            var originZ = -sizeZ * 0.5f;
            var stepX = sizeX / subdivisions;
            var stepZ = sizeZ / subdivisions;

            var index = 0;
            for (int z = 0; z < vertsZ; z++)
            {
                for (int x = 0; x < vertsX; x++, index++)
                {
                    vertices[index] = new Vector3(originX + x * stepX, height, originZ + z * stepZ);
                    normals[index] = Vector3.up;
                    uvs[index] = new Vector2((float)x / subdivisions, (float)z / subdivisions);
                }
            }

            var tris = new int[subdivisions * subdivisions * 6];
            var t = 0;
            for (int z = 0; z < subdivisions; z++)
            {
                for (int x = 0; x < subdivisions; x++)
                {
                    var start = z * vertsX + x;
                    tris[t++] = start;
                    tris[t++] = start + vertsX;
                    tris[t++] = start + 1;

                    tris[t++] = start + 1;
                    tris[t++] = start + vertsX;
                    tris[t++] = start + vertsX + 1;
                }
            }

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.normals = normals;
            _mesh.uv = uvs;
            _mesh.triangles = tris;
            _mesh.RecalculateBounds();

            var collider = GetComponent<MeshCollider>();
            collider.sharedMesh = null;
            collider.sharedMesh = _mesh;
        }

        void ApplyMaterial()
        {
            var renderer = GetComponent<MeshRenderer>();
            if (renderer == null)
                return;

            var material = materialOverride ? materialOverride : LoadSharedMaterial();
            if (material != null)
            {
                renderer.sharedMaterial = material;
            }
        }

        Material LoadSharedMaterial()
        {
            if (s_SharedMaterial != null)
                return s_SharedMaterial;

            if (!string.IsNullOrWhiteSpace(resourceMaterialPath))
            {
                s_SharedMaterial = UnityResources.Load<Material>(resourceMaterialPath);
            }

            if (s_SharedMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader != null)
                {
                    s_SharedMaterial = new Material(shader)
                    {
                        name = "ProceduralGround_Fallback",
                        color = new Color(0.52f, 0.43f, 0.33f)
                    };
                    s_SharedMaterial.hideFlags = HideFlags.HideAndDontSave;
                }
            }

            return s_SharedMaterial;
        }
    }
}
