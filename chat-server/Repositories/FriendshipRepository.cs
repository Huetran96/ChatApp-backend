using chat_server.data;
using chat_server.DTOs;
using chat_server.Models;
using Microsoft.EntityFrameworkCore;

namespace chat_server.Repositories
{
    public class FriendshipRepository : IFriendshipRepository
    {
        private readonly AppDbContext _context;

        // Constructor: Khởi tạo FriendshipRepository với AppDbContext để tương tác với cơ sở dữ liệu
        public FriendshipRepository(AppDbContext context)
        {
            _context = context; // Gán context vào biến thành viên để sử dụng trong các phương thức
        }

        // Thêm một yêu cầu kết bạn vào cơ sở dữ liệu
        public async Task<Friendship> AddFriend(Friendship friendship)
        {
            if (friendship == null)
            {
                throw new ArgumentNullException(nameof(friendship), "Đối tượng Friendship không được phép là null.");
            }

            // Kiểm tra tính hợp lệ của các trường
            if (string.IsNullOrEmpty(friendship.RequestedId) || string.IsNullOrEmpty(friendship.AcceptedId))
            {
                throw new Exception("RequestedId và AcceptedId không được phép rỗng.");
            }


            try
            {
                await _context.Friendships.AddAsync(friendship); // Thêm đối tượng Friendship vào DbSet Friendships
                await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
                return friendship; // Trả về đối tượng Friendship đã thêm
            }
            catch (DbUpdateException dbEx)
            {
                // Lấy thông tin chi tiết từ inner exception
                var innerExceptionMessage = dbEx.InnerException != null ? dbEx.InnerException.Message : "Không có thông tin chi tiết.";
                throw new Exception($"Đã xảy ra lỗi khi thêm mối quan hệ bạn bè: {innerExceptionMessage}", dbEx);
            }
            catch (Exception ex)
            {
                // Xử lý các lỗi khác
                throw new Exception("Thêm bạn thất bại: " + ex.Message, ex);
            }
        }


        // Lấy danh sách bạn bè của người dùng theo userId
        public async Task<List<Friendship>> GetFriends(string userId)
        {
            return await _context.Friendships
                .Where(f => (f.RequestedId == userId || f.AcceptedId == userId) && f.Status == "Accepted") // Lọc danh sách bạn bè có trạng thái "Accepted"
                .ToListAsync(); // Chuyển đổi kết quả thành danh sách
        }

        // Chấp nhận một yêu cầu kết bạn
        public async Task<Friendship> AcceptFriend(string requestedId, string acceptedId)
        {
            // Tìm kiếm yêu cầu kết bạn dựa trên requestedId và acceptedId
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.RequestedId == requestedId && f.AcceptedId == acceptedId);

            if (friendship == null) return null; // Trả về null nếu không tìm thấy yêu cầu

            // Cập nhật trạng thái yêu cầu thành "Accepted"
            friendship.Status = "Accepted";
            friendship.UpdatedAt = DateTime.Now; // Cập nhật thời gian sửa đổi
            await _context.SaveChangesAsync(); // Lưu thay đổi vào cơ sở dữ liệu
            return friendship; // Trả về đối tượng Friendship đã được chấp nhận
        }

        // Chặn một người dùng
        public async Task<Friendship> BlockUser(Friendship friendship, bool isNew)
        {
            try
            {
             
                if (isNew)
                {
                    // Nếu đây là một mối quan hệ mới, thêm nó vào ngữ cảnh
                     _context.Friendships.Add(friendship);
                }
                else
                {
                    // Nếu mối quan hệ đã tồn tại, cập nhật nó
                    _context.Friendships.Update(friendship);
                }

                // Lưu các thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                return friendship; // Trả về mối quan hệ đã lưu
            }
            catch (DbUpdateException dbEx)
            {
               
                throw new Exception("Đã xảy ra lỗi khi lưu các thay đổi. Vui lòng kiểm tra dữ liệu đầu vào hoặc các ràng buộc.", dbEx);
            }
            catch (Exception ex)
            {
           
                throw new Exception("Chặn người dùng thất bại do lỗi không mong muốn.", ex);
            }
        }

        public async Task<Friendship> RemoveFriend(Friendship friendship, bool isNew)
        {
            try
            {

                if (isNew)
                {
                    // Nếu đây là một mối quan hệ mới, thêm nó vào ngữ cảnh
                    _context.Friendships.Add(friendship);
                }
                else
                {
                    // Nếu mối quan hệ đã tồn tại, cập nhật nó
                    _context.Friendships.Update(friendship);
                }

                // Lưu các thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                return friendship; // Trả về mối quan hệ đã lưu
            }
            catch (DbUpdateException dbEx)
            {

                throw new Exception("Đã xảy ra lỗi khi lưu các thay đổi. Vui lòng kiểm tra dữ liệu đầu vào hoặc các ràng buộc.", dbEx);
            }
            catch (Exception ex)
            {

                throw new Exception("Xóa người dùng thất bại do lỗi không mong muốn.", ex);
            }
        }


        // Lấy danh sách người dùng bị chặn
        public async Task<List<Friendship>> GetBlockedUsers(string userId)
        {
            return await _context.Friendships
                .Where(f => (f.RequestedId == userId ) && f.Status == "Blocked") // Lọc danh sách người dùng bị chặn
                .ToListAsync(); // Chuyển đổi kết quả thành danh sách
        }

        // Lấy danh sách yêu cầu kết bạn chưa được chấp nhận
        public async Task<List<Friendship>> GetFriendRequests(string userId)
        {
            return await _context.Friendships
                .Where(f => f.RequestedId == userId && f.Status == "Pending") // Lọc danh sách yêu cầu kết bạn có trạng thái "Pending"
                .ToListAsync(); // Chuyển đổi kết quả thành danh sách
        }
        public async Task<List<Friendship>> GetFriendReceives(string userId)
        {
            return await _context.Friendships
                .Where(f => f.AcceptedId == userId && f.Status == "Pending") // Lọc danh sách yêu cầu kết bạn có trạng thái "Pending"
                .ToListAsync(); // Chuyển đổi kết quả thành danh sách
        }

        public async Task<Friendship> GetFriendship(string requestedId, string acceptedId)
        {
            // Tìm yêu cầu kết bạn dựa trên RequestedId và AcceptedId
            return await _context.Friendships
                .FirstOrDefaultAsync(f => f.RequestedId == requestedId && f.AcceptedId == acceptedId);
        }

        public async Task UpdateFriendship(Friendship friendship)
        {
            _context.Friendships.Update(friendship); // Cập nhật đối tượng Friendship trong DbSet
            await _context.SaveChangesAsync();       // Lưu thay đổi vào cơ sở dữ liệu
        }

        public async Task<List<User>> SearchUsersByNameOrPhone(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<User>(); // Trả về danh sách rỗng nếu searchTerm trống hoặc chỉ có khoảng trắng
            }

            searchTerm = searchTerm.Trim().ToLower(); // Loại bỏ khoảng trắng và chuyển về chữ thường

            return await _context.Users
                .Where(user =>
                    user.UserName.ToLower().Contains(searchTerm) ||
                    user.PhoneNumber.Contains(searchTerm)) // Case-insensitive tìm kiếm cho UserName
                .Take(10) // Giới hạn kết quả trả về 10 người dùng đầu tiên
                .ToListAsync();
        }

    }



}
