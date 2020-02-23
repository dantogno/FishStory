using FishStory.Managers;
using FishStory.Screens;
using FlatRedBall;
using FlatRedBall.Input;
using Gum.Wireframe;
using Microsoft.Xna.Framework.Audio;
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
        double secondsBeforePromptingAction = 5;

        RootObject dialogTree;
        string currentNodeId;

        public IPressableInput UpInput { get; set; }
        public IPressableInput DownInput { get; set; }
        public IPressableInput SelectInput { get; set; }

        Action<Link> linkSelected;

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

        float lettersToShow = 0;

        private Passage CurrentPassage => 
            dialogTree.passages.FirstOrDefault(item => item.pid == currentNodeId);

        #endregion

        #region Events/Properties

        public event Action AfterHide;
        public event Action<string> DialogTagShown;
        public event Action<string> StoreShouldShow;
        public event Action<string> SellingShouldShow;
        public event Action IdentifyPerformed;

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
            RootObject rootObject =
                GlobalContent.GetFile(dialogName) as RootObject;
            return TryShow(rootObject);
        }

        public bool TryShow(RootObject rootObject, Action<Link> linkSelected = null)
        {      
            this.linkSelected = linkSelected;
            dialogTree = rootObject;
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
                const string idPrefix = "id=";

                // clear out the dialog options
                while (this.DialogOptions.Children.Count() > 0)
                {
                    var option = this.DialogOptions.Children.Last();
                    option.Destroy();
                }
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
                    var sellerName = passage.StrippedText.Substring(sellPrefix.Length);
                    SellingShouldShow(sellerName);
                    if (TryHide())
                    {
                        AfterHide();
                    }
                }
                else if(passage.StrippedText.ToLowerInvariant().StartsWith(idPrefix))
                {
                    IdentifyPerformed();
                    if (TryHide())
                    {
                        AfterHide();
                    }
                }
                else
                {
                    if (passage.StrippedText.Contains("[FishType1]"))
                    {
                        var topFish = Screens.MainLevel.GetKeysWithTopValues(PlayerDataManager.PlayerData.TimesFishIdentified, 2);
                        var adjustedText = passage.StrippedText.Replace("[FishType1]", topFish[0])
                            .Replace("[FishType2]", topFish[1])
                            .Replace("[Trait1]", GlobalContent.ItemDefinition[topFish[0]].AssociatedTrait)
                            .Replace("[Trait2]", GlobalContent.ItemDefinition[topFish[1]].AssociatedTrait);                  
                        this.TextInstance.Text = adjustedText;
                    }
                    else if (passage.StrippedText.Contains("[ChosenName]"))
                    {
                        this.TextInstance.Text = passage.StrippedText.Replace("[ChosenName]", CharacterNames.DisplayNames[MainLevel.CharacterToSacrifice]);
                    }
                    else if (passage.StrippedText.Contains("[ChosenLine]"))
                    {
                        this.TextInstance.Text = passage.StrippedText.Replace("[ChosenName]", CharacterNames.ChosenLines[MainLevel.CharacterToSacrifice]);
                    }
                    else
                    {
                        this.TextInstance.Text = passage.StrippedText;
                    }

                    ShowLinks(passage);

                    if (passage.tags != null)
                    {
                        foreach (var tag in passage.tags)
                        {
                            DialogTagShown(tag);
                        }
                    }

                    lettersToShow = 0;
                    this.TextInstance.MaxLettersToShow = (int)lettersToShow;

                    // we want to force the size of the frame, so we'll do the following:
                    // make everything visible
                    DialogOptions.Visible = true;
                    CustomNiceSliceInstance.HeightUnits = Gum.DataTypes.DimensionUnitType.RelativeToChildren;
                    CustomNiceSliceInstance.Height = 0;
                    var height = CustomNiceSliceInstance.GetAbsoluteHeight();
                    DialogOptions.Visible = false;
                    CustomNiceSliceInstance.HeightUnits = Gum.DataTypes.DimensionUnitType.Absolute;
                    CustomNiceSliceInstance.Height = height;
                }
            }
        }


        private void ShowLinks(Passage passage)
        {
            // This used to be > 1 but we want to show them
            var shouldShowLinks = passage.links?.Count() > 1;
            if(!shouldShowLinks && passage.links?.Count() == 1)
            {
                var firstLink = passage.links[0];

                shouldShowLinks = firstLink.name.Contains("|");

            }

            if(shouldShowLinks)
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
            int LettersPerSecond = 26;
            if (this.Visible)
            {
                lettersToShow += TimeManager.SecondDifference * LettersPerSecond;

                this.TextInstance.MaxLettersToShow = (int)lettersToShow;

                var isPrintingDone = this.TextInstance.MaxLettersToShow > this.TextInstance.Text.Length;
                DialogOptions.Visible = isPrintingDone;
                ActionIndicatorInstance.Visible = isPrintingDone && DialogOptions.Children.Count() == 0;

                if (!isPrintingDone)
                {
                    PlayTypingSoundEffect();
                }
                else if (DialogOptions.Children.Any() && isPrintingDone)
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
                        SoundManager.Play(GlobalContent.MenuMoveSound);
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
                        SoundManager.Play(GlobalContent.MenuMoveSound);
                    }
                }

                if (SelectInput.WasJustPressed)
                {
                    if(!isPrintingDone)
                    {
                        lettersToShow += this.TextInstance.Text.Length;
                    }
                    else
                    {
                        HandleSelect();
                    }
                }
            }
        }

        int lastTypeSoundEffectNumber = 1;
        bool lastTypePlayAttemptSuccessful = true;
        private void PlayTypingSoundEffect()
        {
            if (lastTypePlayAttemptSuccessful)
            {
                lastTypeSoundEffectNumber = FlatRedBallServices.Random.Next(1, 6);
            }
            var soundType = "TypewriterKey";
            var stringName = $"{soundType}{lastTypeSoundEffectNumber}Sound";
            var soundEffectAsObject = GlobalContent.GetFile(stringName);
            if (soundEffectAsObject is SoundEffect soundEffect)
            {
                lastTypePlayAttemptSuccessful = SoundManager.PlayIfNotPlaying(soundEffect, soundType);
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
                    currentNodeId = dialogTree.passages.FirstOrDefault(p => p.name == link.StrippedLink)?.pid;
                }
                else
                {
                    currentNodeId = link.pid;
                }

                linkSelected?.Invoke(link);


                UpdateToCurrentTreeAndNode();
                SoundManager.Play(GlobalContent.DialogueAdvanceSound);
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
