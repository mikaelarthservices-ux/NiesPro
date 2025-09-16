using Microsoft.EntityFrameworkCore;

// Test syntax for Pomelo MySQL
var options = new DbContextOptionsBuilder()
    .UseMySql("Server=localhost;Database=test;Uid=root;Pwd=;", 
              ServerVersion.AutoDetect("Server=localhost;Database=test;Uid=root;Pwd=;"))
    .Options;

Console.WriteLine("MySQL configuration test successful!");
