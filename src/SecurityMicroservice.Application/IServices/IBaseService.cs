using SecurityMicroservice.Shared.Response.Common;

namespace SecurityMicroservice.Application.IServices
{
    public interface IBaseService<TRequest, TResponse>
    {
        [Obsolete]
        public Task<ResponseDto<IEnumerable<TResponse>>> Get(object filter)  => Task.FromResult(new ResponseDto<IEnumerable<TResponse>>());
        public Task<ResponseDto<TResponse>> GetById(object id);
        public Task<ResponseDto<TResponse>> Update(object id, TRequest requestDto) => Task.FromResult(new ResponseDto<TResponse>());
        public Task<ResponseDto<TResponse>> Create(TRequest requestDto) => Task.FromResult(new ResponseDto<TResponse>());
        public Task<ResponseDto> Delete(object id);
    }
}
