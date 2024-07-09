using System.Text.RegularExpressions;

List<Person> users = new List<Person>
{
    new() {Id = Guid.NewGuid().ToString(), Name = "Tom", Age = 25},
    new() {Id = Guid.NewGuid().ToString(), Name = "Bob", Age = 33},
    new() {Id = Guid.NewGuid().ToString(), Name = "Sam", Age = 44},
};

var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.Run(async (context) =>
{
    var responce = context.Response;
    var request = context.Request;
    var path = request.Path;
    
    string expressionForGuid = @"^/api/users/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

    if (path == "/api/users" && request.Method == "GET")
    {
        await GetAllPeople(responce);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "GET")
    {
        string? id = path.Value?.Split("/")[3];
        await GetPerson(id, responce);
    }
    else if (path == "/api/users" && request.Method == "POST")
    {
        await CreatePerson(responce, request);
    }
    else if (path == "/api/users" && request.Method == "PUT")
    {
        await UpdatePerson(responce, request);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split("/")[3];
        await DeletePerson(id, responce);
    }
    else
    {
        responce.ContentType = "text/html; charset=utf-8";
        await responce.SendFileAsync("html/index.html");
    }
});

app.Run();
//отримання всіх користувачів
async Task GetAllPeople(HttpResponse response)
{
    await response.WriteAsJsonAsync(users);
}
//Отримати одного користувача
async Task GetPerson(string? id, HttpResponse response)
{
    Person? user = users.FirstOrDefault((u) => u.Id == id);
    if (user != null)
        await response.WriteAsJsonAsync(user);
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new {message = "User not found"});
    }
}
//Видалення користувача
async Task DeletePerson(string? id, HttpResponse response)
{
    Person? user = users.FirstOrDefault((u) => u.Id == id);
    if (user != null)
    {
        users.Remove(user);
        await response.WriteAsJsonAsync(user);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new {message = "User not found"});
    }
}
//Створення користувача
async Task CreatePerson(HttpResponse response, HttpRequest request)
{
    try
    {
        var user = await request.ReadFromJsonAsync<Person>();
        if (user != null)
        {
            user.Id = Guid.NewGuid().ToString();
            users.Add(user);
            await response.WriteAsJsonAsync(user);
        }
        else
        {
            throw new Exception("Incorrect Data");
        }
    }
    catch(Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new {message = "Incorrect Data"});
    }
}
//Оновлення користувача
async Task UpdatePerson(HttpResponse response, HttpRequest request)
{
    try
    {
        Person? userData = await request.ReadFromJsonAsync<Person>();
        if (userData != null)
        {
            var user = users.FirstOrDefault(u => u.Id == userData.Id);
            if (user != null)
            {
                user.Age = userData.Age;
                user.Name = userData.Name;
                await response.WriteAsJsonAsync(user);
            }
            else
            {
                response.StatusCode = 400;
                await response.WriteAsJsonAsync(new { message = "User not found" });
            }
        }
        else
        {
            throw new Exception("Incorrect Data");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new {message = "Incorrect Data"});
    }
}


async Task HandlerSendJson(HttpContext context)
{
    var responce = context.Response;
    var request = context.Request;
    if (request.Path == "/api/user")
    {
        var message = "Incorrect Data";

        try
        {
            var person = await request.ReadFromJsonAsync<Person>();
            if (person != null)
                message = $"Name: {person.Name} Age: {person.Age}";
        }
        catch {}

        await responce.WriteAsJsonAsync(new {text = message});
    }
    else
    {
        responce.ContentType = "text/html; charset=utf-8";
        await responce.SendFileAsync("html/index.html");
    }
}

async Task HandleSendForm(HttpContext context)
{
    context.Request.ContentType = "text/html; charset=utf-8";

    if (context.Request.Path == "/postuser")
    {
        var form = context.Request.Form;
        string name = form["name"];
        string age = form["age"];
        await context.Response.WriteAsync($":<div><p>Name: {name}</p><p>Age: {age}</p></div>");
    }
    else
    {
        await context.Response.SendFileAsync("html/index.html");
    }
}

async Task HandleRedirect(HttpContext context)
{
    if (context.Request.Path == "/old")
    {
        context.Response.Redirect("https://mystat.itstep.org/ua/auth/login/index?returnUrl=%2Fua%2Fmain%2Fdashboard%2Fpage%2Findex");
    }
    else if (context.Request.Path == "/new")
    {
        await context.Response.WriteAsync("New Page");
    }
    else
    {
        await context.Response.WriteAsync("Main page");
    }
}

async Task HandleSendJson1(HttpContext context)
{
    // Person tom = new("Tom", 22);
    // await context.Response.WriteAsJsonAsync(tom);

    // var responce = context.Response;
    // responce.Headers.ContentType = "application/json; charset=utf-8";
    // await responce.WriteAsync("{\"name\":Tom\",\"age\":22}");
}

public class Person
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public int Age { get; set; }
}