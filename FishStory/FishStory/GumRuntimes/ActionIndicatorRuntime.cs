using System;
using System.Collections.Generic;
using System.Linq;

namespace FishStory.GumRuntimes
{
    public partial class ActionIndicatorRuntime
    {
        partial void CustomInitialize () 
        {
            FlashActionAnimation.Play();

        }
    }
}
