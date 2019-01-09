using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Troschuetz.Random;
using Troschuetz.Random.Generators;

namespace Bendang.Controllers
{
    [Route("/")]
    [ApiController]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get(int? seed)
        {
            if (seed == null) seed = (int) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return Process(1, seed.Value);
        }

        [HttpGet("{count}")]
        public ActionResult<IEnumerable<string>> Get(int count, int? seed)
        {
            if (seed == null) seed = (int) DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return Process(count, seed.Value);
        }

        private ActionResult<IEnumerable<string>> Process(int count, int seed)
        {
            if (count < 1) count  = 1;
            if (count > 10) count = 10;

            var random = new TRandom(new MT19937Generator(seed));
            return Enumerable.Range(0, count).Select(_ => random.Next())
                             .Select(x => new PseudoJapaneseGenerator(x).Generate())
                             .ToList();
        }
    }
}