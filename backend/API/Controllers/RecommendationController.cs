using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Core.Features.GetStockRecommendations;
using API.Contracts;
using Mapster;

namespace API.Controllers
{
    [ApiController]
    [Route("api/recommendations")]
    public class RecommendationController : ControllerBase
    {
        private readonly ISender _mediator;

        public RecommendationController(ISender mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetRecommendations(RecommendationRequest request)
        {
            var command = request.Adapt<GetStockRecommendationsCommand>();
            var recommendations = await _mediator.Send(command);

            if (recommendations == null || recommendations.Count == 0)
            {
                return NotFound("No recommendations found for this user.");
            }

            return Ok(recommendations);
        }
    }
}
