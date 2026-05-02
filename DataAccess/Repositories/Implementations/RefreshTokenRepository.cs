using DataAccess.Context;
using DataAccess.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models;

namespace DataAccess.Repositories.Implementations;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ProxarDbContext _context;

    public RefreshTokenRepository(ProxarDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenHash)
    {
        return await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task<RefreshToken?> RevokeIfActiveAsync(string tokenHash)
    {
        // SEGURIDAD: Revocación atómica de refresh token para prevenir reuso.
        //
        // Bajo concurrencia (dos requests simultáneos con mismo token):
        // - Request A ejecuta ExecuteUpdateAsync → affectedRows = 1 → retorna entidad
        // - Request B ejecuta ExecuteUpdateAsync → affectedRows = 0 (ya revocado) → retorna null
        //
        // Esto es seguro con el nivel de aislamiento por defecto de PostgreSQL (ReadCommitted):
        // - ExecuteUpdateAsync genera SQL directo (UPDATE ... WHERE RevokedAt IS NULL)
        // - PostgreSQL aplica row-level locking en la fila actualizada
        // - La escritura de A es visible inmediatamente para B
        // - Solo UN request puede actualizar la fila (affectedRows = 1)
        //
        // Con niveles más altos (RepeatableRead, Serializable) también funciona correctamente.
        var now = DateTime.UtcNow;

        var affectedRows = await _context.RefreshTokens
            .Where(t => t.TokenHash == tokenHash && t.RevokedAt == null && t.ExpiresAt > now)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(t => t.RevokedAt, now));

        if (affectedRows == 0)
            return null;

        return await _context.RefreshTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task<RefreshToken> AddAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync();
        return token;
    }

    public async Task UpdateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Update(token);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAllForUserAsync(Guid userId, Guid companyId)
    {
        var tokens = await _context.RefreshTokens
            .Include(t => t.User)
            .Where(t => t.UserId == userId && t.User.CompanyId == companyId && t.RevokedAt == null)
            .ToListAsync();

        var now = DateTime.UtcNow;
        foreach (var token in tokens)
            token.RevokedAt = now;

        await _context.SaveChangesAsync();
    }
}
