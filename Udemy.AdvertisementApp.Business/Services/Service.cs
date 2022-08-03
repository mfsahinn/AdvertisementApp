using AutoMapper;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Udemy.AdvertisementApp.Business.Extensions;
using Udemy.AdvertisementApp.Business.Interfaces;
using Udemy.AdvertisementApp.Common;
using Udemy.AdvertisementApp.DataAccess.UnitOfWork;
using Udemy.AdvertisementApp.Dtos.Interfaces;
using Udemy.AdvertisementApp.Entities;

namespace Udemy.AdvertisementApp.Business.Services
{
    public class Service<CreatedDto, UpdateDto, ListDto, T> : IService<CreatedDto, UpdateDto, ListDto, T>
        where CreatedDto : class, IDto, new()
        where UpdateDto : class, IUpdateDto, new()
        where ListDto : class, IDto, new()
        where T : BaseEntity
    {
        private readonly IMapper _mapper;
        private readonly IValidator<CreatedDto> _createDtoValidator;
        private readonly IValidator<UpdateDto> _updateDtoValidator;
        private readonly IUow _uow;


        public Service(IMapper mapper, IValidator<CreatedDto> createDtoValidator, IValidator<UpdateDto> updateDtoValidator, IUow uow)
        {
            _mapper = mapper;
            _createDtoValidator = createDtoValidator;
            _updateDtoValidator = updateDtoValidator;
            _uow = uow;
        }

        public async Task<IResponse<CreatedDto>> CreateAsync(CreatedDto dto)
        {
            var result = _createDtoValidator.Validate(dto);
            if (result.IsValid)
            {
                var createdEntity = _mapper.Map<T>(dto);
                await _uow.GetRepository<T>().CreateAsync(createdEntity);

                return new Response<CreatedDto>(ResponseType.Success, dto);
            }
            return new Response<CreatedDto>(dto,result.ConvertToCustomValidationError());
        }

        public async Task<IResponse<List<ListDto>>> GetAllAsync()
        {
            var data = await _uow.GetRepository<T>().GetAllAsync();
            var dto = _mapper.Map<List<ListDto>>(data);

            return new Response<List<ListDto>>(ResponseType.Success, dto);
        }

        public async Task<IResponse<IDto>> GetByIdAsync<IDto>(int id)
        {
            var data = await _uow.GetRepository<T>().GetByFilter(x => x.Id == id);
            if (data == null)
            {
                return new Response<IDto>(ResponseType.NotFound, $"{id} ye sahip data bulunamadı");
            }
            var dto = _mapper.Map<IDto>(data);
            
            return new Response<IDto>(ResponseType.Success, dto);
        }

        public async Task<IResponse> RemoveAsync(int id)
        {
            var data = await _uow.GetRepository<T>().FindAsync(id);
            if (data ==null)
            {
                return new Response(ResponseType.NotFound, $"{id} ye sahip data bulunamadı");
            }

            _uow.GetRepository<T>().Remove(data);
            return new Response(ResponseType.Success);
        }

        public async Task<IResponse<UpdateDto>> UpdateAsync(UpdateDto dto)
        {
            var result =_updateDtoValidator.Validate(dto);
            if (result.IsValid)
            {
                var unchangedData = await _uow.GetRepository<T>().FindAsync(dto.Id);
                if (unchangedData == null)
                {
                    return new Response<UpdateDto>(ResponseType.NotFound, $"{dto.Id} idsine sahip veri bulunamadı.");
                }
                var entity = _mapper.Map<T>(dto);
                _uow.GetRepository<T>().Update(entity, unchangedData);
                return new Response<UpdateDto>(ResponseType.Success, dto);
            }

            return new Response<UpdateDto>(dto, result.ConvertToCustomValidationError());
        }
    }
}
