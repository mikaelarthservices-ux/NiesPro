using System;

class Program 
{
    static void Main()
    {
        string password = "password123";
        
        // Test avec work factor 11 (comme dans le test)
        string hash11 = BCrypt.Net.BCrypt.HashPassword(password, 11);
        Console.WriteLine($"Hash avec work factor 11: {hash11}");
        
        // Test avec work factor 12 (comme dans PasswordService)
        string hash12 = BCrypt.Net.BCrypt.HashPassword(password, 12);
        Console.WriteLine($"Hash avec work factor 12: {hash12}");
        
        // Vérifier le hash existant
        string existingHash = "$2a$11$8oJ5Y7WzGJTq8DZBhp.b6OxbCqE4y6pLgDrJ1HEhH8QxNzU3zXzZa";
        bool isValid = BCrypt.Net.BCrypt.Verify(password, existingHash);
        Console.WriteLine($"Le hash existant est valide: {isValid}");
        
        // Vérifier avec les nouveaux hashs
        bool isValid11 = BCrypt.Net.BCrypt.Verify(password, hash11);
        bool isValid12 = BCrypt.Net.BCrypt.Verify(password, hash12);
        Console.WriteLine($"Hash 11 est valide: {isValid11}");
        Console.WriteLine($"Hash 12 est valide: {isValid12}");
    }
}