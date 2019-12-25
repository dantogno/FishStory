using FlatRedBall;
using System;
using System.Collections.Generic;
using System.Linq;
using static DialogTreePlugin.SaveClasses.DialogTreeRaw;

namespace FishStory.GumRuntimes
{
    public partial class DialogBoxRuntime
    {
        double lastTimeHiddenOrShown;

        RootObject dialogTree;
        string currentNodeId;

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

        public bool TryShow(string dialogName)
        {
            dialogTree = GlobalContent.Dialog1;
            currentNodeId = dialogTree.startnode;

            UpdateToCurrentTreeAndNode();

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

        private void UpdateToCurrentTreeAndNode()
        {
            var passage = dialogTree.passages.FirstOrDefault(item => item.pid == currentNodeId);

            this.TextInstance.Text = passage.StrippedText;

            while(this.DialogOptions.Children.Count() > 0)
            {
                this.DialogOptions.Children.Last().Parent = null;
            }

            for(int i = 0; i < 3; i++)
            {
                var option = new SelectableOptionRuntime();
                option.Text = "Option " + (i + 1);
                this.DialogOptions.AddChild(option);
            }
        }
    }
}
