using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory.Managers
{
    public static class SoundManager
    {
        #region Fields & Properties
        public static float CurrentVolume = 0.1f;

        private static SoundEffectInstance currentSoundEffectInstance = null;
        private static SoundEffect lastSoundEffectPlayed = null;
        #endregion

        #region Public methods
        public static void Play(SoundEffect instanceToPlay)
        {
            TryPlay(instanceToPlay);
        }

        public static bool PlayIfNotPlaying(SoundEffect soundEffect)
        {
            if (currentSoundEffectInstance != null)
            {
                if (currentSoundEffectInstance.State != SoundState.Playing)
                {
                    currentSoundEffectInstance.Dispose();
                    lastSoundEffectPlayed = soundEffect;
                    currentSoundEffectInstance = soundEffect.GetCustomInstance();
                    Internal_Play(currentSoundEffectInstance);
                    return true;
                }
                return false;
            }
            else
            {
                lastSoundEffectPlayed = soundEffect;
                currentSoundEffectInstance = soundEffect.CreateInstance();
                Internal_Play(currentSoundEffectInstance);
                return true;
            }
        }
        #endregion

        #region Private methods
        private static void TryPlay(SoundEffect soundEffect)
        {
            var instanceToPlay = soundEffect.GetCustomInstance();

            if (instanceToPlay.State != SoundState.Playing && !instanceToPlay.IsDisposed)
                Internal_Play(instanceToPlay);
        }

        private static void Internal_Play(SoundEffectInstance instanceToPlay)
        {
            instanceToPlay.Volume = CurrentVolume;
            instanceToPlay.Play();
        }

        private static Dictionary<SoundEffect, SoundEffectInstance> SoundEffectInstances = new Dictionary<SoundEffect, SoundEffectInstance>();
        private static SoundEffectInstance GetCustomInstance(this SoundEffect soundEffect)
        {
            SoundEffectInstance instanceToPlay;
            if (SoundEffectInstances.TryGetValue(soundEffect, out instanceToPlay) == false)
            {
                instanceToPlay = soundEffect.CreateInstance();
                SoundEffectInstances.Add(soundEffect, instanceToPlay);
            }
            if (instanceToPlay.IsDisposed)
                instanceToPlay = soundEffect.CreateInstance();

            return instanceToPlay;
        }
        #endregion
    }
}
