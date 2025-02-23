using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProtectedAPI.Model;

namespace ProtectedAPI.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
	public AppDbContext(DbContextOptions opt) : base(opt)
	{
	}

	public DbSet<Todo> Todos { get; set; } = null!;
}
