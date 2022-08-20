using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Context
{
    public class TodoDb : DbContext
    {
        public TodoDb(DbContextOptions<TodoDb> options) : base(options) => Database.EnsureCreated();

        public DbSet<Todo> Todos => Set<Todo>();
    }
}
