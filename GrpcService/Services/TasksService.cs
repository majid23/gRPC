using Grpc.Core;
using GrpcService.Models;
using Microsoft.EntityFrameworkCore;
using TaskService;

namespace GrpcService.Services
{
    public class TasksService : TaskService.Task.TaskBase
    {
        private readonly DataContext _context;
        public TasksService(DataContext context)
        {
            _context= context;
        }

        public override async Task<CreateTaskResponse> CreateTask(CreateTaskRequest request, ServerCallContext context)
        {
            if(request.Name== string.Empty || request.Description == string.Empty) {
                throw new RpcException(new Status(statusCode: StatusCode.InvalidArgument, "Task name or description is empty."));
            }

            var task = new Models.Task()
            {
                Name = request.Name,
                Description = request.Description,
                Priority = request.Priority
            };

            await _context.Tasks.AddAsync(task);
            await _context.SaveChangesAsync();

            return await System.Threading.Tasks.Task.FromResult(new CreateTaskResponse()
            {
                Id = task.Id
            });
        }

        public override async Task<ReadTaskResponse> ReadTask(ReadTaskRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(new Status(statusCode: StatusCode.InvalidArgument, "Invalid Argument."));
            }

            var t = await _context.Tasks.Where(a => a.Id == request.Id).FirstOrDefaultAsync();

            if (t != null)
            {
                return await System.Threading.Tasks.Task.FromResult(new ReadTaskResponse()
                {
                    Id = t.Id,
                    Description = t.Description,
                    Priority = t.Priority,
                    Name = t.Name,
                    Status = t.Status,
                });
            }

            throw new RpcException(new Status(statusCode: StatusCode.NotFound, $"Task {request.Id} does not exist."));
        }

        public override async Task<ReadAllTaskResponse> ReadAllTask(ReadAllTaskRequest request, ServerCallContext context)
        {
            var r = new ReadAllTaskResponse();
            var tasks = await _context.Tasks.ToListAsync();
            foreach ( var task in tasks )
            {
                r.Task.Add(new ReadTaskResponse()
                {
                    Id = task.Id,
                    Description = task.Description,
                    Priority = task.Priority,
                    Name = task.Name,
                    Status= task.Status,
                });
            }

            return await System.Threading.Tasks.Task.FromResult(r);
        }

        public override async Task<UpdateTaskResponse> UpdateTask(UpdateTaskRequest request, ServerCallContext context)
        {
            if (request.Id <= 0 || request.Name == string.Empty || request.Description == string.Empty)
            {
                throw new RpcException(new Status(statusCode: StatusCode.InvalidArgument, "Invalid Arguments."));
            }

            var t = await _context.Tasks.Where(a => a.Id == request.Id).FirstOrDefaultAsync();

            if (t == null)
            {
                throw new RpcException(new Status(statusCode: StatusCode.NotFound, $"Task {request.Id} does not exist."));
            }
            t.Status = request.Status;
            t.Description = request.Description;
            t.Priority = request.Priority;
            t.Name = request.Name;

            await _context.SaveChangesAsync();

            return await System.Threading.Tasks.Task.FromResult(new UpdateTaskResponse() { Id = t.Id });
        }

        public override async Task<DeleteTaskResponse> DeleteTask(DeleteTaskRequest request, ServerCallContext context)
        {
            if (request.Id <= 0)
            {
                throw new RpcException(new Status(statusCode: StatusCode.InvalidArgument, "This task does not exist."));
            }
            var t = await _context.Tasks.Where(a => a.Id == request.Id).FirstOrDefaultAsync();

            if (t == null)
            {
                throw new RpcException(new Status(statusCode: StatusCode.NotFound, $"Task {request.Id} does not exist."));
            }

            _context.Tasks.Remove(t);
            await _context.SaveChangesAsync();

            return await System.Threading.Tasks.Task.FromResult(new DeleteTaskResponse() { Id= t.Id});
        }
    }
}
