using Grpc.Net.Client;
using GrpcClient;
using TaskClient;

using System;
using Grpc.Core;

namespace GrpcClient
{
    static class Program
    {
        static async System.Threading.Tasks.Task Main(string[] args)
        {
            // The port number must match the port of the gRPC server.
            using var channel = GrpcChannel.ForAddress("https://localhost:7277");
            var greeterClient = new Greeter.GreeterClient(channel);
            var reply = await greeterClient.SayHelloAsync(
                              new HelloRequest { Name = "majid" });

            Console.WriteLine("Greeting: " + reply.Message);

            //channel
            var taskClient = new TaskClient.Task.TaskClient(channel);

            //create task
            Console.WriteLine("Create Task");
            var createResponse = await taskClient.CreateTaskAsync(new CreateTaskRequest() { Name = "Backend Task", Description = "Create gRPC api...", Priority = 500 });
            var create2Response = await taskClient.CreateTaskAsync(new CreateTaskRequest() { Name = "Frontend Task", Description = "Create UI...", Priority = 600 });
            Console.WriteLine($"Task Id : {createResponse.Id} created.\n");
            Console.WriteLine($"Task Id : {create2Response.Id} created.\n");

            //read task
            Console.WriteLine("Read Task");
            var readResponse = await taskClient.ReadTaskAsync(new ReadTaskRequest() { Id = createResponse.Id });
            PrintTask(readResponse);
            Console.WriteLine("\n");

            //read all task
            Console.WriteLine("Read All Tasks");
            var readAllResponse = await taskClient.ReadAllTaskAsync(new ReadAllTaskRequest());
            foreach (var task in readAllResponse.Task)
            {
                PrintTask(task);
            }
            Console.WriteLine("\n");

            //update task
            Console.WriteLine("Update Task");
            var updateResponse = await taskClient.UpdateTaskAsync(new UpdateTaskRequest() { 
                Id = readResponse.Id, 
                Name = readResponse.Name,
                Description = readResponse.Description,
                Priority = readResponse.Priority + 1,
                Status = "In Progress"
            });
            var read2Response = await taskClient.ReadTaskAsync(new ReadTaskRequest() { Id = createResponse.Id });
            PrintTask(read2Response);
            Console.WriteLine("\n");

            //delete task
            Console.WriteLine("Delete Task");
            var deleteResponse = await taskClient.DeleteTaskAsync(new DeleteTaskRequest() { Id = createResponse.Id });
            Console.WriteLine($"Task {createResponse.Id} Deleted.\n");

            try
            {
                readResponse = await taskClient.ReadTaskAsync(new ReadTaskRequest() { Id = createResponse.Id });
                PrintTask(readResponse);
            }
            catch (RpcException ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("\n\nPress any key to exit...");
            Console.ReadKey();
        }

        static void PrintTask(ReadTaskResponse task)
        {
            var status = string.IsNullOrEmpty(task.Status) ? "" : $", {task.Status}";
            Console.WriteLine(@$"Task {task.Id} details => name: {task.Name}, description: {task.Description}, priority: {task.Priority}{status}");
        }
    }
}