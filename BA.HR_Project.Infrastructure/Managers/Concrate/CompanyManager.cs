﻿using AutoMapper;
using BA.HR_Project.Application.DTOs;
using BA.HR_Project.Application.Interfaces;
using BA.HR_Project.Infrasturucture.Managers.Abstract;
using BA.HR_Project.Infrasturucture.Services.Concrate;
using BA.HR_Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BA.HR_Project.Persistance.Context;
using BA.HR_Project.Infrasturucture.RequestResponse;

namespace BA.HR_Project.Infrasturucture.Managers.Concrate
{
    public class CompanyManager : BaseManager<Company, CompanyDto>, ICompanyService
    {
        private readonly AppDbContext _appDbContext;
        public CompanyManager(IMapper mapper, IUow uow, AppDbContext appDbContext) : base(mapper, uow)
        {
            _appDbContext = appDbContext;
        }
        public async Task<Response> AddCompany(CompanyDto companyDto)
        {
            if (!IsMersisNoOrTaxNoAvailable(companyDto.MersisNo, companyDto.TaxNo))
            {
                return Response.Failure("MersisNo and TaxNo is already in use by another company.");
            }
            if (!IsMailAvailable(companyDto.Mail))
            {
                return Response.Failure("Mail is already in use by another company.");
            }
            if (!IsMailAvailable(companyDto.Phone))
            {
                return Response.Failure("Phone is already in use by another company.");
            }
            else
            {
                var result= await Insert(companyDto);
                return result.IsSuccess ? Response.Success() : Response.Failure("Failed to insert Company.");
            }


        }
        private bool IsMersisNoOrTaxNoAvailable(string mersisNo, string taxNo, string companyId = null)
        {
            var existingCompany = _appDbContext.Companies.FirstOrDefault(c => (c.MersisNo == mersisNo) || (c.TaxNo == taxNo));

           if(existingCompany == null)
            {
                return true;
            }
            return false;
        }
        private bool IsPhoneAvailable(string phone)
        {
            var existingCompany = _appDbContext.Companies.FirstOrDefault(c => (c.Phone == phone));

            if (existingCompany == null)
            {
                return true;
            }
            return false;
        }
        private bool IsMailAvailable(string mail)
        {
            var existingCompany2 = _appDbContext.Companies.FirstOrDefault(c => (c.Mail == mail));

            if (existingCompany2 == null)
            {
                return true;
            }
            return false;
        }

        public List<CompanyCustom> GetAllCompanyCustomColumn()
        {
            return _appDbContext.Companies
                        .AsEnumerable() 
                        .Select(c => new CompanyCustom { Id = c.Id.ToString(), CompanyName = c.Name })
                        .ToList();
        }

        
    }
    public class CompanyCustom
    {
 
        public string Id { get; set; }
        public string CompanyName { get; set; }
   
    }
}
