using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Udemy.AdvertisementApp.Business.Interfaces;
using Udemy.AdvertisementApp.Business.Mappings.AutoMapper;
using Udemy.AdvertisementApp.Business.Services;
using Udemy.AdvertisementApp.Business.ValidationRules;
using Udemy.AdvertisementApp.DataAccess.Contexts;
using Udemy.AdvertisementApp.DataAccess.UnitOfWork;
using Udemy.AdvertisementApp.Dtos;

namespace Udemy.AdvertisementApp.Business.DependencyResolvers.Microsoft
{
    public static class DependencyExtension
    {
        public static void AddDependencies(this IServiceCollection service, IConfiguration configuration)
        {
            service.AddDbContext<AdvertisementContext>(opt =>
            {
                opt.UseSqlServer(configuration.GetConnectionString("Local"));
            });

            var mapperConfiguration = new MapperConfiguration(opt =>
            {
                opt.AddProfile(new ProvidedServiceProfile());
            });
            var mapper = mapperConfiguration.CreateMapper();

            service.AddSingleton(mapper);

            service.AddScoped<IUow, Uow>();

            service.AddTransient<IValidator<ProvidedServiceCreateDto>,ProvidedServiceCreateDtoValidator>();
            service.AddTransient<IValidator<ProvidedServiceUpdateDto>, ProvidedServiceUpdateDtoValidator>();

            service.AddScoped<IProvidedServiceService, ProvidedServiceService>();
        }
    }
}
