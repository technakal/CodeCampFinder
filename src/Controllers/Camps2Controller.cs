using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore.Internal;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
  [Route("api/camps")]
  [ApiVersion("2.0")]
  [ApiController]
  public class Camps2Controller : ControllerBase
  {
    private readonly ICampRepository campRepository;
    private readonly IMapper mapper;
    private readonly LinkGenerator linkGenerator;

    public Camps2Controller(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
    {
      this.campRepository = campRepository;
      this.mapper = mapper;
      this.linkGenerator = linkGenerator;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllCamps(bool includeTalks = false)
    {
      try
      {
        var results = await campRepository.GetAllCampsAsync(includeTalks);
        var result = new
        {
          Count = results.Count(),
          Results = mapper.Map<CampModel[]>(results)
        };
        return Ok(result);
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
      }
    }

    [HttpGet("{moniker}")]
    public async Task<ActionResult<CampModel>> GetCampByMoniker(string moniker)
    {
      try
      {
        var result = await campRepository.GetCampAsync(moniker);

        if (result == null) return NotFound();

        return mapper.Map<CampModel>(result);
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
      }
    }

    [HttpGet("search")]
    public async Task<ActionResult<CampModel[]>> GetCampsByDate(DateTime eventDate, bool includeTalks = false)
    {
      try
      {
        var results = await campRepository.GetAllCampsByEventDate(eventDate, includeTalks);

        if (!results.Any()) return NotFound(results);

        return mapper.Map<CampModel[]>(results);
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Database Failure");
      }
    }

    [HttpPost]
    public async Task<ActionResult<CampModel>> Post(CampModel model)
    {
      try
      {
        var existingCamp = await campRepository.GetCampAsync(model.Moniker);
        if (existingCamp != null)
        {
          return BadRequest("Moniker not unique");
        }
        var location = linkGenerator.GetPathByAction("GetCampByMoniker", "Camps", new { moniker = model.Moniker });
        if (string.IsNullOrWhiteSpace(location))
        {
          return BadRequest("Could not use current moniker.");
        }
        var camp = mapper.Map<Camp>(model);
        campRepository.Add(camp);
        if (await campRepository.SaveChangesAsync())
        {
          return Created(location, mapper.Map<CampModel>(camp));
        }
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create resource");
      }
      return BadRequest();
    }

    [HttpPut("{moniker}")]
    public async Task<ActionResult<CampModel>> Put(string moniker, CampModel model)
    {
      try
      {
        var oldCamp = await campRepository.GetCampAsync(model.Moniker);

        if (oldCamp == null) return NotFound($"No camp found matching moniker: {moniker}");

        mapper.Map(model, oldCamp);
        Console.WriteLine(oldCamp);

        if (await campRepository.SaveChangesAsync())
        {
          return mapper.Map<CampModel>(oldCamp);
        }
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update resource");
      }
      return BadRequest("Base fail condition");
    }

    [HttpDelete("{moniker}")]
    public async Task<IActionResult> Delete(string moniker)
    {
      try
      {
        var camp = await campRepository.GetCampAsync(moniker);
        if (camp == null) return NotFound($"No camp found matching moniker: {moniker}");

        campRepository.Delete(camp);

        if (await campRepository.SaveChangesAsync())
        {
          return Ok();
        }
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update resource");
      }
      return BadRequest();
    }
  }
}