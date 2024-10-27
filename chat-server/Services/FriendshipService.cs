using chat_server.DTOs;
using chat_server.Models;
using chat_server.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NuGet.Protocol.Core.Types;


namespace chat_server.Services
{
    public class FriendshipService : IFriendshipService
    {
        private readonly IFriendshipRepository _repository;
        private readonly UserManager<User> _userManager;

        // Constructor: Khởi tạo FriendshipService với repository để tương tác với cơ sở dữ liệu
        public FriendshipService(IFriendshipRepository repository, UserManager<User> userManager)
        {
            _repository = repository; // Gán repository vào biến thành viên để sử dụng trong các phương thức
            _userManager = userManager; // gán userManager
        }

        // Tìm user lấy số điện thoại
        public async Task<User> GetUserByPhoneNumber(string phoneNumber)
        {
            // Tìm kiếm người dùng theo số điện thoại
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);

            if (user == null)
            {
                // Nếu không tìm thấy người dùng với số điện thoại này, ném ra ngoại lệ
                throw new Exception("Không tìm thấy người dùng với số điện thoại này");
            }

            return user; // Trả về đối tượng User đã tìm thấy
        }

        // Thêm một yêu cầu kết bạn mới
        public async Task<FriendshipResponseDto> AddFriend(string requestedId, FriendshipCreateDto dto)
        {
            try
            {
                // Kiểm tra nếu RequestedId hợp lệ (người gửi yêu cầu)
                if (string.IsNullOrEmpty(requestedId))
                {
                    throw new Exception("RequestedId không được phép rỗng hoặc null.");
                }

                // Kiểm tra tính hợp lệ của DTO
                if (dto == null || string.IsNullOrEmpty(dto.PhoneNumber))
                {
                    throw new Exception("Thông tin kết bạn không hợp lệ.");
                }

                // Tìm người dùng bằng số điện thoại
                var acceptedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == dto.PhoneNumber);


                // Kiểm tra nếu người dùng có số điện thoại này tồn tại
                if (acceptedUser == null)
                {
                    throw new Exception("Không tìm thấy người dùng với số điện thoại này.");
                }

                // Kiểm tra nếu người dùng tự gửi yêu cầu cho chính mình
                if (requestedId == acceptedUser.Id)
                {
                    throw new Exception("Bạn không thể gửi yêu cầu kết bạn cho chính mình.");
                }

                // Kiểm tra mối quan hệ đã tồn tại chưa (cả bị chặn hoặc đang pending)
                var existingFriendship = await _repository.GetFriendship(requestedId, acceptedUser.Id);
                if (existingFriendship != null)
                {
                    if (existingFriendship.Status == "Blocked")
                    {
                        throw new Exception("Bạn đã bị chặn bởi người dùng này.");
                    }
                    if (existingFriendship.Status == "Pending")
                    {
                        throw new Exception("Yêu cầu kết bạn đã tồn tại và đang chờ xử lý.");
                    }
                }

                // Tạo một đối tượng Friendship mới
                var friendship = new Friendship
                {
                    RequestedId = requestedId,
                    AcceptedId = acceptedUser.Id,
                    Status = "Pending", // Trạng thái yêu cầu là "Chờ xử lý"
                    CreateAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                };
                try
                {
                    // Thêm yêu cầu kết bạn vào cơ sở dữ liệu thông qua repository
                    var createdFriendship = await _repository.AddFriend(friendship);
                    // Trả về DTO phản hồi
                    return new FriendshipResponseDto
                    {
                        RequestedId = createdFriendship.RequestedId,
                        AcceptedId = createdFriendship.AcceptedId,
                        Status = createdFriendship.Status,
                        CreatedAt = createdFriendship.CreateAt,
                        UpdatedAt = createdFriendship.UpdatedAt
                    };
                }
                catch (Exception e)
                {
                    throw new Exception($"Lưu quan hệ thất bại: {e.Message}");
                }



            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và trả về thông báo
                throw new Exception($"Thêm bạn thất bại: {ex.Message}");
            }
        }


        // Lấy danh sách bạn bè của người dùng theo userId
        public async Task<List<FriendshipResponseDto>> GetFriends(string userId)
        {
            try
            {
                // Lấy danh sách bạn bè từ repository
                var friends = await _repository.GetFriends(userId);

                // Khởi tạo danh sách DTO phản hồi
                var friendsDto = new List<FriendshipResponseDto>();

                foreach (var friend in friends)
                {
                    // Tìm thông tin bạn bè dựa trên AcceptedId hoặc RequestedId
                    // Chúng ta sẽ kiểm tra ID của người bạn trong mối quan hệ
                    var friendUserId = friend.RequestedId == userId ? friend.AcceptedId : friend.RequestedId;

                    // Lấy thông tin người dùng (bạn bè) để lấy số điện thoại
                    var friendUser = await _userManager.FindByIdAsync(friendUserId);

                    if (friendUser == null)
                    {
                        throw new Exception("Không tìm thấy thông tin người dùng.");
                    }

                    // Tạo đối tượng DTO với thông tin bạn bè, bao gồm số điện thoại
                    var dto = new FriendshipResponseDto
                    {
                        RequestedId = friend.RequestedId,    // ID của người gửi yêu cầu
                        AcceptedId = friend.AcceptedId,      // ID của người nhận yêu cầu
                        Status = friend.Status,              // Trạng thái của yêu cầu
                        CreatedAt = friend.CreateAt,         // Thời gian tạo yêu cầu
                        UpdatedAt = friend.UpdatedAt,        // Thời gian cập nhật yêu cầu
                        PhoneNumber = friendUser.PhoneNumber // Số điện thoại của bạn bè
                    };

                    friendsDto.Add(dto);
                }

                return friendsDto; // Trả về danh sách DTO phản hồi
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu không lấy được danh sách bạn bè
                throw new Exception("Failed to retrieve friends", ex);
            }
        }


        // Chấp nhận yêu cầu kết bạn
        public async Task<FriendshipResponseDto> AcceptFriend(string senderPhoneNumber, string userId)
        {
            try
            {
                // Tìm người gửi (người gửi yêu cầu kết bạn) dựa trên số điện thoại
                var senderUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == senderPhoneNumber);
                if (senderUser == null)
                {
                    throw new Exception("Không tìm thấy người dùng với số điện thoại của người gửi.");
                }

                // Tìm yêu cầu kết bạn trong cơ sở dữ liệu
                var friendship = await _repository.GetFriendship(senderUser.Id, userId);
                if (friendship == null)
                {
                    throw new Exception("Không tìm thấy yêu cầu kết bạn.");
                }

                // Cập nhật trạng thái của yêu cầu kết bạn thành "Accepted"
                friendship.Status = "Accepted";
                friendship.UpdatedAt = DateTime.Now;

                // Lưu thay đổi vào cơ sở dữ liệu
                await _repository.UpdateFriendship(friendship);

                // Trả về DTO phản hồi
                return new FriendshipResponseDto
                {
                    RequestedId = friendship.RequestedId, // ID của người gửi yêu cầu
                    AcceptedId = friendship.AcceptedId,   // ID của người nhận yêu cầu
                    Status = friendship.Status,           // Trạng thái của yêu cầu
                    CreatedAt = friendship.CreateAt,      // Thời gian tạo yêu cầu
                    UpdatedAt = friendship.UpdatedAt      // Thời gian cập nhật yêu cầu
                };
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu chấp nhận yêu cầu thất bại
                throw new Exception("Failed to accept friend request", ex);
            }
        }

        // Chặn người dùng
        public async Task<FriendshipResponseDto> BlockUser(string blockedUserPhoneNumber, string userId)
        {
            try
            {
                // Check if userId is valid (blocker)
                if (string.IsNullOrEmpty(userId))
                {
                    throw new Exception("UserId không được phép rỗng hoặc null.");
                }

                // Find the user to be blocked by their phone number
                var blockedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == blockedUserPhoneNumber);
                if (blockedUser == null)
                {
                    throw new Exception("Không tìm thấy người dùng với số điện thoại này.");
                }

                // Check if the user is trying to block themselves
                if (userId == blockedUser.Id)
                {
                    throw new Exception("Bạn không thể tự chặn chính mình.");
                }

                // Check if the friendship relationship already exists
                var existingFriendship = await _repository.GetFriendship(userId, blockedUser.Id);
                bool isNew = false;
                Friendship friendship;

                if (existingFriendship == null)
                {
                    // If no existing relationship, create a new "Blocked" relationship
                    friendship = new Friendship
                    {
                        RequestedId = userId,          // The user who is blocking
                        AcceptedId = blockedUser.Id,   // The user who is being blocked
                        Status = "Blocked",            // Set the status to "Blocked"
                        CreateAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    isNew = true; // Mark as a new relationship
                }
                else
                {
                    // If the relationship exists, update its status to "Blocked"
                    if (existingFriendship.Status == "Blocked")
                    {
                        throw new Exception("Người dùng này đã bị chặn.");
                    }

                    friendship = existingFriendship;
                    friendship.Status = "Blocked";
                    friendship.UpdatedAt = DateTime.Now;
                }

                // Call repository to save the new or updated relationship
                await _repository.BlockUser(friendship, isNew);

                // Return the friendship response DTO
                return new FriendshipResponseDto
                {
                    RequestedId = friendship.RequestedId,
                    AcceptedId = friendship.AcceptedId,
                    Status = friendship.Status,
                    CreatedAt = friendship.CreateAt,
                    UpdatedAt = friendship.UpdatedAt
                };
            }
            catch (Exception ex)
            {
                // Handle exception and return a meaningful error message
                throw new Exception($"Chặn người dùng thất bại: {ex.Message}", ex);
            }
        }

        // Xóa hoặc hủy kết bạn
        public async Task RemoveFriendByPhone(string requestedId, string friendPhoneNumber)
        {
            try
            {
                // Tìm người dùng (bạn bè) dựa trên số điện thoại
                var acceptedUser = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == friendPhoneNumber);
                if (acceptedUser == null)
                {
                    throw new Exception("Không tìm thấy người dùng với số điện thoại này.");
                }

                // Tìm mối quan hệ kết bạn trong cơ sở dữ liệu
                var friendship = await _repository.GetFriendship(requestedId, acceptedUser.Id);
                if (friendship == null)
                {
                    throw new Exception("Không tìm thấy mối quan hệ kết bạn.");
                }

                // Xóa mối quan hệ kết bạn
                await _repository.RemoveFriend(friendship);
            }
            catch (Exception ex)
            {
                throw new Exception("Không thể xóa kết bạn.", ex);
            }
        }

        // Lấy danh sách người dùng bị chặn
        public async Task<List<FriendshipResponseDto>> GetBlockedUsers(string userId)
        {
            try
            {
                // Lấy danh sách người dùng bị chặn từ repository
                var blockedUsers = await _repository.GetBlockedUsers(userId);

                // Map danh sách người dùng bị chặn thành danh sách DTO phản hồi
                var blockedDto = new List<FriendshipResponseDto>();

                foreach (var b in blockedUsers)
                {
                    // Lấy thông tin người dùng bị chặn để lấy số điện thoại
                    var blockedUser = await _userManager.FindByIdAsync(b.AcceptedId);

                    if (blockedUser == null)
                    {
                        throw new Exception("Không tìm thấy thông tin người dùng bị chặn.");
                    }

                    // Tạo đối tượng DTO với thông tin người dùng bị chặn và số điện thoại
                    var dto = new FriendshipResponseDto
                    {
                        RequestedId = b.RequestedId,  // ID của người gửi yêu cầu
                        AcceptedId = b.AcceptedId,    // ID của người nhận yêu cầu
                        Status = b.Status,            // Trạng thái của yêu cầu
                        CreatedAt = b.CreateAt,       // Thời gian tạo yêu cầu
                        UpdatedAt = b.UpdatedAt,      // Thời gian cập nhật yêu cầu
                        PhoneNumber = blockedUser.PhoneNumber // Số điện thoại của người bị chặn
                    };

                    blockedDto.Add(dto);
                }

                return blockedDto; // Trả về danh sách DTO phản hồi
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu không lấy được danh sách người dùng bị chặn
                throw new Exception("Failed to retrieve blocked users", ex);
            }
        }


        // Lấy danh sách yêu cầu kết bạn chưa được chấp nhận
        public async Task<List<FriendshipResponseDto>> GetFriendRequests(string userId)
        {
            try
            {
                // Lấy danh sách yêu cầu kết bạn từ repository
                var requests = await _repository.GetFriendRequests(userId);

                // Khởi tạo danh sách DTO phản hồi
                var requestsDto = new List<FriendshipResponseDto>();

                foreach (var request in requests)
                {
                    // Lấy thông tin người gửi yêu cầu dựa trên RequestedId
                    var senderUser = await _userManager.FindByIdAsync(request.RequestedId);

                    if (senderUser == null)
                    {
                        throw new Exception("Không tìm thấy thông tin người gửi yêu cầu.");
                    }

                    // Tạo đối tượng DTO với thông tin người gửi yêu cầu, bao gồm số điện thoại
                    var dto = new FriendshipResponseDto
                    {
                        RequestedId = request.RequestedId,  // ID của người gửi yêu cầu
                        AcceptedId = request.AcceptedId,    // ID của người nhận yêu cầu
                        Status = request.Status,            // Trạng thái của yêu cầu
                        CreatedAt = request.CreateAt,       // Thời gian tạo yêu cầu
                        UpdatedAt = request.UpdatedAt,      // Thời gian cập nhật yêu cầu
                        PhoneNumber = senderUser.PhoneNumber // Số điện thoại của người gửi yêu cầu
                    };

                    requestsDto.Add(dto);
                }

                return requestsDto; // Trả về danh sách DTO phản hồi
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu không lấy được danh sách yêu cầu kết bạn
                throw new Exception("Failed to retrieve friend requests", ex);
            }
        }


        public async Task<List<FriendshipResponseDto>> GetFriendReceives(string userId)
        {
            try
            {
                // Lấy danh sách yêu cầu kết bạn từ repository
                var requests = await _repository.GetFriendReceives(userId);

                // Khởi tạo danh sách DTO phản hồi
                var requestsDto = new List<FriendshipResponseDto>();

                foreach (var request in requests)
                {
                    // Lấy thông tin người gửi yêu cầu dựa trên RequestedId
                    var senderUser = await _userManager.FindByIdAsync(request.RequestedId);

                    if (senderUser == null)
                    {
                        throw new Exception("Không tìm thấy thông tin người gửi yêu cầu.");
                    }

                    // Tạo đối tượng DTO với thông tin người gửi yêu cầu, bao gồm số điện thoại
                    var dto = new FriendshipResponseDto
                    {
                        RequestedId = request.RequestedId,  // ID của người gửi yêu cầu
                        AcceptedId = request.AcceptedId,    // ID của người nhận yêu cầu
                        Status = request.Status,            // Trạng thái của yêu cầu
                        CreatedAt = request.CreateAt,       // Thời gian tạo yêu cầu
                        UpdatedAt = request.UpdatedAt,      // Thời gian cập nhật yêu cầu
                        PhoneNumber = senderUser.PhoneNumber // Số điện thoại của người gửi yêu cầu
                    };

                    requestsDto.Add(dto);
                }

                return requestsDto; // Trả về danh sách DTO phản hồi
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ nếu không lấy được danh sách yêu cầu kết bạn
                throw new Exception("Failed to retrieve friend requests", ex);
            }
        }
    }



}



