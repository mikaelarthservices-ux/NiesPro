using System.Net;
using System.Text;
using Newtonsoft.Json;

class Program 
{
    static async Task Main()
    {
        var client = new HttpClient();
        client.BaseAddress = new Uri("http://localhost:5000");
        
        var registerRequest = new
        {
            Username = "newuser",
            Email = "newuser@example.com", 
            Password = "NewPassword123!",
            ConfirmPassword = "NewPassword123!",
            FirstName = "New",
            LastName = "User",
            PhoneNumber = "+1234567890",
            DeviceKey = "new-device-key",
            DeviceName = "New Device"
        };

        var json = JsonConvert.SerializeObject(registerRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        Console.WriteLine("Sending request:");
        Console.WriteLine(json);
        
        try 
        {
            var response = await client.PostAsync("/api/v1/auth/register", content);
            Console.WriteLine($"Status: {response.StatusCode}");
            
            var responseContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Response: {responseContent}");
        } 
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }
}