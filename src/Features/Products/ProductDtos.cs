namespace CafeteriaApi.Products;

public record CreateProductsRequest(
    string Name
);

public record DisplayProducts(
    int Id,
    string Name
);