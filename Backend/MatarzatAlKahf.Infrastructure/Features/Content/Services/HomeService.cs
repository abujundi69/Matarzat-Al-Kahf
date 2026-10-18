using MatarzatAlKahf.Application.Catalog.Categories;
using MatarzatAlKahf.Application.Catalog.Products;
using MatarzatAlKahf.Application.Features.Banners.Interfaces;
using MatarzatAlKahf.Application.Features.Home.DTOs;
using MatarzatAlKahf.Application.Features.Home.Interfaces;
using MatarzatAlKahf.Application.Features.SocialMediaLinks.Interfaces;
using MatarzatAlKahf.Application.Features.StoreSettings.Interfaces;
using MatarzatAlKahf.Domain.Enums;

namespace MatarzatAlKahf.Infrastructure.Features.Content.Services;

public sealed class HomeService(
    IBannerService bannerService,
    IPublicCategoryService categoryService,
    IPublicProductService productService,
    ISocialLinkService socialLinkService,
    IStoreSettingService settingService) : IHomeService
{
    public async Task<HomeResponse> GetAsync(CancellationToken cancellationToken)
    {
        var featuredQuery = new ProductListQuery(
            PageSize: 8,
            IsFeatured: true,
            InStock: true);
        var newQuery = new ProductListQuery(PageSize: 8, InStock: true);
        var offersQuery = new ProductListQuery(
            PageSize: 8,
            InStock: true,
            HasDiscount: true,
            Sort: ProductSortValues.PriceAscending);
        return new HomeResponse(
            await bannerService.ListPublicAsync(BannerPosition.HomeMain, cancellationToken),
            await bannerService.ListPublicAsync(BannerPosition.HomeSecondary, cancellationToken),
            (await categoryService.GetAllAsync(null, cancellationToken)).Take(12).ToArray(),
            (await productService.GetPageAsync(featuredQuery, cancellationToken)).Items,
            (await productService.GetPageAsync(newQuery, cancellationToken)).Items,
            (await productService.GetPageAsync(offersQuery, cancellationToken)).Items,
            await settingService.GetPublicGroupAsync("Introduction", cancellationToken),
            await socialLinkService.ListPublicAsync(cancellationToken),
            await settingService.GetPublicGroupAsync("Contact", cancellationToken));
    }
}
