using System.Collections.Generic;

namespace MedAdvice.viewmodel
{
    public class UserListItemViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
        public bool IsDeactivated { get; set; }
        public bool IsTemporarilyLockedOut { get; set; }
    }

    public class UserListViewModel
    {
        public string Query { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public List<string> AvailableRoles { get; set; } = new List<string>();
        public List<UserListItemViewModel> Users { get; set; } = new List<UserListItemViewModel>();
    }
}
