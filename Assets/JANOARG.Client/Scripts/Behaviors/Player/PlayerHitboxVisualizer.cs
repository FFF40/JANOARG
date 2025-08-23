using System.Collections.Generic;
using JANOARG.Client.Behaviors.Common;
using UnityEngine;

namespace JANOARG.Client.Behaviors.Player
{
    internal class PlayerHitboxVisualizer : MonoBehaviour
    {
        public static PlayerHitboxVisualizer sMain;

        private static readonly int sr_SrcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int sr_DstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int sr_Cull     = Shader.PropertyToID("_Cull");
        private static readonly int sr_ZWrite   = Shader.PropertyToID("_ZWrite");

        public void Awake()
        {
            sMain = this;
        }

        private List<HitScreenCoord> _DebugCoords = new();
        private List<Color>          _DebugColors = new();
        private Material             _DebugLineMaterial;

        public void DrawHitScreenCoordDebug(HitScreenCoord coord, Color color)
        {
            if (!_DebugLineMaterial)
            {
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _DebugLineMaterial = new Material(shader);
                _DebugLineMaterial.hideFlags = HideFlags.HideAndDontSave;
                _DebugLineMaterial.SetInt(sr_SrcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _DebugLineMaterial.SetInt(sr_DstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                _DebugLineMaterial.SetInt(sr_Cull, (int)UnityEngine.Rendering.CullMode.Off);
                _DebugLineMaterial.SetInt(sr_ZWrite, 0);


                Camera.onPostRender += PostRender;
            }

            _DebugCoords.Add(coord);
            _DebugColors.Add(color);
        }

        public void PostRender(Camera cam)
        {
            if (_DebugCoords.Count == 0) return;


            _DebugLineMaterial.SetPass(0);

            GL.PushMatrix();
            GL.Begin(GL.LINES);

            // Draw circles
            var steps = 16;
            float angleStep = Mathf.PI * 2 / steps;

            for (var index = 0; index < _DebugCoords.Count; index++)
            {
                HitScreenCoord coord = _DebugCoords[index];
                float angle = 0;
                Vector3 vectors;

                void f_updateVectors()
                {
                    vectors = CommonSys.sMain.MainCamera.ScreenToWorldPoint(new Vector3(Mathf.Cos(angle) * coord.Radius + coord.Position.x, Mathf.Sin(angle) * coord.Radius + coord.Position.y, 10));
                }

                f_updateVectors();

                for (var i = 0; i < steps; i++)
                {
                    GL.Color(_DebugColors[index]);
                    GL.Vertex(vectors);

                    angle += angleStep;

                    f_updateVectors();
                    GL.Vertex(vectors);
                }
            }

            GL.End();
            GL.PopMatrix();

            _DebugCoords.Clear();
            _DebugColors.Clear();
        }
    }
}