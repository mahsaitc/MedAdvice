using System;
using System.Linq;
using System.Threading.Tasks;
using MedAdvice.Areas.Customer.Controllers;
using MedAdvice.Areas.Identity.Data;
using MedAdvice.Models;
using MedAdvice.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace MedAdvice.Tests.Cart
{
    /// Regression cover for C4. Before the fix, the cart endpoints took an item id straight
    /// from the request and never checked who owned it, so any signed-in visitor could edit
    /// or delete another user's basket by guessing integers.
    public class CartOwnershipTests : IDisposable
    {
        readonly TestDatabase database = new TestDatabase();
        readonly IdentityTestHost identity;

        ApplicationUser owner;
        ApplicationUser intruder;
        int ownersItemId;

        public CartOwnershipTests()
        {
            identity = new IdentityTestHost(database);
        }

        async Task SeedAsync(bool cartOpen = true)
        {
            owner = await identity.CreateUserAsync("owner@test.local");
            intruder = await identity.CreateUserAsync("intruder@test.local");

            // Product has required foreign keys to Brand and ProductCategory, and SQLite
            // enforces them, so both have to exist first.
            Brand brand = new Brand { name = "TestBrand" };
            ProductCategory category = new ProductCategory { CategoryName = "TestCategory" };
            identity.Db.Add(brand);
            identity.Db.Add(category);
            await identity.Db.SaveChangesAsync();

            Product product = new Product
            {
                englishname = "Item",
                price = 100,
                count = 5,
                BrandId = brand.Id,
                ProductCategoryId = category.Id
            };
            identity.Db.Add(product);
            await identity.Db.SaveChangesAsync();

            Purchasecart cart = new Purchasecart
            {
                UserId = owner.Id,
                isOpen = cartOpen,
                createdDate = DateTime.UtcNow
            };
            identity.Db.Add(cart);
            await identity.Db.SaveChangesAsync();

            PurchaseCartItem item = new PurchaseCartItem
            {
                PurchaseCartId = cart.Id,
                ProductId = product.Id,
                count = 1
            };
            identity.Db.Add(item);
            await identity.Db.SaveChangesAsync();

            ownersItemId = item.Id;
        }

        CustomerController ControllerFor(ApplicationUser user)
        {
            CustomerController controller = new CustomerController(identity.Db, identity.Users);
            TestContext.Prepare(controller, user, new FakeSession());
            return controller;
        }

        static bool StatusOf(IActionResult result)
        {
            JsonResult json = Assert.IsType<JsonResult>(result);
            object value = json.Value;
            return (bool)value.GetType().GetProperty("status").GetValue(value);
        }

        [Fact]
        public async Task The_owner_can_change_the_quantity_of_their_own_item()
        {
            await SeedAsync();

            IActionResult result = await ControllerFor(owner).ChangeCountPurchaseItem(4, ownersItemId);

            Assert.True(StatusOf(result));
            using MedAdvice.Data.MedAdviceDb verify = database.NewContext();
            Assert.Equal(4, (await verify.purchaseCartItems.SingleAsync(x => x.Id == ownersItemId)).count);
        }

        [Fact]
        public async Task Another_user_cannot_change_the_quantity_of_someone_elses_item()
        {
            await SeedAsync();

            IActionResult result = await ControllerFor(intruder).ChangeCountPurchaseItem(99, ownersItemId);

            Assert.False(StatusOf(result));
            using MedAdvice.Data.MedAdviceDb verify = database.NewContext();
            Assert.Equal(1, (await verify.purchaseCartItems.SingleAsync(x => x.Id == ownersItemId)).count);
        }

        [Fact]
        public async Task Another_user_cannot_remove_someone_elses_item()
        {
            await SeedAsync();

            IActionResult result = await ControllerFor(intruder).RemoveFromPurchaseCart(ownersItemId);

            Assert.False(StatusOf(result));
            using MedAdvice.Data.MedAdviceDb verify = database.NewContext();
            Assert.True(await verify.purchaseCartItems.AnyAsync(x => x.Id == ownersItemId));
        }

        [Fact]
        public async Task The_owner_can_remove_their_own_item()
        {
            await SeedAsync();

            IActionResult result = await ControllerFor(owner).RemoveFromPurchaseCart(ownersItemId);

            Assert.True(StatusOf(result));
            using MedAdvice.Data.MedAdviceDb verify = database.NewContext();
            Assert.False(await verify.purchaseCartItems.AnyAsync(x => x.Id == ownersItemId));
        }

        [Fact]
        public async Task A_refused_item_is_indistinguishable_from_one_that_does_not_exist()
        {
            // Both answer status=false, so the endpoint cannot be used to discover which item
            // ids exist.
            await SeedAsync();

            IActionResult refused = await ControllerFor(intruder).RemoveFromPurchaseCart(ownersItemId);
            IActionResult missing = await ControllerFor(intruder).RemoveFromPurchaseCart(987654);

            Assert.Equal(StatusOf(refused), StatusOf(missing));
            Assert.False(StatusOf(refused));
        }

        [Fact]
        public async Task Items_in_a_closed_cart_are_not_writable()
        {
            await SeedAsync(cartOpen: false);

            IActionResult result = await ControllerFor(owner).ChangeCountPurchaseItem(7, ownersItemId);

            Assert.False(StatusOf(result));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-500)]
        public async Task A_non_positive_quantity_is_rejected(int quantity)
        {
            await SeedAsync();

            IActionResult result = await ControllerFor(owner).ChangeCountPurchaseItem(quantity, ownersItemId);

            Assert.False(StatusOf(result));
            using MedAdvice.Data.MedAdviceDb verify = database.NewContext();
            Assert.Equal(1, (await verify.purchaseCartItems.SingleAsync(x => x.Id == ownersItemId)).count);
        }

        public void Dispose()
        {
            identity.Dispose();
            database.Dispose();
        }
    }
}
