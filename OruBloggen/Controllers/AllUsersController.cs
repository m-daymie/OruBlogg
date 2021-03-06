﻿using Microsoft.AspNet.Identity;
using OruBloggen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace OruBloggen.Controllers
{
    [Authorize, AuthorizeUser]
    public class AllUsersController : Controller
    {
        // GET: AllUsers
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ShowAllUsers()
        {
            var ctx = new OruBloggenDbContext();
            var userList = new List<UserModel>();

            foreach(var item in ctx.Users)
            {
                if (User.Identity.GetUserId() != item.UserID)
                {
                    userList.Add(item);
                }
            }
            var model = new AllUsersViewModelcs
            {
                AllUsers = userList,
                
            };
            return View(model);
        }

        public JsonResult ListSearchedUsers(string searchString)
        {
            var ctx = new OruBloggenDbContext();

            var userList = ctx.Users.Where(u => String.Concat(u.UserFirstname, " ", u.UserLastname)
            .Contains(searchString)|| u.TeamModel.TeamName.Contains(searchString) || searchString == null).ToList();

            var users = new List<UserModel>();
            foreach (var item in userList)
            {
                users.Add(new UserModel
                {
                    UserFirstname = item.UserFirstname,
                    UserLastname = item.UserLastname,
                    UserID = item.UserID
                });
            }

            return new JsonResult { Data = users, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
           
        }
    }
}