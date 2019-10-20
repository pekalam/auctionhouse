var categories = [
  {
    id: 0,
    name: "Fashion",
    "sub-categories": [
      {
        id: 0,
        name: "Women&apos;s Clothing",
        "sub-sub-categories": [
          { id: 0, name: "Women&apos;s Dresses" },
          { id: 1, name: "Women&apos;s Tops &amp; Shirts" },
          { id: 2, name: "Women&apos;s Coats, Jackets &amp; Waistcoats" },
          { id: 3, name: "Women&apos;s Lingerie &amp; Nightwear" },
          { id: 4, name: "Women&apos;s Jumpers &amp; Cardigans" },
          { id: 5, name: "Women&apos;s Skirts" }
        ]
      },
      {
        id: 1,
        name: "Women&apos;s Accessories",
        "sub-sub-categories": [
          { id: 0, name: "Women&apos;s Purses &amp; Wallets" },
          { id: 1, name: "Women&apos;s Scarves &amp; Shawls" },
          { id: 2, name: "Women&apos;s Sunglasses &amp; Accessories" },
          { id: 3, name: "Women&apos;s Hats" },
          { id: 4, name: "Women&apos;s Hair Accessories" },
          { id: 5, name: "Women&apos;s Belts" }
        ]
      },
      {
        id: 2,
        name: "Men&apos;s Clothing",
        "sub-sub-categories": [
          { id: 0, name: "Men&apos;s Coats &amp; Jackets" },
          { id: 1, name: "Men&apos;s Shirts &amp; Tops" },
          { id: 2, name: "Men&apos;s Activewear" },
          { id: 3, name: "Men&apos;s Jeans" },
          { id: 4, name: "Men&apos;s Jumpers &amp; Cardigans" },
          { id: 5, name: "Men&apos;s Trousers" }
        ]
      },
      {
        id: 3,
        name: "Men&apos;s Accessories",
        "sub-sub-categories": [
          { id: 0, name: "Men&apos;s Hats" },
          { id: 1, name: "Men&apos;s Bags" },
          { id: 2, name: "Men&apos;s Sunglasses &amp; Sunglasses Accessories" },
          { id: 3, name: "Men&apos;s Wallets" },
          { id: 4, name: "Men&apos;s Belts" },
          { id: 5, name: "Men&apos;s Ties, Bow Ties &amp; Cravats" }
        ]
      },
      {
        id: 4,
        name: "Shoes (Men&apos;s Shoes, Women&apos;s Shoes, Kids&apos; Shoes)",
        "sub-sub-categories": [
          { id: 0, name: "Men&apos;s Trainers" },
          { id: 1, name: "Women&apos;s Boots" },
          { id: 2, name: "Women&apos;s Heels" },
          { id: 3, name: "Women&apos;s Trainers" },
          { id: 4, name: "Women&apos;s Sandals" },
          { id: 5, name: "Men&apos;s Boots" }
        ]
      },
      {
        id: 5,
        name: "Jewellery &amp; Watches",
        "sub-sub-categories": [
          { id: 0, name: "Watches, Parts &amp; Accessories" },
          { id: 1, name: "Fine Jewellery" },
          { id: 2, name: "Costume Jewellery" },
          { id: 3, name: "Vintage &amp; Antique Jewellery" },
          { id: 4, name: "Men&apos;s Jewellery" },
          { id: 5, name: "Jewellery Boxes &amp; Supplies" }
        ]
      },
      {
        id: 6,
        name: "Kids&apos; Clothes, Shoes &amp; Accessories",
        "sub-sub-categories": [
          { id: 0, name: "Girls&apos; Clothing (2-16 Years)" },
          { id: 1, name: "Boys&apos; Clothing 2-16 Years" },
          { id: 2, name: "Girls&apos; Accessories" },
          { id: 3, name: "Boys&apos; Accessories" },
          { id: 4, name: "Other Kids&apos; Clothing &amp; Accessories" }
        ]
      },
      {
        id: 7,
        name:
          "Costume, Occasion &amp; Specialized Clothing, Shoes &amp; Accessories",
        "sub-sub-categories": [
          { id: 0, name: "Fancy Dress &amp; Period Costumes" },
          { id: 1, name: "Wedding &amp; Formal Occasion Clothing" },
          { id: 2, name: "Vintage Clothing &amp; Accessories" },
          { id: 3, name: "Dancewear &amp; Accessories" },
          { id: 4, name: "World &amp; Traditional Clothing" },
          { id: 5, name: "Erotic Clothing" }
        ]
      },
      {
        id: 8,
        name: "Top Brands",
        "sub-sub-categories": [
          { id: 0, name: "Rolex" },
          { id: 1, name: "Louis Vuitton" },
          { id: 2, name: "VANS" },
          { id: 3, name: "Seiko" },
          { id: 4, name: "OMEGA" },
          { id: 5, name: "Camper" },
          { id: 6, name: "Gucci" },
          { id: 7, name: "Supreme" },
          { id: 8, name: "Ted Baker" },
          { id: 9, name: "Nike" }
        ]
      },
      {
        id: 9,
        name: "Popular Topics",
        "sub-sub-categories": [
          { id: 0, name: "PANDORA Fine Charms and Charm Bracelets" },
          { id: 1, name: "Women&apos;s Coats, Jackets &amp; Waistcoats" },
          { id: 2, name: "Wristwatches" },
          { id: 3, name: "Women&apos;s Bags &amp; Handbags" },
          { id: 4, name: "Yeezy Trainers for Men" },
          { id: 5, name: "Mulberry Bags &amp; Handbags for Women" },
          { id: 6, name: "Air Max Trainers for Men" },
          { id: 7, name: "Tops &amp; Shirts Size 14 for Women" },
          { id: 8, name: "Men&apos;s Coats &amp; Jackets" },
          { id: 9, name: "PANDORA Other Fine Rings" }
        ]
      },
      {
        id: 10,
        name: "Top Interests",
        "sub-sub-categories": [
          { id: 0, name: "Women&apos;s Edgy Style" },
          { id: 1, name: "Men&apos;s Edgy Style" },
          { id: 2, name: "Women&apos;s Beach Style" },
          { id: 3, name: "Women&apos;s Bridal Style" },
          { id: 4, name: "Curvy Women&apos;s Style" },
          { id: 5, name: "Women&apos;s Mod &amp; Retro Style" },
          { id: 6, name: "Kids&apos; Clothing" },
          { id: 7, name: "Women&apos;s American Designer Fashion" },
          { id: 8, name: "Women&apos;s Gothic Style" },
          { id: 9, name: "Women&apos;s Boho Chic Style" }
        ]
      }
    ]
  },
  {
    id: 1,
    name: "Electronics",
    "sub-categories": [
      {
        id: 0,
        name: "Mobile Phones, Smart Watches, Accessories &amp; Communication",
        "sub-sub-categories": [
          { id: 0, name: "Mobile Phones &amp; Smartphones" },
          { id: 1, name: "Mobile Phone Accessories" },
          { id: 2, name: "Smartwatches" },
          { id: 3, name: "Mobile Phone Parts" },
          { id: 4, name: "Radio Communication Equipment" },
          { id: 5, name: "Home Phones &amp; Accessories" }
        ]
      },
      {
        id: 1,
        name: "Computers, Tablets &amp; Network Hardware",
        "sub-sub-categories": [
          { id: 0, name: "Laptops &amp; Netbooks" },
          { id: 1, name: "Computer Components &amp; Parts" },
          { id: 2, name: "Tablets &amp; eReaders" },
          { id: 3, name: "Desktops &amp; All-In-One Computers" },
          { id: 4, name: "Computer Printers, Scanners &amp; Supplies" },
          { id: 5, name: "Tablet &amp; eReader Accessories" }
        ]
      },
      {
        id: 2,
        name: "Sound &amp; Vision",
        "sub-sub-categories": [
          { id: 0, name: "Home Audio Systems" },
          { id: 1, name: "Portable Audio Equipment &amp; Headphones" },
          { id: 2, name: "TV &amp; Home Audio Accessories" },
          { id: 3, name: "TVs" },
          { id: 4, name: "Performance &amp; DJ Equipment" },
          { id: 5, name: "DVD, Blu-ray &amp; Home Cinema" }
        ]
      },
      {
        id: 3,
        name: "Video Games &amp; Consoles",
        "sub-sub-categories": [
          { id: 0, name: "Video Game Consoles" },
          { id: 1, name: "Video Games" },
          { id: 2, name: "Video Game Accessories" },
          { id: 3, name: "Coin-Operated Gaming" },
          { id: 4, name: "Video Game Merchandise" },
          { id: 5, name: "Other Video Games &amp; Consoles" }
        ]
      },
      {
        id: 4,
        name: "Cameras &amp; Photography Equipment",
        "sub-sub-categories": [
          { id: 0, name: "Digital Cameras" },
          { id: 1, name: "Camera Lenses &amp; Filters" },
          { id: 2, name: "Film Photography Equipment" },
          { id: 3, name: "Camera &amp; Photo Accessories" },
          { id: 4, name: "Camcorders" },
          { id: 5, name: "Camera &amp; Camera-Ready Drones" }
        ]
      },
      {
        id: 5,
        name: "Top Brands",
        "sub-sub-categories": [
          { id: 0, name: "Bang &amp; Olufsen" },
          { id: 1, name: "Lego" },
          { id: 2, name: "Bose" },
          { id: 3, name: "DJI" },
          { id: 4, name: "GoPro" },
          { id: 5, name: "Lenovo" },
          { id: 6, name: "Samsung" },
          { id: 7, name: "Mirror" },
          { id: 8, name: "Nikon" },
          { id: 9, name: "Quad" }
        ]
      },
      {
        id: 6,
        name: "Popular Topics",
        "sub-sub-categories": [
          { id: 0, name: "Apple iPhone 7" },
          { id: 1, name: "TVs" },
          { id: 2, name: "Samsung Galaxy S8 Smartphones" },
          { id: 3, name: "Samsung Galaxy S7 edge Smartphones" },
          {
            id: 4,
            name: "6.0&quot; &amp; Over Screen Mobile and Smart Phones"
          },
          { id: 5, name: "Sony PlayStation 4 Consoles||*" },
          { id: 6, name: "Mobile Phones &amp; Smartphones" },
          { id: 7, name: "Samsung Galaxy S7 Smartphones" },
          { id: 8, name: "Apple Phones" },
          { id: 9, name: "PC Laptops &amp; Netbooks" }
        ]
      },
      {
        id: 7,
        name: "Top Interests",
        "sub-sub-categories": [
          { id: 0, name: "Virtual Reality" },
          { id: 1, name: "3D Printing" },
          { id: 2, name: "Home Cinema" },
          { id: 3, name: "PC Gaming" },
          { id: 4, name: "Hi-End Audio" },
          { id: 5, name: "Apple Products" },
          { id: 6, name: "Gadgets" },
          { id: 7, name: "Nintendo" },
          { id: 8, name: "Photography" },
          { id: 9, name: "Robotics &amp; Drones" }
        ]
      }
    ]
  },
  {
    id: 2,
    name: "Sports, Hobbies &amp; Leisure",
    "sub-categories": [
      {
        id: 0,
        name: "Sporting Goods",
        "sub-sub-categories": [
          { id: 0, name: "Cycling Equipment" },
          { id: 1, name: "Golf Equipment" },
          { id: 2, name: "Fishing Equipment" },
          { id: 3, name: "Fitness, Running &amp; Yoga Equipment" },
          { id: 4, name: "Camping &amp; Hiking Equipment" },
          { id: 5, name: "Equestrian Equipment" }
        ]
      },
      {
        id: 1,
        name: "Musical Instruments",
        "sub-sub-categories": [
          { id: 0, name: "Guitars &amp; Basses" },
          { id: 1, name: "Pro Audio Equipment" },
          { id: 2, name: "Percussion Instruments" },
          { id: 3, name: "Keyboards &amp; Pianos" },
          { id: 4, name: "String Instruments" },
          { id: 5, name: "Wind &amp; Woodwind Instruments" }
        ]
      },
      {
        id: 2,
        name: "Art &amp; Craft Supplies",
        "sub-sub-categories": [
          { id: 0, name: "Fabric" },
          { id: 1, name: "Sewing Tools &amp; Supplies" },
          { id: 2, name: "Yarn, Sewing &amp; Needlecraft Supplies" },
          { id: 3, name: "Cardmaking &amp; Scrapbooking Supplies" },
          { id: 4, name: "Painting, Drawing &amp; Art Supplies" },
          { id: 5, name: "Beads &amp; Jewellery Making Supplies" }
        ]
      },
      {
        id: 3,
        name: "Top Brands",
        "sub-sub-categories": [
          { id: 0, name: "Specialized" },
          { id: 1, name: "Trek" },
          { id: 2, name: "Fitbit" },
          { id: 3, name: "Slime" },
          { id: 4, name: "Santa Cruz" },
          { id: 5, name: "Giant" },
          { id: 6, name: "Pinarello" },
          { id: 7, name: "Colnago" },
          { id: 8, name: "Brompton" },
          { id: 9, name: "Scotty Cameron" }
        ]
      },
      {
        id: 4,
        name: "Popular Topics",
        "sub-sub-categories": [
          { id: 0, name: "Electric Bikes" },
          { id: 1, name: "Bikes" },
          { id: 2, name: "Fender Electric Guitars" },
          { id: 3, name: "Giant Bikes" },
          { id: 4, name: "Specialized Bikes" },
          { id: 5, name: "Downhill Bikes" },
          { id: 6, name: "Garmin GPS &amp; Running Watches" },
          { id: 7, name: "Gibson Les Paul Electric Guitars" },
          { id: 8, name: "Fishing Boats" },
          { id: 9, name: "Gibson Electric Guitars" }
        ]
      },
      {
        id: 5,
        name: "Top Interests",
        "sub-sub-categories": [
          { id: 0, name: "Fishing" },
          { id: 1, name: "Game Night" },
          { id: 2, name: "Beer" },
          { id: 3, name: "Music Festivals" },
          { id: 4, name: "Painting &amp; Drawing" },
          { id: 5, name: "Mountain Biking" },
          { id: 6, name: "Skateboarding" },
          { id: 7, name: "Equestrian &amp; Horse Riding" },
          { id: 8, name: "Field Hockey" },
          { id: 9, name: "Archery" }
        ]
      }
    ]
  },
  {
    id: 3,
    name: "Home Garden",
    "sub-categories": [
      {
        id: 0,
        name: "Household Accessories &amp; Supplies",
        "sub-sub-categories": [
          { id: 0, name: "DIY Materials" },
          { id: 1, name: "Home Appliances" },
          { id: 2, name: "Home Décor Items" },
          { id: 3, name: "Home Cookware, Dining &amp; Bar Supplies" },
          { id: 4, name: "Home Lighting" },
          { id: 5, name: "Home Bedding" }
        ]
      },
      {
        id: 1,
        name: "Furniture",
        "sub-sub-categories": [
          { id: 0, name: "Sofas, Armchairs &amp; Suites" },
          { id: 1, name: "Beds &amp; Mattresses" },
          { id: 2, name: "Tables" },
          { id: 3, name: "Cabinets &amp; Cupboards" },
          { id: 4, name: "Chairs" },
          { id: 5, name: "Table &amp; Chair Sets" }
        ]
      },
      {
        id: 2,
        name: "Garden &amp; Patio",
        "sub-sub-categories": [
          { id: 0, name: "Garden Structures &amp; Shade Equipment" },
          { id: 1, name: "Garden Power Tools &amp; Equipment" },
          { id: 2, name: "Garden &amp; Patio Furniture" },
          { id: 3, name: "Lawn Mowers" },
          { id: 4, name: "Outdoor Heating, Cooking &amp; Eating" },
          { id: 5, name: "Landscaping &amp; Garden Materials" }
        ]
      },
      {
        id: 3,
        name: "Baby Essentials",
        "sub-sub-categories": [
          { id: 0, name: "Baby Clothes, Shoes &amp; Accessories" },
          { id: 1, name: "Pushchairs, Prams &amp; Accessories" },
          { id: 2, name: "Nursery Decoration &amp; Furniture" },
          { id: 3, name: "Baby Feeding Supplies" },
          { id: 4, name: "Nursery Bedding" },
          { id: 5, name: "Baby Toys &amp; Activities" }
        ]
      },
      {
        id: 4,
        name: "Pet Supplies",
        "sub-sub-categories": [
          { id: 0, name: "Dog Supplies" },
          { id: 1, name: "Fish &amp; Aquarium Supplies" },
          { id: 2, name: "Small Animal Supplies" },
          { id: 3, name: "Cat Supplies" },
          { id: 4, name: "Poultry &amp; Waterfowl Supplies" },
          { id: 5, name: "Bird Supplies" }
        ]
      },
      {
        id: 5,
        name: "Celebration &amp; Occasion Supplies",
        "sub-sub-categories": [
          { id: 0, name: "Party Supplies" },
          { id: 1, name: "Christmas Decorations &amp; Trees" },
          { id: 2, name: "Celebrations &amp; Occasions" },
          {
            id: 3,
            name: "Cards &amp; Invitations for Celebrations &amp; Occasions"
          },
          { id: 4, name: "Gift Wrapping &amp; Supplies" },
          { id: 5, name: "Memorial &amp; Funeral Supplies" }
        ]
      },
      {
        id: 6,
        name: "Bathroom Accessories &amp; Fittings",
        "sub-sub-categories": [
          { id: 0, name: "Shower Plumbing Supplies" },
          { id: 1, name: "Home Bathroom Sinks" },
          { id: 2, name: "Shower Doors, Trays Panels &amp; Enclosures" },
          { id: 3, name: "Bathroom Taps" },
          { id: 4, name: "Baths" },
          { id: 5, name: "Toilets &amp; Bidets" }
        ]
      },
      {
        id: 7,
        name: "DIY Tools &amp; Workshop Equipment",
        "sub-sub-categories": [
          { id: 0, name: "Power Tools" },
          { id: 1, name: "Home Hand Tools" },
          { id: 2, name: "Home Workshop &amp; Storage Equipment" },
          { id: 3, name: "Tool Boxes &amp; Storage" },
          { id: 4, name: "Home Measuring &amp; Layout Tools" },
          { id: 5, name: "DIY Tools &amp; Workshop Equipment" }
        ]
      },
      {
        id: 8,
        name: "Top Brands",
        "sub-sub-categories": [
          { id: 0, name: "DEWALT" },
          { id: 1, name: "Ercol" },
          { id: 2, name: "Festool" },
          { id: 3, name: "IKEA" },
          { id: 4, name: "Milwaukee" },
          { id: 5, name: "STIHL" },
          { id: 6, name: "Makita" },
          { id: 7, name: "Hilti" },
          { id: 8, name: "Duresta" },
          { id: 9, name: "Unicorn" }
        ]
      },
      {
        id: 9,
        name: "Popular Topics",
        "sub-sub-categories": [
          { id: 0, name: "Sofas" },
          { id: 1, name: "Corner/Sectional Sofas" },
          { id: 2, name: "Pushchair &amp; Pram Travel Systems" },
          { id: 3, name: "Sofa Beds" },
          { id: 4, name: "Vintage/Retro Furniture" },
          { id: 5, name: "Dyson Vacuum Cleaners" },
          { id: 6, name: "Bunk Bed Frames" },
          { id: 7, name: "Garden Sheds" },
          { id: 8, name: "Complete Kitchen Units" },
          { id: 9, name: "Tank Only Aquarium" }
        ]
      },
      {
        id: 10,
        name: "Top Interests",
        "sub-sub-categories": [
          { id: 0, name: "Traditional Home" },
          { id: 1, name: "Modern Spaces" },
          { id: 2, name: "Travel" },
          { id: 3, name: "Home DIY" },
          { id: 4, name: "Mid-Century Modern" },
          { id: 5, name: "Pet Fish" },
          { id: 6, name: "Emojis" },
          { id: 7, name: "Rustic Home" },
          { id: 8, name: "Coffee" },
          { id: 9, name: "Asian Interiors" }
        ]
      }
    ]
  },
  {
    id: 4,
    name: "Motors",
    "sub-categories": [
      {
        id: 0,
        name: "Vehicle Parts &amp; Accessories",
        "sub-sub-categories": [
          { id: 0, name: "Car Parts" },
          { id: 1, name: "Car Tuning &amp; Styling Parts" },
          { id: 2, name: "Car Manuals &amp; Literature" },
          { id: 3, name: "Car Wheels, Tyres &amp; Trims" },
          { id: 4, name: "Car Accessories" },
          { id: 5, name: "Garage Equipment &amp; Tools" }
        ]
      },
      {
        id: 1,
        name: "Cars, Motorcycles &amp; Vehicles",
        "sub-sub-categories": [
          { id: 0, name: "Cars" },
          { id: 1, name: "Commercial Vehicles" },
          { id: 2, name: "Motorcycles &amp; Scooters" },
          { id: 3, name: "Campervans, Caravans &amp; Motorhomes" },
          { id: 4, name: "Classic Cars" },
          { id: 5, name: "Boats &amp; Watercraft" }
        ]
      },
      {
        id: 2,
        name: "Top Vehicle Makes",
        "sub-sub-categories": [
          { id: 0, name: "Ford" },
          { id: 1, name: "Volkswagen" },
          { id: 2, name: "Vauxhall" },
          { id: 3, name: "BMW" },
          { id: 4, name: "Audi" },
          { id: 5, name: "Mercedes-Benz" },
          { id: 6, name: "Renault" },
          { id: 7, name: "Peugeot" },
          { id: 8, name: "Toyota" },
          { id: 9, name: "Nissan" }
        ]
      },
      {
        id: 3,
        name: "Top Interests",
        "sub-sub-categories": [
          { id: 0, name: "Classical Cars" },
          { id: 1, name: "Car Tuning &amp; Styling" },
          { id: 2, name: "Boats &amp; Watercraft" },
          { id: 3, name: "Convertibles &amp; Cabriolets" },
          { id: 4, name: "Supercars" },
          { id: 5, name: "Classical Sports Cars" },
          { id: 6, name: "Campervans &amp; Motorhomes" },
          { id: 7, name: "Electric &amp; Hybrid Cars" }
        ]
      }
    ]
  },
  {
    id: 5,
    name: "Collectables &amp; Art",
    "sub-categories": [
      {
        id: 0,
        name: "Toys &amp; Games",
        "sub-sub-categories": [
          { id: 0, name: "Action Figures" },
          { id: 1, name: "RC Model Vehicles &amp; Remote-Controlled Toys" },
          { id: 2, name: "Diecast Vehicles, Parts &amp; Accessories" },
          { id: 3, name: "Outdoor Toys &amp; Activities" },
          { id: 4, name: "Construction &amp; Building Toys" },
          { id: 5, name: "Wargames &amp; Role-Playing Games" }
        ]
      },
      {
        id: 1,
        name: "Collectables",
        "sub-sub-categories": [
          { id: 0, name: "Model Railways &amp; Trains" },
          { id: 1, name: "Militaria" },
          { id: 2, name: "Tobacciana &amp; Smoking Supplies" },
          { id: 3, name: "Transportation Collectables" },
          { id: 4, name: "Collectable Tools &amp; Hardware" },
          { id: 5, name: "Collectible Card Games &amp; Accessories" }
        ]
      },
      {
        id: 2,
        name: "Antiques",
        "sub-sub-categories": [
          { id: 0, name: "Antique Furniture" },
          { id: 1, name: "Architectural Antiques" },
          { id: 2, name: "Antique Silver" },
          { id: 3, name: "Antique Clocks" },
          { id: 4, name: "Asian Antiques" },
          { id: 5, name: "Antique Woodenware" }
        ]
      },
      {
        id: 3,
        name: "Pottery, Porcelain &amp; Glass",
        "sub-sub-categories": [
          { id: 0, name: "Pottery" },
          { id: 1, name: "Porcelain &amp; China" },
          { id: 2, name: "Glass" },
          { id: 3, name: "Date-Lined Ceramics" },
          { id: 4, name: "Stoneware" }
        ]
      },
      {
        id: 4,
        name: "Coins, Banknotes &amp; Bullion",
        "sub-sub-categories": [
          { id: 0, name: "Coins" },
          { id: 1, name: "Banknotes" },
          { id: 2, name: "Virtual Currency, Miners &amp; Mining Contracts" },
          { id: 3, name: "Bullion &amp; Bars" },
          { id: 4, name: "Historical Medals &amp; Medallions" },
          { id: 5, name: "Token Coins" }
        ]
      },
      {
        id: 5,
        name: "Dolls &amp; Teddy Bears",
        "sub-sub-categories": [
          { id: 0, name: "Dolls, Clothing &amp; Accessories" },
          { id: 1, name: "Teddy Bears" },
          { id: 2, name: "Dolls&apos; Miniatures &amp; Houses" }
        ]
      },
      {
        id: 6,
        name: "Art",
        "sub-sub-categories": [
          { id: 0, name: "Art Prints" },
          { id: 1, name: "Art Paintings" },
          { id: 2, name: "Art Posters" },
          { id: 3, name: "Art Sculptures" },
          { id: 4, name: "Art Photographs" },
          { id: 5, name: "Art Drawings" }
        ]
      },
      {
        id: 7,
        name: "Sports Memorabilia",
        "sub-sub-categories": [
          { id: 0, name: "Memorabilia Football Shirts" },
          { id: 1, name: "Football Memorabilia" },
          { id: 2, name: "Motor Sport Memorabilia" },
          { id: 3, name: "Sports Stickers, Sets &amp; Albums" },
          { id: 4, name: "Football Programmes" },
          { id: 5, name: "Sports Trading Cards &amp; Accessories" }
        ]
      },
      {
        id: 8,
        name: "Top Brands",
        "sub-sub-categories": [
          { id: 0, name: "Lego" },
          { id: 1, name: "Fender" },
          { id: 2, name: "Tamiya" },
          { id: 3, name: "Littlest Pet Shop" },
          { id: 4, name: "Marshall" },
          { id: 5, name: "Hornby" },
          { id: 6, name: "Charlie Bears" },
          { id: 7, name: "Scalextric" },
          { id: 8, name: "NERF" },
          { id: 9, name: "Meccano" }
        ]
      },
      {
        id: 9,
        name: "Popular Topics",
        "sub-sub-categories": [
          { id: 0, name: "Toys &amp; Games" },
          { id: 1, name: "LEGO Complete Sets &amp; Packs" },
          { id: 2, name: "RC Parts &amp; Accessories" },
          { id: 3, name: "Warhammer 40K Miniatures" },
          { id: 4, name: "Rare 50P Coin" },
          { id: 5, name: "Peter Rabbit 50p Coins" },
          { id: 6, name: "Raleigh Choppers" },
          { id: 7, name: "Art Prints" },
          { id: 8, name: "Creative Toys &amp; Activities" },
          { id: 9, name: "Mixed Wholesale &amp; Job Lots" }
        ]
      },
      {
        id: 10,
        name: "Top Interests",
        "sub-sub-categories": [
          { id: 0, name: "Modern Art" },
          { id: 1, name: "Nostalgic Toys" },
          { id: 2, name: "Fossils &amp; Minerals" },
          { id: 3, name: "Sci-Fi" },
          { id: 4, name: "Scrapbooking" },
          { id: 5, name: "Fantasy" },
          { id: 6, name: "Superheroes" },
          { id: 7, name: "Action Figure Collecting" },
          { id: 8, name: "Comic Book Collecting" },
          { id: 9, name: "Stamp Collecting" }
        ]
      }
    ]
  },
  {
    id: 6,
    name: "Business, Office &amp; Industrial Supplies",
    "sub-categories": [
      {
        id: 0,
        name: "Agriculture &amp; Farming Equipment",
        "sub-sub-categories": [
          { id: 0, name: "Tractors" },
          { id: 1, name: "Farm Implements &amp; Equipment" },
          { id: 2, name: "Agriculture &amp; Farming Trailers" },
          { id: 3, name: "Equipment Parts &amp; Accessories" },
          { id: 4, name: "Agriculture &amp; Farming Stationary Engines" },
          { id: 5, name: "Agriculture &amp; Farming Storage Tanks" }
        ]
      },
      {
        id: 1,
        name: "Industrial Tools",
        "sub-sub-categories": [
          { id: 0, name: "Construction Tools" },
          { id: 1, name: "Industrial Generators" },
          { id: 2, name: "Industrial Drilling Tools" },
          { id: 3, name: "Industrial Measuring Tools" },
          { id: 4, name: "Industrial Cutting &amp; Sawing Tools" },
          { id: 5, name: "Industrial Engineering Tools" }
        ]
      },
      {
        id: 2,
        name: "Industrial Power Tools",
        "sub-sub-categories": [
          { id: 0, name: "Industrial Power Drills, Bits &amp; Accessories" },
          { id: 1, name: "Industrial Power Saws &amp; Saw Blades" },
          { id: 2, name: "Industrial Power Tool Batteries &amp; Chargers" },
          { id: 3, name: "Industrial Nail Guns &amp; Accessories" },
          { id: 4, name: "Industrial Power Grinders &amp; Accessories" },
          { id: 5, name: "Industrial Power Tool Routers &amp; Accessories" }
        ]
      },
      {
        id: 3,
        name: "CNC, Metalworking &amp; Manufacturing",
        "sub-sub-categories": [
          { id: 0, name: "Welding &amp; Soldering Equipment" },
          { id: 1, name: "Metalworking Lathes" },
          { id: 2, name: "Metalworking Supplies" },
          { id: 3, name: "Raw Materials" },
          {
            id: 4,
            name: "Metalworking Machining Centers &amp; Milling Machines"
          },
          { id: 5, name: "Anvils" }
        ]
      },
      {
        id: 4,
        name: "Office Equipment &amp; Supplies",
        "sub-sub-categories": [
          { id: 0, name: "Office Supplies &amp; Stationery" },
          { id: 1, name: "Office Furniture" },
          { id: 2, name: "Office Equipment" },
          { id: 3, name: "Presentation &amp; A/V Equipment &amp; Projectors" },
          { id: 4, name: "Trade Show Displays &amp; Stands" },
          { id: 5, name: "Copiers &amp; Copier Supplies" }
        ]
      },
      {
        id: 5,
        name: "Building Materials &amp; Supplies",
        "sub-sub-categories": [
          { id: 0, name: "Scaffolding &amp; Ladders" },
          { id: 1, name: "Roofing Materials &amp; Supplies" },
          { id: 2, name: "Building Materials &amp; Supplies" },
          { id: 3, name: "Building Insulation Materials &amp; Supplies" },
          { id: 4, name: "Electrical Building Materials &amp; Supplies" },
          { id: 5, name: "Plumbing Materials &amp; Supplies" }
        ]
      },
      {
        id: 6,
        name: "Restaurant &amp; Food Service",
        "sub-sub-categories": [
          { id: 0, name: "Food Trucks &amp; Concession Trailers" },
          { id: 1, name: "Commercial Kitchen Equipment" },
          { id: 2, name: "Commercial Bar &amp; Beverage Equipment" },
          { id: 3, name: "Commercial Restaurant Refrigeration" },
          { id: 4, name: "Restaurant Décor &amp; Furniture" },
          { id: 5, name: "Restaurant Tabletop &amp; Serving" }
        ]
      },
      {
        id: 7,
        name: "Material Handling",
        "sub-sub-categories": [
          { id: 0, name: "Packing &amp; Shipping" },
          { id: 1, name: "Warehouse Loading &amp; Unloading" },
          { id: 2, name: "Hoists, Winches &amp; Rigging" },
          { id: 3, name: "Other Material Handling" },
          { id: 4, name: "Warehouse Carts &amp; Trucks" },
          { id: 5, name: "Conveyors &amp; Conveyor Parts" }
        ]
      },
      {
        id: 8,
        name: "Industrial Hand Tools",
        "sub-sub-categories": [
          { id: 0, name: "Business &amp; Industrial Measuring Tools" },
          { id: 1, name: "Industrial Vices" },
          { id: 2, name: "Industrial Screwdrivers" },
          { id: 3, name: "Industrial Spanners" },
          { id: 4, name: "Industrial Pliers" },
          { id: 5, name: "Industrial Wire Strippers &amp; Cutters" }
        ]
      },
      {
        id: 9,
        name: "Retail &amp; Shop Fitting Equipment",
        "sub-sub-categories": [
          { id: 0, name: "Retail Shelving &amp; Racking" },
          { id: 1, name: "Retail &amp; Shop Displays" },
          { id: 2, name: "Point of Sale (POS) Equipment" },
          { id: 3, name: "Retail &amp; Shop Mannequins" },
          {
            id: 4,
            name: "Retail &amp; Shop Surveillance, CCTV &amp; Security Systems"
          },
          { id: 5, name: "Candy &amp; Bulk Vending Machines" }
        ]
      },
      {
        id: 10,
        name: "Test, Measurement &amp; Inspection Equipment",
        "sub-sub-categories": [
          { id: 0, name: "Test Meters &amp; Detectors" },
          { id: 1, name: "Analysers &amp; Data Acquisition Equipment" },
          { id: 2, name: "Test, Measurement &amp; Inspection" },
          { id: 3, name: "Levels &amp; Surveying Equipment" },
          {
            id: 4,
            name:
              "Test, Measurement &amp; Inspection Cameras &amp; Imaging Equipment"
          },
          { id: 5, name: "Testers &amp; Calibrators" }
        ]
      },
      {
        id: 11,
        name: "Electrical Equipment &amp; Supplies",
        "sub-sub-categories": [
          { id: 0, name: "Electronic Components &amp; Semiconductors" },
          { id: 1, name: "Wire, Cable &amp; Conduit" },
          { id: 2, name: "Wire &amp; Cable Connectors" },
          { id: 3, name: "Circuit Breakers &amp; Disconnectors" },
          { id: 4, name: "Industrial Power Supplies" },
          { id: 5, name: "Industrial Switches" }
        ]
      },
      {
        id: 12,
        name: "Facility Maintenance &amp; Safety",
        "sub-sub-categories": [
          { id: 0, name: "Personal Protective Equipment (PPE)" },
          { id: 1, name: "Public Safety Staff Equipment" },
          { id: 2, name: "Access Control Equipment" },
          { id: 3, name: "Industrial Fire Protection Equipment" },
          { id: 4, name: "Safety Signs &amp; Traffic Control" },
          { id: 5, name: "Industrial Surveillance &amp; Alarm Equipment" }
        ]
      },
      {
        id: 13,
        name: "Woodworking Equipment",
        "sub-sub-categories": [
          { id: 0, name: "Woodworking Supplies" },
          { id: 1, name: "Plastic Processing Equipment &amp; Supplies" },
          { id: 2, name: "Other Manufacturing &amp; Woodworking Equipment" },
          { id: 3, name: "Textile &amp; Clothes Manufacturing Equipment" },
          { id: 4, name: "Manufacturing &amp; Woodworking Process Equipment" }
        ]
      },
      {
        id: 14,
        name: "Popular Topics",
        "sub-sub-categories": [
          { id: 0, name: "Farm Implements &amp; Equipment" },
          { id: 1, name: "Tractors" },
          { id: 2, name: "Food Trucks &amp; Concession Trailers" },
          { id: 3, name: "Industrial Fasteners &amp; Hardware" },
          { id: 4, name: "Ifor Williams Agriculture &amp; Farming Trailers" },
          { id: 5, name: "Commercial Shipping &amp; Storage Containers" },
          { id: 6, name: "Warehouse Forklifts &amp; Stackers" },
          { id: 7, name: "Industrial Mini Diggers" },
          { id: 8, name: "John Deere Modern Tractors" },
          { id: 9, name: "Office Chairs" }
        ]
      }
    ]
  },
  {
    id: 7,
    name: "Health &amp; Beauty",
    "sub-categories": [
      {
        id: 0,
        name: "Make-Up Products",
        "sub-sub-categories": [
          { id: 0, name: "Eye Make-Up" },
          { id: 1, name: "Face Make-Up" },
          { id: 2, name: "Lip Make-Up Products" },
          { id: 3, name: "Make-Up Tools &amp; Accessories" },
          { id: 4, name: "Make-Up Cases &amp; Bags" },
          { id: 5, name: "Make-Up Sets &amp; Kits" }
        ]
      },
      {
        id: 1,
        name: "Hair Care &amp; Styling Products",
        "sub-sub-categories": [
          { id: 0, name: "Hair Extension &amp; Wigs" },
          { id: 1, name: "Hair Straighteners &amp; Curling Tongs" },
          { id: 2, name: "Shampoos &amp; Conditioners" },
          { id: 3, name: "Hair Colourants" },
          { id: 4, name: "Hair Dryers" },
          { id: 5, name: "Hair Styling Products" }
        ]
      },
      {
        id: 2,
        name: "Fragrances &amp; Aftershaves",
        "sub-sub-categories": [
          { id: 0, name: "Fragrances for Women" },
          { id: 1, name: "Fragrances &amp; Aftershaves for Men" },
          { id: 2, name: "Unisex Fragrances" },
          { id: 3, name: "Fragrances for Children" }
        ]
      },
      {
        id: 3,
        name: "Medical &amp; Mobility",
        "sub-sub-categories": [
          { id: 0, name: "Mobility/Walking Equipment" },
          { id: 1, name: "Orthopaedic Products &amp; Supports" },
          { id: 2, name: "Mobility Furniture &amp; Fixtures" },
          { id: 3, name: "Daily Living Aids" },
          { id: 4, name: "Other Mobility, Disability &amp; Medical Equipment" }
        ]
      },
      {
        id: 4,
        name: "Manicure &amp; Pedicure Supplies",
        "sub-sub-categories": [
          { id: 0, name: "Gel Nail Polish" },
          { id: 1, name: "Nail Art Accessories" },
          { id: 2, name: "Nail Polish" },
          { id: 3, name: "Manicure &amp; Pedicure Tools &amp; Kits" },
          { id: 4, name: "Nail Dryers &amp; UV/LED Lamps" },
          { id: 5, name: "Artificial Nail Tips" }
        ]
      },
      {
        id: 5,
        name: "Facial Skin Care Products",
        "sub-sub-categories": [
          { id: 0, name: "Anti-Ageing Products" },
          { id: 1, name: "Facial Moisturisers" },
          { id: 2, name: "Acne &amp; Blemish Treatments" },
          { id: 3, name: "Skin Cleansers &amp; Toners" },
          { id: 4, name: "Skin Masks &amp; Peels" },
          { id: 5, name: "Other Facial Skin Care Products" }
        ]
      },
      {
        id: 6,
        name: "Shaving &amp; Hair Removal Products &amp; Equipment",
        "sub-sub-categories": [
          { id: 0, name: "Electric Shavers" },
          { id: 1, name: "Razors &amp; Razor Blades" },
          { id: 2, name: "Hair Clippers &amp; Trimmers" },
          { id: 3, name: "Waxing Supplies for Hair Removal" },
          { id: 4, name: "Epilators &amp; Electrolysis" },
          { id: 5, name: "Laser Hair Removal &amp; IPL Equipment" }
        ]
      },
      {
        id: 7,
        name: "Vitamins &amp; Dietary Supplements",
        "sub-sub-categories": [
          { id: 0, name: "Vitamins &amp; Minerals Health Supplements" },
          { id: 1, name: "Weight Loss Products &amp; Slimming Aids" },
          { id: 2, name: "Herb &amp; Botanical Supplements" },
          { id: 3, name: "Other Vitamins &amp; Dietary Supplements" },
          { id: 4, name: "Dietary Supplements" },
          { id: 5, name: "Endurance &amp; Energy Bars, Shakes &amp; Pills" }
        ]
      },
      {
        id: 8,
        name: "Healthcare Products",
        "sub-sub-categories": [
          { id: 0, name: "Over-the-Counter Medications &amp; Treatments" },
          { id: 1, name: "Foot Creams &amp; Treatments" },
          { id: 2, name: "First Aid Supplies" },
          { id: 3, name: "Over-The-Counter Diabetic Care Products" },
          { id: 4, name: "Hot &amp; Cold Therapies" },
          { id: 5, name: "Medical Monitoring &amp; Testing Kits" }
        ]
      },
      {
        id: 9,
        name: "Bath &amp; Body Products",
        "sub-sub-categories": [
          { id: 0, name: "Bath &amp; Body Mixed Items &amp; Gift Sets" },
          { id: 1, name: "Body Lotions &amp; Moisturisers" },
          { id: 2, name: "Other Bath &amp; Body Products" },
          { id: 3, name: "Bath Bombs" },
          { id: 4, name: "Deodorants &amp; Anti-Perspirants" },
          { id: 5, name: "Body Cleansers" }
        ]
      },
      {
        id: 10,
        name: "Oral &amp; Dental Care Products",
        "sub-sub-categories": [
          { id: 0, name: "Electric Toothbrushes" },
          { id: 1, name: "Electric Toothbrush Heads" },
          { id: 2, name: "Teeth Whitening Products" },
          { id: 3, name: "Other Oral &amp; Dental Care Products" },
          { id: 4, name: "Toothpaste" },
          { id: 5, name: "Dental Floss &amp; Flossers" }
        ]
      },
      {
        id: 11,
        name: "Top Brands",
        "sub-sub-categories": [
          { id: 0, name: "Kylie Minogue" },
          { id: 1, name: "Jo Malone" },
          { id: 2, name: "Slimming World" },
          { id: 3, name: "Estée Lauder" },
          { id: 4, name: "Lush" },
          { id: 5, name: "Tom Ford" },
          { id: 6, name: "Olaplex" },
          { id: 7, name: "Urban Decay" },
          { id: 8, name: "ghd" },
          { id: 9, name: "Elemis" }
        ]
      },
      {
        id: 12,
        name: "Top Interests",
        "sub-sub-categories": [
          { id: 0, name: "Makeup" },
          { id: 1, name: "Fragrance &amp; Perfume" },
          { id: 2, name: "Natural Beauty Products" },
          { id: 3, name: "Tattoos" },
          { id: 4, name: "Skin Care" },
          { id: 5, name: "Hair Care" },
          { id: 6, name: "Men&apos;s Grooming" },
          { id: 7, name: "Nail Art" }
        ]
      }
    ]
  },
  {
    id: 8,
    name: "Media",
    "sub-categories": [
      {
        id: 0,
        name: "Books, Comics &amp; Magazines",
        "sub-sub-categories": [
          { id: 0, name: "Non-Fiction Books" },
          { id: 1, name: "Fiction Books" },
          { id: 2, name: "Children&apos;s &amp; Young Adults&apos; Books" },
          { id: 3, name: "Comics" },
          { id: 4, name: "Magazines" },
          { id: 5, name: "Antiquarian &amp; Collectable Books" }
        ]
      },
      {
        id: 1,
        name: "Music",
        "sub-sub-categories": [
          { id: 0, name: "Music CDs" },
          { id: 1, name: "Vinyl Records" },
          { id: 2, name: "Music Memorabilia" },
          { id: 3, name: "Music Cassettes" },
          { id: 4, name: "Music Storage &amp; Media Accessories" },
          { id: 5, name: "Other Music Formats" }
        ]
      },
      {
        id: 2,
        name: "DVDs, Films &amp; TV",
        "sub-sub-categories": [
          { id: 0, name: "DVDs &amp; Blu-rays" },
          { id: 1, name: "Film Memorabilia" },
          { id: 2, name: "VHS Films" },
          { id: 3, name: "Other Film Formats" },
          { id: 4, name: "TV Memorabilia" },
          { id: 5, name: "LaserDisc Films" }
        ]
      },
      {
        id: 3,
        name: "Popular Topics",
        "sub-sub-categories": [
          { id: 0, name: "DVDs &amp; Blu-rays" },
          { id: 1, name: "Vinyl Records" },
          { id: 2, name: "Antiquarian &amp; Collectable Books" },
          { id: 3, name: "Art Books" },
          { id: 4, name: "Action and Adventure DVDs and Blu-rays" },
          { id: 5, name: "Music CDs" },
          { id: 6, name: "Comedy DVDs and Blu-rays" },
          { id: 7, name: "General &amp; Literary Fiction Books" },
          { id: 8, name: "Children&apos;s Fiction Books" },
          { id: 9, name: "Biographies &amp; True Stories Books" }
        ]
      },
      {
        id: 4,
        name: "Top Interests",
        "sub-sub-categories": [
          { id: 0, name: "Punk Music" },
          { id: 1, name: "Comedies" },
          { id: 2, name: "Jazz Music" },
          { id: 3, name: "Action Films" },
          { id: 4, name: "Classical Music" },
          { id: 5, name: "Electronic &amp; EDM Music" },
          { id: 6, name: "Documentaries" },
          { id: 7, name: "Architecture" },
          { id: 8, name: "Zombies" },
          { id: 9, name: "Art &amp; Design Books" }
        ]
      }
    ]
  }
];
