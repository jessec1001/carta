using System;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amazon.CognitoIdentityProvider;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;


using CartaCore.Serialization.Json;
using CartaCore.Persistence;
using CartaWeb.Models.Options;
using CartaWeb.Models.Data;
using CartaWeb.Models.DocumentItem;

namespace CartaWeb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private readonly Persistence _persistence;


        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        /// <summary>
        /// Static constructor for initializing JSON serialization/deserialization options
        /// </summary>
        static TestController()
        {
            JsonOptions.PropertyNameCaseInsensitive = false;
            JsonOptions.IgnoreNullValues = true;
            JsonOptions.Converters.Add(new JsonObjectConverter());
        }

        /// <inheritdoc />
        public TestController(
            ILogger<TestController> logger,
            INoSqlDbContext noSqlDbContext,
            IAmazonCognitoIdentityProvider identityProvider,
            IOptions<AwsCdkOptions> options)
        {
            _logger = logger;
            _persistence = new Persistence(noSqlDbContext, options.Value);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> PostJobItem(
            [FromRoute] string id
        )
        {
            TestItem testItem = new(id, "secretId")
            {
                SecretValues = new Dictionary<string, object>() { { "myKey1", "myValue1" }, { "myKey2", "myValue2" } },
                SomeOtherProperty = "someOtherPropertyWithAValue"
            };

            await _persistence.WriteDbDocumentAsync(testItem.SaveDbDocument(), User);
            return Ok();
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<JobItem>> GetJobItem(
            [FromRoute] string id
        )
        {
            Item item = await _persistence.LoadItemAsync(new TestItem(id, "secretId"), User);
            return Ok((TestItem)item);
        }
    }
}
