using BilbolStack.RedisPubSub.Repository;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BilbolStack.RedisPubSub.Controllers
{
    [ApiController]
    [Route("/cache")]
    public class CacheController : ControllerBase
    {
        private ILayeredCacheAdapter _layeredCacheAdapter;
        public CacheController(ILayeredCacheAdapter layeredCacheAdapter)
        {
            _layeredCacheAdapter = layeredCacheAdapter;
        }

        [HttpGet]
        public object Get([FromQuery][Required] string key)
        {
            return _layeredCacheAdapter.Get<object>(key);
        }

        [HttpDelete]
        public void Delete([FromQuery][Required] string key, [FromQuery] bool byPattern)
        {
            if(byPattern)
                _layeredCacheAdapter.ClearByPattern(key);
            else
                _layeredCacheAdapter.Clear(key);
        }

        [HttpPost]
        public void Create([FromQuery] [Required] string key, [FromQuery] [Required] int minutes, [FromBody] [Required] object content)
        {
            _layeredCacheAdapter.Set(key, content, minutes);
        }
    }
}
