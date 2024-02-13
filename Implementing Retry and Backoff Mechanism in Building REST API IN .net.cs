using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace REST_API
{
    public class MockEntityRepository : IEntityRepository
    {
        private List<IEntity> _entities;

        public EntityRepository()
        {
            
            _entities = new List<IEntity>();
            
            {
            new Entity
            {
                Id = "1",
                Addresses = new List<Address>
                {
                    new Address { AddressLine = "123 Main St", City = "New York", Country = "USA" }
                },
                Dates = new List<Date>
                {
                    new Date { DateType = "Birth", Date = new DateTime(1990, 5, 15) }
                },
                Deceased = false,
                Gender = "Male",
                Names = new List<Name>
                {
                    new Name { FirstName = "John", MiddleName = "Michael", Surname = "Doe" }
                }
            },
            new Entity
            {
                Id = "2",
                Addresses = new List<Address>
                {
                    new Address { AddressLine = "456 Elm St", City = "Los Angeles", Country = "USA" }
                },
                Dates = new List<Date>
                {
                    new Date { DateType = "Birth", Date = new DateTime(1985, 10, 20) }
                },
                Deceased = false,
                Gender = "Female",
                Names = new List<Name>
                {
                    new Name { FirstName = "Alice", MiddleName = "Elizabeth", Surname = "Smith" }
                }
            }
            // Add more entities if needed...
        };
    }
        }

        public IEnumerable<IEntity> GetEntities(EntityFilterParameters filterParameters)
        {
            var entities = _entities.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(filterParameters.Search))
            {
                var searchTerm = filterParameters.Search.ToLower();
                entities = entities.Where(entity =>
                    entity.Names.Any(name => name.FirstName?.ToLower().Contains(searchTerm) == true ||
                                              name.MiddleName?.ToLower().Contains(searchTerm) == true ||
                                              name.Surname?.ToLower().Contains(searchTerm) == true) ||
                    entity.Addresses.Any(address =>
                        address.AddressLine?.ToLower().Contains(searchTerm) == true ||
                        address.Country?.ToLower().Contains(searchTerm) == true));
            }

            if (!string.IsNullOrWhiteSpace(filterParameters.Gender))
            {
                entities = entities.Where(entity => entity.Gender == filterParameters.Gender);
            }

            if (filterParameters.StartDate.HasValue && filterParameters.EndDate.HasValue)
            {
                entities = entities.Where(entity =>
                    entity.Dates.Any(date =>
                        date.Date >= filterParameters.StartDate && date.Date <= filterParameters.EndDate));
            }

            if (filterParameters.Countries != null && filterParameters.Countries.Any())
            {
                entities = entities.Where(entity =>
                    entity.Addresses.Any(address => filterParameters.Countries.Contains(address.Country)));
            }

            return entities.ToList();
        }

        public IEntity GetEntityById(string id)
        {
            return _entities.FirstOrDefault(entity => entity.Id == id);
        }
        public IEntity CreateEntity(Entity entity)
        {
            RetryPolicy(() => _entities.Add(entity));
            return entity;
        }

        public void UpdateEntity(Entity entity)
        {
            RetryPolicy(() =>
            {
                var existingEntity = _entities.FirstOrDefault(e => e.Id == entity.Id);
                if (existingEntity != null)
                {
                    // Update existingEntity with entity properties
                    // ...
                }
            });
        }

        public void DeleteEntity(string id)
        {
            RetryPolicy(() =>
            {
                var entityToRemove = _entities.FirstOrDefault(entity => entity.Id == id);
                if (entityToRemove != null)
                {
                    _entities.Remove(entityToRemove);
                }
            });
        }

        private void RetryPolicy(Action action, int retryCount = 3, TimeSpan? initialDelay = null)
        {
            int retries = 0;
            TimeSpan delay = initialDelay ?? TimeSpan.FromSeconds(1);
            
            while (true)
            {
                try
                {
                    action.Invoke();
                    break; // Operation succeeded, exit retry loop
                }
                catch (Exception ex)
                {
                    if (retries >= retryCount)
                        throw; // Maximum retries reached, propagate exception

                    // Log or handle the exception if needed
                    Console.WriteLine($"Error: {ex.Message}. Retrying in {delay.TotalSeconds} seconds...");

                    // Increment retry count and apply exponential backoff
                    retries++;
                    Thread.Sleep(delay);
                    delay *= 2; // Exponential backoff
                }
            }
        }
    }
}
