// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Framework.Graphics.Shaders;
using osu.Framework.Graphics.Shaders.Types;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.UserInterfaceFumo;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components.Shapes
{
    public partial class GraphPartTriangle : Box
    {
        public float StartAngle
        {
            get => startAngle;
            set
            {
                startAngle = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        public float EndAngle
        {
            get => endAngle;
            set
            {
                endAngle = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        public float StartPosition
        {
            get => startPosition;
            set
            {
                startPosition = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        public float EndPosition
        {
            get => endPosition;
            set
            {
                endPosition = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        public Color4 BackgroundColour
        {
            get => backgroundColour;
            set
            {
                backgroundColour = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private float startAngle, endAngle, startPosition, endPosition;
        private Color4 backgroundColour = FumoColours.SeaBlue.Regular.Opacity(0.5f);

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "GraphPartTriangle");
        }

        protected override DrawNode CreateDrawNode() => new GraphPartTriangleDrawNode(this);

        private class GraphPartTriangleDrawNode : SpriteDrawNode
        {
            protected new GraphPartTriangle Source => (GraphPartTriangle)base.Source;

            private IUniformBuffer<GraphPartTrianglePathParameters>? parametersBuffer;

            public GraphPartTriangleDrawNode(GraphPartTriangle source)
                : base(source)
            {
            }

            private float startAngle, endAngle, startPosition, endPosition;
            private Colour4 backgroundColour = FumoColours.SeaBlue.Regular;
            private Vector2 size;

            public override void ApplyState()
            {
                base.ApplyState();

                startAngle = Source.StartAngle;
                endAngle = Source.EndAngle;
                startPosition = Source.StartPosition;
                endPosition = Source.EndPosition;
                size = Source.DrawSize;
                backgroundColour = Source.BackgroundColour;
            }

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                base.BindUniformResources(shader, renderer);

                parametersBuffer ??= renderer.CreateUniformBuffer<GraphPartTrianglePathParameters>();
                parametersBuffer.Data = new GraphPartTrianglePathParameters
                {
                    StartAngle = startAngle,
                    EndAngle = endAngle,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    Size = size,
                    BackgroundColour = new Vector4(backgroundColour.R, backgroundColour.G, backgroundColour.B, backgroundColour.A),
                };

                shader.BindUniformBlock("m_GraphPartTriangleParameters", parametersBuffer);
            }

            protected override bool CanDrawOpaqueInterior => false;

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                parametersBuffer?.Dispose();
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private record struct GraphPartTrianglePathParameters
            {
                public UniformFloat StartAngle;
                public UniformFloat EndAngle;
                public UniformFloat StartPosition;
                public UniformFloat EndPosition;
                public UniformVector2 Size;
                private readonly UniformPadding8 padding;
                public UniformVector4 BackgroundColour;
            }
        }
    }
}
