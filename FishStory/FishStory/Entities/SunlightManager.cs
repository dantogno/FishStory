using FishStory.Screens;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory.Entities
{
    public static class SunlightManager
    {
        public static TimeSpan TimeOfDay => OurInGameDay.TimeOfDay;
        public static DateTime OurInGameDay = new DateTime();
        public static bool SunIsUp = OurInGameDay.Hour > 6 && OurInGameDay.Hour < 19;
        public static bool MoonIsUp = OurInGameDay.Hour > 22 || OurInGameDay.Hour < 3;
        public static float SunlightEffectiveness =>MathHelper.Clamp(DistanceToNoon(), 0f,1.0f);

        private static double dayStartGameTime = 0;
        private static double minutesElapsedPerSecond = 6;

        private const float minutesAtNoon = 720;
        private const float minutesAtSundown = 1140;
        private static float minutesWhenPlayerWakes = (float)GameScreen.HourOnClockPlayerWakesIn24H * 60f;
        private static float minutesWhenPlayerIsForcedAsleep = (float)GameScreen.HourOfClockPlayerForcedSleepIn24H * 60f;

        public static void Activity(bool firstCall)
        {
            if (firstCall)
                ResetDay();

            var timeToAdd = FlatRedBall.TimeManager.SecondDifference * minutesElapsedPerSecond;
            OurInGameDay = OurInGameDay.AddMinutes(timeToAdd);
        }

        public static void ResetDay()
        {
            dayStartGameTime = FlatRedBall.TimeManager.CurrentTime;
            minutesElapsedPerSecond = (1440 / GameScreen.RealMinutesPerDay)/60;

            OurInGameDay = new DateTime(2020, 1, 5);
            OurInGameDay = OurInGameDay.AddHours(GameScreen.HourOnClockPlayerWakesIn24H);
        }

        private static float DistanceToNoon()
        {
            var minutesElapsed = (float)OurInGameDay.TimeOfDay.TotalMinutes;
            if (minutesElapsed <= minutesWhenPlayerIsForcedAsleep)
            {
                return 0f;
            }
            else if (minutesElapsed > minutesWhenPlayerWakes && minutesElapsed < minutesAtNoon)
            {
                return ((minutesElapsed - minutesWhenPlayerWakes) / (minutesAtNoon - minutesWhenPlayerWakes));
            }
            else if (minutesElapsed == minutesAtNoon)
            {
                return 1f;
            }
            else if (minutesElapsed < minutesAtSundown)
            {
                var minutesPastNoon = minutesElapsed - minutesAtNoon;
                return 1f - (minutesPastNoon / (minutesAtSundown - minutesAtNoon));
            }
            else return 0f;
        }
    }
}
