﻿using LuxotticaSorting.Core.BoxTypes;
using LuxotticaSorting.Core.CarrierCodes;
using LuxotticaSorting.Core.ConfirmationBoxes;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.ContainerTypes;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.LogisticAgents;
using LuxotticaSorting.Core.Mapping.BoxTypeDivertLane;
using LuxotticaSorting.Core.Mapping.CarrierCodeDivertLine;
using LuxotticaSorting.Core.Mapping.CarrierCodeLogisticAgent;
using LuxotticaSorting.Core.Mapping.DivertLaneZebraConfiguration;
using LuxotticaSorting.Core.MappingSorter;
using LuxotticaSorting.Core.MultiBoxWaves;
using LuxotticaSorting.Core.RecirculationLimits;
using LuxotticaSorting.Core.ScanLogSortings;
using LuxotticaSorting.Core.WCSRoutingV10;
using LuxotticaSorting.Core.Zebra;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess
{
    public class SortingContext : DbContext
    {
        public DbSet<CarrierCode> CarrierCodes { get; set; }
        public DbSet<BoxType> BoxTypes { get; set; }
        public DbSet<ContainerType> ContainerTypes { get; set; }
        public DbSet<DivertLane> DivertLanes { get; set; }
        public DbSet<LogisticAgent> LogisticAgents { get; set; }
        public DbSet<CarrierCodeDivertLaneMapping> CarrierCodeDivertLaneMappings { get; set; }
        public DbSet<BoxTypeDivertLaneMapping> BoxTypeDivertLaneMappings { get; set; }
        public DbSet<CarrierCodeLogisticAgentMapping> CarrierCodeLogisticAgentMappings { get; set; }
        public DbSet<ContainerTable> Containers { get; set; }
        public DbSet<MappingSorter> MappingSorters { get; set; }
        public DbSet<WCSRoutingV10> wCSRoutingV10s { get; set; }
        public DbSet<ZebraConfiguration> ZebraConfigurations { get; set; }
        public DbSet<ZebraHistorial> ZebraHistorials { get; set; }
        public DbSet<DivertLaneZebraConfigurationMapping> DivertLaneZebraConfigurationMappings { get; set; }
        public DbSet<ConfirmationBox> ConfirmationBoxes { get; set; }
        public DbSet<ScanLogSorting> scanLogSortings { get; set; }
        public DbSet<MultiBoxWave> multiBox_Wave { get; set; }
        public DbSet<RecirculationLimit> RecirculationLimits { get; set; }

        public SortingContext(DbContextOptions<SortingContext> options) : base(options)
        {
            
        }
    }
}
