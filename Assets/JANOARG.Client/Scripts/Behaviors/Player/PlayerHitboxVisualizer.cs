using System.Collections.Generic;
using JANOARG.Client.Behaviors.Common;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Player
{
    internal class PlayerHitboxVisualizer : MonoBehaviour
    {
        public static PlayerHitboxVisualizer main;
        public void Awake()
        {
            main = this;
        }

        List<HitScreenCoord> DebugCoords = new();
        List<Color> DebugColors = new();
        Material DebugLineMaterial;

        public void DrawHitScreenCoordDebug(HitScreenCoord coord, Color color)
        {
            if (!DebugLineMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                DebugLineMaterial = new Material(shader);
                DebugLineMaterial.hideFlags = HideFlags.HideAndDontSave;
                DebugLineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                DebugLineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                DebugLineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                DebugLineMaterial.SetInt("_ZWrite", 0);


                Camera.onPostRender += PostRender;
            }

            DebugCoords.Add(coord);
            DebugColors.Add(color);
        }
    
        public void PostRender(Camera cam)
        {
            if (DebugCoords.Count == 0) return;


            DebugLineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.LINES);

            // Draw circles
            int steps = 16;
            float angleStep = Mathf.PI * 2 / steps;
            for (int idx = 0; idx < DebugCoords.Count; idx++)
            {
                var coord = DebugCoords[idx];
                float angle = 0;
                Vector3 vec = new();
                void updateVec()
                {
                    vec = CommonSys.main.MainCamera.ScreenToWorldPoint(new (Mathf.Cos(angle) * coord.Radius + coord.Position.x, Mathf.Sin(angle) * coord.Radius + coord.Position.y, 10));
                }
                updateVec();
                for (int i = 0; i < steps; i++)
                {
                    GL.Color(DebugColors[idx]);
                    GL.Vertex(vec);
                    angle += angleStep;
                    updateVec();
                    GL.Vertex(vec);
                }
            }

            GL.End();
            GL.PopMatrix();
            DebugCoords.Clear();
            DebugColors.Clear();
        }
    }
}