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
using FishStory.GumRuntimes.CreditsComponents;

namespace FishStory.Screens
{
    public partial class CreditsScreen
    {
        void CustomInitialize()
        {
            CreateCreditDisplays();
        }

        private void CreateCreditDisplays()
        {
            foreach (var credit in CreditList.Values)
            {
                var newCredit = new CreditDisplayRuntime();
                newCredit.CreditsTitleTextDisplay = credit.CreditTitle;
                newCredit.CreditsNameTextDisplay = credit.CreditName;

                CreditDisplayContainer.Children.Add(newCredit);
            }
        }

        void CustomActivity(bool firstTimeCalled)
        {
            if (CreditDisplayContainer.Y <= (-1.85 * Camera.Main.OrthogonalHeight) || InputManager.Keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                MoveToScreen(nameof(TitleScreen));
            }
            else 
            {
                var defaultTextPixelsPerSecond = 50;

                if (InputManager.Keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.Space) || InputManager.Mouse.ButtonPushed(Mouse.MouseButtons.LeftButton))
                    defaultTextPixelsPerSecond *= 2;

                CreditDisplayContainer.Y -= TimeManager.SecondDifference* defaultTextPixelsPerSecond;
            }
        }

        void CustomDestroy()
        {

        }

        static void CustomLoadStaticContent(string contentManagerName)
        {


        }

    }
}
