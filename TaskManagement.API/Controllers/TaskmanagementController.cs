
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Query;

namespace TaskManagement.API.Controllers
{
   
    [ApiController]
    public class TaskmanagementController : ControllerBase
    {
        private readonly IMediator _mediator;   

        public TaskmanagementController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpGet]
        [Route("userId")]
        public async Task<IActionResult> GetAssignedTasks([FromQuery] int userId)
        {
            var result = await _mediator.Send(new GetAssignedTasks.Query(userId));
            return Ok(result);
        }


        //[HttpGet("assigned/{userId}/pending")]
        //public async Task<IActionResult> GetPendingTasks(int userId)
        //{
        //    var tasks = await _taskRepository.GetPendingTasks(userId);
        //    return Ok(tasks);
        //}

        //[HttpGet("assigned/{userId}/completed")]
        //public async Task<IActionResult> GetCompletedTasks(int userId)
        //{
        //    var tasks = await _taskRepository.GetCompletedTasks(userId);
        //    return Ok(tasks);
        //}

        //[HttpGet("assigned/{userId}/high-priority")]
        //public async Task<IActionResult> GetHighPriorityTasks(int userId)
        //{
        //    var tasks = await _taskRepository.GetHighPriorityTasks(userId);
        //    return Ok(tasks);
        //}

        //[HttpGet("assigned-by/{userId}")]
        //public async Task<IActionResult> GetTasksAssignedByUser(int userId)
        //{
        //    var tasks = await _taskRepository.GetTasksAssignedByUser(userId);
        //    return Ok(tasks);
        //}

        //[HttpGet("assigned/{userId}/count")]
        //public async Task<IActionResult> GetTotalAssignedTasks(int userId)
        //{
        //    var count = await _taskRepository.GetTotalAssignedTasks(userId);
        //    return Ok(new { TotalTasks = count });
        //}

        //[HttpGet("assigned/{userId}/last-7-days")]
        //public async Task<IActionResult> GetTasksAssignedLast7Days(int userId)
        //{
        //    var tasks = await _taskRepository.GetTasksAssignedLast7Days(userId);
        //    return Ok(tasks);
        //}

        //[HttpGet("assigned/{userId}/overdue")]
        //public async Task<IActionResult> GetOverdueTasks(int userId)
        //{
        //    var tasks = await _taskRepository.GetOverdueTasks(userId);
        //    return Ok(tasks);
        //}

        //[HttpGet("assigned/{userId}/today")]
        //public async Task<IActionResult> GetTasksAssignedToday(int userId)
        //{
        //    var tasks = await _taskRepository.GetTasksAssignedToday(userId);
        //    return Ok(tasks);
        //}

        //[HttpGet("assigned/{userId}/tags")]
        //public async Task<IActionResult> GetTasksWithTags(int userId)
        //{
        //    var tasks = await _taskRepository.GetTasksWithTags(userId);
        //    return Ok(tasks);
        //}
    }
}
