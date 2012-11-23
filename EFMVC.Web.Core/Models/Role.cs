using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EFMVC.Web.Core.Models
{
    public class Roles
    {
        public const string Admin = "Admin";
        public const string Manager = "Manager";
        public const string User = "User";  
    }
    public enum UserRoles
    {
        Admin = 1,
        Manager = 2,
        User = 3       
    }
}
