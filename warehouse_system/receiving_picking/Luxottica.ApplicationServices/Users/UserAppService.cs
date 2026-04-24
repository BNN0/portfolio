using AutoMapper;
using Luxottica.ApplicationServices.Shared.Dto.Users;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Luxottica.DataAccess;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Luxottica.ApplicationServices.PhysicalMaps;

namespace Luxottica.ApplicationServices.Users
{
    public class UserAppService : IUserAppService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly V10Context _context;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<UserAppService> _logger;
        public UserAppService(UserManager<IdentityUser> userManager, V10Context context, IMapper mapper, RoleManager<IdentityRole> roleManager, ILogger<UserAppService> logger)
        {
            _userManager = userManager;
            _context = context;
            _mapper = mapper;
            _roleManager = roleManager;
            _logger = logger;
        }

        public async Task<List<UserDto>> GetUsersAsync()
        {
            try
            {
                List<IdentityUser> users = await _userManager.Users.ToListAsync();

                List<UserDto> usersDto = new List<UserDto>();

                foreach (var user in users)
                {
                    var roleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault(); 

                    var userDto = new UserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        PhoneNumber = user.PhoneNumber,
                        Email = user.Email,
                        Role = roleName
                    };
                    usersDto.Add(userDto);
                }
                return usersDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT FROM USERS IN GetUsersAsync SERVICE {ex.Message}");
                throw new Exception($"GetUsersAsync unsuccessful. Error: {ex.Message}");
            }

            
        }

        public async Task<UserDto> GetUserAsync(string id)
        {
            try
            {
                IdentityUser user = await _userManager.Users.Where(x => x.Id == id).FirstOrDefaultAsync();
                var roleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                //UserDto userDto = _mapper.Map<UserDto>(user);
                var userDto = new UserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    PhoneNumber = user.PhoneNumber,
                    Email = user.Email,
                    Role = roleName
                };

                return userDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR SELECT FROM USER WHERE Id = {id} IN GetUserAsync SERVICE {ex.Message}");
                throw new Exception($"GetUserAsync unsuccessful. Error: {ex.Message}");
            }
        }

        public async Task AddUserAsync(NewUserDto userDto)
        {
            try
            {
                var existingUserByUsername = await _userManager.FindByNameAsync(userDto.UserName);

                if (existingUserByUsername != null)
                {
                    _logger.LogError($"ERROR Insert USER IN AddUserAsync SERVICE, Message: Username is already exist!.");
                    throw new InvalidOperationException("Username is already exist!.");
                }

                var existingUserByEmail = await _userManager.FindByEmailAsync(userDto.Email);
                if (existingUserByEmail != null)
                {
                    _logger.LogError($"ERROR Insert USER IN AddUserAsync SERVICE, Message: Email already exists!.");
                    throw new InvalidOperationException("Email already exists!.");
                }

                if (!userDto.Email.Contains("@") || !userDto.Email.Contains(".com"))
                {
                    _logger.LogError($"ERROR Insert USER IN AddUserAsync SERVICE, Message: Email is invalid!. correct are: user@example.com");
                    throw new InvalidOperationException("Email is invalid!. correct are: user@example.com ");
                }

                if (userDto.PhoneNumber.Length != 10 || !userDto.PhoneNumber.All(char.IsDigit))
                {
                    _logger.LogError($"ERROR Insert USER IN AddUserAsync SERVICE, Message: Phone Number is invalid!. The phone number must be in 10 digit format.");
                    throw new InvalidOperationException("Phone Number is invalid!. The phone number must be in 10 digit format.");
                }

                if (!IsValidPassword(userDto.Password))
                {
                    _logger.LogError($"ERROR Insert USER IN AddUserAsync SERVICE, Message: Password is invalid!. The password minimum length 7, must contain letters, numbers, and special character");
                    throw new InvalidOperationException("Password is invalid!. The password minimum length 7, must contain letters, numbers, and special character");
                }

                var u = _mapper.Map<IdentityUser>(userDto);
                var result = await _userManager.CreateAsync(u, userDto.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(u, userDto.RoleNameAssignment);
                }
            }
            catch(Exception ex)
            {
                _logger.LogError($"ERROR Insert USER IN AddUserAsync SERVICE {ex.Message}");
                throw new Exception($"AddUserAsync unsuccessful. Error: {ex.Message}");
            }
        }

        public async Task EditUserAsync(string id, EditUserDto userDto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);
                var roleName = (await _userManager.GetRolesAsync(user)).FirstOrDefault();
                if (user == null)
                {
                    _logger.LogError($"ERROR Update USER where Id = {id} IN EditUserAsync SERVICE, Message: User not found!..");
                    throw new InvalidOperationException("User not found!..");
                }

                if (userDto.UserName != user.UserName)
                {
                    var existingUserByUsername = await _userManager.FindByNameAsync(userDto.UserName);

                    if (existingUserByUsername != null)
                    {
                        _logger.LogError($"ERROR Update USER where Id = {id} IN EditUserAsync SERVICE, Message: Username is already exist!.");
                        throw new InvalidOperationException("Username is already exist!.");
                    }
                }

                if (userDto.PhoneNumber.Length != 10 || !userDto.PhoneNumber.All(char.IsDigit))
                {
                    _logger.LogError($"ERROR Update USER where Id = {id} IN EditUserAsync SERVICE, Message: Phone Number is invalid!. The phone number must be in 10 digit format.");
                    throw new InvalidOperationException("Phone Number is invalid!. The phone number must be in 10 digit format.");
                }

                user.PhoneNumber = userDto.PhoneNumber;
                user.UserName = userDto.UserName;

                string updateRole = userDto.RoleNameAssignment;
                
                if (updateRole != roleName)
                {
                    bool searchRoleName = await _roleManager.RoleExistsAsync(updateRole);
                    if (!searchRoleName)
                    {
                        _logger.LogError($"ERROR Update USER where Id = {id} IN EditUserAsync SERVICE, Message: Role '{updateRole}' does not exist.");
                        throw new Exception($"Role '{updateRole}' does not exist.");
                    }

                    var userRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, userRoles);

                    await _userManager.AddToRoleAsync(user, updateRole);
                }

                UserStore<IdentityUser> store = new UserStore<IdentityUser>(_context);
                if (store == null)
                {
                    _logger.LogError($"ERROR Update USER where Id = {id} IN EditUserAsync SERVICE, Message: The entity was not saved, the fileds are required");
                    throw new InvalidOperationException("The entity was not saved, the fileds are required");
                }
                await store.UpdateAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR Update USER where Id = {id} IN EditUserAsync SERVICE {ex.Message}");
                throw;
            }
            
        }

        public async Task DeleteUserAsync(string id)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(id);

                UserStore<IdentityUser> store = new UserStore<IdentityUser>(_context);
                await store.DeleteAsync(user);
            }
            catch(Exception ex)
            {
                _logger.LogError($"ERROR Delete USER where Id = {id} IN DeleteUserAsync SERVICE {ex.Message}");
                throw new Exception($"DeleteUserAsync unsuccessful. Error: {ex.Message}");
            }
        }

        public async Task<List<RolesNameDto>> GetRolesAsync()
        {
            try
            {
                var r = await _roleManager.Roles.ToListAsync();
                List<RolesNameDto> rolesDto = _mapper.Map<List<RolesNameDto>>(r);
                return rolesDto;
            }
            catch (Exception ex)
            {
                _logger.LogError($"ERROR GetRolesAsync IN UserAppService SERVICE {ex.Message}");
                throw;
            }
        }

        private static bool IsValidPassword(string password)
        {
            string pattern = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{7,}$";
            Regex regex = new Regex(pattern);

            return regex.IsMatch(password);
        }
    }
}
