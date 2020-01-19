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
        private static SoundEffectInstance currentSoundEffectInstance = null;
        private static SoundEffect lastSoundEffectPlayed = null;
        public static void Play(SoundEffect soundEffect)
        {
            soundEffect.Play();
        }
        public static void PlayIfNotPlaying(SoundEffect soundEffect)
        {
            if (currentSoundEffectInstance != null)
            {
                if (currentSoundEffectInstance.State != SoundState.Playing)
                {
                    currentSoundEffectInstance.Dispose();
                    lastSoundEffectPlayed = soundEffect;
                    currentSoundEffectInstance = soundEffect.CreateInstance();
                    currentSoundEffectInstance.Play();
                }
                else if (soundEffect != lastSoundEffectPlayed)
                {
                    soundEffect.Play();
                }
            }
            else
            {
                lastSoundEffectPlayed = soundEffect;
                currentSoundEffectInstance = soundEffect.CreateInstance();
                currentSoundEffectInstance.Play();
            }
        }
    }
}
