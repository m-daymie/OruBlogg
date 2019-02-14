﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading;
using System.Diagnostics;
using System.Timers;
using Microsoft.AspNet.Identity;
using OruBloggen.Controllers;
using System.Threading.Tasks;

namespace OruBloggen.Models
{
    public sealed class Jobs
    {
        public Jobs() { }
        public static OruBloggenDbContext ctx = new OruBloggenDbContext();
        public static ApplicationDbContext atx = new ApplicationDbContext();

        public static void MeetingNotification(object source, ElapsedEventArgs e)
        {
            var meetingList = ctx.Meetings.ToList();

            foreach(var meeting in meetingList.Where(m => m.MeetingActive == true))
            {
                
                var compareTime = meeting.MeetingStartDate.AddMinutes(-30);
                var currentTime = DateTime.Now;
                TimeSpan span = compareTime.Subtract(currentTime);
                //Debug.WriteLine(span);
                int days = (int)span.TotalDays;
                int hours = (int)span.TotalHours;
                int minutes = (int)span.TotalMinutes;
                int seconds = (int)span.TotalSeconds;
                string totalString = days + hours + minutes + seconds + "";
                int total = int.Parse(totalString);
                Debug.WriteLine("MellanSkillnad: " + total);
                if (-30 < total && total < 30)
                {
                    var mailLista = new List<string>();
                    foreach(var usermeeting in meeting.UserMeetingModel.Where(u => u.UserModel.UserEmailNotification == true))
                    {
                        mailLista.Add(atx.Users.FirstOrDefault(u => u.Id == usermeeting.UserID).Email);
                    }
                    var notificationController = new NotificationController();
                    var body = "Hej, du har ett möte om 30 minuter, du hittar mötet i OruBloggens kalender: " + meeting.MeetingTitle;
                    notificationController.SendEmail(mailLista, "Påminnelse av möte - Orubloggen", body);
                }
            }
        }
    }

    public sealed class Scheduler
    {
        public Scheduler() { }

        public void Scheduler_Start()
        { 
            System.Timers.Timer aTimer = new System.Timers.Timer();
            aTimer.Elapsed += new ElapsedEventHandler(Jobs.MeetingNotification);
            aTimer.Interval = 60000;
            aTimer.Enabled = true;
        }
    }

}