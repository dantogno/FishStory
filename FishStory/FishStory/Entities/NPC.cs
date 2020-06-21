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
using static DialogTreePlugin.SaveClasses.DialogTreeRaw;
using Microsoft.Xna.Framework;
using FlatRedBall.Graphics;

namespace FishStory.Entities
{
    public partial class NPC
    {        
        public Vector3 SpawnPosition { get; set; }
        public RootObject DirectlySetDialog { get; set; }
        /// <summary>
        /// Initialization logic which is execute only one time for this Entity (unless the Entity is pooled).
        /// This method is called when the Entity is added to managers. Entities which are instantiated but not
        /// added to managers will not have this method called.
        /// </summary>
        /// 
        public void SetDialogue(string dialogueId, EmotiveIcon.IconDisplay iconDisplay = null)
        {
            this.TwineDialogId = dialogueId;
            SetEmoteIcon(iconDisplay);
        }

        public void SetEmoteIcon(EmotiveIcon.IconDisplay iconDisplay = null)
        {
            EmotiveIconInstance.CurrentIconDisplayState = iconDisplay ?? EmotiveIcon.IconDisplay.Thought;
            EmotiveIconInstance.CurrentDisplayState = EmotiveIcon.Display.Appearing;
            EmotiveIconInstance.Visible = true;
            EmotiveIconInstance.BeginAnimations(shouldHideAfter: false);

            if (Name.Contains("Sign"))
                EmotiveIconInstance.Y = 12;
        }

        public void HandleDialogueSeen()
        {
            EmotiveIconInstance.Visible = false;
        }
        private void CustomInitialize()
        {

        }

        public void MoveDisplayElementsToUiLayer(Layer uiLayer)
        {
            EmotiveIconInstance.MoveToLayer(uiLayer);
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

        public Rectangle GetTextureRectangle()
        {
            try
            {
                var idleFrame = SpriteInstance.AnimationChains[SpriteInstance.CurrentChainIndex][1];
                var textureHeight = idleFrame.Texture.Bounds.Height;
                var textureWidth = idleFrame.Texture.Bounds.Width;

                var frameTopPixel = (int)(textureHeight * idleFrame.TopCoordinate);
                var frameLeftPixel = (int)(textureWidth * idleFrame.LeftCoordinate);
                var frameBottomPixel = (int)(textureHeight * idleFrame.BottomCoordinate);
                var frameRightPixel = (int)(textureWidth * idleFrame.RightCoordinate);

                var frameWidth = frameRightPixel - frameLeftPixel;
                var frameHeight = frameBottomPixel - frameTopPixel;

                var portraitHeight = 16;
                var portraitWidth = 16;

                var rect = new Rectangle(
                    x: frameLeftPixel + (frameWidth / 2) - (portraitWidth/2) - (int)idleFrame.RelativeX,
                    y: frameTopPixel,
                    width: portraitWidth,
                    height: portraitHeight
                    );
                return rect;
            }
            catch (Exception)
            {

                return new Rectangle();
            }

        }
        public bool WillBeOnScreenAtPosition(float x, float y)
        {
            var camera = Camera.Main;
            var isOffScreen = x > camera.X + camera.OrthogonalWidth / 2 + SpriteInstance.Width / 2 ||
                x < camera.X - camera.OrthogonalWidth / 2 - SpriteInstance.Width / 2 ||
                y > camera.Y + camera.OrthogonalHeight / 2 + SpriteInstance.Height / 2 ||
                y < camera.Y - camera.OrthogonalHeight / 2 - SpriteInstance.Height / 2;
            return !isOffScreen;
        }
        public bool IsOnScreen()
        {
            var camera = Camera.Main;
            var isOffScreen = SpriteInstance.X > camera.X + camera.OrthogonalWidth / 2 + SpriteInstance.Width / 2 ||
                SpriteInstance.X < camera.X - camera.OrthogonalWidth / 2 - SpriteInstance.Width / 2 ||
                SpriteInstance.Y > camera.Y + camera.OrthogonalHeight / 2 + SpriteInstance.Height / 2 ||
                SpriteInstance.Y < camera.Y - camera.OrthogonalHeight / 2 - SpriteInstance.Height / 2;
            return !isOffScreen;
        }

        internal void InitializeEmoteIcon()
        {
            if (!string.IsNullOrWhiteSpace(TwineDialogId))
            {
                SetEmoteIcon();
            }
        }
    }
}
