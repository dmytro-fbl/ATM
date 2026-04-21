using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ATM.Domain.Entities;
using ATM.Domain.Interfaces.Services;
using ATM.Infrastructure.Services;

namespace ATM.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task SeedDataAsync(AppDbContext context, IPasswordHasher passwordHasher)
        {
            if (context.Cards.Any())
                return;
            var userId = Guid.NewGuid();
            var accountId = Guid.NewGuid();

            User testUser = new User
            {
                Id = userId,
                FirstName = "Дмитро",
                LastName = "Руденко",
                PhoneNumber = "0688600206"
            };

            Account testAccount = new Account
            {
                Id = accountId,
                UserId = userId,
                Balance = 50000.0m,
                Currency = "USD"
            };

            Card testCard = new Card
            {
                Id = Guid.NewGuid(),
                AccountId = accountId,
                CardNumber = "4444555566667770",
                PinHash = passwordHasher.HashPassword("4422")
            };

            Card receiverCard = new Card
            {
                Id = Guid.NewGuid(),
                AccountId = accountId, 
                CardNumber = "4444333322221111",
                PinHash = passwordHasher.HashPassword("2244")
            };

            await context.Cards.AddAsync(receiverCard);

            AtmCassette AtmCassette_1000 = new AtmCassette
            {
                Count = 50,
                Denomination = 1000
            };
            AtmCassette AtmCassette_500 = new AtmCassette
            {
                Count = 100,
                Denomination = 500
            };
            AtmCassette AtmCassette_200 = new AtmCassette
            {
                Count = 200,
                Denomination = 200
            };
            AtmCassette AtmCassette_100 = new AtmCassette
            {
                Count = 500,
                Denomination = 100
            };

            List<AtmCassette> listCassette = new List<AtmCassette>();
            listCassette.Add(AtmCassette_100);
            listCassette.Add(AtmCassette_200);
            listCassette.Add(AtmCassette_500);
            listCassette.Add(AtmCassette_1000);

            await context.Users.AddAsync(testUser);
            await context.Accounts.AddAsync(testAccount);
            await context.Cards.AddAsync(testCard);
            await context.AtmCassettes.AddRangeAsync(listCassette);

            await context.SaveChangesAsync();
        }
    }
}
