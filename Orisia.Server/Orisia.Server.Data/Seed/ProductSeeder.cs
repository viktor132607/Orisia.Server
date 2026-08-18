using System.Text;
using Orisia.Server.Data.Entities;

namespace Orisia.Server.Data.Seed;

public static class ProductSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        Dictionary<string, Guid> categoryIds = db.Categories.ToDictionary(category => category.Name, category => category.Id);

        List<Product> products =
        [
            CreateProduct(
                title: "Nike Air Zoom Pegasus 40",
                description: BuildDescription(
                    "A dependable daily road shoe with responsive foam and enough cushioning for easy runs, tempo work, and long weekend sessions.",
                    "Upper: engineered mesh with targeted ventilation through the forefoot.",
                    "Midsole: Nike React foam with Zoom Air units in the forefoot and heel.",
                    "Use case: neutral road running from 5 km training runs to half-marathon preparation.",
                    "Weight: approximately 288 g in men's EU 42.5.",
                    "Drop: 10 mm."),
                mainImageUrl: "https://images.unsplash.com/photo-1542291026-7eec264c27ff?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 132.42m,
                discountPercentage: 12,
                discountedPrice: 117.09m,
                quantity: 18,
                rating: 4.7,
                categoryId: categoryIds["Running"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1552346154-21d32810aba3?auto=format&fit=crop&w=1200&q=80",
                    "https://images.unsplash.com/photo-1600185365483-26d7a4cc7519?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "Garmin Forerunner 255 GPS Running Watch",
                description: BuildDescription(
                    "Lightweight multisport watch for runners who want reliable GPS tracking, wrist-based metrics, and enough battery life for a full training week.",
                    "Display: 1.3-inch always-on color screen.",
                    "Tracking: multi-band GPS, daily suggested workouts, race widget, and recovery insights.",
                    "Battery: up to 14 days in smartwatch mode.",
                    "Use case: structured run training, race prep, and daily activity tracking.",
                    "Connectivity: Bluetooth, ANT+, Wi-Fi."),
                mainImageUrl: "https://images.unsplash.com/photo-1510017803434-a899398421b3?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 280.70m,
                discountPercentage: 4,
                discountedPrice: 270.47m,
                quantity: 12,
                rating: 4.8,
                categoryId: categoryIds["Running"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1546868871-7041f2a55e12?auto=format&fit=crop&w=1200&q=80",
                    "https://images.unsplash.com/photo-1579586337278-3f436f25d4d6?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "Adidas Predator Accuracy.1 FG Football Boots",
                description: BuildDescription(
                    "Firm-ground boots built for players who want a secure midfoot fit and a clean strike on natural grass pitches.",
                    "Upper: hybridtouch material with textured strike zones.",
                    "Outsole: firm-ground plate for dry natural grass.",
                    "Fit: mid-cut collar with adaptive lacing system.",
                    "Use case: match play and team training on outdoor grass.",
                    "Included: removable sockliner."),
                mainImageUrl: "https://images.unsplash.com/photo-1511886929837-354d827aae26?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 178.44m,
                discountPercentage: 17,
                discountedPrice: 147.76m,
                quantity: 7,
                rating: 4.8,
                categoryId: categoryIds["Football"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1575361204480-aadea25e6e68?auto=format&fit=crop&w=1200&q=80",
                    "https://images.unsplash.com/photo-1522778119026-d647f0596c20?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "PUMA Orbita 2 TB Football",
                description: BuildDescription(
                    "Training ball with a consistent shape and durable outer surface for regular five-a-side sessions and academy drills.",
                    "Construction: machine-stitched 32-panel design.",
                    "Outer: textured PU for a more controlled touch.",
                    "Size: 5.",
                    "Use case: club training, school sport, and weekend kickabouts.",
                    "Recommended inflation: printed on the valve panel."),
                mainImageUrl: "https://images.unsplash.com/photo-1614632537190-23e4146777db?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 45.97m,
                discountPercentage: 11,
                discountedPrice: 40.85m,
                quantity: 15,
                rating: 4.5,
                categoryId: categoryIds["Football"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1570498839593-e565b39455fc?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "Under Armour HeatGear Training T-Shirt",
                description: BuildDescription(
                    "Fitted short-sleeve top designed for gym sessions, conditioning circuits, and warm-weather training.",
                    "Fabric: lightweight HeatGear knit with four-way stretch.",
                    "Moisture management: dries quickly and helps reduce cling during hard sessions.",
                    "Fit: close to the body without compression-level tightness.",
                    "Use case: strength training, HIIT, and indoor cardio.",
                    "Care: machine wash cold."),
                mainImageUrl: "https://images.unsplash.com/photo-1521572163474-6864f9cf17ab?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 30.63m,
                discountPercentage: 0,
                discountedPrice: 0m,
                quantity: 34,
                rating: 4.6,
                categoryId: categoryIds["Fitness & Training"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1515886657613-9f3515b0c78f?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "Bowflex SelectTech 552 Adjustable Dumbbells",
                description: BuildDescription(
                    "Space-saving adjustable dumbbells for home strength training without a full rack of fixed weights.",
                    "Weight range: 2.2 kg to 24 kg per dumbbell.",
                    "Adjustment: selector dial changes resistance in small increments.",
                    "Use case: home training for presses, rows, lunges, and accessory work.",
                    "Storage: supplied with molded trays.",
                    "Ideal for: limited-space training setups."),
                mainImageUrl: "https://images.unsplash.com/photo-1517838277536-f5f99be501cd?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 510.78m,
                discountPercentage: 5,
                discountedPrice: 485.22m,
                quantity: 4,
                rating: 4.9,
                categoryId: categoryIds["Fitness & Training"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1518611012118-696072aa579a?auto=format&fit=crop&w=1200&q=80",
                    "https://images.unsplash.com/photo-1534438327276-14e5300c3a48?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "Salomon X Ultra 4 GTX Hiking Shoes",
                description: BuildDescription(
                    "Supportive waterproof hiking shoes for mixed terrain, weekend trails, and fast day hikes.",
                    "Upper: abrasion-resistant mesh with welded overlays.",
                    "Protection: GORE-TEX membrane and reinforced toe cap.",
                    "Outsole: Contagrip rubber for rock, dirt, and wet paths.",
                    "Use case: three-season hiking and travel.",
                    "Fit system: Quicklace."),
                mainImageUrl: "https://images.unsplash.com/photo-1525966222134-fcfa99b8ae77?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 168.22m,
                discountPercentage: 0,
                discountedPrice: 0m,
                quantity: 9,
                rating: 4.7,
                categoryId: categoryIds["Outdoor"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1500534314209-a25ddb2bd429?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "Coleman Darwin 3 Tent",
                description: BuildDescription(
                    "Compact dome tent for campsite weekends, short festivals, and family road trips where quick setup matters.",
                    "Capacity: sleeps up to three people.",
                    "Weather protection: PU-coated flysheet with taped seams.",
                    "Packed size: compact enough for car camping and weekend travel.",
                    "Use case: spring and summer camping.",
                    "Includes: carry bag and fiberglass poles."),
                mainImageUrl: "https://images.unsplash.com/photo-1504280390367-361c6d9f38f4?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 204.01m,
                discountPercentage: 10,
                discountedPrice: 183.55m,
                quantity: 8,
                rating: 4.4,
                categoryId: categoryIds["Outdoor"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1523987355523-c7b5b84d2d31?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "Giro Register MIPS Cycling Helmet",
                description: BuildDescription(
                    "All-round road and fitness helmet with a straightforward fit system and added rotational impact protection.",
                    "Safety: MIPS liner and in-mold shell construction.",
                    "Ventilation: 22 vents for everyday road riding.",
                    "Fit: Roc Loc Sport adjustment dial.",
                    "Use case: commuting, fitness rides, and weekend training.",
                    "Weight: approximately 280 g."),
                mainImageUrl: "https://images.unsplash.com/photo-1558980664-10ea2928dc44?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 76.18m,
                discountPercentage: 13,
                discountedPrice: 65.96m,
                quantity: 6,
                rating: 4.6,
                categoryId: categoryIds["Cycling"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1507035895480-2b3156c31fc8?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "Wilson Pro Staff RF97 v13 Tennis Racket",
                description: BuildDescription(
                    "A heavier control-oriented racket for advanced players who want a stable response and precise ball placement.",
                    "Head size: 97 sq in.",
                    "Weight: 340 g unstrung.",
                    "String pattern: 16x19.",
                    "Use case: aggressive baseline play and confident net approaches.",
                    "Grip: premium leather feel."),
                mainImageUrl: "https://images.unsplash.com/photo-1622279457486-62dcc4a431d6?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 255.13m,
                discountPercentage: 6,
                discountedPrice: 239.80m,
                quantity: 5,
                rating: 4.8,
                categoryId: categoryIds["Tennis"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1617083934551-7449a7f53f56?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "HEAD Tour XT Tennis Balls 4 Pack",
                description: BuildDescription(
                    "Pressurized tennis balls with a lively bounce and durable felt for club sessions and match practice.",
                    "Pack size: 4 balls.",
                    "Felt: woven cloth suited to hard and clay courts.",
                    "Use case: training, league matches, and coaching baskets.",
                    "Bounce profile: consistent from the first session.",
                    "Approved for regular club play."),
                mainImageUrl: "https://images.unsplash.com/photo-1530915365347-e35b749a0381?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 10.17m,
                discountPercentage: 0,
                discountedPrice: 0m,
                quantity: 42,
                rating: 4.4,
                categoryId: categoryIds["Tennis"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1617083934551-7449a7f53f56?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "Wilson Evolution Indoor Game Basketball",
                description: BuildDescription(
                    "Popular indoor basketball with a soft composite cover and dependable grip for team training and game use.",
                    "Size: 7.",
                    "Cover: microfiber composite for indoor courts.",
                    "Use case: school gyms, club practices, and local league games.",
                    "Feel: cushioned touch with easy hand control.",
                    "Recommended surface: indoor hardwood."),
                mainImageUrl: "https://images.unsplash.com/photo-1546519638-68e109498ffc?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 60.84m,
                discountPercentage: 0,
                discountedPrice: 0m,
                quantity: 13,
                rating: 4.7,
                categoryId: categoryIds["Basketball"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1518063319789-7217e6706b04?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "Spalding Slam Jam Basketball Rim",
                description: BuildDescription(
                    "Powder-coated steel rim for home driveways and school yard setups where dependable hardware matters more than flashy extras.",
                    "Rim size: regulation 45 cm diameter.",
                    "Material: solid steel with weather-resistant finish.",
                    "Includes: net and mounting hardware.",
                    "Use case: outdoor home courts and training walls.",
                    "Compatibility: suitable for most flat backboards."),
                mainImageUrl: "https://images.unsplash.com/photo-1519861531473-9200262188bf?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 76.18m,
                discountPercentage: 0,
                discountedPrice: 0m,
                quantity: 3,
                rating: 4.3,
                categoryId: categoryIds["Basketball"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1505666287802-931dc83948e9?auto=format&fit=crop&w=1200&q=80",
                ]),
            CreateProduct(
                title: "YETI Rambler 26 oz Water Bottle",
                description: BuildDescription(
                    "Insulated stainless steel bottle for the gym bag, hiking pack, or office desk when you need cold water to stay cold.",
                    "Capacity: 769 ml.",
                    "Material: kitchen-grade stainless steel with powder-coated finish.",
                    "Insulation: double-wall vacuum construction.",
                    "Use case: commuting, training, and outdoor sessions.",
                    "Lid: leak-resistant chug cap."),
                mainImageUrl: "https://images.unsplash.com/photo-1602143407151-7111542de6e8?auto=format&fit=crop&w=1200&q=80",
                regularPrice: 40.39m,
                discountPercentage: 0,
                discountedPrice: 0m,
                quantity: 21,
                rating: 4.6,
                categoryId: categoryIds["Accessories"],
                secondaryImageUrls:
                [
                    "https://images.unsplash.com/photo-1544145945-f90425340c7e?auto=format&fit=crop&w=1200&q=80",
                ]),
        ];

        HashSet<string> existingProductTitles = db.Products
            .Where(product => !string.IsNullOrWhiteSpace(product.Title))
            .Select(product => product.Title)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        List<Product> productsToAdd = products
            .Where(product => !existingProductTitles.Contains(product.Title))
            .ToList();

        if (productsToAdd.Count == 0)
        {
            return;
        }

        await db.Products.AddRangeAsync(productsToAdd);
        await db.SaveChangesAsync();
    }

    private static Product CreateProduct(
        string title,
        string description,
        string mainImageUrl,
        decimal regularPrice,
        byte discountPercentage,
        decimal discountedPrice,
        uint quantity,
        double rating,
        Guid categoryId,
        params string[] secondaryImageUrls)
    {
        return new Product
        {
            Title = title,
            Description = description,
            MainImageUrl = mainImageUrl,
            RegularPrice = regularPrice,
            DiscountPercentage = discountPercentage,
            DiscountedPrice = discountedPrice,
            Quantity = quantity,
            Rating = rating,
            CategoryId = categoryId,
            SecondaryImages = secondaryImageUrls.Select(url => new Image { Uri = url }).ToList(),
        };
    }

    private static string BuildDescription(string summary, params string[] bulletPoints)
    {
        StringBuilder builder = new();
        builder.Append("<p>").Append(summary).Append("</p><ul>");

        foreach (string bulletPoint in bulletPoints)
        {
            builder.Append("<li>").Append(bulletPoint).Append("</li>");
        }

        builder.Append("</ul>");
        return builder.ToString();
    }
}
