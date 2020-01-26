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
        public const double DefaultMusicLevel = 100.0;
        private static double _musicVolumeLevel = DefaultMusicLevel;
        public static double MusicVolumeLevel
        {
            get => _musicVolumeLevel;
            set
            {
                _musicVolumeLevel = value;
                effectiveMusicVolumeLevel = getSmoothedVolumeLevel(_musicVolumeLevel);
                MediaPlayer.Volume = effectiveMusicVolumeLevel;
            }
        }

        private static float effectiveMusicVolumeLevel = 0.4f;

        private static List<string> _currentPlayList;
        private static int _playlistIndex;
        private static bool _shouldLoopPlaylist;
        private static bool _shouldLoopOneSong;
        public static Song CurrentSong;
        public static bool IsSongPlaying => CurrentSong != null &&
                                            CurrentSong.Position >= TimeSpan.Zero &&
                                            (CurrentSong.Position < CurrentSong.Duration || _shouldLoopOneSong);


        public static void PlaySong(Song songToPlay, bool forceRestart = true, bool shouldLoop = true)
        {
            try
            {
                CurrentSong = songToPlay;
                _shouldLoopOneSong = shouldLoop;
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

        public static void PlaySongList(List<string> songsToPlay, bool loopPlaylist = true)
        {
            if (songsToPlay == null || songsToPlay.Count < 1) return;
            _currentPlayList = songsToPlay;
            _playlistIndex = 0;
            _shouldLoopPlaylist = loopPlaylist;
            _shouldLoopOneSong = false;
            var firstSongName = _currentPlayList[_playlistIndex];
            var firstSong = GlobalContent.GetFile(firstSongName) as Song;

            AudioManager.PlaySong(firstSong, true, true);
            MediaPlayer.Volume = effectiveMusicVolumeLevel;
        }

        private static void PlayNextSongInList()
        {
            if (_currentPlayList == null || _currentPlayList.Count < 1) return;

            _playlistIndex += 1;
            if (_playlistIndex > _currentPlayList.Count - 1)
            {
                if (_shouldLoopPlaylist)
                {
                    _playlistIndex = 0;
                }
                else
                {
                    _currentPlayList = null;
                    return;
                }
            }
            var nextSongName = _currentPlayList[_playlistIndex];
            var nextSong = GlobalContent.GetFile(nextSongName) as Song;

            AudioManager.PlaySong(nextSong, true, true);
            MediaPlayer.Volume = effectiveMusicVolumeLevel;
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
