namespace chat_server.DTOs
{
    public class FriendshipResponseDto
    {
        public int Id { get; set; }
        public string RequestedId { get; set; } // ID của người gửi yêu cầu
        public string AcceptedId { get; set; }  // ID của người nhận yêu cầu
        public string Status { get; set; }      // Trạng thái của yêu cầu kết bạn
        public DateTime CreatedAt { get; set; } // Thời gian tạo yêu cầu
        public DateTime UpdatedAt { get; set; } // Thời gian cập nhật yêu cầu
        public string PhoneNumber { get; set; } // Số điện thoại 
    }
}
