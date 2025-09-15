namespace NiesPro.Common.Constants
{
    /// <summary>
    /// Application-wide constants
    /// </summary>
    public static class AppConstants
    {
        public const string DefaultCulture = "fr-FR";
        public const string DefaultTimeZone = "Central European Standard Time";
        
        // JWT Settings
        public const int JwtExpiryInMinutes = 60;
        public const int RefreshTokenExpiryInDays = 30;
        
        // Database Settings
        public const int DefaultPageSize = 20;
        public const int MaxPageSize = 100;
        
        // File Upload
        public const int MaxFileSize = 10 * 1024 * 1024; // 10MB
        public static readonly string[] AllowedImageTypes = { ".jpg", ".jpeg", ".png", ".gif" };
        public static readonly string[] AllowedDocumentTypes = { ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
    }

    /// <summary>
    /// User roles constants
    /// </summary>
    public static class Roles
    {
        public const string Admin = "admin";
        public const string Manager = "manager";
        public const string Cashier = "cashier";
        public const string Server = "server";
        public const string StockManager = "stock_manager";
    }

    /// <summary>
    /// Permissions constants
    /// </summary>
    public static class Permissions
    {
        // Auth permissions
        public const string AuthRead = "auth.read";
        public const string AuthWrite = "auth.write";
        
        // Product permissions
        public const string ProductRead = "product.read";
        public const string ProductWrite = "product.write";
        public const string ProductDelete = "product.delete";
        
        // Order permissions
        public const string OrderRead = "order.read";
        public const string OrderWrite = "order.write";
        public const string OrderCancel = "order.cancel";
        
        // Stock permissions
        public const string StockRead = "stock.read";
        public const string StockWrite = "stock.write";
        public const string StockInventory = "stock.inventory";
        
        // Customer permissions
        public const string CustomerRead = "customer.read";
        public const string CustomerWrite = "customer.write";
        
        // Report permissions
        public const string ReportRead = "report.read";
        public const string ReportExport = "report.export";
        
        // Admin permissions
        public const string AdminUsers = "admin.users";
        public const string AdminSettings = "admin.settings";
        public const string AdminLogs = "admin.logs";
    }
}