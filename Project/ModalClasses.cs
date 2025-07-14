using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project
{

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public int Id { get; set; }
    }
    public class RequestModel
    {
        public string request { get; set; }
    }
    public class MenuModel
    {
        public int MenuId { get; set; }
        public int? ParentMenuId { get; set; }
        public int? PageId { get; set; }
        public string PageUrl { get; set; }
        public string MenuTitle { get; set; }
        public int MenuNo { get; set; }
        public string MenuIcon { get; set; }
        public List<MenuModel> MenuList { get; set; }
    }
}
