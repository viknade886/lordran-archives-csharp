using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

partial class Program
{
    static readonly HttpClient client = new HttpClient();
    static readonly string API = "https://localhost:7024/api";
    static string? token = null;
    static string? role = null;
    static string? username = null;

    static async Task Main(string[] args)
    {
        // Ignorirane na ssl certificati za local host
        var handler = new HttpClientHandler();
        handler.ServerCertificateCustomValidationCallback = (m, c, ch, e) => true;
        var httpClient = new HttpClient(handler);
        client.BaseAddress = new Uri(API);

        Console.WriteLine("<=========================================>");
        Console.WriteLine("      <LORDRAN ARCHIVES - CONSOLE>         ");
        Console.WriteLine("<=========================================>");

        while (true)
        {
            Console.WriteLine("\n--- MAIN MENU ---");
            if (token == null)
            {
                Console.WriteLine("1. Browse Items");
                Console.WriteLine("2. Login");
                Console.WriteLine("3. Register");
                Console.WriteLine("0. Exit");
            }
            else
            {
                Console.WriteLine($"Logged in as: {username} ({role})");
                Console.WriteLine("1. Browse Items");
                Console.WriteLine("2. Submit Item");
                if (role == "admin" || role == "operator")
                {
                    Console.WriteLine("3. View Pending Items");
                    Console.WriteLine("4. Approve Item");
                    Console.WriteLine("5. Delete Item");
                }
                Console.WriteLine("6. Logout");
                Console.WriteLine("0. Exit");
            }

            Console.Write("\nChoice: ");
            var choice = Console.ReadLine();

            if (token == null)
            {
                switch (choice)
                {
                    case "1": await BrowseItems(); break;
                    case "2": await Login(); break;
                    case "3": await Register(); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
            else
            {
                switch (choice)
                {
                    case "1": await BrowseItems(); break;
                    case "2": await SubmitItem(); break;
                    case "3":
                        if (role == "admin" || role == "operator") await ViewPending();
                        break;
                    case "4":
                        if (role == "admin" || role == "operator") await ApproveItem();
                        break;
                    case "5":
                        if (role == "admin" || role == "operator") await DeleteItem();
                        break;
                    case "6": Logout(); break;
                    case "0": return;
                    default: Console.WriteLine("Invalid choice."); break;
                }
            }
        }
    }

    static async Task BrowseItems()
    {
        using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
        using var http = new HttpClient(handler);

        Console.WriteLine("\n--- BROWSE ITEMS ---");
        Console.WriteLine("1. All Items");
        Console.WriteLine("2. By Category");
        Console.Write("Choice: ");
        var choice = Console.ReadLine();

        string url = $"{API}/items";
        if (choice == "2")
        {
            Console.Write("Category (weapon/armor/ring/consumable/boss_weapon/covenant): ");
            var cat = Console.ReadLine();
            url = $"{API}/items/category/{cat}";
        }

        var items = await http.GetFromJsonAsync<List<ItemResponse>>(url);
        if (items == null || items.Count == 0)
        {
            Console.WriteLine("No items found.");
            return;
        }

        foreach (var item in items)
        {
            Console.WriteLine($"\n[{item.Id}] {item.Name} ({item.Category})");
            Console.WriteLine($"    {item.Description}");
            Console.WriteLine($"    Submitted by: {item.SubmittedBy}");
        }
    }

    static async Task Login()
    {
        Console.Write("Username: ");
        var username_input = Console.ReadLine();
        Console.Write("Password: ");
        var password = Console.ReadLine();

        using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
        using var http = new HttpClient(handler);

        var res = await http.PostAsJsonAsync($"{API}/auth/login", new { username = username_input, password });
        if (!res.IsSuccessStatusCode)
        {
            Console.WriteLine("Login failed. Check your credentials.");
            return;
        }

        var data = await res.Content.ReadFromJsonAsync<AuthResponse>();
        token = data?.Token;
        role = data?.Role;
        username = data?.Username;
        Console.WriteLine($"Welcome back, {username}!");
    }

    static async Task Register()
    {
        Console.Write("Username: ");
        var username_input = Console.ReadLine();
        Console.Write("Password: ");
        var password = Console.ReadLine();

        using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
        using var http = new HttpClient(handler);

        var res = await http.PostAsJsonAsync($"{API}/auth/register", new { username = username_input, password });
        if (res.IsSuccessStatusCode)
            Console.WriteLine("Registered successfully! You can now login.");
        else
            Console.WriteLine("Registration failed. Username may already exist.");
    }

    static async Task SubmitItem()
    {
        Console.Write("Item Name: ");
        var name = Console.ReadLine();
        Console.Write("Category (weapon/armor/ring/consumable/boss_weapon/covenant): ");
        var category = Console.ReadLine();
        Console.Write("Description: ");
        var description = Console.ReadLine();
        Console.Write("Image URL (optional, press Enter to skip): ");
        var image = Console.ReadLine();

        using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
        using var http = new HttpClient(handler);
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var res = await http.PostAsJsonAsync($"{API}/items", new { name, category, description, image = string.IsNullOrEmpty(image) ? null : image });
        if (res.IsSuccessStatusCode)
            Console.WriteLine("Item submitted successfully! Awaiting approval.");
        else
            Console.WriteLine("Failed to submit item.");
    }

    static async Task ViewPending()
    {
        using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
        using var http = new HttpClient(handler);
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var items = await http.GetFromJsonAsync<List<ItemResponse>>($"{API}/items/pending");
        if (items == null || items.Count == 0)
        {
            Console.WriteLine("No pending items.");
            return;
        }

        Console.WriteLine("\n--- PENDING ITEMS ---");
        foreach (var item in items)
        {
            Console.WriteLine($"\n[{item.Id}] {item.Name} ({item.Category})");
            Console.WriteLine($"    {item.Description}");
            Console.WriteLine($"    Submitted by: {item.SubmittedBy}");
        }
    }

    static async Task ApproveItem()
    {
        Console.Write("Enter Item ID to approve: ");
        var id = Console.ReadLine()?.Trim();

        using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
        using var http = new HttpClient(handler);
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new HttpRequestMessage(HttpMethod.Put, $"{API}/items/approve/{id}");
        request.Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json");
        var res = await http.SendAsync(request);

        if (res.IsSuccessStatusCode)
            Console.WriteLine("Item approved!");
        else
        {
            var body = await res.Content.ReadAsStringAsync();
            Console.WriteLine($"Failed. Status: {res.StatusCode}, Body: {body}");
        }
    }

    static async Task DeleteItem()
    {
        Console.Write("Enter Item ID to delete: ");
        var id = Console.ReadLine();

        using var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (m, c, ch, e) => true };
        using var http = new HttpClient(handler);
        http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var res = await http.DeleteAsync($"{API}/items/{id}");
        if (res.IsSuccessStatusCode)
            Console.WriteLine("Item deleted!");
        else
            Console.WriteLine("Failed to delete item.");
    }

    static void Logout()
    {
        token = null;
        role = null;
        username = null;
        Console.WriteLine("Logged out.");
    }
}

class ItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Image { get; set; }
    public string SubmittedBy { get; set; } = string.Empty;
}

class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}