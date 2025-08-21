using HuloToys_Service.Models.Models;
using System;
using System.Collections.Generic;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace ENTITIES.Models
{
    public partial class Role
    {
        public Role()
        {
            RolePermission = new HashSet<RolePermission>();
            UserRole = new HashSet<UserRole>();
        }


        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? SortOrder { get; set; }
        public int Status { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? ModifiedOn { get; set; }

        public virtual ICollection<RolePermission> RolePermission { get; set; }
        public virtual ICollection<UserRole> UserRole { get; set; }
    }
    public partial class RolePermission
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public int MenuId { get; set; }
        public int PermissionId { get; set; }

        public virtual Menu Menu { get; set; }
        public virtual Permission Permission { get; set; }
        public virtual Role Role { get; set; }
    }
}
