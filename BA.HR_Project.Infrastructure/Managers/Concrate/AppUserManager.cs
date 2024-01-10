﻿using BA.HR_Project.Infrasturucture.Managers.Abstract;
using BA.HR_Project.Infrasturucture.Services.Concrate;
using BA.HR_Project.Application.DTOs;
using BA.HR_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using BA.HR_Project.Application.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using BA.HR_Project.Infrastructure.Services.Abstract;
using System.Security.Claims;
using BA.HR_Project.Persistance.Context;
using Microsoft.EntityFrameworkCore;
using BA.HR_Project.Infrasturucture.RequestResponse;

namespace BA.HR_Project.Infrasturucture.Managers.Concrate
{
    public class AppUserManager : BaseManager<AppUser, IDTO>, IAppUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IEmailService _emailService;
        private readonly AppDbContext _appDbContext;
        public AppUserManager(IMapper mapper, IUow uow, UserManager<AppUser> userManager, IUrlHelperFactory urlHelperFactory, IActionContextAccessor actionContextAccessor, IEmailService emailService, AppDbContext appDbContext) : base(mapper, uow)
        {
            _userManager = userManager;

            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
            _emailService = emailService;
            _appDbContext = appDbContext;
            _userManager = userManager;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _emailService = emailService;
            _appDbContext = appDbContext;
        }

        public async Task<Response> AddAppUser(AppUserDto userDto, ClaimsPrincipal User)
        {
            var newUser = _mapper.Map<AppUser>(userDto);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                newUser.CompanyId = currentUser.CompanyId;
                newUser.DepartmentId = currentUser.DepartmentId;
            }

            string mail = newUser.Name + newUser.SecondName + "." + newUser.Surname + newUser.SecondSurname + "@bilgeadamboost.com";
            if (await _userManager.FindByEmailAsync(mail) != null)
            {
                string emailPrefix = newUser.Name[0].ToString().ToLower();
                newUser.Email = $"{emailPrefix + newUser.SecondSurname}.{newUser.Surname + newUser.SecondSurname}@bilgeadamboost.com";
                if (await _userManager.FindByEmailAsync(newUser.Email) != null)
                {
                    string emailPrefix2 = newUser.Surname[0].ToString().ToLower();
                    newUser.Email = $"{newUser.Name + newUser.SecondName}.{emailPrefix2 + newUser.SecondSurname}@bilgeadamboost.com";
                }
            }
            else
            {
                newUser.Email = mail;
            }

            if (!IsPhoneAvailable(userDto.PhoneNumber))
            {
                return Response.Failure("Phone Number is already in use by another company.");
            }
            if (!IsEmailAvailable(userDto.Email))
            {
                return Response.Failure("Email is already in use by another company.");
            }
            if (!IsIdentityNumberAvailable(userDto.IdentityNumber))
            {
                return Response.Failure("Identity No is already in use by another company.");
            }


            newUser.PhotoPath = "/mexant/assets/images/Default.jpg";
            newUser.UserName = newUser.Email;
            newUser.Id = Guid.NewGuid().ToString();




            var createUserAction = await _userManager.CreateAsync(newUser, "Pw.1234");
            var AddRoleAction = await _userManager.AddToRoleAsync(newUser, "Employee");

            if (createUserAction.Succeeded && AddRoleAction.Succeeded)
            {
                await SendEmail(newUser);
                return Response.Success("User added successfully");
            }

            return Response.Failure("Failed to insert User.");
        }

        private bool IsPhoneAvailable(string phone)
        {
            var existingUser = _userManager.Users.FirstOrDefault(u => u.PhoneNumber == phone);

            if (existingUser == null)
            {
                return true;
            }
            return false;
        }
        private bool IsEmailAvailable(string email)
        {
            var existingUser = _userManager.Users.FirstOrDefault(u => u.Email == email);
            if (existingUser == null)
            {
                return true;
            }
            return false;
        }
        private bool IsIdentityNumberAvailable(string identityNumber)
        {
            var existingUser = _userManager.Users.FirstOrDefault(u => u.IdentityNumber == identityNumber);
            if (existingUser == null)
            {
                return true;
            }
            return false;
        }

        public async Task<Response> UpdateAppUser(AppUser userNewProps)
        {
            var oldUser = await _userManager.FindByIdAsync(userNewProps.Id);
            _mapper.Map(userNewProps, oldUser);

            //oldUser.Email = oldUser.Name + oldUser.SecondName + "." + oldUser.Surname + oldUser.SecondSurname + "@bilgeadamboost.com";

            //oldUser.NormalizedEmail = (oldUser.Email).ToUpper();

            //oldUser.UserName = oldUser.Email;
            //oldUser.NormalizedUserName = (oldUser.UserName).ToUpper();

            //if (!IsPhoneAvailable(userNewProps.PhoneNumber))
            //{
            //    return Response.Failure("Phone Number is already in use by another company.");
            //}
            //if (!IsEmailAvailable(userNewProps.Email))
            //{
            //    return Response.Failure("Email is already in use by another company.");
            //}
            //if (!IsIdentityNumberAvailable(userNewProps.IdentityNumber))
            //{
            //    return Response.Failure("Identity No is already in use by another company.");
            //}
            var updateAction = await _userManager.UpdateAsync(oldUser);
            if (updateAction.Succeeded )
            {
                return Response.Success("User Update successfully");
            }

            return Response.Failure("Failed to insert User.");
        }

        private async Task SendEmail(AppUser newUser)
        {
            var resgisteredUser = await _userManager.FindByEmailAsync(newUser.Email);

            var token = await _userManager.GeneratePasswordResetTokenAsync(resgisteredUser);
            var userId = newUser.Id;

            var url = EmailChangePasswordLinkGenerator(token, userId);
            var html = GenerateChangePasswordEmail(url, newUser.Email, "Pw.1234");

            _emailService.SendEmail(newUser.Email, "Change Password", html);
        }

        private string EmailChangePasswordLinkGenerator(string token, string newUserId)
        {
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            return urlHelper.Action("UpdatePassword", "Account", new { area = "", token, newUserId }, "https");

        }

        private string GenerateChangePasswordEmail(string url, string newUserEmail, string newUserPw)
        {
            var html = $@"<html><head></head>

                    <body>
                    <h2>Welcome to BilgeAdam Technology</2>
                     <p> We are happy to see you among us. You are registered by BilgeAdam Technology.Please,firstly you click the link after than you must be Login to refresh your password.</p>
                     <p>Your Login informations:</p><br>
                     <p>Email: {newUserEmail}</p> <br>
                     <p>Password: {newUserPw}</p><br>
                     <a href ={url}>Click</a>
                     </html>";
            return html;

        }

        public async Task<Response> AddManager(AppUserDto managerDto)
        {
           var newManager = _mapper.Map<AppUser>(managerDto);
            string mail = newManager.Name + newManager.SecondName + "." + newManager.Surname + newManager.SecondSurname + "@bilgeadamboost.com";
            if (await _userManager.FindByEmailAsync(mail) !=null) 
            {
              string regulatedMail = newManager.Name[0].ToString().ToLower();
                newManager.Email = $"{regulatedMail + newManager.SecondName}.{newManager.Surname + newManager.SecondSurname}@bilgeadamboost.com";
                if (await _userManager.FindByEmailAsync(newManager.Email) != null)
                {
                    string regulatedMail2 = newManager.Surname[0].ToString().ToLower();
                    newManager.Email = $"{newManager.Name + newManager.SecondName}.{regulatedMail2 + newManager.SecondSurname}@bilgeadamboost.com";
                }
            
            }
            else
            {
                newManager.Email = mail;
            }
            // company and departmant ıd eklenecek...
            newManager.PhotoPath = "/mexant/assets/images/Default.jpg";
            newManager.UserName = newManager.Email;
            newManager.Id = Guid.NewGuid().ToString();

            if (!IsPhoneAvailable(managerDto.PhoneNumber))
            {
                return Response.Failure("Phone Number is already in use by another company.");
            }
            if (!IsEmailAvailable(managerDto.Email))
            {
                return Response.Failure("Email is already in use by another company.");
            }
            if (!IsIdentityNumberAvailable(managerDto.IdentityNumber))
            {
                return Response.Failure("Identity No is already in use by another company.");
            }

            var createManagerAction = await _userManager.CreateAsync(newManager,"Mngr.9876");
            var addRoleAction = await _userManager.AddToRoleAsync(newManager, "Admin");

            if (createManagerAction.Succeeded && addRoleAction.Succeeded)
            {
                await SendEmail(newManager);
                return Response.Success("Manager added successfully");
            }

            return Response.Failure("Failed to insert manager.");
        }

       
        
    }
}
