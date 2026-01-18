using UnityEngine;

namespace JANOARG.Client.Behaviors.Intro
{
    public class IntroFlick : MonoBehaviour
    {
        public MeshFilter MeshFilter;

        public void Awake()
        {
            Mesh mesh = new();

            mesh.SetVertices(
                new Vector3[]
                {
                    new(1, 1, .5f), new(-.125f, .625f, .5f), new(.625f, .625f, .5f),
                    new(1, 1, -.5f), new(-.125f, .625f, -.5f), new(.625f, .625f, -.5f),
                    new(1, 1, .5f), new(-.125f, .625f, .5f),
                    new(1, 1, -.5f), new(-.125f, .625f, -.5f),
                    new(-.125f, .625f, .5f), new(.625f, .625f, .5f),
                    new(-.125f, .625f, -.5f), new(.625f, .625f, -.5f),
                    new(.625f, .625f, .5f), new(1, 1, .5f),
                    new(.625f, .625f, -.5f), new(1, 1, -.5f)
                });

            mesh.SetTriangles(
                new[]
                {
                    0, 1, 2, 5, 4, 3,
                    6, 8, 7, 7, 8, 9,
                    10, 12, 11, 11, 12, 13,
                    14, 16, 15, 15, 16, 17
                }, 0);

            mesh.RecalculateNormals();
            MeshFilter.mesh = mesh;
        }
    }
}
