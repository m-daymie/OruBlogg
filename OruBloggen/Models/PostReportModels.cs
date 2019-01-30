﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace OruBloggen.Models
{
    public class PostReportModel
    {
        [Key]
        public int PostReportID { get; set; }
        [ForeignKey("PostID")]
        public virtual PostModel PostModel { get; set; }
        public int PostID { get; set; }
        [ForeignKey("ReportID")]
        public virtual ReportModel ReportModel { get; set; }
        public int ReportID { get; set; }
    }
}