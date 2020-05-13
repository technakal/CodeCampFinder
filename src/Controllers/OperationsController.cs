﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreCodeCamp.Controllers
{
  [Route("api/[controller]")]
  [ApiController]
  public class OperationsController : ControllerBase
  {
    private readonly IConfiguration config;

    public OperationsController(IConfiguration config)
    {
      this.config = config;
    }

    [HttpOptions("reloadconfig")]
    public ActionResult ReloadConfig()
    {
      try
      {
        var root = (IConfigurationRoot)config;
        root.Reload();
        return Ok();
      }
      catch (Exception exc)
      {
        Console.WriteLine(exc);
        return StatusCode(StatusCodes.Status500InternalServerError, "Something real bad happened.");
      }
    }
  }
}