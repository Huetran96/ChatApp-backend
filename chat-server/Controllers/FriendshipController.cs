using chat_server.DTOs;
using chat_server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace chat_server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FriendshipController : ControllerBase
    {
        private readonly IFriendshipService _friendshipService;

        public FriendshipController(IFriendshipService friendshipService)
        {
            _friendshipService = friendshipService;
        }

        // Thêm yêu cầu kết bạn (POST)
        [Authorize]
        [HttpPost("add-friend")]
        public async Task<IActionResult> AddFriendByPhone([FromBody] FriendshipCreateDto dto)
        {
            try
            {
                // Lấy ID của người nhận từ context người dùng hiện tại
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Không có ID người dùng trong token.");
                }
                var userId = userIdClaim.Value;
                // Tìm kiếm người dùng theo số điện thoại
                var friendUser = await _friendshipService.GetUserByPhoneNumber(dto.PhoneNumber);

                // Nếu không tìm thấy người dùng, trả về 404
                if (friendUser == null)
                {
                    return NotFound("Không tìm thấy người dùng với số điện thoại này.");
                }

                // Tạo yêu cầu kết bạn với ID người nhận
                var result = await _friendshipService.AddFriend(userId, dto);

                // Trả về HTTP 200 với kết quả
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Trả về HTTP 500 nếu có lỗi
                return StatusCode(500, ex.Message);
            }
        }

        // Lấy danh sách bạn bè của người dùng (GET)
        [Authorize]
        [HttpGet("friends")]
        public async Task<IActionResult> GetFriends()
        {
            try
            {
                // Lấy ID của người nhận từ context người dùng hiện tại
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Không có ID người dùng trong token.");
                }
                var userId = userIdClaim.Value;
                var result = await _friendshipService.GetFriends(userId);
                return Ok(result); // Trả về HTTP 200 với danh sách bạn bè
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Trả về HTTP 500 nếu có lỗi
            }
        }

        // Chấp nhận yêu cầu kết bạn (PUT)
        [Authorize]
        [HttpPut("accept-friend")]
        public async Task<IActionResult> AcceptFriend([FromQuery] string senderPhoneNumber)
        {
            try
            {
                // Lấy ID của người nhận từ context người dùng hiện tại
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Không có ID người dùng trong token.");
                }
                var userId = userIdClaim.Value;

                // Gọi service để chấp nhận yêu cầu kết bạn dựa trên số điện thoại của người gửi và ID của người nhận
                var result = await _friendshipService.AcceptFriend(senderPhoneNumber, userId);

                // Trả về kết quả với HTTP 200
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Xử lý ngoại lệ và trả về HTTP 500 nếu có lỗi
                return StatusCode(500, ex.Message);
            }
        }

        // Chặn người dùng (POST)
        [Authorize]
        [HttpPost("block-user")]
        public async Task<IActionResult> BlockUser([FromQuery] string blockedUserPhoneNumber)
        {
            try
            {
                // Lấy ID của người nhận từ context người dùng hiện tại
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Không có ID người dùng trong token.");
                }
                var userId = userIdClaim.Value;
                // Gọi service để chặn người dùng dựa trên số điện thoại của người bị chặn và ID người chặn
                var result = await _friendshipService.BlockUser(blockedUserPhoneNumber, userId);

                // Trả về HTTP 200 sau khi chặn người dùng
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Trả về HTTP 500 nếu có lỗi
                return StatusCode(500, ex.Message);
            }
        }

        // Xóa kết bạn (DELETE)
        [Authorize]
        [HttpDelete("remove-friend")]
        public async Task<IActionResult> RemoveFriend([FromQuery] string friendPhoneNumber)
        {
            try
            {
                // Lấy ID của người nhận từ context người dùng hiện tại
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Không có ID người dùng trong token.");
                }
                var userId = userIdClaim.Value;
                // Kiểm tra nếu userId null (người dùng chưa đăng nhập)
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Người dùng không được xác thực."); // Trả về HTTP 401
                }

                // Gọi service để xóa bạn bè dựa trên số điện thoại của người bị xóa
                await _friendshipService.RemoveFriendByPhone(userId, friendPhoneNumber);

                return NoContent(); // Trả về HTTP 204 sau khi xóa kết bạn
            }
            catch (KeyNotFoundException ex)
            {
                // Trả về HTTP 404 nếu không tìm thấy người dùng hoặc quan hệ kết bạn
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Trả về HTTP 500 nếu có lỗi khác
                return StatusCode(500, "Xóa kết bạn thất bại: " + ex.Message);
            }
        }


        // Lấy danh sách người dùng bị chặn (GET)
        [Authorize]
        [HttpGet("blocked-users")]
        public async Task<IActionResult> GetBlockedUsers()
        {
            try
            {
                // Lấy ID của người nhận từ context người dùng hiện tại
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Không có ID người dùng trong token.");
                }
                var userId = userIdClaim.Value;
                var result = await _friendshipService.GetBlockedUsers(userId);
                return Ok(result); // Trả về HTTP 200 với danh sách người bị chặn
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Trả về HTTP 500 nếu có lỗi
            }
        }

        // Lấy danh sách yêu cầu kết bạn chưa được chấp nhận (GET)
        [Authorize]
        [HttpGet("friend-requests")]
        public async Task<IActionResult> GetFriendRequests()
        {
            try
            {
                // Lấy ID của người nhận từ context người dùng hiện tại
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Không có ID người dùng trong token.");
                }
                var userId = userIdClaim.Value;
                var result = await _friendshipService.GetFriendRequests(userId);
                return Ok(result); // Trả về HTTP 200 với danh sách yêu cầu kết bạn
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Trả về HTTP 500 nếu có lỗi
            }
        }

        [Authorize]
        [HttpGet("friend-receives")]
        public async Task<IActionResult> GetFriendReceives()
        {
            try
            {
                // Lấy ID của người nhận từ context người dùng hiện tại
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    return Unauthorized("Không có ID người dùng trong token.");
                }
                var userId = userIdClaim.Value;
                var result = await _friendshipService.GetFriendReceives(userId);
                return Ok(result); // Trả về HTTP 200 với danh sách yêu cầu kết bạn
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Trả về HTTP 500 nếu có lỗi
            }
        }
    }

}
