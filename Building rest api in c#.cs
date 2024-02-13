using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YourNamespace.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EntityController : ControllerBase
    {
        private readonly IEntityRepository _entityRepository;

        public EntityController(IEntityRepository entityRepository)
        {
            _entityRepository = entityRepository;
        }

        // GET /entity
        [HttpGet]
        public ActionResult<IEnumerable<IEntity>> GetEntities([FromQuery] EntityFilterParameters filterParameters)
        {
            var entities = _entityRepository.GetEntities(filterParameters);
            return Ok(entities);
        }

        // GET /entity/{id}
        [HttpGet("{id}")]
        public ActionResult<IEntity> GetEntity(string id)
        {
            var entity = _entityRepository.GetEntityById(id);
            if (entity == null)
            {
                return NotFound();
            }
            return Ok(entity);
        }

        // POST /entity
        [HttpPost]
        public ActionResult<IEntity> CreateEntity(Entity entity)
        {
            // Validate entity if needed
            var createdEntity = _entityRepository.CreateEntity(entity);
            return CreatedAtAction(nameof(GetEntity), new { id = createdEntity.Id }, createdEntity);
        }

        // PUT /entity/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateEntity(string id, Entity entity)
        {
            if (id != entity.Id)
            {
                return BadRequest();
            }

            _entityRepository.UpdateEntity(entity);
            return NoContent();
        }

        // DELETE /entity/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteEntity(string id)
        {
            _entityRepository.DeleteEntity(id);
            return NoContent();
        }
    }

    public interface IEntityRepository
    {
        IEnumerable<IEntity> GetEntities(EntityFilterParameters filterParameters);
        IEntity GetEntityById(string id);
        IEntity CreateEntity(Entity entity);
        void UpdateEntity(Entity entity);
        void DeleteEntity(string id);
    }

    public class EntityFilterParameters
    {
        public string? Search { get; set; }
        public string? Gender { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public List<string>? Countries { get; set; }
    }

    public class EntityRepository : IEntityRepository
    {
        private List<IEntity> _entities;

        public EntityRepository()
        {
            // Initialize mock data
            _entities = new List<IEntity>();
            // Populate _entities with mock data
            // ...
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
            _entities.Add(entity);
            return entity;
        }

        public void UpdateEntity(Entity entity)
        {
            var existingEntity = _entities.FirstOrDefault(e => e.Id == entity.Id);
            if (existingEntity != null)
            {
                // Update existingEntity with entity properties
                // ...
            }
        }

        public void DeleteEntity(string id)
        {
            var entityToRemove = _entities.FirstOrDefault(entity => entity.Id == id);
            if (entityToRemove != null)
            {
                _entities.Remove(entityToRemove);
            }
        }
    }
}
