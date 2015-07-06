﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Mathematics;
using Penumbra.Utilities;

namespace Penumbra
{
    public sealed class Light
    {
        private bool _castsShadows;
        private bool _enabled;
        private Vector2 _position;
        private float _range;
        private float _radius;

        public Light(Texture texture = null) // TODO: allow creation of light without tex, use cached tex based on radius internally in these cases.
        {
            Enabled = true;
            CastsShadows = true;
            Range = 100f;
            Radius = 20f;
            Intensity = 1;
            ShadowType = ShadowType.Illuminated;
            Color = new Color(1f,1f,1f,1f);
            Texture = texture;
            DirtyFlags = LightComponentDirtyFlags.All;
        }        

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled != value)
                {
                    DirtyFlags |= LightComponentDirtyFlags.Enabled;
                    _enabled = value;
                }
            }
        }

        public bool CastsShadows
        {
            get { return _castsShadows; }
            set
            {
                if (_castsShadows != value)
                {
                    DirtyFlags |= LightComponentDirtyFlags.CastsShadows;
                    _castsShadows = value;
                }
            }
        }

        public Vector2 Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    DirtyFlags |= LightComponentDirtyFlags.Position;
                    _position = value;
                }
            }
        }

        public float Range
        {
            get { return _range; }
            set
            {
                Check.ArgumentNotLessThan(value, 1, "value", "Range cannot be smaller than 1.");
                if (_range != value)
                {
                    DirtyFlags |= LightComponentDirtyFlags.Range;
                    _range = value;
                }
            }
        }

        public float RangeSquared => Range*Range;

        public float Radius
        {
            get { return _radius; }
            set
            {
                Check.ArgumentWithinRange(value, 1, Range, "value", "Radius cannot cannot be smaller than 1 and larger than Range.");
                if (_radius != value)
                {
                    DirtyFlags |= LightComponentDirtyFlags.Radius;
                    _radius = value;
                }
            }
        }

        public float Intensity { get; set; }
        public ShadowType ShadowType { get; set; }
        public Color Color { get; set; }
        public Texture Texture { get; set; }

        internal float IntensityFactor => 1 / (Intensity * Intensity);
        internal LightComponentDirtyFlags DirtyFlags { get; set; }

        internal bool AnyDirty(LightComponentDirtyFlags flags)
        {
            return (DirtyFlags & flags) != 0;
        }

        internal Rectangle GetBoundingRectangle()
        {
            return new Rectangle(
                (int)(Position.X - Range),
                (int)(Position.Y - Range),
                (int)(Range * 2),
                (int)(Range * 2));
        }

        internal bool Intersects(HullPart hullPart)
        {
            // Ref: Jason Gregory Game Engine Architecture 2nd p.172
            float sumOfRadiuses = Range + hullPart.Radius;
            return Vector2.DistanceSquared(Position, hullPart.Centroid) < sumOfRadiuses * sumOfRadiuses;
        }

        internal bool IsInside(Hull hull)
        {
            return hull.Parts.Any(IsInside);
        }

        internal bool IsInside(HullPart hullPart)
        {
            if (!hullPart.Enabled) return false;
            return VectorUtil.PointIsInside(hullPart.TransformedHullVertices, Position);
        }
    }

    [Flags]
    internal enum LightComponentDirtyFlags
    {
        CastsShadows = 1 << 0,
        Position = 1 << 1,
        Radius = 1 << 2,
        Range = 1 << 3,
        Enabled = 1 << 4,
        All = int.MaxValue
    }
}
