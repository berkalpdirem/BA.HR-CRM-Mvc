﻿using BA.HR_Project.Domain.Entities;
using BA.HR_Project.Domain.Enums;

namespace BA.HR_Project.WEB.Models
{
    public class ExpenseViewModel
    {
        public string Id { get; set; }
        public string RequestNumber { get; set; }
        public float RequestPrice { get; set; }

        public DateTime RequestDate { get; set; }
        public DateTime? ReplyDate { get; set; }
        public string? PhotoPath { get; set; }
        public string? FilePath { get; set; }
        public ConfirmStatus ConfirmStatus { get; set; }
        public Currency Currency { get; set; }
        public ExpenseType? ExpenseType { get; set; }

        public AppUser AppUser { get; set; }
        public string AppUserId { get; set; }
    }
}
