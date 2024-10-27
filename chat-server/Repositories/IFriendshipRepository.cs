using chat_server.Models;

namespace chat_server.Repositories
{
    public interface IFriendshipRepository
    {
        // Phương thức để thêm một yêu cầu kết bạn vào cơ sở dữ liệu
        Task<Friendship> AddFriend(Friendship friendship);

        // Phương thức để lấy danh sách bạn bè của người dùng dựa trên userId
        Task<List<Friendship>> GetFriends(string userId);

        // Phương thức để chấp nhận yêu cầu kết bạn dựa trên requestedId và acceptedId
        Task<Friendship> AcceptFriend(string requestedId, string acceptedId);

        // Phương thức để chặn một người dùng dựa trên requestedId và acceptedId
        Task<Friendship> BlockUser(Friendship friendship,bool isNew);

        Task<Friendship> RemoveFriend(Friendship friendship, bool isNew); // Xóa mối quan hệ kết bạn

        // Phương thức để lấy danh sách người dùng bị chặn dựa trên userId
        Task<List<Friendship>> GetBlockedUsers(string userId);

        // Phương thức để lấy danh sách yêu cầu kết bạn chưa được chấp nhận dựa trên userId
        Task<List<Friendship>> GetFriendRequests(string userId);
        Task<List<Friendship>> GetFriendReceives(string userId);

        Task<Friendship> GetFriendship(string requestedId, string acceptedId); // Lấy yêu cầu kết bạn dựa vào ID
        Task UpdateFriendship(Friendship friendship); // Cập nhật yêu cầu kết bạn

        Task<List<User>> SearchUsersByNameOrPhone(string searchTerm);
    }

}
