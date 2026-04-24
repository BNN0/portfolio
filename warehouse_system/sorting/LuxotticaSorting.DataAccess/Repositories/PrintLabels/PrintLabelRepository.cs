using Luxottica.DataAccess.Repositories;
using LuxotticaSorting.ApplicationServices.Shared.DTO.ConfirmationBoxes;
using LuxotticaSorting.ApplicationServices.Shared.DTO.Containers;
using LuxotticaSorting.ApplicationServices.Shared.DTO.PrintLabel;
using LuxotticaSorting.Core.Container;
using LuxotticaSorting.Core.ContainerTypes;
using LuxotticaSorting.Core.DivertLanes;
using LuxotticaSorting.Core.Zebra;
using LuxotticaSorting.DataAccess.Repositories.DivertLanes;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LuxotticaSorting.DataAccess.Repositories.PrintLabels
{

    public class PrintLabelRepository : Repository<int, ZebraConfiguration>
    {

        private readonly SAPContext _sapContext;
        private IConfiguration Configuration { get; }
        public PrintLabelRepository(SortingContext context, SAPContext sapContext, IConfiguration configuration) : base(context)
        {
            _sapContext = sapContext;
            Configuration = configuration;
        }
        public async Task<ZebraConfiguration> AddAsync(ZebraConfiguration conf)
        {

            var existingEntity = await Context.ZebraConfigurations.FirstOrDefaultAsync(x => x.NamePrinter == conf.NamePrinter);
            if (existingEntity != null)
            {
                throw new Exception("This Name Printer value already exists.");
            }
            if (string.IsNullOrWhiteSpace(conf.HostName) && string.IsNullOrWhiteSpace(conf.Ip))
            {
                throw new Exception("The hostname or IP cannot be null.");
            }
            if (conf.Ip != null && conf.Ip != "")
            {
                if (!IsValidIPAddress(conf.Ip))
                {
                    throw new Exception("IP address is invalid.");
                }
            }

            var entitytoAdd = new ZebraConfiguration
            {
                NamePrinter = conf.NamePrinter,
                Ip = conf.Ip,
                Port = conf.Port,
                HostName = conf.HostName,
                PortType = conf.PortType,
            };

            Context.ZebraConfigurations.Add(entitytoAdd);
            await Context.SaveChangesAsync();

            return entitytoAdd;
        }

        public override async Task<ZebraConfiguration> DeleteAsync(int id)
        {
            var entity = await Context.ZebraConfigurations.FindAsync(id);

            if (entity == null)
            {
                throw new Exception("The Zebra Configuration does not exist.");
            }
            var existingConfiguration = await Context.DivertLaneZebraConfigurationMappings.FirstOrDefaultAsync(x => x.ZebraConfigurationId == id);
            if (existingConfiguration != null)
            {
                throw new Exception("This Zebra Configuration is mapped to a Divert Lane, please delete that mapping.");
            }
            Context.ZebraConfigurations.Remove(entity);
            await Context.SaveChangesAsync();

            return entity;
        }

        public async Task<ZebraConfiguration> UpdateAsync(ZebraConfiguration conf)
        {
            var entity = await Context.ZebraConfigurations.FindAsync(conf.Id);
            if (entity == null)
            {
                throw new Exception("The Zebra Configuration does not exist.");
            }
            if (conf.NamePrinter != entity.NamePrinter)
            {
                var existingEntity = await Context.ZebraConfigurations.FirstOrDefaultAsync(x => x.NamePrinter == conf.NamePrinter);
                if (existingEntity != null)
                {
                    throw new Exception("This Name Printer value already exists.");
                }
            }
            if (string.IsNullOrWhiteSpace(conf.Ip) && string.IsNullOrWhiteSpace(conf.HostName))
            {
                throw new Exception("The hostname or IP cannot be null.");
            }
            if (conf.Ip != null && conf.Ip != "")
            {
                if (!IsValidIPAddress(conf.Ip))
                {
                    throw new Exception("IP address is invalid.");
                }
            }

            entity.NamePrinter = conf.NamePrinter;
            entity.Ip = conf.Ip;
            entity.Port = conf.Port;
            entity.HostName = conf.HostName;
            entity.PortType = conf.PortType;

            await Context.SaveChangesAsync();

            return entity;
        }
        public async Task AddHistorial(int DivertLineId, int ZebraId, int ContainerId, string Timestamp, bool status)
        {
            var zebraHistorial = new Core.Zebra.ZebraHistorial
            {
                DivertLaneId = DivertLineId,
                ZebraConfigurationId = ZebraId,
                ContainerId = ContainerId,
                Timestamp = Timestamp,
                Status = status
            };
            await Context.ZebraHistorials.AddAsync(zebraHistorial);

            await Context.SaveChangesAsync();
        }
        public async Task<PrintLabelRespDto> PrintLabel(PrintLabelDTO printLabelDTO)
        {
            PrintLabelRespDto response;
            var existingContainer = await Context.Containers.FirstOrDefaultAsync(x => x.Id == printLabelDTO.ContainerId);
            if (existingContainer == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This Container value does not exists."
                };
            }
            var existingSorter = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == printLabelDTO.DivertLaneId);
            if (existingSorter == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = $"This DivertLane value does not exists in MappingSorter."
                };

            }
            string carrierCodeId = existingSorter.CarrierCodeId;
            var carrierCodes = !string.IsNullOrEmpty(carrierCodeId)
                ? carrierCodeId.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(code => code.Trim())
                            .ToArray()
                : new string[0];

            var existingCarrier = await Context.CarrierCodes.FirstOrDefaultAsync(x => carrierCodes.Contains(x.Id.ToString()));
            if (existingCarrier == null)
            {
                return new PrintLabelRespDto
                {
                    Value = false,
                    Message = "One or more CarrierCode values do not exist."
                };
            }
            var existingDivertLane = await Context.DivertLanes.FirstOrDefaultAsync(x => x.Id == existingSorter.DivertLaneId);
            if (existingDivertLane == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This DivertLane value does not exists."
                };
            }
            var existingDivertLane2 = await Context.DivertLaneZebraConfigurationMappings.FirstOrDefaultAsync(x => x.DivertLaneId == printLabelDTO.DivertLaneId);
            if (existingDivertLane2 == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = $"This divertlane is not mapped to any Zebra configuration."
                };
            }

            var existingLogisticAgent = await Context.LogisticAgents.FirstOrDefaultAsync(x => x.Id == existingSorter.LogisticAgentId);
            if (existingLogisticAgent == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "The chosen DivertLane does not have a registered Logistic Agent."
                };
            }


            var existingPrint = await Context.ZebraConfigurations.FirstOrDefaultAsync(x => x.Id == existingDivertLane2.ZebraConfigurationId);
            if (existingPrint == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This Zebra Configuration does not exists."
                };
            }



            string Timestamp = ObtenerFechaActual();

            var label = zplLabel(existingContainer.ContainerId, Timestamp, existingCarrier.CarrierCodes, existingLogisticAgent.LogisticAgents, existingDivertLane.DivertLanes);

            if (existingPrint.HostName != null && existingPrint.HostName != "")
            {
                bool success = SendZPLToPrinter(label, existingPrint.HostName, existingPrint.Port, existingPrint.PortType);
                if (success)
                {

                    return response = new PrintLabelRespDto
                    {
                        Value = true,
                        Message = "Correct"
                    };
                }
                else
                {

                    return response = new PrintLabelRespDto
                    {
                        Value = false,
                        Message = "The connection with the Zebra printer has failed."
                    };
                }

            }
            else
            {
                bool success = SendZPLToPrinter(label, existingPrint.Ip, existingPrint.Port, existingPrint.PortType);
                if (success)
                {

                    return response = new PrintLabelRespDto
                    {
                        Value = true,
                        Message = "Correct"
                    };
                }
                else
                {

                    return response = new PrintLabelRespDto
                    {
                        Value = false,
                        Message = "The connection with the Zebra printer has failed."
                    };
                }
            }
        }

        public async Task<PrintLabelRespDto> Reprint(PrintLabelReprint printLabelDTO)
        {
            PrintLabelRespDto response;
            var historyRecord = await Context.Containers.FirstOrDefaultAsync(x => x.Id == printLabelDTO.ContainerId && x.Status == false);
            if (historyRecord == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This record in Container does not exist or has a successful status."
                };
            }
            var existingDivertLane = await Context.DivertLanes.FirstOrDefaultAsync(x => x.ContainerId == historyRecord.Id);
            if (existingDivertLane == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This DivertLane value does not exists."
                };
            }

            var existingContainer = await Context.Containers.FirstOrDefaultAsync(x => x.Id == historyRecord.Id);
            if (existingContainer == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This Container value does not exists."
                };
            }


            var existingSorter = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == existingDivertLane.Id);
            if (existingSorter == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = $"This DivertLane value {existingDivertLane.Id} does not exists in MappingSorter."
                };

            }


            // var carrierCodes = existingSorter.CarrierCodeId.Split(',').Select(code => code.Trim()).ToArray();
            string carrierCodeId = existingSorter.CarrierCodeId;
            var carrierCodes = !string.IsNullOrEmpty(carrierCodeId)
                ? carrierCodeId.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(code => code.Trim())
                            .ToArray()
                : new string[0];

            var existingCarrier = await Context.CarrierCodes.FirstOrDefaultAsync(x => carrierCodes.Contains(x.Id.ToString()));
            if (existingCarrier == null)
            {
                return new PrintLabelRespDto
                {
                    Value = false,
                    Message = "One or more CarrierCode values do not exist."
                };
            }


            var existingLogisticAgent = await Context.LogisticAgents.FirstOrDefaultAsync(x => x.Id == existingSorter.LogisticAgentId);
            if (existingLogisticAgent == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "The chosen DivertLane does not have a registered Logistic Agent."
                };
            }


            var existingPrint = await Context.ZebraConfigurations.FirstOrDefaultAsync(x => x.Id == printLabelDTO.ZebraConfigurationId);
            if (existingPrint == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This printer is not configured."
                };
            }

            string Timestamp = ObtenerFechaActual();

            var label = zplLabel(existingContainer.ContainerId, Timestamp, existingCarrier.CarrierCodes, existingLogisticAgent.LogisticAgents, existingDivertLane.DivertLanes);

            if (existingPrint.HostName != null && existingPrint.HostName != "")
            {
                bool success = SendZPLToPrinter(label, existingPrint.HostName, existingPrint.Port, existingPrint.PortType);
                if (success)
                {

                    return response = new PrintLabelRespDto
                    {
                        Value = true,
                        Message = "Correct"
                    };
                }
                else
                {
                    await containerToFalse(printLabelDTO.ContainerId);
                    return response = new PrintLabelRespDto
                    {
                        Value = false,
                        Message = "The connection with the Zebra printer has failed."
                    };
                }

            }
            else
            {
                bool success = SendZPLToPrinter(label, existingPrint.Ip, existingPrint.Port, existingPrint.PortType);
                if (success)
                {


                    return response = new PrintLabelRespDto
                    {
                        Value = true,
                        Message = "Correct"
                    };
                }
                else
                {
                    await containerToFalse(printLabelDTO.ContainerId);
                    return response = new PrintLabelRespDto
                    {
                        Value = false,
                        Message = "The connection with the Zebra printer has failed."
                    };
                }
            }
        }
        public async Task<PrintLabelRespDto> PrintLabelManual(PrintManualDTO printLabelDTO)
        {
            PrintLabelRespDto response;
            var existingContainer = await Context.Containers.FirstOrDefaultAsync(x => x.Id == printLabelDTO.ContainerId);
            if (existingContainer == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = $"This container does not exist"
                };
            }
            var existingDivertLane = await Context.DivertLanes.FirstOrDefaultAsync(x => x.ContainerId == printLabelDTO.ContainerId);
            if (existingDivertLane == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This ContainerId is no longer associated with any DivertLane."
                };
            }

            var existingSorter = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == existingDivertLane.Id);
            if (existingSorter == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = $"This DivertLane value {existingDivertLane.Id} does not exists in MappingSorter."
                };

            }
            string carrierCodeId = existingSorter.CarrierCodeId;
            var carrierCodes = !string.IsNullOrEmpty(carrierCodeId)
                ? carrierCodeId.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(code => code.Trim())
                            .ToArray()
                : new string[0];

            var existingCarrier = await Context.CarrierCodes.FirstOrDefaultAsync(x => carrierCodes.Contains(x.Id.ToString()));
            if (existingCarrier == null)
            {
                return new PrintLabelRespDto
                {
                    Value = false,
                    Message = "One or more CarrierCode values do not exist."
                };
            }

            var existingDivertLane2 = await Context.DivertLaneZebraConfigurationMappings.FirstOrDefaultAsync(x => x.DivertLaneId == existingDivertLane.Id);
            if (existingDivertLane2 == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = $"This divertlane {existingDivertLane.Id} is not mapped to any Zebra configuration."
                };
            }


            var existingLogisticAgent = await Context.LogisticAgents.FirstOrDefaultAsync(x => x.Id == existingSorter.LogisticAgentId);
            if (existingLogisticAgent == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "The chosen ContainerId have a DivertLane that does not have a registered Logistic Agent."
                };
            }


            var existingPrint = await Context.ZebraConfigurations.FirstOrDefaultAsync(x => x.Id == existingDivertLane2.ZebraConfigurationId);
            if (existingPrint == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This ZebraConfiguration does not exists."
                };
            }



            string Timestamp = ObtenerFechaActual();

            var label = zplLabel(existingContainer.ContainerId, Timestamp, existingCarrier.CarrierCodes, existingLogisticAgent.LogisticAgents, existingDivertLane.DivertLanes);

            if (existingPrint.HostName != null && existingPrint.HostName != "")
            {
                bool success = SendZPLToPrinter(label, existingPrint.HostName, existingPrint.Port, existingPrint.PortType);
                if (success)
                {

                    return response = new PrintLabelRespDto
                    {
                        Value = true,
                        Message = "Correct"
                    };
                }
                else
                {
                    await containerToFalse(printLabelDTO.ContainerId);
                    return response = new PrintLabelRespDto
                    {
                        Value = false,
                        Message = "The connection with the Zebra printer has failed."
                    };
                }

            }
            else
            {
                bool success = SendZPLToPrinter(label, existingPrint.Ip, existingPrint.Port, existingPrint.PortType);
                if (success)
                {

                    return response = new PrintLabelRespDto
                    {
                        Value = true,
                        Message = "Correct"
                    };
                }
                else
                {
                    await containerToFalse(printLabelDTO.ContainerId);
                    return response = new PrintLabelRespDto
                    {
                        Value = false,
                        Message = "The connection with the Zebra printer has failed."
                    };
                }
            }

        }

        private static string ObtenerFechaActual()
        {
            DateTime now = DateTime.Now;
            string formatoFecha = now.ToString("yyyy/MM/dd  HH:mm:ss");


            return formatoFecha;
        }
        private bool SendZPLToPrinter(string zplLabel, string printerIpAddress, int port, string portType)
        {
            try
            {
                if (portType == "TCP")
                {
                    using (TcpClient client = new TcpClient(printerIpAddress, port))
                    {
                        using (NetworkStream stream = client.GetStream())
                        {
                            byte[] labelBytes = System.Text.Encoding.ASCII.GetBytes(zplLabel);
                            stream.Write(labelBytes, 0, labelBytes.Length);
                        }
                    }
                    return true;
                }

                if (portType == "UDP")
                {
                    using (UdpClient udpClient = new UdpClient())
                    {
                        // Obtener la dirección IP del destino
                        IPAddress printerIpAddressObj = IPAddress.Parse(printerIpAddress);

                        // Crear el extremo de destino
                        IPEndPoint endPoint = new IPEndPoint(printerIpAddressObj, port);

                        // Convertir el mensaje a bytes
                        byte[] labelBytes = Encoding.ASCII.GetBytes(zplLabel);

                        // Enviar los datos por UDP
                        udpClient.Send(labelBytes, labelBytes.Length, endPoint);
                    }

                    return true;
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error printing ZPL label: " + ex.Message);
                return false;
            }
        }

        private string zplLabel(string containerId, string Timestamp, string CarrierCode, string LogisticAgent, int DivertLanes)
        {
            string zplLabel1 = $@"^XA
                            ^FO200,100
                            ^BXN,8,200^FD{containerId}^FS
                            ^FO400,100
                            ^CF0,28
                            ^FD{containerId}^FS
                            ^FO400,100
                            ^FS
                            ^CF0,35
                            ^FO450,150
                            ^FD{Timestamp}^FS
                            ^CF0,35
                            ^FO400,230
                            ^FD {CarrierCode}  {LogisticAgent}^FS
                            ^FO400,300
                            ^FDLane {DivertLanes}^FS
                            ^BY2,2,180^FO200,360^BC,,,,,A^FD{containerId}^FS^CF0,20
                            ^FS
                            ^XZ";
            return zplLabel1;
        }

        private async Task containerToFalse(int id)
        {
            var entity = await Context.Containers.FindAsync(id);
            entity.Status = false;
            await Context.SaveChangesAsync();
        }

        private static bool IsValidIPAddress(string ipAddress)
        {
            string pattern = @"^((25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            Regex regex = new Regex(pattern);
            return regex.IsMatch(ipAddress);
        }

        private string zplLabelMultibox(PrintLabelMultiBoxDto printLabelMultiBoxDto)
        {
            StringBuilder zplBuilder = new StringBuilder();
            zplBuilder.AppendLine("^XA");
            zplBuilder.AppendLine("^FO30,130^GB1150,9,9^FS");

            // Section: Order Number
            zplBuilder.AppendLine($"^CF0,40");
            zplBuilder.AppendLine($"^FO50,180^FDConfirmation Number:^FS");
            zplBuilder.AppendLine($"^FO420,182^ADN,28,10^FD{printLabelMultiBoxDto.ConfirmationNumber}^FS");

            // Section: Boxes Properly Delivered
            zplBuilder.AppendLine($"^CF0,40");
            zplBuilder.AppendLine($"^FO50,240^FDBoxes Properly Delivered:^FS");

            int boxY = 300;
            foreach (var box in printLabelMultiBoxDto.BoxesProperlyDelivered)
            {
                zplBuilder.AppendLine($"^FO60,{boxY}^ADN,30,10^FD-{box}^FS");
                boxY += 60;
            }

            // Section: Boxes Not Delivered Due to Closure
            zplBuilder.AppendLine($"^CF0,40");
            zplBuilder.AppendLine($"^FO50,{boxY + 20}^FDBoxes Not Delivered Due to Closure:^FS");

            int closureBoxY = boxY + 80;
            foreach (var closureBox in printLabelMultiBoxDto.BoxesNotDeliveredDueToClosure)
            {
                zplBuilder.AppendLine($"^FO60,{closureBoxY}^ADN,30,10^FD-{closureBox}^FS");
                closureBoxY += 60;
            }

            // Section: Additional Information
            zplBuilder.AppendLine($"^FO50,{closureBoxY + 20}^GB1150,9,9^FS");
            zplBuilder.AppendLine("^XZ");

            return zplBuilder.ToString();
        }

        public async Task<PrintLabelRespDto> PrintLabelMultiBox(BoxfromRoutingReqDto printLabelDTO)
        {
            PrintLabelRespDto response;
            var existingContainer = await Context.Containers.FirstOrDefaultAsync(x => x.ContainerId == printLabelDTO.ContainerId);
            if (existingContainer == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This Container value does not exists."
                };
            }

            var existingDivertLane = await Context.DivertLanes.FirstOrDefaultAsync(x => x.DivertLanes == printLabelDTO.DivertLane);
            if (existingDivertLane == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This DivertLane value does not exists."
                };
            }

            var existingSorter = await Context.MappingSorters.FirstOrDefaultAsync(x => x.DivertLaneId == existingDivertLane.Id);
            if (existingSorter == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = $"This DivertLane value does not exists in MappingSorter."
                };

            }

            string carrierCodeId = existingSorter.CarrierCodeId;
            var carrierCodes = !string.IsNullOrEmpty(carrierCodeId)
                ? carrierCodeId.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(code => code.Trim())
                            .ToArray()
                : new string[0];

            var existingCarrier = await Context.CarrierCodes.FirstOrDefaultAsync(x => carrierCodes.Contains(x.Id.ToString()));
            if (existingCarrier == null)
            {
                return new PrintLabelRespDto
                {
                    Value = false,
                    Message = "One or more CarrierCode values do not exist."
                };
            }


            var existingDivertLane2 = await Context.DivertLaneZebraConfigurationMappings.FirstOrDefaultAsync(x => x.DivertLaneId == existingDivertLane.Id);
            if (existingDivertLane2 == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = $"This divertlane is not mapped to any Zebra configuration."
                };
            }

            var existingLogisticAgent = await Context.LogisticAgents.FirstOrDefaultAsync(x => x.Id == existingSorter.LogisticAgentId);
            if (existingLogisticAgent == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "The chosen DivertLane does not have a registered Logistic Agent."
                };
            }


            var existingPrint = await Context.ZebraConfigurations.FirstOrDefaultAsync(x => x.Id == existingDivertLane2.ZebraConfigurationId);
            if (existingPrint == null)
            {
                return response = new PrintLabelRespDto
                {
                    Value = false,
                    Message = "This Zebra Configuration does not exists."
                };
            }

            //-----------------------------------------------------------------------------------------------------


            var confirmationNumbers = await Context.multiBox_Wave
    .Where(x =>
        x.ContainerId == existingContainer.ContainerId &&
        x.DivertLane == existingDivertLane.DivertLanes &&
        !string.IsNullOrEmpty(x.ConfirmationNumber) &&
        x.Status != null && x.QtyCount >= 1).Select(x => x.ConfirmationNumber)
    .ToListAsync();

            using (SqlConnection connection = new SqlConnection(Configuration.GetConnectionString("SAP_connection")))
            {
                await connection.OpenAsync();

                foreach (var confirmationNumber in confirmationNumbers)
                {
                    var query = $"SELECT BoxId FROM [dbo].[SAP_Orders] where ConfirmationNumber = '{confirmationNumber}'";

                    

                    var tContainerBoxIds = await Context.wCSRoutingV10s
                        .Where(x =>
                            x.ContainerId == existingContainer.ContainerId &&
                            x.DivertLane == existingDivertLane.DivertLanes &&
                            x.ConfirmationNumber == confirmationNumber &&
                            x.ContainerType == "T" &&
                            x.Status == "NA")
                        .Select(x => x.BoxId)
                        .ToListAsync();

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        var reader = await command.ExecuteReaderAsync();

                        var nonMatchingBoxIds = new List<string>();

                        while (await reader.ReadAsync())
                        {
                            string boxId = reader["BoxId"] as string;
                            boxId = boxId.Trim();

                            if (boxId != null && !tContainerBoxIds.Contains(boxId))
                            {
                                nonMatchingBoxIds.Add(boxId);
                            }
                        }

                        var pContainerBoxIds = new List<string>(nonMatchingBoxIds);

                        var printLabelMultiBoxDto = new PrintLabelMultiBoxDto
                        {
                            ConfirmationNumber = confirmationNumber,
                            BoxesNotDeliveredDueToClosure = pContainerBoxIds,
                            BoxesProperlyDelivered = tContainerBoxIds
                        };

                        var label = zplLabelMultibox(printLabelMultiBoxDto);

                        bool success;

                        if (pContainerBoxIds.Any())
                        {
                            if (!string.IsNullOrEmpty(existingPrint.HostName))
                            {
                                success = SendZPLToPrinter(label, existingPrint.HostName, existingPrint.Port, existingPrint.PortType);
                            }
                            else
                            {
                                success = SendZPLToPrinter(label, existingPrint.Ip, existingPrint.Port, existingPrint.PortType);
                            }

                            if (!success)
                            {
                                return new PrintLabelRespDto
                                {
                                    Value = false,
                                    Message = "La conexión con la impresora Zebra ha fallado."
                                };
                            }
                        }
                        tContainerBoxIds.Clear();
                        reader.Close();
                    }
                }

                connection.Close();
            }

            return new PrintLabelRespDto
            {
                Value = true,
                Message = "Correcto"
            };
        }
    }
}


