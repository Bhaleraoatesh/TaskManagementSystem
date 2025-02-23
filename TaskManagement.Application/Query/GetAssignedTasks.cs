using MediatR;
using TaskManagement.Domain.Entity;
using TaskManagement.Domain.Repository;
namespace TaskManagement.Application.Query
{
    public sealed class GetAssignedTasks
    {
        // Query returning a single string
        public sealed record Query(int UserId) : IRequest<List<GetAssignedTaskByUserId>>;

        public class Handler : IRequestHandler<Query, List<GetAssignedTaskByUserId>>
        {
            private readonly IRepository _repository;   
            public Handler(IRepository repository)
            {
                _repository = repository;
            }   
            public async Task<List<GetAssignedTaskByUserId>> Handle(Query request, CancellationToken cancellationToken)
            {
                var result = await _repository.GetAssignedTasks(request.UserId);   
                return result;
            }
        }
    }
}
