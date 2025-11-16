# 🌸 Shefaaf Foods & Spices - E-commerce API

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=for-the-badge&logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-8.0-512BD4?style=for-the-badge&logo=dotnet)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server)

Premium Kashmir Saffron, Dry Fruits & Authentic Spices E-commerce Backend API.

## 📋 Features

### 🔐 **Authentication & Authorization**
- JWT-based authentication
- Role-based access control (Admin, Customer)
- Secure password hashing with BCrypt
- Password reset functionality

### 🛍️ **Product Management**
- Full CRUD operations
- Category-based filtering
- Search functionality
- Image upload with Cloudinary
- Stock management
- Product ratings & reviews

### 🛒 **Shopping Cart**
- Add/Remove products
- Update quantities
- View cart with product details
- Clear cart

### 📦 **Order System**
- Place orders
- Track order status
- Order history
- Order cancellation (customer)
- Order status management (admin)

### ⭐ **Reviews & Ratings**
- 5-star rating system
- Customer reviews
- Verified purchase badges
- Average rating calculation

### ❤️ **Wishlist**
- Add/Remove products
- View wishlist
- Move items to cart

### 🎟️ **Discount Coupons**
- Create discount codes
- Percentage or fixed amount discounts
- Usage limits
- Expiry dates
- Minimum order requirements

### 📊 **Admin Dashboard**
- Sales analytics
- User management
- Order management
- Product inventory
- Low stock alerts

---

## 🚀 Tech Stack

- **Framework:** ASP.NET Core 8.0
- **Language:** C# 12
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** JWT Bearer Tokens
- **Image Storage:** Cloudinary
- **Email:** SMTP (Gmail)
- **Password Hashing:** BCrypt.Net

---

## 📁 Project Structure

\\\
ShefaafAPI/
├── Controllers/v1/     # API endpoints
├── Models/            # Data models
├── Services/          # Business logic
├── Data/              # Database context
├── Middleware/        # Custom middleware
└── Program.cs         # Entry point
\\\

---

## ⚙️ Setup Instructions

### Prerequisites
- .NET 8.0 SDK
- SQL Server
- Cloudinary Account (for images)
- Gmail Account (for emails)

### Installation

1. **Clone the repository**
\\\ash
git clone https://github.com/Mehranmanzoor/ShefaafApi.git
cd ShefaafAPI
\\\

2. **Configure appsettings.json**
\\\ash
cp appsettings.example.json appsettings.json
\\\

Edit \ppsettings.json\ with your:
- Database connection string
- JWT secret key
- Cloudinary credentials
- Email SMTP settings

3. **Install dependencies**
\\\ash
dotnet restore
\\\

4. **Run migrations**
\\\ash
dotnet ef database update
\\\

5. **Run the API**
\\\ash
dotnet run
\\\

API will be available at: \http://localhost:5245\

---

## 🔑 API Endpoints

### **Authentication**
- \POST /v1/User/Register\ - Register new user
- \POST /v1/User/Login\ - User login
- \POST /v1/User/Forgot-pass\ - Password reset
- \POST /v1/User/Change-password\ - Change password

### **Products**
- \GET /v1/Product/All\ - Get all products
- \GET /v1/Product/{id}\ - Get product by ID
- \GET /v1/Product/Category/{category}\ - Filter by category
- \GET /v1/Product/Search?q={query}\ - Search products
- \POST /v1/Product/Create\ - Create product (Admin)
- \PUT /v1/Product/Update/{id}\ - Update product (Admin)
- \DELETE /v1/Product/Delete/{id}\ - Delete product (Admin)

### **Cart**
- \POST /v1/Cart/Add\ - Add to cart
- \GET /v1/Cart/View\ - View cart
- \PUT /v1/Cart/Update\ - Update quantity
- \DELETE /v1/Cart/Remove\ - Remove item
- \DELETE /v1/Cart/Clear\ - Clear cart

### **Orders**
- \POST /v1/Order/Place\ - Place order
- \GET /v1/Order/MyOrders\ - User orders
- \GET /v1/Order/Details/{id}\ - Order details
- \POST /v1/Order/Cancel/{id}\ - Cancel order
- \PUT /v1/Order/UpdateStatus/{id}\ - Update status (Admin)
- \GET /v1/Order/All\ - All orders (Admin)

### **Reviews**
- \POST /v1/Review/Add\ - Add review
- \GET /v1/Review/Product/{id}\ - Product reviews
- \DELETE /v1/Review/Delete/{id}\ - Delete review

### **Wishlist**
- \POST /v1/Wishlist/Add\ - Add to wishlist
- \GET /v1/Wishlist/View\ - View wishlist
- \DELETE /v1/Wishlist/Remove\ - Remove item
- \POST /v1/Wishlist/MoveToCart\ - Move to cart

### **Coupons**
- \POST /v1/Coupon/Create\ - Create coupon (Admin)
- \POST /v1/Coupon/Apply\ - Apply coupon
- \GET /v1/Coupon/All\ - View all coupons (Admin)

### **Admin Dashboard**
- \GET /v1/Admin/Dashboard/Stats\ - Dashboard statistics
- \GET /v1/Admin/Users/All\ - All users
- \POST /v1/Admin/Products/UploadImage\ - Upload image

---

## 🔒 Security Features

- ✅ JWT authentication
- ✅ Password hashing (BCrypt)
- ✅ Role-based authorization
- ✅ CORS configuration
- ✅ Input validation
- ✅ SQL injection protection (EF Core)

---

## 📦 NuGet Packages

\\\xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" />
<PackageReference Include="BCrypt.Net-Next" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" />
<PackageReference Include="CloudinaryDotNet" />
<PackageReference Include="MailKit" />
\\\

---

## 🌐 Deployment

### Deploy to Azure
\\\ash
az webapp up --name shefaaf-api --resource-group ShefaafRG
\\\

### Deploy to Railway
1. Push to GitHub
2. Connect Railway to your repo
3. Add environment variables
4. Deploy!

---

## 👨‍💻 Author

**Shefaaf Foods & Spices**
- Website: [Coming Soon]
- Location: Kashmir, India

---

## 📄 License

This project is private and proprietary.

---

## 🤝 Contributing

This is a private project. Contributions are not accepted at this time.

---

## 📞 Support

For support, email: support@shefaaf.com

---

Made with ❤️ in Kashmir
