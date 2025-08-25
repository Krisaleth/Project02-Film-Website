# ASP.NET Core MVC Admin Authenticaton - Hướng dẫn cơ bản & giải thích code

## 1. Mục tiêu, tổng quan

Mục tiêu: Bảo vệ các trang "/admin/**" (trừ trang "/admin/login" vì lúc mới vào thì cần phải thông qua trang này) chỉ cho user có Role = "Admin", đăng nhập qua form và tạo cookies, dùng PBKDF2 (SHA-256, 100k vòng) để băm password nhằm tăng cường bảo mật.

Luòng chính: 

1. Người dùng truy cập các URL được bảo vệ (các URL được đề cập ở trên như /admin).

2. Người dùng sẽ bị đưa về trang /admin/login nếu như chưa đăng nhập, còn nếu đã đăng nhập mà vào URL /admin/login thì tự động vào URL /admin.

3. Sau khi điền thông tin và gửi về máy chủ -> Controller kiểm tra DB các yếu tố sau:

- User có tồn tại trong DB, Status đang là true.

- Role là Admin.

- Dùng hàm Verify trong class PasswordHasher để kiểm tra password.

- Tất cả điểu kiện trên đúng -> Tạo Claim + Cookie -> return về ReturnUrl trong AdminLoginVm của folder ViewModel.

- Nếu có 1 điều kiện sai -> hiện lỗi trên form.
		
## 2. Giải thích từ khoá: 

- Claim: là một thông tin (key-value) về người dùng, thường được gắn vào Identity sau khi xác thực thành công.

- Identity: “Danh tính” của một user trong hệ thống.

- Cookie: là một đoạn dữ liệu nhỏ (key-value) mà server gửi xuống và lưu lại ở trình duyệt; mỗi lần client gửi request lên server, cookie sẽ được gửi kèm để server nhận diện.

- PBKDF2 (Password-Based Key Derivation Function 2):

	- Là thuật toán băm mật khẩu có “kéo dài khóa” (key stretching), chuẩn hóa trong PKCS #5 (RFC 8018).
	
	- Mục đích: biến mật khẩu gốc (dễ đoán, yếu) thành một chuỗi khóa khó bị brute-force.
	
	- Nó không chỉ băm mật khẩu một lần mà sẽ lặp lại nhiều lần (iterations) → làm cho việc tấn công thử mật khẩu tốn nhiều tài nguyên CPU/GPU hơn.
	
- Pipeline: Là chuỗi middleware chạy theo thứ tự bạn đăng ký trong Program.cs.

- Route: bản đồ ánh xạ URL → Controller/Action (hoặc Endpoint).

- Async / Await: từ khóa trong C# để làm việc với bất đồng bộ (asynchronous programming).
	
## 3. Các thành phần cần thiết:

- AdminController: xử lý redirect và kiểm tra các yêu cầu trên.

- AdminMiddleware: chặn mọi yêu cầu đến các trang "/admin/**" (trừ "/admin/login") khi chưa có cookie và claim, kiểm tra login và role.

- AdminLoginVm: ViewModel chứa dữ liệu của form.

- PasswordHasher: sinh và kiểm tra PBKDF2.

- Program.cs: đăng ký Cookie Authenticaton, thêm MVC, sắp xếp pipeline đúng thứ tự. cấu hình route.
