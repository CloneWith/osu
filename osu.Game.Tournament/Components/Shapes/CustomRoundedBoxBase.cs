// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Runtime.InteropServices;
using osu.Framework.Allocation;
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
    public partial class CustomRoundedBoxBase : Box
    {
        public new float Width
        {
            get => base.Width;
            set
            {
                if (base.Width == value)
                    return;

                base.Width = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        public new float Height
        {
            get => base.Height;
            set
            {
                if (base.Height == value)
                    return;

                base.Height = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private Color4 backgroundColour = Color4.White;

        public Color4 BackgroundColour
        {
            get => backgroundColour;
            set
            {
                if (backgroundColour == value)
                    return;

                backgroundColour = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private Color4 borderColour = Color4.White;

        public Color4 BorderColour
        {
            get => borderColour;
            set
            {
                if (borderColour == value)
                    return;

                borderColour = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        private float borderWidth = 1;

        public float BorderWidth
        {
            get => borderWidth;
            set
            {
                if (borderWidth == value)
                    return;

                borderWidth = value;
                Invalidate(Invalidation.DrawNode);
            }
        }

        [BackgroundDependencyLoader]
        private void load(ShaderManager shaders)
        {
            RelativeSizeAxes = Axes.Both;
            TextureShader = shaders.Load(VertexShaderDescriptor.TEXTURE_2, "CustomRoundedBoxPath");
        }

        protected override DrawNode CreateDrawNode() => new CustomRoundedBoxPathDrawNode(this);

        private class CustomRoundedBoxPathDrawNode : SpriteDrawNode
        {
            protected new CustomRoundedBoxBase Source => (CustomRoundedBoxBase)base.Source;

            private IUniformBuffer<CustomRoundedBoxPathParameters>? parametersBuffer;

            public CustomRoundedBoxPathDrawNode(CustomRoundedBoxBase source)
                : base(source)
            {
            }

            private Colour4 backgroundColour = Color4.White;
            private Colour4 borderColour = FumoColours.SeaBlue.Regular;
            private Vector2 size;
            private float borderWidth = 1;

            public override void ApplyState()
            {
                base.ApplyState();

                size = Source.DrawSize;
                borderColour = Source.borderColour;
                backgroundColour = Source.BackgroundColour;
                borderWidth = Source.BorderWidth;
            }

            protected override void BindUniformResources(IShader shader, IRenderer renderer)
            {
                base.BindUniformResources(shader, renderer);

                parametersBuffer ??= renderer.CreateUniformBuffer<CustomRoundedBoxPathParameters>();
                parametersBuffer.Data = new CustomRoundedBoxPathParameters
                {
                    BorderColour = new Vector4(borderColour.R, borderColour.G, borderColour.B, borderColour.A),
                    BackgroundColour = new Vector4(backgroundColour.R, backgroundColour.G, backgroundColour.B, backgroundColour.A),
                    Size = size,
                    BorderWidth = borderWidth
                };

                shader.BindUniformBlock("m_CustomRoundedBoxPathParameters", parametersBuffer);
            }

            protected override bool CanDrawOpaqueInterior => false;

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                parametersBuffer?.Dispose();
            }

            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            private record struct CustomRoundedBoxPathParameters
            {
                public UniformVector4 BackgroundColour;
                public UniformVector4 BorderColour;
                public UniformVector2 Size;
                public UniformFloat BorderWidth;
                private readonly UniformPadding8 pad;
                private readonly UniformPadding12 anotherPad;
            }
        }
    }
}
