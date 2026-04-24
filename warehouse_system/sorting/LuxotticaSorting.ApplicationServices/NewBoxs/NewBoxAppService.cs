using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.MappingSorter;
using LuxotticaSorting.ApplicationServices.Shared.DTO.NewBoxs;
using LuxotticaSorting.DataAccess.Repositories.MappingSorters;
using LuxotticaSorting.DataAccess.Repositories.NewBoxs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.NewBoxs
{
    public class NewBoxAppService : INewBoxAppService
    {
        private readonly IRepository<int, NewBoxAddDto> _repository;
        private readonly IMapper _mapper;
        private readonly ILogger<INewBoxAppService> _logger;
        public NewBoxAppService(IRepository<int, NewBoxAddDto> repository, IMapper mapper, ILogger<INewBoxAppService> logger)
        {
            _repository = repository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task AddNewBoxAsync(NewBoxAddDto NewBox)
        {
            try
            {
                var c = _mapper.Map<NewBoxAddDto>(NewBox);
                await _repository.AddAsync(c);
            }
            catch (Exception ex)
            {
                _logger.LogError($"NewBoxAppService method AddNewBoxAsync failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
