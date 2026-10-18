using MatarzatAlKahf.Domain.Entities.Content;
using MatarzatAlKahf.Domain.Entities.System;
using MatarzatAlKahf.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace MatarzatAlKahf.Infrastructure.Persistence.Development;

public sealed partial class DevelopmentDataSeeder
{
    private static readonly SettingSeed[] SettingSeeds =
    [
        new("store.name", "General", null, true, "مطرزات الكهف", "Matarzat Al Kahf"),
        new("store.description", "General", null, true, "متجر فلسطيني للتطريز والهدايا التراثية.", "A Palestinian store for embroidery and heritage gifts."),
        new("store.currency", "General", "ILS", true),
        new("store.timezone", "General", "Asia/Hebron", false),
        new("store.defaultLanguage", "General", "Arabic", true),
        new("store.supportedCurrencies", "General", "ILS", true),
        new("introduction.title", "Introduction", null, true, "تراث حيّ مطرز لأيامك", "Living heritage, stitched for today"),
        new("introduction.body", "Introduction", null, true, "قطع مستوحاة من جمال التطريز الفلسطيني وروح الحرفة المتوارثة.", "Pieces inspired by the beauty of Palestinian embroidery and generations of craft."),
        new("contact.address", "Contact", null, true, "فلسطين", "Palestine"),
        new("payment-method.cashondelivery", "payment-methods", "True", true, "يتم الدفع عند استلام الطلب.", "Payment is collected when the order is delivered."),
        new("payment-method.banktransfer", "payment-methods", "False", true, "تحتاج بيانات التحويل إلى إعداد من إدارة المتجر.", "The store must configure transfer instructions before enabling this method."),
        new("payment-method.electronicwallet", "payment-methods", "False", true, "تحتاج بيانات المحفظة إلى إعداد من إدارة المتجر.", "The store must configure wallet instructions before enabling this method."),
        new("payment-method.creditcard", "payment-methods", "False", true, "الدفع الإلكتروني غير مهيأ.", "Online card payment is not configured.")
    ];

    private async Task<int> SeedContentAsync(DateTime now, CancellationToken cancellationToken)
    {
        await SeedStoreSettingsAsync(now, cancellationToken);
        await SeedBannersAsync(now, cancellationToken);

        List<ContentPage> existing = await _dbContext.ContentPages.AsNoTracking()
            .Include(page => page.Translations).ToListAsync(cancellationToken);
        (ContentPageType Type, string Key, string ArabicTitle, string EnglishTitle,
            string ArabicBody, string EnglishBody)[] pages =
        [
            (ContentPageType.AboutUs, "about-us", "من نحن", "About Us",
                "مطرزات الكهف مساحة للتطريز الفلسطيني والمنتجات المستوحاة من تراثه، من الأثواب والحقائب إلى قطع المنزل والهدايا.",
                "Matarzat Al Kahf is a home for Palestinian embroidery and heritage-inspired pieces, from thobes and bags to home textiles and gifts."),
            (ContentPageType.ProjectStory, "our-story", "حكايتنا", "Our Story",
                "تحمل غرز التطريز الفلسطيني حكايات المكان والناس. تستلهم مجموعة مطرزات الكهف هذه الذاكرة لتقدم قطعًا تصل التراث بالحياة اليومية.",
                "Palestinian stitches carry stories of places and people. The Matarzat Al Kahf collection draws on this memory to connect heritage with everyday life.")
        ];
        foreach (var seed in pages)
        {
            Guid id = DevelopmentSeedIds.For($"content-page:{seed.Key}");
            if (existing.Any(page => page.Id == id || page.Type == seed.Type
                || page.Translations.Any(translation => SameKey(translation.Slug, seed.Key)
                    || SameKey(translation.Slug, $"{seed.Key}-ar"))))
            {
                continue;
            }

            var page = new ContentPage
            {
                Id = id, CreatedAtUtc = now, Type = seed.Type, IsPublished = true,
                Translations =
                [
                    CreateContentTranslation(seed.Key, LanguageCode.Arabic, seed.ArabicTitle, seed.ArabicBody, now),
                    CreateContentTranslation(seed.Key, LanguageCode.English, seed.EnglishTitle, seed.EnglishBody, now)
                ]
            };
            _dbContext.ContentPages.Add(page);
            existing.Add(page);
        }

        return existing.Count(page => pages.Any(seed => page.Type == seed.Type));
    }

    private async Task SeedStoreSettingsAsync(DateTime now, CancellationToken cancellationToken)
    {
        List<StoreSetting> existing = await _dbContext.StoreSettings.AsNoTracking()
            .ToListAsync(cancellationToken);
        foreach (SettingSeed seed in SettingSeeds)
        {
            Guid id = DevelopmentSeedIds.For($"setting:{seed.Key}");
            if (existing.Any(setting => setting.Id == id || SameKey(setting.Key, seed.Key)))
            {
                continue;
            }

            var setting = new StoreSetting
            {
                Id = id, CreatedAtUtc = now, Key = seed.Key, Value = seed.Value,
                Group = seed.Group, Description = $"Default {seed.Key} setting.",
                IsPublic = seed.IsPublic, IsLocalized = seed.ArabicValue is not null,
            };
            if (setting.IsLocalized)
            {
                setting.Translations =
                [
                    new StoreSettingTranslation
                    {
                        Id = DevelopmentSeedIds.For($"setting-translation:{seed.Key}:{LanguageCode.Arabic}"),
                        CreatedAtUtc = now, Language = LanguageCode.Arabic, Value = seed.ArabicValue!
                    },
                    new StoreSettingTranslation
                    {
                        Id = DevelopmentSeedIds.For($"setting-translation:{seed.Key}:{LanguageCode.English}"),
                        CreatedAtUtc = now, Language = LanguageCode.English, Value = seed.EnglishValue!
                    }
                ];
            }

            _dbContext.StoreSettings.Add(setting);
            existing.Add(setting);
        }
    }

    private async Task SeedBannersAsync(DateTime now, CancellationToken cancellationToken)
    {
        List<Banner> existing = await _dbContext.Banners.AsNoTracking().ToListAsync(cancellationToken);
        (string Key, BannerPosition Position, int SortOrder, string Category,
            string ArabicTitle, string EnglishTitle)[] banners =
        [
            ("main", BannerPosition.HomeMain, 1, "dresses", "حكاية تُنسج بخيط من الذاكرة", "Stories stitched from memory"),
            ("gifts", BannerPosition.HomeSecondary, 1, "gifts", "هدايا فلسطينية تحمل معنى", "Palestinian gifts with meaning"),
            ("dresses", BannerPosition.HomeSecondary, 2, "dresses", "أثواب تحكي حكاية المكان", "Thobes telling stories of place")
        ];
        foreach (var seed in banners)
        {
            Guid id = DevelopmentSeedIds.For($"banner:{seed.Key}");
            if (existing.Any(banner => banner.Id == id
                || banner.Position == seed.Position && banner.SortOrder == seed.SortOrder))
            {
                continue;
            }

            var banner = new Banner
            {
                Id = id, CreatedAtUtc = now, IsActive = true,
                Position = seed.Position, SortOrder = seed.SortOrder,
                Translations =
                [
                    CreateBannerTranslation(seed.Key, seed.Category, LanguageCode.Arabic, seed.ArabicTitle, now),
                    CreateBannerTranslation(seed.Key, seed.Category, LanguageCode.English, seed.EnglishTitle, now)
                ]
            };
            _dbContext.Banners.Add(banner);
            existing.Add(banner);
        }
    }

    private static BannerTranslation CreateBannerTranslation(
        string key, string categoryKey, LanguageCode language, string title, DateTime now) => new()
    {
        Id = DevelopmentSeedIds.For($"banner-translation:{key}:{language}"),
        CreatedAtUtc = now, Language = language, Title = title,
        DesktopImageUrl = CategoryImageUrl(categoryKey), MobileImageUrl = CategoryImageUrl(categoryKey),
        AltText = DevelopmentSeedImageCredits.Get(CategoryImageUrl(categoryKey)),
        LinkUrl = "/products"
    };

    private static ContentPageTranslation CreateContentTranslation(string key,
        LanguageCode language, string title, string body, DateTime now) => new()
    {
        Id = DevelopmentSeedIds.For($"content-page-translation:{key}:{language}"),
        CreatedAtUtc = now, Language = language, Title = title,
        Slug = language == LanguageCode.Arabic ? $"{key}-ar" : key,
        Content = key == "about-us" ? AboutContent(body, language) : body,
        MetaTitle = title, MetaDescription = body
    };

    private static string AboutContent(string body, LanguageCode language)
    {
        string heading = language == LanguageCode.Arabic ? "مصادر الصور وتراخيصها" : "Image credits and licenses";
        string credits = string.Concat(DevelopmentSeedImageCredits.All.Select(credit =>
            $"<li>{System.Net.WebUtility.HtmlEncode(credit)}</li>"));
        return $"<p>{System.Net.WebUtility.HtmlEncode(body)}</p><h2>{heading}</h2><ul>{credits}</ul>";
    }

    private sealed record SettingSeed(string Key, string Group, string? Value, bool IsPublic,
        string? ArabicValue = null, string? EnglishValue = null);
}
