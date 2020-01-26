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

        public void SetLayers(Layer lightEffectLayer)
        {
            if (CreatesLight && LightSpriteList.Count == 0)
            {
                LightSpriteInstance.TextureScale = NormalizedLightRadius;
                SpriteManager.AddToLayer(LightSpriteInstance, lightEffectLayer);
                LightSpriteList.Add(LightSpriteInstance);
                

                if (CurrentPropNameState == PropName.TriStreetLight)
                {
                    //Otherwise the player can walk right over the lights
                    this.Z = 7;

                    var newLight1 = LightSpriteInstance.Clone();
                    var newLight2 = newLight1.Clone();

                    newLight1.AttachTo(this, false);
                    newLight2.AttachTo(this, false);

                    SpriteManager.AddToLayer(newLight1, lightEffectLayer);
                    SpriteManager.AddToLayer(newLight2, lightEffectLayer);

                    //Left of middle, slightly lower
                    newLight1.RelativeX += -16;
                    newLight1.RelativeY -= 16;

                    //Right of middle, slightly lower
                    newLight2.RelativeX -= 16;
                    newLight2.RelativeY -= 16;

                    LightSpriteList.Add(newLight1);
                    LightSpriteList.Add(newLight2);
                }
            }
        }

        public void ShowLight()
        {
            if (CreatesLight)
            {
                CurrentLightStatusState = LightStatus.LightOn;
            }
        }

        public void HideLight()
        {
            if (CreatesLight)
            {
                CurrentLightStatusState = LightStatus.LightOff;
            }
        }

        public void SetLightBrightness(float requestedBrightness)
        {
            this.LightBrightness = requestedBrightness;
        }

        private void CustomDestroy()
        {

        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }
    }
}
