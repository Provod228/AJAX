namespace WebApplication4.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Category { get; set; } = string.Empty;

        // ===== V2: 2 новых поля (читаются только API v2) =====
        // Nullable/default-значения: старые записи в products.json без этих
        // полей десериализуются без ошибок (Description="", Stock=0).
        public string Description { get; set; } = string.Empty;
        public int Stock { get; set; }

        // ===== ETag/конкурентность: версия для If-Match / If-None-Match =====
        // ETag ресурса = "v{Version}". PUT/PATCH увеличивают версию.
        public int Version { get; set; } = 1;
    }
}
