namespace Domain;

public enum EventRole
{
    CoHost = 1,     // Quản lý toàn bộ sự kiện
    Moderator = 2,  // Quản lý comment/nội dung
    Staff = 3       // Chỉ được quyền Check-in
}