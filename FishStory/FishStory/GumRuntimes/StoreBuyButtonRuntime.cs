using System;
using System.Collections.Generic;
using System.Linq;

namespace FishStory.GumRuntimes
{
    public partial class StoreBuyButtonRuntime
    {
        partial void CustomInitialize () 
        {
            this.RollOn += StoreBuyButtonRuntime_RollOn;
            this.RollOver += StoreBuyButtonRuntime_RollOver;
            this.Click += StoreBuyButtonRuntime_Click;
            this.RollOff += StoreBuyButtonRuntime_RollOff;
        }

        private void StoreBuyButtonRuntime_RollOff(FlatRedBall.Gui.IWindow window)
        {
            if (!this.Enabled && CurrentButtonCategoryState != ButtonCategory.Disabled)
            {
                CurrentButtonCategoryState = ButtonCategory.Disabled;
            }
        }

        private void StoreBuyButtonRuntime_Click(FlatRedBall.Gui.IWindow window)
        {
            if (!this.Enabled && CurrentButtonCategoryState != ButtonCategory.Disabled)
            {
                CurrentButtonCategoryState = ButtonCategory.Disabled;
            }
        }

        private void StoreBuyButtonRuntime_RollOver(FlatRedBall.Gui.IWindow window)
        {
            if (!this.Enabled && CurrentButtonCategoryState != ButtonCategory.Disabled)
            {
                CurrentButtonCategoryState = ButtonCategory.Disabled;
            }
        }

        private void StoreBuyButtonRuntime_RollOn(FlatRedBall.Gui.IWindow window)
        {
            if (!this.Enabled && CurrentButtonCategoryState != ButtonCategory.Disabled)
            {
                CurrentButtonCategoryState = ButtonCategory.Disabled;
            }
        }
    }
}
