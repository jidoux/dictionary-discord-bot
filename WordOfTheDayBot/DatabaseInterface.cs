using Microsoft.EntityFrameworkCore;
using WordOfTheDayBot.Database;

namespace WordOfTheDayBot;

internal sealed class DatabaseInterface(IDbContextFactory<AppDbContext> contextFactory) {
	public async Task<List<Server>> FindAllServersToSendForThisUTCHour(CancellationToken stoppingToken) {
		int currentHourUTC = DateTime.UtcNow.Hour;
		await using AppDbContext db = await contextFactory.CreateDbContextAsync(stoppingToken);
		return await db.Servers.Where(s => s.HourToSendDailyWordUTC == currentHourUTC).ToListAsync(stoppingToken);
	}

	public async Task AddServerIfNotExists(ulong guildId, CancellationToken stoppingToken) {
		await using AppDbContext db = await contextFactory.CreateDbContextAsync(stoppingToken);

		Server server = new() {
			DiscordGuildId = guildId,
			HourToSendDailyWordUTC = 12, // Arbitrary chosen time.. I figure its fine.
		};
		if (await db.Servers.AnyAsync(s => s.DiscordGuildId == server.DiscordGuildId, stoppingToken)) {
			// 99% use case - since this gets executed when the bot starts up, it can fail sometimes. Column also has
			// a unique constraint. TODO - is this the best way to do this? Feels subpar.
			return;
		}
		db.Servers.Add(server);
		await db.SaveChangesAsync(stoppingToken);
	}

	public async Task DeleteServerByGuildId(ulong guildId, CancellationToken stoppingToken) {
		await using AppDbContext db = await contextFactory.CreateDbContextAsync(stoppingToken);

		await db.Servers.Where(s => s.DiscordGuildId == guildId).ExecuteDeleteAsync(stoppingToken);
	}

	public async Task<bool> WasWordAlreadySentInThisServer(string word, int serverId, CancellationToken stoppingToken) {
		await using AppDbContext db = await contextFactory.CreateDbContextAsync(stoppingToken);

		return await db.SentWords.AnyAsync(x => x.ServerId == serverId && x.Word == word, stoppingToken);
	}

	public async Task AddSentWordToServer(string word, int serverId, CancellationToken stoppingToken) {
		await using AppDbContext db = await contextFactory.CreateDbContextAsync(stoppingToken);

		SentWord sentWord = new() { Word = word, ServerId = serverId };
		db.SentWords.Add(sentWord);
		await db.SaveChangesAsync(stoppingToken);
	}

	public async Task SaveServerSettings(ulong guildId, ulong channelId, int hourUtc, CancellationToken stoppingToken) {
		await using AppDbContext db = await contextFactory.CreateDbContextAsync(stoppingToken);

		await db.Servers
			.Where(s => s.DiscordGuildId == guildId)
			.ExecuteUpdateAsync(s => s
				.SetProperty(x => x.DiscordChannelIdToSendWordsTo, channelId)
				.SetProperty(x => x.HourToSendDailyWordUTC, hourUtc), stoppingToken);
	}

	public async Task InsertError(Exception ex, string? additionalMessage, CancellationToken stoppingToken) {
		await using AppDbContext db = await contextFactory.CreateDbContextAsync(stoppingToken);

		Error errorToAdd = new() { Exception = ex.ToString(), AdditionalMessage = additionalMessage };

		db.Errors.Add(errorToAdd);
		await db.SaveChangesAsync(stoppingToken);
	}
}
