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
    public partial class FishIdentifiedSign
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

        private void CustomDestroy()
        {


        }

        private static void CustomLoadStaticContent(string contentManagerName)
        {


        }

        public void MoveDisplayElementsToUiLayer(Layer uiLayer)
        {
            EmotiveIconInstance.MoveToLayer(uiLayer);
        }

        public void SetEmoteIcon(EmotiveIcon.IconDisplay iconDisplay = null)
        {
            EmotiveIconInstance.CurrentIconDisplayState = iconDisplay ?? EmotiveIcon.IconDisplay.Thought;
            EmotiveIconInstance.CurrentDisplayState = EmotiveIcon.Display.Appearing;
            EmotiveIconInstance.Visible = true;
            EmotiveIconInstance.BeginAnimations(shouldHideAfter: false);
        }
    }
}
