using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Features.Banners.DTOs;
using MatarzatAlKahf.Application.Features.SocialMediaLinks.DTOs;
using MatarzatAlKahf.Application.Features.StoreSettings.DTOs;

namespace MatarzatAlKahf.Application.Features.Home.DTOs;

public sealed record HomeResponse(
    IReadOnlyList<BannerResponse> MainBanners,
    IReadOnlyList<BannerResponse> SecondaryBanners,
    IReadOnlyList<PublicCategoryDto> MainCategories,
    IReadOnlyList<PublicProductListItemDto> FeaturedProducts,
    IReadOnlyList<PublicProductListItemDto> NewProducts,
    IReadOnlyList<PublicProductListItemDto> ActiveOffers,
    IReadOnlyList<StoreSettingResponse> Introduction,
    IReadOnlyList<SocialLinkResponse> SocialLinks,
    IReadOnlyList<StoreSettingResponse> ContactInformation);
