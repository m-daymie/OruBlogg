﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;
namespace OruBloggen.Models
{
    public static class Utility
    {



        //public static bool isAdmin(string userId)
        //{
        //    var ctx = new OruBloggenDbContext();

        //    return ctx.Users.FirstOrDefault(u => u.UserID == userId).UserIsAdmin;
        //}

        public static string getCategoryById(int categoryId)
        {
            var ctx = new OruBloggenDbContext();
            return ctx.Categories.FirstOrDefault(u => u.CategoryID == categoryId).CategoryName;
        }
    }
}