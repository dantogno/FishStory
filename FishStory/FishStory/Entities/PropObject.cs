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
                SpriteManager.AddToLayer(LightSpriteInstance, lightEffectLayer);
                LightSpriteList.Add(LightSpriteInstance);

                if (CurrentPropNameState == PropName.TriStreetLight)
                {
                    this.Z = 7;
                    //The tiled map instantiator doesn't instantiate them in quite the right place
                    //this.Y -= 2;
                    //this.X -= 1;

                    var newLight1 = LightSpriteInstance.Clone();
                    var newLight2 = newLight1.Clone();

                    SpriteManager.AddToLayer(newLight1, lightEffectLayer);
                    SpriteManager.AddToLayer(newLight2, lightEffectLayer);

                    //Left of middle, slightly higher
                    newLight1.RelativeX = -10;
                    newLight1.RelativeY = 10;

                    //Right of middle, slightly higher
                    newLight2.RelativeX = 10;
                    newLight2.RelativeY = 10;

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
