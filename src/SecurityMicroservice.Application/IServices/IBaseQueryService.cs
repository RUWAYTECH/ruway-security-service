using SecurityMicroservice.Shared.Response.Common;

namespace SecurityMicroservice.Application.IServices
{

    public interface IBaseQueryService<TResponse>
    {       
         public Task<ResponseDto<IEnumerable<TResponse>>> GetAsync();
    }
}
