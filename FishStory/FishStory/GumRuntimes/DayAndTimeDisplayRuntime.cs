using FishStory.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using FishStory.Screens;

namespace FishStory.GumRuntimes
{
    public partial class DayAndTimeDisplayRuntime
    {
        private DateTime dayOne = new DateTime(2020, 1, 5);
        private int currentDay = 1;
        private int currentHour = (int)GameScreen.HourOnClockPlayerWakesIn24H;

        partial void CustomInitialize () 
        {
        }


        public void UpdateTime(DateTime gameDateTime)
        {
            SetHour(gameDateTime.TimeOfDay.TotalHours);
            SetDay((gameDateTime - dayOne).Days + 1);
        }

        private void SetDay(int day)
        {
            if (currentDay != day)
            {
                currentDay = day;
                DayCountDisplay = $"Day {currentDay}";
            }
        }

        private void SetHour(double hour)
        {
            if (currentHour != (int)hour)
            {
                currentHour = (int)hour;
                TimeDisplay = $"{GetHourDisplay(currentHour)}{(currentHour > 11 ? "PM" : "AM")}";
            }
        }

        private int GetHourDisplay(int hourForDisplay)
        {
            if (hourForDisplay % 12 == 0)
                return 12;
            else if (hourForDisplay < 12)
                return hourForDisplay;
            else 
                return hourForDisplay - 12;
        }

    }
}
