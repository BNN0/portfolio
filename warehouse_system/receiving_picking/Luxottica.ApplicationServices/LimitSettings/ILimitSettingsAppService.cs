using Luxottica.ApplicationServices.Shared.Dto.Camera;
using Luxottica.ApplicationServices.Shared.Dto.LimitSettings;
using Luxottica.Core.Entities.LimitsSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.LimitSettings
{
    public interface ILimitSettingsAppService
    {
        Task<List<LimitSettingDTO>> GetLimitSettingsAsync();

        Task<int> AddLimitSettingsAsync(LimitSettingDTO limitSetting);

        Task DeleteLimitSettingAsync(int limitSettingId);

        Task<LimitSettingDTO> GetLimitSettingByIdAsync(int limitSettingId);

        Task EditLimitSettingAsync(LimitSettingDTO limitSetting);

        Task<LimitSettingDTO> GetLimitSettingInCam10();
    }
}
