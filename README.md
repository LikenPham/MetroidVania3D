# Game Metroidvania 3D

## Giới thiệu

Đây là dự án game thuộc thể loại **Metroidvania**, được phát triển trên nền tảng **Unity** với góc nhìn side-view 2D trong môi trường 3D.

Người chơi sẽ khám phá một thế giới liên kết với nhiều khu vực, chiến đấu với kẻ địch, thu thập và mở khóa các khả năng mới. Những khả năng này cho phép người chơi tiếp cận các khu vực trước đó chưa thể đến, từ đó tiếp tục khám phá và phát triển nhân vật.

Dự án tập trung vào việc xây dựng trải nghiệm **khám phá, chiến đấu, tương tác với môi trường và phát triển nhân vật**, đồng thời áp dụng kiến trúc phần mềm có khả năng mở rộng và bảo trì.

---

## Tính năng chính

- Khám phá thế giới với các khu vực được liên kết với nhau.
- Di chuyển và vượt chướng ngại vật trong môi trường.
- Chiến đấu với nhiều loại kẻ địch.
- Sử dụng các kỹ năng và khả năng đặc biệt.
- Mở khóa khả năng mới để tiếp cận những khu vực chưa thể khám phá.
- Tương tác với môi trường và các đối tượng trong game.
- Hệ thống camera phù hợp với gameplay side-view 2D trong môi trường 3D.
- Hệ thống nhân vật và gameplay được thiết kế theo hướng module hóa.

---

## Công nghệ sử dụng

- **Unity 6.3**
- **Universal Render Pipeline (URP)**
- **C#**
- **Unity Input System**
- **Git**
- **Git LFS**

---

## Kiến trúc dự án

Dự án được xây dựng theo hướng **module hóa**, trong đó các chức năng gameplay được chia thành những hệ thống riêng biệt nhằm giảm sự phụ thuộc giữa các thành phần và giúp việc mở rộng dự án dễ dàng hơn.

Một số phương pháp được áp dụng:

- **State Machine** – quản lý các trạng thái và hành vi của nhân vật.
- **ScriptableObject** – quản lý và lưu trữ dữ liệu gameplay.
- **Event-driven Architecture** – giao tiếp giữa các hệ thống thông qua sự kiện.
- **Dependency Injection** – truyền các đối tượng cần thiết trực tiếp khi khởi tạo.
- **Object Pooling** – tái sử dụng các đối tượng được tạo và hủy thường xuyên.
- **Interface-based Design** – giảm sự phụ thuộc trực tiếp giữa các module.
- **Caching** – hạn chế việc tìm kiếm Component lặp lại trong quá trình gameplay.
- **Tối ưu Physics Query** – sử dụng các phương pháp truy vấn vật lý phù hợp để giảm cấp phát bộ nhớ.

---

## Cấu trúc dự án

```text
Assets/
├── Core/
├── Gameplay/
├── UI/
├── Art/
├── Audio/
└── Scenes/