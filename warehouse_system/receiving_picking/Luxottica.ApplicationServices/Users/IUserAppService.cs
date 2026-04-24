using Luxottica.ApplicationServices.Shared.Dto.Users;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luxottica.ApplicationServices.Users
{
    public interface IUserAppService
    {
        Task<List<UserDto>> GetUsersAsync();
        Task<UserDto> GetUserAsync(string id);
        Task AddUserAsync(NewUserDto userDto);
        Task EditUserAsync(string id, EditUserDto userDto);
        Task DeleteUserAsync(string id);
        Task<List<RolesNameDto>> GetRolesAsync();
    }
}
