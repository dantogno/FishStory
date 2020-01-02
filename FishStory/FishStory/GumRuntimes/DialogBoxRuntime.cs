using FlatRedBall;
using FlatRedBall.Input;
using Gum.Wireframe;
using System;
using System.Collections.Generic;
using System.Linq;
using static DialogTreePlugin.SaveClasses.DialogTreeRaw;

namespace FishStory.GumRuntimes
{
    public partial class DialogBoxRuntime
    {
        #region Fields/Properties

        double lastTimeHiddenOrShown;

        RootObject dialogTree;
        string currentNodeId;

        public IPressableInput UpInput { get; set; }
        public IPressableInput DownInput { get; set; }
        public IPressableInput SelectInput { get; set; }

        public int? SelectedIndex
        {
            get
            {
                SelectableOptionRuntime selectedOption = SelectedOption;

                if (selectedOption == null)
                {
                    if(CurrentPassage.links?.Count() == 1)
                    {
                        return 0;
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return (DialogOptions as GraphicalUiElement)
                        .Children.IndexOf(selectedOption);
                }
            }
            set
            {
                int index = 0;
                foreach(var item in DialogOptions.Children)
                {
                    if(index == value)
                    {
                        item.CurrentSelectedStateState = SelectableOptionRuntime.SelectedState.Selected;
                    }
                    else
                    {
                        item.CurrentSelectedStateState = SelectableOptionRuntime.SelectedState.Deselected;
                    }
                    index++;
                }
            }
        }

        private SelectableOptionRuntime SelectedOption
        {
            get
            {
                SelectableOptionRuntime selectedOption = null;

                if (DialogOptions.Children.Any())
                {
                    selectedOption =
                        DialogOptions.
                        Children.
                        FirstOrDefault(item =>
                            item.CurrentSelectedStateState ==
                                SelectableOptionRuntime.SelectedState.Selected);


                }

                return selectedOption;
            }
        }

        public int OptionCount => DialogOptions.Children.Count();

        private Passage CurrentPassage => 
            dialogTree.passages.FirstOrDefault(item => item.pid == currentNodeId);

        #endregion

        #region Events/Properties

        public event Action AfterHide;
        public event Action<string> DialogTagShown;
        public event Action<string> StoreShouldShow;
        public event Action SellingShouldShow;

        #endregion

        #region Initialize

        partial void CustomInitialize () 
        {
        }

        #endregion

        #region Activity

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
            dialogTree = GlobalContent.GetFile(dialogName) as RootObject;
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
            Passage passage = CurrentPassage;
            if (passage != null)
            {
                const string storePrefix = "store=";
                const string sellPrefix = "sell=";
                if (passage.StrippedText.ToLowerInvariant().StartsWith(storePrefix))
                {
                    var storeName = passage.StrippedText.Substring(storePrefix.Length);
                    StoreShouldShow(storeName);
                    if (TryHide())
                    {
                        AfterHide();
                    }
                }
                else if(passage.StrippedText.ToLowerInvariant().StartsWith(sellPrefix))
                {
                    SellingShouldShow();
                    if (TryHide())
                    {
                        AfterHide();
                    }
                }
                else
                {
                    this.TextInstance.Text = passage.StrippedText;

                    // clear out the dialog options
                    while (this.DialogOptions.Children.Count() > 0)
                    {
                        this.DialogOptions.Children.Last().Parent = null;
                    }

                    ShowLinks(passage);

                    if (passage.tags != null)
                    {
                        foreach (var tag in passage.tags)
                        {
                            DialogTagShown(tag);
                        }
                    }
                }
            }
        }


        private void ShowLinks(Passage passage)
        {
            if(passage.links != null && passage.links.Count() > 1)
            {
                foreach (var link in passage.links)
                {
                    var option = new SelectableOptionRuntime();
                    option.Text = link.StrippedName;
                    option.CurrentSelectedStateState = SelectableOptionRuntime.SelectedState.Deselected;
                    this.DialogOptions.AddChild(option);
                }
            }

            var firstOption = this.DialogOptions.Children.FirstOrDefault();
            if(firstOption != null)
            {
                firstOption.CurrentSelectedStateState = 
                    SelectableOptionRuntime.SelectedState.Selected;
            }
        }

        public void CustomActivity()
        {
            if(this.Visible)
            {
                if (DialogOptions.Children.Any())
                {
                    if(UpInput.WasJustPressed)
                    {
                        var index = SelectedIndex;
                    
                        if(index == 0)
                        {
                            SelectedIndex = OptionCount - 1;
                        }
                        else
                        {
                            SelectedIndex--;
                        }
                    }
                    if(DownInput.WasJustPressed)
                    {
                        var index = SelectedIndex;

                        if(index == OptionCount - 1)
                        {
                            SelectedIndex = 0;
                        }
                        else
                        {
                            SelectedIndex++;
                        }
                    }
                }

                if (SelectInput.WasJustPressed)
                {
                    HandleSelect();
                }


            }
        }

        private void HandleSelect()
        {
            var selectedIndex = SelectedIndex;

            if (CurrentPassage != null && selectedIndex != null)
            {
                var currentPassage = CurrentPassage;

                var link = currentPassage.links[selectedIndex.Value];
                if (link.pid.IsNullOrWhitespace())
                {
                    currentNodeId = dialogTree.passages.Single(p => p.name == link.StrippedLink).pid;
                }
                else
                {
                    currentNodeId = link.pid;
                }
                
                UpdateToCurrentTreeAndNode();
            }
            else
            {
                if (TryHide())
                {
                    AfterHide();
                }
            }
        }

        #endregion
    }
}
