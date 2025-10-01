using Content.Server.Cargo.Systems;
using Content.Shared.Cargo.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests._Horizon;

[TestFixture]
public sealed class EconomyTest
{
    [Test]
    public async Task CheckAllCargoProducts()
    {
        await using var pair = await PoolManager.GetServerClient();
        var server = pair.Server;

        var entManager = server.ResolveDependency<IEntityManager>();
        var protoManager = server.ResolveDependency<IPrototypeManager>();

        var pricingSystem = entManager.System<PricingSystem>();

        await server.WaitPost(() =>
        {
            Assert.Multiple(() =>
            {
                foreach (var product in protoManager.EnumeratePrototypes<CargoProductPrototype>())
                {
                    var entity = entManager.SpawnEntity(product.Product, MapCoordinates.Nullspace);
                    var sellPrice = pricingSystem.GetPrice(entity) * 1.25; // 1.25 для карго депо
                    if (sellPrice > product.Cost)
                        Assert.Fail($"CargoProduct ({product.ID}) стоит {product.Cost}, что меньше цены продажи {sellPrice}!");
                    entManager.DeleteEntity(entity);
                }
            });
        });
        await server.WaitRunTicks(1);
        await pair.CleanReturnAsync();
    }
}
