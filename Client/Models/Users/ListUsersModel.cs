using BusinessObjects.Common;
using BusinessObjects.DTOs;

namespace Client.Models.Users
{
    public class ListUsersModel
    {
        public FilterUserDTO Filter { get; set; } = new();
        public PaginateResult<UserDto> Users { get; set; }

        public bool HasFilter()
        {
            return
                !string.IsNullOrWhiteSpace(Filter.Keyword)  
                || Filter.Status.HasValue
                || Filter.RoleIds.Count > 0
                || Filter.CreatedFrom.HasValue
                || Filter.CreatedTo.HasValue;
        }
    }
}
