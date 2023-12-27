﻿using AutoMapper;
using BA.HR_Project.Application.DTOs;
using BA.HR_Project.Domain.Entities;
using BA.HR_Project.Infrasturucture.Services.Concrate;
using BA.HR_Project.WEB.Areas.Admin.Models;
using BA.HR_Project.WEB.Models;
using BA.HR_Project.WEB.ModelValidators;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using NuGet.Packaging.Signing;

namespace BA.HR_Project.WEB.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles =("Admin"))]

    public class AdminEmployeeController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        private readonly IAppUserService _appUserManager;
        private readonly ICompanyService _companyManager;
        private readonly IDepartmentService _departmentManager;
        private readonly IMapper _mapper;



        public AdminEmployeeController(UserManager<AppUser> userManager, IAppUserService appUserManager, ICompanyService companyManager, IDepartmentService departmentManager, IMapper mapper)
        {
            _userManager = userManager;
            _appUserManager = appUserManager;
            _companyManager = companyManager;
            _departmentManager = departmentManager;
            _mapper = mapper;
        }
        [HttpGet("/Admin/Employee/Index")]
        public IActionResult Index()
        {
            return View();
        }


        [HttpGet()]
        public async Task<IActionResult> ListEmployee()
        {
            var users = await _userManager.Users.Include(x => x.Company).Include(x => x.Department).ToListAsync();
            var usersDto = _mapper.Map<List<AppUserDto>>(users);
            var usersvm = _mapper.Map<List<ListEmployeeViewModel>>(usersDto);

            return View(usersvm);
        }

        public async Task<IActionResult> AddEmployee()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> AddEmployee(AppUserViewModel vm)
        {
            var newUser = new AppUser();
            var userDto = _mapper.Map<AppUserDto>(vm);
            newUser = _mapper.Map<AppUser>(userDto);

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                newUser.CompanyId = currentUser.CompanyId;
                newUser.DepartmentId = currentUser.DepartmentId;
            }

            newUser.Email = newUser.Name + "." + newUser.Surname + "@bilgeadamboost.com";

            newUser.PhotoPath = "/mexant/assets/images/Default.jpg";
            newUser.UserName = newUser.Email;
            newUser.Id = Guid.NewGuid().ToString();


            var createUserAction = await _userManager.CreateAsync(newUser, "Pw.1234");
            var AddRoleAction = await _userManager.AddToRoleAsync(newUser, "Employee");

            if (createUserAction.Succeeded && AddRoleAction.Succeeded)
            {
                return RedirectToAction("ListEmployee");
            }


            return RedirectToAction("Warning", "Home");
        }


        public async Task<IActionResult> UpdateEmployee(string Id)
        {
            var user = await _userManager.FindByIdAsync(Id);

            var userdto = _mapper.Map<AppUserUpdateForAdminDto>(user);
            var userViewModel = _mapper.Map<AppUserUpdateForAdminVM>(userdto);

            ViewBag.Citizien = user.IsTurkishCitizen;

            return View(userViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> UpdateEmployee(AppUserUpdateForAdminVM vm)
        {

            //var validator = new AppUserViewModelValidator();
            //var validationResult = await validator.ValidateAsync(updateuser);

            //if (!validationResult.IsValid)
            //{
            //    foreach (var error in validationResult.Errors)
            //    {
            //        ModelState.AddModelError(error.PropertyName, error.ErrorMessage);

            //    }
            //    return View(vm);
            //}


            var updateUserDto = _mapper.Map<AppUserUpdateForAdminDto>(vm);
            var userNewProps = _mapper.Map<AppUser>(updateUserDto);
            var user = await _userManager.FindByIdAsync(vm.Id);

            _mapper.Map(userNewProps, user);
            var UpdateAction = await _userManager.UpdateAsync(user);

            if (UpdateAction.Succeeded)
            {
                return RedirectToAction("ListEmployee");
            }

            return View();
        }




        public async Task<IActionResult> DetailsEmployee(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var userdto = _mapper.Map<AppUserDto>(user);

            var departmentId = userdto.DepartmentId;
            var companyId = userdto.CompanyId;
            var company = await _companyManager.Get(true, x => x.Id == companyId);
            var department = await _departmentManager.Get(true, x => x.Id == departmentId);

            var userViewModels = _mapper.Map<ListDetailInfoViewModel>(userdto);
            ViewBag.DepartmentName = department.Context.Name;
            ViewBag.CompanyName = company.Context.Name;

            return View(userViewModels);

        }


    }
}
