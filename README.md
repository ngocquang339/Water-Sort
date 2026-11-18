# Water-Sort

## Cấu trúc script

Script của game nằm trong `Assets/Scripts/`, chia theo chức năng:

| Thư mục | Chức năng |
| --- | --- |
| `Gameplay/` | Luật chơi, chai nước, chọn chai, rót nước và trạng thái thắng |
| `Gameplay/Helpers/` | Gợi ý, hoàn tác, thêm chai và popup mua lượt trợ giúp |
| `Gameplay/UI/` | Giao diện trong màn chơi |
| `Levels/` | Dữ liệu màn chơi, bố trí chai và hiển thị số màn |
| `Economy/` | Tiền tệ, tài nguyên và loại phần thưởng |
| `Shop/` | Dữ liệu gói hàng và giao diện cửa hàng |
| `Rewards/` | Vật phẩm thưởng và hiệu ứng mở rương |
| `Rewards/Daily/` | Điểm danh hằng ngày và rương mốc |
| `Profile/` | Hồ sơ người chơi, đặt tên và đổi tên |
| `Leaderboard/` | Gửi điểm, tải và hiển thị bảng xếp hạng |
| `Settings/` | Cài đặt âm lượng và ngôn ngữ |
| `Services/` | Âm thanh, quảng cáo và Unity Gaming Services |
| `SceneManagement/` | Loading và chuyển scene |
| `UI/Navigation/` | Điều hướng tab chính |
| `UI/Common/` | Panel, toast, hiệu ứng xoay và thanh tài nguyên dùng chung |

Đặt script mới vào nhóm chức năng tương ứng. Script thư viện bên ngoài
(DOTween, TextMesh Pro) giữ trong thư mục của thư viện.

Khi di chuyển script, chuyển cả `.cs` và `.cs.meta` để giữ GUID và các
tham chiếu trong Unity. Unity sẽ sinh lại `.csproj` khi đồng bộ project.
Khi merge nhánh cũ, giữ đường dẫn mới trong `Assets/Scripts/` và áp dụng
thay đổi code vào file tương ứng, tránh tạo lại script ở thư mục cũ.
