using FishStory.Managers;
using FlatRedBall;
using FlatRedBall.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FishStory.GumRuntimes
{
    public partial class PauseMenuRuntime
    {
        #region Events

        public event Action Closed;
        public event Action ExitGame;

        #endregion

        public IPressableInput UpInput { get; set; }
        public IPressableInput DownInput { get; set; }
        public IPressableInput SelectInput { get; set; }

        public IPressableInput CancelInput { get; internal set; }

        public SelectableOptionRuntime CurrentlySelectedItem => SelectableOptionContainer.Children.FirstOrDefault(x => x is SelectableOptionRuntime option && option.CurrentSelectedStateState == SelectableOptionRuntime.SelectedState.Selected) as SelectableOptionRuntime;

        public int OptionCount => SelectableOptionContainer.Children.Count();

        public int? SelectedIndex
        {
            get
            {
                var selectedOption = CurrentlySelectedItem;

                if (selectedOption == null)
                {
                    return null;
                }
                else
                {
                    return SelectableOptionContainer.Children.IndexOf(selectedOption);
                }
            }
            set
            {
                    var optionCount = OptionCount;
                    for (var i = 0; i < optionCount; i++)
                        (SelectableOptionContainer.Children[i] as SelectableOptionRuntime).CurrentSelectedStateState = value.HasValue && i == value.Value  ? 
                            SelectableOptionRuntime.SelectedState.Selected : 
                            SelectableOptionRuntime.SelectedState.Deselected;
            }
        }

        partial void CustomInitialize () 
        {

        }

        private void HandlePlayerInput()
        {
            if (CancelInput.WasJustPressed)
            {
                Close();
            }
            else
            {
                if (UpInput.WasJustPressed)
                {
                    int index = SelectedIndex ?? 0;

                    if (index == 0)
                    {
                        SelectedIndex = OptionCount - 1;
                    }
                    else
                    {
                        if (!SelectedIndex.HasValue)
                        {
                            SelectedIndex = 1;
                        }
                        else
                        {
                            SelectedIndex--;
                        }
                    }
                    SoundManager.Play(GlobalContent.MenuMoveSound);
                }
                if (DownInput.WasJustPressed)
                {
                    int index = SelectedIndex ?? 0;

                    if (SelectedIndex == OptionCount - 1)
                    {
                        SelectedIndex = 0;
                    }
                    else
                    {
                        if (!SelectedIndex.HasValue)
                        {
                            SelectedIndex = 0;
                        }
                        else
                        {
                            SelectedIndex++;
                        }
                    }
                    SoundManager.Play(GlobalContent.MenuMoveSound);
                }
                if (SelectInput.WasJustPressed)
                {
                    if (SelectedIndex == 0) Close();
                    else if (SelectedIndex == 1) FlatRedBallServices.Game.Exit();
                }
            }
        }

        private void Close()
        {
            Visible = false;

            Closed?.Invoke();
        }

        public void CustomActivity()
        {
            if (Visible)
            {
                HandlePlayerInput();
            }
        }
    }
}
