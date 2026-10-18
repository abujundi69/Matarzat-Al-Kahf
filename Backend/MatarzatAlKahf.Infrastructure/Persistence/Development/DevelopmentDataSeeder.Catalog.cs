using MatarzatAlKahf.Domain.Entities.Catalog;
using MatarzatAlKahf.Domain.Entities.Orders;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Persistence.Development;

public sealed partial class DevelopmentDataSeeder
{
    private async Task<int> SeedDeliveryLocationsAsync(DateTime now, CancellationToken cancellationToken)
    {
        (string Key, string Arabic, string English, decimal Cost)[] seeds =
        [
            ("jerusalem", "القدس", "Jerusalem", 20m),
            ("ramallah", "رام الله والبيرة", "Ramallah and Al-Bireh", 18m),
            ("bethlehem", "بيت لحم", "Bethlehem", 18m),
            ("hebron", "الخليل", "Hebron", 22m),
            ("nablus", "نابلس", "Nablus", 22m),
            ("jenin", "جنين", "Jenin", 25m),
            ("tulkarm", "طولكرم", "Tulkarm", 25m),
            ("qalqilya", "قلقيلية", "Qalqilya", 25m),
            ("jericho", "أريحا", "Jericho", 22m)
        ];
        List<DeliveryLocation> existing = await _dbContext.DeliveryLocations
            .AsNoTracking().ToListAsync(cancellationToken);
        foreach (var seed in seeds)
        {
            Guid id = DevelopmentSeedIds.For($"delivery:{seed.Key}");
            // A deleted/disabled location is an operator decision, not a missing default.
            if (existing.Any(item => item.Id == id || item.CountryCode == "PS"
                && (string.Equals(item.NameEnglish, seed.English, StringComparison.OrdinalIgnoreCase)
                    || item.NameArabic == seed.Arabic
                    || seed.Key == "ramallah" && item.NameArabic == "رام الله")))
            {
                continue;
            }

            var location = new DeliveryLocation
            {
                Id = id, CreatedAtUtc = now, CountryCode = "PS",
                CountryNameArabic = "فلسطين", CountryNameEnglish = "Palestine",
                NameArabic = seed.Arabic, NameEnglish = seed.English,
                IsActive = true, CashOnDeliveryAllowed = true,
                ProductPriceIncludesDelivery = false, AdditionalDeliveryCost = seed.Cost,
                MinimumOrderAmount = 0m, SortOrder = Array.IndexOf(seeds, seed) + 1
            };
            _dbContext.DeliveryLocations.Add(location);
            existing.Add(location);
        }

        return existing.Count(item => seeds.Any(seed => item.Id == DevelopmentSeedIds.For($"delivery:{seed.Key}")
            || item.CountryCode == "PS" && string.Equals(item.NameEnglish, seed.English, StringComparison.OrdinalIgnoreCase)));
    }

    private async Task<(int Categories, int Products, int Variants)> SeedCatalogAsync(
        DateTime now, CancellationToken cancellationToken)
    {
        List<Category> existingCategories = await _dbContext.Categories.AsNoTracking()
            .Include(category => category.Translations).ToListAsync(cancellationToken);
        var categories = new Dictionary<string, Category>(StringComparer.Ordinal);
        foreach (CategorySeed seed in CategorySeeds)
        {
            Guid id = DevelopmentSeedIds.For($"category:{seed.Key}");
            Category? category = ResolveSeedEntity(existingCategories,
                item => item.Id == id || item.Translations.Any(translation =>
                    translation.Language == LanguageCode.English && SameKey(translation.Slug, seed.Key)
                    || translation.Language == LanguageCode.Arabic && SameKey(translation.Slug, $"{seed.Key}-ar")), seed.Key);
            if (category is null)
            {
                category = new Category
                {
                    Id = id, CreatedAtUtc = now, IsActive = true,
                    ImageUrl = CategoryImageUrl(seed.Key), SortOrder = categories.Count + 1,
                    Translations =
                    [
                        CreateCategoryTranslation(seed, LanguageCode.Arabic, now),
                        CreateCategoryTranslation(seed, LanguageCode.English, now)
                    ]
                };
                _dbContext.Categories.Add(category);
                existingCategories.Add(category);
            }

            categories.Add(seed.Key, category);
        }

        List<Product> existingProducts = await _dbContext.Products.AsNoTracking()
            .Include(product => product.Translations).Include(product => product.Variants)
            .AsSplitQuery().ToListAsync(cancellationToken);
        HashSet<string> usedSkus = (await _dbContext.ProductVariants.AsNoTracking()
            .Select(variant => variant.Sku).ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        int products = 0;
        int variants = 0;
        for (int index = 0; index < ProductSeeds.Length; index++)
        {
            ProductSeed seed = ProductSeeds[index];
            Guid id = DevelopmentSeedIds.For($"product:{seed.Key}");
            Product? product = ResolveSeedEntity(existingProducts,
                item => item.Id == id || item.Translations.Any(translation =>
                    translation.Language == LanguageCode.English && SameKey(translation.Slug, seed.Key)
                    || translation.Language == LanguageCode.Arabic && SameKey(translation.Slug, $"{seed.Key}-ar")), seed.Key);
            if (product is null)
            {
                Category category = categories[seed.CategoryKey];
                if (category.IsDeleted || !category.IsActive)
                {
                    continue;
                }

                product = new Product
                {
                    Id = id, CreatedAtUtc = now.AddDays(-ProductSeeds.Length + index),
                    CategoryId = category.Id, BasePrice = seed.BasePrice,
                    Currency = CurrencyCode.ILS, Status = ProductStatus.Active,
                    IsFeatured = seed.IsFeatured, PublishedAtUtc = now.AddDays(-ProductSeeds.Length + index),
                    Translations =
                    [
                        CreateProductTranslation(seed, LanguageCode.Arabic, now),
                        CreateProductTranslation(seed, LanguageCode.English, now)
                    ],
                    Images = [CreateProductImage(seed, now)]
                };
                bool clothing = seed.CategoryKey is "dresses" or "abayas";
                int variantCount = clothing ? 2 : 1;
                for (int offset = 0; offset < variantCount; offset++)
                {
                    string suffix = offset == 0 ? "A" : "B";
                    string sku = $"MK-{seed.Key.ToUpperInvariant()}-{suffix}";
                    if (!usedSkus.Add(sku))
                    {
                        throw new InvalidOperationException($"Seed SKU '{sku}' is already assigned to another product.");
                    }

                    product.Variants.Add(CreateVariant(seed, sku, suffix, offset,
                        clothing ? 6 - offset : 12 + index % 9, now));
                }

                _dbContext.Products.Add(product);
                existingProducts.Add(product);
            }

            // Existing products are counted but never updated: edits, stock, reservations,
            // prices, publication status, and deletions survive every later migration.
            products++;
            variants += product.Variants.Count;
        }

        return (categories.Count, products, variants);
    }

    private static bool SameKey(string left, string right) =>
        string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    private static string CategoryImageUrl(string key) => $"/uploads/categories/seed/{key}.jpg";

    private static CategoryTranslation CreateCategoryTranslation(
        CategorySeed seed, LanguageCode language, DateTime now)
    {
        bool arabic = language == LanguageCode.Arabic;
        string name = arabic ? seed.ArabicName : seed.EnglishName;
        string description = arabic ? seed.ArabicDescription : seed.EnglishDescription;
        return new CategoryTranslation
        {
            Id = DevelopmentSeedIds.For($"category-translation:{seed.Key}:{language}"),
            CreatedAtUtc = now, Language = language, Name = name,
            Slug = arabic ? $"{seed.Key}-ar" : seed.Key,
            Description = description + "\n\n" + DevelopmentSeedImageCredits.Get(CategoryImageUrl(seed.Key)),
            MetaTitle = name, MetaDescription = description
        };
    }

    private static ProductTranslation CreateProductTranslation(
        ProductSeed seed, LanguageCode language, DateTime now)
    {
        bool arabic = language == LanguageCode.Arabic;
        string name = arabic ? seed.ArabicName : seed.EnglishName;
        string description = arabic ? seed.ArabicDescription : seed.EnglishDescription;
        return new ProductTranslation
        {
            Id = DevelopmentSeedIds.For($"product-translation:{seed.Key}:{language}"),
            CreatedAtUtc = now, Language = language, Name = name,
            Slug = arabic ? $"{seed.Key}-ar" : seed.Key,
            ShortDescription = description,
            Description = description + (arabic
                ? " الصورة مرجع توضيحي للتطريز والتراث الفلسطيني، وقد تختلف تفاصيل القطعة المعروضة."
                : " The image is an illustrative reference to Palestinian embroidery and heritage; details of the listed piece may differ."),
            Story = (arabic
                ? "قطعة مستوحاة من جمال التطريز الفلسطيني ونقوشه الهندسية المتوارثة عبر الأجيال."
                : "Inspired by Palestinian embroidery and its geometric motifs passed down through generations.")
                + "\n\n" + DevelopmentSeedImageCredits.Get($"/uploads/products/seed/{seed.Key}.jpg"),
            MetaTitle = name, MetaDescription = description
        };
    }

    private static ProductImage CreateProductImage(ProductSeed seed, DateTime now)
    {
        string url = $"/uploads/products/seed/{seed.Key}.jpg";
        return new ProductImage
        {
            Id = DevelopmentSeedIds.For($"product-image:{seed.Key}"),
            CreatedAtUtc = now, ImageUrl = url, ThumbnailUrl = url,
            SortOrder = 1, IsPrimary = true,
            Translations =
            [
                new ProductImageTranslation
                {
                    Id = DevelopmentSeedIds.For($"product-image-translation:{seed.Key}:{LanguageCode.Arabic}"),
                    CreatedAtUtc = now, Language = LanguageCode.Arabic,
                    AltText = $"مرجع توضيحي: {seed.ArabicName}. " + DevelopmentSeedImageCredits.GetShort(url)
                },
                new ProductImageTranslation
                {
                    Id = DevelopmentSeedIds.For($"product-image-translation:{seed.Key}:{LanguageCode.English}"),
                    CreatedAtUtc = now, Language = LanguageCode.English,
                    AltText = $"{seed.EnglishName}. " + DevelopmentSeedImageCredits.GetShort(url)
                }
            ]
        };
    }

    private static ProductVariant CreateVariant(ProductSeed seed, string sku, string suffix,
        int offset, int quantity, DateTime now)
    {
        Guid inventoryId = DevelopmentSeedIds.For($"inventory:{sku}");
        return new ProductVariant
        {
            Id = DevelopmentSeedIds.For($"variant:{seed.Key}:{suffix}"),
            CreatedAtUtc = now, Sku = sku, IsActive = true,
            Size = seed.CategoryKey is "dresses" or "abayas" ? offset == 0 ? "M" : "L" : null,
            Color = seed.Color, WeightInGrams = seed.WeightInGrams, Capacity = seed.Capacity,
            InventoryItem = new InventoryItem
            {
                Id = inventoryId, CreatedAtUtc = now, QuantityOnHand = quantity,
                ReservedQuantity = 0, ReorderLevel = 3,
                Transactions =
                [
                    new InventoryTransaction
                    {
                        Id = DevelopmentSeedIds.For($"inventory-transaction:{inventoryId}:initial"),
                        CreatedAtUtc = now, Type = InventoryTransactionType.InitialStock,
                        Quantity = quantity, Reference = "DEV-SEED",
                        Notes = "Development opening inventory / مخزون افتتاحي للتطوير"
                    }
                ]
            }
        };
    }
}
