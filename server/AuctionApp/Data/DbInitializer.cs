using AuctionApp.Models;
using Microsoft.EntityFrameworkCore;

namespace AuctionApp.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.MigrateAsync();

        if (await db.Lots.AnyAsync()) return;

        var now = DateTime.UtcNow;

        var hash = BCrypt.Net.BCrypt.HashPassword("Password1!");
        foreach (var user in db.Users)
            user.PasswordHash = hash;
        await db.SaveChangesAsync();

        var activeLots = new List<Lot>
        {
            new()
            {
                Title = "Вінтажна фотокамера «Зеніт-Е»",
                Description = "Радянська плівкова камера 1970-х. Стан відмінний, є оригінальний шкіряний чохол та ремінь.",
                ImagePath = "/uploads/lots/lot-01.jpg",
                StartingPrice = 800,
                CurrentPrice = 800,
                SellerId = 2,
                StatusId = 1,
                EndsAt = now.AddDays(5),
                CreatedAt = now.AddDays(-1)
            },
            new()
            {
                Title = "Картина олією «Карпатський світанок»",
                Description = "Авторська робота, полотно 70×50 см. Підписана художником, є сертифікат.",
                ImagePath = "/uploads/lots/lot-03.jpg",
                StartingPrice = 3500,
                CurrentPrice = 3500,
                SellerId = 4,
                StatusId = 1,
                EndsAt = now.AddDays(7),
                CreatedAt = now.AddDays(-1)
            },
            new()
            {
                Title = "Золотий перстень із рубіном",
                Description = "585 проба, рубін 0.3 карат. Повний комплект: коробка, сертифікат геммолога.",
                ImagePath = "/uploads/lots/lot-06.jpg",
                StartingPrice = 7500,
                CurrentPrice = 7500,
                SellerId = 3,
                StatusId = 1,
                EndsAt = now.AddDays(8),
                CreatedAt = now.AddDays(-1)
            },
            new()
            {
                Title = "Дубовий книжковий стелаж",
                Description = "Ручна робота, масив дуба. Розміри: 180×80×30 см. Самовивіз Київ.",
                ImagePath = "/uploads/lots/lot-08.jpg",
                StartingPrice = 4200,
                CurrentPrice = 4200,
                SellerId = 5,
                StatusId = 1,
                EndsAt = now.AddDays(10),
                CreatedAt = now.AddDays(-1)
            },
            new()
            {
                Title = "«Кобзар» Т.Г. Шевченка (1964 р.)",
                Description = "Ювілейне видання, тверда обкладинка. Стан добрий, без підкреслень і написів.",
                ImagePath = null,
                StartingPrice = 2200,
                CurrentPrice = 2200,
                SellerId = 5,
                StatusId = 1,
                EndsAt = now.AddDays(6),
                CreatedAt = now.AddDays(-2)
            },
            new()
            {
                Title = "Гірський велосипед Trek Marlin 7",
                Description = "Алюмінієва рама L, колеса 29\". Куплено рік тому, пробіг ~300 км. Є чохол.",
                ImagePath = null,
                StartingPrice = 12000,
                CurrentPrice = 12000,
                SellerId = 2,
                StatusId = 1,
                EndsAt = now.AddDays(5),
                CreatedAt = now.AddDays(-2)
            },
        };

        var soldLots = new List<Lot>
        {
            new()
            {
                Title = "Ноутбук ASUS VivoBook 15",
                Description = "AMD Ryzen 5 5600H, 16 ГБ RAM, SSD 512 ГБ. Без подряпин, у комплекті зарядний пристрій.",
                ImagePath = "/uploads/lots/lot-02.jpg",
                StartingPrice = 14000,
                CurrentPrice = 16500,
                SellerId = 3,
                StatusId = 2,
                EndsAt = now.AddDays(-2),
                CreatedAt = now.AddDays(-10)
            },
            new()
            {
                Title = "Набір поштових марок України (1992–2000)",
                Description = "Понад 200 марок першого дня випуску. Всі в ідеальному стані.",
                ImagePath = "/uploads/lots/lot-04.jpg",
                StartingPrice = 1200,
                CurrentPrice = 1800,
                SellerId = 5,
                StatusId = 2,
                EndsAt = now.AddDays(-3),
                CreatedAt = now.AddDays(-12)
            },
            new()
            {
                Title = "Кросівки Nike Air Force 1 (42 розмір)",
                Description = "Дедсток, ніколи не носились. Оригінальна коробка та бирки.",
                ImagePath = "/uploads/lots/lot-05.jpg",
                StartingPrice = 2800,
                CurrentPrice = 3400,
                SellerId = 2,
                StatusId = 2,
                EndsAt = now.AddDays(-1),
                CreatedAt = now.AddDays(-8)
            },
        };

        var cancelledLots = new List<Lot>
        {
            new()
            {
                Title = "Зламана PlayStation 4 Slim",
                Description = "Тільки на запчастини. Не вмикається після падіння.",
                ImagePath = null,
                StartingPrice = 800,
                CurrentPrice = 800,
                SellerId = 4,
                StatusId = 3,
                EndsAt = now.AddDays(-1),
                CreatedAt = now.AddDays(-5)
            },
        };

        db.Lots.AddRange(activeLots);
        db.Lots.AddRange(soldLots);
        db.Lots.AddRange(cancelledLots);
        await db.SaveChangesAsync();

        var tagMap = await db.Tags.ToDictionaryAsync(t => t.Id);
        void AddTags(Lot lot, params int[] ids)
        {
            foreach (var id in ids) lot.Tags.Add(tagMap[id]);
        }

        AddTags(activeLots[0], 1, 5);
        AddTags(activeLots[1], 3);
        AddTags(activeLots[2], 8, 5);
        AddTags(activeLots[3], 7);
        AddTags(activeLots[4], 2, 5);
        AddTags(activeLots[5], 9, 6);

        AddTags(soldLots[0], 1);
        AddTags(soldLots[1], 5, 2);
        AddTags(soldLots[2], 4, 9);

        AddTags(cancelledLots[0], 1, 10);

        await db.SaveChangesAsync();

        var activeBids = new List<Bid>
        {
            new() { LotId = activeLots[0].Id, BidderId = 3, Amount = 850, PlacedAt = now.AddHours(-10) },
            new() { LotId = activeLots[0].Id, BidderId = 4, Amount = 950, PlacedAt = now.AddHours(-8) },
            new() { LotId = activeLots[0].Id, BidderId = 5, Amount = 1100, PlacedAt = now.AddHours(-3) },

            new() { LotId = activeLots[1].Id, BidderId = 2, Amount = 3700, PlacedAt = now.AddHours(-12) },
            new() { LotId = activeLots[1].Id, BidderId = 5, Amount = 4200, PlacedAt = now.AddHours(-6) },

            new() { LotId = activeLots[2].Id, BidderId = 2, Amount = 7800, PlacedAt = now.AddHours(-14) },
            new() { LotId = activeLots[2].Id, BidderId = 5, Amount = 8500, PlacedAt = now.AddHours(-8) },
            new() { LotId = activeLots[2].Id, BidderId = 4, Amount = 9200, PlacedAt = now.AddHours(-3) },

            new() { LotId = activeLots[3].Id, BidderId = 3, Amount = 4400, PlacedAt = now.AddHours(-7) },
            new() { LotId = activeLots[3].Id, BidderId = 2, Amount = 4800, PlacedAt = now.AddHours(-3) },

            new() { LotId = activeLots[4].Id, BidderId = 4, Amount = 2400, PlacedAt = now.AddHours(-13) },
            new() { LotId = activeLots[4].Id, BidderId = 3, Amount = 2700, PlacedAt = now.AddHours(-5) },

            new() { LotId = activeLots[5].Id, BidderId = 3, Amount = 12500, PlacedAt = now.AddHours(-16) },
            new() { LotId = activeLots[5].Id, BidderId = 4, Amount = 13200, PlacedAt = now.AddHours(-10) },
            new() { LotId = activeLots[5].Id, BidderId = 5, Amount = 14000, PlacedAt = now.AddHours(-4) },
        };

        var soldBids = new List<Bid>
        {
            new() { LotId = soldLots[0].Id, BidderId = 2, Amount = 14500, PlacedAt = now.AddDays(-8) },
            new() { LotId = soldLots[0].Id, BidderId = 4, Amount = 15500, PlacedAt = now.AddDays(-6) },
            new() { LotId = soldLots[0].Id, BidderId = 5, Amount = 16500, PlacedAt = now.AddDays(-4) },

            new() { LotId = soldLots[1].Id, BidderId = 3, Amount = 1300, PlacedAt = now.AddDays(-10) },
            new() { LotId = soldLots[1].Id, BidderId = 2, Amount = 1800, PlacedAt = now.AddDays(-7) },

            new() { LotId = soldLots[2].Id, BidderId = 3, Amount = 2900, PlacedAt = now.AddDays(-6) },
            new() { LotId = soldLots[2].Id, BidderId = 4, Amount = 3400, PlacedAt = now.AddDays(-3) },
        };

        db.Bids.AddRange(activeBids);
        db.Bids.AddRange(soldBids);
        await db.SaveChangesAsync();

        activeLots[0].CurrentPrice = 1100;
        activeLots[1].CurrentPrice = 4200;
        activeLots[2].CurrentPrice = 9200;
        activeLots[3].CurrentPrice = 4800;
        activeLots[4].CurrentPrice = 2700;
        activeLots[5].CurrentPrice = 14000;

        soldLots[0].WinnerBidId = soldBids[2].Id;
        soldLots[1].WinnerBidId = soldBids[4].Id;
        soldLots[2].WinnerBidId = soldBids[6].Id;

        await db.SaveChangesAsync();
    }
}
