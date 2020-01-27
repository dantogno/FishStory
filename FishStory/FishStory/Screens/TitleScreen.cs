using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using FlatRedBall.AI.Pathfinding;
using FlatRedBall.Graphics.Animation;
using FlatRedBall.Graphics.Particle;
using FlatRedBall.Math.Geometry;
using FlatRedBall.Localization;
using FlatRedBall.TileEntities;

namespace FishStory.Screens
{
    public partial class TitleScreen
    {

        void CustomInitialize()
        {
            InitializeEntitiesFromMap();
            InitializeCamera();

            //InitializeCollision();

            InitializeDarkness();
        }

        private void InitializeDarkness()
        {
            BaseUnderwaterDarknessColor.ColorOperation = FlatRedBall.Graphics.ColorOperation.Color;

            LightEffectsLayer.RenderTarget = DarknessRenderTarget;
            DarknessOverlaySprite.Texture = DarknessRenderTarget;

            DarknessOverlaySprite.BlendOperation = FlatRedBall.Graphics.BlendOperation.Modulate;
        }

        private void InitializeCamera()
        {
            //Camera.Main.X = PlayerCharacterInstance.X;
            //Camera.Main.Y = PlayerCharacterInstance.Y;

            Camera.Main.SetBordersAtZ(Map.X, Map.Y - Map.Height, Map.X + Map.Width, Map.Y, 0);

            SpriteManager.OrderedSortType = FlatRedBall.Graphics.SortType.ZSecondaryParentY;
        }

        private void InitializeEntitiesFromMap()
        {
            TileEntityInstantiator.CreateEntitiesFrom(Map);

            foreach (var fish in FishList)
            {
                //propObject.Z = PlayerCharacterInstance.Z; // same as player so they sort
                fish.SetLayers(LightEffectsLayer);
            }


        }

        void CustomActivity(bool firstTimeCalled)
        {
            HandleInputActivity();

        }

        private void HandleInputActivity()
        {
            //if (FlatRedBall.Input.InputManager.Xbox360GamePads[0].IsConnected)
            //{
            //    FlatRedBall.Input.InputManager.Xbox360GamePads[0];
            //}
            //else
            //{
            //    FlatRedBall.Input.InputManager.Keyboard;
            //}
        }


        void CustomDestroy()
        {


        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

    }
}
