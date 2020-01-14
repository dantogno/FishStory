using System;
using System.Collections.Generic;
using System.Text;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Graphics;

namespace FishStory.Entities
{
    public partial class PropObject
    {
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
        private void CustomInitialize()
        {


        }

        private void CustomActivity()
        {


        }

        public void SetLayers(Layer worldLayer, Layer lightEffectLayer)
        {
            SpriteManager.AddToLayer(SpriteInstance, worldLayer);
            if (CreatesLight)
            {
                SpriteManager.AddToLayer(LightSpriteInstance, lightEffectLayer);
            }
        }

        public void ShowLight()
        {
            if (CreatesLight)
            {
                LightSpriteInstance.Visible = true;
                SpriteInstance.CurrentChainName = "On";
            }
        }

        public void HideLight()
        {
            if (CreatesLight)
            {
                LightSpriteInstance.Visible = false;
                SpriteInstance.CurrentChainName = "Off";
            }
        }

        private void CustomDestroy()
        {


        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
    }
}
