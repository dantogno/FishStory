using FishStory.Screens;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FishStory.Entities
{
    public static class InGameDateTimeManager
    {
        public static TimeSpan TimeOfDay => OurInGameDay.TimeOfDay;
        public static DateTime OurInGameDay = new DateTime();
        public static bool SunIsUp = OurInGameDay.Hour > GameScreen.HourOnClockSunRisesIn24H && OurInGameDay.Hour < GameScreen.HourOnClockSunSetsIn24H;
        public static bool MoonIsUp = OurInGameDay.Hour > 22 || OurInGameDay.Hour < 3;
        public static float SunlightEffectiveness =>MathHelper.Clamp(GetSunlightCoefficient(DistanceToNoon()), 0f,1.0f);

        private static double minutesElapsedPerSecond = 6;

        private const float minutesAtNoon = 720;
        private const float minutesAtSundown = 1140;
        private static float minutesWhenPlayerWakes = (float)GameScreen.HourOnClockPlayerWakesIn24H * 60f;
        private static float minutesWhenPlayerIsForcedAsleep = (float)GameScreen.HourOfClockPlayerForcedSleepIn24H * 60f;

        public static void Activity(bool firstCall)
        {
            if (firstCall)
            {
                InitializeDay();
            }

            var timeToAdd = FlatRedBall.TimeManager.SecondDifference * minutesElapsedPerSecond;
            OurInGameDay = OurInGameDay.AddMinutes(timeToAdd);
        }

        private static void InitializeDay()
        {
            minutesElapsedPerSecond = (1440 / GameScreen.RealMinutesPerDay) / 60;

            OurInGameDay = new DateTime(2020, 1, 5);
            OurInGameDay = OurInGameDay.AddHours(GameScreen.HourOnClockPlayerWakesIn24H);
        }

        public static void ResetDay()
        {
            if (TimeOfDay.TotalHours < GameScreen.HourOfClockPlayerForcedSleepIn24H)
            {
                OurInGameDay = new DateTime(OurInGameDay.Year, OurInGameDay.Month, OurInGameDay.Day);
            }
            else
            {
                OurInGameDay = new DateTime(OurInGameDay.Year, OurInGameDay.Month, OurInGameDay.Day+1);
            }
            OurInGameDay = OurInGameDay.AddHours(GameScreen.HourOnClockPlayerWakesIn24H);
        }

        public static void SetTimeOfDay(TimeSpan time)
        {
            OurInGameDay = new DateTime(OurInGameDay.Year, OurInGameDay.Month, OurInGameDay.Day);
            OurInGameDay = OurInGameDay.Add(time);
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

        private static float GetSunlightCoefficient(float coefficient)
        {
            return -1f * (float)Math.Pow((coefficient - 1), 2.0) + 1f;
        }
    }
}
