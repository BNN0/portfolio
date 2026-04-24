﻿using AutoMapper;
using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.NewBoxs;
using LuxotticaSorting.ApplicationServices.Shared.DTO.NewBoxs;
using LuxotticaSorting.ApplicationServices.Shared.DTO.RoutingV10S;
using LuxotticaSorting.Core.WCSRoutingV10;
using LuxotticaSorting.DataAccess.Repositories.RoutingV10S;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.ApplicationServices.DivertBox
{
    public class DivertBoxAppService : IDivertBoxAppService
    {
        private readonly IRepository<int, WCSRoutingV10> _repository;
        private readonly RoutingV10Repository _routingV10Repository;
        private readonly ILogger<IDivertBoxAppService> _logger;
        public DivertBoxAppService(IRepository<int, WCSRoutingV10> repository, ILogger<IDivertBoxAppService> logger, RoutingV10Repository routingV10Repository)
        {
            _repository = repository;
            _routingV10Repository = routingV10Repository;
            _logger = logger;
        }

        public async Task<DivertBoxRespDto> DivertBox(string boxId, int trackingId)
        {
            var result = await _routingV10Repository.DivertBox(boxId, trackingId);
            return result;
        }

        public async Task<bool> DivertConfirm(string div_timestamp, int trackingId)
        {
            try
            {
                var result = await _routingV10Repository.DivertConfirmBox(div_timestamp, trackingId);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertBoxAppService method DivertConfirm error occurred while confirming divert for div_timestamp: {div_timestamp} and TrackingId: {trackingId}, error:{ex.Message}");
                throw;
            }
        }

        public async Task RegisterHospital(string boxId, int d)
        {
            try
            {
                await _routingV10Repository.AddScanLogHospital(boxId,d);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertBoxAppService method RegisterHospital failed, error: {ex.Message}");
                throw;
            }
        }


        public async Task RegisterAddScanLog(WCSRoutingV10 result)
        {
            try
            {
                await _routingV10Repository.AddScanLog(result);
            }
            catch (Exception ex)
            {
                _logger.LogError($"DivertBoxAppService method RegisterAddScanLog failed, error: {ex.Message}");
                throw;
            }
        }
    }
}
