using FlatRedBall;
using FlatRedBall.Input;
using FishStory.GumRuntimes.CreditsComponents;
using System.Collections.Generic;

namespace FishStory.Screens
{
    public partial class CreditsScreen
    {       
        private bool CreditsAreOffScreen => CreditDisplayContainer.Y <= (-0.4625 * NumberOfCredits * Camera.Main.OrthogonalHeight);

        private int NumberOfCredits => CreditList.Values.Count;
        void CustomInitialize()
        {
            CreateCreditDisplays();
        }

        private void CreateCreditDisplays()
        {
            float creditFadeInDelay = 1.25f;
            var runningDelay = creditFadeInDelay;
            foreach (var credit in CreditList.Values)
            {
                var newCredit = new CreditDisplayRuntime();
                newCredit.CreditsTitleTextDisplay = credit.CreditTitle;
                newCredit.CreditsNameTextDisplay = credit.CreditName;
                newCredit.CurrentFadeStatusState = CreditDisplayRuntime.FadeStatus.Out;

                newCredit.FadeInAnimation.PlayAfter(runningDelay);
                runningDelay += creditFadeInDelay;

                newCredit.FadeOutAnimation.PlayAfter(runningDelay+4);

                CreditDisplayContainer.Children.Add(newCredit);
            }
        }

        void CustomActivity(bool firstTimeCalled)
        {
            if (CreditsAreOffScreen || InputManager.Keyboard.KeyPushed(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                MoveToScreen(nameof(TitleScreen));
            }
            else 
            {
                var defaultTextPixelsPerSecond = 55;

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
