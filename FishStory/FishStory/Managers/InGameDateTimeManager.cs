using FishStory.DataTypes;
using FishStory.Screens;
using Microsoft.Xna.Framework;
using System;

namespace FishStory.Managers
{
    public static class InGameDateTimeManager
    {
        private const float minutesPerHour = 60f;
        private const float minutesPerDay = 60f * 24f;
        public static TimeSpan TimeOfDay => OurInGameDay.TimeOfDay;
        public static DateTime OurInGameDay = new DateTime();
        public static bool SunIsUp = OurInGameDay.Hour > GameScreen.HourOnClockSunRisesIn24H && OurInGameDay.Hour < GameScreen.HourOnClockSunSetsIn24H;
        public static bool MoonIsUp = OurInGameDay.Hour > 22 || OurInGameDay.Hour < 3;
        public static float SunlightEffectiveness =>MathHelper.Clamp(GetSunlightCoefficient(DistanceToNoon()), 0.35f,1.0f);

        public static int HourToFreezeTimeIfPlayerNeedsKeyOnDay1 = 20;
        private static double minutesElapsedPerSecond = 6;

        private const float minutesAtNoon = minutesPerDay/2;
        private static float minutesWhenItStartsGettingDark = 15 * minutesPerHour;
        private static float minutesAtSundown = (float)GameScreen.HourOnClockSunSetsIn24H * minutesPerHour;
        private static float minutesWhenPlayerWakes = (float)GameScreen.HourOnClockPlayerWakesIn24H * minutesPerHour;
        private static float minutesWhenPlayerIsForcedAsleep = (float)GameScreen.HourOnClockPlayerForcedSleepIn24H * minutesPerHour;

        public static void Activity(bool firstCall)
        {
            if (firstCall)
            {
                InitializeDay();
            }

            var timeToAdd = FlatRedBall.TimeManager.SecondDifference * minutesElapsedPerSecond;

            // David: I'm putting this in as a quick way to prevent the case
            // where the time runs out on the first day before the player has
            // gotten the key. 
            // There's probably a more elegant solution, but this works for now.
            // https://github.com/dantogno/FishStory/issues/142
            bool shouldNotUpdateTimeBecausePlayerNeedsKey =
                PlayerDataManager.PlayerData.CurrentDay == 1
                && !PlayerDataManager.PlayerData.Has(ItemDefinition.Trailer_Key)
                && TimeOfDay.Hours >= HourToFreezeTimeIfPlayerNeedsKeyOnDay1;

            if (!shouldNotUpdateTimeBecausePlayerNeedsKey)
                OurInGameDay = OurInGameDay.AddMinutes(timeToAdd);
        }

        private static void InitializeDay()
        {
            minutesElapsedPerSecond = (minutesPerDay / GameScreen.RealMinutesPerDay) / minutesPerHour;

            OurInGameDay = new DateTime(2020, 1, 5);
            OurInGameDay = OurInGameDay.AddHours(GameScreen.HourOnClockPlayerWakesIn24H);
        }

        public static void ResetDay()
        {
            if (TimeOfDay.TotalHours < GameScreen.HourOnClockPlayerForcedSleepIn24H)
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
            else if (minutesElapsed >= minutesAtNoon && minutesElapsed <= minutesWhenItStartsGettingDark)
            {
                return 1f;
            }
            else if (minutesElapsed > minutesWhenPlayerWakes && minutesElapsed < minutesAtNoon)
            {
                return ((minutesElapsed - minutesWhenPlayerWakes) / (minutesAtNoon - minutesWhenPlayerWakes));
            }
            else if (minutesElapsed < minutesAtSundown)
            {
                var minutesPastWhenItGetsDark = minutesElapsed - minutesWhenItStartsGettingDark;
                return 1f - (minutesPastWhenItGetsDark / (minutesAtSundown - minutesWhenItStartsGettingDark));
            }
            else return 0f;
        }

        private static float GetSunlightCoefficient(float coefficient)
        {
            return -1f * (float)Math.Pow((coefficient - 1), 2.0) + 1f;
        }
    }
}
