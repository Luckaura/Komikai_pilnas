using Komikai_pilnas.Datat;
using Komikai_pilnas.Datat.Entities;
using Komikai_pilnas.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Komikai_pilnas.Auth
{
    public class SessionService(ForumDbContext dbContext)
    {
        public async Task CreateSessionAsync(Guid sessionId, string userid, string refreshToken, DateTime expiresAt)
        {
            dbContext.Sessions.Add(new Session
            {
                Id = sessionId,
                UserId = userid,
                InitiatedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                LastRefreshToken = refreshToken.ToSHA256()
            });

            await dbContext.SaveChangesAsync();
        }

        public async Task ExtendSessionAsync(Guid sessionId, string refreshToken, DateTime expiresAt)
        {

            var session = await dbContext.Sessions.FindAsync(sessionId);
            session.ExpiresAt = expiresAt;
            session.LastRefreshToken = refreshToken.ToSHA256();
         
            await dbContext.SaveChangesAsync();
        }

        public async Task InvalidSessionAsync(Guid sessionId)
        {

            var session = await dbContext.Sessions.FindAsync(sessionId);
            if (session is null)
            {
                return;
            };

            session.IsRevoked = true;

            await dbContext.SaveChangesAsync();
        }


        public async Task<bool> IsSessionValidAsync(Guid sessionId, string refreshToken)
        {

            var session = await dbContext.Sessions.FindAsync(sessionId);
            return session is not null && session.ExpiresAt >DateTimeOffset.UtcNow && !session.IsRevoked && session.LastRefreshToken == refreshToken.ToSHA256();
        }
    }
}
