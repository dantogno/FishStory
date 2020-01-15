using System;
using System.Collections.Generic;
using System.Linq;

namespace FishStory.GumRuntimes
{
    public partial class OptionsMarkerRuntime
    {
        partial void CustomInitialize () 
        {
            this.SpriteMoveAnimation.Play();
        }
    }
}
