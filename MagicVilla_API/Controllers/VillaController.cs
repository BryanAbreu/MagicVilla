﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MagicVilla_API.Models.DTO;
using MagicVilla_API.Datos;
using Microsoft.AspNetCore.JsonPatch;
using MagicVilla_API.Models;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using MagicVilla_API.Repository.IRepository;
using System.Net;

namespace MagicVilla_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaController : ControllerBase
    {
        private readonly ILogger<VillaController> _logger;
        private readonly IVillaRepository _villaRepository;
        private readonly IMapper _mapper;
        protected APIResponse _response;

        public VillaController(ILogger<VillaController> logger, IVillaRepository villaRepository, IMapper mapper)
        {
            _logger = logger;
            _villaRepository = villaRepository;
            _mapper = mapper;
            _response = new();
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task< ActionResult< APIResponse>> Getvillas()
        {
            try
            {
                _logger.LogInformation("get villa");

                IEnumerable<Villa> villaList = await _villaRepository.GetAll();

                _response.Result = _mapper.Map<IEnumerable<VillaDTO>>(villaList);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors = new List<string>() { ex.ToString() };
               
            }
            return _response;
           
        }


        [HttpGet("id:int",Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<APIResponse>> GetVilla(int id) 
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Error get villa with id " + id);
                    _response.StatusCode=HttpStatusCode.BadRequest;
                    _response.IsSuccess = false;
                    return BadRequest(_response);
                }
                var villa = await _villaRepository.Get(v => v.Id == id);
                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    _response.IsSuccess = false;
                    return NotFound(_response);
                }
                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode=HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors = new List<string>() { ex.ToString() };

            }
            return _response;


        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]

        public async Task<ActionResult<APIResponse>> CrearVilla([FromBody]VillaCreateDTO createvillaDTO)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                if (await _villaRepository.Get(v => v.Name.ToLower() == createvillaDTO.Name.ToLower()) != null)
                {
                    ModelState.AddModelError("NameExist", "That Name of village already exist!");
                    return BadRequest(ModelState);
                }
                if (createvillaDTO == null)
                {
                    return BadRequest(createvillaDTO);
                }

                Villa model = _mapper.Map<Villa>(createvillaDTO);

                model.DateCreate= DateTime.Now;
                model.DateUpdate= DateTime.Now;
                await _villaRepository.Create(model);
                _response.Result = model;
                _response.StatusCode = HttpStatusCode.Created;

                return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors = new List<string>() { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVilla(int id) 
        {
            try
            {
                if (id == 0)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }
                var vill = await _villaRepository.Get(V => V.Id == id);
                if (vill == null)
                {
                    _response.IsSuccess = false;
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }
                await _villaRepository.Remove(vill);

                _response.StatusCode=HttpStatusCode.NoContent;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.Errors = new List<string>() { ex.ToString() };
            }
            return BadRequest(_response);

        }

        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO UpdatevillaDTO)
        {
            if (UpdatevillaDTO == null || id!= UpdatevillaDTO.Id)
            {
                _response.IsSuccess = false;
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }
            
            Villa model = _mapper.Map<Villa>(UpdatevillaDTO);

            
            await _villaRepository.Update(model);
            _response.StatusCode = HttpStatusCode.NoContent;
         

            return Ok(_response);    
        }

        [HttpPatch("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchdto)
        {
            if (patchdto == null || id ==0)
            {
                return BadRequest();
            }
            var vill = await _villaRepository.Get(V => V.Id == id,traked:false);

            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(vill);


            if (vill == null) return BadRequest(); 

            patchdto.ApplyTo(villaDTO, ModelState);

            if (!ModelState.IsValid)
            { 
                return BadRequest(ModelState);
            }
            Villa model = _mapper.Map<Villa>(villaDTO);
           
            await _villaRepository.Update(model);
            _response.StatusCode = HttpStatusCode.NoContent;
            return Ok(_response);
        }

    }
}
