using LuxotticaSorting.ApplicationServices.Shared.DTO.NewBoxs;

namespace LuxotticaSorting.ApplicationServices.NewBoxs
{
    public interface INewBoxAppService
    {
        Task AddNewBoxAsync(NewBoxAddDto NewBox);
    }
}
