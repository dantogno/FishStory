using FlatRedBall;
using FlatRedBall.Screens;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FishStory.GumRuntimes
{
    public partial class NotificationBoxRuntime
    {
        partial void CustomInitialize () 
        {
        }
        const int NotificationDisplayTimeInSeconds = 5;
        public void CustomActivity()
        {
            if (Visible)
            {
                for(int i = NotificationContainer.Children.Count() -1; i > -1; i--)
                {
                    var notification = NotificationContainer.Children.ElementAt(i);
                    if(TimeManager.CurrentScreenSecondsSince(notification.TimeCreated) >
                        NotificationDisplayTimeInSeconds)
                    {
                        NotificationContainer.RemoveChild(notification);

                        UpdateVisibility();
                    }
                }
            }
        }

        public void AddNotification(string notificationText)
        {
            var notification = new NotificationRuntime();
            notification.TimeCreated = TimeManager.CurrentScreenTime;
            notification.Text = notificationText;
            this.NotificationContainer.AddChild(notification);

            UpdateVisibility();
        }

        private void UpdateVisibility()
        {
            Visible = NotificationContainer.Children.Count() > 0;
        }
    }
}
