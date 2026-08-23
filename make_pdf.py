import os
from fpdf import FPDF
import warnings
warnings.filterwarnings("ignore", category=DeprecationWarning)

class PDF(FPDF):
    def header(self):
        pass

    def footer(self):
        self.set_y(-15)
        self.set_font("Arial", "", 8)
        self.cell(0, 10, f"Trang {self.page_no()}", align="C")

def create_pdf():
    pdf = PDF()
    font_path = "C:/Windows/Fonts/arial.ttf"
    font_path_b = "C:/Windows/Fonts/arialbd.ttf"
    font_path_i = "C:/Windows/Fonts/ariali.ttf"
    if not os.path.exists(font_path):
        print("Font not found!")
        return

    pdf.add_font("Arial", "", font_path)
    pdf.add_font("Arial", "B", font_path_b)
    pdf.add_font("Arial", "I", font_path_i)
    pdf.add_page()
    pdf.set_font("Arial", size=11)
    
    # Title
    pdf.set_font("Arial", "B", 16)
    pdf.cell(0, 10, "TÀI LIỆU KỸ THUẬT: GAME WATER SORT", new_x="LMARGIN", new_y="NEXT", align='C')
    pdf.ln(5)
    
    # Content
    pdf.set_font("Arial", "I", 11)
    pdf.multi_cell(0, 7, "Mục tiêu phân phối: Dự án được tối ưu hóa để xuất bản và chạy trực tiếp trên trình duyệt web thông qua nền tảng itch.io (WebGL), cho phép mọi người chơi ngay mà không cần tải về.")
    pdf.ln(5)
    
    pdf.set_font("Arial", "B", 14)
    pdf.cell(0, 8, "I. MÔ TẢ THIẾT KẾ (DESIGN DOCUMENT)", new_x="LMARGIN", new_y="NEXT")
    pdf.ln(2)
    
    pdf.set_font("Arial", "B", 12)
    pdf.cell(0, 6, "1. Kiến trúc hệ thống", new_x="LMARGIN", new_y="NEXT")
    pdf.set_font("Arial", "", 11)
    pdf.multi_cell(0, 6, "- Môi trường phát triển: Unity Engine và ngôn ngữ C#.\n- Kiến trúc phần mềm: Sử dụng Component-based architecture đặc trưng của Unity. Phân tách rõ ràng giữa các hệ thống Quản lý Game (GameManager, LevelManager), Dữ liệu người dùng (ProfileManager), và Kinh tế trong game (CurrencyManager, Shop).")
    pdf.ln(3)

    pdf.set_font("Arial", "B", 12)
    pdf.cell(0, 6, "2. Các thành phần logic cốt lõi", new_x="LMARGIN", new_y="NEXT")
    pdf.set_font("Arial", "", 11)
    pdf.multi_cell(0, 6, "- GameManager: Quản lý vòng đời màn chơi (Khởi tạo, Đang chơi, Hoàn thành, Tạm dừng). Xử lý tương tác cốt lõi khi người chơi chọn bình và đổ nước.\n- Bottle: Quản lý dữ liệu của từng bình nước (thay thế cho TubeController). Lưu trữ các lớp màu theo cấu trúc Stack để đảm bảo nguyên tắc LIFO khi rót nước. Đảm nhiệm logic và hiệu ứng rót nước (thay thế cho LiquidMechanic), kiểm tra điều kiện rót hợp lệ (chưa đầy và cùng màu).\n- LevelManager: Quản lý hệ thống cấp độ. Đọc cấu hình màn chơi, khởi tạo số lượng bình nước tương ứng và phân bổ dữ liệu màu sắc ban đầu.")
    pdf.ln(5)

    pdf.set_font("Arial", "B", 14)
    pdf.cell(0, 8, "II. HƯỚNG DẪN CÀI ĐẶT & SỬ DỤNG (DEPLOYMENT GUIDE)", new_x="LMARGIN", new_y="NEXT")
    pdf.ln(2)

    pdf.set_font("Arial", "B", 12)
    pdf.cell(0, 6, "1. Trải nghiệm trên nền tảng Web (itch.io)", new_x="LMARGIN", new_y="NEXT")
    pdf.set_font("Arial", "", 11)
    pdf.multi_cell(0, 6, "- Bước 1: Truy cập vào đường dẫn itch.io của dự án.\n- Bước 2: Chờ trình duyệt tải dữ liệu (Load WebGL). Quá trình này chạy ổn định nhất trên các trình duyệt như Google Chrome, Microsoft Edge hoặc Firefox.\n- Bước 3: Nhấn \"Run Game\" hoặc tương tác trực tiếp vào khung game để bắt đầu.")
    pdf.ln(3)

    pdf.set_font("Arial", "B", 12)
    pdf.cell(0, 6, "2. Dành cho Giảng viên kiểm tra mã nguồn (Source Code)", new_x="LMARGIN", new_y="NEXT")
    pdf.set_font("Arial", "", 11)
    pdf.multi_cell(0, 6, "- Sử dụng Unity Hub để chọn Add Project và trỏ tới thư mục mã nguồn.\n- Mở dự án, điều hướng đến thư mục Assets/Scenes.\n- Nhấp đúp mở Scene chính (thường là LoadingScene hoặc MainGame) và nhấn nút Play trên thanh công cụ của Unity Editor để chạy thử.")
    pdf.ln(5)

    pdf.set_font("Arial", "B", 14)
    pdf.cell(0, 8, "III. HƯỚNG DẪN BẢO TRÌ & MỞ RỘNG (MAINTENANCE)", new_x="LMARGIN", new_y="NEXT")
    pdf.ln(2)
    
    pdf.set_font("Arial", "B", 12)
    pdf.cell(0, 6, "1. Cập nhật và triển khai lên itch.io", new_x="LMARGIN", new_y="NEXT")
    pdf.set_font("Arial", "", 11)
    pdf.multi_cell(0, 6, "- Khi mã nguồn hoặc màn chơi được cập nhật, thực hiện xuất bản mới qua File -> Build Settings -> WebGL -> Build.\n- Nén toàn bộ thư mục vừa xuất thành một tệp .zip.\n- Trên trang quản lý của itch.io (phần Edit Game), tải tệp .zip mới lên và đảm bảo tùy chọn \"This file will be played in the browser\" được đánh dấu.")
    pdf.ln(3)

    pdf.set_font("Arial", "B", 12)
    pdf.cell(0, 6, "2. Thiết kế và thêm màn chơi mới (Level Design)", new_x="LMARGIN", new_y="NEXT")
    pdf.set_font("Arial", "", 11)
    pdf.multi_cell(0, 6, "- Truy cập hệ thống dữ liệu màn chơi (sử dụng ScriptableObject hoặc file dữ liệu cấu hình Level).\n- Tạo dữ liệu cho Level mới: Cấu hình số lượng bình và quy định ID màu sắc cho từng tầng (layer) trong bình.\n- Hệ thống LevelManager sẽ tự động phân tích dữ liệu này để sinh ra môi trường tương ứng khi người chơi chọn màn.")
    pdf.ln(3)

    pdf.set_font("Arial", "B", 12)
    pdf.cell(0, 6, "3. Xử lý sự cố thường gặp (Troubleshooting)", new_x="LMARGIN", new_y="NEXT")
    pdf.set_font("Arial", "", 11)
    pdf.multi_cell(0, 6, "- Lỗi trình duyệt không tải được WebGL: Kiểm tra phần Player Settings -> Publishing Settings, thay đổi Compression Format sang Disabled hoặc Gzip nếu máy chủ không hỗ trợ giải nén Brotli.\n- Không thể click vào bình nước: Kiểm tra trong Hierarchy xem đối tượng EventSystem có tồn tại hay không, và Main Camera đã được gắn Physics 2D Raycaster để nhận diện tương tác chuột/chạm chưa.")
    
    pdf.output("Report_WaterSort_Corrected.pdf")
    print("PDF generated successfully.")

if __name__ == "__main__":
    create_pdf()
