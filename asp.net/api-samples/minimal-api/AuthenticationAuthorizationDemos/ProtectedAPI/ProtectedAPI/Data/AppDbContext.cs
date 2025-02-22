using System;
using Microsoft.EntityFrameworkCore;
using ProtectedAPI.Model;

namespace ProtectedAPI.Data;

public class AppDbContext:DbContext
{
	public AppDbContext(DbContextOptions opt):base(opt)
	{
		
	}
	public DbSet<Todo> Todos { get; set; } = null!;

}
