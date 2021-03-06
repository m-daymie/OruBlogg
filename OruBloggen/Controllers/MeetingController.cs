﻿using Microsoft.AspNet.Identity;
using OruBloggen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace OruBloggen.Controllers
{
    [Authorize, AuthorizeUser]
    public class MeetingController : Controller
    {
        // GET: Meeting
        public ActionResult Meeting()
        {
            var meetingView = ListUsersBeginning();

            return View(meetingView);
        }

        public MeetingViewModel ListUsersBeginning()
        {
            var ctx = new OruBloggenDbContext();
            var userList = ctx.Users;

            var users = new List<SelectListItem>();
            foreach (var item in userList)
            {
                users.Add(new SelectListItem
                {
                    Text = item.UserFirstname + " " + item.UserLastname,
                    Value = item.UserID
                });
            }

            var meetingView = new MeetingViewModel
            {
                Users = users,
                SelectedUsers = new List<SelectListItem>()
            };

            return meetingView;
        }

        public JsonResult ListSearchedUsers(string searchString)
        {
            var ctx = new OruBloggenDbContext();

            var userList = ctx.Users.Where(u => String.Concat(u.UserFirstname, " ", u.UserLastname)
                                    .Contains(searchString) ||
                                    searchString == null).ToList();


            var users = new List<SelectListItem>();
            foreach (var item in userList)
            {
                users.Add(new SelectListItem
                {
                    Text = item.UserFirstname + " " + item.UserLastname,
                    Value = item.UserID
                });
            }
            return new JsonResult { Data = users, JsonRequestBehavior = JsonRequestBehavior.AllowGet };

        }

        [HttpPost]
        public ActionResult CreateMeeting(MeetingViewModel model)
        {
            var ctx = new OruBloggenDbContext();
            var userId = User.Identity.GetUserId();

            var notification = new NotificationController();
            var body = "Du har blivit inbjuden till " + model.Meeting.MeetingTitle + Environment.NewLine +
                       "Startdatum: " + model.Meeting.MeetingStartDate.ToShortDateString() + " "
                       + model.Meeting.MeetingStartDate.ToShortTimeString() + Environment.NewLine +

                       "Slutdatum: " + model.Meeting.MeetingEndDate.ToShortDateString() + " "
                       + model.Meeting.MeetingEndDate.ToShortTimeString() + Environment.NewLine +

                       "Beskrivning: " + model.Meeting.MeetingDesc;

            var meeting = ctx.Meetings.Add(new MeetingModel
            {
                MeetingTitle = model.Meeting.MeetingTitle,
                MeetingDesc = model.Meeting.MeetingDesc,
                MeetingStartDate = model.Meeting.MeetingStartDate,
                MeetingEndDate = model.Meeting.MeetingEndDate,
                MeetingUserID = User.Identity.GetUserId()
            });
            ctx.SaveChanges();

                var appCtx = new ApplicationDbContext();
                var emails = new List<string>();
                var phoneNumbers = new List<string>();
                var pmReceivers = new List<UserModel>();
                if (model.SelectedUserIds != null)
                {
                    foreach (var item in model.SelectedUserIds)
                    {
                        ctx.UserMeetings.Add(new UserMeetingModel
                        {
                            MeetingID = ctx.Meetings.OrderByDescending(m => m.MeetingID).First().MeetingID,
                            UserID = item
                        });
                    var user = ctx.Users.FirstOrDefault(u => u.UserID.Equals(item));


                    if (user.UserEmailNotification == true)
                    {
                        emails.Add(appCtx.Users.FirstOrDefault(u => u.Id.Equals(user.UserID)).Email);
                    }

                    if (user.UserSmsNotification == true)
                    {
                        phoneNumbers.Add(ctx.Users.FirstOrDefault(u => u.UserID.Equals(user.UserID)).UserPhoneNumber.ToString());
                        notification.SendSms(ctx.Users.FirstOrDefault(u => u.UserID.Equals(user.UserID)).UserPhoneNumber.ToString(), body);
                    }

                    if (user.UserPmNotification == true)
                    {
                        notification.SendReminderPM(user.UserID, meeting.MeetingTitle, meeting.MeetingDesc, body, meeting.MeetingStartDate, meeting.MeetingEndDate);
                    }

                }
            }

            ctx.SaveChanges();

           
            notification.SendEmail(emails, "Inbjudan till möte", body);

            //foreach (var number in phoneNumbers)
            //{
            //    notificationController.SendSms(number, body);
            //}
            //notificationController.SendMeetingPm(userId, pmReceivers, model.Meeting.MeetingTitle, model.Meeting.MeetingDesc, model.Meeting.MeetingStartDate, model.Meeting.MeetingEndDate);

            //return RedirectToAction("MeetingDetails", new { id = meeting.MeetingID});
            return RedirectToAction("Index", "MeetingCalendar");
           
        }

        public ActionResult MeetingDetails(int? id)
        {
            var ctx = new OruBloggenDbContext();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MeetingModel meeting = ctx.Meetings.Find(id);
            if (meeting == null)
            {
                Console.WriteLine("Test");
                return HttpNotFound();
            }
            return View(meeting);
        }

        //GET
        public ActionResult ListCreatedMeetings()
        {
            var ctx = new OruBloggenDbContext();
            var userId = User.Identity.GetUserId();

            var meetingUserView = new MeetingUserViewModel
            {
                Meetings = ctx.Meetings.Where(m => m.MeetingUserID.Equals(userId)).ToList(),
                UserMeetings = ctx.UserMeetings.Where(u => u.UserID.Equals(userId)).ToList()
            };

            foreach (var meeting in meetingUserView.Meetings)
            {
                if (meeting.MeetingActive)
                {
                    if (DateTime.Now > meeting.MeetingEndDate)
                    {
                        meeting.MeetingActive = false;
                    }
                }
            }
            ctx.SaveChanges();

            return View(meetingUserView);
        }
        /// <summary>
        /// //////////////////////////////////////////////////////////////////////////////
        /// </summary>
        /// <param name="meetingId"></param>
        /// <param name="title"></param>
        /// <param name="startDate"></param>
        /// <returns></returns>
        public ActionResult CancelMeeting(int meetingId, string title, DateTime startDate)
        {
            var ctx = new OruBloggenDbContext();
            var appCtx = new ApplicationDbContext();
            var notification = new NotificationController();
            var emails = new List<string>();
            var phoneNumbers = new List<string>();

            //var meetingActive = ctx.Meetings.FirstOrDefault(m => m.MeetingID == meetingId).MeetingActive;

            if (ctx.Meetings.FirstOrDefault(m => m.MeetingID == meetingId).MeetingActive)
            {
                ctx.Meetings.FirstOrDefault(m => m.MeetingID == meetingId).MeetingActive = false;

                var userMeetings = ctx.UserMeetings.Where(m => m.MeetingID == meetingId);
                string ebody = "";
                foreach (var user in userMeetings)
                
                {
                    var body = "Du har blivit inbjuden till " + user.MeetingModel.MeetingTitle + Environment.NewLine +
                               "Startdatum: " + user.MeetingModel.MeetingStartDate.ToShortDateString() + " "
                               + user.MeetingModel.MeetingStartDate.ToShortTimeString() + Environment.NewLine +

                               "Slutdatum: " + user.MeetingModel.MeetingEndDate.ToShortDateString() + " "
                               + user.MeetingModel.MeetingEndDate.ToShortTimeString() + Environment.NewLine +

                               "Beskrivning: " + user.MeetingModel.MeetingDesc;
                    ebody = body;

                    if (user.UserModel.UserEmailNotification == true)
                    {
                        emails.Add(appCtx.Users.FirstOrDefault(u => u.Id.Equals(user.UserID)).Email);
                    }

                    if (user.UserModel.UserSmsNotification == true)
                    {
                        phoneNumbers.Add(ctx.Users.FirstOrDefault(u => u.UserID.Equals(user.UserID)).UserPhoneNumber.ToString());
                        notification.SendSms(ctx.Users.FirstOrDefault(u => u.UserID.Equals(user.UserID)).UserPhoneNumber.ToString(), body);
                    }

                    if (user.UserModel.UserPmNotification == true)
                    {
                        notification.SendReminderPM(user.UserID, user.MeetingModel.MeetingTitle, user.MeetingModel.MeetingDesc, body, user.MeetingModel.MeetingStartDate, user.MeetingModel.MeetingEndDate);
                    }
                }
                ctx.SaveChanges();

                var notificationController = new NotificationController();
                notificationController.SendEmail(emails, "Mötet är inställt", ebody);
            }
            return RedirectToAction("ListCreatedMeetings");
        }

        public ActionResult AcceptMeeting(int meetingId, bool accepted)
        {
            var ctx = new OruBloggenDbContext();
            var userId = User.Identity.GetUserId();
            ctx.UserMeetings.FirstOrDefault(m => m.MeetingID == meetingId && m.UserID.Equals(userId)).AcceptedInvite = accepted;
            ctx.SaveChanges();

            return RedirectToAction("ListCreatedMeetings");

        }
    }
}