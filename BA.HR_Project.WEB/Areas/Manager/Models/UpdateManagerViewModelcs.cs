﻿namespace BA.HR_Project.WEB.Areas.Manager.Models
{
    public class UpdateManagerViewModelcs
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string? SecondName { get; set; }
        public string Surname { get; set; }
        public string? SecondSurname { get; set; }
        public IFormFile Photo { get; set; }
        public string? PhotoPath { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime BirthDate { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? Salary { get; set; }
        public bool IsActive { get; set; }
        public string? BirthPlace { get; set; }
        public bool IsTurkishCitizen { get; set; }
        public string? IdentityNumber { get; set; }

        public string? PassportNumber { get; set; }
        public string Adress { get; set; }
        public string CompanyId { get; set; }
        public string DepartmentId { get; set; }
       
    }
}
