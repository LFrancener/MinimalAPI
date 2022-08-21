using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Annotations;
using TodoApi.Context;
using TodoApi.DTO.Commands;
using TodoApi.DTO.ViewModels;
using TodoApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEntityFrameworkNpgsql()
    .AddDbContext<TodoDb>(options =>
        options.UseNpgsql("Host=localhost;Port=5432;Pooling=true;Database=MinimalApiDatabase;User Id=postgres;Password=admin;"));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "TodoApi", Description = "Minimal API Sample", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("./v1/swagger.json", "TodoApi v1");
    });
}

app.MapGet("/todoitems", async (TodoDb db) =>
    await db.Todos.Select(t => new TodoItemViewModel(t)).ToListAsync())
    .WithMetadata(new SwaggerOperationAttribute(summary: "Get all Todo items"));

app.MapGet("/todoitems/complete", async (TodoDb db) =>
    await db.Todos.Where(t => t.IsComplete).Select(t => new TodoItemViewModel(t)).ToListAsync())
    .WithMetadata(new SwaggerOperationAttribute(summary: "Get all completed Todo items"));

app.MapGet("/todoitems/{id}", async (Guid id, TodoDb db) =>
    await db.Todos.FindAsync(id)
        is Todo todo
            ? Results.Ok(new TodoItemViewModel(todo))
            : Results.NotFound())
    .WithMetadata(new SwaggerOperationAttribute(summary: "Get Todo item by Id"));

app.MapPost("/todoitems", async (SaveOrUpdateTodoItemCommand command, TodoDb db) =>
{
    var todoItem = new Todo()
    {
        Id = Guid.NewGuid(),
        Name = command.Name,
        IsComplete = command.IsComplete
    };

    db.Todos.Add(todoItem);
    await db.SaveChangesAsync();

    Results.Created($"/todoitems/{todoItem.Id}", new TodoItemViewModel(todoItem));
})
.WithMetadata(new SwaggerOperationAttribute(summary: "Insert a Todo item"));

app.MapPut("/todoitems/{id}", async (Guid id, SaveOrUpdateTodoItemCommand command, TodoDb db) =>
{
    var todo = await db.Todos.FindAsync(id);

    if (todo is null) return Results.NotFound();

    todo.Name = command.Name;
    todo.IsComplete = command.IsComplete;

    await db.SaveChangesAsync();

    return Results.NoContent();
})
.WithMetadata(new SwaggerOperationAttribute(summary: "Edit a Todo item"));

app.MapDelete("/todoitems/{id}", async (Guid id, TodoDb db) =>
{
    if (await db.Todos.FindAsync(id) is Todo todo)
    {
        db.Todos.Remove(todo);
        await db.SaveChangesAsync();
        return Results.Ok(new TodoItemViewModel(todo));
    }

    return Results.NotFound();
})
.WithMetadata(new SwaggerOperationAttribute(summary: "Delete a Todo item"));

app.Run();
