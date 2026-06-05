using AutoMarket.Core.Entities;
using AutoMarket.Core.Interfaces;
using AutoMarket.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AutoMarket.Infrastructure.Repositories;

public class UserRepository : GenericRepository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
        => await _dbSet.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByUsernameAsync(string username)
        => await _dbSet.FirstOrDefaultAsync(u => u.Username == username);

    public async Task<bool> EmailExistsAsync(string email)
        => await _dbSet.AnyAsync(u => u.Email == email);
}
