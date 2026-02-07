using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TaskManagement.Domain.Entity
{
    public class GetAssignedTaskByUserId
    {
        public int TaskId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public string? StatusName { get; set; }
        public string? PriorityName { get; set; }
        public int AssignedBy { get; set; }
        public string? AssignedByFirstName { get; set; }
        public string? AssignedByLastName { get; set; }
        public DateTime DateAssigned { get; set; }
        public DateTime? DateCompleted { get; set; }
    }
}
