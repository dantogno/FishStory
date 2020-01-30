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

        private static Dictionary<string, SoundEffectInstance> SoundEffectDictionary = new Dictionary<string, SoundEffectInstance>();
        private static Dictionary<SoundEffect, SoundEffectInstance> SoundEffectInstances = new Dictionary<SoundEffect, SoundEffectInstance>();

        #endregion

        #region Public methods
        public static SoundEffectInstance Play(SoundEffect instanceToPlay, bool shouldLoop = false)
        {
            return TryPlay(instanceToPlay, shouldLoop);
        }

        public static bool IsPlaying(SoundEffect instanceToPlay)
        {
            SoundEffectInstance currentSoundEffectInstance;
            if (SoundEffectInstances.TryGetValue(instanceToPlay, out currentSoundEffectInstance))
            {
                return currentSoundEffectInstance.IsDisposed == false && 
                        currentSoundEffectInstance.State == SoundState.Playing;
            }
            else
            {
                return false;
            }
        }

        public static bool PlayIfNotPlaying(SoundEffect soundEffect, string soundTypeName)
        {
            SoundEffectInstance currentSoundEffectInstance;

            if (SoundEffectDictionary.TryGetValue(soundTypeName, out currentSoundEffectInstance))
            {
                if (currentSoundEffectInstance.State != SoundState.Playing)
                {
                    if (currentSoundEffectInstance.IsDisposed)
                    {
                        currentSoundEffectInstance = soundEffect.GetCustomInstance();
                    }
                    Internal_Play(currentSoundEffectInstance);
                    return true;
                }
                return false;
            }
            else
            {
                currentSoundEffectInstance = soundEffect.CreateInstance();
                SoundEffectDictionary.Add(soundTypeName, currentSoundEffectInstance);
                Internal_Play(currentSoundEffectInstance);
                return true;
            }
        }

        public static void Stop(SoundEffect soundEffect)
        {
            SoundEffectInstance currentSoundEffectInstance;

            if (SoundEffectInstances.TryGetValue(soundEffect, out currentSoundEffectInstance))
            {
                currentSoundEffectInstance.Stop();
            }
        }
        #endregion

        #region Private methods
        private static SoundEffectInstance TryPlay(SoundEffect soundEffect, bool shouldLoop = false)
        {
            var instanceToPlay = soundEffect.GetCustomInstance();
            instanceToPlay.IsLooped = shouldLoop;
            if (instanceToPlay.State != SoundState.Playing && !instanceToPlay.IsDisposed)
                Internal_Play(instanceToPlay);

            return instanceToPlay;
        }

        private static void Internal_Play(SoundEffectInstance instanceToPlay)
        {
            instanceToPlay.Volume = CurrentVolume;
            instanceToPlay.Play();
        }

        
        private static SoundEffectInstance GetCustomInstance(this SoundEffect soundEffect)
        {
            SoundEffectInstance instanceToPlay;
            if (SoundEffectInstances.TryGetValue(soundEffect, out instanceToPlay) == false)
            {
                instanceToPlay = soundEffect.CreateInstance();
                SoundEffectInstances.Add(soundEffect, instanceToPlay);
            }
            if (instanceToPlay.IsDisposed)
            {
                instanceToPlay = soundEffect.CreateInstance();
            }
            else if (instanceToPlay.State == SoundState.Playing)
            {
                return soundEffect.CreateInstance();
            }

            return instanceToPlay;
        }
        #endregion
    }
}
