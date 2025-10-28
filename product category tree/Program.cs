using System;
using System.Collections.Generic;
using System.Linq;


public class Product
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string Description { get; set; }

    public Product(string name, decimal price, int stock, string description = "")
    {
        Name = name;
        Price = price;
        Stock = stock;
        Description = description;
    }

    public override string ToString()
    {
        return $"{Name} - ${Price:F2} (в наличии: {Stock})";
    }

    public string GetDetailedInfo()
    {
        return $"{Name}\nЦена: ${Price:F2}\nНа складе: {Stock} шт.\nОписание: {Description}";
    }
}


public class Category
{
    public string Name { get; set; }
    public string Description { get; set; }
    public List<Product> Products { get; set; }
    public List<Category> Subcategories { get; set; }

    public Category(string name, string description = "")
    {
        Name = name;
        Description = description;
        Products = new List<Product>();
        Subcategories = new List<Category>();
    }

    
    public void AddSubcategory(Category subcategory)
    {
        Subcategories.Add(subcategory);
    }

    
    public void AddProduct(Product product)
    {
        Products.Add(product);
    }

 
    public int GetTotalProductsCount()
    {
        int count = Products.Count;
        foreach (var subcategory in Subcategories)
        {
            count += subcategory.GetTotalProductsCount();
        }
        return count;
    }

   
    public List<Product> GetAllProducts()
    {
        var allProducts = new List<Product>();
        allProducts.AddRange(Products);

        foreach (var subcategory in Subcategories)
        {
            allProducts.AddRange(subcategory.GetAllProducts());
        }

        return allProducts;
    }


    public decimal GetTotalInventoryValue()
    {
        return GetAllProducts().Sum(p => p.Price * p.Stock);
    }


    public Category FindCategory(string name)
    {
        if (Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            return this;

        foreach (var subcategory in Subcategories)
        {
            var found = subcategory.FindCategory(name);
            if (found != null)
                return found;
        }

        return null;
    }

    public Product FindProduct(string productName)
    {
        var product = Products.FirstOrDefault(p =>
            p.Name.Equals(productName, StringComparison.OrdinalIgnoreCase));

        if (product != null)
            return product;

        foreach (var subcategory in Subcategories)
        {
            product = subcategory.FindProduct(productName);
            if (product != null)
                return product;
        }

        return null;
    }


    public List<Product> GetProductsInPriceRange(decimal minPrice, decimal maxPrice)
    {
        return GetAllProducts()
            .Where(p => p.Price >= minPrice && p.Price <= maxPrice)
            .ToList();
    }

    public void PrintCategoryTree(string indent = "", bool isLast = true)
    {
        Console.Write(indent);
        if (isLast)
        {
            Console.Write("└── ");
            indent += "    ";
        }
        else
        {
            Console.Write("├── ");
            indent += "│   ";
        }

        Console.WriteLine($"{Name} ({Products.Count} товаров, {Subcategories.Count} подкатегорий)");

        for (int i = 0; i < Subcategories.Count; i++)
        {
            Subcategories[i].PrintCategoryTree(indent, i == Subcategories.Count - 1);
        }
    }


    public void PrintProducts(string categoryPath = "")
    {
        var currentPath = string.IsNullOrEmpty(categoryPath) ? Name : $"{categoryPath} > {Name}";

        if (Products.Count > 0)
        {
            Console.WriteLine($"\n{currentPath}:");
            Console.WriteLine(new string('-', 50));
            foreach (var product in Products)
            {
                Console.WriteLine($"  • {product}");
            }
        }

        foreach (var subcategory in Subcategories)
        {
            subcategory.PrintProducts(currentPath);
        }
    }

 
    public string GetCategoryPath(string separator = " > ")
    {
        var path = new List<string> { Name };
        var current = this;


        return Name;
    }
}


public class ProductCatalog
{
    public Category RootCategory { get; private set; }
    public string StoreName { get; set; }

    public ProductCatalog(string storeName, string rootCategoryName = "Каталог")
    {
        StoreName = storeName;
        RootCategory = new Category(rootCategoryName, $"Главный каталог магазина {storeName}");
    }

 
    public bool AddCategory(string categoryName, string parentCategoryName = null, string description = "")
    {
        var parent = string.IsNullOrEmpty(parentCategoryName) ?
            RootCategory : RootCategory.FindCategory(parentCategoryName);

        if (parent == null)
        {
            Console.WriteLine($"Ошибка: Родительская категория '{parentCategoryName}' не найдена!");
            return false;
        }

        var newCategory = new Category(categoryName, description);
        parent.AddSubcategory(newCategory);
        return true;
    }


    public bool AddProductToCategory(Product product, string categoryName)
    {
        var category = RootCategory.FindCategory(categoryName);
        if (category == null)
        {
            Console.WriteLine($"Ошибка: Категория '{categoryName}' не найдена!");
            return false;
        }

        category.AddProduct(product);
        return true;
    }

    // Поиск товара по имени
    public Product FindProduct(string productName)
    {
        return RootCategory.FindProduct(productName);
    }

    // Поиск категории по имени
    public Category FindCategory(string categoryName)
    {
        return RootCategory.FindCategory(categoryName);
    }

    // Получение статистики по каталогу
    public void PrintCatalogStats()
    {
        Console.WriteLine($"\n=== СТАТИСТИКА КАТАЛОГА '{StoreName}' ===");
        Console.WriteLine($"Общее количество товаров: {RootCategory.GetTotalProductsCount()}");
        Console.WriteLine($"Общая стоимость инвентаря: ${RootCategory.GetTotalInventoryValue():F2}");
        Console.WriteLine($"Корневая категория: {RootCategory.Name}");
    }

    // Вывод всего дерева категорий
    public void PrintFullCategoryTree()
    {
        Console.WriteLine($"\n=== ДЕРЕВО КАТЕГОРИЙ '{StoreName}' ===");
        RootCategory.PrintCategoryTree();
    }

    // Вывод всех товаров с группировкой по категориям
    public void PrintAllProducts()
    {
        Console.WriteLine($"\n=== ВСЕ ТОВАРЫ '{StoreName}' ===");
        RootCategory.PrintProducts();
    }

    // Поиск товаров в диапазоне цен
    public void PrintProductsInPriceRange(decimal minPrice, decimal maxPrice)
    {
        var products = RootCategory.GetProductsInPriceRange(minPrice, maxPrice);
        Console.WriteLine($"\n=== ТОВАРЫ ОТ ${minPrice:F2} ДО ${maxPrice:F2} ===");

        if (products.Count == 0)
        {
            Console.WriteLine("Товары не найдены.");
            return;
        }

        foreach (var product in products.OrderBy(p => p.Price))
        {
            Console.WriteLine($"  • {product}");
        }
    }
}


public class CategoryTreeDemo
{
    public static void Run()
    {
        Console.WriteLine("=== ДЕРЕВО КАТЕГОРИЙ ТОВАРОВ ===\n");


        var catalog = new ProductCatalog("TechStore", "Электроника");


        catalog.AddCategory("Компьютеры", "Электроника");
        catalog.AddCategory("Смартфоны", "Электроника");
        catalog.AddCategory("Телевизоры", "Электроника");
        catalog.AddCategory("Аксессуары", "Электроника");

        catalog.AddCategory("Ноутбуки", "Компьютеры");
        catalog.AddCategory("Настольные ПК", "Компьютеры");
        catalog.AddCategory("Мониторы", "Компьютеры");

        catalog.AddCategory("Android", "Смартфоны");
        catalog.AddCategory("iOS", "Смартфоны");

        catalog.AddCategory("Игровые", "Мониторы");
        catalog.AddCategory("Офисные", "Мониторы");


        catalog.AddProductToCategory(new Product("Dell XPS 13", 1200m, 5, "Ультрабук с безрамочным дисплеем"), "Ноутбуки");
        catalog.AddProductToCategory(new Product("MacBook Pro 16", 2500m, 3, "Профессиональный ноутбук от Apple"), "Ноутбуки");
        catalog.AddProductToCategory(new Product("HP Pavilion", 800m, 7, "Надежный домашний компьютер"), "Настольные ПК");

        catalog.AddProductToCategory(new Product("Samsung Galaxy S23", 900m, 10, "Флагманский смартфон Samsung"), "Android");
        catalog.AddProductToCategory(new Product("Google Pixel 7", 700m, 8, "Смартфон с лучшей камерой"), "Android");
        catalog.AddProductToCategory(new Product("iPhone 15 Pro", 1200m, 6, "Премиальный iPhone"), "iOS");
        catalog.AddProductToCategory(new Product("iPhone 14", 800m, 4, "Популярная модель iPhone"), "iOS");

        catalog.AddProductToCategory(new Product("Samsung QLED 55", 1500m, 3, "Телевизор с квантовыми точками"), "Телевизоры");
        catalog.AddProductToCategory(new Product("LG OLED 65", 1800m, 2, "Телевизор с идеальным черным"), "Телевизоры");

        catalog.AddProductToCategory(new Product("ASUS ROG Swift", 600m, 5, "Игровой монитор 144 Гц"), "Игровые");
        catalog.AddProductToCategory(new Product("Dell UltraSharp", 350m, 8, "Офисный монитор с точной цветопередачей"), "Офисные");

        catalog.AddProductToCategory(new Product("Чехол для iPhone", 20m, 50, "Защитный чехол"), "Аксессуары");
        catalog.AddProductToCategory(new Product("USB-C кабель", 15m, 100, "Быстрый кабель для зарядки"), "Аксессуары");


        catalog.PrintCatalogStats();
        catalog.PrintFullCategoryTree();
        catalog.PrintAllProducts();


        catalog.PrintProductsInPriceRange(500, 1000);


        Console.WriteLine("\n=== ПОИСК ТОВАРА ===");
        var foundProduct = catalog.FindProduct("iPhone 15 Pro");
        if (foundProduct != null)
        {
            Console.WriteLine("Найден товар:");
            Console.WriteLine(foundProduct.GetDetailedInfo());
        }


        Console.WriteLine("\n=== ПОИСК КАТЕГОРИИ ===");
        var foundCategory = catalog.FindCategory("Ноутбуки");
        if (foundCategory != null)
        {
            Console.WriteLine($"Найдена категория: {foundCategory.Name}");
            Console.WriteLine($"Количество товаров: {foundCategory.GetTotalProductsCount()}");
        }


        Console.WriteLine("\n=== ДОБАВЛЕНИЕ НОВОЙ КАТЕГОРИИ ===");
        catalog.AddCategory("Планшеты", "Электроника");
        catalog.AddCategory("iPad", "Планшеты");
        catalog.AddProductToCategory(new Product("iPad Air", 600m, 10, "Универсальный планшет"), "iPad");

        catalog.PrintFullCategoryTree();


        var tabletsCategory = catalog.FindCategory("Планшеты");
        if (tabletsCategory != null)
        {
            Console.WriteLine("\nТовары в категории Планшеты:");
            tabletsCategory.PrintProducts();
        }
    }
}


class Program
{
    static void Main(string[] args)
    {

        CategoryTreeDemo.Run();

        Console.WriteLine("\n\nНажмите любую клавишу для выхода...");
        Console.ReadKey();
    }
}