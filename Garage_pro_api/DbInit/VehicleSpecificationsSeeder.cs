// File: Garage_pro_api/DbInit/VehicleSpecificationsSeeder.cs

using BusinessObject.InspectionAndRepair;
using DataAccessLayer;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Garage_pro_api.DbInit
{
    public class VehicleSpecificationsSeeder
    {
        private readonly MyAppDbContext _context;

        public VehicleSpecificationsSeeder(MyAppDbContext context)
        {
            _context = context;
        }

        public async Task SeedAllSpecificationsDataAsync()
        {
            await SeedVehicleLookupsAsync();
            await SeedSpecificationCategoriesAsync();
            await SeedSpecificationsAsync();
            await SeedSpecificationsDataAsync();
        }

        // 1. Seed Vehicle Lookups
        private async Task SeedVehicleLookupsAsync()
        {
            if (!await _context.VehicleLookups.AnyAsync())
            {
                var vehicleLookups = new List<VehicleLookup>
                {
                    // 5 vehicle lookups
                    new VehicleLookup
                    {
                        LookupID = Guid.Parse("dcf91522-a34a-4bb5-bdee-05d51f137796"),
                        Automaker = "Honda",
                        NameCar = "Honda Civic RS"
                    },
                    new VehicleLookup
                    {
                        LookupID = Guid.Parse("f7661756-5a17-48ce-a080-c1d765e4710f"),
                        Automaker = "Toyota",
                        NameCar = "Toyota Camry 2.5Q"
                    },
                    new VehicleLookup
                    {
                        LookupID = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                        Automaker = "Toyota",
                        NameCar = "Toyota Fortuner 2.8V"
                    },
                    new VehicleLookup
                    {
                        LookupID = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                        Automaker = "Ford",
                        NameCar = "Ford Ranger Raptor"
                    },
                    new VehicleLookup
                    {
                        LookupID = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                        Automaker = "Mitsubishi",
                        NameCar = "Mitsubishi Xpander 1.5"
                    }
                };

                await _context.VehicleLookups.AddRangeAsync(vehicleLookups);
                await _context.SaveChangesAsync();
                Console.WriteLine("✓ VehicleLookups seeded successfully (5 vehicles)!");
            }
        }

        // 2. Seed Specification Categories
        private async Task SeedSpecificationCategoriesAsync()
        {
            if (!await _context.SpecificationCategory.AnyAsync())
            {
                var categories = new List<SpecificationCategory>
                {
                    new SpecificationCategory
                    {
                        CategoryID = Guid.Parse("d3c851c3-bfd6-4fd5-b2e4-11f5ca62c8e1"),
                        Title = "Exterior & Interior",
                        DisplayOrder = 7
                    },
                    new SpecificationCategory
                    {
                        CategoryID = Guid.Parse("b0112b3d-9020-4aa2-8446-153d6deda471"),
                        Title = "Engine",
                        DisplayOrder = 2
                    },
                    new SpecificationCategory
                    {
                        CategoryID = Guid.Parse("a883d77a-e8b7-430d-a618-1c69512f8a87"),
                        Title = "Drivetrain",
                        DisplayOrder = 3
                    },
                    new SpecificationCategory
                    {
                        CategoryID = Guid.Parse("b70487ab-29ba-4559-98d3-5a5d9933dea1"),
                        Title = "Chassis & Suspension",
                        DisplayOrder = 4
                    },
                    new SpecificationCategory
                    {
                        CategoryID = Guid.Parse("9fdc4f44-39af-4b05-a159-75902beff63b"),
                        Title = "Performance",
                        DisplayOrder = 5
                    },
                    new SpecificationCategory
                    {
                        CategoryID = Guid.Parse("172cbd94-4f33-4fc9-abcc-7cbda0ff27dc"),
                        Title = "Wheels & Tires",
                        DisplayOrder = 6
                    },
                    new SpecificationCategory
                    {
                        CategoryID = Guid.Parse("44c7631d-4858-4833-8d36-88e3b861b84d"),
                        Title = "Safety",
                        DisplayOrder = 8
                    },
                    new SpecificationCategory
                    {
                        CategoryID = Guid.Parse("88bd3ab9-15b3-4872-b5c3-8c77dd26948e"),
                        Title = "Dimensions & Weight",
                        DisplayOrder = 1
                    }
                };

                await _context.SpecificationCategory.AddRangeAsync(categories);
                await _context.SaveChangesAsync();
                Console.WriteLine("✓ SpecificationCategories seeded successfully!");
            }
        }

        // 3. Seed Specifications
        private async Task SeedSpecificationsAsync()
        {
            if (!await _context.Specification.AnyAsync())
            {
                var specifications = new List<Specification>
                {
                    // Safety (9 specifications)
                    new Specification { SpecificationID = Guid.Parse("ee069d30-369d-4aa8-a5bf-08b7dfb6af64"), Label = "BA", DisplayOrder = 4, CategoryID = Guid.Parse("44c7631d-4858-4833-8d36-88e3b861b84d") },
                    new Specification { SpecificationID = Guid.Parse("22817107-5580-4480-b9cb-1e6a62c2bbd0"), Label = "Cruise Control", DisplayOrder = 8, CategoryID = Guid.Parse("44c7631d-4858-4833-8d36-88e3b861b84d") },
                    new Specification { SpecificationID = Guid.Parse("72234e93-20a7-4b39-87ca-314a7b087a21"), Label = "TCS", DisplayOrder = 6, CategoryID = Guid.Parse("44c7631d-4858-4833-8d36-88e3b861b84d") },
                    new Specification { SpecificationID = Guid.Parse("00afeaa4-43fb-41af-85ea-41ed45a3004d"), Label = "ESP", DisplayOrder = 5, CategoryID = Guid.Parse("44c7631d-4858-4833-8d36-88e3b861b84d") },
                    new Specification { SpecificationID = Guid.Parse("b9b37318-6ce0-4b33-a1a5-536f74b56968"), Label = "EBD", DisplayOrder = 3, CategoryID = Guid.Parse("44c7631d-4858-4833-8d36-88e3b861b84d") },
                    new Specification { SpecificationID = Guid.Parse("63cf2134-a8d0-4cdb-8fe2-5a3aa1dd4196"), Label = "ABS", DisplayOrder = 2, CategoryID = Guid.Parse("44c7631d-4858-4833-8d36-88e3b861b84d") },
                    new Specification { SpecificationID = Guid.Parse("88f34b4f-da40-4a38-b2ba-842ba4b47974"), Label = "Airbags", DisplayOrder = 1, CategoryID = Guid.Parse("44c7631d-4858-4833-8d36-88e3b861b84d") },
                    new Specification { SpecificationID = Guid.Parse("09afc042-2e89-4beb-b272-85f7fe636df8"), Label = "Sensors & Cameras", DisplayOrder = 9, CategoryID = Guid.Parse("44c7631d-4858-4833-8d36-88e3b861b84d") },
                    new Specification { SpecificationID = Guid.Parse("5d5c8f66-118e-4e06-b7b5-cf3d7999a267"), Label = "HSA", DisplayOrder = 7, CategoryID = Guid.Parse("44c7631d-4858-4833-8d36-88e3b861b84d") },
                    
                    // Dimensions & Weight (6 specifications)
                    new Specification { SpecificationID = Guid.Parse("95a2b5c4-56ca-425a-b718-0d2b49f8165c"), Label = "Gross weight", DisplayOrder = 6, CategoryID = Guid.Parse("88bd3ab9-15b3-4872-b5c3-8c77dd26948e") },
                    new Specification { SpecificationID = Guid.Parse("63f26dc8-9afd-4a3b-b2c2-1c374d904b5e"), Label = "Ground clearance", DisplayOrder = 3, CategoryID = Guid.Parse("88bd3ab9-15b3-4872-b5c3-8c77dd26948e") },
                    new Specification { SpecificationID = Guid.Parse("59724aec-369d-42b9-b7af-75f79487b694"), Label = "Wheelbase", DisplayOrder = 2, CategoryID = Guid.Parse("88bd3ab9-15b3-4872-b5c3-8c77dd26948e") },
                    new Specification { SpecificationID = Guid.Parse("9880fc52-484d-45b2-bf48-987603fc94ac"), Label = "Minimum turning radius", DisplayOrder = 4, CategoryID = Guid.Parse("88bd3ab9-15b3-4872-b5c3-8c77dd26948e") },
                    new Specification { SpecificationID = Guid.Parse("3ed2f196-779b-48d2-9e4f-e0870c6907d1"), Label = "Curb weight", DisplayOrder = 5, CategoryID = Guid.Parse("88bd3ab9-15b3-4872-b5c3-8c77dd26948e") },
                    new Specification { SpecificationID = Guid.Parse("22f4cc10-39b6-4c23-9003-d307da062a35"), Label = "Overall size", DisplayOrder = 1, CategoryID = Guid.Parse("88bd3ab9-15b3-4872-b5c3-8c77dd26948e") },
                    
                    // Engine (6 specifications)
                    new Specification { SpecificationID = Guid.Parse("1062c9e7-f2e9-425a-bd83-130d8cb6bc48"), Label = "Displacement", DisplayOrder = 2, CategoryID = Guid.Parse("b0112b3d-9020-4aa2-8446-153d6deda471") },
                    new Specification { SpecificationID = Guid.Parse("5a1667e0-4824-4b75-b7f6-2d5b18f28484"), Label = "Cylinders", DisplayOrder = 5, CategoryID = Guid.Parse("b0112b3d-9020-4aa2-8446-153d6deda471") },
                    new Specification { SpecificationID = Guid.Parse("b7841385-72a3-43a5-9ded-3979e452f33e"), Label = "Emission standard", DisplayOrder = 6, CategoryID = Guid.Parse("b0112b3d-9020-4aa2-8446-153d6deda471") },
                    new Specification { SpecificationID = Guid.Parse("d71eed56-c8e9-49e1-b343-53ffe5048579"), Label = "Max torque", DisplayOrder = 4, CategoryID = Guid.Parse("b0112b3d-9020-4aa2-8446-153d6deda471") },
                    new Specification { SpecificationID = Guid.Parse("2a61b12a-540d-4cd7-b342-7206cdb93718"), Label = "Max power", DisplayOrder = 3, CategoryID = Guid.Parse("b0112b3d-9020-4aa2-8446-153d6deda471") },
                    new Specification { SpecificationID = Guid.Parse("e92474ca-01c5-430e-938d-f174d4f2b006"), Label = "Engine type", DisplayOrder = 1, CategoryID = Guid.Parse("b0112b3d-9020-4aa2-8446-153d6deda471") },
                    
                    // Exterior & Interior (7 specifications)
                    new Specification { SpecificationID = Guid.Parse("eeab5a03-f89a-4d50-9139-2c0618368721"), Label = "Audio system", DisplayOrder = 7, CategoryID = Guid.Parse("d3c851c3-bfd6-4fd5-b2e4-11f5ca62c8e1") },
                    new Specification { SpecificationID = Guid.Parse("e8705ad9-0ef3-42bc-a80d-4951c25cba7b"), Label = "Seats material", DisplayOrder = 4, CategoryID = Guid.Parse("d3c851c3-bfd6-4fd5-b2e4-11f5ca62c8e1") },
                    new Specification { SpecificationID = Guid.Parse("d403352c-6edd-4534-abc5-ac2a024a974d"), Label = "Infotainment", DisplayOrder = 6, CategoryID = Guid.Parse("d3c851c3-bfd6-4fd5-b2e4-11f5ca62c8e1") },
                    new Specification { SpecificationID = Guid.Parse("60320b26-3f03-4391-b107-b84d9ebcebd5"), Label = "Seats", DisplayOrder = 1, CategoryID = Guid.Parse("d3c851c3-bfd6-4fd5-b2e4-11f5ca62c8e1") },
                    new Specification { SpecificationID = Guid.Parse("b1c58de3-c14c-4de8-bfc3-babec7ba3315"), Label = "Lighting system", DisplayOrder = 3, CategoryID = Guid.Parse("d3c851c3-bfd6-4fd5-b2e4-11f5ca62c8e1") },
                    new Specification { SpecificationID = Guid.Parse("993b032e-c199-4bff-9d6b-d99724ee4b9b"), Label = "Body style", DisplayOrder = 2, CategoryID = Guid.Parse("d3c851c3-bfd6-4fd5-b2e4-11f5ca62c8e1") },
                    new Specification { SpecificationID = Guid.Parse("23014666-df2f-4816-9ea2-f12063ab12c3"), Label = "Air conditioning", DisplayOrder = 5, CategoryID = Guid.Parse("d3c851c3-bfd6-4fd5-b2e4-11f5ca62c8e1") },
                    
                    // Chassis & Suspension (5 specifications)
                    new Specification { SpecificationID = Guid.Parse("959b1378-b75b-4ee1-a5d2-4eb633ed50e5"), Label = "Front brakes", DisplayOrder = 3, CategoryID = Guid.Parse("b70487ab-29ba-4559-98d3-5a5d9933dea1") },
                    new Specification { SpecificationID = Guid.Parse("a10d70a8-e92a-49be-bd7d-5aa0c2ac245e"), Label = "Rear suspension", DisplayOrder = 2, CategoryID = Guid.Parse("b70487ab-29ba-4559-98d3-5a5d9933dea1") },
                    new Specification { SpecificationID = Guid.Parse("eeaef53b-56d2-4c1a-9f06-6db358ac0768"), Label = "Steering system", DisplayOrder = 5, CategoryID = Guid.Parse("b70487ab-29ba-4559-98d3-5a5d9933dea1") },
                    new Specification { SpecificationID = Guid.Parse("664d94d4-3a7c-47e9-9ad1-7e3064357b23"), Label = "Rear brakes", DisplayOrder = 4, CategoryID = Guid.Parse("b70487ab-29ba-4559-98d3-5a5d9933dea1") },
                    new Specification { SpecificationID = Guid.Parse("9dac9885-4edc-4b8d-ad26-ed8aa39760a2"), Label = "Front suspension", DisplayOrder = 1, CategoryID = Guid.Parse("b70487ab-29ba-4559-98d3-5a5d9933dea1") },
                    
                    // Wheels & Tires (3 specifications)
                    new Specification { SpecificationID = Guid.Parse("52e11e7f-7527-43de-a7e1-5a25d5650bb1"), Label = "Wheel material", DisplayOrder = 3, CategoryID = Guid.Parse("172cbd94-4f33-4fc9-abcc-7cbda0ff27dc") },
                    new Specification { SpecificationID = Guid.Parse("9584d60e-2d8d-4d60-b37b-6b44a88ee253"), Label = "Tire size", DisplayOrder = 1, CategoryID = Guid.Parse("172cbd94-4f33-4fc9-abcc-7cbda0ff27dc") },
                    new Specification { SpecificationID = Guid.Parse("39a4e1c4-97fa-44c7-a883-9ea1910c0752"), Label = "Wheel size", DisplayOrder = 2, CategoryID = Guid.Parse("172cbd94-4f33-4fc9-abcc-7cbda0ff27dc") },
                    
                    // Performance (4 specifications)
                    new Specification { SpecificationID = Guid.Parse("c06286dc-8907-4d0c-8111-4833290219d3"), Label = "Top speed", DisplayOrder = 1, CategoryID = Guid.Parse("9fdc4f44-39af-4b05-a159-75902beff63b") },
                    new Specification { SpecificationID = Guid.Parse("a07d762f-3246-4c8c-8a7d-add5d3cee2c5"), Label = "Fuel consumption", DisplayOrder = 3, CategoryID = Guid.Parse("9fdc4f44-39af-4b05-a159-75902beff63b") },
                    new Specification { SpecificationID = Guid.Parse("9f1e0f75-af7c-403b-9e40-b4ff0d695f77"), Label = "Fuel tank capacity", DisplayOrder = 4, CategoryID = Guid.Parse("9fdc4f44-39af-4b05-a159-75902beff63b") },
                    new Specification { SpecificationID = Guid.Parse("0267940c-c6f6-4235-b6cc-d62e8ff334f8"), Label = "0-100 km/h acceleration", DisplayOrder = 2, CategoryID = Guid.Parse("9fdc4f44-39af-4b05-a159-75902beff63b") },
                    
                    // Drivetrain (3 specifications)
                    new Specification { SpecificationID = Guid.Parse("4b6fd7f6-c921-4d5e-b61d-88cd8d43364f"), Label = "Drive type", DisplayOrder = 1, CategoryID = Guid.Parse("a883d77a-e8b7-430d-a618-1c69512f8a87") },
                    new Specification { SpecificationID = Guid.Parse("ab72e6de-2301-4fe8-b9ca-c071f9aea9ec"), Label = "Transmission", DisplayOrder = 2, CategoryID = Guid.Parse("a883d77a-e8b7-430d-a618-1c69512f8a87") },
                    new Specification { SpecificationID = Guid.Parse("5bd55c9a-c9d5-4e89-b120-fe8800d25761"), Label = "Gear levels", DisplayOrder = 3, CategoryID = Guid.Parse("a883d77a-e8b7-430d-a618-1c69512f8a87") }
                };

                await _context.Specification.AddRangeAsync(specifications);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✓ Specifications seeded successfully ({specifications.Count} specifications)!");
            }
        }

        // 4. Seed Specifications Data (COMPLETE DATA FOR ALL 5 VEHICLES)
        private async Task SeedSpecificationsDataAsync()
        {
            if (!await _context.SpecificationsData.AnyAsync())
            {
                var hondaCivicId = Guid.Parse("dcf91522-a34a-4bb5-bdee-05d51f137796");
                var toyotaCamryId = Guid.Parse("f7661756-5a17-48ce-a080-c1d765e4710f");
                var toyotaFortunerId = Guid.Parse("11111111-1111-1111-1111-111111111111");
                var fordRaptorId = Guid.Parse("22222222-2222-2222-2222-222222222222");
                var mitsubishiXpanderId = Guid.Parse("33333333-3333-3333-3333-333333333333");

                var specificationsData = new List<SpecificationsData>();

                // Honda Civic RS Data
                specificationsData.AddRange(new[]
                {
                    new SpecificationsData { DataID = Guid.Parse("2312153b-5b6b-4bbb-ab6b-04cff8e383d4"), Value = "215/50R17", LookupID = hondaCivicId, SpecificationID = Guid.Parse("9584d60e-2d8d-4d60-b37b-6b44a88ee253") },
                    new SpecificationsData { DataID = Guid.Parse("760933c1-595c-45de-b7eb-08cdf4ceb04b"), Value = "CVT (Continuously Variable Transmission)", LookupID = hondaCivicId, SpecificationID = Guid.Parse("ab72e6de-2301-4fe8-b9ca-c071f9aea9ec") },
                    new SpecificationsData { DataID = Guid.Parse("e0ccc8b9-db85-411a-9dd5-0e077c69a93e"), Value = "FWD (Front-Wheel Drive)", LookupID = hondaCivicId, SpecificationID = Guid.Parse("4b6fd7f6-c921-4d5e-b61d-88cd8d43364f") },
                    new SpecificationsData { DataID = Guid.Parse("f599b4e5-39fa-4b0d-80d0-1374b437099d"), Value = "134 mm", LookupID = hondaCivicId, SpecificationID = Guid.Parse("63f26dc8-9afd-4a3b-b2c2-1c374d904b5e") },
                    new SpecificationsData { DataID = Guid.Parse("5708c36e-a08d-46aa-970c-1763805dc3aa"), Value = "240 Nm @ 1,700-4,500 rpm", LookupID = hondaCivicId, SpecificationID = Guid.Parse("d71eed56-c8e9-49e1-b343-53ffe5048579") },
                    new SpecificationsData { DataID = Guid.Parse("674778da-f25c-43ee-b364-18fff71a00e2"), Value = "6 airbags", LookupID = hondaCivicId, SpecificationID = Guid.Parse("88f34b4f-da40-4a38-b2ba-842ba4b47974") },
                    new SpecificationsData { DataID = Guid.Parse("712ce0dc-e1ff-4d30-963f-1c77e4e72052"), Value = "Automatic climate control", LookupID = hondaCivicId, SpecificationID = Guid.Parse("23014666-df2f-4816-9ea2-f12063ab12c3") },
                    new SpecificationsData { DataID = Guid.Parse("993bf948-28ff-4541-87ef-1c861f566543"), Value = "Yes", LookupID = hondaCivicId, SpecificationID = Guid.Parse("ee069d30-369d-4aa8-a5bf-08b7dfb6af64") },
                    new SpecificationsData { DataID = Guid.Parse("cde101d9-38a7-4f67-912b-1db8dee39dc8"), Value = "Ventilated disc", LookupID = hondaCivicId, SpecificationID = Guid.Parse("959b1378-b75b-4ee1-a5d2-4eb633ed50e5") },
                    new SpecificationsData { DataID = Guid.Parse("809769ee-d864-4248-a203-232fd55c027a"), Value = "Full LED headlights with DRL", LookupID = hondaCivicId, SpecificationID = Guid.Parse("b1c58de3-c14c-4de8-bfc3-babec7ba3315") },
                    new SpecificationsData { DataID = Guid.Parse("b999b9e4-4afc-4433-955c-255df8856ee4"), Value = "MacPherson Strut", LookupID = hondaCivicId, SpecificationID = Guid.Parse("9dac9885-4edc-4b8d-ad26-ed8aa39760a2") },
                    new SpecificationsData { DataID = Guid.Parse("3cfec564-e703-4b8a-a723-27378489d6c2"), Value = "178 hp @ 6,000 rpm", LookupID = hondaCivicId, SpecificationID = Guid.Parse("2a61b12a-540d-4cd7-b342-7206cdb93718") },
                    new SpecificationsData { DataID = Guid.Parse("107711d2-39d5-4f26-9d9c-2c1e2b3bc45b"), Value = "5 seats", LookupID = hondaCivicId, SpecificationID = Guid.Parse("60320b26-3f03-4391-b107-b84d9ebcebd5") },
                    new SpecificationsData { DataID = Guid.Parse("0f5bbd20-79bf-446e-9420-2f999b8c7a38"), Value = "1,498 cc", LookupID = hondaCivicId, SpecificationID = Guid.Parse("1062c9e7-f2e9-425a-bd83-130d8cb6bc48") },
                    new SpecificationsData { DataID = Guid.Parse("e4a953fa-c566-4215-8cc3-3366bafcc75f"), Value = "Euro 5", LookupID = hondaCivicId, SpecificationID = Guid.Parse("b7841385-72a3-43a5-9ded-3979e452f33e") },
                    new SpecificationsData { DataID = Guid.Parse("a567437d-86cd-4d68-8132-47e37aecd720"), Value = "Yes", LookupID = hondaCivicId, SpecificationID = Guid.Parse("b9b37318-6ce0-4b33-a1a5-536f74b56968") },
                    new SpecificationsData { DataID = Guid.Parse("48ff562f-0ffd-406a-965f-59e102949ad3"), Value = "8 speakers", LookupID = hondaCivicId, SpecificationID = Guid.Parse("eeab5a03-f89a-4d50-9139-2c0618368721") },
                    new SpecificationsData { DataID = Guid.Parse("b49fb539-f0d5-4b08-9621-60c8e23d08c1"), Value = "1,329 kg", LookupID = hondaCivicId, SpecificationID = Guid.Parse("3ed2f196-779b-48d2-9e4f-e0870c6907d1") },
                    new SpecificationsData { DataID = Guid.Parse("393f4e17-9c94-4b8b-8ff5-660a0842c2b0"), Value = "9-inch touchscreen with Apple CarPlay/Android Auto", LookupID = hondaCivicId, SpecificationID = Guid.Parse("d403352c-6edd-4534-abc5-ac2a024a974d") },
                    new SpecificationsData { DataID = Guid.Parse("5a5fb9e4-ef04-444e-9c3e-867aed0af1fe"), Value = "Leather", LookupID = hondaCivicId, SpecificationID = Guid.Parse("e8705ad9-0ef3-42bc-a80d-4951c25cba7b") },
                    new SpecificationsData { DataID = Guid.Parse("8f22080f-6fb9-4641-b31e-8b89f9a88296"), Value = "Electric Power Steering (EPS)", LookupID = hondaCivicId, SpecificationID = Guid.Parse("eeaef53b-56d2-4c1a-9f06-6db358ac0768") },
                    new SpecificationsData { DataID = Guid.Parse("db1f87bc-6333-441d-a8a6-8c7d52761601"), Value = "4678 x 1802 x 1415 mm", LookupID = hondaCivicId, SpecificationID = Guid.Parse("22f4cc10-39b6-4c23-9003-d307da062a35") },
                    new SpecificationsData { DataID = Guid.Parse("99b20a4f-2a5c-4edc-b60d-5458c8407d4b"), Value = "Yes (VSA)", LookupID = hondaCivicId, SpecificationID = Guid.Parse("00afeaa4-43fb-41af-85ea-41ed45a3004d") },
                    new SpecificationsData { DataID = Guid.Parse("8423e846-452a-4a13-9184-a65f52fcdb76"), Value = "Yes", LookupID = hondaCivicId, SpecificationID = Guid.Parse("72234e93-20a7-4b39-87ca-314a7b087a21") },
                    new SpecificationsData { DataID = Guid.Parse("89504531-c9c6-482d-8a9e-c262613a1cc6"), Value = "Adaptive Cruise Control (ACC)", LookupID = hondaCivicId, SpecificationID = Guid.Parse("22817107-5580-4480-b9cb-1e6a62c2bbd0") },
                    new SpecificationsData { DataID = Guid.Parse("8a03522b-d36f-4955-9fac-c6365c8651b1"), Value = "8.2 seconds", LookupID = hondaCivicId, SpecificationID = Guid.Parse("0267940c-c6f6-4235-b6cc-d62e8ff334f8") },
                    new SpecificationsData { DataID = Guid.Parse("489115aa-178a-4fd6-bd84-c896b6546ff5"), Value = "Solid disc", LookupID = hondaCivicId, SpecificationID = Guid.Parse("664d94d4-3a7c-47e9-9ad1-7e3064357b23") },
                    new SpecificationsData { DataID = Guid.Parse("be02f0b8-bd8c-4d89-aaad-cb6aaa2cfd9f"), Value = "Yes", LookupID = hondaCivicId, SpecificationID = Guid.Parse("63cf2134-a8d0-4cdb-8fe2-5a3aa1dd4196") },
                    new SpecificationsData { DataID = Guid.Parse("af528ea9-6b6c-4869-a139-d0449621ada5"), Value = "Stepless", LookupID = hondaCivicId, SpecificationID = Guid.Parse("5bd55c9a-c9d5-4e89-b120-fe8800d25761") },
                    new SpecificationsData { DataID = Guid.Parse("eff5850f-50d7-4ea7-af66-db72b81d80d7"), Value = "4 cylinders inline", LookupID = hondaCivicId, SpecificationID = Guid.Parse("5a1667e0-4824-4b75-b7f6-2d5b18f28484") },
                    new SpecificationsData { DataID = Guid.Parse("49779590-9b9b-4485-9411-db95257345b7"), Value = "200 km/h", LookupID = hondaCivicId, SpecificationID = Guid.Parse("c06286dc-8907-4d0c-8111-4833290219d3") },
                    new SpecificationsData { DataID = Guid.Parse("2ad02e8b-4327-48b0-86a6-e77f87478ca4"), Value = "1.5L VTEC Turbo", LookupID = hondaCivicId, SpecificationID = Guid.Parse("e92474ca-01c5-430e-938d-f174d4f2b006") },
                    new SpecificationsData { DataID = Guid.Parse("aac0482e-4b6f-46c2-9dda-ef993873f345"), Value = "5.5 m", LookupID = hondaCivicId, SpecificationID = Guid.Parse("9880fc52-484d-45b2-bf48-987603fc94ac") },
                    new SpecificationsData { DataID = Guid.Parse("6e1d8d39-a83c-4303-8dda-f101b5af22f2"), Value = "Yes", LookupID = hondaCivicId, SpecificationID = Guid.Parse("5d5c8f66-118e-4e06-b7b5-cf3d7999a267") },
                    new SpecificationsData { DataID = Guid.Parse("25a3e1b1-0894-4ed4-888a-f5c8351415b2"), Value = "6.0 L/100km (Combined)", LookupID = hondaCivicId, SpecificationID = Guid.Parse("a07d762f-3246-4c8c-8a7d-add5d3cee2c5") },
                    new SpecificationsData { DataID = Guid.Parse("89bfe49e-178b-4c5b-a867-b4502c697212"), Value = "Sedan", LookupID = hondaCivicId, SpecificationID = Guid.Parse("993b032e-c199-4bff-9d6b-d99724ee4b9b") },
                    new SpecificationsData { DataID = Guid.Parse("eb648c31-7f5e-4296-bae3-a7e24038fc42"), Value = "2735 mm", LookupID = hondaCivicId, SpecificationID = Guid.Parse("59724aec-369d-42b9-b7af-75f79487b694") },
                    new SpecificationsData { DataID = Guid.Parse("dac693f7-b6d1-4bc2-b953-b0405fe3df87"), Value = "Multi-link", LookupID = hondaCivicId, SpecificationID = Guid.Parse("a10d70a8-e92a-49be-bd7d-5aa0c2ac245e") },
                    new SpecificationsData { DataID = Guid.Parse("1970b9f1-86ba-4e25-8886-fe8be47fd1fb"), Value = "4 sensors + Rear camera", LookupID = hondaCivicId, SpecificationID = Guid.Parse("09afc042-2e89-4beb-b272-85f7fe636df8") },
                    new SpecificationsData { DataID = Guid.Parse("c4b2654a-cb68-41a6-9e47-7bbf0e5e4f05"), Value = "Alloy wheels", LookupID = hondaCivicId, SpecificationID = Guid.Parse("52e11e7f-7527-43de-a7e1-5a25d5650bb1") },
                    new SpecificationsData { DataID = Guid.Parse("f0f5f17f-1942-44fa-b273-c1fc9e2d846e"), Value = "1,879 kg", LookupID = hondaCivicId, SpecificationID = Guid.Parse("95a2b5c4-56ca-425a-b718-0d2b49f8165c") },
                    new SpecificationsData { DataID = Guid.Parse("413574ba-a1a2-4de9-8d87-bd2c2e64a16d"), Value = "17 inches", LookupID = hondaCivicId, SpecificationID = Guid.Parse("39a4e1c4-97fa-44c7-a883-9ea1910c0752") },
                    new SpecificationsData { DataID = Guid.Parse("70cac260-3e3b-4a5c-8b2c-e04beb6e555e"), Value = "47 liters", LookupID = hondaCivicId, SpecificationID = Guid.Parse("9f1e0f75-af7c-403b-9e40-b4ff0d695f77") },
                });

                // Toyota Camry 2.5Q Data
                specificationsData.AddRange(new[]
                {
                    new SpecificationsData { DataID = Guid.Parse("d6234a33-de1b-411e-99dc-0afc4cb046c1"), Value = "2,487 cc", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("1062c9e7-f2e9-425a-bd83-130d8cb6bc48") },
                    new SpecificationsData { DataID = Guid.Parse("eb772da6-7f39-4061-ad06-0db05b1ee0c0"), Value = "8 gears", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("5bd55c9a-c9d5-4e89-b120-fe8800d25761") },
                    new SpecificationsData { DataID = Guid.Parse("dc63fef7-98ff-494c-a015-31a45076387a"), Value = "Yes", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("ee069d30-369d-4aa8-a5bf-08b7dfb6af64") },
                    new SpecificationsData { DataID = Guid.Parse("eebd0c9b-ace8-4cd4-8de0-34347dbf159b"), Value = "Ventilated disc", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("959b1378-b75b-4ee1-a5d2-4eb633ed50e5") },
                    new SpecificationsData { DataID = Guid.Parse("2ce447c7-60a2-4437-badc-36c576735aea"), Value = "Alloy wheels", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("52e11e7f-7527-43de-a7e1-5a25d5650bb1") },
                    new SpecificationsData { DataID = Guid.Parse("7542559d-3974-4506-a07c-375a222afd89"), Value = "Euro 5", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("b7841385-72a3-43a5-9ded-3979e452f33e") },
                    new SpecificationsData { DataID = Guid.Parse("a5c3cd8b-d18b-46ae-b9f2-416c35f714cf"), Value = "9 airbags", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("88f34b4f-da40-4a38-b2ba-842ba4b47974") },
                    new SpecificationsData { DataID = Guid.Parse("d4eef140-b282-4f90-b757-4a87aa6700f9"), Value = "2.5L Dynamic Force", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("e92474ca-01c5-430e-938d-f174d4f2b006") },
                    new SpecificationsData { DataID = Guid.Parse("37c3f0de-9d37-4dc4-b197-50be8c509cc5"), Value = "1,535 kg", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("3ed2f196-779b-48d2-9e4f-e0870c6907d1") },
                    new SpecificationsData { DataID = Guid.Parse("f7dfee84-56db-44e5-a09c-51425e33f470"), Value = "Yes", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("5d5c8f66-118e-4e06-b7b5-cf3d7999a267") },
                    new SpecificationsData { DataID = Guid.Parse("806b113f-ce7b-4c8b-b947-2ba3c7868d2a"), Value = "Electric Power Steering (EPS)", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("eeaef53b-56d2-4c1a-9f06-6db358ac0768") },
                    new SpecificationsData { DataID = Guid.Parse("cc3783d6-7f75-4ddc-98fe-55467621cae0"), Value = "Double wishbone", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("a10d70a8-e92a-49be-bd7d-5aa0c2ac245e") },
                    new SpecificationsData { DataID = Guid.Parse("f0a65216-b021-4c1a-bd76-614a09195bb7"), Value = "215/55R17", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("9584d60e-2d8d-4d60-b37b-6b44a88ee253") },
                    new SpecificationsData { DataID = Guid.Parse("8f356528-1890-49ea-bae9-660c166ab225"), Value = "Solid disc", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("664d94d4-3a7c-47e9-9ad1-7e3064357b23") },
                    new SpecificationsData { DataID = Guid.Parse("88be2173-a8cf-4e23-a409-72b60237e75a"), Value = "MacPherson Strut", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("9dac9885-4edc-4b8d-ad26-ed8aa39760a2") },
                    new SpecificationsData { DataID = Guid.Parse("5e8d9c93-1167-4d81-aaf6-734cfe4fb827"), Value = "Yes", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("63cf2134-a8d0-4cdb-8fe2-5a3aa1dd4196") },
                    new SpecificationsData { DataID = Guid.Parse("779d8110-71eb-4fb2-80df-78af3af08ae3"), Value = "Bi-LED headlights with DRL", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("b1c58de3-c14c-4de8-bfc3-babec7ba3315") },
                    new SpecificationsData { DataID = Guid.Parse("bc8ba759-6bd4-41df-9f0f-8515e543f3ee"), Value = "2825 mm", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("59724aec-369d-42b9-b7af-75f79487b694") },
                    new SpecificationsData { DataID = Guid.Parse("918dd8c5-f77b-4261-8112-8a22fff0b776"), Value = "8 sensors + 360° camera", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("09afc042-2e89-4beb-b272-85f7fe636df8") },
                    new SpecificationsData { DataID = Guid.Parse("c0f3a3cc-35f0-40ab-b181-906ab2fdd910"), Value = "Adaptive Cruise Control (ACC)", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("22817107-5580-4480-b9cb-1e6a62c2bbd0") },
                    new SpecificationsData { DataID = Guid.Parse("5aa80aee-7702-49ea-8384-911013a163ba"), Value = "210 km/h", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("c06286dc-8907-4d0c-8111-4833290219d3") },
                    new SpecificationsData { DataID = Guid.Parse("a06a1f3f-e9f0-4e62-9b59-93d8e794eb7f"), Value = "140 mm", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("63f26dc8-9afd-4a3b-b2c2-1c374d904b5e") },
                    new SpecificationsData { DataID = Guid.Parse("a15e7de6-60a2-41ff-9d09-95b1e0f70a14"), Value = "4885 x 1840 x 1445 mm", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("22f4cc10-39b6-4c23-9003-d307da062a35") },
                    new SpecificationsData { DataID = Guid.Parse("f0cde638-d90d-4fd6-97bb-a385e0b6216c"), Value = "8-speed Automatic", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("ab72e6de-2301-4fe8-b9ca-c071f9aea9ec") },
                    new SpecificationsData { DataID = Guid.Parse("88c0c9a6-7701-49fa-b57c-a44619e414a2"), Value = "Leather", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("e8705ad9-0ef3-42bc-a80d-4951c25cba7b") },
                    new SpecificationsData { DataID = Guid.Parse("46fb7318-b3fc-4ff6-96de-a6db5acbff57"), Value = "FWD (Front-Wheel Drive)", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("4b6fd7f6-c921-4d5e-b61d-88cd8d43364f") },
                    new SpecificationsData { DataID = Guid.Parse("bbf33d4d-9546-46d4-911d-a7a77f7bc8c2"), Value = "231 Nm @ 3,600-5,200 rpm", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("d71eed56-c8e9-49e1-b343-53ffe5048579") },
                    new SpecificationsData { DataID = Guid.Parse("f8a808af-fa9d-4f2a-b53e-ae85e37c75f6"), Value = "2,045 kg", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("95a2b5c4-56ca-425a-b718-0d2b49f8165c") },
                    new SpecificationsData { DataID = Guid.Parse("eaf77c82-9089-4415-8ac5-b18a6b6254e4"), Value = "Yes (VSC)", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("00afeaa4-43fb-41af-85ea-41ed45a3004d") },
                    new SpecificationsData { DataID = Guid.Parse("cc2e2503-904b-4405-8bb3-b6ebfe22c27b"), Value = "Yes", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("72234e93-20a7-4b39-87ca-314a7b087a21") },
                    new SpecificationsData { DataID = Guid.Parse("bd680460-8ca2-4c5c-a606-b8e1e45b79fe"), Value = "60 liters", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("9f1e0f75-af7c-403b-9e40-b4ff0d695f77") },
                    new SpecificationsData { DataID = Guid.Parse("7d42c7f2-fcc7-4736-a0e3-bcc4eaea42b4"), Value = "4 cylinders inline", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("5a1667e0-4824-4b75-b7f6-2d5b18f28484") },
                    new SpecificationsData { DataID = Guid.Parse("49805976-b5af-482e-aeb4-c98bc202e678"), Value = "6.7 L/100km (Combined)", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("a07d762f-3246-4c8c-8a7d-add5d3cee2c5") },
                    new SpecificationsData { DataID = Guid.Parse("328bf316-f651-4ac5-bdb5-ce8036d121ad"), Value = "181 hp @ 6,000 rpm", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("2a61b12a-540d-4cd7-b342-7206cdb93718") },
                    new SpecificationsData { DataID = Guid.Parse("a8d9136b-f421-4675-a469-d09c474bbc70"), Value = "5.7 m", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("9880fc52-484d-45b2-bf48-987603fc94ac") },
                    new SpecificationsData { DataID = Guid.Parse("e61a7b64-8631-4338-944c-e04025bf545c"), Value = "9.1 seconds", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("0267940c-c6f6-4235-b6cc-d62e8ff334f8") },
                    new SpecificationsData { DataID = Guid.Parse("2dc4c1c6-eda5-4e1c-b56e-e26a556d75e1"), Value = "17 inches", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("39a4e1c4-97fa-44c7-a883-9ea1910c0752") },
                    new SpecificationsData { DataID = Guid.Parse("e7eaf8a6-b576-4d06-9aeb-f2131508c4b3"), Value = "Sedan", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("993b032e-c199-4bff-9d6b-d99724ee4b9b") },
                    new SpecificationsData { DataID = Guid.Parse("4a63fe48-3d2d-48b3-a89e-f289633a8d3e"), Value = "9 speakers JBL", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("eeab5a03-f89a-4d50-9139-2c0618368721") },
                    new SpecificationsData { DataID = Guid.Parse("2dbaaec9-7191-44a9-a7c7-fa840301ca6b"), Value = "5 seats", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("60320b26-3f03-4391-b107-b84d9ebcebd5") },
                    new SpecificationsData { DataID = Guid.Parse("0e3dd75b-2af3-42df-8a6d-faecd3e5672f"), Value = "Dual-zone automatic climate control", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("23014666-df2f-4816-9ea2-f12063ab12c3") },
                    new SpecificationsData { DataID = Guid.Parse("bbe5a2e1-96f9-46d6-8b16-eee7615c6803"), Value = "Yes", LookupID = toyotaCamryId, SpecificationID = Guid.Parse("b9b37318-6ce0-4b33-a1a5-536f74b56968") },
                });

                // Toyota Fortuner 2.8V Data
                specificationsData.AddRange(new[]
                {
                    new SpecificationsData { DataID = Guid.Parse("aaaa0001-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "2,755 cc", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("1062c9e7-f2e9-425a-bd83-130d8cb6bc48") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0002-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "2.8L Diesel Turbo", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("e92474ca-01c5-430e-938d-f174d4f2b006") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0003-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "204 hp @ 3,400 rpm", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("2a61b12a-540d-4cd7-b342-7206cdb93718") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0004-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "500 Nm @ 1,600-2,800 rpm", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("d71eed56-c8e9-49e1-b343-53ffe5048579") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0005-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "4 cylinders inline", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("5a1667e0-4824-4b75-b7f6-2d5b18f28484") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0006-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "Euro 5", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("b7841385-72a3-43a5-9ded-3979e452f33e") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0007-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "6-speed Automatic", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("ab72e6de-2301-4fe8-b9ca-c071f9aea9ec") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0008-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "6 gears", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("5bd55c9a-c9d5-4e89-b120-fe8800d25761") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0009-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "4WD (Four-Wheel Drive)", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("4b6fd7f6-c921-4d5e-b61d-88cd8d43364f") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0010-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "4795 x 1855 x 1835 mm", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("22f4cc10-39b6-4c23-9003-d307da062a35") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0011-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "2745 mm", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("59724aec-369d-42b9-b7af-75f79487b694") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0012-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "225 mm", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("63f26dc8-9afd-4a3b-b2c2-1c374d904b5e") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0013-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "5.8 m", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("9880fc52-484d-45b2-bf48-987603fc94ac") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0014-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "2,180 kg", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("3ed2f196-779b-48d2-9e4f-e0870c6907d1") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0015-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "2,800 kg", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("95a2b5c4-56ca-425a-b718-0d2b49f8165c") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0016-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "SUV", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("993b032e-c199-4bff-9d6b-d99724ee4b9b") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0017-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "7 seats", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("60320b26-3f03-4391-b107-b84d9ebcebd5") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0018-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "265/60R18", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("9584d60e-2d8d-4d60-b37b-6b44a88ee253") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0019-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "18 inches", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("39a4e1c4-97fa-44c7-a883-9ea1910c0752") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0020-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "Alloy wheels", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("52e11e7f-7527-43de-a7e1-5a25d5650bb1") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0021-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "10.5 seconds", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("0267940c-c6f6-4235-b6cc-d62e8ff334f8") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0022-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "180 km/h", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("c06286dc-8907-4d0c-8111-4833290219d3") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0023-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "8.5 L/100km", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("a07d762f-3246-4c8c-8a7d-add5d3cee2c5") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0024-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "80 liters", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("9f1e0f75-af7c-403b-9e40-b4ff0d695f77") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0025-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "7 airbags", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("88f34b4f-da40-4a38-b2ba-842ba4b47974") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0026-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "Yes", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("63cf2134-a8d0-4cdb-8fe2-5a3aa1dd4196") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0027-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "Yes", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("b9b37318-6ce0-4b33-a1a5-536f74b56968") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0028-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "Yes", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("ee069d30-369d-4aa8-a5bf-08b7dfb6af64") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0029-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "Yes (VSC)", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("00afeaa4-43fb-41af-85ea-41ed45a3004d") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0030-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "Yes", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("72234e93-20a7-4b39-87ca-314a7b087a21") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0031-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "Yes", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("5d5c8f66-118e-4e06-b7b5-cf3d7999a267") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0032-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "Adaptive Cruise Control", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("22817107-5580-4480-b9cb-1e6a62c2bbd0") },
                    new SpecificationsData { DataID = Guid.Parse("aaaa0033-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), Value = "6 sensors + Rear camera", LookupID = toyotaFortunerId, SpecificationID = Guid.Parse("09afc042-2e89-4beb-b272-85f7fe636df8") },
                });

                // Ford Ranger Raptor Data
                specificationsData.AddRange(new[]
                {
                    new SpecificationsData { DataID = Guid.Parse("bbbb0001-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "3,000 cc", LookupID = fordRaptorId, SpecificationID = Guid.Parse("1062c9e7-f2e9-425a-bd83-130d8cb6bc48") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0002-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "3.0L EcoBoost V6", LookupID = fordRaptorId, SpecificationID = Guid.Parse("e92474ca-01c5-430e-938d-f174d4f2b006") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0003-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "392 hp @ 5,850 rpm", LookupID = fordRaptorId, SpecificationID = Guid.Parse("2a61b12a-540d-4cd7-b342-7206cdb93718") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0004-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "583 Nm @ 3,500 rpm", LookupID = fordRaptorId, SpecificationID = Guid.Parse("d71eed56-c8e9-49e1-b343-53ffe5048579") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0005-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "6 cylinders V6", LookupID = fordRaptorId, SpecificationID = Guid.Parse("5a1667e0-4824-4b75-b7f6-2d5b18f28484") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0006-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "10-speed Automatic", LookupID = fordRaptorId, SpecificationID = Guid.Parse("ab72e6de-2301-4fe8-b9ca-c071f9aea9ec") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0007-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "10 gears", LookupID = fordRaptorId, SpecificationID = Guid.Parse("5bd55c9a-c9d5-4e89-b120-fe8800d25761") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0008-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "4WD (Four-Wheel Drive)", LookupID = fordRaptorId, SpecificationID = Guid.Parse("4b6fd7f6-c921-4d5e-b61d-88cd8d43364f") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0009-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "5398 x 2028 x 1919 mm", LookupID = fordRaptorId, SpecificationID = Guid.Parse("22f4cc10-39b6-4c23-9003-d307da062a35") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0010-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "3270 mm", LookupID = fordRaptorId, SpecificationID = Guid.Parse("59724aec-369d-42b9-b7af-75f79487b694") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0011-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "283 mm", LookupID = fordRaptorId, SpecificationID = Guid.Parse("63f26dc8-9afd-4a3b-b2c2-1c374d904b5e") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0012-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "6.9 m", LookupID = fordRaptorId, SpecificationID = Guid.Parse("9880fc52-484d-45b2-bf48-987603fc94ac") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0013-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "2,500 kg", LookupID = fordRaptorId, SpecificationID = Guid.Parse("3ed2f196-779b-48d2-9e4f-e0870c6907d1") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0014-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "3,500 kg", LookupID = fordRaptorId, SpecificationID = Guid.Parse("95a2b5c4-56ca-425a-b718-0d2b49f8165c") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0015-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "Pickup Truck", LookupID = fordRaptorId, SpecificationID = Guid.Parse("993b032e-c199-4bff-9d6b-d99724ee4b9b") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0016-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "5 seats", LookupID = fordRaptorId, SpecificationID = Guid.Parse("60320b26-3f03-4391-b107-b84d9ebcebd5") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0017-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "315/70R17", LookupID = fordRaptorId, SpecificationID = Guid.Parse("9584d60e-2d8d-4d60-b37b-6b44a88ee253") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0018-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "17 inches", LookupID = fordRaptorId, SpecificationID = Guid.Parse("39a4e1c4-97fa-44c7-a883-9ea1910c0752") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0019-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "Alloy wheels", LookupID = fordRaptorId, SpecificationID = Guid.Parse("52e11e7f-7527-43de-a7e1-5a25d5650bb1") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0020-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "5.5 seconds", LookupID = fordRaptorId, SpecificationID = Guid.Parse("0267940c-c6f6-4235-b6cc-d62e8ff334f8") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0021-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "190 km/h", LookupID = fordRaptorId, SpecificationID = Guid.Parse("c06286dc-8907-4d0c-8111-4833290219d3") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0022-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "11.5 L/100km", LookupID = fordRaptorId, SpecificationID = Guid.Parse("a07d762f-3246-4c8c-8a7d-add5d3cee2c5") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0023-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "80 liters", LookupID = fordRaptorId, SpecificationID = Guid.Parse("9f1e0f75-af7c-403b-9e40-b4ff0d695f77") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0024-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "6 airbags", LookupID = fordRaptorId, SpecificationID = Guid.Parse("88f34b4f-da40-4a38-b2ba-842ba4b47974") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0025-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "Yes", LookupID = fordRaptorId, SpecificationID = Guid.Parse("63cf2134-a8d0-4cdb-8fe2-5a3aa1dd4196") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0026-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "Yes", LookupID = fordRaptorId, SpecificationID = Guid.Parse("b9b37318-6ce0-4b33-a1a5-536f74b56968") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0027-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "Yes", LookupID = fordRaptorId, SpecificationID = Guid.Parse("ee069d30-369d-4aa8-a5bf-08b7dfb6af64") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0028-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "Yes", LookupID = fordRaptorId, SpecificationID = Guid.Parse("00afeaa4-43fb-41af-85ea-41ed45a3004d") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0029-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "Yes", LookupID = fordRaptorId, SpecificationID = Guid.Parse("72234e93-20a7-4b39-87ca-314a7b087a21") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0030-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "Yes", LookupID = fordRaptorId, SpecificationID = Guid.Parse("5d5c8f66-118e-4e06-b7b5-cf3d7999a267") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0031-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "Adaptive Cruise Control", LookupID = fordRaptorId, SpecificationID = Guid.Parse("22817107-5580-4480-b9cb-1e6a62c2bbd0") },
                    new SpecificationsData { DataID = Guid.Parse("bbbb0032-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), Value = "8 sensors + 360° camera", LookupID = fordRaptorId, SpecificationID = Guid.Parse("09afc042-2e89-4beb-b272-85f7fe636df8") },
                });

                // Mitsubishi Xpander 1.5 Data
                specificationsData.AddRange(new[]
                {
                    new SpecificationsData { DataID = Guid.Parse("cccc0001-cccc-cccc-cccc-cccccccccccc"), Value = "1,499 cc", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("1062c9e7-f2e9-425a-bd83-130d8cb6bc48") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0002-cccc-cccc-cccc-cccccccccccc"), Value = "1.5L MIVEC", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("e92474ca-01c5-430e-938d-f174d4f2b006") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0003-cccc-cccc-cccc-cccccccccccc"), Value = "105 hp @ 6,000 rpm", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("2a61b12a-540d-4cd7-b342-7206cdb93718") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0004-cccc-cccc-cccc-cccccccccccc"), Value = "141 Nm @ 4,000 rpm", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("d71eed56-c8e9-49e1-b343-53ffe5048579") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0005-cccc-cccc-cccc-cccccccccccc"), Value = "4 cylinders inline", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("5a1667e0-4824-4b75-b7f6-2d5b18f28484") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0006-cccc-cccc-cccc-cccccccccccc"), Value = "Euro 4", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("b7841385-72a3-43a5-9ded-3979e452f33e") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0007-cccc-cccc-cccc-cccccccccccc"), Value = "CVT (Continuously Variable Transmission)", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("ab72e6de-2301-4fe8-b9ca-c071f9aea9ec") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0008-cccc-cccc-cccc-cccccccccccc"), Value = "Stepless", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("5bd55c9a-c9d5-4e89-b120-fe8800d25761") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0009-cccc-cccc-cccc-cccccccccccc"), Value = "FWD (Front-Wheel Drive)", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("4b6fd7f6-c921-4d5e-b61d-88cd8d43364f") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0010-cccc-cccc-cccc-cccccccccccc"), Value = "4475 x 1750 x 1700 mm", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("22f4cc10-39b6-4c23-9003-d307da062a35") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0011-cccc-cccc-cccc-cccccccccccc"), Value = "2775 mm", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("59724aec-369d-42b9-b7af-75f79487b694") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0012-cccc-cccc-cccc-cccccccccccc"), Value = "205 mm", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("63f26dc8-9afd-4a3b-b2c2-1c374d904b5e") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0013-cccc-cccc-cccc-cccccccccccc"), Value = "5.2 m", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("9880fc52-484d-45b2-bf48-987603fc94ac") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0014-cccc-cccc-cccc-cccccccccccc"), Value = "1,230 kg", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("3ed2f196-779b-48d2-9e4f-e0870c6907d1") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0015-cccc-cccc-cccc-cccccccccccc"), Value = "1,800 kg", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("95a2b5c4-56ca-425a-b718-0d2b49f8165c") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0016-cccc-cccc-cccc-cccccccccccc"), Value = "MPV", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("993b032e-c199-4bff-9d6b-d99724ee4b9b") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0017-cccc-cccc-cccc-cccccccccccc"), Value = "7 seats", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("60320b26-3f03-4391-b107-b84d9ebcebd5") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0018-cccc-cccc-cccc-cccccccccccc"), Value = "195/65R15", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("9584d60e-2d8d-4d60-b37b-6b44a88ee253") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0019-cccc-cccc-cccc-cccccccccccc"), Value = "15 inches", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("39a4e1c4-97fa-44c7-a883-9ea1910c0752") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0020-cccc-cccc-cccc-cccccccccccc"), Value = "Steel wheels", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("52e11e7f-7527-43de-a7e1-5a25d5650bb1") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0021-cccc-cccc-cccc-cccccccccccc"), Value = "12.5 seconds", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("0267940c-c6f6-4235-b6cc-d62e8ff334f8") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0022-cccc-cccc-cccc-cccccccccccc"), Value = "170 km/h", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("c06286dc-8907-4d0c-8111-4833290219d3") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0023-cccc-cccc-cccc-cccccccccccc"), Value = "7.6 L/100km", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("a07d762f-3246-4c8c-8a7d-add5d3cee2c5") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0024-cccc-cccc-cccc-cccccccccccc"), Value = "45 liters", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("9f1e0f75-af7c-403b-9e40-b4ff0d695f77") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0025-cccc-cccc-cccc-cccccccccccc"), Value = "2 airbags", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("88f34b4f-da40-4a38-b2ba-842ba4b47974") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0026-cccc-cccc-cccc-cccccccccccc"), Value = "Yes", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("63cf2134-a8d0-4cdb-8fe2-5a3aa1dd4196") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0027-cccc-cccc-cccc-cccccccccccc"), Value = "Yes", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("b9b37318-6ce0-4b33-a1a5-536f74b56968") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0028-cccc-cccc-cccc-cccccccccccc"), Value = "No", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("ee069d30-369d-4aa8-a5bf-08b7dfb6af64") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0029-cccc-cccc-cccc-cccccccccccc"), Value = "No", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("00afeaa4-43fb-41af-85ea-41ed45a3004d") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0030-cccc-cccc-cccc-cccccccccccc"), Value = "No", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("72234e93-20a7-4b39-87ca-314a7b087a21") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0031-cccc-cccc-cccc-cccccccccccc"), Value = "No", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("5d5c8f66-118e-4e06-b7b5-cf3d7999a267") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0032-cccc-cccc-cccc-cccccccccccc"), Value = "Standard Cruise Control", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("22817107-5580-4480-b9cb-1e6a62c2bbd0") },
                    new SpecificationsData { DataID = Guid.Parse("cccc0033-cccc-cccc-cccc-cccccccccccc"), Value = "2 sensors + Rear camera", LookupID = mitsubishiXpanderId, SpecificationID = Guid.Parse("09afc042-2e89-4beb-b272-85f7fe636df8") },
                });

                await _context.SpecificationsData.AddRangeAsync(specificationsData);
                await _context.SaveChangesAsync();
                Console.WriteLine($"✓ SpecificationsData seeded successfully ({specificationsData.Count} data entries for 5 vehicles)!");
            }
        }
    }
}