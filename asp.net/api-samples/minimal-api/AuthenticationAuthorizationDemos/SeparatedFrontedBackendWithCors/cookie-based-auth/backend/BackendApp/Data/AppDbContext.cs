using System;
using BackendApp.Models;
using Microsoft.EntityFrameworkCore;

namespace BackendApp.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    public DbSet<User> Users => Set<User>();
}
