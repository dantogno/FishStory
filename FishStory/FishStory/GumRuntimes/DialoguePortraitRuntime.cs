using FlatRedBall.Math.Geometry;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FishStory.GumRuntimes
{
    public partial class DialoguePortraitRuntime
    {
        partial void CustomInitialize () 
        {
        }

        public void SetTextureCoordinates(Rectangle rect)
        {
            SpriteInstance.TextureTop = rect.Top;
            SpriteInstance.TextureLeft = rect.Left;
            SpriteInstance.TextureWidth = rect.Width;
            SpriteInstance.TextureHeight = rect.Height;
        }
    }
}
