using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FlatRedBall.Audio;
using FlatRedBall.Managers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace FishStory.Managers
{
    public static class MusicManager
    {
        public const double DefaultMusicLevel = 80.0;
        private static double _musicVolumeLevel = DefaultMusicLevel;
        public static double MusicVolumeLevel
        {
            get => _musicVolumeLevel;
            set
            {
                _musicVolumeLevel = value;
                
                effectiveMusicVolumeLevel = getSmoothedVolumeLevel(_musicVolumeLevel);
                //effectiveMusicVolumeLevel = (float)value;

                MediaPlayer.Volume = effectiveMusicVolumeLevel;
            }
        }

        private static float effectiveMusicVolumeLevel = 0.4f;

        public static Song CurrentSong;
        public static bool IsSongPlaying => CurrentSong != null && 
                                            CurrentSong.Position >= TimeSpan.Zero &&
                                            (CurrentSong.Position < CurrentSong.Duration);


        public static void PlaySong(Song songToPlay, bool forceRestart = true)
        {
            try
            {
                CurrentSong = songToPlay;
                AudioManager.PlaySong(CurrentSong, forceRestart, true);
                MediaPlayer.Volume = effectiveMusicVolumeLevel;
            }
            catch (Exception e)
            {
#if DEBUG
                throw e;//Alert developer if debugging
#endif
                //Else do nothing
            }
        }

        private const double a = 1e-3;
        private const double b = 6.908;
        private static float getSmoothedVolumeLevel(double value)
        {
            var x = value / 100f;

            var smoothValue = MathHelper.Clamp((float)(a * Math.Exp(b * x)), 0f, 1f);

            return smoothValue;
        }

    }
}
