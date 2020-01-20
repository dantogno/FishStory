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
            //I don't know why I have to reverse these - bug in Gum maybe?
            SpriteInstance.TextureTop = rect.Left;
            SpriteInstance.TextureLeft = rect.Top;
        }
    }
}
