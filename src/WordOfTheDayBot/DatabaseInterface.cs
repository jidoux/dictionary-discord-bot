using Microsoft.EntityFrameworkCore;
using WordOfTheDayBot.Database;

namespace WordOfTheDayBot;

public sealed class DatabaseInterface(AppDbContext db) {
	// Pass in the current hour, which is a value 0-23 in UTC time.
	public async Task<List<Server>> FindAllServersToSendForThisUTCHour(int currentHourUTC) {
		return await db.Servers.Where(s => s.TimeToSendDailyWordUTC.Hour == currentHourUTC).ToListAsync();
	}

	public async Task AddServerIfNotExists(ulong guildId) {
		Server server = new() {
			DiscordGuildId = guildId,
			TimeToSendDailyWordUTC = TimeOnly.MinValue, // arbitrary default - can be set with slashcommands, so.
		};
		if (await db.Servers.AnyAsync(s => s.DiscordGuildId == server.DiscordGuildId)) {
			// 99% use case - since this gets executed when the bot starts up, it can fail sometimes. Column also has
			// a unique constraint. TODO - is this the best way to do this? Feels subpar.
			return;
		}
		db.Servers.Add(server);
		await db.SaveChangesAsync();
	}

	public async Task DeleteServerByGuildId(ulong guildId) {
		await db.Servers.Where(s => s.DiscordGuildId == guildId).ExecuteDeleteAsync();
	}

	public async Task<bool> WasWordAlreadySentInThisServer(string word, int serverId) {
		return await db.SentWords.AnyAsync(x => x.ServerId == serverId && x.Word == word);
	}
}
