using System;
using FlatRedBall;
using FlatRedBall.Input;
using FlatRedBall.Instructions;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Specialized;
using FlatRedBall.Audio;
using FlatRedBall.Screens;
using FishStory.Entities;
using FishStory.Screens;
namespace FishStory.Screens
{
    public partial class GameScreen
    {
        void OnPlayerCharacterInstanceVsExitListCollisionOccurred (FishStory.Entities.PlayerCharacter first, Entities.Exit second) 
        {
            var screenToGoTo = second.LevelName;
            if(string.IsNullOrEmpty(screenToGoTo))
            {
                throw new Exception($"The exit {second.Name} does not have a target screen to go to!");
            }
            this.MoveToScreen(screenToGoTo);
        }

    }
}
