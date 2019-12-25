using FlatRedBall;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FishStory.GumRuntimes
{
    public partial class DialogBoxRuntime
    {
        double lastTimeHiddenOrShown;

        partial void CustomInitialize () 
        {
        }

        public bool TryHide()
        {
            if(lastTimeHiddenOrShown != TimeManager.CurrentTime)
            {
                Visible = false;
                lastTimeHiddenOrShown = TimeManager.CurrentTime;
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryShow()
        {
            if (lastTimeHiddenOrShown != TimeManager.CurrentTime)
            {
                Visible = true;
                lastTimeHiddenOrShown = TimeManager.CurrentTime;
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
