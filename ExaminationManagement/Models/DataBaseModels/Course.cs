﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExaminationManagement.Models.DataBaseModels
{
    public class Course: WebModels.Course
    {
        public int CourseId { get; set; }
        public int? Grade { get; set; }
        public string Teacher { get; set; }
    }
}