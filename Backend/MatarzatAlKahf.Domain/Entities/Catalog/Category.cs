using MatarzatAlKahf.Domain.Common;

namespace MatarzatAlKahf.Domain.Entities.Catalog;

public sealed class Category : BaseEntity
{
    public Guid? ParentCategoryId { get; set; }

    public string ImageUrl { get; set; } = string.Empty;

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public Category? ParentCategory { get; set; }

    public ICollection<Category> Children { get; set; } = new List<Category>();

    public ICollection<CategoryTranslation> Translations { get; set; } = new List<CategoryTranslation>();

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
