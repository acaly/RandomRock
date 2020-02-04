using LightDx;
using LightDx.InputAttributes;
using PerlinNoise.DualContouring;
using PerlinNoise.Functions;
using PerlinNoise.Functions.Basic;
using PerlinNoise.Functions.Fractal;
using PerlinNoise.Functions.Random;
using PerlinNoise.Functions.Transform;
using PerlinNoise.MeshStorage;
using PerlinNoiseTest.Viewer;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PerlinNoiseTest
{
    class Program
    {
        private struct Vertex
        {
            [Position]
            public Vector4 Position;
            [Normal]
            public Vector4 Normal;
        }

        private class TestFunction : IFunction
        {
            public float Get(uint? seed, Vector3 coord)
            {
                var x = Math.Abs(coord.X);
                var y = Math.Abs(coord.Y);
                var z = Math.Abs(coord.Z);
                return 1 - (Math.Max(x, Math.Max(y, z)) + 0.3f) / 3;
            }
        }

        private class TestFunction2 : IFunction
        {
            public float Get(uint? seed, Vector3 coord)
            {
                return 1 - (coord.Length() + 0.3f) / 30;
            }
        }

        private static IFunction MakeFunction(float modelScale)
        {
            var rand = new RandomGradientFunction
            {
                Interpolation = new QuinticInterpolation(),
                Normal = HashedRandomNormal.Arbitrary,
            };
            var frac = new ScaleDomainToSphereFunction
            {
                Input = new FBMFractalFunction
                {
                    BasisFunction = rand,
                    Seed = 10,
                },
                Radius = 1,
            };
            var frac2 = new ScaleDomainToSphereFunction
            {
                Input = new FBMFractalFunction
                {
                    BasisFunction = rand,
                    Seed = 11,
                },
                Radius = 1,
            };
            var frac3 = new ScaleDomainToSphereFunction
            {
                Input = new FBMFractalFunction
                {
                    BasisFunction = rand,
                    Seed = 12,
                },
                Radius = 1,
            };
            var circle = new DistanceFunction
            {
                Reference = new Vector3(),
            };
            var pertCircle = new TranslateDomainFunction
            {
                Input = circle,
                TranslationX = frac,
                TranslationY = frac2,
                TranslationZ = frac3,
            };
            var rescale = new RescaleFunction
            {
                Input = pertCircle,
                K = -0.5f,
                B = 1,
            };
            var scaleDomain = new ScaleDomainFunction
            {
                Input = rescale,
                Ratio = 2 / modelScale,
            };
            return scaleDomain;
        }

        [STAThread]
        static void Main()
        {
            const float BlockSize = 0.02f;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form = new Form();
            form.ClientSize = new Size(800, 600);

            var func = MakeFunction(1 / BlockSize);
            //var func = new TestFunction2();
            var s = new DCSolver(func, (int)(4 / BlockSize));
            var m = NormalMesh.MakeNormal(s.Solve());

            using (var device = LightDevice.Create(form))
            {
                var target = new RenderTargetList(device.GetDefaultTarget(Color.AliceBlue.WithAlpha(1)), device.CreateDepthStencilTarget());
                target.Apply();

                Pipeline pipeline = device.CompilePipeline(InputTopology.Triangle,
                    ShaderSource.FromResource("Viewer.Shader.fx", ShaderType.Vertex | ShaderType.Pixel));
                pipeline.Apply();

                var vertexConstant = pipeline.CreateConstantBuffer<Matrix4x4>();
                pipeline.SetConstant(ShaderType.Vertex, 0, vertexConstant);

                var input = pipeline.CreateVertexDataProcessor<Vertex>();
                var vb = input.CreateImmutableBuffer(m.Vertices.Select(vv => new Vertex {
                    Position = new Vector4(m.Positions[vv.Position] * BlockSize, 1),
                    Normal = new Vector4(vv.Normal, 0),
                }).ToArray());

                var ib = pipeline.CreateImmutableIndexBuffer(m.Triangles.SelectMany(tt => new[] { tt.Va, tt.Vb, tt.Vc }).ToArray());

                var camera = new Camera(new Vector3(10, 0, 0));
                camera.SetForm(form);
                var proj = device.CreatePerspectiveFieldOfView((float)Math.PI / 4).Transpose();

                vertexConstant.Value = proj * camera.GetViewMatrix();
                var pt = new Vector4(0, 0, 0, 0);
                var r = Vector4.Transform(pt, vertexConstant.Value);

                form.Show();
                device.RunMultithreadLoop(delegate ()
                {
                    target.ClearAll();

                    camera.Step();
                    var view = camera.GetViewMatrix();
                    vertexConstant.Value = proj * view;
                    vertexConstant.Update();

                    ib.DrawAll(vb);
                    device.Present(true);
                });
            }
        }

        static void Main1()
        {
            var selCircle = new StairFunction
            {
                Input = MakeFunction(1),
                Threshold = 0,
                High = 1,
                Low = -1,
            };
            const float OutputScale = 32;
            using (var bitmap = new Bitmap(128, 128))
            {
                for (int y = 0; y < bitmap.Height; ++y)
                {
                    for (int x = 0; x < bitmap.Width; ++x)
                    {
                        var fx = (x - bitmap.Width / 2 - 0.5f) / OutputScale;
                        var fy = (y - bitmap.Height / 2 - 0.5f) / OutputScale;
                        var fz = 1;
                        var val = selCircle.Get(null, new Vector3(fx, fy, fz));
                        var ival = (int)((val + 1) / 2 * 255);
                        bitmap.SetPixel(x, y, Color.FromArgb(ival, ival, ival));
                    }
                }
                bitmap.Save("testoutput.bmp");
            }
        }
    }
}
