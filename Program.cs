
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using TodoApi;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddDbContext<ToDoDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("ToDoDB"), new MySqlServerVersion(new Version(8, 0, 36))));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();




// Configure CORS policy
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    options.RoutePrefix = string.Empty;
});

app.UseHttpsRedirection();

app.UseRouting();

// Enable CORS
app.UseCors();

app.UseAuthorization();

app.MapControllers();


// Fetching all tasks
app.MapGet("/tasks", async (ToDoDbContext context) =>
{
    var tasks = await context.Items.ToListAsync();
    return tasks;
});

// Adding a new task
app.MapPost("/tasks", async (ToDoDbContext context, Item item) =>
{
    context.Items.Add(item);
    await context.SaveChangesAsync();
    return Results.Created($"/tasks/{item.Id}", item);
});

// Updating a task
app.MapPut("/tasks/{id}", async (int id, Item item, [FromServices] ToDoDbContext context) =>
{
    var existingItem = await context.Items.FindAsync(id);
    if (existingItem == null) return Results.NotFound();

    existingItem.Name = item.Name;
    existingItem.IsComplete = item.IsComplete;
    await context.SaveChangesAsync();

    return Results.NoContent();
});

// Deleting a task
app.MapDelete("/tasks/{id}", async (ToDoDbContext context, int id) =>
{
    var existingItem = await context.Items.FindAsync(id);
    if (existingItem == null)
    {
        return Results.NotFound();
    }

    context.Items.Remove(existingItem);
    await context.SaveChangesAsync();

    return Results.NoContent();
});

app.Run();