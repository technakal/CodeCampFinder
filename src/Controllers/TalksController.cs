using AutoMapper;
using CoreCodeCamp.Data;
using CoreCodeCamp.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
  [Route("api/camps/{moniker}/talks")]
  [ApiController]
  public class TalksController : ControllerBase
  {
    private readonly ICampRepository campRepository;
    private readonly IMapper mapper;
    private readonly LinkGenerator linkGenerator;

    public TalksController(ICampRepository campRepository, IMapper mapper, LinkGenerator linkGenerator)
    {
      this.campRepository = campRepository;
      this.mapper = mapper;
      this.linkGenerator = linkGenerator;
    }

    [HttpGet]
    public async Task<ActionResult<TalkModel[]>> Get(string moniker) 
    {
      try
      {
        var talks = await campRepository.GetTalksByMonikerAsync(moniker, true);

        return mapper.Map<TalkModel[]>(talks);
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve resource.");
      }
    }

    [HttpGet("{talkId:int}")]
    public async Task<ActionResult<TalkModel>> Get(string moniker, int talkId)
    {
      try
      {
        var talk = await campRepository.GetTalkByMonikerAsync(moniker, talkId, true);
        if (talk == null) return NotFound("No talk found with that moniker and Id combination.");
        return mapper.Map<TalkModel>(talk);
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve resource.");
      }
    }

    [HttpPost]
    public async Task<ActionResult<TalkModel>> Post(string moniker, TalkModel model)
    {
      try
      {
        var camp = await campRepository.GetCampAsync(moniker);
        if (camp == null) return BadRequest("Camp does not exist.");

        var talk = mapper.Map<Talk>(model);
        if (model.Speaker == null) return BadRequest("Speaker ID is required.");
        var speaker = await campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
        if (speaker == null) return BadRequest("Speaker could not be found.");
        talk.Camp = camp;
        talk.Speaker = speaker;
        campRepository.Add(talk);
        if(await campRepository.SaveChangesAsync())
        {
          var location = linkGenerator.GetPathByAction(
            HttpContext,
            "Get",
            values: new { moniker, id = talk.TalkId });
          return Created(location, mapper.Map<TalkModel>(talk));
        } 
        else
        {
          return BadRequest("Failed to save new talk.");
        }
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve resource.");
      }
    }

    [HttpPut("{talkId:int}")]
    public async Task<ActionResult<TalkModel>> Put(string moniker, int talkId, TalkModel model)
    {
      try
      {
        var oldTalk = await campRepository.GetTalkByMonikerAsync(moniker, talkId, true);
        if (oldTalk == null) return NotFound("Talk not found.");

        mapper.Map(model, oldTalk);

        if(model.Speaker != null)
        {
          var speaker = await campRepository.GetSpeakerAsync(model.Speaker.SpeakerId);
          if(speaker != null)
          {
            oldTalk.Speaker = speaker;
          }
        }

        if(await campRepository.SaveChangesAsync())
        {
          return mapper.Map<TalkModel>(oldTalk);
        }
        else
        {
          return BadRequest();
        }
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve resource.");
      }
    }

    [HttpDelete("{talkId:int}")]
    public async Task<IActionResult> Delete(string moniker, int talkId)
    {
      try
      {
        var talk = await campRepository.GetTalkByMonikerAsync(moniker, talkId);
        if (talk == null) return NotFound("Talk not found.");

        campRepository.Delete(talk);

        if(await campRepository.SaveChangesAsync())
        {
          return Ok();
        }
        else
        {
          return BadRequest("Failed to delete talk.");
        }
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Failed to retrieve resource.");
      }
    }
  }
}
