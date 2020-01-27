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
    public partial class IntroFish
    {
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
        private void CustomInitialize()
        {
            this.RelativeZ += 1;
            
            ShadowSpriteInstance.RelativeY += 48;
            ShadowSpriteInstance.RelativeZ = -1;
        }

        private void CustomActivity()
        {


        }

        private void CustomDestroy()
        {


        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }

        public void SetLayers(Layer lightEffectLayer)
        {
            //FishLightSpriteInstance.TextureScale = NormalizedLightRadius;

            SpriteManager.AddToLayer(FishLightSpriteInstance, lightEffectLayer);
        }
    }
}
