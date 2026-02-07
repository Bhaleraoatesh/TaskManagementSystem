using System;
using System.Collections.Generic;

namespace TaskManagement.Domain.Entities
{
    /// <summary>
    /// User entity with authentication support
    /// Maps to Users table
    /// </summary>
    public class User
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FullName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastLoginDate { get; set; }
        public DateTime? LastPasswordChangeDate { get; set; }
        
        // For aggregated data from queries
        public List<Role> Roles { get; set; } = new List<Role>();
    }
    
    /// <summary>
    /// Role entity
    /// Maps to Roles table
    /// </summary>
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
    
    /// <summary>
    /// User-Role relationship
    /// Maps to UserRoles table
    /// </summary>
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}