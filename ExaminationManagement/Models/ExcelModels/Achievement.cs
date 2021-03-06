﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ExaminationManagement.Models.ExcelModels
{
    /// <summary>
    /// Excel文件学生成绩导入模板
    /// </summary>
    public class Achievement
    {
        /// <summary>
        /// 学生姓名
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// 学号
        /// </summary>
        public string StudentId { get; set; }
        /// <summary>
        /// 平时成绩
        /// </summary>
        public double RegularGrade{ get; set; }
        /// <summary>
        /// 期中成绩
        /// </summary>
        public double MidtermGrade { get; set; }
        /// <summary>
        /// 期末成绩
        /// </summary>
        public double FinalExamGrade { get; set; }
    }
}