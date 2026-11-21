
XÂY DỰNG HỆ THỐNG TƯ VẤN VÀ GỢI Ý TOUR DU LỊCH CÁ NHÂN HÓA TÍCH HỢP TRỢ LÝ AI

1. Thông tin đồ án

- Tên đề tài: Xây dựng hệ thống tư vấn và gợi ý tour du lịch cá nhân hóa tích hợp trợ lý AI
- Sinh viên thực hiện: Mai Nguyễn Trường Duy
- Mã số sinh viên: 1150070006
- Lớp: 11_ĐH_TMĐT
- Khóa học: 2022–2026
- Giảng viên hướng dẫn: ThS. Từ Thanh Trí
- Khoa: Công nghệ thông tin
- Trường: Đại học Tài nguyên và Môi trường Thành phố Hồ Chí Minh

2. Giới thiệu đề tài

Đồ án thực hiện xây dựng hệ thống tư vấn và gợi ý tour du lịch cá nhân hóa tích hợp trợ lý AI trên nền tảng website HappyTrip.

Hệ thống được phát triển trên cơ sở kế thừa các chức năng của website đặt tour du lịch, đồng thời bổ sung chức năng tư vấn và gợi ý tour dựa trên nhu cầu thực tế của người dùng.

Thay vì chỉ tìm kiếm tour bằng các bộ lọc có sẵn, người dùng có thể trao đổi với trợ lý AI bằng ngôn ngữ tự nhiên để mô tả nhu cầu chuyến đi, chẳng hạn như điểm đến, thời gian khởi hành, ngân sách, số lượng người tham gia, loại hình du lịch và sở thích trải nghiệm.

Từ các thông tin này, hệ thống tiến hành nhận diện nhu cầu, truy xuất dữ liệu tour, đánh giá mức độ phù hợp và đề xuất các tour để người dùng tham khảo.

Hệ thống còn hỗ trợ hội thoại nhiều lượt. Khi người dùng bổ sung hoặc thay đổi một điều kiện, trợ lý có thể cập nhật nhu cầu và thực hiện tìm kiếm lại dựa trên các thông tin đã trao đổi.

Phạm vi nghiên cứu của đồ án tập trung vào hoạt động tư vấn, lựa chọn và đăng ký tour du lịch khu vực miền Nam Việt Nam, sử dụng dữ liệu được quản lý trên website HappyTrip.

3. Mục tiêu thực hiện

Mục tiêu chính của đồ án là xây dựng hệ thống hỗ trợ người dùng lựa chọn tour du lịch phù hợp với nhu cầu cá nhân thông qua việc kết hợp dữ liệu tour thực tế và công nghệ trí tuệ nhân tạo.

Các mục tiêu cụ thể gồm:

1. Kế thừa và hoàn thiện các chức năng nghiệp vụ của website đặt tour du lịch.
2. Tiếp nhận và phân tích nhu cầu du lịch do người dùng nhập bằng ngôn ngữ tự nhiên.
3. Xác định các tiêu chí tìm kiếm, bao gồm điểm đến, thời gian, ngân sách, số lượng khách, thời lượng, loại tour và sở thích.
4. Truy xuất thông tin tour, giá và lịch khởi hành từ cơ sở dữ liệu SQL Server.
5. Đánh giá mức độ phù hợp của tour dựa trên các tiêu chí tìm kiếm.
6. Xếp hạng và đề xuất danh sách tour phù hợp với nhu cầu người dùng.
7. Hỗ trợ giải thích lý do đề xuất tour.
8. Duy trì ngữ cảnh trong quá trình hội thoại nhiều lượt.
9. Kết hợp dữ liệu có cấu trúc và nội dung văn bản để phục vụ quá trình tư vấn.
10. Xây dựng các tình huống thử nghiệm nhằm đánh giá kết quả hoạt động của trợ lý AI.

4. Công nghệ sử dụng

4.1. Giao diện

- HTML
- CSS
- Bootstrap 5
- JavaScript
- Razor Views

4.2. Xử lý phía máy chủ

- Ngôn ngữ C#
- ASP.NET Core MVC
- .NET 8
- Entity Framework Core

4.3. Cơ sở dữ liệu

- Microsoft SQL Server
- SQL Server Management Studio (SSMS)

4.4. Trí tuệ nhân tạo

- Google Gemini API
- Xử lý ngôn ngữ tự nhiên
- Phân tích và nhận diện nhu cầu người dùng
- Truy xuất dữ liệu tour có cấu trúc
- Đánh giá mức độ phù hợp và xếp hạng tour
- Retrieval-Augmented Generation (RAG)
- Duy trì ngữ cảnh hội thoại nhiều lượt

4.5. Công cụ phát triển

- Visual Studio Code
- .NET SDK
- SQL Server Management Studio
- Git
- GitHub
- Draw.io

4.6. Các dịch vụ hỗ trợ

- VNPay Sandbox phục vụ kiểm thử thanh toán trực tuyến
- SMTP Gmail phục vụ chức năng gửi email

5. Các chức năng của hệ thống

5.1. Chức năng dành cho khách hàng

Người dùng có thể thực hiện các chức năng:

- Đăng ký tài khoản.
- Đăng nhập và đăng xuất.
- Quản lý thông tin cá nhân.
- Xem danh sách tour du lịch.
- Tìm kiếm và xem thông tin chi tiết tour.
- Xem giá và lịch khởi hành.
- Lựa chọn tour và thực hiện đặt tour.
- Áp dụng phiếu giảm giá khi đáp ứng điều kiện.
- Thanh toán hoặc giữ chỗ theo các phương thức được hệ thống hỗ trợ.
- Theo dõi thông tin và trạng thái đơn đặt tour.
- Xem thông tin vé và check-in.
- Xem bài viết và gửi bình luận.
- Sử dụng trợ lý AI Tripi để tìm kiếm và được tư vấn tour.

5.2. Chức năng dành cho quản trị viên

Quản trị viên có thể sử dụng các chức năng quản lý của website, bao gồm:

- Quản lý loại tour.
- Quản lý thông tin và hình ảnh tour.
- Quản lý lịch khởi hành.
- Quản lý phiếu giảm giá.
- Quản lý đơn đặt tour.
- Theo dõi thông tin thanh toán và trạng thái đơn.
- Quản lý hoạt động check-in.
- Quản lý bài viết và bình luận.
- Theo dõi thống kê trên Dashboard.
- Quản lý các dữ liệu liên quan đến hoạt động của hệ thống.

5.3. Chức năng trợ lý AI Tripi

Trợ lý AI Tripi được phát triển nhằm hỗ trợ người dùng tìm kiếm và lựa chọn tour du lịch dựa trên nhu cầu cá nhân.

Các chức năng nghiên cứu và triển khai gồm:

- Tiếp nhận câu hỏi bằng tiếng Việt.
- Phân tích nội dung người dùng nhập.
- Nhận diện điểm đến, ngân sách, thời gian, số lượng khách và các điều kiện khác.
- Ghi nhận các điều kiện đã được cung cấp trong cuộc hội thoại.
- Cập nhật điều kiện khi người dùng thay đổi nhu cầu.
- Truy xuất dữ liệu tour và lịch khởi hành trong cơ sở dữ liệu.
- Đối chiếu nhu cầu của người dùng với thông tin tour.
- Tính mức độ phù hợp và xếp hạng kết quả gợi ý.
- Hiển thị danh sách tour đề xuất.
- Hỗ trợ giải đáp thông tin tour dựa trên dữ liệu có sẵn.
- Phản hồi khi chưa tìm được tour đáp ứng đầy đủ điều kiện.

Thông tin về giá, lịch khởi hành và số chỗ còn lại cần được đối chiếu với dữ liệu thực tế của hệ thống trước khi đặt tour.

6. Cấu trúc thư mục mã nguồn

Repository được tổ chức với thư mục chính là TourWebApp.

Cấu trúc các thư mục quan trọng:

```text
DATN_MaiNguyenTruongDuy_1150070006_11DHTMDT/
│
├── README.md
├── .gitignore
│
└── TourWebApp/
    │
    ├── Controllers/
    ├── Data/
    ├── Helpers/
    ├── Models/
    ├── Properties/
    ├── Services/
    ├── SQL/
    ├── Views/
    ├── wwwroot/
    │   ├── css/
    │   ├── fonts/
    │   ├── img/
    │   ├── js/
    │   └── lib/
    │
    ├── Program.cs
    └── TourWebApp.csproj
```

Trong đó:

- Controllers: Tiếp nhận và xử lý các yêu cầu của người dùng.
- Data: Chứa các lớp làm việc với cơ sở dữ liệu.
- Models: Chứa các lớp mô hình dữ liệu.
- Services: Chứa các lớp xử lý nghiệp vụ và các chức năng liên quan đến trợ lý AI.
- Views: Chứa giao diện được xây dựng bằng Razor.
- wwwroot: Chứa CSS, JavaScript, hình ảnh và các tài nguyên tĩnh.
- SQL: Chứa các tệp SQL phục vụ hệ thống.
- Program.cs: Cấu hình và khởi động ứng dụng.
- TourWebApp.csproj: Chứa thông tin dự án và các gói phụ thuộc.

Các thư mục bin và obj là kết quả phát sinh trong quá trình biên dịch nên không cần đưa lên GitHub.

7. Yêu cầu môi trường cài đặt

Để cài đặt và chạy dự án, máy tính cần có:

1. .NET SDK 8.0.
2. Microsoft SQL Server.
3. SQL Server Management Studio hoặc công cụ quản lý SQL Server tương đương.
4. Visual Studio Code hoặc Visual Studio.
5. Trình duyệt web như Google Chrome hoặc Microsoft Edge.
6. Kết nối Internet khi sử dụng các chức năng cần gọi dịch vụ bên ngoài, chẳng hạn như Gemini API hoặc VNPay Sandbox.

Các gói thư viện của dự án được khai báo trong tệp TourWebApp.csproj và được khôi phục thông qua lệnh dotnet restore.

8. Hướng dẫn tải và cài đặt mã nguồn

Bước 1. Tải mã nguồn

Truy cập repository:

https://github.com/Mai-Duy-04/DATN_MaiNguyenTruongDuy_1150070006_11DHTMDT

Có thể chọn Code → Download ZIP hoặc sử dụng Git:

```bash
git clone https://github.com/Mai-Duy-04/DATN_MaiNguyenTruongDuy_1150070006_11DHTMDT.git
```

Bước 2. Mở thư mục dự án

Di chuyển vào thư mục chứa dự án:

```bash
cd DATN_MaiNguyenTruongDuy_1150070006_11DHTMDT
cd TourWebApp
```

Mở thư mục này bằng Visual Studio Code hoặc công cụ phát triển .NET phù hợp.

Bước 3. Khôi phục các gói thư viện

Mở Terminal và thực hiện:

```bash
dotnet restore
```

Lệnh này sẽ khôi phục các gói thư viện được khai báo trong TourWebApp.csproj.

9. Hướng dẫn thiết lập cơ sở dữ liệu

9.1. Tạo cơ sở dữ liệu

Mở SQL Server Management Studio và kết nối đến SQL Server trên máy.

Tạo cơ sở dữ liệu có tên HappyTripDB bằng câu lệnh:

```sql
IF DB_ID(N'HappyTripDB') IS NULL
BEGIN
    CREATE DATABASE HappyTripDB;
END;
GO
```

9.2. Khôi phục cấu trúc và dữ liệu mẫu

Mở tệp SQL cơ sở dữ liệu được cung cấp kèm mã nguồn.

Kiểm tra tên cơ sở dữ liệu trong tệp SQL, sau đó thực thi script để tạo các bảng, khóa liên kết và dữ liệu phục vụ quá trình chạy thử.

Lưu ý: Nếu tệp SQL bắt đầu bằng câu lệnh USE [HappyTripDB] thì cần tạo cơ sở dữ liệu HappyTripDB trước khi chạy script.

Sau khi thực thi, kiểm tra các bảng dữ liệu trong SQL Server Management Studio.

Một số nhóm dữ liệu chính của hệ thống gồm:

- Tài khoản và phân quyền.
- Thông tin tour và loại tour.
- Lịch khởi hành.
- Đơn đặt tour.
- Thanh toán và phiếu giảm giá.
- Nội dung bài viết, bình luận.
- Dữ liệu phục vụ trợ lý AI.
- Lịch sử và ngữ cảnh hội thoại.
- Dữ liệu tri thức phục vụ quá trình truy xuất thông tin.

9.3. Điều chỉnh chuỗi kết nối

Thông tin SQL Server trên mỗi máy có thể khác nhau.

Cần điều chỉnh mục ConnectionStrings trong tệp cấu hình theo tên SQL Server đang sử dụng.

Ví dụ, với SQL Server sử dụng Windows Authentication:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TEN_MAY\\TEN_INSTANCE;Database=HappyTripDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

Thay TEN_MAY và TEN_INSTANCE bằng tên máy chủ và SQL Server Instance thực tế.

Nếu sử dụng SQL Server Authentication, cần điều chỉnh chuỗi kết nối tương ứng và cung cấp thông tin đăng nhập hợp lệ.

10. Hướng dẫn cấu hình ứng dụng

Tạo tệp appsettings.json trong thư mục TourWebApp nếu tệp này chưa có.

Các giá trị cấu hình riêng của từng máy và thông tin bí mật không được công khai trong repository.

Cấu trúc cấu hình tham khảo:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=TEN_MAY\\TEN_INSTANCE;Database=HappyTripDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "VNPAY": {
    "TmnCode": "MA_WEBSITE_VNPAY_SANDBOX",
    "HashSecret": "KHOA_BI_MAT_VNPAY_SANDBOX",
    "BaseUrl": "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html",
    "PublicBaseUrl": "http://localhost:5161",
    "CallbackUrl": "http://localhost:5161/DatTour/VnpayCallback",
    "IpnUrl": "",
    "Version": "2.1.0",
    "OrderType": "other"
  },
  "EmailSettings": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderName": "HappyTrip",
    "SenderEmail": "your-email@gmail.com",
    "Username": "your-email@gmail.com",
    "Password": "MAT_KHAU_UNG_DUNG",
    "EnableSsl": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

10.1. Cấu hình SQL Server

Thay chuỗi DefaultConnection bằng thông tin kết nối SQL Server phù hợp với máy đang sử dụng.

Tên cơ sở dữ liệu của dự án là HappyTripDB.

10.2. Cấu hình VNPay Sandbox

Các thông số trong mục VNPAY phục vụ chức năng thanh toán thử nghiệm.

- TmnCode: Mã website do VNPay Sandbox cung cấp.
- HashSecret: Khóa bí mật dùng để xác thực dữ liệu thanh toán.
- BaseUrl: Địa chỉ môi trường thanh toán Sandbox.
- PublicBaseUrl: Địa chỉ ứng dụng đang chạy.
- CallbackUrl: Đường dẫn nhận kết quả thanh toán.
- IpnUrl: Địa chỉ tiếp nhận thông báo thanh toán từ máy chủ VNPay nếu chức năng này được cấu hình.
- Version: Phiên bản giao thức tích hợp.
- OrderType: Loại đơn hàng.

Trong môi trường chạy thử trên máy cá nhân, ứng dụng được cấu hình sử dụng cổng 5161.

Để sử dụng thanh toán trên một địa chỉ công khai, cần thay đổi các URL tương ứng và cấu hình đường dẫn có thể truy cập từ bên ngoài.

Không đưa HashSecret thật lên GitHub.

10.3. Cấu hình gửi email

Hệ thống sử dụng SMTP Gmail với máy chủ smtp.gmail.com và cổng 587.

Cần điền:

- Địa chỉ email gửi.
- Tên đăng nhập email.
- Mật khẩu ứng dụng hoặc phương thức xác thực được tài khoản Gmail hỗ trợ.

Nếu chưa cấu hình thông tin xác thực hợp lệ, các chức năng gửi email có thể không hoạt động.

Không sử dụng mật khẩu tài khoản Gmail thông thường làm thông tin công khai trong mã nguồn.

10.4. Cấu hình Google Gemini API

Chức năng trợ lý AI sử dụng Google Gemini API để hỗ trợ phân tích nhu cầu và tạo phản hồi tư vấn.

Để kiểm thử các chức năng cần kết nối đến Gemini, phải cung cấp API key hợp lệ theo đúng cách đọc cấu hình được triển khai trong mã nguồn của dự án.

API key phải được lưu trong cấu hình riêng hoặc biến môi trường và không đưa lên GitHub.

Cần có kết nối Internet trong quá trình sử dụng API.

11. Hướng dẫn chạy chương trình

Sau khi hoàn tất cấu hình cơ sở dữ liệu và các dịch vụ cần thiết, thực hiện lệnh:

```bash
dotnet build
```

Nếu biên dịch thành công, chạy ứng dụng bằng lệnh:

```bash
dotnet run --urls "http://localhost:5161"
```

Truy cập địa chỉ:

http://localhost:5161

Nếu sử dụng một cổng khác, cần điều chỉnh lại các URL liên quan đến chức năng thanh toán để phù hợp với cổng đang chạy.

12. Tài khoản kiểm thử

12.1. Tài khoản quản trị viên

Thông tin tài khoản quản trị được chuẩn bị để giảng viên có thể đăng nhập và kiểm tra các chức năng quản lý của hệ thống:

- Email: admin@happydulich.vn
- Mật khẩu: Admin@123
- Vai trò: Quản trị viên

Tài khoản này được sử dụng trong môi trường kiểm thử của đồ án.

12.2. Tài khoản khách hàng

Người kiểm thử có thể sử dụng chức năng Đăng ký trên website để tự tạo tài khoản khách hàng.

Sau khi đăng ký, có thể đăng nhập để thực hiện các chức năng dành cho người dùng, bao gồm tìm kiếm tour, đặt tour và theo dõi đơn đặt tour.

13. Thông tin kiểm thử thanh toán VNPay Sandbox

Hệ thống sử dụng môi trường VNPay Sandbox để mô phỏng quá trình thanh toán.

Thông tin thẻ kiểm thử:

- Ngân hàng: NCB
- Số thẻ: 9704198526191432198
- Tên chủ thẻ: NGUYEN VAN A
- Ngày phát hành: 07/15
- Mật khẩu OTP kiểm thử: 123456

Đây là thông tin phục vụ môi trường kiểm thử VNPay Sandbox, không phải tài khoản thanh toán thật.

Các bước kiểm thử tham khảo:

1. Đăng nhập bằng tài khoản khách hàng.
2. Chọn một tour có lịch khởi hành phù hợp và còn chỗ.
3. Nhập thông tin đặt tour.
4. Lựa chọn phương thức thanh toán VNPay.
5. Chuyển đến trang thanh toán Sandbox.
6. Nhập thông tin thẻ kiểm thử.
7. Nhập OTP kiểm thử để hoàn tất thao tác.
8. Kiểm tra kết quả trả về và trạng thái đơn đặt tour trên website.

Kết quả thanh toán phụ thuộc vào cấu hình VNPay và khả năng tiếp nhận phản hồi của ứng dụng.

14. Hướng dẫn kiểm thử trợ lý AI Tripi

Trên giao diện website, truy cập chức năng trợ lý AI Tripi.

Người kiểm thử có thể nhập các yêu cầu bằng tiếng Việt và quan sát kết quả trả lời cùng danh sách tour đề xuất.

Tình huống 1. Tìm tour theo nhu cầu

Nội dung nhập:

"Mình muốn đi Phú Quốc trong tháng 12 năm 2026, ngân sách dưới 5 triệu đồng mỗi người, nhóm 4 người. HappyTrip có tour nào phù hợp không?"

Kết quả cần kiểm tra:

- Hệ thống nhận diện điểm đến Phú Quốc.
- Nhận diện tháng khởi hành.
- Nhận diện ngân sách theo mỗi người.
- Nhận diện số lượng khách.
- Truy xuất và hiển thị các tour đáp ứng điều kiện từ dữ liệu hiện có.
- Hiển thị thông tin tour để người dùng tham khảo.

Tình huống 2. Thay đổi ngân sách

Tiếp tục trong cùng cuộc hội thoại:

"Nếu mình tăng ngân sách lên 6 triệu mỗi người thì có thêm lựa chọn nào không?"

Kết quả cần kiểm tra:

- Hệ thống ghi nhận ngân sách mới.
- Giữ lại điểm đến, thời gian và số lượng người đã cung cấp.
- Thực hiện tìm kiếm lại.
- Cập nhật danh sách tour được đề xuất.

Tình huống 3. Thay đổi số lượng khách

Tiếp tục trong cùng cuộc hội thoại:

"Nếu nhóm mình tăng lên 20 người thì còn lịch nào đủ chỗ không?"

Kết quả cần kiểm tra:

- Hệ thống cập nhật số lượng khách thành 20.
- Giữ các điều kiện còn lại nếu người dùng không yêu cầu thay đổi.
- Đối chiếu số chỗ còn lại của từng lịch khởi hành.
- Chỉ đề xuất các lịch đáp ứng điều kiện số lượng khách.

Tình huống 4. Không tìm thấy tour phù hợp

Nhập yêu cầu với các điều kiện không được dữ liệu hiện tại đáp ứng.

Kết quả cần kiểm tra:

- Hệ thống không hiển thị tour không đáp ứng điều kiện như thể đó là kết quả chính xác.
- Trợ lý thông báo rõ rằng chưa tìm được tour phù hợp.
- Có thể hướng dẫn người dùng điều chỉnh thời gian, điểm đến hoặc các điều kiện khác.
- Nội dung phản hồi cần dễ hiểu và không hiển thị các thuật ngữ kỹ thuật nội bộ.

Lưu ý: Kết quả gợi ý phụ thuộc vào dữ liệu tour, lịch khởi hành và số chỗ còn lại tại thời điểm kiểm thử.

15. Phương pháp tư vấn và gợi ý tour

Quá trình tư vấn được nghiên cứu theo các bước:

15.1. Tiếp nhận nhu cầu

Người dùng nhập câu hỏi hoặc yêu cầu bằng ngôn ngữ tự nhiên.

15.2. Nhận diện tiêu chí

Trợ lý phân tích các thông tin liên quan đến chuyến đi như:

- Điểm đến.
- Ngân sách.
- Thời gian dự kiến.
- Số lượng khách.
- Thời lượng chuyến đi.
- Loại hình du lịch.
- Sở thích trải nghiệm.

15.3. Truy xuất dữ liệu

Hệ thống sử dụng các tiêu chí đã nhận diện để truy xuất dữ liệu tour, giá và lịch khởi hành trong SQL Server.

Đối với thông tin dạng văn bản, hướng nghiên cứu có sử dụng cơ chế truy xuất nội dung phục vụ quá trình tư vấn.

15.4. Đánh giá và xếp hạng

Các tour được đối chiếu với nhu cầu người dùng để xác định mức độ phù hợp.

Kết quả được sắp xếp nhằm ưu tiên những tour đáp ứng tốt hơn các điều kiện đã cung cấp.

15.5. Tạo phản hồi

Trợ lý tổng hợp thông tin và tạo câu trả lời bằng tiếng Việt, đồng thời hiển thị danh sách tour để người dùng tham khảo.

15.6. Duy trì hội thoại nhiều lượt

Các điều kiện đã được ghi nhận có thể tiếp tục được sử dụng trong những lượt hội thoại sau.

Khi người dùng bổ sung, thay đổi hoặc bỏ giới hạn một điều kiện, hệ thống cần cập nhật nhu cầu và xử lý lại kết quả tương ứng.

16. Dữ liệu và nội dung thực nghiệm

Theo định hướng của đề cương, việc thực nghiệm tập trung vào các nội dung:

1. Đánh giá khả năng nhận diện nhu cầu du lịch.
2. Đánh giá kết quả truy xuất tour.
3. Đánh giá mức độ phù hợp và thứ tự xếp hạng tour.
4. Kiểm tra tính chính xác của thông tin trong phản hồi.
5. Kiểm tra khả năng ghi nhớ và cập nhật ngữ cảnh hội thoại nhiều lượt.
6. Kiểm tra tình huống không tìm thấy tour đáp ứng điều kiện.

Đề cương định hướng xây dựng khoảng 30–50 tình huống kiểm thử và sử dụng các chỉ số như Accuracy, Precision@N và NDCG@N cho những nội dung đánh giá phù hợp.

Kết quả đánh giá thực tế, số lượng tình huống đã thực hiện và các bảng thống kê cần được đối chiếu với nội dung thực nghiệm trong báo cáo đồ án tốt nghiệp.

17. Một số lưu ý khi chạy chương trình

- Cần khởi động SQL Server trước khi chạy website.
- Phải khôi phục cơ sở dữ liệu và cấu hình đúng chuỗi kết nối.
- Một số chức năng yêu cầu đăng nhập.
- Chức năng gửi email yêu cầu thông tin SMTP hợp lệ.
- Chức năng AI yêu cầu cấu hình API phù hợp.
- Chức năng thanh toán thử nghiệm phụ thuộc vào VNPay Sandbox.
- Không công khai khóa API, mật khẩu email hoặc khóa bí mật thanh toán.
- Dữ liệu mẫu và tài khoản thử nghiệm chỉ được sử dụng cho mục đích học tập, nghiên cứu và đánh giá đồ án.
- Cần đối chiếu dữ liệu tour thực tế khi kiểm thử các chức năng tư vấn, đặt tour và thanh toán.

18. Thông tin repository

Mã nguồn đồ án được cung cấp tại:

https://github.com/Mai-Duy-04/DATN_MaiNguyenTruongDuy_1150070006_11DHTMDT

Repository được sử dụng để lưu trữ mã nguồn và phục vụ quá trình kiểm tra, đánh giá đồ án tốt nghiệp.

Sinh viên thực hiện: Mai Nguyễn Trường Duy.

Giảng viên hướng dẫn: ThS. Từ Thanh Trí.
