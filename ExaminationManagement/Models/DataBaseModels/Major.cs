﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExaminationManagement.Models.DataBaseModels
{
    public class Major
    {
        public int MajorId { get; set; }
        public string MajorName { get; set; }
        public double Credit { get; set; }
    }
}