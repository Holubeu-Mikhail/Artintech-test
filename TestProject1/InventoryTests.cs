using System.Collections.Concurrent;
using Test;

namespace TestProject1
{
    public class InventoryTests
    {
        private Inventory _inventory;

        [SetUp]
        public void Setup()
        {
            _inventory = new Inventory();
        }

        [Test]
        public void AddItem_ValidItem_AddsSuccessfully()
        {
            var item = new Item { Name = "Sword", Weight = 10 };

            _inventory.AddItem(item);

            Assert.That(_inventory.Items.Count, Is.EqualTo(1));
            Assert.That(_inventory.Items[0].Name, Is.EqualTo("Sword"));
            Assert.That(_inventory.CurrentWeight, Is.EqualTo(10));
        }

        [Test]
        public void AddItem_NullItem_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _inventory.AddItem(null));
        }

        [Test]
        public void AddItem_DuplicateItem_SumsWeight()
        {
            var item1 = new Item { Name = "Potion", Weight = 5 };
            var item2 = new Item { Name = "Potion", Weight = 3 };

            _inventory.AddItem(item1);
            _inventory.AddItem(item2);

            Assert.That(_inventory.Items.Count, Is.EqualTo(1));
            Assert.That(_inventory.Items[0].Weight, Is.EqualTo(8));
            Assert.That(_inventory.CurrentWeight, Is.EqualTo(8));
        }

        [Test]
        public void AddItem_ExceedsMaxWeight_ThrowsException()
        {
            var item1 = new Item { Name = "Heavy1", Weight = 60 };
            var item2 = new Item { Name = "Heavy2", Weight = 50 };

            _inventory.AddItem(item1);

            var ex = Assert.Throws<InvalidOperationException>(() => _inventory.AddItem(item2));
            Assert.That(ex.Message, Contains.Substring("exceed maximum"));
        }

        [Test]
        public void RemoveItem_ExistingItem_ReturnsTrue()
        {
            var item = new Item { Name = "Shield", Weight = 20 };
            _inventory.AddItem(item);

            var result = _inventory.RemoveItem(item);

            Assert.That(result, Is.True);
            Assert.That(_inventory.Items.Count, Is.EqualTo(0));
            Assert.That(_inventory.CurrentWeight, Is.EqualTo(0));
        }

        [Test]
        public void RemoveItem_PartialWeight_ReducesWeight()
        {
            var item = new Item { Name = "Gold", Weight = 100 };
            _inventory.AddItem(item);

            var toRemove = new Item { Name = "Gold", Weight = 40 };
            var result = _inventory.RemoveItem(toRemove);

            Assert.That(result, Is.True);
            Assert.That(_inventory.Items[0].Weight, Is.EqualTo(60));
            Assert.That(_inventory.CurrentWeight, Is.EqualTo(60));
        }

        [Test]
        public void FindItems_BySubstring_ReturnsMatches()
        {
            var items = new[]
            {
                new Item { Name = "Health Potion", Weight = 2 },
                new Item { Name = "Mana Potion", Weight = 2 },
                new Item { Name = "Sword", Weight = 15 }
            };

            foreach (var item in items) _inventory.AddItem(item);

            var potions = _inventory.FindItems("potion");

            Assert.That(potions.Count, Is.EqualTo(2));
            Assert.That(potions.All(p => p.Name.Contains("Potion")));
        }

        [Test]
        public void Items_Property_ReturnsCopy()
        {
            var item = new Item { Name = "Test", Weight = 10 };
            _inventory.AddItem(item);

            var items = _inventory.Items;
            items[0].Weight = 999;

            Assert.That(_inventory.Items[0].Weight, Is.EqualTo(10));
        }

        [Test]
        public void ThreadSafety_MultipleThreads_NoRaceConditions()
        {
            const int threadCount = 10;
            const int itemsPerThread = 10;
            var exceptions = new ConcurrentQueue<Exception>();
            var itemsAdded = new ConcurrentBag<string>();

            Parallel.For(0, threadCount, i =>
            {
                try
                {
                    for (int j = 0; j < itemsPerThread; j++)
                    {
                        var item = new Item
                        {
                            Name = $"Item_{i}_{j}",
                            Weight = 1
                        };
                        _inventory.AddItem(item);
                        itemsAdded.Add(item.Name);
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Enqueue(ex);
                }
            });

            Assert.That(exceptions, Is.Empty, $"Exceptions occurred: {string.Join(", ", exceptions.Select(e => e.Message))}");
            Assert.That(_inventory.CurrentWeight, Is.LessThanOrEqualTo(100));
            Assert.That(itemsAdded.Distinct().Count(), Is.EqualTo(itemsAdded.Count()));
        }

        [Test]
        public void Clear_RemovesAllItems()
        {
            _inventory.AddItem(new Item { Name = "A", Weight = 10 });
            _inventory.AddItem(new Item { Name = "B", Weight = 20 });

            _inventory.Clear();

            Assert.That(_inventory.Items.Count, Is.EqualTo(0));
            Assert.That(_inventory.CurrentWeight, Is.EqualTo(0));
        }
    }
}